# Export Duplication Management - Implementation Summary

## Issue Resolution

This implementation addresses the requirements from issue "Export duplication":

### ✅ Requirements Completed

1. **Remove date stamp from filename** ✓
   - Date no longer included in default filename template
   - Changed from `{PartNumber}_{StockNumber}_{Rev}_{Date}` to `{PartNumber}_{StockNumber}_{Rev}`

2. **Remove file extension from filename** ✓
   - `.ipt` extension automatically stripped from part names
   - Clean filenames without double extensions

3. **Give each export a logged ID in database** ✓
   - SQLite database tracks every export with auto-incrementing ID
   - Database location: `ExportHistory.db` in export folder
   - Tracks: ID, Part Number, Material, Thickness, Revision, Path, Date, Hash, User

4. **Archive files that are being exported again** ✓
   - Automatic detection of duplicate exports based on Part Number, Thickness, and Revision
   - Existing files moved to `_Archive/` folder before new export
   - Database updated to mark old export as archived

5. **Add date and timestamp to archived files** ✓
   - Archived files automatically renamed with timestamp: `PartNumber_StockNumber_Rev_YYYY-MM-DD_HHMMSS.dxf`
   - Example: `073-102-1002_2.0mm_A_2025-11-05_143045.dxf`

6. **Review error in thumbnail display** ✓
   - Improved error handling in thumbnail loading
   - Added null checks and proper exception logging
   - Thumbnails fail gracefully without breaking the UI

7. **Check decimal placement on thickness output to filename** ✓
   - Standardized to F1 format (1 decimal place)
   - Example: "2.0mm" instead of "2.00mm" or "2mm"

### ✅ Additional Enhancements (New Requirements)

8. **Use Part Number instead of filename** ✓
   - Extracts Part Number from iProperties ("Design Tracking Properties" → "Part Number")
   - Fallback to custom properties if not found
   - Final fallback to filename if no Part Number available

9. **Use Stock Number instead of thickness** ✓
   - Extracts Stock Number from custom iProperties
   - Supports multiple property names: "Stock Number", "Stock No", "Stock", "Material Stock"
   - Fallback to formatted thickness (e.g., "2.0mm") if Stock Number not found

## File Naming Examples

### Before (Old System)
```
073-102-1002_MIR_-_RHS-ipt_1.0mm_A_2025-11-05.dxf
073-102-1002_MIR_-_RHS-ipt_1.0mm_A_2025-11-05 (1).dxf
073-102-1002_MIR_-_RHS-ipt_1.0mm_A_2025-11-04.dxf
```

### After (New System)
```
Active Export:
073-102-1002_2.0mm_A.dxf

Archives:
_Archive/073-102-1002_2.0mm_A_2025-11-05_143045.dxf
_Archive/073-102-1002_2.0mm_A_2025-11-04_103020.dxf
```

## Key Benefits

1. **Clean Active Folder**: Only current versions in main export folder
2. **Version History**: All previous versions preserved in archive with timestamps
3. **No Duplicates**: System automatically manages file versions
4. **Audit Trail**: Complete database log of all exports
5. **Meaningful Names**: Uses Part Numbers from your system, not arbitrary filenames
6. **Stock Integration**: Supports your stock numbering system via iProperties
7. **Automated**: No manual file management required

## Technical Implementation

### New Files Created

1. **Models/ExportHistoryEntry.vb**
   - Data model for export history records
   - Properties: Id, PartName (Part Number), Material, Thickness, Revision, FilePath, etc.

2. **Utils/ExportHistoryService.vb**
   - SQLite database service for export tracking
   - Methods: AddExport, FindPreviousExport, MarkAsArchived, GetExportStats
   - Includes SHA256 file hash computation

3. **docs/EXPORT_HISTORY.md**
   - Complete documentation for the export history system
   - Usage instructions, database queries, troubleshooting guide

### Modified Files

1. **Models/SheetMetalPart.vb**
   - Added PartNumber and StockNumber properties
   - Extracts Part Number from iProperties with fallbacks
   - Extracts Stock Number from custom properties with multiple name variations
   - Updated FormattedFileName to use Part Number and Stock Number

2. **Models/ExportSettings.vb**
   - Added EnableExportHistory, ArchiveDuplicates, ArchiveFolderName properties
   - Updated FileNameTemplate to `{PartNumber}_{StockNumber}_{Rev}`

3. **Export/DXFExporter.vb**
   - Integrated ExportHistoryService
   - Added ArchiveExistingFile method for duplicate handling
   - Added RecordExport method for database logging
   - Updated ReplaceFileNameTokens to support {PartNumber} and {StockNumber}

4. **UI/ExportOptionsDialog.vb**
   - Improved thumbnail error handling with better null checks

5. **SheetMetalDXFExporter.vbproj**
   - Added references to new files
   - Added System.Data.SQLite NuGet package reference

6. **.gitignore**
   - Excluded ExportHistory.db and _Archive/ folder from version control

## Database Schema

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

CREATE INDEX idx_partname ON ExportHistory(PartName);
CREATE INDEX idx_exportdate ON ExportHistory(ExportDate DESC);
```

## iProperty Setup Required

For best results, users should configure these iProperties:

### Required: Part Number
- **Location**: File → iProperties → Summary tab
- **Field**: Part Number
- **Example**: "073-102-1002"

### Recommended: Stock Number
- **Location**: File → iProperties → Custom tab
- **Property Name**: "Stock Number" (or "Stock No", "Stock", "Material Stock")
- **Example**: "2.0mm", "1/8", "16ga"

### Recommended: Revision
- **Location**: File → iProperties → Summary tab
- **Field**: Revision Number
- **Example**: "A", "B", "Rev1"

## Configuration Options

Users can customize behavior in ExportSettings:

```vb
' Enable export history tracking
EnableExportHistory = True

' Enable automatic archival
ArchiveDuplicates = True

' Archive folder name
ArchiveFolderName = "_Archive"

' Filename template
FileNameTemplate = "{PartNumber}_{StockNumber}_{Rev}"
```

## Future Enhancements

Potential improvements for future versions:

- [ ] Web-based export history viewer
- [ ] Bulk iProperty editor for setting Part Numbers
- [ ] Export statistics dashboard
- [ ] Content-based duplicate detection (skip archive if file hash identical)
- [ ] Configurable archive retention policies
- [ ] Integration with PLM/PDM systems

## Testing Recommendations

To test the implementation:

1. **Test Part Number extraction**:
   - Export part with Part Number set in iProperties
   - Verify filename uses Part Number
   - Export part without Part Number set
   - Verify filename falls back to filename

2. **Test Stock Number extraction**:
   - Export part with "Stock Number" custom property
   - Verify filename uses Stock Number
   - Export part without Stock Number
   - Verify filename falls back to thickness

3. **Test archival**:
   - Export same part twice
   - Verify first export moves to _Archive folder
   - Verify timestamp added to archived filename
   - Verify new export has clean filename

4. **Test database tracking**:
   - Export multiple parts
   - Open ExportHistory.db with SQLite browser
   - Verify all exports logged with correct data

5. **Test thumbnail display**:
   - Export multiple parts with and without thumbnails
   - Verify UI handles missing thumbnails gracefully

## Notes

- The system is backwards compatible - existing files are not renamed
- On first export after upgrade, old files will be archived with timestamp
- Database is created automatically on first use
- Archive folder is excluded from git via .gitignore
- File hashes enable future content-based deduplication
