<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class DxfPreviewPane
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.cboVersion = New System.Windows.Forms.ComboBox()
        Me.cboLayerMap = New System.Windows.Forms.ComboBox()
        Me.btnPreview = New System.Windows.Forms.Button()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.dg = New System.Windows.Forms.DataGridView()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.pnlTop = New System.Windows.Forms.Panel()
        Me.lblBanner = New System.Windows.Forms.Label()
        CType(Me.dg, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlTop.SuspendLayout()
        Me.SuspendLayout()
        '
        'cboVersion
        '
        Me.cboVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboVersion.FormattingEnabled = True
        Me.cboVersion.Location = New System.Drawing.Point(8, 8)
        Me.cboVersion.Name = "cboVersion"
        Me.cboVersion.Size = New System.Drawing.Size(220, 21)
        Me.cboVersion.TabIndex = 0
        '
        'cboLayerMap
        '
        Me.cboLayerMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboLayerMap.FormattingEnabled = True
        Me.cboLayerMap.Location = New System.Drawing.Point(236, 8)
        Me.cboLayerMap.Name = "cboLayerMap"
        Me.cboLayerMap.Size = New System.Drawing.Size(160, 21)
        Me.cboLayerMap.TabIndex = 1
        '
        'btnPreview
        '
        Me.btnPreview.Location = New System.Drawing.Point(404, 7)
        Me.btnPreview.Name = "btnPreview"
        Me.btnPreview.Size = New System.Drawing.Size(75, 23)
        Me.btnPreview.TabIndex = 2
        Me.btnPreview.Text = "Preview"
        Me.btnPreview.UseVisualStyleBackColor = True
        '
        'btnExport
        '
        Me.btnExport.Location = New System.Drawing.Point(486, 7)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(75, 23)
        Me.btnExport.TabIndex = 3
        Me.btnExport.Text = "Export..."
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'dg
        '
        Me.dg.AllowUserToAddRows = False
        Me.dg.AllowUserToDeleteRows = False
        Me.dg.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dg.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dg.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dg.Location = New System.Drawing.Point(8, 64)
        Me.dg.MultiSelect = False
        Me.dg.Name = "dg"
        Me.dg.RowHeadersVisible = False
        Me.dg.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dg.Size = New System.Drawing.Size(760, 368)
        Me.dg.TabIndex = 4
        '
        'lblStatus
        '
        Me.lblStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatus.Location = New System.Drawing.Point(8, 436)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(760, 23)
        Me.lblStatus.TabIndex = 5
        Me.lblStatus.Text = "Ready"
        '
        'pnlTop
        '
        Me.pnlTop.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlTop.Controls.Add(Me.cboVersion)
        Me.pnlTop.Controls.Add(Me.cboLayerMap)
        Me.pnlTop.Controls.Add(Me.btnPreview)
        Me.pnlTop.Controls.Add(Me.btnExport)
        Me.pnlTop.Location = New System.Drawing.Point(8, 8)
        Me.pnlTop.Name = "pnlTop"
        Me.pnlTop.Size = New System.Drawing.Size(760, 36)
        Me.pnlTop.TabIndex = 6
        '
        'lblBanner
        '
        Me.lblBanner.AutoSize = True
        Me.lblBanner.ForeColor = System.Drawing.Color.DarkOrange
        Me.lblBanner.Location = New System.Drawing.Point(8, 48)
        Me.lblBanner.Name = "lblBanner"
        Me.lblBanner.Size = New System.Drawing.Size(240, 13)
        Me.lblBanner.TabIndex = 7
        Me.lblBanner.Text = "R12 mode: LWPOLYLINE will be converted to POLYLINE"
        Me.lblBanner.Visible = False
        '
        'DxfPreviewPane
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.lblBanner)
        Me.Controls.Add(Me.pnlTop)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.dg)
        Me.Name = "DxfPreviewPane"
        Me.Size = New System.Drawing.Size(776, 464)
        CType(Me.dg, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlTop.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents cboVersion As System.Windows.Forms.ComboBox
    Friend WithEvents cboLayerMap As System.Windows.Forms.ComboBox
    Friend WithEvents btnPreview As System.Windows.Forms.Button
    Friend WithEvents btnExport As System.Windows.Forms.Button
    Friend WithEvents dg As System.Windows.Forms.DataGridView
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents pnlTop As System.Windows.Forms.Panel
    Friend WithEvents lblBanner As System.Windows.Forms.Label

End Class
