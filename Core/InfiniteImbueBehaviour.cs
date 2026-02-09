using System;
using System.Collections;
using System.Collections.Generic;
using InfiniteImbueFramework.Configuration;
using ThunderRoad;
using UnityEngine;

namespace InfiniteImbueFramework
{
    public class InfiniteImbueBehaviour : MonoBehaviour
    {
        private const float InitialTransferEnergy = 0.01f;
        private static readonly HashSet<InfiniteImbueBehaviour> activeBehaviours = new HashSet<InfiniteImbueBehaviour>();
        private static readonly Dictionary<string, int> roundRobinNextOffsets = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private Item item;
        private List<ImbueSpellConfig> spells;
        private ImbueAssignmentMode assignmentMode;
        private ImbueConflictPolicy conflictPolicy;
        private bool applyOnSpawn;
        private bool keepFilled;
        private float updateInterval;
        private float maintainBelowRatio;
        private float refillToRatio;
        private float minSetEnergyInterval;
        private float conditionalVelocityThreshold;
        private float conditionalVelocityHysteresis;
        private float conditionalMinSwitchInterval;
        private bool debugLogging;
        private WaitForSeconds updateWait;
        private Coroutine applyRoutine;
        private Coroutine maintainRoutine;
        private int currentRoundRobinStart;
        private int conditionalState = -1;
        private float conditionalLastStateSwitchTime;
        private readonly Dictionary<string, SpellCastCharge> spellCache = new Dictionary<string, SpellCastCharge>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> missingSpellIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, int> randomSpellByImbueIndex = new Dictionary<int, int>();
        private readonly Dictionary<int, float> lastSetTimeByImbueIndex = new Dictionary<int, float>();
        private readonly Dictionary<int, string> lastFrameworkSpellByImbueIndex = new Dictionary<int, string>();

        public static IReadOnlyCollection<InfiniteImbueBehaviour> ActiveBehaviours => activeBehaviours;

        public Item Item => item;
        public IReadOnlyList<ImbueSpellConfig> Spells => spells;
        public ImbueAssignmentMode AssignmentMode => assignmentMode;
        public ImbueConflictPolicy ConflictPolicy => conflictPolicy;
        public bool ApplyOnSpawn => applyOnSpawn;
        public bool KeepFilled => keepFilled;
        public float UpdateInterval => updateInterval;
        public float MaintainBelowRatio => maintainBelowRatio;
        public float RefillToRatio => refillToRatio;
        public float MinSetEnergyInterval => minSetEnergyInterval;
        public float ConditionalVelocityThreshold => conditionalVelocityThreshold;
        public float ConditionalVelocityHysteresis => conditionalVelocityHysteresis;
        public float ConditionalMinSwitchInterval => conditionalMinSwitchInterval;

        private void OnEnable()
        {
            activeBehaviours.Add(this);
        }

        private void OnDisable()
        {
            activeBehaviours.Remove(this);
        }

        public void Init(Item item, ItemModuleInfiniteImbue module)
        {
            this.item = item;
            spells = module.spells ?? new List<ImbueSpellConfig>();
            assignmentMode = module.assignmentMode;
            conflictPolicy = module.conflictPolicy;
            applyOnSpawn = module.applyOnSpawn;
            keepFilled = module.keepFilled;
            updateInterval = Mathf.Max(0.05f, module.updateInterval);
            maintainBelowRatio = Mathf.Clamp01(module.maintainBelowRatio);
            refillToRatio = Mathf.Clamp(module.refillToRatio, maintainBelowRatio, 1f);
            minSetEnergyInterval = Mathf.Max(0f, module.minSetEnergyInterval);
            conditionalVelocityThreshold = Mathf.Max(0f, module.conditionalVelocityThreshold);
            conditionalVelocityHysteresis = Mathf.Max(0f, module.conditionalVelocityHysteresis);
            conditionalMinSwitchInterval = Mathf.Max(0f, module.conditionalMinSwitchInterval);
            debugLogging = module.debugLogging;
            updateWait = new WaitForSeconds(updateInterval);

            item.OnSpawnEvent += OnItemSpawnEvent;
            item.OnDespawnEvent += OnItemDespawnEvent;

            IIFLog.Info(
                $"Attached to item {item?.itemId ?? item?.name ?? "Unknown"} (applyOnSpawn={applyOnSpawn}, keepFilled={keepFilled}, interval={updateInterval:0.00}s, maintainBelow={maintainBelowRatio:0.##}, refillTo={refillToRatio:0.##}, minSetInterval={minSetEnergyInterval:0.##}s, assignment={assignmentMode}, conflictPolicy={conflictPolicy}, velocityThreshold={conditionalVelocityThreshold:0.##}, hysteresis={conditionalVelocityHysteresis:0.##}, minStateSwitch={conditionalMinSwitchInterval:0.##}s).",
                debugLogging);

            if (!applyOnSpawn)
            {
                StartApplyRoutine();
            }
        }

