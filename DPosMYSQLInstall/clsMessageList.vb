Imports System.Web.Script.Serialization

Public Class clsMessageList

    Inherits ArrayList

    ''' <summary>
    ''' Adds an item to the array list.
    ''' </summary>
    ''' <param name="sHeader">The header of the item. This acts like the key in a key/value pair.</param>
    ''' <param name="sValue">The value for the header.</param>
    Public Sub AddToList(ByVal sHeader As String, ByVal sValue As String)

        Try
            Dim messageObject As New clsMessage
            messageObject.Header = sHeader
            messageObject.Value = sValue
            Me.Add(messageObject)

        Catch ex As Exception
            MsgBox("clsMessageList - AddToList" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try
       
    End Sub

    ''' <summary>
    ''' Serializes the arraylist to a multi-dimensional arraylist.
    ''' </summary>
    Public Function SerializeList() As String

        Dim serializer As New JavaScriptSerializer()
        Dim serializedResult As String = ""

        Try
            ' Set the maximum length for the serializer to avoid errors on bulk data.
            serializer.MaxJsonLength = iSerializerLength

            serializedResult = serializer.Serialize(Me)

        Catch ex As Exception
            MsgBox("clsMessageList - SerializeList" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return serializedResult

    End Function

    ''' <summary>
    ''' Deserializes a json string to an arraylist of header/value items.
    ''' </summary>
    ''' <param name="sJsonString">The json string to be deserialized.</param>
    ''' <returns>an arraylist of items cotaining the deserialized json represented by items of format header-value.</returns>
    Public Function DeserializeList(ByVal sJsonString As String) As clsMessageList

        Dim serializer As New JavaScriptSerializer()
        Dim messageList As New clsMessageList

        Try
            Dim headerList = serializer.Deserialize(Of List(Of clsMessage))(sJsonString)

            messageList.AddRange(headerList)

        Catch ex As Exception
            MsgBox("clsMessageList - DeserializeList" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return messageList

    End Function

    ''' <summary>
    ''' Finds a value from the json string and returns true if the value is located.
    ''' </summary>
    ''' <param name="sHeader">The header that needs to be located.</param>
    ''' <param name="sMessage">The value to be compared with the value within the json string.</param>
    ''' <param name="sResponse">The json string to be examined.</param>
    ''' <returns>true if the value is located within the json string.</returns>
    Public Function FindValueAndCompare(ByVal sHeader As String, ByVal sMessage As String, ByVal sResponse As String) As Boolean

        Dim messageArray As New ArrayList
        Dim message As New clsMessage
        Dim bEqual As Boolean = False

        Try
            If IsJSON(sResponse) = True Then
                ' Valid json.

                ' Deserialize the list.
                messageArray = Me.DeserializeList(sResponse)

                For Each message In messageArray
                    ' Check if the value we are looking for is present within the response.
                    If message.Header = sHeader And message.Value = sMessage Then
                        bEqual = True
                        Exit For
                    End If
                Next

            End If

        Catch ex As Exception
            MsgBox("clsMessageList - FindValueAndCompare" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            bEqual = False
        End Try

        Return bEqual

    End Function

    ''' <summary>
    ''' Finds the first header from a json string and that does not contain the exact value passed in.
    ''' </summary>
    ''' <param name="sHeader">The header to be located in the arraylist.</param>
    ''' <param name="sValue">The value that for comparison.</param>
    ''' <returns>the value of the header that we need.</returns>
    Public Function FindValueNotEqualTo(ByVal sHeader As String, ByVal sValue As String) As String

        Dim sMessageValue As String = ""
        Dim message As New clsMessage

        Try
            ' Find the value that we are looking for from the message list.
            For Each message In Me
                ' This is the value that we are looking for.
                If message.Header = sHeader And message.Value <> sValue Then
                    sMessageValue = message.Value
                    Exit For
                End If
            Next

        Catch ex As Exception
            MsgBox("clsMessageList - FindValueNotEqualTo" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            sMessageValue = ""
        End Try

        Return sMessageValue

    End Function

    ''' <summary>
    ''' Finds a header from a json string and returns the value.
    ''' </summary>
    ''' <param name="sHeader">The header to be located in the arraylist.</param>
    ''' <returns>the value of the header that we need.</returns>
    Public Function FindValue(ByVal sHeader As String) As String

        Dim sValue As String = ""
        Dim message As New clsMessage

        Try
            ' Find the value that we are looking for from the message list.
            For Each message In Me
                ' This is the value that we are looking for.
                If message.Header = sHeader Then
                    sValue = message.Value
                    Exit For
                End If
            Next

        Catch ex As Exception
            MsgBox("clsMessageList - FindValue" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            sValue = ""
        End Try

        Return sValue

    End Function

    ''' <summary>
    ''' Finds a value from a json string and returns the value if located.
    ''' </summary>
    ''' <param name="sHeader">The header that needs to be located.</param>
    ''' <param name="sResponse">The json string to be examined.</param>
    ''' <returns>the value of the message from the json string.</returns>
    Public Function FindValueFromResponse(ByVal sHeader As String, ByVal sResponse As String) As String

        Dim messageArray As New ArrayList
        Dim message As New clsMessage
        Dim sValue As String = ""

        Try
            If IsJSON(sResponse) = True Then
                ' Valid json.

                ' Deserialize the list.
                messageArray = Me.DeserializeList(sResponse)

                For Each message In messageArray
                    ' Check if this is the value that we are looking for.
                    If message.Header = sHeader Then
                        sValue = message.Value
                        Exit For
                    End If
                Next

            End If

        Catch ex As Exception
            MsgBox("clsMessageList - FindValueFromResponse" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            sValue = ""
        End Try

        Return sValue

    End Function

End Class
