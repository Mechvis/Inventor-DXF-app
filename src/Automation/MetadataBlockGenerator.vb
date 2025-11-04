Imports System.IO
Imports System.Text

''' <summary>
''' Generates metadata information blocks for DXF files
''' Adds material, quantity, weight, dimensions, and other fabrication info
''' </summary>
Public Class MetadataBlockGenerator
    
    Private ReadOnly _settings As ExportSettings
    
    ' Text positioning constants (in mm)
    Private Const TEXT_HEIGHT As Double = 2.5
    Private Const LINE_SPACING As Double = 3.5
    Private Const MARGIN_X As Double = 5.0
    Private Const MARGIN_Y As Double = 5.0
    
    Public Sub New(settings As ExportSettings)
        _settings = settings
    End Sub
    
    ''' <summary>
    ''' Add metadata block to an existing DXF file
    ''' </summary>
    Public Sub AddMetadataToFile(part As SheetMetalPart, dxfFilePath As String)
        Try
            ' Generate metadata text lines
            Dim metadataLines = GenerateMetadataLines(part)
            
            If metadataLines.Count > 0 Then
                ' Read existing DXF content
                Dim dxfContent = File.ReadAllText(dxfFilePath)
                
                ' Get insertion position based on settings
                Dim insertPos = CalculateInsertionPosition(part, dxfContent)
                
                ' Generate DXF text entities
                Dim textEntities = GenerateTextEntities(metadataLines, insertPos)
                
                ' Insert into DXF file
                Dim modifiedContent = InsertTextIntoDXF(dxfContent, textEntities)
                
                ' Write back to file
                File.WriteAllText(dxfFilePath, modifiedContent)
                
                System.Diagnostics.Debug.WriteLine($"Added metadata block to {Path.GetFileName(dxfFilePath)}")
            End If
            
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Error adding metadata: {ex.Message}")
            ' Don't throw - metadata is optional
        End Try
    End Sub
    
    ''' <summary>
    ''' Generate list of metadata text lines based on settings
    ''' </summary>
    Private Function GenerateMetadataLines(part As SheetMetalPart) As List(Of String)
        Dim lines As New List(Of String)
        
        ' Part identification
        lines.Add($"PART: {part.PartName}")
        
        If _settings.IncludeMaterial AndAlso Not String.IsNullOrEmpty(part.Material) Then
            lines.Add($"MATERIAL: {part.Material}")
            lines.Add($"THICKNESS: {part.Thickness:F2} mm")
        End If
        
        If _settings.IncludeQuantity Then
            ' Default quantity is 1, could be configured per part
            lines.Add("QTY: 1")
        End If
        
        If _settings.IncludeWeight AndAlso part.Weight > 0 Then
            lines.Add($"WEIGHT: {part.Weight:F3} kg")
        End If
        
        If _settings.IncludeDimensions Then
            lines.Add($"SIZE: {part.BoundingBoxLength:F1} x {part.BoundingBoxWidth:F1} mm")
        End If
        
        If _settings.IncludeSurfaceArea AndAlso part.SurfaceArea > 0 Then
            lines.Add($"AREA: {part.SurfaceArea:F2} cm²")
        End If
        
        If _settings.IncludeBendCount AndAlso part.BendCount > 0 Then
            lines.Add($"BENDS: {part.BendCount}")
        End If
        
        ' Export information
        lines.Add($"EXPORTED: {DateTime.Now:yyyy-MM-dd HH:mm}")
        lines.Add($"BY: {Environment.UserName}")
        
        If _settings.IncludeCustomProperties Then
            ' Add important custom properties
            For Each kvp In part.CustomProperties
                If IsImportantProperty(kvp.Key) Then
                    lines.Add($"{kvp.Key.ToUpper()}: {kvp.Value}")
                End If
            Next
        End If
        
        Return lines
    End Function
    
    ''' <summary>
    ''' Determine if a custom property should be included in metadata
    ''' </summary>
    Private Function IsImportantProperty(propertyName As String) As Boolean
        Dim importantProps = {"Part Number", "Revision", "Description", "Vendor", "Stock Number"}
        Return importantProps.Any(Function(prop) prop.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
    End Function
    
    ''' <summary>
    ''' Calculate insertion position for metadata block
    ''' </summary>
    Private Function CalculateInsertionPosition(part As SheetMetalPart, dxfContent As String) As Point2D
        Select Case _settings.MetadataPosition
            Case MetadataPosition.BottomLeft
                Return New Point2D(MARGIN_X, MARGIN_Y)
                
            Case MetadataPosition.BottomRight
                Dim rightX = part.BoundingBoxLength - MARGIN_X - EstimateTextWidth()
                Return New Point2D(rightX, MARGIN_Y)
                
            Case MetadataPosition.TopLeft
                Dim topY = part.BoundingBoxWidth - MARGIN_Y - EstimateBlockHeight()
                Return New Point2D(MARGIN_X, topY)
                
            Case MetadataPosition.TopRight
                Dim rightX = part.BoundingBoxLength - MARGIN_X - EstimateTextWidth()
                Dim topY = part.BoundingBoxWidth - MARGIN_Y - EstimateBlockHeight()
                Return New Point2D(rightX, topY)
                
            Case MetadataPosition.SeparateArea
                ' Position outside part boundary
                Return New Point2D(part.BoundingBoxLength + 10, MARGIN_Y)
                
            Case Else ' UserDefined or fallback
                Return New Point2D(MARGIN_X, MARGIN_Y)
        End Select
    End Function
    
    ''' <summary>
    ''' Estimate text width for positioning calculations
    ''' </summary>
    Private Function EstimateTextWidth() As Double
        ' Rough estimate: average character width * max characters per line
        Return TEXT_HEIGHT * 0.6 * 30 ' Assume max 30 characters
    End Function
    
    ''' <summary>
    ''' Estimate total block height for positioning
    ''' </summary>
    Private Function EstimateBlockHeight() As Double
        ' Estimate based on typical number of lines
        Return LINE_SPACING * 8 ' Assume typical 8 lines
    End Function
    
    ''' <summary>
    ''' Generate DXF TEXT entities for metadata lines
    ''' </summary>
    Private Function GenerateTextEntities(metadataLines As List(Of String), startPos As Point2D) As String
        Dim sb As New StringBuilder()
        
        For i As Integer = 0 To metadataLines.Count - 1
            Dim yPos = startPos.Y - (i * LINE_SPACING)
            
            sb.AppendLine("  0")
            sb.AppendLine("TEXT")
            sb.AppendLine("  8")
            sb.AppendLine(_settings.TextLayer.LayerName)
            sb.AppendLine(" 10")
            sb.AppendLine(startPos.X.ToString("F3"))
            sb.AppendLine(" 20")
            sb.AppendLine(yPos.ToString("F3"))
            sb.AppendLine(" 40")
            sb.AppendLine(TEXT_HEIGHT.ToString("F3"))
            sb.AppendLine("  1")
            sb.AppendLine(metadataLines(i))
            sb.AppendLine(" 50")
            sb.AppendLine("0.0") ' Rotation angle
            sb.AppendLine(" 72")
            sb.AppendLine("0") ' Horizontal justification (left)
            sb.AppendLine(" 73")
            sb.AppendLine("0") ' Vertical justification (baseline)
        Next
        
        Return sb.ToString()
    End Function
    
    ''' <summary>
    ''' Insert text entities into DXF file content
    ''' </summary>
    Private Function InsertTextIntoDXF(dxfContent As String, textEntities As String) As String
        Try
            ' Find the ENTITIES section end
            Dim entitiesEndIndex = dxfContent.LastIndexOf("ENDSEC")
            If entitiesEndIndex = -1 Then
                Throw New Exception("Could not find ENTITIES section end")
            End If
            
            ' Insert text entities before ENDSEC
            Return dxfContent.Substring(0, entitiesEndIndex) & _
                   textEntities & _
                   dxfContent.Substring(entitiesEndIndex)
            
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Error inserting text into DXF: {ex.Message}")
            Return dxfContent ' Return original if modification fails
        End Try
    End Function
    
    ''' <summary>
    ''' Create a separate metadata file alongside the DXF (alternative approach)
    ''' </summary>
    Public Sub CreateSeparateMetadataFile(part As SheetMetalPart, dxfFilePath As String)
        Try
            Dim metadataPath = Path.ChangeExtension(dxfFilePath, ".txt")
            Dim metadataLines = GenerateMetadataLines(part)
            
            File.WriteAllLines(metadataPath, metadataLines)
            System.Diagnostics.Debug.WriteLine($"Created metadata file: {Path.GetFileName(metadataPath)}")
            
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Error creating metadata file: {ex.Message}")
        End Try
    End Sub
End Class