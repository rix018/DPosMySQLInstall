Imports System.Collections.Specialized
Imports System.IO
Imports System.Net
Imports System.Threading
Imports System.Web.Script.Serialization
Imports MySql.Data.MySqlClient
Imports Renci.SshNet

Module modRestore

    Public Function DownloadRestoreFilesV2(ByVal oThisMSGList As clsRestore) As Boolean

        Dim bOk As Boolean = False
        Dim sResponse As String = ""
        Dim creds As New clsSSHCredentials

        Try
            ' Send the message list.
            sResponse = SendWithWaitV2(oThisMSGList.MessageList)

            ' Check if the response is not nothing.
            If sResponse <> "" Then
                ' The download link is now available.
                Dim sDownloadSource As String = sResponse
                Dim sFileName As String = sDownloadSource.Substring(sDownloadSource.LastIndexOf("/") + 1)
                Dim sError As String = ""

                creds = GenerateSSHCredentials(sRetSSH, sError, True)

                If creds Is Nothing Then
                    Return False
                End If

                ' Create the restore directory.
                CreateRestoreDirectory(oThisMSGList)

                ' Download the file to the specified directory.
                If DownloadDataViaSFTP(creds, sDownloadSource, oThisMSGList.CsvDirectory) = True Then
                    oThisMSGList.ZipFileName = sFileName
                    bOk = True
                Else
                    ' Failed to download the file.
                    bOk = False
                End If
            End If

        Catch ex As Exception
            myMsgBox("DownloadRestoreFiles: " & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOk = False
        End Try

        Return bOk

    End Function

    Public Function GenerateJSONForTables() As String

        Dim listForSending As New clsSerializer
        Dim sTableName As String = ""
        Dim sJSONString As String = ""

        Try

            arDPOSTables.Sort()

            ' Build the json for each table.
            For Each sTableName In arDPOSTables
                Dim item As New clsColumns
                Dim sTblInfo As String() = sTableName.Split(CHAR_PARA)
                If sTblInfo(1).ToUpper <> "AUDIT" Then
                    item.TableName = sTblInfo(1).ToLower
                    listForSending.Add(item)
                End If
            Next

            If listForSending.Count <> 0 Then
                sJSONString = listForSending.SerializeList
            End If

        Catch ex As Exception
            MsgBox("GenerateJSONForColumns" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            sJSONString = ""
        End Try

        Return sJSONString

    End Function

    Public Function ImportCSVDataV2(ByVal sFilePath As String, ByVal sTableName As String, ByVal headers As String, ByVal bReplace As Boolean) As Boolean

        Dim bSuccess As Boolean = False
        Dim fileList As New ArrayList
        Dim bHeaders As Boolean = True
        Dim fileName As String = ""

        Try
            If headers = "" Then
                bHeaders = False
            End If

            fileName = sFilePath.Substring(sFilePath.LastIndexOf("\") + 1)
            fileName = fileName.Substring(0, fileName.LastIndexOf("."))

            fileList = BreakDownCSVFile(sFilePath.Substring(0, sFilePath.LastIndexOf("\") + 1), fileName, sTableName, bHeaders, bReplace)

            If fileList.Count > 0 Then
                For Each thisFile As String In fileList
                    If ImportLoadInFile(sFilePath.Substring(0, sFilePath.LastIndexOf("\") + 1) & thisFile, sTableName, headers, bReplace) Then
                        bSuccess = True
                    Else
                        bSuccess = False
                        Exit For
                    End If
                Next
            Else
                bSuccess = ImportLoadInFile(sFilePath.Substring(0, sFilePath.LastIndexOf("\") + 1) & sTableName & ".csv", sTableName, headers, bReplace)
            End If

        Catch ex As Exception
            myMsgBox("ImportCSVDataV2" & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bSuccess = False
        End Try

        Return bSuccess

    End Function

    Public Function ImportLoadInFile(ByVal sFilePath As String, ByVal sTableName As String, ByVal headers As String, ByVal bReplace As Boolean) As Boolean

        Dim dataTable As New DataTable()
        Dim sSQL As String = ""
        Dim bImportOK As Boolean = True
        Dim MYCON As New MySqlConnection
        Dim MYCMD As New MySqlCommand
        Dim iTimeOut As Integer = 100

        Try

            If File.Exists(sFilePath) = False Then
                ' The file specified does not exists.
                bImportOK = False
                Return bImportOK
            End If

            iTimeOut = SetSizeBasedTimeout(sFilePath)

            If MYCON.State = ConnectionState.Open Then
                MYCON.Close()
            End If

            Dim sSchema As String = GetDatabaseOfTable(sTableName)

            MYCON.ConnectionString = GetConnectionStringMY(sSchema) & ";allowuservariables=true;"

            MYDBOpen(MYCON)
            MYCMD.Connection = MYCON
            MYCMD.CommandTimeout = iTimeOut

            DisableIndex(sTableName, MYCMD)
            DisableUniqueChecks(MYCMD)
            DisableForeignKeyChecks(MYCMD)

            sSQL = "LOAD DATA LOCAL INFILE '" & sFilePath.Replace("\", "/") & "' "
            If bReplace Then
                sSQL &= "REPLACE "
            End If
            sSQL &= "INTO TABLE " & sTableName.ToLower & " "
            sSQL &= "FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '""' ESCAPED BY '\\' LINES TERMINATED BY '\n' "
            If headers <> "" Then
                sSQL &= "IGNORE 1 LINES (" & headers & ") "
            End If

            MYCMD.CommandText = sSQL

            Dim sError As String = ""
            sError = MyExecuteNonQueryMY(MYCMD)

            If sError <> "" Then
                LogDataImport("Import Data: FAILED")
                LogDataImportDetails2(sSchema, sTableName, sSQL, sError)
                bImportOK = False
            End If

            EnableForeignKeyChecks(MYCMD)
            EnableUniqueChecks(MYCMD)
            EnableIndex(sTableName, MYCMD)

        Catch ex As Exception
            myMsgBox("ImportLoadInFile" & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bImportOK = False
        Finally
            If MYCON.State = ConnectionState.Open Then
                MYCON.Close()
            End If
        End Try

        Return bImportOK

    End Function

    ''' <summary>
    ''' Deletes data from a table.
    ''' </summary>
    ''' <param name="sTableName">The table whose data will be deleted.</param>
    ''' <returns>true if the data in the table has been successfully deleted.</returns>
    Public Function TruncateDataFromTable(ByVal sTableName As String) As Boolean

        Dim bOk As Boolean = True
        Dim sSQL As String = ""
        Dim sStartEnd As String = ""
        Dim iRows As Integer = 0
        Dim MYCON As New MySqlConnection
        Dim MYCMD As New MySqlCommand

        Try
            MYCON.ConnectionString = GetConnectionStringMY(GetDatabaseOfTable(sTableName)) & ";allowuservariables=true;"

            MYDBOpen(MYCON)
            MYCMD.Connection = MYCON

            DisableIndex(sTableName, MYCMD)
            DisableUniqueChecks(MYCMD)
            DisableForeignKeyChecks(MYCMD)

            ' Delete all data.
            sSQL = "TRUNCATE " & sTableName

            MYCMD.CommandText = sSQL

            If MyExecuteNonQueryMY(MYCMD) <> "" Then
                bOk = False
            Else
                bOk = True
            End If

            EnableForeignKeyChecks(MYCMD)
            EnableUniqueChecks(MYCMD)
            EnableIndex(sTableName, MYCMD)

        Catch ex As Exception
            MsgBox("clsRestore - DeleteDataFromTable" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            bOk = False
        Finally
            If MYCON.State = ConnectionState.Open Then
                MYCON.Close()
            End If
        End Try

        Return bOk

    End Function

    Public Function DisableIndex(ByVal sTable As String, ByRef cmd As MySqlCommand) As Boolean

        Dim sSQL As String
        Dim bOK As Boolean = False
        Dim sDB As String

        Try
            sDB = GetDatabaseOfTable(sTable)

            sSQL = "ALTER TABLE `" & sDB & "`.`" & sTable & "` DISABLE KEYS"
            cmd.CommandText = sSQL

            If MyExecuteNonQueryMY(cmd) <> "" Then
                bOK = True
            End If

        Catch ex As Exception
            MsgBox("EnableIndex" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return bOK

    End Function

    Public Function DisableUniqueChecks(ByRef cmd As MySqlCommand) As Boolean

        Dim sSQL As String = ""
        Dim bOK As Boolean = False

        Try
            sSQL = "SET unique_checks=0"
            cmd.CommandText = sSQL

            If MyExecuteNonQueryMY(cmd) <> "" Then
                bOK = True
            End If

        Catch ex As Exception
            MsgBox("DisableUniqueChecks" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return bOK

    End Function

    Public Function DisableForeignKeyChecks(ByRef cmd As MySqlCommand) As Boolean

        Dim sSQL As String = ""
        Dim bOK As Boolean = False

        Try
            sSQL = "SET FOREIGN_KEY_CHECKS=0"
            cmd.CommandText = sSQL

            If MyExecuteNonQueryMY(cmd) <> "" Then
                bOK = True
            End If

        Catch ex As Exception
            MsgBox("DisableForeignKeyChecks" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return bOK

    End Function

    Public Function EnableForeignKeyChecks(ByRef cmd As MySqlCommand) As Boolean

        Dim sSQL As String = ""
        Dim bOK As Boolean = False

        Try
            sSQL = "SET FOREIGN_KEY_CHECKS=1"
            cmd.CommandText = sSQL

            If MyExecuteNonQueryMY(cmd) <> "" Then
                bOK = True
            End If

        Catch ex As Exception
            MsgBox("EnableForeignKeyChecks" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return bOK

    End Function

    Public Function EnableUniqueChecks(ByRef cmd As MySqlCommand) As Boolean

        Dim sSQL As String = ""
        Dim bOK As Boolean = False

        Try
            sSQL = "SET unique_checks=1"
            cmd.CommandText = sSQL

            If MyExecuteNonQueryMY(cmd) <> "" Then
                bOK = True
            End If

        Catch ex As Exception
            MsgBox("EnableUniqueChecks" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return bOK

    End Function

    Public Function EnableIndex(ByVal sTable As String, ByRef cmd As MySqlCommand) As Boolean

        Dim sSQL As String
        Dim bOK As Boolean = False
        Dim sDB As String

        Try
            sDB = GetDatabaseOfTable(sTable)

            sSQL = "ALTER TABLE `" & sDB & "`.`" & sTable & "` ENABLE KEYS"
            cmd.CommandText = sSQL

            If MyExecuteNonQueryMY(cmd) <> "" Then
                bOK = True
            End If

        Catch ex As Exception
            MsgBox("EnableIndex" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return bOK

    End Function

    Public Function BreakDownCSVFile(ByVal fileLocation As String, ByVal fileName As String, ByVal tableName As String, ByVal bHeaders As Boolean, ByVal bReplace As Boolean) As ArrayList

        Dim csvFile As String = fileName & ".csv"
        Dim lineCount As Integer = 0
        Dim fileNameArray As New ArrayList
        Dim cloudHeaders As String = ""
        Dim maxRow As Integer = 25000

        Try
            ' Get the number of rows in the csv file
            lineCount = File.ReadLines(fileLocation & csvFile).Count()
            If bHeaders Then
                cloudHeaders = File.ReadLines(fileLocation & csvFile).First()
            End If

            If bReplace Then
                maxRow = 20000
            End If

            If tableName = "tblcustomers" OrElse tableName = "tblorderdetails" OrElse tableName = "tblorderheaders" Then
                ' Decrease bulk size for these tables.
                If bReplace Then
                    maxRow = 5000
                Else
                    maxRow = 10000
                End If
            End If

            ' If row count exceeds 10000, divide file by 25000 rows each.
            If lineCount > maxRow Then

                Using sr As StreamReader = New StreamReader(fileLocation & csvFile)

                    Dim numberTag As Integer = 1
                    ' Added number tag to the file name.
                    Dim newFileName As String = ""

                    While Not sr.EndOfStream

                        Dim i As Integer = 0
                        newFileName = fileName & numberTag.ToString & ".csv"

                        If File.Exists(fileLocation & newFileName) Then
                            File.Delete(fileLocation & newFileName)
                        End If

                        Using sw As StreamWriter = New StreamWriter(fileLocation & newFileName)

                            If bHeaders AndAlso numberTag <> 1 Then
                                ' The first file will have the headers, so start adding headers only on the second file.
                                sw.WriteLine(cloudHeaders)
                            End If

                            While Not sr.EndOfStream AndAlso i <= maxRow
                                sw.WriteLine(sr.ReadLine())
                                i += 1
                            End While

                            sw.Flush()

                        End Using

                        ' Add the file to array so they can be imported one by one later.
                        fileNameArray.Add(newFileName)
                        numberTag += 1

                    End While
                End Using
            End If
        Catch ex As Exception

            'During failure, delete all created files and reset the file array.
            For Each file In fileNameArray
                If file.Exists(fileLocation & file) Then
                    file.Delete(fileLocation & file)
                End If
            Next

            fileNameArray.Clear()

            MsgBox("clsRestore - BreakDownCSVFile" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return fileNameArray

    End Function

    Public Function GetComparedColumns(ByVal localHeaders As String, ByVal filePath As String) As String

        Dim headers As String = ""
        Dim comma As String = ""

        Try
            Dim cloudHeaders As String = File.ReadLines(filePath).First()
            Dim localColumns As ArrayList = New ArrayList(localHeaders.Split(CHAR_COMMA))
            Dim cloudColumns As ArrayList = New ArrayList(cloudHeaders.Split(CHAR_COMMA))

            For Each col As String In cloudColumns
                If localColumns.Contains(col) Then
                    headers &= comma & col
                Else
                    headers &= comma & "@" & col
                End If
                comma = CHAR_COMMA.ToString
            Next

            Return headers
        Catch ex As Exception
            Return ""
        End Try

    End Function

    Public Function GetTableColumnNames(ByVal tableName As String) As String

        Dim headers As String = ""
        Dim schema As String = ""
        Dim sSQL As String = ""
        Dim MYCN As New MySqlConnection
        Dim MYCM As New MySqlCommand

        Try
            schema = GetDatabaseOfTable(tableName).ToLower
            If schema = "" Then
                schema = "deliverit"
            End If
            'schema &= "sql"

            sSQL = "SELECT GROUP_CONCAT(column_name) AS headers "
            sSQL &= "FROM information_schema.columns "
            sSQL &= "WHERE table_schema='" & schema & "' AND table_name='" & tableName & "' "
            sSQL &= "GROUP BY table_schema,table_name; "

            MYCN.ConnectionString = GetConnectionStringMY("")
            MYDBOpen(MYCN)

            MYCM.Connection = MYCN
            MYCM.CommandText = sSQL

            Dim bSuccess As Boolean = True
            headers = MyExecuteScalarMY(MYCM, bSuccess).ToString

        Catch ex As Exception
            MsgBox("GetTableColumnNames" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        Finally
            MYCN.Close()
        End Try

        Return headers

    End Function

    ''' <summary>
    ''' Checks if a file is empty.
    ''' </summary>
    ''' <param name="sFilePath">The file to be examined if it is empty or not.</param>
    ''' <returns>true if a file is empty.</returns>
    Public Function IsFileEmpty(ByVal sFilePath As String) As Boolean

        Dim bEmpty As Boolean = False

        Try
            If File.Exists(sFilePath) = True Then
                If File.ReadAllText(sFilePath).Length = 0 Then
                    bEmpty = True
                Else
                    bEmpty = False
                End If
            End If

        Catch ex As Exception
            MsgBox("clsRestore - IsFileEmpty" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            bEmpty = False
        End Try

        Return bEmpty

    End Function

    Private Function SetSizeBasedTimeout(ByVal sFile As String) As Integer

        Dim iTime As Integer = 100

        Try
            Dim myFileInfo As New FileInfo(sFile)
            Dim myFileSize As Long = myFileInfo.Length

            If myCInt(myFileSize) > 10000000 Then
                iTime = myCInt(myFileSize) / 10000000
                iTime *= 100
                If iTime > 900 Then
                    iTime = 900
                End If
            End If

        Catch ex As Exception

        End Try

        Return 100

    End Function



    ''' <summary>
    ''' Get the database string for determining the connection string of a table.
    ''' </summary>
    ''' <param name="sTableName">The table to be evaluated for getting its database string.</param>
    ''' <returns>the database string for determining the connection string of a table</returns>
    Public Function GetDatabaseOfTable(ByVal sTableName As String) As String

        Dim sDatabase As String = ""
        Dim sString As String = ""

        Try
            sTableName = sTableName.Trim

            For Each sString In arDPOSTables
                Dim thisStringInfo As String() = sString.Split(CHAR_PARA)
                If thisStringInfo(1).ToUpper = sTableName.ToUpper Then
                    sDatabase = thisStringInfo(0).ToString
                    Return sDatabase
                End If
            Next
        Catch ex As Exception
            MsgBox("GetDatabaseOfTable" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return sDatabase

    End Function

    Public Sub CreateRestoreDirectory(ByVal oThisMSGList As clsRestore)

        Try
            ' Delete the directory first if it exists.
            If Directory.Exists(oThisMSGList.CsvDirectory) = True Then
                My.Computer.FileSystem.DeleteDirectory(oThisMSGList.CsvDirectory, FileIO.DeleteDirectoryOption.DeleteAllContents)
                System.Threading.Thread.Sleep(5000)
            End If

            ' Create the directory where the restore files would be downloaded.
            My.Computer.FileSystem.CreateDirectory(oThisMSGList.CsvDirectory)

            System.Threading.Thread.Sleep(5000)

        Catch ex As Exception
            myMsgBox("CreateRestoreDirectory" & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

    End Sub

    Public Function DownloadDataViaSFTP(ByVal credentials As clsSSHCredentials, ByVal source As String, ByVal destination As String) As Boolean

        Dim bStatus As Boolean = True
        Dim fileName As String = ""

        Try
            fileName = source.Substring(source.LastIndexOf("/") + 1)

            ' If the file exists in the download directory, delete the existing file first.
            If (File.Exists(destination & fileName)) Then
                File.Delete(destination & fileName)
            End If

            Dim client As SftpClient = New SftpClient(credentials.host, credentials.port, credentials.username, credentials.password)
            client.Connect()

            Dim filestream As Stream = File.OpenWrite(Path.Combine(destination, fileName))

            Using filestream
                client.DownloadFile(source, filestream)
            End Using

            Threading.Thread.Sleep(3000)
            client.Disconnect()
            filestream.Close()

        Catch ex As WebException
            MsgBox("WebException: DownloadDataViaSFTP" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            bStatus = False
        Catch ex As Exception
            MsgBox("DownloadDataViaSFTP" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            bStatus = False
        End Try

        Return bStatus

    End Function

    Public Function SendWithWaitV2(ByVal sendMessageList As clsMessageList) As String

        Dim iRetryCount As Integer = 0
        Dim message As New clsMessage
        Dim messageList As New clsMessageList
        Dim sDownloadLink As String = ""
        Dim sResponse As String = ""
        Dim sJsonData As String = ""
        Dim exportID As String = ""
        Dim iRets As Integer = 0

        Try
            sJsonData = sendMessageList.SerializeList

            While sDownloadLink = ""

                If exportID <> "" Then
                    sendMessageList.AddToList("ExportID", exportID)
                    sJsonData = sendMessageList.SerializeList
                End If

                ' Send the restore parameters to the server.
                sResponse = SendMessage(sJsonData, GetURL("COMMUNICATE", sRetCloudURL))

                If sResponse = "CONNERROR" Then
                    ' Connection problem. Either timeout or some other problem.
                    If CheckForInternetConnection() = False Then
                        ' No internet connection.
                        myMsgBox("Import from Cloud Error: No internet connection detected.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                        'ReWritetoLog("Downloading Files from cloud", "FAILED")
                        'WritetoLog("Restore from Cloud: No internet connection detected. ", "")
                        sResponse = ""
                        sDownloadLink = ""
                        Exit While
                    Else
                        myMsgBox("Import from Cloud Error: A connection error has occured. Please try again.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                        'ReWritetoLog("Downloading Files from cloud", "FAILED")
                        'WritetoLog("Restore from Cloud: No internet connection detected. ", "")
                        sDownloadLink = ""
                        Exit While
                    End If
                Else
                    ' Check if the response is a json.
                    If IsJSON(sResponse) = True Then

                        ' Deserialize the reply.
                        messageList = messageList.DeserializeList(sResponse)

                        If messageList.Count <> 0 Then
                            ' Browse each value in the reply.
                            For Each message In messageList
                                If message.Header = "RestoreOutfiles" And message.Value <> "InProgress" Then

                                    If message.Value = "Credentials_Invalid" Then
                                        ' Invalid username and password.
                                        myMsgBox("Incorrect username or password.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                                        'ReWritetoLog("Downloading Files from cloud", "FAILED")
                                        'WritetoLog("Incorrect username or password.", "")
                                        sDownloadLink = ""
                                        Exit While
                                    ElseIf message.Value = "DposTables_Error" Then
                                        ' A problem with the dpos tables json.
                                        myMsgBox("Dpos tables json error.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                                        'ReWritetoLog("Downloading Files from cloud", "FAILED")
                                        'WritetoLog("Dpos tables json error.", "")
                                        sDownloadLink = ""
                                        Exit While
                                    Else
                                        ' The server has finished processing the csv files. Get the download link provided.
                                        sDownloadLink = message.Value
                                        Exit While
                                    End If
                                Else
                                    If message.Header = "RestoreOutfiles" AndAlso message.Value = "InProgress" Then
                                        If iRets >= 30 Then
                                            Dim bRetry As Boolean = True

                                            If myMsgBox("Do you still want to continue?", "Downloading Files from cloud takes too long", myMsgBoxDisplay.CustomYesNo, "CONTINUE", "ABORT") = DialogResult.OK Then
                                                iRets = 0
                                            Else
                                                iRets = 0
                                                'ReWritetoLog("Downloading Files from cloud", "ABORTED")
                                                'WritetoLog("Downloading Files from cloud takes too long", "")
                                                sDownloadLink = ""
                                                Exit While
                                            End If
                                        Else
                                            iRets += 1
                                        End If
                                    End If
                                End If

                                If message.Header = "DposVersion" Then
                                    ' Incompatible dpos versions.
                                    myMsgBox("DPos must be on version " & message.Value & " to do a restore.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                                    'ReWritetoLog("Downloading Files from cloud", "FAILED")
                                    'WritetoLog("Dpos tables json error.", "")
                                    sDownloadLink = ""
                                    Exit While
                                End If

                                If message.Header = "ER111" Then
                                    ' Invalid client id.
                                    myMsgBox("Invalid client id.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                                    'ReWritetoLog("Downloading Files from cloud", "FAILED")
                                    'WritetoLog("Invalid client id.", "")
                                    sDownloadLink = ""
                                    Exit While
                                End If

                                If message.Header = "ExportID" Then
                                    ' Get exportID.
                                    exportID = message.Value
                                End If

                            Next
                        End If

                        '' Clear the message list to be able to send a follow up request.
                        'messageList = New clsMessageList
                        'messageList.AddToList("ClientID", sClientID)
                        'messageList.AddToList("RestoreOutfiles", "FollowUp")
                        'sJsonData = messageList.SerializeList
                    Else
                        ' The server is not responding with any json format message. An error must have occured.
                        myMsgBox("Import from Cloud Error: The server responded with an error - " & sResponse, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                        'ReWritetoLog("Downloading Files from cloud", "FAILED")
                        'WritetoLog("Import from Cloud Error: The server responded with an error - " & sResponse, "")
                        sDownloadLink = ""
                        Exit While
                    End If

                End If

                ' Wait 10 seconds before retrying the request.
                Thread.Sleep(10000)

            End While

        Catch ex As Exception
            MsgBox("SendWithWait" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            sDownloadLink = ""
        End Try

        Return sDownloadLink

    End Function

    Public Function SendMessage(ByVal sMessage As String, ByVal sURL As String) As String

        Dim sResponse As String = ""
        Dim sJSON As String = sMessage

        Try
            sResponse = SendData(sJSON, sURL, ContentTypeJSON)

        Catch ex As Exception
            sResponse = ""
        End Try

        Return sResponse

    End Function

    ''' <summary>
    ''' This function sends the data to the host server via HTTP Post method.
    ''' </summary>
    ''' <param name="sData">The data to be posted to the host server.</param>
    ''' <param name="sURI">The url where the data will be posted.</param>
    ''' <returns>the response of the host server.</returns>
    Public Function SendData(ByVal sData As String, ByVal sURI As String, ByVal sContentType As String) As String

        Dim sResponseCode As String = ""

        Try
            Dim sMyURI As New Uri(sURI)
            Dim request As HttpWebRequest

            Dim byteArray As Byte() = System.Text.Encoding.UTF8.GetBytes(sData)
            request = CType(HttpWebRequest.Create(sMyURI), HttpWebRequest)
            request.Method = "POST"
            request.ContentType = sContentType
            request.ContentLength = byteArray.Length
            request.Credentials = CredentialCache.DefaultNetworkCredentials
            request.KeepAlive = False
            request.Timeout = 300000

            Dim dataStream As Stream = request.GetRequestStream()
            dataStream.Write(byteArray, 0, byteArray.Length)
            dataStream.Close()

            Dim response As WebResponse = request.GetResponse()
            dataStream = response.GetResponseStream()

            Dim reader As New StreamReader(dataStream)
            sResponseCode = reader.ReadToEnd

            reader.Close()
            dataStream.Close()
            response.Close()

        Catch ex As WebException
            If bDemo = True Then
                MsgBox("SendData" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            End If
            'WriteToApplicationLog("SendData method has encountered an error: " & sResponseCode & " " & ex.ToString)
            sResponseCode = "CONNERROR"
            Return sResponseCode
        Catch ex As Exception
            If bDemo = True Then
                MsgBox("SendData" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            End If
            'WriteToApplicationLog("SendData method has encountered an error: " & sResponseCode)
            sResponseCode = "CONNERROR"
            Return sResponseCode
        End Try

        Return sResponseCode

    End Function

    Public Function GetURL(ByVal sFilter As String, ByVal sURL As String) As String

        Dim sReturnURL As String = ""

        Try
            If sFilter = "BATCH" Then
                ' Batch sending.
                sReturnURL = sURL.Substring(0, sURL.LastIndexOf("/") + 1) + "batch.php"
            ElseIf sFilter = "RESYNC" Then
                ' Used for resync of data.
                sReturnURL = sURL.Substring(0, sURL.LastIndexOf("/") + 1) + "resync.php"
            ElseIf sFilter = "COMMUNICATE" Then
                ' Used for the communicate API.
                sReturnURL = sURL.Substring(0, sURL.LastIndexOf("/") + 1) + "communicate.php"
            Else
                ' Single sending.
                sReturnURL = sURL.Substring(0, sURL.LastIndexOf("/") + 1) + "simple.php"
            End If

        Catch ex As Exception
            MsgBox("GetURL" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return sReturnURL

    End Function

    Public Function GenerateSSHCredentials(ByVal thisSSHUri As String, ByRef sError As String, ByVal bReportError As Boolean) As clsSSHCredentials

        Dim downloadURL As String = ""
        Dim sResponse As String = ""
        Dim creds As clsSSHCredentials

        Try
            If thisSSHUri.Trim = "" Then
                If bReportError Then
                    myMsgBox("Error detected - SSHCredentialsURL is not present.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                End If

                sError = "Error detected - SSHCredentialsURL is not present."
                Return Nothing
            End If

            If GetSSHCredentials(sResponse, thisSSHUri) = True Then
                If sResponse.ToUpper.Contains("HOST") Then
                    Dim jss As New JavaScriptSerializer()
                    creds = jss.Deserialize(Of clsSSHCredentials)(sResponse)
                Else
                    If bReportError Then
                        myMsgBox("Error detected - Client ID not recognised by SSH Credentials API.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sError = "Error detected - Client ID not recognised by SSH Credentials API."
                    Return Nothing
                End If
            Else
                ' Other errors.
                If bReportError Then
                    myMsgBox("Error detected - Unable to get SSH credentials.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                End If

                sError = "Error detected - Unable to get SSH credentials."
                Return Nothing
            End If
        Catch ex As Exception
            If bReportError Then
                myMsgBox("GenerateSSHCredentials: " & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If
            sError = "GenerateSSHCredentials: " & ex.Message
            Return Nothing
        End Try

        Return creds

    End Function

    ''' <summary>
    ''' This function sends a POST request to an API which will give the SSH credentials needed for uploading data.
    ''' </summary>
    ''' <param name="sResponseCode">The response string received from the API. It contains a JSON string of either the credentials or error response code.</param>
    ''' <returns>a bit that determines whether communication to API is successful or not.</returns>
    Public Function GetSSHCredentials(ByRef sResponseCode As String, ByVal sURI As String) As Boolean

        Dim params As New StringDictionary
        Dim bOK As Boolean = True

        Try

            params.Add("client_token", GenerateClientID(sRetClientID))
            sURI = AppendParameters(sURI, params)

            Dim sMyURI As New Uri(sURI)
            Dim request As HttpWebRequest

            request = CType(HttpWebRequest.Create(sMyURI), HttpWebRequest)
            request.Method = "POST"
            request.ContentType = "application/x-www-form-urlencoded"
            request.Credentials = CredentialCache.DefaultNetworkCredentials
            request.KeepAlive = False
            request.Timeout = 60000

            Dim response As HttpWebResponse = CType(request.GetResponse, HttpWebResponse)
            Dim reader As New StreamReader(response.GetResponseStream())

            While reader.Peek >= 0
                sResponseCode = reader.ReadToEnd
            End While

            reader.Close()
            response.Close()

        Catch ex As WebException
            If bDemo = True Then
                MsgBox("GetSSHCredentials" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            End If
            sResponseCode = "CONNERROR"
            Return False
        Catch ex As Exception
            If bDemo = True Then
                myMsgBox("GetSSHCredentials" & vbCrLf & ex.ToString, "MySQL DPos Install and Import Data: Error", myMsgBoxDisplay.OkOnly)
            End If
            sResponseCode = "CONNERROR"
            Return False
        End Try

        Return bOK

    End Function
End Module
