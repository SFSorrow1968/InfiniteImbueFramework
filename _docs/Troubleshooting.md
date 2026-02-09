# Troubleshooting

Use this when a weapon does not behave as expected.

## Symptom: No Imbue On Spawn

Checks:
- Confirm your module `$type` is exactly `InfiniteImbueFramework.ItemModuleInfiniteImbue, InfiniteImbueFramework`.
- Confirm `applyOnSpawn` is `true`.
- Confirm your item has at least one imbue-enabled collider group.
- Confirm spell IDs exist in the loaded catalog.

Actions:
1. Set IIF `Log Level` to `Verbose`.
2. Trigger `Reapply Imbues`.
3. Spawn the weapon and inspect fresh logs.

## Symptom: Imbue Is Infinite But Initial VFX Is Weak Or Missing

Cause:
- The spell may apply after spawn but the starting energy/assignment is not what you expect.

Checks:
- Set `spells[].level` to `1.0` (or `spells[].energy` to a strong absolute value).
- Use `assignmentMode: "ByImbueIndex"` while debugging.
- Use `conflictPolicy: "ForceConfiguredSpell"` to rule out external overrides.

Actions:
1. Reapply imbues from the menu.
2. Despawn and respawn the weapon.
3. Verify VFX on each imbue-capable area.

## Symptom: Wrong Spell Appears

Checks:
- Another mod may be overriding spell assignment.
- Your assignment mode may rotate/randomize by design.

Actions:
1. Temporarily set `assignmentMode: "ByImbueIndex"`.
2. Temporarily set `conflictPolicy: "ForceConfiguredSpell"`.
3. Retest and compare logs before/after.

## Symptom: Only One Effect On A Multi-Element Weapon

Cause:
- Each imbue slot can hold one spell at a time.

Checks:
- Confirm the weapon actually has multiple imbue slots.
- Confirm you configured multiple `spells` entries.
- Confirm assignment mode is not `FirstOnly`.

Actions:
1. Run `Dump Weapon Catalog`.
2. Check `iif-weapon-summary-*.csv` for `imbueEnabledGroups` and `imbueEnabledModifiers`.
3. Check `iif-weapon-collider-modifiers-*.csv` for per-group imbue capability.

## Symptom: Refill Works But Performance Feels Heavy

Checks:
- `updateInterval` too small can over-update.
- `minSetEnergyInterval` too small can force frequent writes.

Recommended baseline:
- `updateInterval: 0.2`
- `maintainBelowRatio: 0.98`
- `refillToRatio: 1.0`
- `minSetEnergyInterval: 0.5`

## Log Collection Checklist

1. Use fresh logs only.
2. Include exact weapon item ID.
3. Include your full IIF module JSON.
4. Include whether issue happens on spawn, pickup, or both.
5. Include whether other spell/weapon mods are active.
