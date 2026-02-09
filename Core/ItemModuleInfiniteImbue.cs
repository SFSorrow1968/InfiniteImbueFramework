using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace InfiniteImbueFramework
{
    public class ItemModuleInfiniteImbue : ItemModule
    {
        private static readonly HashSet<string> validationWarnings = new HashSet<string>();

        public List<ImbueSpellConfig> spells = new List<ImbueSpellConfig>();
        public ImbueAssignmentMode assignmentMode = ImbueAssignmentMode.ByImbueIndex;
        public ImbueConflictPolicy conflictPolicy = ImbueConflictPolicy.ForceConfiguredSpell;
        public bool applyOnSpawn = true;
        public bool keepFilled = true;
        public float updateInterval = 0.2f;
        public int schemaVersion = 1;
        public float maintainBelowRatio = 0.98f;
        public float refillToRatio = 1f;
        public float minSetEnergyInterval = 0.5f;
        public float conditionalVelocityThreshold = 6f;
        public float conditionalVelocityHysteresis = 1f;
        public float conditionalMinSwitchInterval = 0.25f;
        public bool debugLogging;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            ValidateModule(item);

            InfiniteImbueBehaviour behaviour = item.gameObject.GetComponent<InfiniteImbueBehaviour>();
            if (!behaviour)
            {
                behaviour = item.gameObject.AddComponent<InfiniteImbueBehaviour>();
            }
            behaviour.Init(item, this);
        }

        private void ValidateModule(Item item)
        {
            string itemId = item?.data?.id ?? item?.itemId ?? "UnknownItem";
            if (spells == null || spells.Count == 0)
            {
                WarnOnce($"{itemId}:spells-empty", $"Item '{itemId}' has ItemModuleInfiniteImbue but no spells configured.");
                return;
            }

            if (schemaVersion != 1)
            {
                WarnOnce($"{itemId}:schema-version", $"Item '{itemId}' uses schemaVersion={schemaVersion}. This framework currently targets schemaVersion=1.");
            }

            int validSpellEntries = 0;
            for (int i = 0; i < spells.Count; i++)
            {
                ImbueSpellConfig cfg = spells[i];
                if (cfg == null || string.IsNullOrWhiteSpace(cfg.spellId))
                {
                    WarnOnce($"{itemId}:spell-empty:{i}", $"Item '{itemId}' spell config at index {i} is missing spellId.");
                    continue;
                }

                validSpellEntries++;
                if (Catalog.GetData<SpellCastCharge>(cfg.spellId, false) == null)
                {
                    WarnOnce($"{itemId}:spell-missing:{cfg.spellId}", $"Item '{itemId}' references missing SpellCastCharge id '{cfg.spellId}'.");
                }
            }

            if (validSpellEntries == 0)
            {
                WarnOnce($"{itemId}:spells-invalid", $"Item '{itemId}' has no valid spell entries.");
            }

            if (assignmentMode == ImbueAssignmentMode.RandomPerSpawn && spells.Count < 2)
            {
                WarnOnce($"{itemId}:mode-random-few", $"Item '{itemId}' uses RandomPerSpawn with fewer than 2 spells; behavior will look identical to static assignment.");
            }

            if (assignmentMode == ImbueAssignmentMode.RoundRobinPerSpawn && spells.Count < 2)
            {
                WarnOnce($"{itemId}:mode-roundrobin-few", $"Item '{itemId}' uses RoundRobinPerSpawn with fewer than 2 spells; behavior will look identical to static assignment.");
            }

            if (assignmentMode == ImbueAssignmentMode.ConditionalHandVelocity && spells.Count < 2)
            {
                WarnOnce($"{itemId}:mode-conditional-few", $"Item '{itemId}' uses ConditionalHandVelocity with fewer than 2 spells; configure held/velocity spells in spells[0]/spells[1].");
            }

            if (maintainBelowRatio < 0f || maintainBelowRatio > 1f)
            {
                WarnOnce($"{itemId}:maintainBelowRatio-range", $"Item '{itemId}' maintainBelowRatio={maintainBelowRatio:0.###} is outside 0..1.");
            }

            if (refillToRatio < 0f || refillToRatio > 1f)
            {
                WarnOnce($"{itemId}:refillToRatio-range", $"Item '{itemId}' refillToRatio={refillToRatio:0.###} is outside 0..1.");
            }

            if (refillToRatio < maintainBelowRatio)
            {
                WarnOnce($"{itemId}:refill-below-maintain", $"Item '{itemId}' refillToRatio={refillToRatio:0.###} is below maintainBelowRatio={maintainBelowRatio:0.###}.");
            }

            if (minSetEnergyInterval < 0f)
            {
                WarnOnce($"{itemId}:minSetEnergyInterval-negative", $"Item '{itemId}' minSetEnergyInterval={minSetEnergyInterval:0.###} must be >= 0.");
            }

            if (conditionalVelocityThreshold < 0f)
            {
                WarnOnce($"{itemId}:conditionalVelocityThreshold-negative", $"Item '{itemId}' conditionalVelocityThreshold={conditionalVelocityThreshold:0.###} must be >= 0.");
            }

            if (conditionalVelocityHysteresis < 0f)
            {
                WarnOnce($"{itemId}:conditionalVelocityHysteresis-negative", $"Item '{itemId}' conditionalVelocityHysteresis={conditionalVelocityHysteresis:0.###} must be >= 0.");
            }

            if (conditionalMinSwitchInterval < 0f)
            {
                WarnOnce($"{itemId}:conditionalMinSwitchInterval-negative", $"Item '{itemId}' conditionalMinSwitchInterval={conditionalMinSwitchInterval:0.###} must be >= 0.");
            }

            if (item?.imbues == null || item.imbues.Count == 0)
            {
                WarnOnce($"{itemId}:imbues-missing", $"Item '{itemId}' has no imbue slots detected on load.");
                return;
            }

            int validImbueSlots = 0;
            for (int i = 0; i < item.imbues.Count; i++)
            {
                Imbue imbue = item.imbues[i];
                if (imbue?.colliderGroup == null)
                {
                    continue;
                }
                if (imbue.colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.None)
                {
                    continue;
                }
                validImbueSlots++;
            }

            if (validImbueSlots == 0)
            {
                WarnOnce($"{itemId}:imbues-invalid", $"Item '{itemId}' has imbue slots, but none allow imbues (ImbueType.None).");
            }
        }

        private static void WarnOnce(string key, string message)
        {
            if (validationWarnings.Add(key))
            {
                IIFLog.Warn(message, true);
            }
        }
    }
}
