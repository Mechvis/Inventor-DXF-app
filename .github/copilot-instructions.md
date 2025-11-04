# Copilot Instructions - Inventor Sheet Metal DXF Export Add-in

## Project Overview

This project is an Autodesk Inventor 2026 add-in that exports sheet metal flat patterns to DXF files with comprehensive UI options for layer control, feature selection, and metadata inclusion. Targeted at sheet metal fabrication workflows.

## Architecture & Key Components

- **Inventor Add-in Framework**: VB.NET/C# COM add-in using Autodesk Inventor API
- **Sheet Metal Engine**: `SheetMetalComponentDefinition` and `FlatPattern` manipulation
- **DXF Export Core**: `DataIO.WriteDataToFile()` with URL-style parameter strings
- **UI Framework**: WinForms/WPF interface for layer toggle and export options
- **Center Mark Generator**: Custom automation for hole center marks (not native points)

## Development Workflow

### Build & Run
```powershell
# Build Inventor Add-in (requires .NET Framework 4.8)
msbuild "SheetMetalDXFExporter.sln" /p:Configuration=Release

# Debug Setup in Visual Studio:
# Project → Debug → Start external program: point to Inventor.exe
# Copy .addin file to Inventor addins folder
# Use Add-In Manager in Inventor to unblock and load

# Deploy .addin manifest to scan folder
copy "SheetMetalDXFExporter.addin" "C:\ProgramData\Autodesk\Inventor 2026\Addins\"
```

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

*Document project-specific patterns as they emerge:*

- File naming conventions
- Module organization patterns
- Error handling approaches
- Configuration management

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

### DXF Export Validation
- Test with known sheet metal parts to verify layer assignment
- Use DXF viewer to validate geometry and layer structure  
- Check flat pattern exists: `oCompDef.HasFlatPattern` before export
- Verify coordinate transformation with `RebaseGeometry=True`

### Performance Optimization
- Close unused Inventor documents before batch export
- Use `AdvancedLegacyExport=False` for better performance on newer files
- Process large assemblies in smaller batches
- Consider background processing for extensive part lists

---

**Note**: This file should be updated as the codebase grows. Focus on documenting:
1. Non-obvious architectural decisions and their rationale
2. DXF format specifics that affect implementation choices  
3. Performance considerations for large drawing exports
4. Error handling patterns for malformed input data
5. Testing strategies for validating DXF output correctness