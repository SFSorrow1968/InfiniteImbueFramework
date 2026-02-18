# Agent Instructions

## 1. Central Planning (START HERE)

**Goal**: Minimize noise. Only read what you need.
**Rule**: For every C# file you touch (e.g. `MyFeature.cs`), you MUST manage its context files.

### Workflow

1. **Identify** the target C# file/feature.
2. **Context**: Read `SCRATCHPAD.md` for active context.
3. **Check** for companion files:
   - `[Feature]_VISION.md`
   - `[Feature]_QUIRKS.md`
   - `[Feature]_RESOURCES.md`
   *(Look in `_visions/`, `_quirks/`, or `_resources/` subfolders near the code)*
4. **Action**:
   - **MISSING CONTEXT FILES?** RUN: `./_agent/bootstrap-context.ps1`
   - **NEED FEATURE COMPANIONS?** RUN: `./_agent/bootstrap-context.ps1 -Feature "<FeatureName>"`
   - **PRESENT?** READ THEM. They contain the *Logging Strategy*, *Architecture*, *Edge Cases*, and *Resources* specific to this file.
5. **Update**:
   - **COMPANION COMPLETENESS (REQUIRED)?** For every touched C# feature, before commit you MUST update all three companion files: [Feature]_VISION.md, [Feature]_QUIRKS.md, and [Feature]_RESOURCES.md.
   - **LEARNING?** If you learn something new or change architecture, YOU MUST update the companion files immediately.
   - **FINAL DOC CHECK?** Before finishing, add a short SCRATCHPAD.md note listing each touched C# feature and confirming VISION/QUIRKS/RESOURCES were updated.
   - **DOCS CHANGED?** If you change Global Docs (`AGENTS.md`, `_docs/`), YOU MUST check/update `WORKFLOW.txt`. (Ignore feature-specific `_visions/` & `_quirks/`).
   - **AGENTS.MD?** If you update this file, YOU MUST sync it to `GEMINI.md` and `CLAUDE.md`.

## 2. Mod Resources (Review if Needed)

**Rule**: Only read these if relevant to your current task.

- **Git Workflow**: `_docs/GIT_WORKFLOW.md` (**MANDATORY before any commit/tag/push**. Follow exactly: version bump + new immutable snapshot branch/tag each successful build.)
- **Design Specs**: `_docs/DESIGN.md` (Read if changing features)
- **Publishing**: `_docs/PUBLISH.md` (Read if preparing release)
- **Tools**: `_docs/TOOLS.md` (Read if using scripts)
- **Description**: `_docs/DESCRIPTION.md` (Read if updating mod description)

## 3. Build & Publish (Conditional)

**Rule**: Trigger this flow ONLY if you modified C# code (`*.cs`).

1. **Bootstrap**: `./_agent/bootstrap-context.ps1`
2. **Build + Test**: `./_agent/test.ps1 -Strict`
3. **Publish**: `./_agent/publish.ps1 -Force`
4. **Snapshot**: `./_agent/snapshot.ps1 -Message "snapshot: <topic>"`
5. **Workspace Router**: From `BS/`, run `./mod.ps1 "Infinite Imbue Framework" ship -Force`
   - *Output*: `bin/PCVR/InfiniteImbueFramework/` & `bin/Nomad/InfiniteImbueFramework/`
   - *Libs*: `D:\Documents\Projects\repos\BS\SDK\libs`

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
- **Parallel Agents**: Assume modified/untracked files may come from another active agent in the same repo.
- **Dirty Worktree Policy**: Do not halt solely because unexpected local changes exist.
- **Collaboration Rule**: Inspect overlap, preserve others' edits, and layer your changes without reverting unrelated files.
- **Escalation Rule**: Ask the user only when there is a true edit conflict or a destructive action is required.

## 6. Memory & Handoffs

**Rule**: Prevent "amnesia" between sessions.

1. **Check Scratchpad**: Read `SCRATCHPAD.md` at the start of every task to understand the broader context.
2. **Update Scratchpad**: Before finishing your session (or when blocking on user feedback), update `SCRATCHPAD.md` with:
   - **Current Status**: What is working, what is broken.
   - **Next Steps**: Precise instructions for the next agent.


