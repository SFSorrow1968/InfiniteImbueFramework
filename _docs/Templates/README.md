# Modder Template Pack

These are copy-paste `ItemModuleInfiniteImbue` presets.

How to use:
1. Open one preset file.
2. Copy the JSON object.
3. Paste it into your item's `"modules"` array.
4. Replace spell IDs and tuning values for your weapon.

Reference wrapper:

```json
{
  "$type": "ThunderRoad.ItemData, ThunderRoad",
  "id": "YourWeaponId",
  "version": 4,
  "modules": [
    {
      "$type": "InfiniteImbueFramework.ItemModuleInfiniteImbue, InfiniteImbueFramework",
      "schemaVersion": 1,
      "spells": [
        { "spellId": "Fire", "level": 1.0 }
      ]
    }
  ]
}
```

Presets:
- `Weapon-Baseline.module.json`: stable default for melee weapons.
- `MultiSlot-ElementSet.module.json`: multiple simultaneous imbues across multiple imbue slots.
- `RandomPerSpawn-Elemental.module.json`: random spell selection per spawn.
- `RoundRobinPerSpawn-Elemental.module.json`: deterministic rotating selection per spawn.
- `Conditional-HandVelocity.module.json`: held/fast/slow spell switching.
- `Compatibility-Safe.module.json`: respects external spell changes from other mods/systems.

Notes:
- A weapon can show multiple imbue effects at once if it has multiple imbue-enabled collider groups.
- Each imbue slot can hold one active spell at a time.
