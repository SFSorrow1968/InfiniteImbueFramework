using System.Collections.Generic;
using System;
using InfiniteImbueFramework.Configuration;
using ThunderRoad;
using UnityEngine;

namespace InfiniteImbueFramework
{
    public static class IIFDiagnostics
    {
        private const float UpdateIntervalSeconds = 0.1f;
        private const float SummaryIntervalSeconds = 5f;
        private const float VerboseEnergyLogIntervalSeconds = 1f;
        private const float ThrottleCleanupIntervalSeconds = 30f;
        private const int ThrottleCleanupCountThreshold = 256;
        private static float nextUpdateTime;
        private static float nextSummaryTime;
        private static float nextThrottleCleanupTime;
        private static bool lastEnableState = true;
        private static int swapsSinceSummary;
        private static int refillsSinceSummary;
        private static int loadFailuresSinceSummary;
        private static int cooldownSkipsSinceSummary;
        private static int conflictsSinceSummary;
        private static readonly Dictionary<string, float> throttledLogExpiry = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<string> expiredThrottleKeys = new List<string>(64);

        public static void Update()
        {
            float now = Time.unscaledTime;
            if (now < nextUpdateTime)
            {
                return;
            }
            nextUpdateTime = now + UpdateIntervalSeconds;

            if (IIFModOptions.EnableMod != lastEnableState)
            {
                lastEnableState = IIFModOptions.EnableMod;
                if (IIFModOptions.EnableMod)
                {
                    IIFLog.Info("Framework enabled via menu. Reapplying imbues.", true);
                    ReapplyAll(forceReload: true);
                }
                else
                {
                    IIFLog.Info("Framework disabled via menu.", true);
                }
            }

            if (IIFModOptions.DumpState)
            {
                IIFModOptions.DumpState = false;
                DumpState();
                ModManager.RefreshModOptionsUI();
            }

            if (IIFModOptions.Reapply)
            {
                IIFModOptions.Reapply = false;
                ReapplyAll(forceReload: false);
                ModManager.RefreshModOptionsUI();
            }

            if (IIFModOptions.ForceReload)
            {
                IIFModOptions.ForceReload = false;
                ReapplyAll(forceReload: true);
                ModManager.RefreshModOptionsUI();
            }

            if (IIFModOptions.DumpWeaponCatalog)
            {
                IIFModOptions.DumpWeaponCatalog = false;
                if (IIFWeaponCatalogExporter.TryExport(out string exportDir, out int weaponCount, out string error))
                {
                    IIFLog.Info($"Weapon catalog export complete. Weapons: {weaponCount}. Output: {exportDir}", true);
                }
                else
                {
                    IIFLog.Error($"Weapon catalog export failed: {error}");
                }
                ModManager.RefreshModOptionsUI();
            }

            CleanupThrottledLogCache(now);
            EmitSummary(now);
        }

        public static void ReapplyAll(bool forceReload)
        {
            if (!IIFModOptions.EnableMod)
            {
                IIFLog.Warn("Reapply skipped because the framework is disabled.", true);
                return;
            }

            IReadOnlyCollection<InfiniteImbueBehaviour> behaviours = InfiniteImbueBehaviour.ActiveBehaviours;
            if (behaviours == null || behaviours.Count == 0)
            {
                IIFLog.Info("No active InfiniteImbueBehaviour instances found.", true);
                return;
            }

            int applied = 0;
            foreach (InfiniteImbueBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }
                behaviour.ForceApply(forceReload);
                applied++;
            }

            IIFLog.Info($"Reapply complete. Items processed: {applied}. ForceReload: {forceReload}.", true);
        }

        public static void RecordSwap()
        {
            swapsSinceSummary++;
        }

        public static void RecordLoadFailure()
        {
            loadFailuresSinceSummary++;
        }

        public static void RecordEnergyRefill()
        {
            refillsSinceSummary++;
        }

        public static void RecordCooldownSkip()
        {
            cooldownSkipsSinceSummary++;
        }

        public static void RecordConflict()
        {
            conflictsSinceSummary++;
        }

        public static bool ShouldLogEnergyWrite(string itemId, int imbueIndex, bool force = false)
        {
            if (!force && !IIFLog.IsVerboseEnabled)
            {
                return false;
            }
            return ShouldLogThrottled($"energy:{itemId}:{imbueIndex}", VerboseEnergyLogIntervalSeconds);
        }

        public static bool ShouldLogThrottled(string key, float minIntervalSeconds)
        {
            float now = Time.unscaledTime;
            if (throttledLogExpiry.Count >= ThrottleCleanupCountThreshold && now >= nextThrottleCleanupTime)
            {
                CleanupThrottledLogCache(now);
            }

            if (throttledLogExpiry.TryGetValue(key, out float expiry) && now < expiry)
            {
                return false;
            }
            throttledLogExpiry[key] = now + Mathf.Max(0.05f, minIntervalSeconds);
            return true;
        }

