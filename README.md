# <SolutionName> — Inventor 2026 Add-in

## Target
- Inventor: 2026
- Visual Studio: 2022
- Framework: .NET Framework 4.8
- Platform: x64

## Build and debug
1. Copy `.addin` to `%APPDATA%\Autodesk\Inventor 2026\Addins\` or keep beside the DLL during debug.
2. In VS: Project → Debug → Start external program → path to `Inventor.exe`.
3. Put a breakpoint in `ApplicationAddInServer.Activate`.

## Command summary
- <Your ribbon path and button ids here>

## DXF export policy
- Translator string example:


FLAT PATTERN DXF?AcadVersion=R12&OuterProfileLayer=Outer&SimplifySplines=True&TrimCenterlinesAtContour=True&RebaseGeometry=True

- Read-only parts: export via temporary copy if flat pattern needs creation or update.

## Review bundle
- CI creates `ReviewBundle.zip` on each PR in the Actions tab.
- Local: `pwsh ./scripts/Make-ReviewBundle.ps1`

## Notes
- CAD binaries tracked with Git LFS.