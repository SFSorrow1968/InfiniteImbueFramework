# Getting Started

This guide is the fastest path from "new weapon" to "working infinite imbue".

## Prerequisites

- Infinite Imbue Framework is installed and loaded by the game.
- Your item is an `ItemData` with at least one imbue-capable collider group.
- You know at least one valid spell ID (example: `Fire`, `Lightning`, `Gravity`).

## Step 1: Add The Module

Add this object inside your weapon's `"modules"` array:

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

## Step 2: Spawn-Test In Game

1. Start the game with your mod and IIF enabled.
2. Spawn the weapon.
3. Confirm the imbue is visible right after spawn.
4. Swing, hit, and wait a few seconds.
5. Confirm the imbue stays charged.

## Step 3: If It Fails, Capture Useful Logs

1. Open IIF Mod Options.
2. Set `Log Level` to `Verbose`.
3. Use `Reapply Imbues`.
4. Spawn or pick up the weapon again.
5. Collect fresh logs and share them.

## Common Next Upgrades

- Multiple effects: add more `spells` entries and use `assignmentMode: "Cycle"` or `"ByImbueIndex"`.
- Per-spawn variety: use `"RandomPerSpawn"` or `"RoundRobinPerSpawn"`.
- Compatibility mode: use `conflictPolicy: "RespectExternalSpell"` if another system also changes spells.

## Related Docs

- `_docs/Usage.md`
- `_docs/Templates/README.md`
- `_docs/Troubleshooting.md`
