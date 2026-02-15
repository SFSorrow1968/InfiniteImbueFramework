# Git Workflow

## 1. Branch Strategy
- **Feature Branches**: `agent/<topic>`. Never commit to `main`.

## 2. Session Workflow
1. Sync `main`.
2. Checkout feature branch.
3. **Execute Task**: Code, Test, Verify builds (`Release` & `Nomad`).
4. **Snapshot Commit**: Create a commit with a concise task description.
   - Example: `git commit -m "Implement Fireball feature/Snapshot"`
5. **Version Tag**: Create/update a tag pointing to this snapshot.
   - Example: `git tag -f v1.0.0-snapshot`
6. **Merge**: Merge to `main` only after validation.