        private void OnDestroy()
        {
            if (item != null)
            {
                item.OnSpawnEvent -= OnItemSpawnEvent;
                item.OnDespawnEvent -= OnItemDespawnEvent;
            }
            if (applyRoutine != null)
            {
                StopCoroutine(applyRoutine);
            }
            if (maintainRoutine != null)
            {
                StopCoroutine(maintainRoutine);
            }
        }

        private void OnItemSpawnEvent(EventTime eventTime)
        {
            if (eventTime != EventTime.OnEnd)
            {
                return;
            }
            StartApplyRoutine();
        }

        private void OnItemDespawnEvent(EventTime eventTime)
        {
            if (eventTime != EventTime.OnStart)
            {
                return;
            }
            if (applyRoutine != null)
            {
                StopCoroutine(applyRoutine);
                applyRoutine = null;
            }
            if (maintainRoutine != null)
            {
                StopCoroutine(maintainRoutine);
                maintainRoutine = null;
            }
            randomSpellByImbueIndex.Clear();
            lastSetTimeByImbueIndex.Clear();
            lastFrameworkSpellByImbueIndex.Clear();
            conditionalState = -1;
            conditionalLastStateSwitchTime = 0f;
        }

        private void StartApplyRoutine()
        {
            if (applyRoutine != null)
            {
                StopCoroutine(applyRoutine);
            }
            PrepareSpawnAssignment();
            applyRoutine = StartCoroutine(ApplyWhenReady());
        }

        private void PrepareSpawnAssignment()
        {
            randomSpellByImbueIndex.Clear();
            lastSetTimeByImbueIndex.Clear();
            currentRoundRobinStart = 0;
            conditionalState = -1;
            conditionalLastStateSwitchTime = Time.unscaledTime;

            if (spells == null || spells.Count == 0)
            {
                return;
            }

            if (assignmentMode == ImbueAssignmentMode.RoundRobinPerSpawn)
            {
                string itemId = GetItemId();
                if (!roundRobinNextOffsets.TryGetValue(itemId, out currentRoundRobinStart))
                {
                    currentRoundRobinStart = 0;
                }
                currentRoundRobinStart %= spells.Count;
                roundRobinNextOffsets[itemId] = (currentRoundRobinStart + 1) % spells.Count;
            }
        }

        private IEnumerator ApplyWhenReady()
        {
            int safetyFrames = 120;
            while (item != null && (item.imbues == null || item.imbues.Count == 0) && safetyFrames-- > 0)
            {
                yield return null;
            }
            ApplyImbues(forceReload: true);
            if (keepFilled)
            {
                StartMaintainRoutine();
            }
        }

        private void StartMaintainRoutine()
        {
            if (maintainRoutine != null)
            {
                StopCoroutine(maintainRoutine);
            }
            maintainRoutine = StartCoroutine(MaintainRoutine());
        }

        private IEnumerator MaintainRoutine()
        {
            while (item != null)
            {
                ApplyImbues(forceReload: false);
                yield return updateWait;
            }
        }

        public void ForceApply(bool forceReload)
        {
            ApplyImbues(forceReload);
            if (keepFilled && maintainRoutine == null)
            {
                StartMaintainRoutine();
            }
        }

        private bool ApplyImbues(bool forceReload)
        {
            if (!IIFModOptions.EnableMod)
            {
                IIFLog.Verbose("Apply skipped because framework is disabled.", debugLogging);
                return false;
            }
            string itemId = GetItemId();
            if (item == null || spells == null || spells.Count == 0)
            {
                IIFLog.Verbose($"No spells configured for item {itemId}.", debugLogging);
                return false;
            }
            List<Imbue> imbues = item.imbues;
            if (imbues == null || imbues.Count == 0)
            {
                IIFLog.Verbose($"No imbue slots found for item {itemId}.", debugLogging);
                return false;
            }
            Creature creature = ResolveImbuingCreature();
            bool appliedAny = false;
            for (int i = 0; i < imbues.Count; i++)
            {
                Imbue imbue = imbues[i];
                if (imbue == null || imbue.colliderGroup == null)
                {
                    continue;
                }
                if (imbue.colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.None)
                {
                    continue;
                }
                ImbueSpellConfig config = ResolveConfig(i);
                if (config == null || string.IsNullOrWhiteSpace(config.spellId))
                {
                    continue;
                }
                ApplyToImbue(imbue, config, creature, forceReload, i, itemId);
                appliedAny = true;
            }
            return appliedAny;
        }

