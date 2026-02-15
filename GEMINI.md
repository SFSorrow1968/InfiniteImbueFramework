# Agent Instructions

## 1. Central Planning (START HERE)
**Goal**: Minimize noise. Only read what you need.
**Rule**: For every C# file you touch (e.g. `MyFeature.cs`), you MUST manage its context files.

### Workflow:
1. **Identify** the target C# file/feature.
2. **Check** for companion files:
   - `[Feature]_VISION.md`
   - `[Feature]_QUIRKS.md`
   *(Look in `_visions/` or `_quirks/` subfolders near the code)*
3. **Action**:
   - **MISSING?** YOU MUST CREATE THEM immediately using templates in `D:\Documents\Projects\repos\BS\Docs\templates\`.
   - **PRESENT?** READ THEM. They contain the *Logging Strategy*, *Architecture*, and *Edge Cases* specific to this file.

## 2. Mod Resources (READ THESE)
- **Git Workflow**: `[_docs/GIT_WORKFLOW.md](file:///D:/Documents/Projects/repos/BS/Mods/Infinite%20Imbue%20Framework/_docs/GIT_WORKFLOW.md)`
- **Design Specs**: `[_docs/DESIGN.md](file:///D:/Documents/Projects/repos/BS/Mods/Infinite%20Imbue%20Framework/_docs/DESIGN.md)`
- **Publishing**: `[_docs/PUBLISH.md](file:///D:/Documents/Projects/repos/BS/Mods/Infinite%20Imbue%20Framework/_docs/PUBLISH.md)`
- **Tools**: `[_docs/TOOLS.md](file:///D:/Documents/Projects/repos/BS/Mods/Infinite%20Imbue%20Framework/_docs/TOOLS.md)`

## 3. Build & Artifacts
- **Targets**: Always build `Release` (PCVR) and `Nomad` (Nomad).
- **Shared Libs**: `D:\Documents\Projects\repos\BS\SDK\libs`
- **Output Paths**:
  - `bin/PCVR/Infinite Imbue Framework/`
  - `bin/Nomad/Infinite Imbue Framework/`
