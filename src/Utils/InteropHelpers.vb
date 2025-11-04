Imports Inventor
Imports System.Drawing

''' <summary>
''' Utility helpers for COM interop and image conversion
''' </summary>
Public NotInheritable Class InteropHelpers
    Private Sub New()
    End Sub

    Private Class AxHostConverter
        Inherits System.Windows.Forms.AxHost
        Public Sub New()
            MyBase.New("")
        End Sub
        Public Shared Function ToImage(pic As Object) As Image
            If pic Is Nothing Then Return Nothing
            Return CType(GetPictureFromIPicture(pic), Image)
        End Function
    End Class

    Public Shared Function IPictureDispToImage(p As Object) As Image
        Return AxHostConverter.ToImage(p)
    End Function
End Class
