Imports Inventor
Imports System
Imports System.Globalization

''' <summary>
''' Represents a sheet metal part with its properties and flat pattern information
''' </summary>
Public Class SheetMetalPart

    Public Property Document As PartDocument
    Public Property ComponentDefinition As SheetMetalComponentDefinition
    Public Property FileName As String
    Public Property PartName As String
    Public Property PartNumber As String = ""
    Public Property StockNumber As String = ""
    Public Property Revision As String = "A"

    ' Material Properties
    Public Property Material As String = ""
    ' Thickness stored in millimeters for external use (DB, filenames)
    Public Property Thickness As Double = 0.0
    Public Property Weight As Double = 0.0
    Public Property Density As Double = 0.0

    ' Geometric Properties
    Public Property SurfaceArea As Double = 0.0
    Public Property BoundingBoxLength As Double = 0.0
    Public Property BoundingBoxWidth As Double = 0.0
    Public Property BendCount As Integer = 0

    ' Custom Properties
    Public Property CustomProperties As New Dictionary(Of String, String)

    ' Export Status
    Public Property IsSelected As Boolean = True
    Public Property ExportPath As String = ""
    Public Property HasFlatPattern As Boolean = False

    ' Read-only and source flags for export decisions
    Public Property IsReadOnly As Boolean = False
    Public Property IsContentCenter As Boolean = False
    Public Property NeedsUpdate As Boolean = False

    Public Sub New(partDoc As PartDocument)
        Document = partDoc
        ComponentDefinition = CType(partDoc.ComponentDefinition, SheetMetalComponentDefinition)

        ' Get filename without .ipt extension
        Dim fullFileName As String = partDoc.FullFileName
        If Not String.IsNullOrEmpty(fullFileName) Then
            FileName = System.IO.Path.GetFileNameWithoutExtension(fullFileName)
        Else
            FileName = partDoc.DisplayName
        End If

        ' Remove .ipt extension if present in DisplayName
        PartName = partDoc.DisplayName
        If PartName.EndsWith(".ipt", System.StringComparison.OrdinalIgnoreCase) Then
            PartName = PartName.Substring(0, PartName.Length - 4)
        End If

        ' Extract part number and stock number from iProperties
        ExtractPartNumberAndStockNumber()

        ' Extract properties
        ExtractMaterialProperties()
        ExtractGeometricProperties()
        ExtractCustomProperties()
        ExtractSourceFlags()
        ExtractRevision()

        ' Normalize stock number representation once we know thickness
        StockNumber = NormalizeStockNumber(StockNumber, Thickness)
    End Sub

    ''' <summary>
    ''' Extract Part Number and Stock Number from iProperties
    ''' </summary>
    Private Sub ExtractPartNumberAndStockNumber()
        Try
            ' Try to get Part Number from Design Tracking Properties
            Dim designProps As PropertySet = Document.PropertySets("Design Tracking Properties")
            For Each p As Inventor.Property In designProps
                If String.Equals(p.Name, "Part Number", StringComparison.OrdinalIgnoreCase) AndAlso Not IsNothing(p.Value) Then
                    PartNumber = p.Value.ToString().Trim()
                    Exit For
                End If
            Next

            ' If Part Number not found in design tracking, try custom properties
            If String.IsNullOrWhiteSpace(PartNumber) Then
                Dim userProps As PropertySet = Document.PropertySets("Inventor User Defined Properties")
                For Each p As Inventor.Property In userProps
                    If String.Equals(p.Name, "Part Number", StringComparison.OrdinalIgnoreCase) AndAlso Not IsNothing(p.Value) Then
                        PartNumber = p.Value.ToString().Trim()
                        Exit For
                    End If
                Next
            End If

            ' Fallback to filename if Part Number not found
            If String.IsNullOrWhiteSpace(PartNumber) Then
                PartNumber = FileName
            End If

            ' Stock Number: first check Design Tracking Properties (Project tab)
            For Each p As Inventor.Property In designProps
                If String.Equals(p.Name, "Stock Number", StringComparison.OrdinalIgnoreCase) AndAlso Not IsNothing(p.Value) Then
                    StockNumber = p.Value.ToString().Trim()
                    Exit For
                End If
            Next

            ' If not in design tracking, try User Defined Properties
            If String.IsNullOrWhiteSpace(StockNumber) Then
                Dim customProps As PropertySet = Document.PropertySets("Inventor User Defined Properties")
                For Each p As Inventor.Property In customProps
                    If String.Equals(p.Name, "Stock Number", StringComparison.OrdinalIgnoreCase) AndAlso Not IsNothing(p.Value) Then
                        StockNumber = p.Value.ToString().Trim()
                        Exit For
                    End If
                Next

                ' Also check for common variations
                If String.IsNullOrWhiteSpace(StockNumber) Then
                    For Each p As Inventor.Property In customProps
                        Dim propName = p.Name.ToLowerInvariant()
                        If (propName = "stock no" OrElse propName = "stock" OrElse propName = "stockno" OrElse propName = "material stock") AndAlso Not IsNothing(p.Value) Then
                            StockNumber = p.Value.ToString().Trim()
                            Exit For
                        End If
                    Next
                End If
            End If

            ' Defer to thickness if nothing found; will be formatted later
            If String.IsNullOrWhiteSpace(StockNumber) Then
                StockNumber = ""
            End If

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Could not extract Part Number or Stock Number: {ex.Message}")
            ' Use fallbacks
            If String.IsNullOrWhiteSpace(PartNumber) Then
                PartNumber = FileName
            End If
        End Try
    End Sub

    Private Sub ExtractMaterialProperties()
        Try
            If Document.PropertySets("Design Tracking Properties")("Material").Value IsNot Nothing Then
                Material = Document.PropertySets("Design Tracking Properties")("Material").Value.ToString()
            End If

            ' Get thickness from sheet metal definition
            ' Inventor API internal length units are centimeters (cm). Convert to millimeters (mm).
            If ComponentDefinition.Thickness IsNot Nothing Then
                Dim thicknessCm As Double = ComponentDefinition.Thickness.Value
                Thickness = thicknessCm * 10.0 ' cm -> mm

                ' If Stock Number wasn't found in properties, use thickness as fallback
                If String.IsNullOrWhiteSpace(StockNumber) Then
                    StockNumber = $"{Thickness:F1} mm"
                End If
            End If

            ' Calculate weight if mass properties available
            If Document.ComponentDefinition.MassProperties Is Not Nothing Then
                Weight = Document.ComponentDefinition.MassProperties.Mass
                Density = Document.ComponentDefinition.MassProperties.Density
            End If

        Catch ex As Exception
            ' Material properties may not always be available
            System.Diagnostics.Debug.WriteLine($"Could not extract material properties: {ex.Message}")
        End Try
    End Sub

    Private Sub ExtractGeometricProperties()
        Try
            HasFlatPattern = ComponentDefinition.HasFlatPattern

            If HasFlatPattern Then
                ' Get flat pattern surface area
                Dim flatPattern = ComponentDefinition.FlatPattern
                If flatPattern IsNot Nothing Then
                    SurfaceArea = flatPattern.SurfaceArea

                    ' Get bounding box of flat pattern
                    Dim bbox = flatPattern.RangeBox
                    BoundingBoxLength = Math.Abs(bbox.MaxPoint.X - bbox.MinPoint.X)
                    BoundingBoxWidth = Math.Abs(bbox.MaxPoint.Y - bbox.MinPoint.Y)
                End If
            End If

            ' Count bends (simplified - actual implementation would analyze features)
            BendCount = ComponentDefinition.Features.SheetMetalFeatures.FlangeFeatures.Count +
                       ComponentDefinition.Features.SheetMetalFeatures.FoldFeatures.Count

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Could not extract geometric properties: {ex.Message}")
        End Try
    End Sub

    Private Sub ExtractCustomProperties()
        Try
            ' Extract custom iProperties
            For Each propSet As PropertySet In Document.PropertySets
                If propSet.Name = "Inventor User Defined Properties" Then
                    For Each prop As [Property] In propSet
                        If prop.Value IsNot Nothing Then
                            CustomProperties(prop.Name) = prop.Value.ToString()
                        End If
                    Next
                End If
            Next

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Could not extract custom properties: {ex.Message}")
        End Try
    End Sub

    Private Sub ExtractSourceFlags()
        Try
            IsReadOnly = Document.ReadOnly
            IsContentCenter = IsContentCenterPart(Document)
            NeedsUpdate = Document.RequiresUpdate OrElse (HasFlatPattern AndAlso ComponentDefinition.FlatPattern.RequiresUpdate)
        Catch ex As Exception
            ' ignore
        End Try
    End Sub

    Private Sub ExtractRevision()
        Try
            ' Try built-in revision first
            Dim designProps As PropertySet = Document.PropertySets("Design Tracking Properties")
            For Each p As Inventor.Property In designProps
                If String.Equals(p.Name, "Revision Number", StringComparison.OrdinalIgnoreCase) AndAlso Not IsNothing(p.Value) Then
                    Revision = p.Value.ToString()
                    Exit Sub
                End If
            Next
            ' Fallback to user-defined "Rev"
            Dim userProps As PropertySet = Document.PropertySets("Inventor User Defined Properties")
            For Each p As Inventor.Property In userProps
                If String.Equals(p.Name, "Rev", StringComparison.OrdinalIgnoreCase) AndAlso Not IsNothing(p.Value) Then
                    Revision = p.Value.ToString()
                    Exit Sub
                End If
            Next
        Catch
            ' keep default
        End Try
        If String.IsNullOrWhiteSpace(Revision) Then Revision = "A"
    End Sub

    Private Function IsContentCenterPart(doc As PartDocument) As Boolean
        Try
            Dim ps = doc.PropertySets.Item("Design Tracking Properties")
            Dim p = ps.Item("Is Content Center")
            Return CBool(p.Value)
        Catch
            Return False
        End Try
    End Function

    Private Shared Function NormalizeStockNumber(raw As String, thicknessMm As Double) As String
        Try
            Dim s = If(raw, "").Trim()
            If Not String.IsNullOrWhiteSpace(s) Then
                ' Just sanitize invalid filename characters; do not convert text or units
                For Each ch In System.IO.Path.GetInvalidFileNameChars()
                    s = s.Replace(ch, "_"c)
                Next
                Return s
            End If
            ' Fallback to numeric thickness in millimeters if no stock number iProperty is present
            Return thicknessMm.ToString("F1", CultureInfo.InvariantCulture) & " mm"
        Catch
            Return thicknessMm.ToString("F1", CultureInfo.InvariantCulture) & " mm"
        End Try
    End Function

    Public ReadOnly Property FormattedFileName As String
        Get
            ' Format: PartNumber_StockNumber_Revision
            Dim baseName = $"{PartNumber}_{StockNumber}_{Revision}"
            ' Sanitize for filesystem
            For Each invalidChar In System.IO.Path.GetInvalidFileNameChars()
                baseName = baseName.Replace(invalidChar, "_"c)
            Next
            Return baseName
        End Get
    End Property

    Public ReadOnly Property MaterialInfo As String
        Get
            ' Display material and stock number in UI
            Return $"{Material} - {StockNumber}"
        End Get
    End Property
End Class
