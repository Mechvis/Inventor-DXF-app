# Export History and Archival System

## Overview

The Sheet Metal DXF Exporter now includes an automatic export history tracking and archival system. This system:

1. **Tracks all exports** in a SQLite database with unique IDs
2. **Automatically archives duplicate exports** when re-exporting the same part
3. **Uses Part Number and Stock Number** for clean, meaningful filenames
4. **Removes date stamps from filenames** for cleaner file organization
5. **Adds timestamps to archived files** for version tracking

## How It Works

### Filename Generation

Exported files are now named using Part Number and Stock Number from iProperties:

**Old Format (with filename and date):**
```
073-102-1002_MIR_-_RHS-ipt_1.0mm_A_2025-11-05.dxf
```

**New Format (with Part Number and Stock Number):**
```
073-102-1002_2.0mm_A.dxf
```

**Key Changes:**
- **Part Number** extracted from iProperties replaces filename
- **Stock Number** from iProperties replaces thickness (fallback to thickness if not found)
- Date stamp removed from filename
- File extension (.ipt) removed from part name
- Clean, predictable filenames for easier file management

### iProperties Used

The system extracts the following iProperties:

1. **Part Number** (required)
   - Source: "Design Tracking Properties" → "Part Number"
   - Fallback: "Inventor User Defined Properties" → "Part Number"
   - Final fallback: Filename without extension

2. **Stock Number** (optional)
   - Source: "Inventor User Defined Properties" → "Stock Number"
   - Alternate names: "Stock No", "Stock", "StockNo", "Material Stock"
   - Fallback: Thickness formatted as "X.Xmm" (e.g., "2.0mm")

3. **Revision**
   - Source: "Design Tracking Properties" → "Revision Number"
   - Fallback: "Inventor User Defined Properties" → "Rev"
   - Final fallback: "A"

### Export History Database

All exports are tracked in `ExportHistory.db` located in your export folder:

**Database Fields:**
- **Id**: Unique auto-incrementing export ID
- **PartName**: Part Number from iProperties
- **Material**: Material specification
- **Thickness**: Part thickness in mm
- **Revision**: Part revision (from iProperties)
- **FilePath**: Full path to exported file
- **ExportDate**: Date and time of export
- **IsArchived**: Whether the file has been archived
- **ArchivePath**: Path to archived file (if archived)
- **ExportedBy**: Windows username who performed export
- **FileHash**: SHA256 hash of exported file content

### Automatic Archival

When you export a part that has been exported before:

1. System checks database for previous export with same:
   - Part Number
   - Thickness
   - Revision

2. If found, the existing file is moved to `_Archive/` folder with timestamp:
   ```
   _Archive/073-102-1002_2.0mm_A_2025-11-05_143045.dxf
   ```

3. Database is updated to mark the old export as archived

4. New export proceeds with clean filename

### Archive Folder Structure

```
Export Folder/
├── ExportHistory.db             # SQLite database
├── 073-102-1002_2.0mm_A.dxf    # Current exports (Part Number + Stock Number)
├── 073-102-3001_1.5mm_B.dxf
└── _Archive/                    # Archived previous versions
    ├── 073-102-1002_2.0mm_A_2025-11-04_103020.dxf
    ├── 073-102-1002_2.0mm_A_2025-11-05_093015.dxf
    └── 073-102-3001_1.5mm_B_2025-11-03_141530.dxf
```

## Configuration

### Export Settings

New settings in `ExportSettings`:

```vb
' Enable/disable export history tracking
Public Property EnableExportHistory As Boolean = True

' Enable/disable automatic archival of duplicates
Public Property ArchiveDuplicates As Boolean = True

' Archive folder name (default: "_Archive")
Public Property ArchiveFolderName As String = "_Archive"

' Filename template using Part Number and Stock Number
Public Property FileNameTemplate As String = "{PartNumber}_{StockNumber}_{Rev}"
```

### Customizing Filename Template

Available tokens:
- `{PartNumber}` - Part Number from iProperties (recommended)
- `{StockNumber}` - Stock Number from iProperties (recommended)
- `{PartName}` - Part name from DisplayName (legacy)
- `{FileName}` - Filename without .ipt extension (legacy)
- `{Thickness}` - Thickness with 1 decimal place (e.g., "1.0")
- `{Material}` - Material specification
- `{Weight}` - Part weight
- `{Rev}` - Revision from iProperties
- `{CustomPropertyName}` - Any custom iProperty value

**Note:** Date and time tokens are no longer recommended for main filename but are automatically added to archived files.

## Setting Up iProperties

