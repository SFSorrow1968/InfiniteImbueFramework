using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ThunderRoad;
using UnityEngine;

namespace InfiniteImbueFramework
{
    public static class IIFWeaponCatalogExporter
    {
        private const string ExportFolderName = "InfiniteImbueFramework";

        public static bool TryExport(out string exportDirectory, out int weaponCount, out string error)
        {
            exportDirectory = string.Empty;
            weaponCount = 0;
            error = string.Empty;

            try
            {
                List<ItemData> allItems = Catalog.GetDataList<ItemData>() ?? new List<ItemData>();
                List<ItemData> weapons = allItems
                    .Where(IsWeaponLike)
                    .OrderBy(item => item.id, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                weaponCount = weapons.Count;
                string logsRoot = FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Logs, string.Empty);
                if (string.IsNullOrWhiteSpace(logsRoot))
                {
                    logsRoot = Application.persistentDataPath;
                }
                exportDirectory = Path.Combine(logsRoot, ExportFolderName);
                Directory.CreateDirectory(exportDirectory);

                string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                List<WeaponEntry> entries = weapons.Select(BuildEntry).ToList();

                WriteSummaryCsv(Path.Combine(exportDirectory, $"iif-weapon-summary-{stamp}.csv"), entries);
                WriteColliderModifierCsv(Path.Combine(exportDirectory, $"iif-weapon-collider-modifiers-{stamp}.csv"), entries);
                WriteDamagerCsv(Path.Combine(exportDirectory, $"iif-weapon-damagers-{stamp}.csv"), entries);
                WriteModuleCsv(Path.Combine(exportDirectory, $"iif-weapon-modules-{stamp}.csv"), entries);
                WriteJsonReport(Path.Combine(exportDirectory, $"iif-weapon-report-{stamp}.json"), allItems.Count, entries);

                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }
        }

        private static bool IsWeaponLike(ItemData item)
        {
            if (item == null)
            {
                return false;
            }

            return item.type == ItemData.Type.Weapon ||
                   item.type == ItemData.Type.Shield ||
                   item.type == ItemData.Type.Tool;
        }

