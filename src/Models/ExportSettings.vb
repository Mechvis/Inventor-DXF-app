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
    Public Property AcadVersion As String = "2004"
    Public Property RebaseGeometry As Boolean = True
    Public Property MergeProfilesIntoPolyline As Boolean = True
    Public Property ExportPath As String = ""
    Public Property CustomFileNaming As Boolean = True
    Public Property FileNameTemplate As String = "{PartName}_{Thickness}mm_{Rev}_{Date}"
    ' New: ensure flat pattern exists before export
    Public Property EnsureFlatPatternBeforeExport As Boolean = True
    
    ' Feature Options
    Public Property IncludeBendLines As Boolean = True
    Public Property IncludeEtching As Boolean = True
    Public Property IncludeText As Boolean = True
    Public Property IncludeHoleCenters As Boolean = True
    Public Property GenerateCenterMarks As Boolean = True ' Convert points to cross marks
    Public Property IncludeCornerRelief As Boolean = True
    Public Property IncludeFormFeatures As Boolean = True
    Public Property IncludePunchMarks As Boolean = True
    
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
        OuterProfileLayer = New LayerSettings("IV_OUTER_PROFILE", True, System.Drawing.Color.White)
        FeatureProfilesLayer = New LayerSettings("IV_FEATURE_PROFILES", True, System.Drawing.Color.Yellow)
        BendLinesLayer = New LayerSettings("IV_BEND", True, System.Drawing.Color.Green)
        EtchingLayer = New LayerSettings("IV_ETCHING", True, System.Drawing.Color.Cyan)
        TextLayer = New LayerSettings("IV_TEXT", True, System.Drawing.Color.White)
        HoleCentersLayer = New LayerSettings("IV_TOOL_CENTER", True, System.Drawing.Color.Red)
        TangentLayer = New LayerSettings("IV_TANGENT", False, System.Drawing.Color.Gray)
        CornerReliefLayer = New LayerSettings("IV_CORNER_RELIEF", True, System.Drawing.Color.Magenta)
        FormFeaturesLayer = New LayerSettings("IV_FORM_FEATURES", True, System.Drawing.Color.Orange)
        PunchToolMarksLayer = New LayerSettings("IV_PUNCH_MARKS", True, System.Drawing.Color.Red)
    End Sub
End Class

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