Imports System.Web.Script.Serialization

Public Class clsMessage

    Public Property Header As String
    Public Property Value As String

    Public Function SerializeList() As String

        Dim serializer As New JavaScriptSerializer()
        Dim serializedResult As String = ""

        Try
            ' Set the maximum length for the serializer to avoid errors on bulk data.
            serializer.MaxJsonLength = iSerializerLength

            serializedResult = serializer.Serialize(Me)

        Catch ex As Exception
            MsgBox("clsMessage - SerializeList" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return serializedResult

    End Function

End Class