To get the best filenames, ensure your Inventor parts have proper iProperties set:

### Required iProperty: Part Number

1. In Inventor, go to **File → iProperties**
2. Navigate to **Summary** tab
3. Set **Part Number** field (e.g., "073-102-1002")

### Optional iProperty: Stock Number

1. In Inventor, go to **File → iProperties**
2. Navigate to **Custom** tab
3. Add property named **Stock Number**
4. Enter value (e.g., "2.0mm", "1/8", "16ga")

### Example iProperty Setup

```
Part Number: 073-102-1002
Stock Number: 2.0mm
Revision: A
Material: Steel, Mild
```

**Resulting Filename:**
```
073-102-1002_2.0mm_A.dxf
```

## Database Queries

The export history database can be queried using any SQLite tool:

### Get All Exports
```sql
SELECT * FROM ExportHistory 
ORDER BY ExportDate DESC;
```

### Get Export Statistics
```sql
SELECT 
    COUNT(*) as TotalExports,
    COUNT(DISTINCT PartName) as UniqueParts,
    SUM(CASE WHEN IsArchived = 1 THEN 1 ELSE 0 END) as ArchivedCount
FROM ExportHistory;
```

### Find All Versions of a Part (by Part Number)
```sql
SELECT Id, FilePath, ArchivePath, ExportDate, IsArchived
FROM ExportHistory
WHERE PartName = '073-102-1002'
ORDER BY ExportDate DESC;
```

### Get Recent Exports (Last 24 hours)
```sql
SELECT * FROM ExportHistory
WHERE ExportDate >= datetime('now', '-1 day')
ORDER BY ExportDate DESC;
```

## Benefits

1. **Meaningful Filenames**: Part numbers from your system, not arbitrary filenames
2. **Stock Number Integration**: Use your standard stock designations
3. **Cleaner File Organization**: No date clutter in active export folder
4. **Complete History**: Every export tracked with unique ID
5. **Version Control**: All previous versions preserved in archive
6. **Easy Retrieval**: Find any previous version by timestamp
7. **Audit Trail**: Track who exported what and when
8. **No Data Loss**: Old files automatically preserved before overwrite
9. **Consistent Naming**: Predictable filenames for automation
10. **iProperty Driven**: Leverages existing Inventor metadata

## Troubleshooting

### Missing Part Number

If Part Number is not set in iProperties:
- **Symptom**: Filename uses original file name instead of part number
- **Solution**: Set Part Number in File → iProperties → Summary tab

### Missing Stock Number

If Stock Number is not set in iProperties:
- **Symptom**: Filename uses thickness (e.g., "1.0mm") instead of stock number
- **Solution**: 
  1. Add custom property named "Stock Number" in File → iProperties → Custom tab
  2. Or use one of the alternate names: "Stock No", "Stock", "Material Stock"

### Database Issues

If the database becomes corrupted or needs to be reset:

1. Close Inventor
2. Navigate to export folder
3. Delete `ExportHistory.db` and `ExportHistory.db-journal`
4. Restart Inventor and use add-in
5. Database will be recreated automatically

### Archive Space

Monitor archive folder size periodically:

```powershell
# PowerShell command to check archive size
Get-ChildItem "_Archive" -Recurse | 
    Measure-Object -Property Length -Sum | 
    Select-Object @{Name="Size(MB)";Expression={[math]::Round($_.Sum/1MB,2)}}
```

Consider periodic cleanup of very old archived files:

```powershell
# Delete archives older than 90 days
Get-ChildItem "_Archive" -Recurse | 
    Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-90)} | 
    Remove-Item
```

## Migration from Old System

If you have existing exports with old naming:

1. Old files are not automatically renamed
2. On next export of same part, new Part Number-based name is used
3. Old file will be archived with additional timestamp
4. Gradually, all files will transition to Part Number naming scheme

## Technical Details

### File Hash Calculation

SHA256 hash is computed for each export to:
- Detect if content actually changed between exports
- Provide integrity verification
- Enable future optimizations (skip archive if content identical)

### Database Schema

```sql
CREATE TABLE ExportHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PartName TEXT NOT NULL,        -- Now stores Part Number
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

## Future Enhancements

Potential future improvements:

- [ ] Web-based export history viewer
- [ ] Export to CSV for analysis
- [ ] Duplicate detection based on file hash (skip archive if identical)
- [ ] Configurable archive retention policies
- [ ] Export statistics dashboard
- [ ] Integration with PLM/PDM systems
- [ ] Bulk iProperty editor for setting Part Numbers and Stock Numbers

