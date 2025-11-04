Imports Inventor
Imports System.Text

''' <summary>
''' Core DXF export engine using Inventor's DataIO with URL-style parameters
''' Handles sheet metal flat pattern export with configurable layers and features
''' </summary>
Public Class DXFExporter

    Private ReadOnly _inventorApp As Inventor.Application
    Private ReadOnly _settings As ExportSettings

    Public Sub New(inventorApp As Inventor.Application, settings As ExportSettings)
        _inventorApp = inventorApp
        _settings = settings
    End Sub

    ''' <summary>
    ''' Export multiple sheet metal parts to DXF files
    ''' </summary>
    Public Sub ExportParts(parts As List(Of SheetMetalPart))
        For Each part In parts.Where(Function(p) p.IsSelected)
            Try
                ExportSinglePart(part)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"Failed to export {part.PartName}: {ex.Message}")
                Throw New Exception($"Export failed for {part.PartName}: {ex.Message}")
            End Try
        Next
    End Sub

    ''' <summary>
    ''' Export a single sheet metal part to DXF
    ''' </summary>
    Public Sub ExportSinglePart(part As SheetMetalPart)
        Dim exportString = BuildExportString()
        Dim outputPath = GenerateOutputPath(part)

        ' Route based on read-only and flat pattern state
        If part.IsReadOnly AndAlso (Not part.HasFlatPattern OrElse part.NeedsUpdate) Then
            If Not ExportViaTempCopy(part.Document, outputPath, exportString) Then
                Throw New Exception("Temp-copy export failed")
            End If
            Return
        End If

        ' Direct export (may create/update flat if allowed)
        If Not ExportDirect(part.Document, exportString, outputPath) Then
            Throw New Exception("Direct export failed")
        End If

        ' Add metadata block if requested
        If _settings.IncludeMetadataBlock Then
            AddMetadataBlock(part, outputPath)
        End If

        ' Generate center marks if requested
        If _settings.GenerateCenterMarks AndAlso _settings.IncludeHoleCenters Then
            Dim centerMarkGen As New CenterMarkGenerator(_inventorApp, _settings)
            centerMarkGen.GenerateCenterMarks(part, outputPath)
        End If
    End Sub

    Private Function ExportDirect(doc As PartDocument, args As String, dxfOut As String) As Boolean
        Try
            Dim sm = CType(doc.ComponentDefinition, SheetMetalComponentDefinition)

            If _settings.EnsureFlatPatternBeforeExport AndAlso Not doc.ReadOnly Then
                If Not sm.HasFlatPattern Then
                    sm.Unfold() : sm.FlatPattern.ExitEdit()
                ElseIf sm.FlatPattern.RequiresUpdate OrElse doc.RequiresUpdate Then
                    doc.Update()
                End If
            End If

            sm.DataIO.WriteDataToFile(args, dxfOut)
            Return True
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Direct export failed: " & ex.Message)
            Return False
        End Try
    End Function

    Private Function ExportViaTempCopy(src As PartDocument, dxfOut As String, args As String) As Boolean
        Try
            Dim tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MVD_DXF")
            System.IO.Directory.CreateDirectory(tempDir)

            Dim srcName = If(Not String.IsNullOrEmpty(src.FullFileName), System.IO.Path.GetFileName(src.FullFileName), src.InternalName & ".ipt")
            Dim tempIpt = System.IO.Path.Combine(tempDir, srcName)

            If Not String.IsNullOrEmpty(src.FullFileName) AndAlso System.IO.File.Exists(src.FullFileName) Then
                System.IO.File.Copy(src.FullFileName, tempIpt, True)
            Else
                ' Fall back: save a copy to temp if unsaved
                src.SaveAs(tempIpt, False)
            End If

            Dim tempDoc = CType(_inventorApp.Documents.Open(tempIpt, False), PartDocument)
            Dim sm = CType(tempDoc.ComponentDefinition, SheetMetalComponentDefinition)

            If Not sm.HasFlatPattern Then
                sm.Unfold() : sm.FlatPattern.ExitEdit()
            ElseIf sm.FlatPattern.RequiresUpdate OrElse tempDoc.RequiresUpdate Then
                tempDoc.Update()
            End If

            sm.DataIO.WriteDataToFile(args, dxfOut)
            tempDoc.Close(False)
            Return True
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Temp copy export failed: " & ex.Message)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Build the URL-style export string with all layer parameters
    ''' </summary>
    Private Function BuildExportString() As String
        Dim sb As New StringBuilder()
        sb.Append("FLAT PATTERN DXF")

        ' Basic export options
        sb.Append($"?AcadVersion={_settings.AcadVersion}")
        sb.Append($"&RebaseGeometry={_settings.RebaseGeometry}")

        ' Track invisible layers and append once at the end
        Dim invisibleLayers As New List(Of String)

        ' Layer configurations
        If _settings.OuterProfileLayer.IsEnabled Then
            sb.Append($"&OuterProfileLayer={_settings.OuterProfileLayer.LayerName}")
            If _settings.OuterProfileLayer.CustomColor Then
                sb.Append($"&OuterProfileLayerColor={_settings.OuterProfileLayer.ColorString}")
            End If
        End If

        If _settings.FeatureProfilesLayer.IsEnabled Then
            sb.Append($"&FeatureProfilesLayer={_settings.FeatureProfilesLayer.LayerName}")
            If _settings.FeatureProfilesLayer.CustomColor Then
                sb.Append($"&FeatureProfilesLayerColor={_settings.FeatureProfilesLayer.ColorString}")
            End If
        End If

        If _settings.BendLinesLayer.IsEnabled AndAlso _settings.IncludeBendLines Then
            sb.Append($"&BendLayer={_settings.BendLinesLayer.LayerName}")
            If _settings.BendLinesLayer.CustomColor Then
                sb.Append($"&BendLayerColor={_settings.BendLinesLayer.ColorString}")
            End If
        End If

        If _settings.HoleCentersLayer.IsEnabled AndAlso _settings.IncludeHoleCenters Then
            sb.Append($"&ToolCenterLayer={_settings.HoleCentersLayer.LayerName}")
            sb.Append("&IV_TOOL_CENTER=True")
        End If

        If _settings.TangentLayer.IsEnabled Then
            sb.Append($"&TangentLayer={_settings.TangentLayer.LayerName}")
        Else
            ' Hide tangent lines when not enabled
            invisibleLayers.Add("IV_TANGENT")
        End If

        ' Merge profiles option
        If _settings.MergeProfilesIntoPolyline Then
            sb.Append("&MergeProfilesIntoPolyline=True")
        End If

        ' Additional layer visibility controls
        If Not _settings.IncludeBendLines Then invisibleLayers.Add("IV_BEND")
        If Not _settings.IncludeEtching Then invisibleLayers.Add("IV_ETCHING")
        If Not _settings.IncludeText Then invisibleLayers.Add("IV_TEXT")
        If Not _settings.IncludeCornerRelief Then invisibleLayers.Add("IV_CORNER_RELIEF")
        If Not _settings.IncludeFormFeatures Then invisibleLayers.Add("IV_FORM_FEATURES")
        If Not _settings.IncludePunchMarks Then invisibleLayers.Add("IV_PUNCH_MARKS")

        If invisibleLayers.Count > 0 Then
            sb.Append($"&InvisibleLayers={String.Join(";", invisibleLayers)}")
        End If

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Generate the output file path based on settings and part properties
    ''' </summary>
    Private Function GenerateOutputPath(part As SheetMetalPart) As String
        Dim fileName As String

        If _settings.CustomFileNaming Then
            fileName = ReplaceFileNameTokens(_settings.FileNameTemplate, part)
        Else
            fileName = part.FormattedFileName
        End If

        ' Ensure DXF extension
        If Not fileName.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase) Then
            fileName &= ".dxf"
        End If

        Return System.IO.Path.Combine(_settings.ExportPath, fileName)
    End Function

    ''' <summary>
    ''' Replace tokens in filename template with actual part properties
    ''' </summary>
    Private Function ReplaceFileNameTokens(template As String, part As SheetMetalPart) As String
        Dim result = template

        Dim rev As String = GetRevisionValue(part.Document)

        result = result.Replace("{PartName}", part.PartName)
        result = result.Replace("{FileName}", part.FileName)
        result = result.Replace("{Thickness}", part.Thickness.ToString("F1"))
        result = result.Replace("{Material}", part.Material)
        result = result.Replace("{Weight}", part.Weight.ToString("F2"))
        result = result.Replace("{Date}", DateTime.Now.ToString("yyyy-MM-dd"))
        result = result.Replace("{Time}", DateTime.Now.ToString("HHmm"))
        result = result.Replace("{Rev}", rev)

        ' Replace custom property tokens
        For Each kvp In part.CustomProperties
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value)
        Next

        ' Clean up invalid file name characters
        For Each invalidChar In System.IO.Path.GetInvalidFileNameChars()
            result = result.Replace(invalidChar, "_"c)
        Next

        Return result
    End Function

    Private Function GetRevisionValue(doc As PartDocument) As String
        Try
            ' Try built-in revision first
            Dim designProps As PropertySet = doc.PropertySets("Design Tracking Properties")
            For Each p As Inventor.Property In designProps
                If String.Equals(p.Name, "Revision Number", StringComparison.OrdinalIgnoreCase) AndAlso p.Value IsNot Nothing Then
                    Return p.Value.ToString()
                End If
            Next
            ' Fallback to user-defined "Rev"
            Dim userProps As PropertySet = doc.PropertySets("Inventor User Defined Properties")
            For Each p As Inventor.Property In userProps
                If String.Equals(p.Name, "Rev", StringComparison.OrdinalIgnoreCase) AndAlso p.Value IsNot Nothing Then
                    Return p.Value.ToString()
                End If
            Next
        Catch
        End Try
        Return "A"
    End Function

    ''' <summary>
    ''' Add metadata information block to the DXF file (post-processing)
    ''' </summary>
    Private Sub AddMetadataBlock(part As SheetMetalPart, dxfFilePath As String)
        ' This would require DXF post-processing or using a DXF library
        ' For now, we'll create a separate text file with the metadata
        Try
            Dim metadataGen As New MetadataBlockGenerator(_settings)
            metadataGen.AddMetadataToFile(part, dxfFilePath)
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Could not add metadata block: {ex.Message}")
        End Try
    End Sub
End Class