# Agent Instructions

When making changes in this repo:
- Always build both targets before finalizing (`Release` for PCVR and `Nomad` for Nomad).
- Keep edits aligned with this repo's structure and docs (`DEVELOPMENT.md`, `QUIRKS.md`, `_docs/`).
- Use local references in `References/` and local tooling in `../.tools/` to decompile `../libs/*.dll` when API behavior is unclear.
- Shared game DLL path for this workspace is `D:\Documents\Projects\repos\BS\libs`.
- Build artifacts must end up in `bin/PCVR/<ModName>/` and `bin/Nomad/<ModName>/` (platform folder, then mod-name folder; no intermediate build folders).
- Treat `QUIRKS.md` as an index of theme-specific quirk logs, not a single catch-all file.
- Before deep refactors or debugging sessions, review `DEVELOPMENT.md`, `QUIRKS.md`, and the relevant `<THEME>QUIRKS.md` files.
- Add non-obvious findings to a specifically named quirk file such as `IMBUESQUIRKS.md`, `UIQUIRKS.md`, or `TOOLINGQUIRKS.md`.
- If a themed quirk file does not exist yet, create it with Issue/Context/Solution entries and add it to `QUIRKS.md`.
- If the user asks for a new project, scaffold a similar project structure plus build and git workflows, then tailor `AGENTS.md`, `DEVELOPMENT.md`, and themed quirk files to that project's domain.
- Use a feature branch for every task (for example `agent/<topic>`), and avoid direct work on `main`/`master`.
- End each substantial task update with a merge reminder that names the active feature branch and target branch.

- In BS batch mode, also consult root quirk docs: ../QUIRKS.md and _quirks/IIF_*QUIRKS.md.

