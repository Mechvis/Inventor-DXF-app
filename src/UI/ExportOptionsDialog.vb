Imports System.IO
Imports System.Drawing
Imports Inventor

''' <summary>
''' Main export options dialog for configuring DXF export settings
''' Provides tabbed interface for part selection, layer control, and metadata options
''' </summary>
Public Class ExportOptionsDialog

    Private _sheetMetalParts As List(Of SheetMetalPart)
    Private _exportSettings As ExportSettings
    Private _app As Inventor.Application

    ' Preview window host
    Private _previewForm As Form
    Private _previewPane As DxfPreviewPane

    Public ReadOnly Property ExportSettings As ExportSettings
        Get
            Return _exportSettings
        End Get
    End Property

    Public Sub New(app As Inventor.Application, sheetMetalParts As List(Of SheetMetalPart))
        InitializeComponent()

        _app = app
        _sheetMetalParts = sheetMetalParts
        _exportSettings = New ExportSettings()

        ' Apply runtime sizing and autosize for checkboxes (designer-safe)
        For Each chk As CheckBox In grpLayers.Controls.OfType(Of CheckBox)()
            chk.Size = New System.Drawing.Size(280, 20)
            chk.AutoSize = True
        Next

        SetupPartsGrid()
        SetupEventHandlers()
        LoadDefaultSettings()
    End Sub

    ''' <summary>
    ''' Setup the parts selection data grid
    ''' </summary>
    Private Sub SetupPartsGrid()
        dgvParts.AutoGenerateColumns = False
        dgvParts.AllowUserToAddRows = False
        dgvParts.AllowUserToDeleteRows = False
        dgvParts.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvParts.RowTemplate.Height = 56
        dgvParts.RowHeadersVisible = False

        dgvParts.Columns.Clear()

        ' Thumbnail image column
        Dim colThumb As New DataGridViewImageColumn()
        colThumb.HeaderText = "Preview"
        colThumb.Width = 64
        colThumb.ImageLayout = DataGridViewImageCellLayout.Zoom
        dgvParts.Columns.Add(colThumb)

        ' Add columns
        Dim colSelect As New DataGridViewCheckBoxColumn()
        colSelect.HeaderText = "Export"
        colSelect.DataPropertyName = "IsSelected"
        colSelect.Width = 60
        dgvParts.Columns.Add(colSelect)

        Dim colPartName As New DataGridViewTextBoxColumn()
        colPartName.HeaderText = "Part Name"
        colPartName.DataPropertyName = "PartName"
        colPartName.Width = 200
        colPartName.ReadOnly = True
        dgvParts.Columns.Add(colPartName)

        Dim colMaterial As New DataGridViewTextBoxColumn()
        colMaterial.HeaderText = "Material Info"
        colMaterial.DataPropertyName = "MaterialInfo"
        colMaterial.Width = 150
        colMaterial.ReadOnly = True
        dgvParts.Columns.Add(colMaterial)

        Dim colWeight As New DataGridViewTextBoxColumn()
        colWeight.HeaderText = "Weight (kg)"
        colWeight.DataPropertyName = "Weight"
        colWeight.Width = 80
        colWeight.ReadOnly = True
        colWeight.DefaultCellStyle.Format = "F3"
        dgvParts.Columns.Add(colWeight)

        Dim colDimensions As New DataGridViewTextBoxColumn()
        colDimensions.HeaderText = "Size (L x W mm)"
        colDimensions.Width = 120
        colDimensions.ReadOnly = True
        colDimensions.Name = "Size (L x W mm)"
        dgvParts.Columns.Add(colDimensions)

        Dim colFlatPattern As New DataGridViewCheckBoxColumn()
        colFlatPattern.HeaderText = "Has Flat Pattern"
        colFlatPattern.DataPropertyName = "HasFlatPattern"
        colFlatPattern.Width = 100
        colFlatPattern.ReadOnly = True
        dgvParts.Columns.Add(colFlatPattern)

        ' Per-item preview button
        Dim colPreview As New DataGridViewButtonColumn()
        colPreview.HeaderText = "DXF"
        colPreview.Text = "Preview"
        colPreview.UseColumnTextForButtonValue = True
        colPreview.Width = 80
        dgvParts.Columns.Add(colPreview)

        ' Bind data (without images)
        dgvParts.DataSource = _sheetMetalParts

        ' Populate thumbnails and dimensions
        For i As Integer = 0 To _sheetMetalParts.Count - 1
            Dim part = _sheetMetalParts(i)
            ' Dimensions column
            dgvParts.Rows(i).Cells("Size (L x W mm)").Value = $"{part.BoundingBoxLength:F1} x {part.BoundingBoxWidth:F1}"
            ' Thumbnail using late-binding to avoid stdole reference
            Try
                Dim thumbObj As Object = Microsoft.VisualBasic.CallByName(part.Document, "Thumbnail", CallType.Get)
                If thumbObj IsNot Nothing Then
                    Dim img As Image = InteropHelpers.IPictureDispToImage(thumbObj)
                    If img IsNot Nothing Then
                        dgvParts.Rows(i).Cells(0).Value = img
                    End If
                End If
            Catch ex As Exception
                ' Log thumbnail failure but continue
                System.Diagnostics.Debug.WriteLine($"Failed to load thumbnail for {part.PartName}: {ex.Message}")
            End Try
        Next
    End Sub

    ''' <summary>
    ''' Setup event handlers for UI controls
    ''' </summary>
    Private Sub SetupEventHandlers()
        AddHandler btnBrowsePath.Click, AddressOf BtnBrowsePath_Click
        AddHandler btnPreview.Click, AddressOf BtnPreview_Click
        AddHandler btnOK.Click, AddressOf BtnOK_Click
        AddHandler chkIncludeMetadata.CheckedChanged, AddressOf ChkIncludeMetadata_CheckedChanged

        ' Layer checkbox handlers to update settings
        AddHandler chkOuterProfile.CheckedChanged, AddressOf LayerCheckBox_CheckedChanged
        AddHandler chkFeatureProfiles.CheckedChanged, AddressOf LayerCheckBox_CheckedChanged
        AddHandler chkBendLines.CheckedChanged, AddressOf LayerCheckBox_CheckedChanged
        AddHandler chkEtching.CheckedChanged, AddressOf LayerCheckBox_CheckedChanged
        AddHandler chkText.CheckedChanged, AddressOf LayerCheckBox_CheckedChanged
        AddHandler chkHoleCenters.CheckedChanged, AddressOf LayerCheckBox_CheckedChanged
        AddHandler chkTangentLines.CheckedChanged, AddressOf LayerCheckBox_CheckedChanged
        AddHandler chkCornerRelief.CheckedChanged, AddressOf LayerCheckBox_CheckedChanged
        AddHandler chkFormFeatures.CheckedChanged, AddressOf LayerCheckBox_CheckedChanged
        AddHandler chkPunchMarks.CheckedChanged, AddressOf LayerCheckBox_CheckedChanged

        ' Grid preview actions
        AddHandler dgvParts.CellContentClick, AddressOf DgvParts_CellContentClick
        AddHandler dgvParts.CellDoubleClick, AddressOf DgvParts_CellDoubleClick
    End Sub

    Private Sub DgvParts_CellContentClick(sender As Object, e As DataGridViewCellEventArgs)
        Try
            If e.RowIndex < 0 Then Return
            Dim isPreviewButton As Boolean = TypeOf dgvParts.Columns(e.ColumnIndex) Is DataGridViewButtonColumn
            If isPreviewButton Then
                Dim part = TryCast(dgvParts.Rows(e.RowIndex).DataBoundItem, SheetMetalPart)
                If part IsNot Nothing Then
                    PreviewPart(part)
                End If
            End If
        Catch
        End Try
    End Sub

    Private Sub DgvParts_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        Try
            If e.RowIndex < 0 Then Return
            Dim part = TryCast(dgvParts.Rows(e.RowIndex).DataBoundItem, SheetMetalPart)
            If part IsNot Nothing Then
                PreviewPart(part)
            End If
        Catch
        End Try
    End Sub

    Private Sub EnsurePreviewWindow()
        If _previewForm IsNot Nothing AndAlso Not _previewForm.IsDisposed Then Return
        _previewForm = New Form()
        _previewForm.Text = "DXF Preview"
        _previewForm.StartPosition = FormStartPosition.CenterScreen
        _previewForm.Size = New Size(900, 700)
        _previewForm.ShowInTaskbar = False
        _previewForm.TopMost = False

        _previewPane = New DxfPreviewPane()
        _previewPane.Dock = DockStyle.Fill
        _previewPane.Initialize(_app, _exportSettings)
        _previewForm.Controls.Add(_previewPane)
    End Sub

    Private Sub PreviewPart(part As SheetMetalPart)
        UpdateExportSettings()
        EnsurePreviewWindow()
        Try
            _previewPane.Initialize(_app, _exportSettings)
            _previewPane.ShowPreviewFor(part.Document)
            If Not _previewForm.Visible Then
                _previewForm.Show(Me)
            Else
                _previewForm.Activate()
            End If
        Catch ex As Exception
            MessageBox.Show($"Preview failed: {ex.Message}", "DXF Preview", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Load default settings into UI controls
    ''' </summary>
    Private Sub LoadDefaultSettings()
        ' Set default export path to user's Documents folder
        txtExportPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DXF Export")

        ' Populate DXF versions with only supported values
        If cmbAcadVersion IsNot Nothing Then
            Try
                cmbAcadVersion.Items.Clear()
                cmbAcadVersion.Items.AddRange(New Object() {"2018", "2013", "2010", "2007", "2004", "2000", "R12"})
                If cmbAcadVersion.SelectedItem Is Nothing Then
                    cmbAcadVersion.SelectedItem = "2004"
                End If
            Catch
                ' ignore if designer has not created the combo yet
            End Try
        End If

        ' Enable metadata sub-options based on main checkbox
        ChkIncludeMetadata_CheckedChanged(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Browse for export folder
    ''' </summary>
    Private Sub BtnBrowsePath_Click(sender As Object, e As EventArgs)
        Using folderDialog As New FolderBrowserDialog()
            folderDialog.Description = "Select DXF Export Folder"
            folderDialog.SelectedPath = txtExportPath.Text

            If folderDialog.ShowDialog() = DialogResult.OK Then
                txtExportPath.Text = folderDialog.SelectedPath
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Preview export settings
    ''' </summary>
    Private Sub BtnPreview_Click(sender As Object, e As EventArgs)
        UpdateExportSettings()

        Dim preview As New System.Text.StringBuilder()
        preview.AppendLine("DXF Export Settings Preview:")
        preview.AppendLine("=" & New [String]("="c, 30))
        preview.AppendLine()

        ' Parts to export
        Dim selectedParts = _sheetMetalParts.Where(Function(p) p.IsSelected).ToList()
        preview.AppendLine($"Parts to Export: {selectedParts.Count}")
        For Each part In selectedParts
            preview.AppendLine($"  • {part.PartName}")
        Next
        preview.AppendLine()

        ' Export settings
        preview.AppendLine($"Export Path: {_exportSettings.ExportPath}")
        preview.AppendLine($"DXF Version: {_exportSettings.AcadVersion}")
        preview.AppendLine($"Rebase Geometry: {_exportSettings.RebaseGeometry}")
        preview.AppendLine($"Merge Profiles: {_exportSettings.MergeProfilesIntoPolyline}")
        preview.AppendLine()

        ' Layer settings
        preview.AppendLine("Enabled Layers:")
        If _exportSettings.OuterProfileLayer.IsEnabled Then preview.AppendLine($"  • {_exportSettings.OuterProfileLayer.LayerName}")
        If _exportSettings.FeatureProfilesLayer.IsEnabled Then preview.AppendLine($"  • {_exportSettings.FeatureProfilesLayer.LayerName}")
        If _exportSettings.BendLinesLayer.IsEnabled Then preview.AppendLine($"  • {_exportSettings.BendLinesLayer.LayerName}")
        If _exportSettings.EtchingLayer.IsEnabled Then preview.AppendLine($"  • {_exportSettings.EtchingLayer.LayerName}")
        If _exportSettings.TextLayer.IsEnabled Then preview.AppendLine($"  • {_exportSettings.TextLayer.LayerName}")
        If _exportSettings.HoleCentersLayer.IsEnabled Then preview.AppendLine($"  • {_exportSettings.HoleCentersLayer.LayerName}")
        preview.AppendLine()

        ' Metadata settings
        If _exportSettings.IncludeMetadataBlock Then
            preview.AppendLine("Metadata Block: Enabled")
            If _exportSettings.IncludeMaterial Then preview.AppendLine("  • Material and Thickness")
            If _exportSettings.IncludeQuantity Then preview.AppendLine("  • Quantity")
            If _exportSettings.IncludeWeight Then preview.AppendLine("  • Weight")
            If _exportSettings.IncludeDimensions Then preview.AppendLine("  • Dimensions")
            If _exportSettings.IncludeCustomProperties Then preview.AppendLine("  • Custom iProperties")
        Else
            preview.AppendLine("Metadata Block: Disabled")
        End If

        MessageBox.Show(preview.ToString(), "Export Settings Preview", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ''' <summary>
    ''' Handle OK button click - validate and prepare for export
    ''' </summary>
    Private Sub BtnOK_Click(sender As Object, e As EventArgs)
        ' Validate selections
        Dim selectedParts = _sheetMetalParts.Where(Function(p) p.IsSelected).ToList()
        If selectedParts.Count = 0 Then
            MessageBox.Show("Please select at least one part to export.", "No Parts Selected",
                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Validate export path
        If String.IsNullOrWhiteSpace(txtExportPath.Text) Then
            MessageBox.Show("Please specify an export path.", "Export Path Required",
                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Create export directory if it doesn't exist
        Try
            If Not Directory.Exists(txtExportPath.Text) Then
                Directory.CreateDirectory(txtExportPath.Text)
            End If
        Catch ex As Exception
            MessageBox.Show($"Could not create export directory: {ex.Message}", "Directory Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        ' Update settings before closing
        UpdateExportSettings()

        ' Dialog will close with OK result
    End Sub

    ''' <summary>
    ''' Update export settings object from UI controls
    ''' </summary>
    Private Sub UpdateExportSettings()
        ' Basic export settings
        _exportSettings.ExportPath = txtExportPath.Text
        Dim selectedVersion As Object = If(cmbAcadVersion.SelectedItem, Nothing)
        _exportSettings.AcadVersion = If(selectedVersion IsNot Nothing, selectedVersion.ToString(), "2004")
        _exportSettings.RebaseGeometry = chkRebaseGeometry.Checked
        _exportSettings.MergeProfilesIntoPolyline = chkMergeProfiles.Checked
        _exportSettings.EnsureFlatPatternBeforeExport = chkEnsureFlatPattern.Checked

        ' Layer settings
        _exportSettings.OuterProfileLayer.IsEnabled = chkOuterProfile.Checked
        _exportSettings.FeatureProfilesLayer.IsEnabled = chkFeatureProfiles.Checked
        _exportSettings.BendLinesLayer.IsEnabled = chkBendLines.Checked
        _exportSettings.EtchingLayer.IsEnabled = chkEtching.Checked
        _exportSettings.TextLayer.IsEnabled = chkText.Checked
        _exportSettings.HoleCentersLayer.IsEnabled = chkHoleCenters.Checked
        _exportSettings.TangentLayer.IsEnabled = chkTangentLines.Checked
        _exportSettings.CornerReliefLayer.IsEnabled = chkCornerRelief.Checked
        _exportSettings.FormFeaturesLayer.IsEnabled = chkFormFeatures.Checked
        _exportSettings.PunchToolMarksLayer.IsEnabled = chkPunchMarks.Checked

        ' Feature inclusion settings
        _exportSettings.IncludeBendLines = chkBendLines.Checked
        _exportSettings.IncludeEtching = chkEtching.Checked
        _exportSettings.IncludeText = chkText.Checked
        _exportSettings.IncludeHoleCenters = chkHoleCenters.Checked
        _exportSettings.IncludeCornerRelief = chkCornerRelief.Checked
        _exportSettings.IncludeFormFeatures = chkFormFeatures.Checked
        _exportSettings.IncludePunchMarks = chkPunchMarks.Checked

        ' Metadata settings
        _exportSettings.IncludeMetadataBlock = chkIncludeMetadata.Checked
        _exportSettings.IncludeMaterial = chkIncludeMaterial.Checked
        _exportSettings.IncludeQuantity = chkIncludeQuantity.Checked
        _exportSettings.IncludeWeight = chkIncludeWeight.Checked
        _exportSettings.IncludeDimensions = chkIncludeDimensions.Checked
        _exportSettings.IncludeCustomProperties = chkIncludeCustomProps.Checked

        ' Always generate center marks when hole centers are enabled
        _exportSettings.GenerateCenterMarks = chkHoleCenters.Checked
    End Sub

    ''' <summary>
    ''' Handle metadata checkbox change - enable/disable sub-options
    ''' </summary>
    Private Sub ChkIncludeMetadata_CheckedChanged(sender As Object, e As EventArgs)
        Dim enabled = chkIncludeMetadata.Checked
        chkIncludeMaterial.Enabled = enabled
        chkIncludeQuantity.Enabled = enabled
        chkIncludeWeight.Enabled = enabled
        chkIncludeDimensions.Enabled = enabled
        chkIncludeCustomProps.Enabled = enabled
    End Sub

    ''' <summary>
    ''' Handle layer checkbox changes for immediate feedback
    ''' </summary>
    Private Sub LayerCheckBox_CheckedChanged(sender As Object, e As EventArgs)
        ' Could add immediate preview or validation here
        ' For now, just ensure settings are updated when OK is clicked
    End Sub
End Class
