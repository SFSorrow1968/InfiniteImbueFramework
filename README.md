# Infinite Imbue Framework

Infinite Imbue Framework (IIF) lets modders add persistent imbues to any imbue-capable weapon/tool item with a single `ItemModule`.

## Quick Start

Add this module to any item catalog entry:

```json
{
  "$type": "InfiniteImbueFramework.ItemModuleInfiniteImbue, InfiniteImbueFramework",
  "schemaVersion": 1,
  "spells": [
    { "spellId": "Fire", "level": 1.0 }
  ],
  "assignmentMode": "ByImbueIndex",
  "conflictPolicy": "ForceConfiguredSpell",
  "applyOnSpawn": true,
  "keepFilled": true
}
```

## Documentation Map

- Main index: `_docs/INDEX.md`
- First weapon walkthrough: `_docs/GettingStarted.md`
- Usage and field reference: `_docs/Usage.md`
- Symptom-based fixes: `_docs/Troubleshooting.md`
- Weapon export workflow and CSV reference: `_docs/WeaponCatalogExport.md`
- Ready-to-copy module presets: `_docs/Templates/README.md`
- ThunderRoad decompile notes used by IIF: `_docs/DecompiledNotes.md`

## Project Layout

- Runtime module code: `Core/`
- Mod options and diagnostics menu integration: `Configuration/`
- Included test catalog patches: `Catalogs/Item/`
- Build and validation scripts: `_agent/` and `_tools/`
- External investigation artifacts: `References/`

## Validation And Build

- Catalog validation: `powershell -File _tools/Validate-IIFCatalogs.ps1 -Root . -CatalogPath Catalogs/Item`
- Strict validation: `powershell -File _tools/Validate-IIFCatalogs.ps1 -Root . -CatalogPath Catalogs/Item -Strict`
- Full smoke checks (validator + Release + Nomad): `powershell -File _agent/ci-smoke.ps1 -Strict`

## Included Test Item Patch

`Catalogs/Item/DaggerCommon.json` and `Catalogs/Item/ThrowablesDagger.json` include baseline IIF module examples for dagger spawn testing.

## Diagnostics Menu

- `Enable Framework`
- `Log Level` (`Off`, `Basic`, `Verbose`)
- `Dump Imbue State`
- `Reapply Imbues`
- `Force Reload Spells`
- `Dump Weapon Catalog`
