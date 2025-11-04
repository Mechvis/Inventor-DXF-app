' ExportOptionsDialog.Designer.vb - WinForms Designer file
Partial Class ExportOptionsDialog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Private Sub InitializeComponent()
        Me.tabControl = New System.Windows.Forms.TabControl()
        Me.tabParts = New System.Windows.Forms.TabPage()
        Me.dgvParts = New System.Windows.Forms.DataGridView()
        Me.tabLayers = New System.Windows.Forms.TabPage()
        Me.grpLayers = New System.Windows.Forms.GroupBox()
        Me.chkOuterProfile = New System.Windows.Forms.CheckBox()
        Me.chkFeatureProfiles = New System.Windows.Forms.CheckBox()
        Me.chkBendLines = New System.Windows.Forms.CheckBox()
        Me.chkEtching = New System.Windows.Forms.CheckBox()
        Me.chkText = New System.Windows.Forms.CheckBox()
        Me.chkHoleCenters = New System.Windows.Forms.CheckBox()
        Me.chkTangentLines = New System.Windows.Forms.CheckBox()
        Me.chkCornerRelief = New System.Windows.Forms.CheckBox()
        Me.chkFormFeatures = New System.Windows.Forms.CheckBox()
        Me.chkPunchMarks = New System.Windows.Forms.CheckBox()
        Me.tabExport = New System.Windows.Forms.TabPage()
        Me.grpExportOptions = New System.Windows.Forms.GroupBox()
        Me.lblExportPath = New System.Windows.Forms.Label()
        Me.txtExportPath = New System.Windows.Forms.TextBox()
        Me.btnBrowsePath = New System.Windows.Forms.Button()
        Me.lblAcadVersion = New System.Windows.Forms.Label()
        Me.cmbAcadVersion = New System.Windows.Forms.ComboBox()
        Me.chkRebaseGeometry = New System.Windows.Forms.CheckBox()
        Me.chkMergeProfiles = New System.Windows.Forms.CheckBox()
        Me.chkEnsureFlatPattern = New System.Windows.Forms.CheckBox()
        Me.tabMetadata = New System.Windows.Forms.TabPage()
        Me.grpMetadata = New System.Windows.Forms.GroupBox()
        Me.chkIncludeMetadata = New System.Windows.Forms.CheckBox()
        Me.chkIncludeMaterial = New System.Windows.Forms.CheckBox()
        Me.chkIncludeQuantity = New System.Windows.Forms.CheckBox()
        Me.chkIncludeWeight = New System.Windows.Forms.CheckBox()
        Me.chkIncludeDimensions = New System.Windows.Forms.CheckBox()
        Me.chkIncludeCustomProps = New System.Windows.Forms.CheckBox()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnPreview = New System.Windows.Forms.Button()

        Me.tabControl.SuspendLayout()
        Me.tabParts.SuspendLayout()
        CType(Me.dgvParts, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabLayers.SuspendLayout()
        Me.grpLayers.SuspendLayout()
        Me.tabExport.SuspendLayout()
        Me.grpExportOptions.SuspendLayout()
        Me.tabMetadata.SuspendLayout()
        Me.grpMetadata.SuspendLayout()
        Me.SuspendLayout()

        '
        'tabControl
        '
        Me.tabControl.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tabControl.Controls.Add(Me.tabParts)
        Me.tabControl.Controls.Add(Me.tabLayers)
        Me.tabControl.Controls.Add(Me.tabExport)
        Me.tabControl.Controls.Add(Me.tabMetadata)
        Me.tabControl.Location = New System.Drawing.Point(12, 12)
        Me.tabControl.Name = "tabControl"
        Me.tabControl.SelectedIndex = 0
        Me.tabControl.Size = New System.Drawing.Size(660, 400)
        Me.tabControl.TabIndex = 0

        '
        'tabParts
        '
        Me.tabParts.Controls.Add(Me.dgvParts)
        Me.tabParts.Location = New System.Drawing.Point(4, 22)
        Me.tabParts.Name = "tabParts"
        Me.tabParts.Padding = New System.Windows.Forms.Padding(3)
        Me.tabParts.Size = New System.Drawing.Size(652, 374)
        Me.tabParts.TabIndex = 0
        Me.tabParts.Text = "Parts Selection"
        Me.tabParts.UseVisualStyleBackColor = True

        '
        'dgvParts
        '
        Me.dgvParts.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvParts.Location = New System.Drawing.Point(6, 6)
        Me.dgvParts.Name = "dgvParts"
        Me.dgvParts.Size = New System.Drawing.Size(640, 362)
        Me.dgvParts.TabIndex = 0

        '
        'tabLayers
        '
        Me.tabLayers.Controls.Add(Me.grpLayers)
        Me.tabLayers.Location = New System.Drawing.Point(4, 22)
        Me.tabLayers.Name = "tabLayers"
        Me.tabLayers.Size = New System.Drawing.Size(652, 374)
        Me.tabLayers.TabIndex = 1
        Me.tabLayers.Text = "Layer Options"
        Me.tabLayers.UseVisualStyleBackColor = True

        '
        'grpLayers
        '
        Me.grpLayers.Controls.Add(Me.chkOuterProfile)
        Me.grpLayers.Controls.Add(Me.chkFeatureProfiles)
        Me.grpLayers.Controls.Add(Me.chkBendLines)
        Me.grpLayers.Controls.Add(Me.chkEtching)
        Me.grpLayers.Controls.Add(Me.chkText)
        Me.grpLayers.Controls.Add(Me.chkHoleCenters)
        Me.grpLayers.Controls.Add(Me.chkTangentLines)
        Me.grpLayers.Controls.Add(Me.chkCornerRelief)
        Me.grpLayers.Controls.Add(Me.chkFormFeatures)
        Me.grpLayers.Controls.Add(Me.chkPunchMarks)
        Me.grpLayers.Location = New System.Drawing.Point(6, 6)
        Me.grpLayers.Name = "grpLayers"
        Me.grpLayers.Size = New System.Drawing.Size(640, 200)
        Me.grpLayers.TabIndex = 0
        Me.grpLayers.TabStop = False
        Me.grpLayers.Text = "Layer Selection"

        ' Layer checkboxes setup
        Me.chkOuterProfile.Text = "Outer Profile (IV_OUTER_PROFILE)"
        Me.chkOuterProfile.Location = New System.Drawing.Point(20, 30)
        Me.chkOuterProfile.Checked = True

        Me.chkFeatureProfiles.Text = "Feature Profiles (IV_FEATURE_PROFILES)"
        Me.chkFeatureProfiles.Location = New System.Drawing.Point(20, 55)
        Me.chkFeatureProfiles.Checked = True

        Me.chkBendLines.Text = "Bend Lines (IV_BEND)"
        Me.chkBendLines.Location = New System.Drawing.Point(20, 80)
        Me.chkBendLines.Checked = True

        Me.chkEtching.Text = "Etching Lines (IV_ETCHING)"
        Me.chkEtching.Location = New System.Drawing.Point(20, 105)
        Me.chkEtching.Checked = True

        Me.chkText.Text = "Text Elements (IV_TEXT)"
        Me.chkText.Location = New System.Drawing.Point(20, 130)
        Me.chkText.Checked = True

        Me.chkHoleCenters.Text = "Hole Centers (IV_TOOL_CENTER)"
        Me.chkHoleCenters.Location = New System.Drawing.Point(320, 30)
        Me.chkHoleCenters.Checked = True

        Me.chkTangentLines.Text = "Tangent Lines (IV_TANGENT)"
        Me.chkTangentLines.Location = New System.Drawing.Point(320, 55)
        Me.chkTangentLines.Checked = False

        Me.chkCornerRelief.Text = "Corner Relief (IV_CORNER_RELIEF)"
        Me.chkCornerRelief.Location = New System.Drawing.Point(320, 80)
        Me.chkCornerRelief.Checked = True

        Me.chkFormFeatures.Text = "Form Features (IV_FORM_FEATURES)"
        Me.chkFormFeatures.Location = New System.Drawing.Point(320, 105)
        Me.chkFormFeatures.Checked = True

        Me.chkPunchMarks.Text = "Punch Tool Marks (IV_PUNCH_MARKS)"
        Me.chkPunchMarks.Location = New System.Drawing.Point(320, 130)
        Me.chkPunchMarks.Checked = True

        '
        'tabExport
        '
        Me.tabExport.Controls.Add(Me.grpExportOptions)
        Me.tabExport.Location = New System.Drawing.Point(4, 22)
        Me.tabExport.Name = "tabExport"
        Me.tabExport.Size = New System.Drawing.Size(652, 374)
        Me.tabExport.TabIndex = 2
        Me.tabExport.Text = "Export Settings"
        Me.tabExport.UseVisualStyleBackColor = True

        '
        'grpExportOptions
        '
        Me.grpExportOptions.Controls.Add(Me.lblExportPath)
        Me.grpExportOptions.Controls.Add(Me.txtExportPath)
        Me.grpExportOptions.Controls.Add(Me.btnBrowsePath)
        Me.grpExportOptions.Controls.Add(Me.lblAcadVersion)
        Me.grpExportOptions.Controls.Add(Me.cmbAcadVersion)
        Me.grpExportOptions.Controls.Add(Me.chkRebaseGeometry)
        Me.grpExportOptions.Controls.Add(Me.chkMergeProfiles)
        Me.grpExportOptions.Controls.Add(Me.chkEnsureFlatPattern)
        Me.grpExportOptions.Location = New System.Drawing.Point(6, 6)
        Me.grpExportOptions.Name = "grpExportOptions"
        Me.grpExportOptions.Size = New System.Drawing.Size(640, 180)
        Me.grpExportOptions.TabIndex = 0
        Me.grpExportOptions.TabStop = False
        Me.grpExportOptions.Text = "Export Configuration"

        ' Export options setup
        Me.lblExportPath.Text = "Export Path:"
        Me.lblExportPath.Location = New System.Drawing.Point(20, 30)
        Me.lblExportPath.Size = New System.Drawing.Size(80, 20)

        Me.txtExportPath.Location = New System.Drawing.Point(100, 27)
        Me.txtExportPath.Size = New System.Drawing.Size(450, 22)

        Me.btnBrowsePath.Text = "Browse..."
        Me.btnBrowsePath.Location = New System.Drawing.Point(560, 25)
        Me.btnBrowsePath.Size = New System.Drawing.Size(70, 25)

        Me.lblAcadVersion.Text = "DXF Version:"
        Me.lblAcadVersion.Location = New System.Drawing.Point(20, 65)
        Me.lblAcadVersion.Size = New System.Drawing.Size(80, 20)

        Me.cmbAcadVersion.Location = New System.Drawing.Point(100, 62)
        Me.cmbAcadVersion.Size = New System.Drawing.Size(120, 22)
        Me.cmbAcadVersion.Items.AddRange(New String() {"2004", "2000", "R14", "R12"})
        Me.cmbAcadVersion.SelectedIndex = 0

        Me.chkRebaseGeometry.Text = "Rebase Geometry to Origin"
        Me.chkRebaseGeometry.Location = New System.Drawing.Point(20, 100)
        Me.chkRebaseGeometry.Size = New System.Drawing.Size(200, 20)
        Me.chkRebaseGeometry.Checked = True

        Me.chkMergeProfiles.Text = "Merge Profiles into Polylines"
        Me.chkMergeProfiles.Location = New System.Drawing.Point(20, 125)
        Me.chkMergeProfiles.Size = New System.Drawing.Size(200, 20)
        Me.chkMergeProfiles.Checked = True

        Me.chkEnsureFlatPattern.Text = "Ensure Flat Pattern before export"
        Me.chkEnsureFlatPattern.Location = New System.Drawing.Point(20, 150)
        Me.chkEnsureFlatPattern.Size = New System.Drawing.Size(260, 20)
        Me.chkEnsureFlatPattern.Checked = True

        '
        'tabMetadata
        '
        Me.tabMetadata.Controls.Add(Me.grpMetadata)
        Me.tabMetadata.Location = New System.Drawing.Point(4, 22)
        Me.tabMetadata.Name = "tabMetadata"
        Me.tabMetadata.Size = New System.Drawing.Size(652, 374)
        Me.tabMetadata.TabIndex = 3
        Me.tabMetadata.Text = "Metadata"
        Me.tabMetadata.UseVisualStyleBackColor = True

        '
        'grpMetadata
        '
        Me.grpMetadata.Controls.Add(Me.chkIncludeMetadata)
        Me.grpMetadata.Controls.Add(Me.chkIncludeMaterial)
        Me.grpMetadata.Controls.Add(Me.chkIncludeQuantity)
        Me.grpMetadata.Controls.Add(Me.chkIncludeWeight)
        Me.grpMetadata.Controls.Add(Me.chkIncludeDimensions)
        Me.grpMetadata.Controls.Add(Me.chkIncludeCustomProps)
        Me.grpMetadata.Location = New System.Drawing.Point(6, 6)
        Me.grpMetadata.Name = "grpMetadata"
        Me.grpMetadata.Size = New System.Drawing.Size(640, 180)
        Me.grpMetadata.TabIndex = 0
        Me.grpMetadata.TabStop = False
        Me.grpMetadata.Text = "Metadata Information Block"

        ' Metadata checkboxes setup
        Me.chkIncludeMetadata.Text = "Include Metadata Block in DXF"
        Me.chkIncludeMetadata.Location = New System.Drawing.Point(20, 30)
        Me.chkIncludeMetadata.Size = New System.Drawing.Size(200, 20)
        Me.chkIncludeMetadata.Checked = True

        Me.chkIncludeMaterial.Text = "Material and Thickness"
        Me.chkIncludeMaterial.Location = New System.Drawing.Point(40, 55)
        Me.chkIncludeMaterial.Size = New System.Drawing.Size(180, 20)
        Me.chkIncludeMaterial.Checked = True

        Me.chkIncludeQuantity.Text = "Quantity"
        Me.chkIncludeQuantity.Location = New System.Drawing.Point(40, 80)
        Me.chkIncludeQuantity.Size = New System.Drawing.Size(180, 20)
        Me.chkIncludeQuantity.Checked = True

        Me.chkIncludeWeight.Text = "Weight"
        Me.chkIncludeWeight.Location = New System.Drawing.Point(240, 55)
        Me.chkIncludeWeight.Size = New System.Drawing.Size(180, 20)
        Me.chkIncludeWeight.Checked = True

        Me.chkIncludeDimensions.Text = "Overall Dimensions"
        Me.chkIncludeDimensions.Location = New System.Drawing.Point(240, 80)
        Me.chkIncludeDimensions.Size = New System.Drawing.Size(180, 20)
        Me.chkIncludeDimensions.Checked = True

        Me.chkIncludeCustomProps.Text = "Custom iProperties"
        Me.chkIncludeCustomProps.Location = New System.Drawing.Point(40, 105)
        Me.chkIncludeCustomProps.Size = New System.Drawing.Size(180, 20)
        Me.chkIncludeCustomProps.Checked = True

        '
        'Buttons
        '
        Me.btnOK.Text = "Export DXF Files"
        Me.btnOK.Location = New System.Drawing.Point(430, 430)
        Me.btnOK.Size = New System.Drawing.Size(120, 30)
        Me.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK

        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.Location = New System.Drawing.Point(560, 430)
        Me.btnCancel.Size = New System.Drawing.Size(80, 30)
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel

        Me.btnPreview.Text = "Preview Settings"
        Me.btnPreview.Location = New System.Drawing.Point(300, 430)
        Me.btnPreview.Size = New System.Drawing.Size(120, 30)

        '
        'ExportOptionsDialog
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(684, 481)
        Me.Controls.Add(Me.btnPreview)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.tabControl)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "ExportOptionsDialog"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Sheet Metal DXF Export Options"

        Me.tabControl.ResumeLayout(False)
        Me.tabParts.ResumeLayout(False)
        CType(Me.dgvParts, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabLayers.ResumeLayout(False)
        Me.grpLayers.ResumeLayout(False)
        Me.tabExport.ResumeLayout(False)
        Me.grpExportOptions.ResumeLayout(False)
        Me.tabMetadata.ResumeLayout(False)
        Me.grpMetadata.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents tabControl As TabControl
    Friend WithEvents tabParts As TabPage
    Friend WithEvents dgvParts As DataGridView
    Friend WithEvents tabLayers As TabPage
    Friend WithEvents grpLayers As GroupBox
    Friend WithEvents chkOuterProfile As CheckBox
    Friend WithEvents chkFeatureProfiles As CheckBox
    Friend WithEvents chkBendLines As CheckBox
    Friend WithEvents chkEtching As CheckBox
    Friend WithEvents chkText As CheckBox
    Friend WithEvents chkHoleCenters As CheckBox
    Friend WithEvents chkTangentLines As CheckBox
    Friend WithEvents chkCornerRelief As CheckBox
    Friend WithEvents chkFormFeatures As CheckBox
    Friend WithEvents chkPunchMarks As CheckBox
    Friend WithEvents tabExport As TabPage
    Friend WithEvents grpExportOptions As GroupBox
    Friend WithEvents lblExportPath As Label
    Friend WithEvents txtExportPath As TextBox
    Friend WithEvents btnBrowsePath As Button
    Friend WithEvents lblAcadVersion As Label
    Friend WithEvents cmbAcadVersion As ComboBox
    Friend WithEvents chkRebaseGeometry As CheckBox
    Friend WithEvents chkMergeProfiles As CheckBox
    Friend WithEvents chkEnsureFlatPattern As CheckBox
    Friend WithEvents tabMetadata As TabPage
    Friend WithEvents grpMetadata As GroupBox
    Friend WithEvents chkIncludeMetadata As CheckBox
    Friend WithEvents chkIncludeMaterial As CheckBox
    Friend WithEvents chkIncludeQuantity As CheckBox
    Friend WithEvents chkIncludeWeight As CheckBox
    Friend WithEvents chkIncludeDimensions As CheckBox
    Friend WithEvents chkIncludeCustomProps As CheckBox
    Friend WithEvents btnOK As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnPreview As Button
End Class