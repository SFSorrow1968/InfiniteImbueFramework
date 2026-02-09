# Weapon Catalog Export

This guide documents the `Dump Weapon Catalog` diagnostics action and the exported report files.

## Purpose

Use this export when you need a complete runtime snapshot of weapon-like items currently loaded by the game:
- Vanilla + active modded content.
- Weapon-like item types: `Weapon`, `Shield`, `Tool`.
- Imbue capability, collider modifiers, damagers, and module composition.

## How To Run

1. Start game with Infinite Imbue Framework enabled.
2. Open Mod Options.
3. Open Infinite Imbue Framework diagnostics.
4. Trigger `Dump Weapon Catalog`.
5. Wait for log line:
   - `[IIF] Weapon catalog export complete. Weapons: <N>. Output: <path>`

## Output Location

Exports are written to the game logs folder under `InfiniteImbueFramework`:

- PCVR:
  - `.../BladeAndSorcery_Data/StreamingAssets/Logs/InfiniteImbueFramework/`
- Nomad / Android:
  - `Android/data/<app-id>/files/Logs/InfiniteImbueFramework/`

## Generated Files

Each export creates timestamped files:

- `iif-weapon-report-<timestamp>.json`
- `iif-weapon-summary-<timestamp>.csv`
- `iif-weapon-collider-modifiers-<timestamp>.csv`
- `iif-weapon-damagers-<timestamp>.csv`
- `iif-weapon-modules-<timestamp>.csv`

## CSV Reference

### `iif-weapon-summary-<timestamp>.csv`

One row per weapon-like item.

- `id`: ItemData id.
- `displayName`: Display name string in ItemData.
- `type`: Item type (`Weapon`, `Shield`, `Tool`).
- `tier`: Item tier.
- `category`: Item category string.
- `value`: Item value.
- `mass`: Item mass.
- `colliderGroups`: Number of collider groups defined on item.
- `imbueEnabledGroups`: Number of collider groups with at least one non-`None` imbue modifier.
- `imbueEnabledModifiers`: Total count of non-`None` imbue modifiers across all groups.
- `damagers`: Number of damager entries on item.
- `moduleCount`: Number of modules on item.
- `hasIIF`: `true` if item has `ItemModuleInfiniteImbue`.
- `iifSpellCount`: Number of configured IIF spell entries.
- `iifAssignmentMode`: IIF assignment mode if present.
- `iifConflictPolicy`: IIF conflict policy if present.

### `iif-weapon-collider-modifiers-<timestamp>.csv`

One row per collider-group modifier (or a placeholder row when a group has no modifiers).

- `itemId`: ItemData id.
- `groupTransform`: Collider group transform name.
- `groupId`: ColliderGroupData id.
- `hasData`: Whether ColliderGroupData resolved.
- `supportsImbue`: Whether group has any non-`None` imbue modifier.
- `modifierIndex`: Modifier index (`-1` when no modifiers).
- `tierFilter`: Tier filter flags.
- `imbueType`: `None`, `Metal`, `Blade`, `Crystal`, or `Custom`.
- `imbueMax`: Max imbue energy for modifier.
- `imbueRate`: Imbue transfer rate.
- `imbueConstantLoss`: Constant imbue drain.
- `imbueHitLoss`: Energy loss per hit.
- `imbueVelocityLossPerSecond`: Velocity-based energy loss.
- `spellFilterLogic`: Spell filter logic value.
- `spellIds`: Semicolon-separated allowed/blocked spell IDs from modifier list.

### `iif-weapon-damagers-<timestamp>.csv`

One row per damager entry (or placeholder row if none).

- `itemId`: ItemData id.
- `transformName`: Damager transform.
- `damagerId`: DamagerData id.
- `hasData`: Whether DamagerData resolved.
- `playerMinDamage`: Player min damage.
- `playerMaxDamage`: Player max damage.
- `throwedMultiplier`: Throw damage multiplier.
- `penetrationAllowed`: Whether penetration is enabled.
- `penetrationDamage`: Penetration damage value.
- `dismembermentAllowed`: Whether dismemberment is enabled.
- `damageModifierId`: Damage modifier id from DamagerData.

### `iif-weapon-modules-<timestamp>.csv`

One row per module type (or placeholder row if none).

- `itemId`: ItemData id.
- `moduleIndex`: Module index (`-1` when no modules found).
- `moduleType`: Full runtime type name.

## JSON Reference

`iif-weapon-report-<timestamp>.json` contains the full structured report:
- Top-level counts and generation timestamp.
- Per-item object with:
  - summary values,
  - module type list,
  - full collider group objects and modifier lists,
  - full damager objects,
  - IIF module configuration values when present.

Use JSON if you need the most complete payload; use CSV for quick filtering and spreadsheet workflows.

## Typical Queries

- Find multi-imbue candidates:
  - Filter `iif-weapon-summary` where `imbueEnabledGroups > 1`.
- Find non-imbue-capable tools:
  - Filter `type = Tool` and `imbueEnabledGroups = 0`.
- Find items already using this framework:
  - Filter `hasIIF = true`.
