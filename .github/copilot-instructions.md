# Copilot Instructions - Inventor Sheet Metal DXF Export Add-in

> **For GitHub Copilot Coding Agent**: This file provides context and guidelines for making changes to this repository. Follow these instructions when implementing features, fixing bugs, or refactoring code. Always build and test changes in Autodesk Inventor 2026 before submitting pull requests.

## Project Overview

This project is an Autodesk Inventor 2026 add-in that exports sheet metal flat patterns to DXF files with comprehensive UI options for layer control, feature selection, and metadata inclusion. Targeted at sheet metal fabrication workflows.

## Quick Reference

- **Language**: VB.NET
- **Framework**: .NET Framework 4.7.2
- **Target Platform**: Windows x64
- **IDE**: Visual Studio 2022
- **Build Tool**: MSBuild or dotnet CLI
- **CI/CD**: GitHub Actions (dotnet-desktop.yml, review-bundle.yml)

### Key Commands
```powershell
# Build and register (Administrator required)
.\build_and_register.ps1 -Configuration Release

# Build only
msbuild SheetMetalDXFExporter.sln /p:Configuration=Release

# Generate review bundle for PR review
pwsh ./scripts/Make-ReviewBundle.ps1
```

## Architecture & Key Components

- **Inventor Add-in Framework**: VB.NET/C# COM add-in using Autodesk Inventor API
- **Sheet Metal Engine**: `SheetMetalComponentDefinition` and `FlatPattern` manipulation
- **DXF Export Core**: `DataIO.WriteDataToFile()` with URL-style parameter strings
- **UI Framework**: WinForms/WPF interface for layer toggle and export options
- **Center Mark Generator**: Custom automation for hole center marks (not native points)

## Development Workflow

### Prerequisites
- .NET Framework 4.7.2 or later
- Visual Studio 2022 (or MSBuild tools)
- Autodesk Inventor 2026
- Administrator privileges for COM registration

### Build & Run
```powershell
# Recommended: Use the automated build and registration script (requires Administrator)
.\build_and_register.ps1 -Configuration Release

# Manual build with MSBuild
msbuild "SheetMetalDXFExporter.sln" /p:Configuration=Release /verbosity:minimal

# Manual build with dotnet CLI (alternative)
dotnet build "SheetMetalDXFExporter.sln" --configuration Release

# Register COM Add-in after building (requires Administrator)
regasm "bin\Release\SheetMetalDXFExporter.dll" /codebase

# Debug Setup in Visual Studio:
# Project → Debug → Start external program: point to Inventor.exe
# Copy .addin file to Inventor addins folder
# Use Add-In Manager in Inventor to unblock and load

# Deploy .addin manifest to scan folder
copy "SheetMetalDXFExporter.addin" "C:\ProgramData\Autodesk\Inventor 2026\Addins\"
```

### Testing
Currently, this project does not have automated tests. Manual testing is performed by:
1. Building and registering the add-in
2. Starting Autodesk Inventor 2026
3. Opening a sheet metal part (.ipt file)
4. Using the "Sheet Metal DXF Export" command from the Tools ribbon
5. Verifying DXF output with a DXF viewer or CAM software

**Note**: When adding tests in the future, follow VB.NET testing conventions using MSTest or NUnit.

### Code Formatting & Linting
- **EditorConfig**: Settings are defined in `.editorconfig` at repository root
  - Enforces 4-space indentation
  - CRLF line endings
  - UTF-8 encoding
  - Final newline insertion
- **Visual Studio**: Automatically applies EditorConfig settings
- **Code Analysis**: VB compiler warnings are suppressed selectively (see .vbproj NoWarn settings)
- No external linter is currently configured for VB.NET
- Follow Visual Studio's built-in code formatting (Ctrl+K, Ctrl+D)

### Inventor Add-in Development Setup
```powershell
# Install Inventor SDK Templates
# Navigate to: C:\Users\Public\Documents\Autodesk\Inventor 2026\SDK
# Run: DeveloperTools.msi

# Create new project using Autodesk template:
# New Project → "Autodesk Inventor AddIn" (Visual Studio template)

# Or manual setup:
# - Class Library (.NET Framework 4.8)
# - Add COM reference: Autodesk Inventor Object Library
# - Implement Inventor.ApplicationAddInServer
```