        private static WeaponEntry BuildEntry(ItemData item)
        {
            List<string> moduleTypes = new List<string>();
            if (item.modules != null)
            {
                for (int i = 0; i < item.modules.Count; i++)
                {
                    ItemModule module = item.modules[i];
                    if (module != null)
                    {
                        moduleTypes.Add(module.GetType().FullName ?? module.GetType().Name);
                    }
                }
            }

            bool hasIifModule = item.TryGetModule<ItemModuleInfiniteImbue>(out ItemModuleInfiniteImbue iifModule);
            List<string> iifSpellIds = new List<string>();
            if (hasIifModule && iifModule.spells != null)
            {
                for (int i = 0; i < iifModule.spells.Count; i++)
                {
                    ImbueSpellConfig cfg = iifModule.spells[i];
                    if (cfg != null && !string.IsNullOrWhiteSpace(cfg.spellId))
                    {
                        iifSpellIds.Add(cfg.spellId);
                    }
                }
            }

            List<ColliderGroupEntry> colliderGroups = new List<ColliderGroupEntry>();
            int imbueEnabledGroupCount = 0;
            int imbueEnabledModifierCount = 0;

            if (item.colliderGroups != null)
            {
                for (int i = 0; i < item.colliderGroups.Count; i++)
                {
                    ItemData.ColliderGroup colliderGroup = item.colliderGroups[i];
                    if (colliderGroup == null)
                    {
                        continue;
                    }

                    ColliderGroupData groupData = colliderGroup.colliderGroupData;
                    if (groupData == null && !string.IsNullOrWhiteSpace(colliderGroup.colliderGroupId))
                    {
                        groupData = Catalog.GetData<ColliderGroupData>(colliderGroup.colliderGroupId, false);
                    }

                    ColliderGroupEntry groupEntry = new ColliderGroupEntry
                    {
                        TransformName = colliderGroup.transformName ?? string.Empty,
                        ColliderGroupId = colliderGroup.colliderGroupId ?? string.Empty,
                        HasColliderGroupData = groupData != null,
                        CustomSpellEffects = groupData != null && groupData.customSpellEffects,
                        CustomSpellEffectCount = groupData?.customSpellEffectIDs?.Count ?? 0,
                        IgnoredImbueEffectModuleCount = groupData?.ignoredImbueEffectModules?.Length ?? 0
                    };

                    if (groupData?.modifiers != null)
                    {
                        for (int m = 0; m < groupData.modifiers.Count; m++)
                        {
                            ColliderGroupData.Modifier modifier = groupData.modifiers[m];
                            if (modifier == null)
                            {
                                continue;
                            }

                            ColliderGroupModifierEntry modifierEntry = new ColliderGroupModifierEntry
                            {
                                TierFilter = modifier.tierFilter.ToString(),
                                ImbueType = modifier.imbueType.ToString(),
                                ImbueMax = modifier.imbueMax,
                                ImbueRate = modifier.imbueRate,
                                ImbueConstantLoss = modifier.imbueConstantLoss,
                                ImbueHitLoss = modifier.imbueHitLoss,
                                ImbueVelocityLossPerSecond = modifier.imbueVelocityLossPerSecond,
                                SpellFilterLogic = modifier.spellFilterLogic.ToString(),
                                SpellIds = modifier.spellIds != null ? new List<string>(modifier.spellIds) : new List<string>()
                            };
                            groupEntry.Modifiers.Add(modifierEntry);

                            if (modifier.imbueType != ColliderGroupData.ImbueType.None)
                            {
                                groupEntry.ImbueEnabledModifierCount++;
                            }
                        }
                    }

                    groupEntry.SupportsImbue = groupEntry.ImbueEnabledModifierCount > 0;
                    if (groupEntry.SupportsImbue)
                    {
                        imbueEnabledGroupCount++;
                        imbueEnabledModifierCount += groupEntry.ImbueEnabledModifierCount;
                    }
                    colliderGroups.Add(groupEntry);
                }
            }

            List<DamagerEntry> damagers = new List<DamagerEntry>();
            if (item.damagers != null)
            {
                for (int i = 0; i < item.damagers.Count; i++)
                {
                    ItemData.Damager damager = item.damagers[i];
                    if (damager == null)
                    {
                        continue;
                    }

                    DamagerData damagerData = damager.damagerData;
                    if (damagerData == null && !string.IsNullOrWhiteSpace(damager.damagerID))
                    {
                        damagerData = Catalog.GetData<DamagerData>(damager.damagerID, false);
                    }

                    DamagerEntry damagerEntry = new DamagerEntry
                    {
                        TransformName = damager.transformName ?? string.Empty,
                        DamagerId = damager.damagerID ?? string.Empty,
                        HasDamagerData = damagerData != null
                    };

                    if (damagerData != null)
                    {
                        damagerEntry.PlayerMinDamage = damagerData.playerMinDamage;
                        damagerEntry.PlayerMaxDamage = damagerData.playerMaxDamage;
                        damagerEntry.ThrowedMultiplier = damagerData.throwedMultiplier;
                        damagerEntry.PenetrationAllowed = damagerData.penetrationAllowed;
                        damagerEntry.PenetrationDamage = damagerData.penetrationDamage;
                        damagerEntry.DismembermentAllowed = damagerData.dismembermentAllowed;
                        damagerEntry.DamageModifierId = damagerData.damageModifierId ?? string.Empty;
                    }
                    damagers.Add(damagerEntry);
                }
            }

            WeaponEntry entry = new WeaponEntry
            {
                Id = item.id ?? string.Empty,
                DisplayName = item.displayName ?? string.Empty,
                LocalizationId = item.localizationId ?? string.Empty,
                Author = item.author ?? string.Empty,
                Category = item.category ?? string.Empty,
                Type = item.type.ToString(),
                Tier = item.tier,
                ValueType = item.valueType ?? string.Empty,
                Value = item.value,
                RewardValue = item.rewardValue,
                LevelRequired = item.levelRequired,
                Flags = item.flags.ToString(),
                AllowedStorage = item.allowedStorage.ToString(),
                IsStackable = item.isStackable,
                Grippable = item.grippable,
                DrainImbueWhenIdle = item.drainImbueWhenIdle,
                Mass = item.mass,
                ThrowMultiplier = item.throwMultiplier,
                ColliderGroupCount = colliderGroups.Count,
                ImbueEnabledGroupCount = imbueEnabledGroupCount,
                ImbueEnabledModifierCount = imbueEnabledModifierCount,
                DamagerCount = damagers.Count,
                ModuleCount = moduleTypes.Count,
                ModuleTypes = moduleTypes,
                HasIifModule = hasIifModule,
                IifSpellIds = iifSpellIds,
                IifAssignmentMode = hasIifModule ? iifModule.assignmentMode.ToString() : string.Empty,
                IifConflictPolicy = hasIifModule ? iifModule.conflictPolicy.ToString() : string.Empty,
                IifKeepFilled = hasIifModule && iifModule.keepFilled,
                IifApplyOnSpawn = hasIifModule && iifModule.applyOnSpawn,
                IifUpdateInterval = hasIifModule ? iifModule.updateInterval : 0f,
                IifMaintainBelowRatio = hasIifModule ? iifModule.maintainBelowRatio : 0f,
                IifRefillToRatio = hasIifModule ? iifModule.refillToRatio : 0f,
                IifMinSetEnergyInterval = hasIifModule ? iifModule.minSetEnergyInterval : 0f,
                IifConditionalVelocityThreshold = hasIifModule ? iifModule.conditionalVelocityThreshold : 0f,
                IifConditionalVelocityHysteresis = hasIifModule ? iifModule.conditionalVelocityHysteresis : 0f,
                IifConditionalMinSwitchInterval = hasIifModule ? iifModule.conditionalMinSwitchInterval : 0f,
                ColliderGroups = colliderGroups,
                Damagers = damagers
            };

            return entry;
        }

