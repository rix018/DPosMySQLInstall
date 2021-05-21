Imports System.IO
Imports System.Threading

Public Class clsRestore
    Public Property Restore As String                       ' The restore type. Can be All, DateRange, LastVerifiedDate or Latest.
    Public Property StartDate As String = ""
    Public Property EndDate As String = ""
    Public Property LastVerifiedDate As String = ""
    Public Property ZipFileName As String
    Public Property CsvDirectory As String
    Public Property ExtractedFolderName As String
    Public Property CsvDirectoryContents As New ArrayList
    Public Property UserName As String
    Public Property Password As String
    Public Property MessageList As New clsMessageList

    Private Property ConflictInData As Boolean = False      ' Boolean for indicating that a conflict is present with the data.
    Private Property LastInTime As String = ""              ' The last InTime in Dpos.
    Private Property LastOrderId As String = ""             ' The last OrderID in Dpos.
    Private Property LastOrderDate As String = ""           ' The last OrderDate in Dpos.
    Private Property LastCloudOrderID As String = ""        ' The last OrderID in the cloud.
    Private Property LastCustomerID As String = ""          ' The last CustomerID in Dpos.
    Private Property LastDateJoined As String = ""          ' The last DateJoined in Dpos.
    Private Property LastCloudCustomerID As String = ""     ' The last CustomerID in the cloud.
    Private Property LastCloudDateJoined As String = ""     ' The last DateJoined in the cloud.
    Private Property TopOrderID As String = ""              ' The top OrderID for determining the point of conflict in tblOrderHeaders.
    Private Property FirstCustomerID As String = ""
    Private Property FirstDateJoined As String = ""

    Private Property ConflictingTables As New ArrayList

    Public Sub New()

        CsvDirectory = sDatabaseLocation & "\CloudRestore\"

    End Sub

    ''' <summary>
    ''' Browse the files of the extracted zip file. This function will fill the property Me.CsvDirectoryContents.
    ''' This will only filter all .csv extension files.
    ''' </summary>
    ''' <param name="sExtractedFolderPath">The path where the extracted zip file is located.</param>
    Public Sub ProcessExtractedZIPFile(ByVal sExtractedFolderPath As String)

        Try
            Dim directory As New IO.DirectoryInfo(sExtractedFolderPath)
            Dim directoryContents As IO.FileInfo() = directory.GetFiles()
            Dim item As IO.FileInfo

            ' List the names of all files in the specified directory.
            For Each item In directoryContents
                If item.Extension = ".csv" Then
                    Me.CsvDirectoryContents.Add(item.Name.Replace(item.Extension, ""))
                End If
            Next
        Catch ex As Exception
            myMsgBox("clsRestore - ProcessExtractedZIPFile" & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

    End Sub

    Public Function ExtractZIP(ByVal sZIPPath As String, ByVal sExtractPath As String) As Boolean

        Dim bOK As Boolean = True
        Dim shObj As Object = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"))

        Try
            ' Check if the file exists.
            If (File.Exists(sZIPPath)) Then
                ' Create directory in which you will unzip your items.
                IO.Directory.CreateDirectory(sExtractPath)

                ' Declare the folder where the items will be extracted.
                Dim output As Object = shObj.NameSpace((sExtractPath))

                ' Declare the input zip file.
                Dim input As Object = shObj.NameSpace((sZIPPath))

                ' Extract the items from the zip file.
                output.CopyHere((input.Items), 4)
            Else
                MsgBox("The zip file containing the data backup is not located in the program directory.")
                bOK = False
            End If

        Catch ex As Exception
            myMsgBox("clsRestore - ExtractZIP" & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK

    End Function

    ''' <summary>
    ''' Deletes all cloud restore related files in the program directory.
    ''' </summary>
    Private Sub DeleteRestoreFiles()

        Try
            If Me.ZipFileName <> "" And Me.ExtractedFolderName <> "" Then
                ' There was a file downloaded, delete the file.
                If (File.Exists(Me.CsvDirectory & Me.ZipFileName)) Then
                    DeleteFile(Me.CsvDirectory & Me.ZipFileName)
                End If

                If Directory.Exists(Me.CsvDirectory & Me.ExtractedFolderName) Then
                    Try
                        My.Computer.FileSystem.DeleteDirectory(Me.CsvDirectory, FileIO.DeleteDirectoryOption.DeleteAllContents)
                        Thread.Sleep(2000)
                    Catch ex As Exception

                    End Try
                End If
            End If

        Catch ex As Exception
            MsgBox("clsRestore - DeleteRestoreFiles" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

    End Sub


End Class