        private void ApplyToImbue(Imbue imbue, ImbueSpellConfig config, Creature creature, bool forceReload, int imbueIndex, string itemId)
        {
            SpellCastCharge spellData = GetSpellData(config.spellId);
            if (spellData == null)
            {
                return;
            }
            string currentSpellId = imbue.spellCastBase?.id;
            bool spellMismatch = imbue.spellCastBase == null || !string.Equals(currentSpellId, spellData.id, StringComparison.OrdinalIgnoreCase);
            bool skipSwapForExternalConflict = false;
            bool stopEnergyWritesForConflict = false;

            if (spellMismatch && !forceReload && imbue.spellCastBase != null && conflictPolicy != ImbueConflictPolicy.ForceConfiguredSpell)
            {
                bool frameworkOwned = lastFrameworkSpellByImbueIndex.TryGetValue(imbueIndex, out string lastFrameworkSpellId) &&
                                      string.Equals(lastFrameworkSpellId, currentSpellId, StringComparison.OrdinalIgnoreCase);
                if (!frameworkOwned)
                {
                    IIFDiagnostics.RecordConflict();
                    if (IIFDiagnostics.ShouldLogThrottled($"conflict:{itemId}:{imbueIndex}:{currentSpellId}", 1f))
                    {
                        IIFLog.Info(
                            $"Conflict on {itemId}[{imbueIndex}] current={currentSpellId} desired={spellData.id} policy={conflictPolicy}",
                            debugLogging);
                    }
                    skipSwapForExternalConflict = true;
                    stopEnergyWritesForConflict = conflictPolicy == ImbueConflictPolicy.RespectExternalSpellNoEnergyWrite;
                }
            }

            if (spellMismatch && !skipSwapForExternalConflict)
            {
                IIFDiagnostics.RecordSwap();
                if (IIFDiagnostics.ShouldLogThrottled($"swap:{itemId}:{imbueIndex}:{spellData.id}", 0.5f))
                {
                    IIFLog.Info($"Imbue swap on {itemId}: {imbue.spellCastBase?.id ?? "None"} -> {spellData.id}", debugLogging);
                }
                imbue.UnloadCurrentSpell(true);
                imbue.allowImbue = true;
                imbue.Transfer(spellData, InitialTransferEnergy, creature);

                if (imbue.spellCastBase == null)
                {
                    IIFDiagnostics.RecordLoadFailure();
                    if (IIFDiagnostics.ShouldLogThrottled($"loadfail:{itemId}:{imbueIndex}:{spellData.id}", 2f))
                    {
                        IIFLog.Warn(
                            $"Imbue load failed on {itemId} for spell {spellData.id}. customSpellId={imbue.colliderGroup?.imbueCustomSpellID ?? "None"} allowImbue={imbue.allowImbue}",
                            debugLogging);
                    }
                    return;
                }

                currentSpellId = imbue.spellCastBase?.id;
                lastFrameworkSpellByImbueIndex[imbueIndex] = currentSpellId;
            }
            else if (!string.IsNullOrEmpty(currentSpellId) && string.Equals(currentSpellId, spellData.id, StringComparison.OrdinalIgnoreCase))
            {
                lastFrameworkSpellByImbueIndex[imbueIndex] = currentSpellId;
            }

            if (stopEnergyWritesForConflict)
            {
                return;
            }

            bool treatAsSpellMismatch = spellMismatch && !skipSwapForExternalConflict;
            if (!keepFilled && !forceReload && !treatAsSpellMismatch)
            {
                return;
            }

            float targetEnergy = GetTargetEnergy(imbue, config);
            float requestedEnergy = targetEnergy;
            bool shouldSetEnergy;

            if (forceReload || treatAsSpellMismatch)
            {
                shouldSetEnergy = Mathf.Abs(imbue.energy - targetEnergy) > 0.01f || forceReload;
            }
            else
            {
                float thresholdEnergy = targetEnergy * maintainBelowRatio;
                requestedEnergy = targetEnergy * refillToRatio;
                shouldSetEnergy = imbue.energy <= thresholdEnergy && Mathf.Abs(imbue.energy - requestedEnergy) > 0.01f;
            }

            if (!shouldSetEnergy)
            {
                return;
            }

            bool bypassCooldown = forceReload || treatAsSpellMismatch;
            if (!CanSetEnergyNow(imbueIndex, bypassCooldown))
            {
                IIFDiagnostics.RecordCooldownSkip();
                return;
            }

            if (IIFDiagnostics.ShouldLogEnergyWrite(itemId, imbueIndex, debugLogging))
            {
                IIFLog.Verbose($"Imbue energy set on {itemId}[{imbueIndex}]: {imbue.energy:0.##} -> {requestedEnergy:0.##}", debugLogging);
            }
            imbue.SetEnergyInstant(requestedEnergy);
            lastSetTimeByImbueIndex[imbueIndex] = Time.unscaledTime;
            IIFDiagnostics.RecordEnergyRefill();
        }

