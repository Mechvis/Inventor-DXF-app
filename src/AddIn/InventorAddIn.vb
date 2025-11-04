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
            
            ' Add to both Part and Assembly ribbon panels
            AddToRibbonPanel(partRibbon, "Sheet Metal DXF Export")
            AddToRibbonPanel(assemblyRibbon, "Sheet Metal DXF Export")
            
        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show("Error creating user interface: " & ex.Message)
        End Try
    End Sub
    
    Private Sub AddToRibbonPanel(ribbon As RibbonTab, panelName As String)
        Try
            Dim panel As RibbonPanel = Nothing
            
            ' Try to find existing panel or create new one
            Try
                panel = ribbon.RibbonPanels(panelName)
            Catch
                panel = ribbon.RibbonPanels.Add(panelName, panelName & "_Panel", "{12345678-1234-1234-1234-123456789ABC}")
            End Try
            
            ' Add the button to the panel
            panel.CommandControls.AddButton(_addInButtonDefinition)
            
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error adding ribbon panel: " & ex.Message)
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