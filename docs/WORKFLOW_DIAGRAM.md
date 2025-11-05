# Export Workflow Diagram

## Before: Traditional Export (With Duplication Problem)

```
┌─────────────────────────────────────────────────────────────────┐
│ Export Request: Part 073-102-1002, Rev A, 2.0mm                │
└─────────────────────────────────────────────────────────────────┘
                              ↓
        ┌─────────────────────────────────────┐
        │ Generate filename with date         │
        │ 073-102-1002-ipt_1.0mm_A_2025-11-05│
        └─────────────────────────────────────┘
                              ↓
              ┌───────────────────────┐
              │ Check if file exists? │
              └───────────────────────┘
                     ↓            ↓
                 YES (overwrite)  NO (create)
                     ↓            ↓
        ┌─────────────────────────────────────┐
        │ Export Folder (CLUTTERED)           │
        ├─────────────────────────────────────┤
        │ 073-102-1002-ipt_1.0mm_A_2025-11-04│
        │ 073-102-1002-ipt_1.0mm_A_2025-11-05│  ← Multiple versions!
        │ 073-102-1002-ipt_1.0mm_A_2025-11-05 (1)│
        │ 073-102-3001-ipt_1.5mm_B_2025-11-04│
        │ 073-102-3001-ipt_1.5mm_B_2025-11-05│
        └─────────────────────────────────────┘
```

## After: Smart Export with Archival System

```
┌─────────────────────────────────────────────────────────────────┐
│ Export Request: Part 073-102-1002, Rev A, Stock: 2.0mm         │
└─────────────────────────────────────────────────────────────────┘
                              ↓
        ┌─────────────────────────────────────┐
        │ Extract iProperties                 │
        │ • Part Number: 073-102-1002         │
        │ • Stock Number: 2.0mm               │
        │ • Revision: A                       │
        └─────────────────────────────────────┘
                              ↓
        ┌─────────────────────────────────────┐
        │ Generate clean filename             │
        │ 073-102-1002_2.0mm_A.dxf           │
        └─────────────────────────────────────┘
                              ↓
        ┌─────────────────────────────────────┐
        │ Check Database for Previous Export  │
        │ SELECT * FROM ExportHistory         │
        │ WHERE PartName='073-102-1002'       │
        │   AND Thickness=2.0                 │
        │   AND Revision='A'                  │
        │   AND IsArchived=0                  │
        └─────────────────────────────────────┘
                              ↓
              ┌───────────────────────┐
              │ Previous export found?│
              └───────────────────────┘
                     ↓            ↓
                    YES           NO
                     ↓            ↓
    ┌────────────────────┐      Skip Archive
    │ Archive Old File   │       ↓
    ├────────────────────┤
    │ Old: 073-102-1002_ │
    │      2.0mm_A.dxf   │
    │         ↓          │
    │ New: _Archive/     │
    │ 073-102-1002_2.0mm_│
    │ A_2025-11-05_      │
    │ 143045.dxf         │
    │         ↓          │
    │ Update Database:   │
    │ IsArchived = 1     │
    └────────────────────┘
             ↓
    ┌────────────────────────────────────┐
    │ Export New DXF                     │
    │ • Write file: 073-102-1002_2.0mm_A │
    │ • Compute SHA256 hash              │
    └────────────────────────────────────┘
             ↓
    ┌────────────────────────────────────┐
    │ Record in Database                 │
    │ INSERT INTO ExportHistory          │
    │ (PartName, Material, Thickness,    │
    │  Revision, FilePath, ExportDate,   │
    │  FileHash, ExportedBy)             │
    └────────────────────────────────────┘
             ↓
    ┌─────────────────────────────────────┐
    │ Export Folder (CLEAN)               │
    ├─────────────────────────────────────┤
    │ 073-102-1002_2.0mm_A.dxf           │  ← Current version only
    │ 073-102-3001_1.5mm_B.dxf           │
    │ ExportHistory.db                    │
    │                                     │
    │ _Archive/                           │
    │ ├── 073-102-1002_2.0mm_A_2025-11-04_103020.dxf
    │ ├── 073-102-1002_2.0mm_A_2025-11-05_093015.dxf
    │ └── 073-102-3001_1.5mm_B_2025-11-03_141530.dxf
    └─────────────────────────────────────┘
```

## Database Schema Visualization