### Add-in Manifest (.addin file)
```xml
<?xml version="1.0" encoding="utf-8"?>
<Addin Type="Standard">
  <ClassId>{YOUR-GUID-HERE}</ClassId>
  <ClientId>{YOUR-GUID-HERE}</ClientId>
  <DisplayName>Sheet Metal DXF Exporter</DisplayName>
  <Description>Export sheet metal flat patterns to DXF with layer options</Description>
  <Assembly>SheetMetalDXFExporter.dll</Assembly>
  <FullClassName>SheetMetalDXFExporter.InventorAddIn</FullClassName>
  <LoadAutomatically>1</LoadAutomatically>
  <UserUnloadable>1</UserUnloadable>
  <SupportedSoftwareVersionGreaterThan>26..</SupportedSoftwareVersionGreaterThan>
</Addin>
```

### Deployment Locations
- All users: `C:\ProgramData\Autodesk\Inventor 2026\Addins\`
- Per user: `C:\Users\<user>\AppData\Roaming\Autodesk\Inventor 2026\Addins\`
- Version-independent: `C:\ProgramData\Autodesk\Inventor Addins\`

## Code Conventions

### VB.NET Style Guidelines
- **Indentation**: 4 spaces (enforced by .editorconfig)
- **Line endings**: CRLF (Windows)
- **Encoding**: UTF-8 for VB files, UTF-8 with BOM for C# files
- **Option Explicit**: On (always declare variables)
- **Option Strict**: Off (project setting)
- **Option Infer**: On (type inference enabled)
- **Option Compare**: Binary (case-sensitive comparisons)

### Naming Conventions
- **Classes**: PascalCase (e.g., `DXFExporter`, `SheetMetalScanner`)
- **Methods**: PascalCase (e.g., `ExportFlatPattern`, `ScanAssembly`)
- **Variables**: camelCase (e.g., `oCompDef`, `sFname`)
- **Inventor API objects**: Prefix with `o` (e.g., `oDoc`, `oCompDef`)
- **Strings**: Prefix with `s` (e.g., `sOut`, `sFilename`)
- **Constants**: UPPER_SNAKE_CASE or PascalCase

### File Organization
- **AddIn/**: COM add-in entry point, ribbon commands, event handlers
- **Export/**: DXF generation logic, DataIO wrappers
- **UI/**: WinForms/WPF forms, dialogs, user controls
- **Automation/**: Center mark generation, text block automation
- **Models/**: Data structures, settings classes, POCOs
- **Utils/**: Helper functions, extension methods, interop utilities

### Error Handling
- Use Try/Catch blocks for COM interop calls (Inventor API may throw)
- Log errors to debug output or file for troubleshooting
- Provide user-friendly error messages in UI dialogs
- Don't swallow exceptions silently - always log or notify

### COM Interop Best Practices
- Release COM objects promptly using `Marshal.ReleaseComObject()` when done
- Don't create circular references between .NET and COM objects
- Use `ComVisible(True)` only on classes that need to be exposed to Inventor
- Match GUIDs in .addin file with AssemblyInfo attributes

## Inventor Sheet Metal API Patterns

### Core DXF Export Pattern
```vb
' Standard export using DataIO with URL-style parameters
Dim sOut As String = "FLAT PATTERN DXF?AcadVersion=R12&RebaseGeometry=True" & _
    "&OuterProfileLayer=IV_OUTER_PROFILE&MergeProfilesIntoPolyline=True" & _
    "&SimplifySplines=True&TrimCenterlinesAtContour=True" & _
    "&InvisibleLayers=IV_UNCONSUMED_SKETCH_CONSTRUCTION"

' Ensure flat pattern exists and is current
If Not oCompDef.HasFlatPattern Then 
    oCompDef.Unfold()
Else
    oCompDef.FlatPattern.Edit()
    oCompDef.FlatPattern.ExitEdit()
End If

