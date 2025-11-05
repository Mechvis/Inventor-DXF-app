' Models for DXF Preview workflow
Imports System
Imports System.Collections.Generic

Public Enum DxfEntType
    LwPolyline = 0
    Polyline = 1
    Line = 2
    Arc = 3
    Circle = 4
End Enum

Public Class DxfEnt
    Public Property Id As Integer
    Public Property Type As DxfEntType
    Public Property Layer As String
    Public Property Closed As Boolean
    Public Property Points As New List(Of Tuple(Of Double, Double))
    Public Property Center As Tuple(Of Double, Double)
    Public Property Radius As Double
    Public Property StartAng As Double
    Public Property EndAng As Double
    Public Property BBox As Tuple(Of Double, Double, Double, Double)
End Class

Public Class DxfOverride
    Public Property Include As Boolean = True
    Public Property LayerOverride As String = Nothing
End Class
