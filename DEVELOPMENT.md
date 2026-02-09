# Development Notes

- Target framework: net472.
- Builds output to bin/Release/PCVR/InfiniteImbueFramework/ and bin/Release/Nomad/InfiniteImbueFramework/.
- References are loaded from ..\libs.
- Core entry point: ItemModuleInfiniteImbue attaches InfiniteImbueBehaviour at runtime.
- Spell IDs must match SpellCastCharge IDs present in the catalog.
- Items must have imbue-enabled collider groups or the module will no-op.
- Canonical docs index: _docs/INDEX.md

## Runtime File Map

- Core/ItemModuleInfiniteImbue.cs: catalog-facing module config and validation.
- Core/InfiniteImbueBehaviour.cs: runtime imbue assignment, refill loop, and conflict-policy handling.
- Core/IIFDiagnostics.cs: diagnostics handlers backing mod options actions.
- Core/IIFWeaponCatalogExporter.cs: full weapon-like catalog export (JSON + CSVs).
- Configuration/IIFModOptions.cs: options menu entries and action wiring.