        private static void CleanupThrottledLogCache(float now)
        {
            if (now < nextThrottleCleanupTime && throttledLogExpiry.Count < ThrottleCleanupCountThreshold)
            {
                return;
            }

            nextThrottleCleanupTime = now + ThrottleCleanupIntervalSeconds;
            if (throttledLogExpiry.Count == 0)
            {
                return;
            }

            expiredThrottleKeys.Clear();
            foreach (KeyValuePair<string, float> pair in throttledLogExpiry)
            {
                if (now >= pair.Value)
                {
                    expiredThrottleKeys.Add(pair.Key);
                }
            }

            for (int i = 0; i < expiredThrottleKeys.Count; i++)
            {
                throttledLogExpiry.Remove(expiredThrottleKeys[i]);
            }
        }

        private static void EmitSummary(float now)
        {
            if (now < nextSummaryTime)
            {
                return;
            }
            nextSummaryTime = now + SummaryIntervalSeconds;

            if (swapsSinceSummary == 0 && refillsSinceSummary == 0 && loadFailuresSinceSummary == 0 && cooldownSkipsSinceSummary == 0 && conflictsSinceSummary == 0)
            {
                return;
            }

            int active = InfiniteImbueBehaviour.ActiveBehaviours?.Count ?? 0;
            IIFLog.Info(
                $"Summary ({SummaryIntervalSeconds:0.#}s): active={active} swaps={swapsSinceSummary} refills={refillsSinceSummary} loadFailures={loadFailuresSinceSummary} cooldownSkips={cooldownSkipsSinceSummary} conflicts={conflictsSinceSummary}");

            swapsSinceSummary = 0;
            refillsSinceSummary = 0;
            loadFailuresSinceSummary = 0;
            cooldownSkipsSinceSummary = 0;
            conflictsSinceSummary = 0;
        }

        public static void DumpState()
        {
            IReadOnlyCollection<InfiniteImbueBehaviour> behaviours = InfiniteImbueBehaviour.ActiveBehaviours;
            int count = behaviours?.Count ?? 0;
            IIFLog.Info($"Dumping Infinite Imbue state. Active behaviours: {count}.", true);

            if (behaviours == null || behaviours.Count == 0)
            {
                return;
            }

            foreach (InfiniteImbueBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }
                Item item = behaviour.Item;
                string itemId = item?.data?.id ?? item?.itemId ?? "UnknownItem";
                string itemName = item?.name ?? "UnknownName";
                int imbueCount = item?.imbues?.Count ?? 0;

                IIFLog.Info($"Item: {itemName} (id: {itemId}) imbues: {imbueCount} applyOnSpawn: {behaviour.ApplyOnSpawn} keepFilled: {behaviour.KeepFilled} assignment: {behaviour.AssignmentMode} conflictPolicy: {behaviour.ConflictPolicy} interval: {behaviour.UpdateInterval:0.00}s maintainBelow={behaviour.MaintainBelowRatio:0.##} refillTo={behaviour.RefillToRatio:0.##} minSetInterval={behaviour.MinSetEnergyInterval:0.##}s velocityThreshold={behaviour.ConditionalVelocityThreshold:0.##} hysteresis={behaviour.ConditionalVelocityHysteresis:0.##} minStateSwitch={behaviour.ConditionalMinSwitchInterval:0.##}s", true);

                IReadOnlyList<ImbueSpellConfig> spells = behaviour.Spells;
                if (spells != null && spells.Count > 0)
                {
                    for (int i = 0; i < spells.Count; i++)
                    {
                        ImbueSpellConfig cfg = spells[i];
                        IIFLog.Info($"  Config[{i}] spellId={cfg.spellId} level={cfg.level:0.##} energy={cfg.energy:0.##}", true);
                    }
                }
                else
                {
                    IIFLog.Info("  Config: none", true);
                }

                if (item?.imbues == null || item.imbues.Count == 0)
                {
                    IIFLog.Info("  Imbues: none", true);
                    continue;
                }

                for (int i = 0; i < item.imbues.Count; i++)
                {
                    Imbue imbue = item.imbues[i];
                    if (imbue == null)
                    {
                        IIFLog.Info($"  Imbue[{i}] null", true);
                        continue;
                    }
                    string groupName = imbue.colliderGroup?.name ?? "UnknownGroup";
                    string spell = imbue.spellCastBase?.id ?? "None";
                    string customSpell = imbue.colliderGroup?.imbueCustomSpellID ?? string.Empty;
                    string imbueType = imbue.colliderGroup?.modifier.imbueType.ToString() ?? "Unknown";
                    IIFLog.Info($"  Imbue[{i}] group={groupName} type={imbueType} spell={spell} energy={imbue.energy:0.##}/{imbue.maxEnergy:0.##} allowImbue={imbue.allowImbue} customSpellId={customSpell}", true);
                }
            }
        }
    }
}
