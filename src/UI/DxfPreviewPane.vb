Imports System.IO
Imports System.Text
Imports System.Linq
Imports Inventor

Public Class DxfPreviewPane
    Private _app As Inventor.Application
    Private _settings As ExportSettings
    Private _entities As New List(Of DxfEnt)()
    Private _overrides As New Dictionary(Of Integer, DxfOverride)()
    Private _tempDxf As String

    Public Sub New()
        InitializeComponent()
        _settings = New ExportSettings()
        PopulateUiDefaults()
        AddHandler cboVersion.SelectedIndexChanged, AddressOf OnVersionChanged
        AddHandler btnPreview.Click, AddressOf OnPreview
        AddHandler btnExport.Click, AddressOf OnExport
        AddHandler dg.SelectionChanged, AddressOf OnGridSelection
    End Sub

    Public Sub Initialize(app As Inventor.Application, settings As ExportSettings)
        _app = app
        _settings = settings
        PopulateUiDefaults()
    End Sub

    Private Sub PopulateUiDefaults()
        cboVersion.Items.Clear()
        cboVersion.Items.AddRange(New Object() {
            "AutoCAD 2018 DXF", "AutoCAD 2013 DXF", "AutoCAD 2010 DXF",
            "AutoCAD 2007 DXF", "AutoCAD 2004 DXF", "AutoCAD 2000/AT 2000 DXF", "AutoCAD R12/LT 2 DXF"})
        If String.IsNullOrEmpty(_settings.DxfVersionLabel) Then _settings.DxfVersionLabel = "AutoCAD 2007 DXF"
        cboVersion.SelectedItem = _settings.DxfVersionLabel

        cboLayerMap.Items.Clear()
        cboLayerMap.Items.AddRange(New Object() {"Keep Bends", "No Bends"})
        cboLayerMap.SelectedIndex = If(_settings.LayerMapMode = LayerMapModes.KeepBends, 0, 1)
    End Sub

    Private Sub OnVersionChanged(sender As Object, e As EventArgs)
        lblBanner.Visible = cboVersion.Text.Contains("R12")
    End Sub

    Private Sub OnPreview(sender As Object, e As EventArgs)
        Try
            If _app Is Nothing OrElse _app.ActiveDocument Is Nothing Then
                lblStatus.Text = "No active document"
                Return
            End If

            Dim doc = _app.ActiveDocument
            Dim layerXml = GetLayerMapPath()
            Dim optsXml = GetExportOptsPath()

            Dim ok = DXFExporter.ExportToTempDxfShared(_app, doc, _settings, layerXml, optsXml, _tempDxf)
            If Not ok Then
                lblStatus.Text = "Preview export failed"
                Return
            End If

            _entities = DXFExporter.ParseEntitiesShared(_tempDxf)
            _overrides = _entities.ToDictionary(Function(en) en.Id, Function(en) New DxfOverride() With {.Include = True})
            dg.DataSource = _entities.Select(Function(en) New With {
                .Id = en.Id,
                .Type = en.Type.ToString(),
                .Layer = en.Layer,
                .Closed = en.Closed,
                .BBox = $"{en.BBox.Item1:F3},{en.BBox.Item2:F3} – {en.BBox.Item3:F3},{en.BBox.Item4:F3}"
            }).ToList()
            lblStatus.Text = $"Preview loaded: {_entities.Count} entities"
        Catch ex As Exception
            lblStatus.Text = "Preview error: " & ex.Message
        End Try
    End Sub

    Private Sub OnExport(sender As Object, e As EventArgs)
        Try
            If String.IsNullOrEmpty(_tempDxf) OrElse Not System.IO.File.Exists(_tempDxf) Then
                lblStatus.Text = "No preview DXF to export"
                Return
            End If
            Using sfd As New SaveFileDialog()
                sfd.Filter = "DXF files (*.dxf)|*.dxf"
                sfd.FileName = System.IO.Path.GetFileName(_tempDxf)
                If sfd.ShowDialog() = DialogResult.OK Then
                    Dim r12 = cboVersion.Text.Contains("R12")
                    Dim ok = DXFExporter.ApplyOverridesAndWriteShared(_tempDxf, sfd.FileName, _overrides, r12)
                    If Not ok Then
                        Dim err = DXFExporter.ValidateDxfShared(sfd.FileName, r12)
                        lblStatus.Text = If(String.IsNullOrEmpty(err), "Export failed", err)
                    Else
                        Dim err2 = DXFExporter.ValidateDxfShared(sfd.FileName, r12)
                        If err2 = "OK" Then
                            lblStatus.Text = "Export OK"
                        Else
                            lblStatus.Text = err2
                        End If
                    End If
                End If
            End Using
        Catch ex As Exception
            lblStatus.Text = "Export error: " & ex.Message
        End Try
    End Sub

    Private Sub OnGridSelection(sender As Object, e As EventArgs)
        Try
            If dg.SelectedRows.Count = 0 Then Return
            Dim id As Integer = CInt(dg.SelectedRows(0).Cells(0).Value)
            Dim en = _entities.FirstOrDefault(Function(x) x.Id = id)
            If en IsNot Nothing AndAlso _app IsNot Nothing Then
                DXFExporter.HighlightEntityInViewShared(_app, en)
            End If
        Catch
        End Try
    End Sub

    Private Function GetLayerMapPath() As String
        ' Choose between MechVisionDXF.xml (keep bends) and FaceLoops.xml (no bends)
        Dim base As String = "C:\Users\Public\Documents\Autodesk\Inventor 2026\Design Data\DWG-DXF\"
        If cboLayerMap.SelectedIndex = 0 Then
            Return System.IO.Path.Combine(base, "MechVisionDXF.xml")
        Else
            Return System.IO.Path.Combine(base, "FaceLoops.xml")
        End If
    End Function

    Private Function GetExportOptsPath() As String
        Dim base As String = "C:\Users\Public\Documents\Autodesk\Inventor 2026\Design Data\DWG-DXF\"
        Return System.IO.Path.Combine(base, "FlatPatternExportOpts.xml")
    End Function
End Class
