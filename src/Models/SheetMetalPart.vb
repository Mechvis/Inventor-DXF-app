Imports Inventor

''' <summary>
''' Represents a sheet metal part with its properties and flat pattern information
''' </summary>
Public Class SheetMetalPart
    
    Public Property Document As PartDocument
    Public Property ComponentDefinition As SheetMetalComponentDefinition
    Public Property FileName As String
    Public Property PartName As String
    
    ' Material Properties
    Public Property Material As String = ""
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
        FileName = System.IO.Path.GetFileNameWithoutExtension(partDoc.FullFileName)
        PartName = partDoc.DisplayName

        ' Extract properties
        ExtractMaterialProperties()
        ExtractGeometricProperties()
        ExtractCustomProperties()
        ExtractSourceFlags()
    End Sub

    Private Sub ExtractMaterialProperties()
        Try
            If Document.PropertySets("Design Tracking Properties")("Material").Value IsNot Nothing Then
                Material = Document.PropertySets("Design Tracking Properties")("Material").Value.ToString()
            End If

            ' Get thickness from sheet metal definition
            If ComponentDefinition.Thickness IsNot Nothing Then
                Thickness = ComponentDefinition.Thickness.Value
            End If

            ' Calculate weight if mass properties available
            If Document.ComponentDefinition.MassProperties IsNot Nothing Then
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

    Private Function IsContentCenterPart(doc As PartDocument) As Boolean
        Try
            Dim ps = doc.PropertySets.Item("Design Tracking Properties")
            Dim p = ps.Item("Is Content Center")
            Return CBool(p.Value)
        Catch
            Return False
        End Try
    End Function

    Public ReadOnly Property FormattedFileName As String
        Get
            Return $"{PartName}_{Thickness:F1}mm"
        End Get
    End Property

    Public ReadOnly Property MaterialInfo As String
        Get
            Return $"{Material} - {Thickness:F2}mm"
        End Get
    End Property
End Class