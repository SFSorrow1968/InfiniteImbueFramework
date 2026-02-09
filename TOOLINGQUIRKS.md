# Tooling Quirks

## Entry 1

- **Issue**: The catalog validator can fail on older PowerShell environments.
- **Context**: `ConvertFrom-Json -Depth` is not available in Windows PowerShell 5.1.
- **Solution/Workaround**: Keep catalog validator scripts compatible with PowerShell 5.1 and avoid `ConvertFrom-Json -Depth`.