oCompDef.DataIO.WriteDataToFile(sOut, sFname)
```

### Complete Layer Parameters (Official Inventor 2026 API)
- `OuterProfileLayer` = "IV_OUTER_PROFILE" (main contour)
- `InteriorProfilesLayer` = "IV_INTERIOR_PROFILES" (internal profiles)
- `FeatureProfilesLayer` = "IV_FEATURE_PROFILES" (holes, slots - legacy)
- `FeatureProfilesUpLayer` = "IV_FEATURE_PROFILES" (up-facing features)
- `FeatureProfilesDownLayer` = "IV_FEATURE_PROFILES_DOWN" (down-facing features)
- `TangentLayer` = "IV_TANGENT" (tangent lines)
- `BendLayer` = "IV_BEND" (bend lines - legacy support)
- `BendUpLayer` = "IV_BEND" (up-direction bends)
- `BendDownLayer` = "IV_BEND_DOWN" (down-direction bends)
- `ToolCenterLayer` = "IV_TOOL_CENTER" (hole centers - legacy)
- `ToolCenterUpLayer` = "IV_TOOL_CENTER" (up-facing tool centers)
- `ToolCenterDownLayer` = "IV_TOOL_CENTER_DOWN" (down-facing tool centers)
- `ArcCentersLayer` = "IV_ARC_CENTERS" (arc center points)
- `AltRepFrontLayer` = "IV_ALTREP_FRONT" (alternate representation front)
- `AltRepBackLayer` = "IV_ALTREP_BACK" (alternate representation back)
- `UnconsumedSketchesLayer` = "IV_UNCONSUMED_SKETCHES" (unconsumed sketches)
- `UnconsumedSketchConstructionLayer` = "IV_UNCONSUMED_SKETCH_CONSTRUCTION" (construction lines)
- `TangentRollLinesLayer` = "IV_ROLL_TANGENT" (roll tangent lines)
- `RollLinesLayer` = "IV_ROLL" (roll lines)

### Layer Styling Parameters
- `***Color` = "R;G;B" format (e.g., `TangentLayerColor=255;0;0` for red)
- `***LineType` = Long value from LineTypeEnum (e.g., `TangentLayerLineType=37644`)
- `***LineWeight` = Double in centimeters (e.g., `TangentLayerLineWeight=0.1016`)

### Sheet Metal Features for UI Toggle
- **Bend Lines**: Manufacturing fold indicators
- **Etching**: Marking/engraving lines  
- **Text**: Part information, material specs
- **Hole Centers**: Automated center mark generation (points → marks)
- **Corner Relief**: Stress relief cuts
- **Form Features**: Embossed/coined features
- **Punch Tool Marks**: Special tooling indicators

### Center Mark Automation
```vb
' Problem: Inventor exports hole centers as POINT entities
' Solution: Generate custom center mark geometry (cross lines)
' Target layer: IV_TOOL_CENTER or custom layer for fabrication
```

## Metadata Block Requirements

### Essential Sheet Metal Data
- **Material**: Steel grade, aluminum type, thickness
- **Quantity**: Production count per assembly
- **Weight**: Calculated mass for material estimation
- **Size (Bounding Box)**: Overall dimensions for nesting
- **Surface Area**: For material utilization calculations
- **Bend Count**: Tooling setup requirements
- **Custom iProperties**: Part number, revision, date

### Implementation as DXF Block
```vb
' Create text entities in dedicated info layer
' Position in corner or separate area of flat pattern
' Include material properties from Inventor part
' Format for CNC/laser cutting software compatibility
```

## File Structure Guidelines

```
src/
├── AddIn/          # Inventor add-in entry point and registration
├── Export/         # DXF generation and DataIO wrapper
├── UI/            # Layer selection, feature toggles, options panel
├── Automation/    # Center mark generation, text block creation
├── Models/        # Data structures for export settings
└── Utils/         # Inventor API helpers, file operations
```

## Integration Points

- **Inventor COM API**: Primary interop assembly for sheet metal access
- **Assembly Scanning**: Recursive search for `SheetMetalComponentDefinition` parts
- **Flat Pattern Engine**: `HasFlatPattern`, `Unfold()`, `FlatPattern.Edit` workflow
- **iProperties System**: Material data, custom properties, BOM integration
- **File System Export**: Batch processing with custom naming conventions

### All Export Options Reference (Inventor 2026 API)
```vb
' Complete parameter reference from Autodesk documentation
Dim exportOptions As String = "FLAT PATTERN DXF?" & _
    "AcadVersion=R12" & _                           ' 2018|2013|2010|2007|2004|2000|R12
    "&SimplifySplines=True" & _                     ' Replace splines with lines/arcs
    "&SimplifyAsTangentArcs=False" & _              ' True=arcs, False=line segments  
    "&SplineTolerance=0.01" & _                     ' Chord tolerance (cm)
    "&MergeProfilesIntoPolyline=False" & _          ' Merge outer profiles to polyline
    "&RebaseGeometry=True" & _                      ' Move to first quadrant
    "&TrimCenterlinesAtContour=True" & _            ' Cleanup centerlines at boundaries
    "&InvisibleLayers=IV_UNCONSUMED_SKETCH_CONSTRUCTION" & _ ' Hidden layers (semicolon-separated)
    "&CustomizeFilename=" & _                       ' Custom naming pattern
    "&AdvancedLegacyExport=True"                    ' Legacy compatibility mode
