Imports System.IO
Imports System.Text

''' <summary>
''' Post-export DXF sanitizer for CAM: enforces geometry-only ENTITIES and layer mapping.
''' - Removes TEXT/MTEXT/POINT and arc center markers
''' - Maps Inventor IV_* layers to CAM contract layer names
''' - Ensures EOF terminator is present
''' - Heals stray "0/0/TEXT" anomalies by skipping duplicate 0 tokens before entity name
''' </summary>
Public NotInheritable Class DXFSanitizer
    Private Sub New()
    End Sub

    Public Shared Sub Sanitize(dxfPath As String, settings As ExportSettings)
        Try
            Dim lines = File.ReadAllLines(dxfPath, Encoding.GetEncoding("latin1"))
            Dim outSb As New StringBuilder()

            Dim inEntities As Boolean = False
            Dim i As Integer = 0

            Dim AppendPair = Sub(code As String, value As String)
                                 outSb.AppendLine(code)
                                 outSb.AppendLine(value)
                             End Sub

            While i < lines.Length - 1
                Dim code = lines(i).TrimEnd()
                Dim value = lines(i + 1).TrimEnd()

                If code = "0" AndAlso value = "SECTION" Then
                    AppendPair(code, value)
                    i += 2
                    If i < lines.Length - 1 AndAlso lines(i).Trim() = "2" Then
                        Dim secName = lines(i + 1).TrimEnd()
                        AppendPair("2", secName)
                        i += 2
                        inEntities = (secName = "ENTITIES")
                        Continue While
                    End If
                End If

                If code = "0" AndAlso value = "ENDSEC" Then
                    AppendPair(code, value)
                    inEntities = False
                    i += 2
                    Continue While
                End If

                If inEntities AndAlso code = "0" Then
                    ' Heal duplicate 0 token anomalies (0/0/TEXT ...)
                    Dim entityName As String = value
                    If entityName = "0" AndAlso i + 3 < lines.Length AndAlso lines(i + 2).Trim() = "0" Then
                        ' Skip the stray 0 and read the actual entity name
                        i += 2
                        entityName = lines(i + 1).TrimEnd()
                    End If

                    ' Capture entity buffer
                    Dim buf As New List(Of String)()
                    buf.Add("0")
                    buf.Add(entityName)
                    i += 2

                    Dim layerName As String = Nothing
                    While i < lines.Length - 1
                        Dim c = lines(i).TrimEnd()
                        Dim v = lines(i + 1).TrimEnd()
                        If c = "0" Then Exit While
                        If c = "8" Then layerName = v
                        buf.Add(c)
                        buf.Add(v)
                        i += 2
                    End While

                    ' Filter rules
                    Dim drop As Boolean = False
                    Dim entUpper = entityName.ToUpperInvariant()
                    If entUpper = "TEXT" OrElse entUpper = "MTEXT" OrElse entUpper = "POINT" Then
                        drop = True
                    End If
                    Dim layerUpper = If(layerName, String.Empty).ToUpperInvariant()
                    If layerUpper = "IV_ARC_CENTERS" OrElse layerUpper = "IV_TOOL_CENTER" Then
                        drop = True
                    End If

                    If Not drop Then
                        ' Map layer in-place
                        For j As Integer = 0 To buf.Count - 2 Step 2
                            If buf(j) = "8" Then
                                buf(j + 1) = MapLayerName(buf(j + 1), settings)
                            End If
                        Next
                        ' Write back
                        For k As Integer = 0 To buf.Count - 1 Step 2
                            AppendPair(buf(k), buf(k + 1))
                        Next
                    End If

                    Continue While
                End If

                ' Passthrough
                AppendPair(code, value)
                i += 2
            End While

            ' Ensure EOF
            Dim outText = outSb.ToString()
            If Not outText.Contains(vbCrLf & "0" & vbCrLf & "EOF") AndAlso Not outText.EndsWith("0" & Environment.NewLine & "EOF" & Environment.NewLine) Then
                outSb.AppendLine("0")
                outSb.AppendLine("EOF")
            End If

            File.WriteAllText(dxfPath, outSb.ToString(), Encoding.GetEncoding("latin1"))
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("DXF sanitize failed: " & ex.Message)
        End Try
    End Sub

    Private Shared Function MapLayerName(src As String, settings As ExportSettings) As String
        If String.IsNullOrEmpty(src) Then Return src
        Dim s = src.Trim()
        Select Case s.ToUpperInvariant()
            Case "IV_OUTER_PROFILE"
                Return If(settings.OuterProfileLayer?.LayerName, "10_CUT_OUTER")
            Case "IV_INTERIOR_PROFILES"
                Return If(settings.FeatureProfilesLayer?.LayerName, "11_CUT_INNER")
            Case "IV_BEND", "IV_BEND_UP"
                Return If(settings.BendLinesLayer?.LayerName, "20_BEND_UP")
            Case "IV_BEND_DOWN"
                Return "21_BEND_DOWN"
            Case "IV_TANGENT"
                Return If(settings.TangentLayer?.LayerName, "22_BEND_TANGENT")
            Case "IV_TEXT", "IV_ETCHING"
                Return If(settings.TextLayer?.LayerName, "30_ETCH_TEXT")
            Case Else
                Return s
        End Select
    End Function
End Class
