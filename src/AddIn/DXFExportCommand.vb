Imports Inventor

''' <summary>
''' Handles the main DXF export command execution
''' Manages the export workflow and UI interaction
''' </summary>
Public Class DXFExportCommand
    Implements IDisposable
    
    Private ReadOnly _inventorApp As Inventor.Application
    Private _exportDialog As ExportOptionsDialog
    
    Public Sub New(inventorApp As Inventor.Application)
        _inventorApp = inventorApp
    End Sub
    
    Public Sub Execute()
        Try
            ' Check if we have a valid document
            If _inventorApp.ActiveDocument Is Nothing Then
                System.Windows.Forms.MessageBox.Show("Please open an Inventor document first.", "No Document", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information)
                Return
            End If

            ' Scan for sheet metal parts (includes sub-assemblies)
            Dim scanner As New SheetMetalScanner(_inventorApp)
            Dim sheetMetalParts = scanner.FindSheetMetalParts()

            If sheetMetalParts.Count = 0 Then
                System.Windows.Forms.MessageBox.Show("No sheet metal parts found in the current document.", "No Sheet Metal Parts",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information)
                Return
            End If

            ' Show export options dialog (with per-item DXF preview)
            _exportDialog = New ExportOptionsDialog(_inventorApp, sheetMetalParts)

            If _exportDialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                ' Execute the export with selected options
                Dim exporter As New DXFExporter(_inventorApp, _exportDialog.ExportSettings)
                Dim result = exporter.ExportParts(sheetMetalParts)

                ' Build result message
                Dim selectedCount = sheetMetalParts.FindAll(Function(x) x.IsSelected).Count
                Dim msg As String
                If result IsNot Nothing Then
                    If result.Failed = 0 Then
                        msg = $"Exported {result.Succeeded} of {selectedCount} selected part(s)."
                    Else
                        Dim maxErr = Math.Min(5, result.Failures.Count)
                        Dim sb As New System.Text.StringBuilder()
                        For i As Integer = 0 To maxErr - 1
                            Dim t = result.Failures(i)
                            sb.AppendLine("  • " & t.Item1 & ": " & t.Item2)
                        Next
                        Dim details = sb.ToString()
                        msg = "Export completed with errors." & vbCrLf &
                              $"Succeeded: {result.Succeeded}  Failed: {result.Failed}  (of {selectedCount} selected)" & vbCrLf & vbCrLf &
                              "First errors:" & vbCrLf & details & vbCrLf &
                              "See DXF_Export.log in the export folder for full details."
                    End If
                Else
                    msg = "Export finished."
                End If

                System.Windows.Forms.MessageBox.Show(msg, "DXF Export", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show($"Export failed: {ex.Message}", "Export Error",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error)
        Finally
            If _exportDialog IsNot Nothing Then
                _exportDialog.Dispose()
                _exportDialog = Nothing
            End If
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If _exportDialog IsNot Nothing Then
            _exportDialog.Dispose()
        End If
    End Sub
End Class
