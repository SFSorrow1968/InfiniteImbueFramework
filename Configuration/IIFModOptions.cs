using ThunderRoad;

namespace InfiniteImbueFramework.Configuration
{
    public static class IIFModOptions
    {
        public const string VERSION = "0.1.0";

        public const string CategoryGeneral = "Infinite Imbue";
        public const string CategoryDiagnostics = "Diagnostics";

        public const string OptionEnableMod = "Enable Framework";
        public const string OptionLogLevel = "Log Level";
        public const string OptionDumpState = "Dump Imbue State";
        public const string OptionReapply = "Reapply Imbues";
        public const string OptionForceReload = "Force Reload Spells";
        public const string OptionDumpWeaponCatalog = "Dump Weapon Catalog";

        [ModOption(name = OptionEnableMod, category = CategoryGeneral, categoryOrder = 0, order = 0, defaultValueIndex = 1,
            tooltip = "Enable or disable the Infinite Imbue Framework globally")]
        public static bool EnableMod = true;

        [ModOption(name = OptionLogLevel, category = CategoryGeneral, categoryOrder = 0, order = 10, defaultValueIndex = 1, valueSourceName = nameof(LogLevelProvider),
            tooltip = "Logging detail level for Infinite Imbue Framework")]
        public static string LogLevel = "Basic";

        [ModOption(name = OptionDumpState, category = CategoryDiagnostics, categoryOrder = 10, order = 0, defaultValueIndex = 0,
            tooltip = "Log current imbue state for all active items")]
        [ModOptionDontSave]
        public static bool DumpState = false;

        [ModOption(name = OptionReapply, category = CategoryDiagnostics, categoryOrder = 10, order = 10, defaultValueIndex = 0,
            tooltip = "Reapply imbues using current configs (keeps current spell if matching)")]
        [ModOptionDontSave]
        public static bool Reapply = false;

        [ModOption(name = OptionForceReload, category = CategoryDiagnostics, categoryOrder = 10, order = 20, defaultValueIndex = 0,
            tooltip = "Force reload spells, then reapply imbue energy")]
        [ModOptionDontSave]
        public static bool ForceReload = false;

        [ModOption(name = OptionDumpWeaponCatalog, category = CategoryDiagnostics, categoryOrder = 10, order = 30, defaultValueIndex = 0,
            tooltip = "Export live weapon catalog report (JSON + CSV) to the game Logs/InfiniteImbueFramework folder")]
        [ModOptionDontSave]
        public static bool DumpWeaponCatalog = false;

        public static ModOptionString[] LogLevelProvider()
        {
            return new[]
            {
                new ModOptionString("Off", "Off"),
                new ModOptionString("Basic", "Basic"),
                new ModOptionString("Verbose", "Verbose")
            };
        }
    }
}
