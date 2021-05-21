Imports System.Web.Script.Serialization

Public Class clsSerializer

    Inherits ArrayList

    Public Sub AddToList(ByVal thisObject As Object)

        Try
            Me.Add(thisObject)

        Catch ex As Exception
            MsgBox("clsSerializer - AddToList" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

    End Sub

    Public Function SerializeList() As String

        Dim serializer As New JavaScriptSerializer()
        Dim serializedResult As String = ""

        Try
            ' Set the maximum length for the serializer to avoid errors on bulk data.
            serializer.MaxJsonLength = iSerializerLength

            serializedResult = serializer.Serialize(Me)

        Catch ex As Exception
            MsgBox("clsSerializer - SerializeList" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return serializedResult

    End Function

    Public Function DeserializeList(ByVal sJsonString As String) As clsMessageList

        Dim serializer As New JavaScriptSerializer()
        Dim messageList As New clsMessageList

        Try
            Dim headerList = serializer.Deserialize(Of List(Of clsMessage))(sJsonString)

            messageList.AddRange(headerList)

        Catch ex As Exception
            MsgBox("clsSerializer - DeserializeList" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return messageList

    End Function

End Class