```
┌─────────────────────────────────────────────────────────────┐
│ ExportHistory Table                                          │
├────┬─────────────┬──────────┬───────────┬──────────┬────────┤
│ Id │ PartName    │ Material │ Thickness │ Revision │ ... │
├────┼─────────────┼──────────┼───────────┼──────────┼────────┤
│  1 │ 073-102-1002│ Steel    │ 2.0       │ A        │ ... │
│  2 │ 073-102-3001│ Aluminum │ 1.5       │ B        │ ... │
│  3 │ 073-102-1002│ Steel    │ 2.0       │ A        │ ... │
│    │             │          │           │          │        │
│ ... (continued) ...                                         │
├────┼─────────────┬────────────────┬────────────┬────────────┤
│ Id │ ExportDate  │ IsArchived    │ ArchivePath│ ExportedBy │
├────┼─────────────┼───────────────┼────────────┼────────────┤
│  1 │ 2025-11-04  │ 1 (Archived)  │ _Archive/..│ jsmith     │
│  2 │ 2025-11-05  │ 0 (Active)    │            │ jsmith     │
│  3 │ 2025-11-05  │ 0 (Active)    │            │ jdoe       │
└────┴─────────────┴───────────────┴────────────┴────────────┘
                    ↑
                    └─ New export (Id=3) caused Id=1 to be archived
```

## iProperty Extraction Flow

```
┌─────────────────────────────────────────────────────────────┐
│ Inventor Part Document                                       │
└─────────────────────────────────────────────────────────────┘
                              ↓
        ┌─────────────────────────────────────┐
        │ iProperties Extraction              │
        └─────────────────────────────────────┘
                              ↓
        ┌─────────────────────────────────────┐
        │ 1. Part Number                      │
        │    ↓                                │
        │    Try: Design Tracking Properties  │
        │         → "Part Number"             │
        │    ↓                                │
        │    Fallback: User Defined Properties│
        │         → "Part Number"             │
        │    ↓                                │
        │    Final: Use filename              │
        └─────────────────────────────────────┘
                              ↓
        ┌─────────────────────────────────────┐
        │ 2. Stock Number                     │
        │    ↓                                │
        │    Try: User Defined Properties     │
        │         → "Stock Number"            │
        │         → "Stock No"                │
        │         → "Stock"                   │
        │         → "Material Stock"          │
        │    ↓                                │
        │    Fallback: Format thickness       │
        │         → "2.0mm"                   │
        └─────────────────────────────────────┘
                              ↓
        ┌─────────────────────────────────────┐
        │ 3. Revision                         │
        │    ↓                                │
        │    Try: Design Tracking Properties  │
        │         → "Revision Number"         │
        │    ↓                                │
        │    Fallback: User Defined Properties│
        │         → "Rev"                     │
        │    ↓                                │
        │    Final: Default to "A"            │
        └─────────────────────────────────────┘
                              ↓
        ┌─────────────────────────────────────┐
        │ Result: Clean Filename              │
        │ {PartNumber}_{StockNumber}_{Rev}.dxf│
        │ Example: 073-102-1002_2.0mm_A.dxf  │
        └─────────────────────────────────────┘
```

## File Lifecycle

```
┌─────────────┐
│ Initial     │  Day 1: First export
│ Export      │  073-102-1002_2.0mm_A.dxf → Export Folder
└─────────────┘  Record in DB (Id=1, IsArchived=0)
                              ↓
┌─────────────┐
│ Second      │  Day 2: Re-export same part
│ Export      │  1. Find previous export in DB (Id=1)
│ (Duplicate) │  2. Move to: _Archive/073-102-1002_2.0mm_A_2025-11-05_093015.dxf
└─────────────┘  3. Update DB: Id=1, IsArchived=1
                 4. New export → 073-102-1002_2.0mm_A.dxf
                 5. Record in DB (Id=2, IsArchived=0)
                              ↓
┌─────────────┐
│ Third       │  Day 3: Re-export again
│ Export      │  1. Find previous export in DB (Id=2)
│ (Duplicate) │  2. Move to: _Archive/073-102-1002_2.0mm_A_2025-11-06_103020.dxf
└─────────────┘  3. Update DB: Id=2, IsArchived=1
                 4. New export → 073-102-1002_2.0mm_A.dxf
                 5. Record in DB (Id=3, IsArchived=0)

Final State:
┌─────────────────────────────────────┐
│ Export Folder/                      │
│ ├── 073-102-1002_2.0mm_A.dxf       │ ← Current (Day 3)
│ └── _Archive/                       │
│     ├── ..._2025-11-05_093015.dxf  │ ← Day 1
│     └── ..._2025-11-06_103020.dxf  │ ← Day 2
└─────────────────────────────────────┘
```

## Benefits Summary

```
┌─────────────────┐         ┌─────────────────┐
│ OLD SYSTEM      │         │ NEW SYSTEM      │
├─────────────────┤         ├─────────────────┤
│ ✗ Date in name  │   →→→   │ ✓ Clean names   │
│ ✗ Duplicates    │         │ ✓ Auto-archive  │
│ ✗ No history    │         │ ✓ Full tracking │
│ ✗ Manual cleanup│         │ ✓ Automated     │
│ ✗ Filename-based│         │ ✓ Part Number   │
│ ✗ No audit trail│         │ ✓ Complete logs │
└─────────────────┘         └─────────────────┘
```