        private static void WriteJsonReport(string path, int totalItemsInCatalog, List<WeaponEntry> entries)
        {
            WeaponCatalogReport report = new WeaponCatalogReport
            {
                GeneratedAtUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                TotalItemsInCatalog = totalItemsInCatalog,
                TotalWeaponLikeItems = entries.Count,
                Weapons = entries
            };

            string json;
            if (TrySerializeJson(report, out json))
            {
                File.WriteAllText(path, json, Encoding.UTF8);
                return;
            }

            // Fallback: write a minimal plain text representation if serializer is unavailable.
            StringBuilder fallback = new StringBuilder();
            fallback.AppendLine("JSON serializer unavailable; writing fallback summary.");
            fallback.AppendLine($"GeneratedAtUtc={report.GeneratedAtUtc}");
            fallback.AppendLine($"TotalItemsInCatalog={report.TotalItemsInCatalog}");
            fallback.AppendLine($"TotalWeaponLikeItems={report.TotalWeaponLikeItems}");
            for (int i = 0; i < entries.Count; i++)
            {
                WeaponEntry e = entries[i];
                fallback.AppendLine($"{e.Id}|{e.Type}|Tier{e.Tier}|ImbueGroups={e.ImbueEnabledGroupCount}|Damagers={e.DamagerCount}");
            }
            File.WriteAllText(path, fallback.ToString(), Encoding.UTF8);
        }

        private static bool TrySerializeJson(object value, out string json)
        {
            json = string.Empty;
            try
            {
                Type jsonConvertType = Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json");
                if (jsonConvertType == null)
                {
                    return false;
                }

                MethodInfo serializeOneArg = jsonConvertType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m =>
                    {
                        if (!string.Equals(m.Name, "SerializeObject", StringComparison.Ordinal))
                        {
                            return false;
                        }
                        ParameterInfo[] ps = m.GetParameters();
                        return ps.Length == 1 && ps[0].ParameterType == typeof(object);
                    });

                if (serializeOneArg == null)
                {
                    return false;
                }

                object result = serializeOneArg.Invoke(null, new[] { value });
                json = result as string ?? string.Empty;
                return !string.IsNullOrWhiteSpace(json);
            }
            catch
            {
                return false;
            }
        }

