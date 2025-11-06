''' <summary>
''' Configuration settings for DXF export with layer and feature options
''' </summary>
Public Class ExportSettings
    
    ' Layer Control Options
    Public Property OuterProfileLayer As LayerSettings
    Public Property FeatureProfilesLayer As LayerSettings
    Public Property BendLinesLayer As LayerSettings
    Public Property EtchingLayer As LayerSettings
    Public Property TextLayer As LayerSettings
    Public Property HoleCentersLayer As LayerSettings
    Public Property TangentLayer As LayerSettings
    Public Property CornerReliefLayer As LayerSettings
    Public Property FormFeaturesLayer As LayerSettings
    Public Property PunchToolMarksLayer As LayerSettings

    ' Export Options
    Public Property AcadVersion As String = "2018"
    Public Property RebaseGeometry As Boolean = True
    Public Property MergeProfilesIntoPolyline As Boolean = True
    Public Property ExportPath As String = ""
    Public Property CustomFileNaming As Boolean = True
    Public Property FileNameTemplate As String = "{PartNumber}_{StockNumber}_REV_{Rev}"
    ' New: ensure flat pattern exists before export
    Public Property EnsureFlatPatternBeforeExport As Boolean = True
    
    ' Export history and archival options
    Public Property EnableExportHistory As Boolean = True
    Public Property ArchiveDuplicates As Boolean = True
    Public Property ArchiveFolderName As String = "_Archive"

    ' CAM-optimized DXF output: R12 ASCII, geometry-only, no annotations
    Public Property CamOptimizedOutput As Boolean = True

    ' DXF Preview additions
    Public Property DxfVersionLabel As String = "AutoCAD 2007 DXF"
    Public Property LayerMapMode As LayerMapModes = LayerMapModes.KeepBends
    Public Property UsePreviewOverrides As Boolean = True

    ' Feature Options
    Public Property IncludeBendLines As Boolean = True
    Public Property IncludeEtching As Boolean = True
    Public Property IncludeText As Boolean = True
    Public Property IncludeHoleCenters As Boolean = True
    Public Property GenerateCenterMarks As Boolean = True ' Convert points to cross marks
    Public Property IncludeCornerRelief As Boolean = True
    Public Property IncludeFormFeatures As Boolean = True
    Public Property IncludePunchMarks As Boolean = True

    ' Advanced center mark options
    Public Property IncludeCenterMarksForDrilledHoles As Boolean = True
    Public Property IncludeCenterMarksForThreadedHoles As Boolean = True
    Public Property UseModelForCenterMarks As Boolean = False

    ' Metadata Options  
    Public Property IncludeMetadataBlock As Boolean = True
    Public Property MetadataPosition As MetadataPosition = MetadataPosition.BottomLeft
    Public Property IncludeMaterial As Boolean = True
    Public Property IncludeQuantity As Boolean = True
    Public Property IncludeWeight As Boolean = True
    Public Property IncludeDimensions As Boolean = True
    Public Property IncludeSurfaceArea As Boolean = True
    Public Property IncludeBendCount As Boolean = True
    Public Property IncludeCustomProperties As Boolean = True

    Public Sub New()
        ' Initialize default layer settings
        InitializeDefaultLayers()
    End Sub

    Private Sub InitializeDefaultLayers()
        OuterProfileLayer = New LayerSettings("10_CUT_OUTER", True, System.Drawing.Color.White)
        FeatureProfilesLayer = New LayerSettings("11_CUT_INNER", True, System.Drawing.Color.Yellow)
        BendLinesLayer = New LayerSettings("20_BEND_UP", True, System.Drawing.Color.Green)
        EtchingLayer = New LayerSettings("30_ETCH_TEXT", True, System.Drawing.Color.Cyan)
        TextLayer = New LayerSettings("30_ETCH_TEXT", True, System.Drawing.Color.White)
        HoleCentersLayer = New LayerSettings("40_DRILL", True, System.Drawing.Color.Red)
        TangentLayer = New LayerSettings("22_BEND_TANGENT", False, System.Drawing.Color.Gray)
        CornerReliefLayer = New LayerSettings("12_NO_CUT", True, System.Drawing.Color.Magenta)
        FormFeaturesLayer = New LayerSettings("31_ETCH_MARK", True, System.Drawing.Color.Orange)
        PunchToolMarksLayer = New LayerSettings("31_ETCH_MARK", True, System.Drawing.Color.Red)
    End Sub

    Public Shared Function BuildRunIni(baseIniPath As String, versionLabel As String, layerXmlPath As String) As String
        ' Build a temp INI that forces version + customize options for each run
        Dim iniText As String = System.IO.File.ReadAllText(baseIniPath, System.Text.Encoding.GetEncoding("latin1"))
        Dim lines = iniText.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(New String() {vbLf}, StringSplitOptions.None).ToList()
        Dim mapVersion As String = MapVersionLabelToIni(versionLabel)
        Dim inSelect As Boolean = False
        Dim inDest As Boolean = False
        For i As Integer = 0 To lines.Count - 1
            Dim ln = lines(i).Trim()
            If ln.StartsWith("[EXPORT SELECT OPTIONS]") Then
                inSelect = True : inDest = False
            ElseIf ln.StartsWith("[EXPORT DESTINATION]") Then
                inSelect = False : inDest = True
            End If

            If ln.StartsWith("AUTOCAD VERSION=") Then
                lines(i) = "AUTOCAD VERSION=" & mapVersion
            End If
            If inSelect AndAlso ln.StartsWith("USE CUSTOMIZE=") Then
                lines(i) = "USE CUSTOMIZE=Yes"
            End If
            If inSelect AndAlso ln.StartsWith("CUSTOMIZE FILE=") Then
                lines(i) = "CUSTOMIZE FILE=" & layerXmlPath
            End If
            If inDest AndAlso ln.StartsWith("MODEL GEOMETRY ONLY=") Then
                lines(i) = "MODEL GEOMETRY ONLY=Yes"
            End If
        Next
        Dim tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DXF_PREVIEW_" & Guid.NewGuid().ToString("N") & ".ini")
        System.IO.File.WriteAllText(tempPath, String.Join(vbCrLf, lines), System.Text.Encoding.GetEncoding("latin1"))
        Return tempPath
    End Function

    Private Shared Function MapVersionLabelToIni(label As String) As String
        If String.IsNullOrEmpty(label) Then Return "AutoCAD 2007"
        Dim t = label.Trim()
        If t.Contains("2018") Then Return "AutoCAD 2018"
        If t.Contains("2013") Then Return "AutoCAD 2013"
        If t.Contains("2010") Then Return "AutoCAD 2010"
        If t.Contains("2007") Then Return "AutoCAD 2007"
        If t.Contains("2004") Then Return "AutoCAD 2004"
        If t.Contains("2000") Then Return "AutoCAD 2000"
        If t.ToUpper().Contains("R12") OrElse t.ToUpper().Contains("LT 2") Then Return "AutoCAD R12"
        Return "AutoCAD 2007"
    End Function
End Class

Public Enum LayerMapModes
    KeepBends = 0
    NoBends = 1
End Enum

''' <summary>
''' Layer configuration with name, visibility, and color settings
''' </summary>
Public Class LayerSettings
    Public Property LayerName As String
    Public Property IsEnabled As Boolean
    Public Property LayerColor As System.Drawing.Color
    Public Property CustomColor As Boolean = False

    Public Sub New()
        ' Default constructor
    End Sub

    Public Sub New(layerName As String, isEnabled As Boolean, layerColor As System.Drawing.Color)
        Me.LayerName = layerName
        Me.IsEnabled = isEnabled
        Me.LayerColor = layerColor
    End Sub

    Public ReadOnly Property ColorString As String
        Get
            Return $"{LayerColor.R};{LayerColor.G};{LayerColor.B}"
        End Get
    End Property
End Class

''' <summary>
''' Metadata block positioning options
''' </summary>
Public Enum MetadataPosition
    BottomLeft
    BottomRight
    TopLeft
    TopRight
    SeparateArea
    UserDefined
End Enum

