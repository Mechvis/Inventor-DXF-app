Imports System
Imports System.Data.SQLite
Imports System.IO
Imports System.Security.Cryptography

''' <summary>
''' Service for managing export history database
''' Uses SQLite to track all exports and archival operations
''' </summary>
Public Class ExportHistoryService
    
    Private ReadOnly _connectionString As String
    Private ReadOnly _dbPath As String
    
    Public Sub New(exportPath As String)
        ' Store database in export folder
        _dbPath = Path.Combine(exportPath, "ExportHistory.db")
        _connectionString = $"Data Source={_dbPath};Version=3;"
        InitializeDatabase()
    End Sub
    
    ''' <summary>
    ''' Initialize database schema if not exists
    ''' </summary>
    Private Sub InitializeDatabase()
        Try
            ' Create directory if needed
            Dim dir = Path.GetDirectoryName(_dbPath)
            If Not String.IsNullOrEmpty(dir) Then
                Directory.CreateDirectory(dir)
            End If

            Using conn As New SQLiteConnection(_connectionString)
                conn.Open()

                Dim createTable As String = "
                    CREATE TABLE IF NOT EXISTS ExportHistory (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PartName TEXT NOT NULL,
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
                    
                    CREATE INDEX IF NOT EXISTS idx_partname ON ExportHistory(PartName);
                    CREATE INDEX IF NOT EXISTS idx_exportdate ON ExportHistory(ExportDate DESC);
                "

                Using cmd As New SQLiteCommand(createTable, conn)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Failed to initialize database: {ex.Message}")
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Get the next export ID (for logging purposes)
    ''' </summary>
    Public Function GetNextExportId() As Long
        Try
            Using conn As New SQLiteConnection(_connectionString)
                conn.Open()

                Dim query As String = "SELECT COALESCE(MAX(Id), 0) + 1 FROM ExportHistory"
                Using cmd As New SQLiteCommand(query, conn)
                    Return CLng(cmd.ExecuteScalar())
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Failed to get next export ID: {ex.Message}")
            Return 1
        End Try
    End Function

    ''' <summary>
    ''' Find previous export of the same part
    ''' </summary>
    Public Function FindPreviousExport(partName As String, thickness As Double, revision As String) As ExportHistoryEntry
        Try
            Using conn As New SQLiteConnection(_connectionString)
                conn.Open()

                Dim query As String = "
                    SELECT Id, PartName, Material, Thickness, Revision, FilePath, 
                           ExportDate, IsArchived, ArchivePath, ExportedBy, FileHash
                    FROM ExportHistory
                    WHERE PartName = @PartName 
                      AND Thickness = @Thickness 
                      AND Revision = @Revision
                      AND IsArchived = 0
                    ORDER BY ExportDate DESC
                    LIMIT 1
                "

                Using cmd As New SQLiteCommand(query, conn)
                    cmd.Parameters.AddWithValue("@PartName", partName)
                    cmd.Parameters.AddWithValue("@Thickness", thickness)
                    cmd.Parameters.AddWithValue("@Revision", revision)

                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return New ExportHistoryEntry() With {
                                .Id = reader.GetInt64(0),
                                .PartName = reader.GetString(1),
                                .Material = If(reader.IsDBNull(2), "", reader.GetString(2)),
                                .Thickness = reader.GetDouble(3),
                                .Revision = If(reader.IsDBNull(4), "", reader.GetString(4)),
                                .FilePath = reader.GetString(5),
                                .ExportDate = DateTime.Parse(reader.GetString(6)),
                                .IsArchived = reader.GetInt32(7) = 1,
                                .ArchivePath = If(reader.IsDBNull(8), "", reader.GetString(8)),
                                .ExportedBy = If(reader.IsDBNull(9), "", reader.GetString(9)),
                                .FileHash = If(reader.IsDBNull(10), "", reader.GetString(10))
                            }
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Failed to find previous export: {ex.Message}")
        End Try

        Return Nothing
    End Function

    ''' <summary>
    ''' Add new export entry to history
    ''' </summary>
    Public Function AddExport(entry As ExportHistoryEntry) As Long
        Try
            Using conn As New SQLiteConnection(_connectionString)
                conn.Open()

                Dim insert As String = "
                    INSERT INTO ExportHistory 
                    (PartName, Material, Thickness, Revision, FilePath, ExportDate, 
                     IsArchived, ArchivePath, ExportedBy, FileHash)
                    VALUES 
                    (@PartName, @Material, @Thickness, @Revision, @FilePath, @ExportDate,
                     @IsArchived, @ArchivePath, @ExportedBy, @FileHash);
                    SELECT last_insert_rowid();
                "

                Using cmd As New SQLiteCommand(insert, conn)
                    cmd.Parameters.AddWithValue("@PartName", entry.PartName)
                    cmd.Parameters.AddWithValue("@Material", If(entry.Material, ""))
                    cmd.Parameters.AddWithValue("@Thickness", entry.Thickness)
                    cmd.Parameters.AddWithValue("@Revision", If(entry.Revision, ""))
                    cmd.Parameters.AddWithValue("@FilePath", entry.FilePath)
                    cmd.Parameters.AddWithValue("@ExportDate", entry.ExportDate.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("@IsArchived", If(entry.IsArchived, 1, 0))
                    cmd.Parameters.AddWithValue("@ArchivePath", If(entry.ArchivePath, ""))
                    cmd.Parameters.AddWithValue("@ExportedBy", If(entry.ExportedBy, System.Environment.UserName))
                    cmd.Parameters.AddWithValue("@FileHash", If(entry.FileHash, ""))

                    Return CLng(cmd.ExecuteScalar())
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Failed to add export: {ex.Message}")
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Mark an export as archived
    ''' </summary>
    Public Sub MarkAsArchived(id As Long, archivePath As String)
        Try
            Using conn As New SQLiteConnection(_connectionString)
                conn.Open()

                Dim update As String = "
                    UPDATE ExportHistory 
                    SET IsArchived = 1, ArchivePath = @ArchivePath
                    WHERE Id = @Id
                "

                Using cmd As New SQLiteCommand(update, conn)
                    cmd.Parameters.AddWithValue("@Id", id)
                    cmd.Parameters.AddWithValue("@ArchivePath", archivePath)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Failed to mark as archived: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Compute SHA256 hash of a file
    ''' </summary>
    Public Shared Function ComputeFileHash(filePath As String) As String
        Try
            Using sha256 As SHA256 = SHA256.Create()
                Using stream As FileStream = File.OpenRead(filePath)
                    Dim hash = sha256.ComputeHash(stream)
                    Return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Failed to compute file hash: {ex.Message}")
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Get export statistics
    ''' </summary>
    Public Function GetExportStats() As Dictionary(Of String, Object)
        Dim stats As New Dictionary(Of String, Object)

        Try
            Using conn As New SQLiteConnection(_connectionString)
                conn.Open()

                ' Total exports
                Using cmd As New SQLiteCommand("SELECT COUNT(*) FROM ExportHistory", conn)
                    stats("TotalExports") = Convert.ToInt32(cmd.ExecuteScalar())
                End Using

                ' Archived exports
                Using cmd As New SQLiteCommand("SELECT COUNT(*) FROM ExportHistory WHERE IsArchived = 1", conn)
                    stats("ArchivedExports") = Convert.ToInt32(cmd.ExecuteScalar())
                End Using

                ' Unique parts
                Using cmd As New SQLiteCommand("SELECT COUNT(DISTINCT PartName) FROM ExportHistory", conn)
                    stats("UniqueParts") = Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Failed to get export stats: {ex.Message}")
        End Try
        
        Return stats
    End Function
    
End Class
