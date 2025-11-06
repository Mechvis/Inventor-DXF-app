Imports Inventor
Imports System.Text

''' <summary>
''' Generates center marks (cross lines) from hole center points in DXF files
''' Inventor exports hole centers as POINT entities, this converts them to fabrication-friendly cross marks
''' </summary>
Public Class CenterMarkGenerator

    Private ReadOnly _inventorApp As Inventor.Application
    Private ReadOnly _settings As ExportSettings

    ' Center mark dimensions (in mm)
    Private Const DEFAULT_MARK_SIZE As Double = 2.0
    Private Const DEFAULT_GAP_SIZE As Double = 0.5

    Public Sub New(inventorApp As Inventor.Application, settings As ExportSettings)
        _inventorApp = inventorApp
        _settings = settings
    End Sub

    ''' <summary>
    ''' Generate center marks for a sheet metal part's DXF file
    ''' This processes the DXF file after export to replace POINT entities with cross marks
    ''' </summary>
    Public Sub GenerateCenterMarks(part As SheetMetalPart, dxfFilePath As String)
        Try
            Dim centers As List(Of Point2D)

            If _settings.UseModelForCenterMarks Then
                centers = ExtractCentersFromModel(part)
            Else
                ' Extract hole center points from the exported DXF (POINT entities)
                centers = ExtractHoleCentersFromDXF(dxfFilePath)
            End If

            If centers.Count > 0 Then
                ' Optional filtering by feature type
                Dim filtered = FilterCentersByFeatureType(part, centers)
                If filtered.Count > 0 Then
                    AddCenterMarksToDXF(dxfFilePath, filtered)
                    System.Diagnostics.Debug.WriteLine($"Generated {filtered.Count} center marks for {part.PartName}")
                End If
            End If

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Error generating center marks: {ex.Message}")
            Throw New Exception($"Center mark generation failed: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Filter the detected centers by model feature type using settings toggles.
    ''' When using DXF-based detection, we cannot distinguish feature types; return as-is.
    ''' When using model-derived centers, we apply drilled/threaded toggles.
    ''' </summary>
    Private Function FilterCentersByFeatureType(part As SheetMetalPart, centers As List(Of Point2D)) As List(Of Point2D)
        If Not _settings.UseModelForCenterMarks Then
            Return centers
        End If

        ' We will re-derive centers and feature types from the model and map them to 2D.
        ' For simplicity, GenerateCenterMarks calls ExtractCentersFromModel already and this
        ' function assumes 'centers' are pre-filtered. Keep hook for future refinements.
        Return centers
    End Function

    ''' <summary>
    ''' Extract centers from the model (threaded/tapped and drilled holes) and map to flat pattern 2D
    ''' - Tapped/threaded holes: include when IncludeCenterMarksForThreadedHoles is enabled
    ''' - Drilled holes: include when IncludeCenterMarksForDrilledHoles is enabled
    ''' Uses HoleFeature.HoleCenterPoints for robust center retrieval
    ''' </summary>
    Private Function ExtractCentersFromModel(part As SheetMetalPart) As List(Of Point2D)
        Dim result As New List(Of Point2D)()
        Try
            Dim doc = part.Document
            Dim smDef = TryCast(doc.ComponentDefinition, SheetMetalComponentDefinition)
            If smDef Is Nothing Then Return result

            ' Ensure flat pattern exists for mapping
            Dim fp = smDef.FlatPattern

            ' Iterate hole features and decide inclusion by type
            Dim featureSet = doc.ComponentDefinition.Features
            Dim holeFeatures = featureSet.HoleFeatures

            For Each hf As HoleFeature In holeFeatures
                Try
                    If hf Is Nothing OrElse hf.Suppressed Then Continue For

                    Dim isTapped As Boolean = False
                    Try
                        isTapped = hf.Tapped
                    Catch
                        ' Older versions may not expose .Tapped consistently
                    End Try

                    Dim includeThis As Boolean = False
                    If isTapped Then
                        includeThis = _settings.IncludeCenterMarksForThreadedHoles
                    Else
                        includeThis = _settings.IncludeCenterMarksForDrilledHoles
                    End If

                    If Not includeThis Then Continue For

                    ' Use HoleCenterPoints collection whenever available
                    Dim centers = hf.HoleCenterPoints
                    If centers IsNot Nothing Then
                        For Each p3d As Point In centers
                            Try
                                Dim pt2 As Point2D = fp.GetPointMapping(p3d)
                                result.Add(New Point2D(pt2.X, pt2.Y))
                            Catch
                                ' ignore mapping failures for this point
                            End Try
                        Next
                    Else
                        ' Fallback: try placement definition-derived point
                        Dim pdef = hf.PlacementDefinition
                        If pdef IsNot Nothing Then
                            Try
                                Dim pnt As Point = Nothing
                                ' Not all placement defs expose a Center; skip if missing
                                Dim hasCenter As Boolean = False
                                Try
                                    pnt = pdef.Plane.Center
                                    hasCenter = (pnt IsNot Nothing)
                                Catch
                                End Try
                                If hasCenter Then
                                    Dim pt2 As Point2D = fp.GetPointMapping(pnt)
                                    result.Add(New Point2D(pt2.X, pt2.Y))
                                End If
                            Catch
                            End Try
                        End If
                    End If
                Catch
                    ' continue with next hole feature
                End Try
            Next

            ' Optional: external ThreadFeatures (rare for sheet metal). Best-effort axis mapping.
            If _settings.IncludeCenterMarksForThreadedHoles Then
                Try
                    For Each th As ThreadFeature In featureSet.ThreadFeatures
                        Try
                            If th Is Nothing OrElse th.Suppressed Then Continue For
                            ' Try grabbing a representative point along the axis if accessible
                            Dim axis As WorkAxis = Nothing
                            Try
                                axis = th.Axis
                            Catch
                                axis = Nothing
                            End Try
                            If axis IsNot Nothing Then
                                Dim p3d As Point = axis.Geometry.Line.StartPoint
                                Dim pt2 As Point2D = fp.GetPointMapping(p3d)
                                result.Add(New Point2D(pt2.X, pt2.Y))
                            End If
                        Catch
                        End Try
                    Next
                Catch
                    ' Ignore if ThreadFeatures not accessible
                End Try
            End If

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"ExtractCentersFromModel failed: {ex.Message}")
        End Try
        Return result
    End Function

    ''' <summary>
    ''' Extract hole centers by parsing POINT entities from the DXF file
    ''' </summary>
    Private Function ExtractHoleCentersFromDXF(dxfFilePath As String) As List(Of Point2D)
        Dim centers As New List(Of Point2D)()

        Try
            Dim lines = System.IO.File.ReadAllLines(dxfFilePath)
            Dim i As Integer = 0
            While i < lines.Length - 1
                Dim code = lines(i).Trim()
                Dim value = lines(i + 1).Trim()

                If code = "0" AndAlso value = "POINT" Then
                    Dim layerName As String = Nothing
                    Dim x As Double = 0
                    Dim y As Double = 0

                    ' Read entity group codes until next entity (code 0) or end of file
                    Dim j As Integer = i + 2
                    While j < lines.Length - 1 AndAlso Not (lines(j).Trim() = "0")
                        Dim g = lines(j).Trim()
                        Dim v = lines(j + 1).Trim()
                        Select Case g
                            Case "8"
                                layerName = v
                            Case "10"
                                Double.TryParse(v, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, x)
                            Case "20"
                                Double.TryParse(v, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, y)
                        End Select
                        j += 2
                    End While

                    ' Only include points from the configured hole centers layer if specified
                    If String.IsNullOrEmpty(_settings?.HoleCentersLayer?.LayerName) OrElse _settings.HoleCentersLayer.LayerName.Equals(layerName, StringComparison.OrdinalIgnoreCase) Then
                        centers.Add(New Point2D(x, y))
                    End If

                    i = j ' continue from next entity
                    Continue While
                End If

                i += 2
            End While
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Error parsing DXF for hole centers: {ex.Message}")
        End Try

        Return centers
    End Function

    ''' <summary>
    ''' Add center mark geometry to existing DXF file by appending DXF entities
    ''' </summary>
    Private Sub AddCenterMarksToDXF(dxfFilePath As String, holeCenters As List(Of Point2D))
        Try
            ' Read the existing DXF content
            Dim dxfContent = System.IO.File.ReadAllText(dxfFilePath)

            ' Find the ENTITIES section end
            Dim entitiesEndIndex = dxfContent.LastIndexOf("ENDSEC")
            If entitiesEndIndex = -1 Then
                Throw New Exception("Could not find ENTITIES section end in DXF file")
            End If

            ' Generate center mark DXF entities
            Dim centerMarkDxf = GenerateCenterMarkDXFEntities(holeCenters)

            ' Insert center marks before ENDSEC
            Dim modifiedContent = dxfContent.Substring(0, entitiesEndIndex) &
                                 centerMarkDxf &
                                 dxfContent.Substring(entitiesEndIndex)

            ' Write back to file
            System.IO.File.WriteAllText(dxfFilePath, modifiedContent)

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Error modifying DXF file: {ex.Message}")
            ' Don't throw - center marks are optional
        End Try
    End Sub

    ''' <summary>
    ''' Generate DXF entity strings for center marks (cross lines)
    ''' </summary>
    Private Function GenerateCenterMarkDXFEntities(holeCenters As List(Of Point2D)) As String
        Dim sb As New StringBuilder()

        Dim layerName As String = If(_settings?.HoleCentersLayer?.LayerName, "IV_TOOL_CENTER")

        For Each center In holeCenters
            ' Create horizontal line (left part)
            sb.AppendLine("  0")
            sb.AppendLine("LINE")
            sb.AppendLine("  8")
            sb.AppendLine(layerName)
            sb.AppendLine(" 10")
            sb.AppendLine((center.X - DEFAULT_MARK_SIZE).ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 20")
            sb.AppendLine(center.Y.ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 11")
            sb.AppendLine((center.X - DEFAULT_GAP_SIZE).ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 21")
            sb.AppendLine(center.Y.ToString("F3", Globalization.CultureInfo.InvariantCulture))

            ' Create horizontal line (right part)
            sb.AppendLine("  0")
            sb.AppendLine("LINE")
            sb.AppendLine("  8")
            sb.AppendLine(layerName)
            sb.AppendLine(" 10")
            sb.AppendLine((center.X + DEFAULT_GAP_SIZE).ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 20")
            sb.AppendLine(center.Y.ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 11")
            sb.AppendLine((center.X + DEFAULT_MARK_SIZE).ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 21")
            sb.AppendLine(center.Y.ToString("F3", Globalization.CultureInfo.InvariantCulture))

            ' Create vertical line (bottom part)
            sb.AppendLine("  0")
            sb.AppendLine("LINE")
            sb.AppendLine("  8")
            sb.AppendLine(layerName)
            sb.AppendLine(" 10")
            sb.AppendLine(center.X.ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 20")
            sb.AppendLine((center.Y - DEFAULT_MARK_SIZE).ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 11")
            sb.AppendLine(center.X.ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 21")
            sb.AppendLine((center.Y - DEFAULT_GAP_SIZE).ToString("F3", Globalization.CultureInfo.InvariantCulture))

            ' Create vertical line (top part)
            sb.AppendLine("  0")
            sb.AppendLine("LINE")
            sb.AppendLine("  8")
            sb.AppendLine(layerName)
            sb.AppendLine(" 10")
            sb.AppendLine(center.X.ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 20")
            sb.AppendLine((center.Y + DEFAULT_GAP_SIZE).ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 11")
            sb.AppendLine(center.X.ToString("F3", Globalization.CultureInfo.InvariantCulture))
            sb.AppendLine(" 21")
            sb.AppendLine((center.Y + DEFAULT_MARK_SIZE).ToString("F3", Globalization.CultureInfo.InvariantCulture))
        Next

        Return sb.ToString()
    End Function
End Class

''' <summary>
''' Simple 2D point structure for center mark coordinates
''' </summary>
Public Structure Point2D
    Public X As Double
    Public Y As Double

    Public Sub New(x As Double, y As Double)
        Me.X = x
        Me.Y = y
    End Sub
End Structure
