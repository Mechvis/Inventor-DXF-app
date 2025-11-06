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
- **Dependencies**:
  - Autodesk Inventor 2026 Object Library (COM reference)
  - System.Data.SQLite (NuGet package for export history)
  - Standard .NET Framework libraries

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

- **Inventor Add-in Framework**: VB.NET COM add-in using Autodesk Inventor API
- **Sheet Metal Engine**: `SheetMetalComponentDefinition` and `FlatPattern` manipulation
- **DXF Export Core**: `DataIO.WriteDataToFile()` with URL-style parameter strings
- **UI Framework**: WinForms interface for layer toggle and export options with preview pane
- **Center Mark Generator**: Custom automation for hole center marks (not native points)
- **Metadata Block Generator**: Automatic text block creation with part properties
- **Export History System**: SQLite database tracking all exports with automatic archival
- **iProperties Integration**: Extraction of Part Number, Stock Number, and Revision from Inventor properties

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

### Documentation Structure
- **README.md**: Quick start guide and project overview
- **docs/EXPORT_HISTORY.md**: Export history and archival system documentation
- **docs/IMPLEMENTATION_SUMMARY.md**: Detailed implementation notes for export duplication feature
- **docs/WORKFLOW_DIAGRAM.md**: Visual workflow and process diagrams
- **.github/copilot-instructions.md**: This file - comprehensive development guidelines
- **Resources/**: API reference documentation (see Resources Folder Purpose section)

### Inventor Add-in Development Setup
```powershell
# Install Inventor SDK Templates
# Navigate to: C:\Users\Public\Documents\Autodesk\Inventor 2026\SDK
# Run: DeveloperTools.msi

# Create new project using Autodesk template:
# New Project → "Autodesk Inventor AddIn" (Visual Studio template)

# Or manual setup:
# - Class Library (.NET Framework 4.7.2 or higher)
# - Add COM reference: Autodesk Inventor Object Library
# - Implement Inventor.ApplicationAddInServer
# - Add NuGet package: System.Data.SQLite (for export history)
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
  - `InventorAddIn.vb` - Main add-in class implementing ApplicationAddInServer
  - `DXFExportCommand.vb` - Ribbon command implementation
- **Export/**: DXF generation logic, DataIO wrappers
  - `DXFExporter.vb` - Core export logic with history and archival
- **UI/**: WinForms dialogs and user controls
  - `ExportOptionsDialog.vb` - Main export configuration dialog
  - `DxfPreviewPane.vb` - Preview control for DXF thumbnails
- **Automation/**: Center mark generation, text block creation
  - `CenterMarkGenerator.vb` - Custom center mark geometry creation
  - `MetadataBlockGenerator.vb` - Part information text block insertion
- **Models/**: Data structures, settings classes, POCOs
  - `ExportSettings.vb` - Export configuration and options
  - `SheetMetalPart.vb` - Part data model with iProperties extraction
  - `ExportHistoryEntry.vb` - Database record model
  - `DxfPreviewModels.vb` - Preview-related data structures
- **Utils/**: Helper functions, extension methods, interop utilities
  - `SheetMetalScanner.vb` - Assembly scanning for sheet metal parts
  - `ExportHistoryService.vb` - SQLite database service for export tracking
  - `DXFSanitizer.vb` - DXF file cleanup and validation
  - `InteropHelpers.vb` - COM interop utilities and safe object release
- **Resources/**: Reference documentation and configuration templates
  - `HTML/` - Offline Inventor API documentation (2+ million lines)
  - `*.xml` - DXF/DWG layer configuration templates
  - `*.ini` - Export/import configuration templates
  - `*.pdf` - API documentation and code samples

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

### iProperties Integration

The add-in extracts and uses several iProperties for intelligent file naming and metadata:

**Part Number Extraction** (priority order):
1. Design Tracking Properties → "Part Number"
2. Inventor User Defined Properties → "Part Number"
3. Fallback: Filename without extension

**Stock Number Extraction** (checks multiple property names):
- Custom properties: "Stock Number", "Stock No", "Stock", "StockNo", "Material Stock"
- Fallback: Formatted thickness (e.g., "2.0mm")

**Revision Extraction**:
- Design Tracking Properties → "Revision Number"
- Fallback: Empty string

**Usage in File Naming**:
```vb
' Default filename template: {PartNumber}_{StockNumber}_{Rev}
' Example output: 073-102-1002_2.0mm_A.dxf
' If Part Number = "073-102-1002", Stock Number = "2.0mm", Rev = "A"
```

**Best Practice**: Always set Part Number in iProperties for consistent, meaningful filenames. Stock Number is optional but recommended for shops using standardized material stock codes.

### Export History and Archival System

The add-in includes an automatic export tracking and archival system:

**Key Features**:
- SQLite database (`ExportHistory.db`) tracks all exports with unique IDs
- Automatic detection of duplicate exports based on Part Number, Thickness, and Revision
- Existing files archived to `_Archive/` folder before re-export
- Archived files renamed with timestamp: `PartNumber_StockNumber_Rev_YYYY-MM-DD_HHMMSS.dxf`
- File hash (SHA256) computation for content-based deduplication

**Database Schema**:
```sql
CREATE TABLE ExportHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartName TEXT NOT NULL,        -- Part Number from iProperties
    Material TEXT,
    Thickness REAL,
    Revision TEXT,
    FilePath TEXT NOT NULL,
    ExportDate TEXT NOT NULL,
    IsArchived INTEGER NOT NULL DEFAULT 0,
    ArchivePath TEXT,
    ExportedBy TEXT,
    FileHash TEXT
);
```

**Configuration** (in ExportSettings.vb):
```vb
' Enable export history tracking
EnableExportHistory = True

' Enable automatic archival of duplicates
ArchiveDuplicates = True

' Archive folder name (relative to export folder)
ArchiveFolderName = "_Archive"
```

**Implementation Notes**:
- Export history service is in `Utils/ExportHistoryService.vb`
- Database is created automatically on first use in the export folder
- Archive folder is excluded from version control via `.gitignore`
- See `docs/EXPORT_HISTORY.md` for complete documentation

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
│   ├── InventorAddIn.vb
│   └── DXFExportCommand.vb
├── Export/         # DXF generation and DataIO wrapper
│   └── DXFExporter.vb
├── UI/             # Layer selection, feature toggles, options panel
│   ├── ExportOptionsDialog.vb
│   └── DxfPreviewPane.vb
├── Automation/     # Center mark generation, text block creation
│   ├── CenterMarkGenerator.vb
│   └── MetadataBlockGenerator.vb
├── Models/         # Data structures for export settings
│   ├── ExportSettings.vb
│   ├── SheetMetalPart.vb
│   ├── ExportHistoryEntry.vb
│   └── DxfPreviewModels.vb
├── Utils/          # Inventor API helpers, file operations
│   ├── SheetMetalScanner.vb
│   ├── ExportHistoryService.vb
│   ├── DXFSanitizer.vb
│   └── InteropHelpers.vb
└── Resources/      # Reference documentation and configuration
    ├── HTML/       # Offline Inventor 2026 API documentation
    ├── *.xml       # Layer configuration templates
    ├── *.ini       # Export/import configuration templates
    └── *.pdf       # API reference PDFs
```

### Resources Folder Purpose

The **Resources/** folder contains reference materials and configuration templates that assist during development:

1. **HTML Documentation** (`Resources/HTML/`)
   - Complete offline copy of Autodesk Inventor 2026 API documentation
   - Over 2 million lines of reference material covering all API objects
   - Useful for offline development and quick API lookups
   - Not embedded in the application - reference material only

2. **XML Configuration Templates** (`Resources/*.xml`)
   - `FlatPattern.xml` - DXF/DWG layer editing instructions template
   - `FlatPatternExportOpts.xml` - Export options configuration template
   - `FaceLoops.xml` - Face loop processing configuration
   - `MechVisionDXF.xml` - Custom DXF export configuration
   - Used as reference for understanding Inventor's layer customization system

3. **INI Configuration Files** (`Resources/*.ini`)
   - `exportdxf.ini` - DXF export configuration template
   - `exportdwg.ini` - DWG export configuration template
   - `importdxf.ini`, `importacad.ini`, `importmdt.ini` - Import templates
   - Show available export/import parameters and line type mappings
   - Reference for understanding DataIO parameter strings

4. **PDF Documentation** (`Resources/*.pdf`)
   - `Samples.pdf` - Code samples and examples
   - `Properties.pdf` - iProperties API reference
   - `Ext feat.pdf`, `Extrude def.pdf` - Feature API documentation
   - Useful for understanding complex API patterns

**Best Practice**: When modifying DXF export parameters or layer configurations, consult these files to understand available options and proper formatting. Do not modify these files unless updating reference documentation.

## Integration Points

- **Inventor COM API**: Primary interop assembly for sheet metal access
- **Assembly Scanning**: Recursive search for `SheetMetalComponentDefinition` parts
- **Flat Pattern Engine**: `HasFlatPattern`, `Unfold()`, `FlatPattern.Edit` workflow
- **iProperties System**: Material data, custom properties, Part Number, Stock Number, Revision extraction
- **File System Export**: Batch processing with custom naming conventions
- **SQLite Database**: Export history tracking with automatic archival
- **SHA256 Hashing**: Content-based file comparison for duplicate detection
- **Thumbnail Generation**: DXF preview rendering for UI display

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
- If adding new export options, consult `Resources/*.xml` and `Resources/*.ini` for parameter examples
- If modifying export history, update database schema in `Utils/ExportHistoryService.vb`
- If adding iProperties usage, update extraction logic in `Models/SheetMetalPart.vb`
- Update relevant documentation in `docs/` folder

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
- [ ] Documentation updated if adding new features (especially in docs/ folder)
- [ ] Export history database schema updated if modifying tracked data

### Working with Resources Folder

**Reference Material - Not Application Resources**:
- Files in `src/Resources/` are **reference documentation only**
- They are NOT embedded resources in the application binary
- They are NOT loaded or read by the application at runtime
- Purpose: Assist developers in understanding Inventor API and export parameters

**When to Consult Resources**:
1. **Adding DXF export parameters**: Check `Resources/exportdxf.ini` for parameter syntax
2. **Layer customization**: Review `Resources/FlatPattern.xml` for layer editing instructions
3. **API research**: Search `Resources/HTML/*.htm` for object/method documentation
4. **Understanding iProperties**: Consult `Resources/Properties.pdf` for property structure
5. **Export options**: Review `Resources/FlatPatternExportOpts.xml` for available options

**Do NOT**:
- Modify these files unless updating reference documentation
- Attempt to load or parse these files in application code
- Include paths to these files in user-facing features
- Commit changes to HTML documentation (it's auto-generated by Autodesk)

**Recommended Workflow**:
- Before implementing new export features, search relevant INI/XML files for examples
- Use HTML documentation for offline API reference instead of online docs
- Treat PDFs as code samples and API pattern guides
