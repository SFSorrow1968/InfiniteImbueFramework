# Git Workflow

## 1. Source Of Truth

- Before any `git commit`, `git push`, or `git tag`, read this file and follow it exactly.
- Do not improvise snapshot naming or tagging outside this workflow.

## 2. Branch Strategy

- Never commit directly to `main`.
- Work on a feature branch (example: `agent/<topic>`).
- For each successful build snapshot, create a **new** snapshot branch:
  - `agent/snapshot-<YYYYMMDD-HHMMSS>-<topic>`
- Snapshot branches are immutable records:
  - Do not reuse existing snapshot branch names.
  - Do not force-push over existing snapshot branches.

## 3. Versioning Strategy (Required)

Update version values in both:

- `manifest.json` (`ModVersion`)
- `Core/<YourOptionsFile>.cs` (`Version`)

Rules:

- Gameplay/behavior/code changes: bump patch version (`A.B.C` -> `A.B.(C+1)`).
- 4-part version (`A.B.C.D`, e.g. `0.1.4.0`) is only used when explicitly requested for that release stream.

## 4. Snapshot Workflow (Per Successful Build)

1. Build and test (`Release` and `Nomad` project build).
2. Bump version using rules above.
3. Commit snapshot on current feature branch.
   - Example: `git commit -m "Implement X/Snapshot"`
4. Push feature branch.
   - Example: `git push -u origin agent/<topic>`
5. Create and push a **new immutable snapshot branch** from that commit.
   - Example:
     - `git checkout -b agent/snapshot-<timestamp>-<topic>`
     - `git push -u origin agent/snapshot-<timestamp>-<topic>`
6. Create and push a **new immutable snapshot tag**:
   - `v<VERSION>-snapshot-<YYYYMMDD-HHMMSS>`
   - Example:
     - `git tag v0.1.31-snapshot-20260218-114500`
     - `git push origin v0.1.31-snapshot-20260218-114500`

## 5. Forbidden Snapshot Actions

- No `git tag -f` for snapshot tags.
- No replacing or updating old snapshot tags.
- No reusing previous snapshot branch names.

## 6. Release Tags

- Release tags are separate from snapshot tags (example: `v0.1.31`).
- Create release tags only during release flow, not during routine snapshots.