        private static void WriteSummaryCsv(string path, List<WeaponEntry> entries)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("id,displayName,type,tier,category,value,mass,colliderGroups,imbueEnabledGroups,imbueEnabledModifiers,damagers,moduleCount,hasIIF,iifSpellCount,iifAssignmentMode,iifConflictPolicy");
            for (int i = 0; i < entries.Count; i++)
            {
                WeaponEntry e = entries[i];
                sb.AppendLine(string.Join(",",
                    Csv(e.Id),
                    Csv(e.DisplayName),
                    Csv(e.Type),
                    e.Tier.ToString(CultureInfo.InvariantCulture),
                    Csv(e.Category),
                    e.Value.ToString("0.###", CultureInfo.InvariantCulture),
                    e.Mass.ToString("0.###", CultureInfo.InvariantCulture),
                    e.ColliderGroupCount.ToString(CultureInfo.InvariantCulture),
                    e.ImbueEnabledGroupCount.ToString(CultureInfo.InvariantCulture),
                    e.ImbueEnabledModifierCount.ToString(CultureInfo.InvariantCulture),
                    e.DamagerCount.ToString(CultureInfo.InvariantCulture),
                    e.ModuleCount.ToString(CultureInfo.InvariantCulture),
                    e.HasIifModule ? "true" : "false",
                    e.IifSpellIds.Count.ToString(CultureInfo.InvariantCulture),
                    Csv(e.IifAssignmentMode),
                    Csv(e.IifConflictPolicy)));
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static void WriteColliderModifierCsv(string path, List<WeaponEntry> entries)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("itemId,groupTransform,groupId,hasData,supportsImbue,modifierIndex,tierFilter,imbueType,imbueMax,imbueRate,imbueConstantLoss,imbueHitLoss,imbueVelocityLossPerSecond,spellFilterLogic,spellIds");
            for (int i = 0; i < entries.Count; i++)
            {
                WeaponEntry e = entries[i];
                for (int g = 0; g < e.ColliderGroups.Count; g++)
                {
                    ColliderGroupEntry group = e.ColliderGroups[g];
                    if (group.Modifiers.Count == 0)
                    {
                        sb.AppendLine(string.Join(",",
                            Csv(e.Id),
                            Csv(group.TransformName),
                            Csv(group.ColliderGroupId),
                            group.HasColliderGroupData ? "true" : "false",
                            group.SupportsImbue ? "true" : "false",
                            "-1", "", "", "", "", "", "", "", "", ""));
                        continue;
                    }

                    for (int m = 0; m < group.Modifiers.Count; m++)
                    {
                        ColliderGroupModifierEntry mod = group.Modifiers[m];
                        sb.AppendLine(string.Join(",",
                            Csv(e.Id),
                            Csv(group.TransformName),
                            Csv(group.ColliderGroupId),
                            group.HasColliderGroupData ? "true" : "false",
                            group.SupportsImbue ? "true" : "false",
                            m.ToString(CultureInfo.InvariantCulture),
                            Csv(mod.TierFilter),
                            Csv(mod.ImbueType),
                            mod.ImbueMax.ToString("0.###", CultureInfo.InvariantCulture),
                            mod.ImbueRate.ToString("0.###", CultureInfo.InvariantCulture),
                            mod.ImbueConstantLoss.ToString("0.###", CultureInfo.InvariantCulture),
                            mod.ImbueHitLoss.ToString("0.###", CultureInfo.InvariantCulture),
                            mod.ImbueVelocityLossPerSecond.ToString("0.###", CultureInfo.InvariantCulture),
                            Csv(mod.SpellFilterLogic),
                            Csv(string.Join(";", mod.SpellIds))));
                    }
                }
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static void WriteDamagerCsv(string path, List<WeaponEntry> entries)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("itemId,transformName,damagerId,hasData,playerMinDamage,playerMaxDamage,throwedMultiplier,penetrationAllowed,penetrationDamage,dismembermentAllowed,damageModifierId");
            for (int i = 0; i < entries.Count; i++)
            {
                WeaponEntry e = entries[i];
                if (e.Damagers.Count == 0)
                {
                    sb.AppendLine(string.Join(",", Csv(e.Id), "", "", "false", "", "", "", "", "", "", ""));
                    continue;
                }
                for (int d = 0; d < e.Damagers.Count; d++)
                {
                    DamagerEntry damager = e.Damagers[d];
                    sb.AppendLine(string.Join(",",
                        Csv(e.Id),
                        Csv(damager.TransformName),
                        Csv(damager.DamagerId),
                        damager.HasDamagerData ? "true" : "false",
                        damager.PlayerMinDamage.ToString("0.###", CultureInfo.InvariantCulture),
                        damager.PlayerMaxDamage.ToString("0.###", CultureInfo.InvariantCulture),
                        damager.ThrowedMultiplier.ToString("0.###", CultureInfo.InvariantCulture),
                        damager.PenetrationAllowed ? "true" : "false",
                        damager.PenetrationDamage.ToString("0.###", CultureInfo.InvariantCulture),
                        damager.DismembermentAllowed ? "true" : "false",
                        Csv(damager.DamageModifierId)));
                }
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static void WriteModuleCsv(string path, List<WeaponEntry> entries)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("itemId,moduleIndex,moduleType");
            for (int i = 0; i < entries.Count; i++)
            {
                WeaponEntry e = entries[i];
                if (e.ModuleTypes.Count == 0)
                {
                    sb.AppendLine(string.Join(",", Csv(e.Id), "-1", ""));
                    continue;
                }
                for (int m = 0; m < e.ModuleTypes.Count; m++)
                {
                    sb.AppendLine(string.Join(",", Csv(e.Id), m.ToString(CultureInfo.InvariantCulture), Csv(e.ModuleTypes[m])));
                }
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static string Csv(string value)
        {
            if (value == null)
            {
                return "\"\"";
            }
            string escaped = value.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }

        [Serializable]
        private class WeaponCatalogReport
        {
            public string GeneratedAtUtc;
            public int TotalItemsInCatalog;
            public int TotalWeaponLikeItems;
            public List<WeaponEntry> Weapons = new List<WeaponEntry>();
        }

        [Serializable]
        private class WeaponEntry
        {
            public string Id;
            public string DisplayName;
            public string LocalizationId;
            public string Author;
            public string Category;
            public string Type;
            public int Tier;
            public string ValueType;
            public float Value;
            public float RewardValue;
            public int LevelRequired;
            public string Flags;
            public string AllowedStorage;
            public bool IsStackable;
            public bool Grippable;
            public bool DrainImbueWhenIdle;
            public float Mass;
            public float ThrowMultiplier;
            public int ColliderGroupCount;
            public int ImbueEnabledGroupCount;
            public int ImbueEnabledModifierCount;
            public int DamagerCount;
            public int ModuleCount;
            public List<string> ModuleTypes = new List<string>();
            public bool HasIifModule;
            public List<string> IifSpellIds = new List<string>();
            public string IifAssignmentMode;
            public string IifConflictPolicy;
            public bool IifKeepFilled;
            public bool IifApplyOnSpawn;
            public float IifUpdateInterval;
            public float IifMaintainBelowRatio;
            public float IifRefillToRatio;
            public float IifMinSetEnergyInterval;
            public float IifConditionalVelocityThreshold;
            public float IifConditionalVelocityHysteresis;
            public float IifConditionalMinSwitchInterval;
            public List<ColliderGroupEntry> ColliderGroups = new List<ColliderGroupEntry>();
            public List<DamagerEntry> Damagers = new List<DamagerEntry>();
        }

        [Serializable]
        private class ColliderGroupEntry
        {
            public string TransformName;
            public string ColliderGroupId;
            public bool HasColliderGroupData;
            public bool SupportsImbue;
            public int ImbueEnabledModifierCount;
            public bool CustomSpellEffects;
            public int CustomSpellEffectCount;
            public int IgnoredImbueEffectModuleCount;
            public List<ColliderGroupModifierEntry> Modifiers = new List<ColliderGroupModifierEntry>();
        }

        [Serializable]
        private class ColliderGroupModifierEntry
        {
            public string TierFilter;
            public string ImbueType;
            public float ImbueMax;
            public float ImbueRate;
            public float ImbueConstantLoss;
            public float ImbueHitLoss;
            public float ImbueVelocityLossPerSecond;
            public string SpellFilterLogic;
            public List<string> SpellIds = new List<string>();
        }

        [Serializable]
        private class DamagerEntry
        {
            public string TransformName;
            public string DamagerId;
            public bool HasDamagerData;
            public float PlayerMinDamage;
            public float PlayerMaxDamage;
            public float ThrowedMultiplier;
            public bool PenetrationAllowed;
            public float PenetrationDamage;
            public bool DismembermentAllowed;
            public string DamageModifierId;
        }
    }
}
