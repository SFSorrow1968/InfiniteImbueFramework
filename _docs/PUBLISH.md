# Publishing Workflow
Goal: Strict, safe, and reproducible process for Windows PowerShell 5.1.

## 1. Safety Checks
1.  **Check Working Tree**: Ensure no uncommitted changes.
    ```powershell
    git status
    ```
2.  **Verify Branch**: Ensure you are on the correct dev branch.
    ```powershell
    git branch --show-current
    ```

## 2. Versioning
1.  **Update Manifest**: Increment `ModVersion` in `manifest.json`.
    ```powershell
    # Verify content
    Get-Content manifest.json
    ```
2.  **Commit Manifest**:
    ```powershell
    git add manifest.json
    git commit -m "chore: bump version to v<VERSION>"
    ```
3.  **Annotated Tag**:
    ```powershell
    git tag -a v<VERSION> -m "Release v<VERSION>"
    ```

## 3. Merge to Main (Strict)
1.  **Switch & Merge**:
    ```powershell
    git checkout main
    git merge v<VERSION> --ff-only
    ```
2.  **Push**:
    ```powershell
    git push origin main v<VERSION>
    ```
3.  **Return**:
    ```powershell
    git checkout -
    ```

## 4. Build
**Prerequisite**: Ensure tags are pushed and tree is clean.

1.  **PCVR (Release)**:
    ```powershell
    dotnet build -c Release
    ```
2.  **Nomad**:
    ```powershell
    dotnet build -c Nomad
    ```
3.  **Verify**: Check `bin/PCVR/` and `bin/Nomad/` contain the expected DLLs and updated manifest.

## 5. Package
1.  **Zip PCVR**: Create `[ModName]_PCVR_v<VERSION>.zip` containing `bin/PCVR` contents.
2.  **Zip Nomad**: Create `[ModName]_Nomad_v<VERSION>.zip` containing `bin/Nomad` contents.

## 6. Release
1.  **GitHub Release**: Create a release targeting tag `v<VERSION>`. Upload both zips.
2.  **Nexus Mods**: Upload files.
    - **Generate Descriptions**: Refer to `_docs/nexus_description_guidelines.md`.
        - **Brief Description**: (See guidelines)
        - **Detailed Description**: (See guidelines)
