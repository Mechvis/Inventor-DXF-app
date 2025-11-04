Imports Inventor
Imports System.Collections.Generic

''' <summary>
''' Scans the active Inventor doc for sheet-metal parts that can be exported to DXF.
''' Honors suppression and active model states by using AllLeafOccurrences.
''' Avoids opening documents; de-duplicates by FullFileName/InternalName.
''' </summary>
Public Class SheetMetalScanner
    
    Private ReadOnly _inventorApp As Inventor.Application
    
    Public Sub New(inventorApp As Inventor.Application)
        _inventorApp = inventorApp
    End Sub
    
    ''' <summary>
    ''' Find all sheet metal parts in the current document
    ''' </summary>
    Public Function FindSheetMetalParts() As List(Of SheetMetalPart)
        Dim parts As New List(Of SheetMetalPart)
        Dim doc = _inventorApp.ActiveDocument
        If doc Is Nothing Then Return parts
        
        Select Case doc.DocumentType
            Case DocumentTypeEnum.kPartDocumentObject
                ' Single part document
                ScanPartDocument(CType(doc, PartDocument), parts)
            
            Case DocumentTypeEnum.kAssemblyDocumentObject
                ' Assembly document - use AllLeafOccurrences to respect suppression/model states
                ScanAssemblyDocument(CType(doc, AssemblyDocument), parts)
            
            Case Else
                ' Drawing or other document types not supported
        End Select
        
        Return parts
    End Function
    
    ''' <summary>
    ''' Scan a single part document for sheet metal definition
    ''' </summary>
    Private Sub ScanPartDocument(partDoc As PartDocument, parts As List(Of SheetMetalPart))
        Try
            If TypeOf partDoc.ComponentDefinition Is SheetMetalComponentDefinition Then
                parts.Add(New SheetMetalPart(partDoc))
            End If
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Error scanning part: {ex.Message}")
        End Try
    End Sub
    
    ''' <summary>
    ''' Scan an assembly using AllLeafOccurrences (no recursion) and avoid opening docs
    ''' </summary>
    Private Sub ScanAssemblyDocument(asm As AssemblyDocument, parts As List(Of SheetMetalPart))
        Try
            Dim hits As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            
            For Each occ As ComponentOccurrence In asm.ComponentDefinition.Occurrences.AllLeafOccurrences
                Try
                    If occ.Suppressed Then Continue For
                    
                    ' Optional: skip virtual components
                    If TypeOf occ.Definition Is VirtualComponentDefinition Then Continue For
                    
                    ' Test definition type for sheet metal
                    Dim smDef = TryCast(occ.Definition, SheetMetalComponentDefinition)
                    If smDef Is Nothing Then Continue For
                    
                    ' Resolve PartDocument from definition; do not open files
                    Dim partDoc = TryCast(smDef.Document, PartDocument)
                    If partDoc Is Nothing Then Continue For
                    
                    ' Build unique key using FullFileName or InternalName for unsaved docs
                    Dim key As String = If(Not String.IsNullOrEmpty(partDoc.FullFileName), partDoc.FullFileName, partDoc.InternalName)
                    
                    If hits.Add(key) Then
                        parts.Add(New SheetMetalPart(partDoc))
                    End If
                Catch exOcc As Exception
                    System.Diagnostics.Debug.WriteLine($"Occurrence scan error [{occ.Name}]: {exOcc.Message}")
                End Try
            Next
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Assembly scan error: {ex.Message}")
        End Try
    End Sub
    
    ''' <summary>
    ''' Get summary information about found sheet metal parts
    ''' </summary>
    Public Function GetScanSummary(parts As List(Of SheetMetalPart)) As String
        If parts.Count = 0 Then
            Return "No sheet metal parts found in the current document."
        End If
        
        Dim summary As New System.Text.StringBuilder()
        summary.AppendLine($"Found {parts.Count} sheet metal part(s):")
        
        For Each part In parts
            summary.AppendLine($"  • {part.PartName} ({part.MaterialInfo})")
            If Not part.HasFlatPattern Then
                summary.AppendLine("    ? No flat pattern — will be created during export")
            End If
        Next
        
        Return summary.ToString()
    End Function
End Class