        private ImbueSpellConfig ResolveConfig(int index)
        {
            if (spells == null || spells.Count == 0)
            {
                return null;
            }
            switch (assignmentMode)
            {
                case ImbueAssignmentMode.Cycle:
                    return spells[index % spells.Count];
                case ImbueAssignmentMode.FirstOnly:
                    return index == 0 ? spells[0] : null;
                case ImbueAssignmentMode.RandomPerSpawn:
                    return spells[GetRandomSpellIndex(index)];
                case ImbueAssignmentMode.RoundRobinPerSpawn:
                    return spells[(currentRoundRobinStart + index) % spells.Count];
                case ImbueAssignmentMode.ConditionalHandVelocity:
                    return ResolveConditionalConfig();
                default:
                    if (index < spells.Count)
                    {
                        return spells[index];
                    }
                    return spells[spells.Count - 1];
            }
        }

        private ImbueSpellConfig ResolveConditionalConfig()
        {
            if (spells == null || spells.Count == 0)
            {
                return null;
            }

            float now = Time.unscaledTime;
            bool handed = item != null && item.IsHanded();
            float speed = 0f;
            if (item?.physicBody != null)
            {
                speed = item.physicBody.velocity.magnitude;
            }

            int desiredState;
            if (handed)
            {
                desiredState = 0;
            }
            else
            {
                float fastEnterThreshold = conditionalVelocityThreshold + conditionalVelocityHysteresis;
                float fastExitThreshold = Mathf.Max(0f, conditionalVelocityThreshold - conditionalVelocityHysteresis);

                if (conditionalState == 1)
                {
                    desiredState = speed >= fastExitThreshold ? 1 : 2;
                }
                else
                {
                    desiredState = speed >= fastEnterThreshold ? 1 : 2;
                }
            }

            if (conditionalState < 0 || desiredState == 0)
            {
                conditionalState = desiredState;
                conditionalLastStateSwitchTime = now;
            }
            else if (desiredState != conditionalState && now - conditionalLastStateSwitchTime >= conditionalMinSwitchInterval)
            {
                conditionalState = desiredState;
                conditionalLastStateSwitchTime = now;
            }

            int selectedIndex = GetConditionalSpellIndexFromState(conditionalState);
            selectedIndex = Mathf.Clamp(selectedIndex, 0, spells.Count - 1);
            return spells[selectedIndex];
        }

        private int GetConditionalSpellIndexFromState(int state)
        {
            switch (state)
            {
                case 1:
                    return spells.Count >= 2 ? 1 : 0;
                case 2:
                    return spells.Count >= 3 ? 2 : 0;
                default:
                    return 0;
            }
        }

        private int GetRandomSpellIndex(int imbueIndex)
        {
            if (randomSpellByImbueIndex.TryGetValue(imbueIndex, out int spellIndex))
            {
                return spellIndex;
            }

            spellIndex = UnityEngine.Random.Range(0, spells.Count);
            randomSpellByImbueIndex[imbueIndex] = spellIndex;
            return spellIndex;
        }

        private bool CanSetEnergyNow(int imbueIndex, bool bypassCooldown)
        {
            if (bypassCooldown || minSetEnergyInterval <= 0f)
            {
                return true;
            }

            if (!lastSetTimeByImbueIndex.TryGetValue(imbueIndex, out float lastSetTime))
            {
                return true;
            }

            return Time.unscaledTime - lastSetTime >= minSetEnergyInterval;
        }

        private string GetItemId()
        {
            return item?.data?.id ?? item?.itemId ?? item?.name ?? "UnknownItem";
        }

        private float GetTargetEnergy(Imbue imbue, ImbueSpellConfig config)
        {
            float targetEnergy = config.energy >= 0f ? config.energy : imbue.maxEnergy * Mathf.Clamp01(config.level);
            if (targetEnergy < 0f)
            {
                targetEnergy = 0f;
            }
            return Mathf.Min(targetEnergy, imbue.maxEnergy);
        }

        private SpellCastCharge GetSpellData(string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return null;
            }
            if (spellCache.TryGetValue(spellId, out SpellCastCharge cached))
            {
                return cached;
            }
            SpellCastCharge data = Catalog.GetData<SpellCastCharge>(spellId);
            if (data == null)
            {
                if (!missingSpellIds.Contains(spellId))
                {
                    missingSpellIds.Add(spellId);
                    IIFLog.Warn($"SpellCastCharge not found: {spellId}", debugLogging);
                }
                return null;
            }
            spellCache[spellId] = data;
            return data;
        }

        private Creature ResolveImbuingCreature()
        {
            if (item == null)
            {
                return null;
            }
            RagdollHand handler = item.mainHandler;
            if (handler != null && handler.creature != null)
            {
                return handler.creature;
            }
            if (Player.currentCreature != null)
            {
                return Player.currentCreature;
            }
            return null;
        }
    }
}
