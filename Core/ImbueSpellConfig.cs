using System;

namespace InfiniteImbueFramework
{
    [Serializable]
    public class ImbueSpellConfig
    {
        public string spellId;
        public float level = 1f;
        public float energy = -1f;
    }

    public enum ImbueAssignmentMode
    {
        ByImbueIndex,
        Cycle,
        FirstOnly,
        RandomPerSpawn,
        RoundRobinPerSpawn,
        ConditionalHandVelocity
    }

    public enum ImbueConflictPolicy
    {
        ForceConfiguredSpell,
        RespectExternalSpell,
        RespectExternalSpellNoEnergyWrite
    }
}
