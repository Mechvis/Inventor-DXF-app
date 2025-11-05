Imports System
Imports System.Runtime.InteropServices
Imports Inventor

''' <summary>
''' Main Inventor COM Add-in entry point for Sheet Metal DXF Export
''' Provides integration with Inventor 2026 for sheet metal flat pattern export
''' </summary>
<GuidAttribute("12345678-1234-1234-1234-123456789ABC")>
<ProgIdAttribute("SheetMetalDXFExporter.InventorAddIn")>
<ComVisibleAttribute(True)>
Public Class InventorAddIn
    Implements ApplicationAddInServer
    
    Private _inventorApplication As Inventor.Application
    Private WithEvents _addInButtonDefinition As ButtonDefinition
    Private _exportCommand As DXFExportCommand
    Private _previewWindow As DockableWindow
    Private _previewPane As DxfPreviewPane

    Public Sub Activate(ByVal addInSiteObject As ApplicationAddInSite, ByVal firstTime As Boolean) _
        Implements ApplicationAddInServer.Activate

        ' Store reference to Inventor Application
        _inventorApplication = addInSiteObject.Application

        If firstTime Then
            ' Create the ribbon interface and commands
            CreateUserInterface()
        End If

        ' Initialize the export command handler
        _exportCommand = New DXFExportCommand(_inventorApplication)

        System.Diagnostics.Debug.WriteLine("Sheet Metal DXF Exporter Add-in activated successfully")
    End Sub

    Public Sub Deactivate() _
        Implements ApplicationAddInServer.Deactivate

        ' Clean up resources
        If _exportCommand IsNot Nothing Then
            _exportCommand.Dispose()
            _exportCommand = Nothing
        End If

        _inventorApplication = Nothing

        System.Diagnostics.Debug.WriteLine("Sheet Metal DXF Exporter Add-in deactivated")
    End Sub

    Public ReadOnly Property Automation() As Object Implements ApplicationAddInServer.Automation
        Get
            Return Nothing
        End Get
    End Property

    Public Sub ExecuteCommand(ByVal CommandID As Integer) Implements ApplicationAddInServer.ExecuteCommand
        ' Handle command execution (legacy)
        Select Case CommandID
            Case 1 ' Export DXF Command
                If _exportCommand Is Nothing Then _exportCommand = New DXFExportCommand(_inventorApplication)
                _exportCommand.Execute()
        End Select
    End Sub

    Private Sub CreateUserInterface()
        Try
            ' Get the Part and Assembly environment ribbon tabs
            Dim partRibbon As RibbonTab = _inventorApplication.UserInterfaceManager.Ribbons("Part").RibbonTabs("id_TabTools")
            Dim assemblyRibbon As RibbonTab = _inventorApplication.UserInterfaceManager.Ribbons("Assembly").RibbonTabs("id_TabTools")

            ' Create button definition
            Dim controlDefs As ControlDefinitions = _inventorApplication.CommandManager.ControlDefinitions
            _addInButtonDefinition = controlDefs.AddButtonDefinition(
                "Export Sheet Metal DXF",
                "SheetMetalDXFExport",
                CommandTypesEnum.kShapeEditCmdType,
                "{12345678-1234-1234-1234-123456789ABC}",
                "Export sheet metal flat patterns to DXF with layer options",
                "Export sheet metal parts to DXF with configurable layers and features",
                Nothing,
                Nothing)

            ' Add export button to both Part and Assembly ribbon panels
            Dim panelPart = AddOrGetRibbonPanel(partRibbon, "Sheet Metal DXF Export")
            panelPart.CommandControls.AddButton(_addInButtonDefinition)
            Dim panelAsm = AddOrGetRibbonPanel(assemblyRibbon, "Sheet Metal DXF Export")
            panelAsm.CommandControls.AddButton(_addInButtonDefinition)

            ' Add a second button for Preview pane
            Dim btnPreview = controlDefs.AddButtonDefinition(
                "DXF Preview",
                "SheetMetalDXFPreview",
                CommandTypesEnum.kShapeEditCmdType,
                "{12345678-1234-1234-1234-123456789ABC}",
                "Preview and validate DXF before export",
                "Open the DXF preview pane",
                Nothing,
                Nothing)

            AddHandler btnPreview.OnExecute, Sub(ctx)
                                                 ShowPreviewPane()
                                             End Sub

            ' Add preview button to same panels
            panelPart.CommandControls.AddButton(btnPreview)
            panelAsm.CommandControls.AddButton(btnPreview)
        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show("Error creating user interface: " & ex.Message)
        End Try
    End Sub

    Private Function AddOrGetRibbonPanel(ribbon As RibbonTab, panelName As String) As RibbonPanel
        Dim panel As RibbonPanel = Nothing
        Try
            Try
                panel = ribbon.RibbonPanels(panelName)
            Catch
                panel = ribbon.RibbonPanels.Add(panelName, panelName & "_Panel", "{12345678-1234-1234-1234-123456789ABC}")
            End Try
        Catch
        End Try
        Return panel
    End Function

    Public Sub ShowPreviewPane()
        Try
            Dim uiMgr = _inventorApplication.UserInterfaceManager
            If _previewWindow Is Nothing Then
                _previewWindow = uiMgr.DockableWindows.Add(
                    "{12345678-1234-1234-1234-123456789ABC}",
                    "DXFPreviewPane",
                    "DXF Preview")
                _previewPane = New DxfPreviewPane()
                _previewPane.Initialize(_inventorApplication, New ExportSettings())
                _previewWindow.AddChild(_previewPane.Handle)
                _previewWindow.DockingState = DockingStateEnum.kDockLeft
                _previewWindow.ShowVisibilityCheckBox = True
            End If
            _previewWindow.Visible = True
        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show("Failed to show preview pane: " & ex.Message)
        End Try
    End Sub

    ' Button click handler
    Private Sub _addInButtonDefinition_OnExecute(Context As NameValueMap) Handles _addInButtonDefinition.OnExecute
        Try
            If _exportCommand Is Nothing Then _exportCommand = New DXFExportCommand(_inventorApplication)
            _exportCommand.Execute()
        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show("Error executing export: " & ex.Message)
        End Try
    End Sub
End Class
