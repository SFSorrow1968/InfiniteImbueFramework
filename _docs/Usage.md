# Usage

If you are setting this up for the first time, start with `_docs/GettingStarted.md` first.

Add the ItemModule to any ItemData entry. The module will apply the configured spells and keep the imbues filled.

Example JSON:

```json
{
  "$type": "ThunderRoad.ItemData, ThunderRoad",
  "id": "MySword",
  "version": 4,
  "modules": [
    {
      "$type": "InfiniteImbueFramework.ItemModuleInfiniteImbue, InfiniteImbueFramework",
      "schemaVersion": 1,
      "spells": [
        { "spellId": "Fire", "level": 1.0 }
      ],
      "assignmentMode": "ByImbueIndex",
      "conflictPolicy": "ForceConfiguredSpell",
      "keepFilled": true,
      "updateInterval": 0.2,
      "maintainBelowRatio": 0.98,
      "refillToRatio": 1.0,
      "minSetEnergyInterval": 0.5,
      "conditionalVelocityThreshold": 6.0,
      "conditionalVelocityHysteresis": 1.0,
      "conditionalMinSwitchInterval": 0.25
    }
  ]
}
```

Multiple spells (mapped across imbue slots):

```json
{
  "$type": "ThunderRoad.ItemData, ThunderRoad",
  "id": "MySword",
  "version": 4,
  "modules": [
    {
      "$type": "InfiniteImbueFramework.ItemModuleInfiniteImbue, InfiniteImbueFramework",
      "schemaVersion": 1,
      "spells": [
        { "spellId": "Fire", "level": 1.0 },
        { "spellId": "Lightning", "level": 0.75 }
      ],
      "assignmentMode": "Cycle",
      "keepFilled": true,
      "maintainBelowRatio": 0.98
    }
  ]
}
```

Conditional behavior by hand/velocity:

```json
{
  "$type": "ThunderRoad.ItemData, ThunderRoad",
  "id": "MySword",
  "version": 4,
  "modules": [
    {
      "$type": "InfiniteImbueFramework.ItemModuleInfiniteImbue, InfiniteImbueFramework",
      "schemaVersion": 1,
      "spells": [
        { "spellId": "Fire", "level": 1.0 },
        { "spellId": "Lightning", "level": 1.0 },
        { "spellId": "Gravity", "level": 0.8 }
      ],
      "assignmentMode": "ConditionalHandVelocity",
      "conditionalVelocityThreshold": 6.0,
      "conditionalVelocityHysteresis": 1.0,
      "conditionalMinSwitchInterval": 0.25,
      "keepFilled": true
    }
  ]
}
```

Notes:
Multiple spell entries are distributed across the item's imbue slots based on assignmentMode.
For ConditionalHandVelocity:
- spells[0] is used while the item is held
- spells[1] is used when free and moving above conditionalVelocityThreshold
- spells[2] is optional fallback while free and slow

Conflict policy options:
- ForceConfiguredSpell: always enforce configured spell
- RespectExternalSpell: keep external spell changes but continue energy management
- RespectExternalSpellNoEnergyWrite: keep external spell changes and skip energy writes

## Mod Options

- Enable Framework (global toggle)
- Log Level (Off/Basic/Verbose)
- Dump Imbue State (logs all active items and imbues)
- Reapply Imbues (reapply current configs)
- Force Reload Spells (force reload then reapply)
- Dump Weapon Catalog (exports all live weapon-like item data to Logs/InfiniteImbueFramework)

## Validation

- Validate catalogs: `powershell -File _tools/Validate-IIFCatalogs.ps1 -Root . -CatalogPath Catalogs/Item`
- Strict mode: `powershell -File _tools/Validate-IIFCatalogs.ps1 -Root . -CatalogPath Catalogs/Item -Strict`
- Smoke checks: `powershell -File _agent/ci-smoke.ps1 -Strict`

## Presets

- Ready-to-copy module presets: `_docs/Templates/README.md`
