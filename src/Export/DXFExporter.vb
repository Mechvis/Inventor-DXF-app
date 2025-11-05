Imports Inventor
Imports System.Text
Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Globalization
Imports System.Linq

Partial Public Class DXFExporter

    Private ReadOnly _inventorApp As Inventor.Application
    Private ReadOnly _settings As ExportSettings

    Public Sub New(inventorApp As Inventor.Application, settings As ExportSettings)
        _inventorApp = inventorApp
        _settings = settings
    End Sub

    ''' <summary>
    ''' Export result summary with per-part failures
    ''' </summary>
    Public Class ExportResult
        Public Property Total As Integer
        Public Property Succeeded As Integer
        Public Property Failed As Integer
        Public Property Failures As New List(Of Tuple(Of String, String)) ' (PartName, Error)
    End Class

    ''' <summary>
    ''' Export multiple sheet metal parts to DXF files
    ''' </summary>
    Public Function ExportParts(parts As List(Of SheetMetalPart)) As ExportResult
        Dim result As New ExportResult() With {.Total = parts.Count}

        For Each part In parts.Where(Function(p) p.IsSelected)
            Try
                ExportSinglePart(part)
                result.Succeeded += 1
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Failed to export " & part.PartName & ": " & ex.Message)
                result.Failures.Add(New Tuple(Of String, String)(part.PartName, ex.Message))
                result.Failed += 1
                ' Continue with next part
            End Try
        Next

        Return result
    End Function

    ''' <summary>
    ''' Export a single sheet metal part to DXF
    ''' </summary>
    Public Sub ExportSinglePart(part As SheetMetalPart)
        ' Ensure we have a valid and writable export directory
        If String.IsNullOrWhiteSpace(_settings.ExportPath) Then
            Dim fallback As String = Global.System.IO.Path.Combine(Global.System.Environment.GetFolderPath(Global.System.Environment.SpecialFolder.MyDocuments), "DXF Exports")
            _settings.ExportPath = fallback
        End If

        Dim exportString = BuildExportString()
        Dim outputPath = GenerateOutputPath(part)

        ' Make sure target directory exists
        Try
            Dim dir = Global.System.IO.Path.GetDirectoryName(outputPath)
            If Not String.IsNullOrEmpty(dir) Then
                Global.System.IO.Directory.CreateDirectory(dir)
            End If
        Catch ex As Exception
            Throw New Exception("Cannot create export folder for '" & outputPath & "': " & ex.Message)
        End Try

        ' Route based on read-only and flat pattern state
        If part.IsReadOnly AndAlso (Not part.HasFlatPattern OrElse part.NeedsUpdate) Then
            If Not ExportViaTempCopy(part.Document, outputPath, exportString) Then
                Throw New Exception("Temp-copy export failed")
            End If
            ' Post steps
            PostProcess(part, outputPath)
            Return
        End If

        ' Direct export (may create/update flat if allowed)
        If Not ExportDirect(part.Document, exportString, outputPath) Then
            ' Fallback: try via temp copy even if not read-only (handles odd cases)
            Log("Direct export failed. Retrying via temporary copy...")
            If Not ExportViaTempCopy(part.Document, outputPath, exportString) Then
                Throw New Exception("Direct export failed and temp-copy fallback also failed")
            End If
        End If

        ' Post steps
        PostProcess(part, outputPath)
    End Sub

    Private Sub PostProcess(part As SheetMetalPart, outputPath As String)
        ' Skip metadata and center mark generation in CAM mode
        If Not _settings.CamOptimizedOutput AndAlso _settings.IncludeMetadataBlock Then
            AddMetadataBlock(part, outputPath)
        End If

        If Not _settings.CamOptimizedOutput AndAlso _settings.GenerateCenterMarks AndAlso _settings.IncludeHoleCenters Then
            Dim centerMarkGen As New CenterMarkGenerator(_inventorApp, _settings)
            centerMarkGen.GenerateCenterMarks(part, outputPath)
        End If

        ' Always run final DXF sanitizer to enforce layer mapping and drop unwanted entities
        DXFSanitizer.Sanitize(outputPath, _settings)
    End Sub

    Private Function ExportDirect(doc As PartDocument, args As String, dxfOut As String) As Boolean
        Try
            Dim sm = CType(doc.ComponentDefinition, SheetMetalComponentDefinition)

            ' Be defensive about API differences: avoid ReadOnly/RequiresUpdate properties
            If _settings.EnsureFlatPatternBeforeExport Then
                Try
                    If Not sm.HasFlatPattern Then
                        sm.Unfold()
                        sm.FlatPattern.ExitEdit()
                    Else
                        ' Touch the flat pattern to ensure it regenerates without relying on RequiresUpdate
                        sm.FlatPattern.Edit()
                        sm.FlatPattern.ExitEdit()
                    End If
                Catch exEns As Exception
                    System.Diagnostics.Debug.WriteLine("Ensure flat pattern step failed (ignored): " & exEns.Message)
                End Try
            End If

            System.Diagnostics.Debug.WriteLine("Exporting with args: " & args)
            System.Diagnostics.Debug.WriteLine("Output file: " & dxfOut)
            Log("Exporting: " & doc.DisplayName & " -> " & dxfOut)
            Log("Args: " & args)

            sm.DataIO.WriteDataToFile(args, dxfOut)
            Return True
        Catch comEx As COMException
            Log("Direct export COM error 0x" & Hex(comEx.ErrorCode) & ": " & comEx.Message)
            System.Diagnostics.Debug.WriteLine("Direct export COM failed: " & comEx.Message)
            Return False
        Catch ex As Exception
            Log("Direct export failed: " & ex.Message)
            System.Diagnostics.Debug.WriteLine("Direct export failed: " & ex.Message)
            System.Diagnostics.Debug.WriteLine("Stack trace: " & ex.StackTrace)
            Return False
        End Try
    End Function

    Private Function ExportViaTempCopy(src As PartDocument, dxfOut As String, args As String) As Boolean
        Try
            Dim tempDir = Global.System.IO.Path.Combine(Global.System.IO.Path.GetTempPath(), "MVD_DXF")
            Global.System.IO.Directory.CreateDirectory(tempDir)

            Dim srcName = If(Not String.IsNullOrEmpty(src.FullFileName), Global.System.IO.Path.GetFileName(src.FullFileName), src.InternalName & ".ipt")
            Dim tempIpt = Global.System.IO.Path.Combine(tempDir, srcName)

            If Not String.IsNullOrEmpty(src.FullFileName) AndAlso Global.System.IO.File.Exists(src.FullFileName) Then
                Global.System.IO.File.Copy(src.FullFileName, tempIpt, True)
            Else
                ' Fall back: save a copy to temp if unsaved
                src.SaveAs(tempIpt, False)
            End If

            Dim tempDoc = CType(_inventorApp.Documents.Open(tempIpt, False), PartDocument)
            Dim sm = CType(tempDoc.ComponentDefinition, SheetMetalComponentDefinition)

            Try
                If Not sm.HasFlatPattern Then
                    sm.Unfold()
                    sm.FlatPattern.ExitEdit()
                Else
                    sm.FlatPattern.Edit()
                    sm.FlatPattern.ExitEdit()
                End If
            Catch exEns As Exception
                System.Diagnostics.Debug.WriteLine("Ensure flat pattern (temp) failed (ignored): " & exEns.Message)
            End Try

            Log("Exporting (temp copy): " & tempDoc.DisplayName & " -> " & dxfOut)
            Log("Args: " & args)
            sm.DataIO.WriteDataToFile(args, dxfOut)
            tempDoc.Close(False)
            Return True
        Catch comEx As COMException
            Log("Temp copy export COM error 0x" & Hex(comEx.ErrorCode) & ": " & comEx.Message)
            System.Diagnostics.Debug.WriteLine("Temp copy export COM failed: " & comEx.Message)
            Return False
        Catch ex As Exception
            Log("Temp copy export failed: " & ex.Message)
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

        ' Force R12 + geometry-only when CAM mode is on
        Dim version As String = If(_settings.CamOptimizedOutput, "R12", NormalizeAcadVersion(_settings.AcadVersion))
        sb.Append("?AcadVersion=" & version)
        sb.Append("&RebaseGeometry=" & _settings.RebaseGeometry)
        sb.Append("&SimplifySplines=True")
        sb.Append("&TrimCenterlinesAtContour=True")
        ' In CAM mode prefer modern generator, legacy off
        If _settings.CamOptimizedOutput Then
            sb.Append("&AdvancedLegacyExport=False")
        Else
            sb.Append("&AdvancedLegacyExport=True")
        End If

        Dim invisibleLayers As New List(Of String)

        ' Map to CAM contract layers
        If _settings.OuterProfileLayer IsNot Nothing AndAlso _settings.OuterProfileLayer.IsEnabled Then
            sb.Append("&OuterProfileLayer=" & _settings.OuterProfileLayer.LayerName)
            sb.Append("&OuterProfileLayerColor=" & _settings.OuterProfileLayer.ColorString)
        End If

        ' Always emit interior profile layer; map to inner cut layer name (no null-conditional to keep VB compatibility)
        Dim innerName As String = "11_CUT_INNER"
        If _settings.FeatureProfilesLayer IsNot Nothing AndAlso Not String.IsNullOrEmpty(_settings.FeatureProfilesLayer.LayerName) Then
            innerName = _settings.FeatureProfilesLayer.LayerName
        End If
        sb.Append("&InteriorProfilesLayer=" & innerName)
        sb.Append("&InteriorProfilesLayerColor=255;255;0")

        If _settings.BendLinesLayer IsNot Nothing AndAlso _settings.BendLinesLayer.IsEnabled AndAlso _settings.IncludeBendLines Then
            sb.Append("&BendLayer=" & _settings.BendLinesLayer.LayerName)
            sb.Append("&BendLayerColor=" & _settings.BendLinesLayer.ColorString)
        Else
            invisibleLayers.Add("IV_BEND")
        End If

        If _settings.TangentLayer IsNot Nothing AndAlso _settings.TangentLayer.IsEnabled Then
            sb.Append("&TangentLayer=" & _settings.TangentLayer.LayerName)
            sb.Append("&TangentLayerColor=" & _settings.TangentLayer.ColorString)
        Else
            invisibleLayers.Add("IV_TANGENT")
        End If

        ' Hole centers: in strict CAM mode, hide POINTs to avoid clutter
        If _settings.CamOptimizedOutput Then
            invisibleLayers.Add("IV_TOOL_CENTER")
        ElseIf _settings.HoleCentersLayer IsNot Nothing AndAlso _settings.HoleCentersLayer.IsEnabled AndAlso _settings.IncludeHoleCenters Then
            sb.Append("&ToolCenterLayer=" & _settings.HoleCentersLayer.LayerName)
            sb.Append("&ToolCenterLayerColor=" & _settings.HoleCentersLayer.ColorString)
        Else
            invisibleLayers.Add("IV_TOOL_CENTER")
        End If

        ' Merge profiles option
        If _settings.MergeProfilesIntoPolyline Then
            sb.Append("&MergeProfilesIntoPolyline=True")
        End If

        ' Hide annotations and arc centers in CAM mode
        If _settings.CamOptimizedOutput Then
            invisibleLayers.Add("IV_ARC_CENTERS")
            invisibleLayers.Add("IV_TEXT")
            invisibleLayers.Add("IV_ETCHING")
        Else
            If Not _settings.IncludeEtching Then invisibleLayers.Add("IV_ETCHING")
            If Not _settings.IncludeText Then invisibleLayers.Add("IV_TEXT")
        End If

        If Not _settings.IncludeCornerRelief Then invisibleLayers.Add("IV_CORNER_RELIEF")
        If Not _settings.IncludeFormFeatures Then invisibleLayers.Add("IV_FORM_FEATURES")
        If Not _settings.IncludePunchMarks Then invisibleLayers.Add("IV_PUNCH_MARKS")

        invisibleLayers.Add("IV_UNCONSUMED_SKETCHES")
        invisibleLayers.Add("IV_UNCONSUMED_SKETCH_CONSTRUCTION")

        If invisibleLayers.Count > 0 Then
            sb.Append("&InvisibleLayers=" & String.Join(";", invisibleLayers.ToArray()))
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

        Return Global.System.IO.Path.Combine(_settings.ExportPath, fileName)
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
            result = result.Replace("{" & kvp.Key & "}", kvp.Value)
        Next

        ' Clean up invalid file name characters
        For Each invalidChar In Global.System.IO.Path.GetInvalidFileNameChars()
            result = result.Replace(invalidChar, "_"c)
        Next

        Return result
    End Function

    Private Function GetRevisionValue(doc As PartDocument) As String
        Try
            ' Try built-in revision first
            Dim designProps As PropertySet = doc.PropertySets("Design Tracking Properties")
            For Each p As Inventor.Property In designProps
                If String.Equals(p.Name, "Revision Number", StringComparison.OrdinalIgnoreCase) AndAlso Not IsNothing(p.Value) Then
                    Return p.Value.ToString()
                End If
            Next
            ' Fallback to user-defined "Rev"
            Dim userProps As PropertySet = doc.PropertySets("Inventor User Defined Properties")
            For Each p As Inventor.Property In userProps
                If String.Equals(p.Name, "Rev", StringComparison.OrdinalIgnoreCase) AndAlso Not IsNothing(p.Value) Then
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
            System.Diagnostics.Debug.WriteLine("Could not add metadata block: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Simple file logger in the export folder to help diagnose failures
    ''' </summary>
    Private Sub Log(message As String)
        Try
            Dim folder As String = If(Not String.IsNullOrWhiteSpace(_settings.ExportPath), _settings.ExportPath, Global.System.IO.Path.Combine(Global.System.Environment.GetFolderPath(Global.System.Environment.SpecialFolder.MyDocuments), "DXF Exports"))
            Dim logPath = Global.System.IO.Path.Combine(folder, "DXF_Export.log")
            Global.System.IO.Directory.CreateDirectory(folder)
            Global.System.IO.File.AppendAllText(logPath, "[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "] " & message & vbCrLf)
        Catch
            ' swallow logging errors
        End Try
    End Sub

    ''' <summary>
    ''' Normalize/validate the AcadVersion to values supported by Inventor's DXF exporter.
    ''' Unknown values are coerced to a safe default.
    ''' </summary>
    Private Function NormalizeAcadVersion(raw As String) As String
        If String.IsNullOrWhiteSpace(raw) Then Return "2004"
        Dim v = raw.Trim().ToUpperInvariant()
        Select Case v
            Case "2018", "2013", "2010", "2007", "2004", "2000", "R12"
                Return v
            Case "R14", "R13", "LT98", "LT97"
                ' Map older/unsupported tokens to R12
                Return "R12"
            Case Else
                ' Fallback to a widely supported version
                Return "2004"
        End Select
    End Function

    ' Shared helpers to be used from preview pane
    Public Shared Function ExportToTempDxfShared(app As Inventor.Application, doc As Document, settings As ExportSettings, layerXmlPath As String, optsXmlPath As String, ByRef tempPath As String) As Boolean
        Try
            Dim baseIni = "C:\Users\Public\Documents\Autodesk\Inventor 2026\Design Data\DWG-DXF\MechvisionDXF.ini"
            Dim runIni = ExportSettings.BuildRunIni(baseIni, settings.DxfVersionLabel, layerXmlPath)

            Dim addIn As TranslatorAddIn = TryCast(app.ApplicationAddIns.ItemById("{4B8D88B8-0B24-11D3-8E83-0060B0CE6BB4}"), TranslatorAddIn) ' DXF AddIn
            If addIn Is Nothing OrElse Not addIn.Activated Then addIn.Activate()

            Dim ctx = app.TransientObjects.CreateNameValueMap()
            Dim opts = app.TransientObjects.CreateNameValueMap()
            Dim dm As DataMedium = app.TransientObjects.CreateDataMedium()

            tempPath = Global.System.IO.Path.Combine(Global.System.IO.Path.GetTempPath(), "DXF_PREVIEW_" & Guid.NewGuid().ToString("N") & ".dxf")
            dm.FileName = tempPath

            opts.Value("Export_Acad_IniFile") = runIni
            opts.Value("Export_Layers_Xml") = layerXmlPath
            opts.Value("Export_SheetMetal_OptionsFile") = optsXmlPath

            ' SaveCopyAs is a Sub in Inventor API; call it and return True if no exception
            addIn.SaveCopyAs(doc, ctx, opts, dm)
            Return True
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("ExportToTempDxfShared failed: " & ex.Message)
            Return False
        End Try
    End Function

    Public Shared Function ParseEntitiesShared(dxfPath As String) As List(Of DxfEnt)
        Dim ents As New List(Of DxfEnt)()
        Try
            Dim lines = Global.System.IO.File.ReadAllLines(dxfPath, Encoding.GetEncoding("latin1"))
            If lines.Length Mod 2 <> 0 Then Return ents
            Dim i As Integer = 0
            Dim inEntities As Boolean = False
            Dim id As Integer = 0

            While i < lines.Length - 1
                Dim c = lines(i).Trim()
                Dim v = lines(i + 1).Trim()
                If c = "0" AndAlso v = "SECTION" AndAlso i + 3 < lines.Length AndAlso lines(i + 2).Trim() = "2" AndAlso lines(i + 3).Trim() = "ENTITIES" Then
                    inEntities = True
                    i += 4
                    Continue While
                End If
                If c = "0" AndAlso v = "ENDSEC" AndAlso inEntities Then
                    inEntities = False
                    i += 2
                    Continue While
                End If

                If inEntities AndAlso c = "0" Then
                    Dim name = v.ToUpperInvariant()
                    If name = "LWPOLYLINE" OrElse name = "POLYLINE" OrElse name = "LINE" OrElse name = "ARC" OrElse name = "CIRCLE" Then
                        Dim ent As New DxfEnt() With {.id = id}
                        id += 1
                        Select Case name
                            Case "LWPOLYLINE" : ent.Type = DxfEntType.LwPolyline
                            Case "POLYLINE" : ent.Type = DxfEntType.Polyline
                            Case "LINE" : ent.Type = DxfEntType.Line
                            Case "ARC" : ent.Type = DxfEntType.Arc
                            Case "CIRCLE" : ent.Type = DxfEntType.Circle
                        End Select

                        i += 2
                        Dim minx As Double = Double.PositiveInfinity, miny As Double = Double.PositiveInfinity, maxx As Double = Double.NegativeInfinity, maxy As Double = Double.NegativeInfinity
                        Dim curx As Double = 0, cury As Double = 0

                        While i < lines.Length - 1
                            Dim gc = lines(i).Trim()
                            Dim gv = lines(i + 1).Trim()
                            If gc = "0" Then Exit While
                            Select Case gc
                                Case "8" : ent.Layer = gv
                                Case "70" : ent.Closed = (gv = "1")
                                Case "10" : Double.TryParse(gv, NumberStyles.Float, CultureInfo.InvariantCulture, curx)
                                Case "20" : Double.TryParse(gv, NumberStyles.Float, CultureInfo.InvariantCulture, cury) : ent.Points.Add(Tuple.Create(curx, cury))
                                Case "40" : Double.TryParse(gv, NumberStyles.Float, CultureInfo.InvariantCulture, ent.Radius)
                                Case "50" : Double.TryParse(gv, NumberStyles.Float, CultureInfo.InvariantCulture, ent.StartAng)
                                Case "51" : Double.TryParse(gv, NumberStyles.Float, CultureInfo.InvariantCulture, ent.EndAng)
                            End Select
                            If ent.Points.Count > 0 Then
                                Dim last = ent.Points(ent.Points.Count - 1)
                                minx = Math.Min(minx, last.Item1)
                                miny = Math.Min(miny, last.Item2)
                                maxx = Math.Max(maxx, last.Item1)
                                maxy = Math.Max(maxy, last.Item2)
                            End If
                            i += 2
                        End While
                        If ent.Type = DxfEntType.Circle OrElse ent.Type = DxfEntType.Arc Then
                            ent.Center = Tuple.Create(curx, cury)
                            minx = Math.Min(minx, curx - ent.Radius)
                            miny = Math.Min(miny, cury - ent.Radius)
                            maxx = Math.Max(maxx, curx + ent.Radius)
                            maxy = Math.Max(maxy, cury + ent.Radius)
                        End If
                        If Double.IsInfinity(minx) Then
                            minx = 0 : miny = 0 : maxx = 0 : maxy = 0
                        End If
                        ent.BBox = Tuple.Create(minx, miny, maxx, maxy)
                        ents.Add(ent)
                        Continue While
                    Else
                        ' Skip unwanted entity blocks quickly
                        i += 2
                        While i < lines.Length - 1 AndAlso lines(i).Trim() <> "0"
                            i += 2
                        End While
                        Continue While
                    End If
                End If
                i += 2
            End While
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("ParseEntitiesShared failed: " & ex.Message)
        End Try
        Return ents
    End Function

    Public Shared Sub HighlightEntityInViewShared(app As Inventor.Application, e As DxfEnt)
        ' TODO: Implement ClientGraphics highlight. For now, log selection.
        Try
            System.Diagnostics.Debug.WriteLine("Preview highlight Id=" & e.Id & ", Type=" & e.Type.ToString() & ", Layer=" & e.Layer)
        Catch
        End Try
    End Sub

    Public Shared Function ApplyOverridesAndWriteShared(tempDxf As String, outDxf As String, overridesMap As Dictionary(Of Integer, DxfOverride), r12Mode As Boolean) As Boolean
        Try
            Dim lines = Global.System.IO.File.ReadAllLines(tempDxf, Encoding.GetEncoding("latin1")).ToList()
            Dim outLines As New List(Of String)()
            Dim i As Integer = 0
            Dim inEntities As Boolean = False
            Dim idx As Integer = -1

            While i < lines.Count - 1
                Dim c = lines(i).Trim()
                Dim v = lines(i + 1).Trim()
                If c = "0" AndAlso v = "SECTION" AndAlso i + 3 < lines.Count AndAlso lines(i + 2).Trim() = "2" AndAlso lines(i + 3).Trim() = "ENTITIES" Then
                    inEntities = True
                    outLines.Add(lines(i))
                    outLines.Add(lines(i + 1))
                    outLines.Add(lines(i + 2))
                    outLines.Add(lines(i + 3))
                    i += 4
                    Continue While
                End If
                If c = "0" AndAlso v = "ENDSEC" AndAlso inEntities Then
                    inEntities = False
                    outLines.Add(lines(i))
                    outLines.Add(lines(i + 1))
                    i += 2
                    Continue While
                End If

                If inEntities AndAlso c = "0" Then
                    Dim name = v.ToUpperInvariant()
                    Dim block As New List(Of String)()
                    block.Add(lines(i))
                    block.Add(lines(i + 1))
                    i += 2
                    While i < lines.Count - 1 AndAlso lines(i).Trim() <> "0"
                        block.Add(lines(i))
                        block.Add(lines(i + 1))
                        i += 2
                    End While

                    Dim supported = (name = "LWPOLYLINE" OrElse name = "POLYLINE" OrElse name = "LINE" OrElse name = "ARC" OrElse name = "CIRCLE")
                    If Not supported Then
                        Continue While
                    End If

                    idx += 1
                    Dim ov As DxfOverride = Nothing
                    overridesMap.TryGetValue(idx, ov)
                    If ov IsNot Nothing AndAlso Not ov.Include Then
                        Continue While
                    End If

                    If ov IsNot Nothing AndAlso Not String.IsNullOrEmpty(ov.LayerOverride) Then
                        For j As Integer = 0 To block.Count - 2 Step 2
                            If block(j).Trim() = "8" Then
                                block(j + 1) = ov.LayerOverride
                                Exit For
                            End If
                        Next
                    End If

                    If r12Mode AndAlso name = "LWPOLYLINE" Then
                        Dim layerVal As String = "0"
                        Dim closed As Boolean = False
                        Dim verts As New List(Of Tuple(Of Double, Double))
                        For j As Integer = 0 To block.Count - 2 Step 2
                            Dim gc = block(j).Trim()
                            Dim gv = block(j + 1).Trim()
                            If gc = "8" Then layerVal = gv
                            If gc = "70" Then closed = (gv = "1")
                            If gc = "10" Then
                                Dim x As Double = 0
                                Dim y As Double = 0
                                Double.TryParse(gv, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, x)
                                If j + 2 < block.Count AndAlso block(j + 2).Trim() = "20" Then
                                    Double.TryParse(block(j + 3).Trim(), Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, y)
                                    verts.Add(Tuple.Create(x, y))
                                End If
                            End If
                        Next
                        outLines.Add("  0")
                        outLines.Add("POLYLINE")
                        outLines.Add("  8")
                        outLines.Add(layerVal)
                        outLines.Add(" 66")
                        outLines.Add("1")
                        outLines.Add(" 70")
                        outLines.Add(If(closed, "1", "0"))
                        For Each vtx In verts
                            outLines.Add("  0")
                            outLines.Add("VERTEX")
                            outLines.Add("  8")
                            outLines.Add(layerVal)
                            outLines.Add(" 10")
                            outLines.Add(vtx.Item1.ToString("F6", CultureInfo.InvariantCulture))
                            outLines.Add(" 20")
                            outLines.Add(vtx.Item2.ToString("F6", CultureInfo.InvariantCulture))
                        Next
                        outLines.Add("  0")
                        outLines.Add("SEQEND")
                    Else
                        outLines.AddRange(block)
                    End If

                    Continue While
                End If

                outLines.Add(lines(i))
                outLines.Add(lines(i + 1))
                i += 2
            End While

            If outLines.Count < 2 OrElse Not (outLines(outLines.Count - 2).Trim() = "0" AndAlso outLines(outLines.Count - 1).Trim().ToUpperInvariant() = "EOF") Then
                outLines.Add("  0")
                outLines.Add("EOF")
            End If

            Global.System.IO.File.WriteAllLines(outDxf, outLines, Encoding.GetEncoding("latin1"))
            Return True
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("ApplyOverridesAndWriteShared failed: " & ex.Message)
            Return False
        End Try
    End Function

    Public Shared Function ValidateDxfShared(dxfPath As String, r12Mode As Boolean) As String
        Try
            Dim lines = Global.System.IO.File.ReadAllLines(dxfPath, Encoding.GetEncoding("latin1"))
            If lines.Length Mod 2 <> 0 Then Return "Odd line count"
            Dim hasEof As Boolean = False
            For i As Integer = 0 To lines.Length - 2 Step 2
                If lines(i).Trim() = "0" AndAlso lines(i + 1).Trim().ToUpperInvariant() = "EOF" Then
                    hasEof = True
                End If
            Next
            If Not hasEof Then Return "Missing EOF"
            For i As Integer = 0 To lines.Length - 1 Step 2
                Dim tmp As Integer
                If Not Integer.TryParse(lines(i).Trim(), tmp) Then Return "Non-integer code at line " & (i + 1).ToString()
            Next
            Dim inEnt As Boolean = False
            Dim iIdx As Integer = 0
            While iIdx < lines.Length - 1
                Dim c = lines(iIdx).Trim()
                Dim v = lines(iIdx + 1).Trim().ToUpperInvariant()
                If c = "0" AndAlso v = "SECTION" AndAlso iIdx + 3 < lines.Length AndAlso lines(iIdx + 2).Trim() = "2" AndAlso lines(iIdx + 3).Trim().ToUpperInvariant() = "ENTITIES" Then
                    inEnt = True
                    iIdx += 4
                    Continue While
                End If
                If c = "0" AndAlso v = "ENDSEC" AndAlso inEnt Then
                    inEnt = False
                    iIdx += 2
                    Continue While
                End If
                If inEnt AndAlso c = "0" Then
                    If v = "TEXT" OrElse v = "MTEXT" OrElse v = "POINT" OrElse v = "HATCH" Then Return "Disallowed entity " & v
                    If r12Mode AndAlso v = "LWPOLYLINE" Then Return "R12 cannot contain LWPOLYLINE"
                End If
                iIdx += 2
            End While
            Return "OK"
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

End Class
