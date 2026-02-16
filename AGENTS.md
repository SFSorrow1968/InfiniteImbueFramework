# Agent Instructions

## 1. Central Planning (START HERE)

**Goal**: Minimize noise. Only read what you need.
**Rule**: For every C# file you touch (e.g. `MyFeature.cs`), you MUST manage its context files.

### Workflow

1. **Identify** the target C# file/feature.
2. **Check** for companion files:
   - `[Feature]_VISION.md`
   - `[Feature]_QUIRKS.md`
   - `[Feature]_RESOURCES.md`
   *(Look in `_visions/`, `_quirks/`, or `_resources/` subfolders near the code)*
3. **Action**:
   - **MISSING?** RUN: `copy "D:\Documents\Projects\repos\BS\Docs\templates\QUIRKS_TEMPLATE.md" "_quirks/[Feature]_QUIRKS.md"` (or `RESOURCES_TEMPLATE.md` to `_resources/[Feature]_RESOURCES.md`).
   - **PRESENT?** READ THEM. They contain the *Logging Strategy*, *Architecture*, *Edge Cases*, and *Resources* specific to this file.
4. **Update**:
   - **LEARNING?** If you learn something new or change architecture, YOU MUST update the companion files immediately.
   - **DOCS CHANGED?** If you change Global Docs (`AGENTS.md`, `_docs/`), YOU MUST check/update `WORKFLOW.txt`. (Ignore feature-specific `_visions/` & `_quirks/`).
   - **AGENTS.MD?** If you update this file, YOU MUST run `copy /Y AGENTS.md GEMINI.md` and `copy /Y AGENTS.md CLAUDE.md` to keep them in sync.

## 2. Mod Resources (Review if Needed)

**Rule**: Only read these if relevant to your current task.

- **Git Workflow**: `[_docs/GIT_WORKFLOW.md](file:///D:/Documents/Projects/repos/BS/Mods/Infinite%20Imbue%20Framework/_docs/GIT_WORKFLOW.md)` (Read if pushing code/builds)
- **Design Specs**: `[_docs/DESIGN.md](file:///D:/Documents/Projects/repos/BS/Mods/Infinite%20Imbue%20Framework/_docs/DESIGN.md)` (Read if changing features)
- **Publishing**: `[_docs/PUBLISH.md](file:///D:/Documents/Projects/repos/BS/Mods/Infinite%20Imbue%20Framework/_docs/PUBLISH.md)` (Read if preparing release)
- **Tools**: `[_docs/TOOLS.md](file:///D:/Documents/Projects/repos/BS/Mods/Infinite%20Imbue%20Framework/_docs/TOOLS.md)` (Read if using scripts)
- **Description**: `[_docs/DESCRIPTION.md](file:///D:/Documents/Projects/repos/BS/Mods/Infinite%20Imbue%20Framework/_docs/DESCRIPTION.md)` (Read if updating mod description)

## 3. Build & Publish (Conditional)

**Rule**: Trigger this flow ONLY if you modified C# code (`*.cs`).

1. **Build**: Use VS Code Task `Build Active Mod` (Ctrl+Shift+B).
   - *Output*: `bin/PCVR/Infinite Imbue Framework/` & `bin/Nomad/Infinite Imbue Framework/`
   - *Libs*: `D:\Documents\Projects\repos\BS\SDK\libs`
2. **Publish**: Use VS Code Task `Publish Active Mod` (or run `_agent/publish.ps1`).
3. **Snapshot**: IF build succeeds, you MUST commit. (See `GIT_WORKFLOW.md` for format)

## 4. Agentic Tools

The following extensions are configured to assist you:

- **Error Lens**: Warnings/Errors are shown inline. Trust them over manual compilation checks for quick feedback.
- **Todo Tree**: Scan for `TODO`, `FIXME`, `BUG` tags to find tasks.
- **GitLens**: Use it to understand code history and context.

## 5. Autonomy & Vibe Coding

- **Principle**: The user is here to "vibe code", not learn. The agent must do all the work to achieve the user's vision.
- **No Teaching**: Do not explain "how" or offer lessons. Just implement the solution.
- **Full Scope**: Handle all boilerplates, side-effects, and details effectively.
- **Default Behavior**: Always `ShouldAutoProceed: true` unless proposed action is destructive/irreversible.
