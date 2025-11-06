Imports System

''' <summary>
''' Represents a single export history entry in the database
''' </summary>
Public Class ExportHistoryEntry
    
    ''' <summary>
    ''' Unique identifier for this export (auto-increment)
    ''' </summary>
    Public Property Id As Long
    
    ''' <summary>
    ''' Part name without file extension
    ''' </summary>
    Public Property PartName As String
    
    ''' <summary>
    ''' Material specification
    ''' </summary>
    Public Property Material As String
    
    ''' <summary>
    ''' Thickness in mm
    ''' </summary>
    Public Property Thickness As Double
    
    ''' <summary>
    ''' Revision identifier
    ''' </summary>
    Public Property Revision As String

    ''' <summary>
    ''' Optional free-text describing the change for this revision
    ''' </summary>
    Public Property RevisionComment As String

    ''' <summary>
    ''' Exported file path (without archive prefix)
    ''' </summary>
    Public Property FilePath As String

    ''' <summary>
    ''' Export timestamp
    ''' </summary>
    Public Property ExportDate As DateTime

    ''' <summary>
    ''' Whether this file has been archived
    ''' </summary>
    Public Property IsArchived As Boolean

    ''' <summary>
    ''' Archive path if archived
    ''' </summary>
    Public Property ArchivePath As String

    ''' <summary>
    ''' User who performed the export
    ''' </summary>
    Public Property ExportedBy As String

    ''' <summary>
    ''' Hash of file content for detecting changes
    ''' </summary>
    Public Property FileHash As String

    Public Sub New()
        ExportDate = DateTime.Now
        IsArchived = False
        ExportedBy = System.Environment.UserName
        RevisionComment = String.Empty
    End Sub

End Class