```

## Debugging Tips

### Add-in Loading Issues
- Use **Add-In Manager** in Inventor to unblock and set Load Automatically
- Check that ClassId and ClientId match between .addin file and assembly attributes
- Ensure .addin file is in correct scan folder with proper XML format
- Set breakpoint in `Activate()` method to debug initialization failures
- Verify COM registration: Run `regasm` with `/codebase` flag as Administrator
- Check Windows Event Viewer for COM registration errors

### DXF Export Validation
- Test with known sheet metal parts to verify layer assignment
- Use DXF viewer to validate geometry and layer structure  
- Check flat pattern exists: `oCompDef.HasFlatPattern` before export
- Verify coordinate transformation with `RebaseGeometry=True`
- Compare exported DXF against Inventor's native DXF export as baseline

### Performance Optimization
- Close unused Inventor documents before batch export
- Use `AdvancedLegacyExport=False` for better performance on newer files
- Process large assemblies in smaller batches
- Consider background processing for extensive part lists

## Security & Best Practices

### Secrets and Sensitive Data
- **Never commit** passwords, API keys, or license files to the repository
- Use environment variables or encrypted config files for sensitive settings
- The `.gitignore` already excludes common sensitive files (`.env`, etc.)
- CAD binary files are tracked with Git LFS - ensure LFS is installed before cloning

### Code Security
- Validate all file paths to prevent path traversal attacks
- Sanitize user input before using in file operations or DXF output
- Use `Path.GetFullPath()` and `Path.Combine()` for safe path manipulation
- Don't execute arbitrary code from DXF files or user input

### Git LFS
- CAD binaries (.ipt, .iam files) are tracked with Git LFS
- Run `git lfs install` before first clone
- Run `git lfs fetch --all` to download all LFS objects
- Check `.gitattributes` for LFS-tracked file patterns

## Release Process

### Creating a Release
1. Update version number in `AssemblyInfo.vb`
2. Update CHANGELOG (if present) with release notes
3. Build in Release configuration: `.\build_and_register.ps1 -Configuration Release`
4. Test the release build thoroughly with sample parts
5. Create a GitHub release with the DLL and .addin file
6. Optionally create a review bundle: `pwsh ./scripts/Make-ReviewBundle.ps1`

### Review Bundle
- CI creates `ReviewBundle.zip` on each PR in the Actions tab
- Local generation: `pwsh ./scripts/Make-ReviewBundle.ps1`
- Bundle excludes `.git`, `.vs`, `bin`, `obj`, `packages`, and other build artifacts
- Use for code review by non-developers or offline review

---

**Note**: This file should be updated as the codebase grows. Focus on documenting:
1. Non-obvious architectural decisions and their rationale
2. DXF format specifics that affect implementation choices  
3. Performance considerations for large drawing exports
4. Error handling patterns for malformed input data
5. Testing strategies for validating DXF output correctness

## Guidelines for Making Changes

### Before Making Changes
1. **Build the project first** to ensure starting from a working state
2. **Review existing code patterns** in similar files before adding new code
3. **Check for existing utilities** in Utils/ before creating new helper functions
4. **Understand COM object lifetime** - improper cleanup causes memory leaks in Inventor

### When Adding Features
- Add new commands to `AddIn/` directory
- Add business logic to appropriate subdirectory (Export/, Automation/, etc.)
- Update ribbon UI if adding user-facing commands
- Create models in `Models/` for complex data structures
- Document non-obvious Inventor API usage with comments

### When Fixing Bugs
- Reproduce the issue manually in Inventor first
- Add debug logging to trace execution flow
- Check COM object references aren't causing issues
- Verify flat pattern state before export operations
- Test fix with multiple sheet metal parts (simple and complex)

### When Refactoring
- Keep changes minimal and focused
- Don't break existing Inventor API call patterns without testing
- Maintain backward compatibility with existing .addin files
- Test refactored code with actual Inventor workloads

### Pull Request Checklist
- [ ] Code builds successfully in both Debug and Release configurations
- [ ] Tested manually in Autodesk Inventor 2026
- [ ] No new compiler warnings introduced
- [ ] EditorConfig formatting applied
- [ ] COM objects properly released (no memory leaks)
- [ ] Error handling added for COM interop calls
- [ ] Comments added for complex Inventor API usage
