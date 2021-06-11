Imports System.IO
Imports MySql.Data.MySqlClient
Imports DPosSecurity

Module modBackup
    Public Function ExportBackupCSVMySQL(ByVal sDatabase As String, ByVal sTable As String, ByVal valPath As String) As String
        Dim sReturn As String = ""
        Dim sSQL As String
        Dim sPath As String = valPath & "\" & sDatabase.ToLower & "_" & sTable.ToLower & ".BAK"
        Dim sString1 As String = Date.Today.Year.ToString & Date.Today.Month.ToString.PadLeft(2, "0") & Date.Today.Day.ToString.PadLeft(2, "0")
        Dim sString2 As String = Date.Today.AddDays(1).Year.ToString & Date.Today.AddDays(1).Month.ToString.PadLeft(2, "0") & Date.Today.AddDays(1).Day.ToString.PadLeft(2, "0")
        Dim sDataPath As String

        Try
            File.WriteAllText(sPath, modSecurity.EncryptString(sString1 & sMySQLDPosPass & sString2) & vbCrLf) 'sMySQLDPosPass

            sDataPath = valPath & "\temp_" & sDatabase.ToLower & "_" & sTable.ToLower & ".BAK"
            sDataPath = sDataPath.Replace("\", "\\")

            sSQL = "USE " & sDatabase & ";"
            sSQL &= "SELECT * FROM " & sTable & " "
            sSQL &= "INTO OUTFILE '" & sDataPath & "' "
            sSQL &= "FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '""' LINES TERMINATED BY '\n'"

            If ProcessMySQL(sSQL, GetConnectionStringMY(sDatabase)) <> "" Then
                sPath = ""
            End If

            sDataPath = sDataPath.Replace("\\", "\")

            If MergeTextFiles(sPath, sDataPath) = False Then
                sPath = ""
            End If

            sReturn = sPath
        Catch ex As Exception

        End Try

        Return sReturn
    End Function

    Public Function FillDatabaseStructureMySQL(ByVal sDatabase As String) As ArrayList
        Dim arrReturn As New ArrayList
        Dim MYCN As New MySqlConnection
        Dim MYCM As New MySqlCommand
        Dim MYDR As MySqlDataReader
        Dim sSQL As String

        Try
            sSQL = "USE sys; " &
                   "SELECT TABLE_SCHEMA, TABLE_NAME " &
                   "FROM INFORMATION_SCHEMA.TABLES " &
                   "WHERE TABLE_SCHEMA='" & sDatabase & "';"

            MYCN.ConnectionString = GetConnectionStringMY("")
            MYDBOpen(MYCN)
            MYCM.Connection = MYCN
            MYCM.CommandText = sSQL
            MYDR = MYCM.ExecuteReader

            Do While MYDR.Read
                arrReturn.Add(MYDR.Item("TABLE_SCHEMA") & CHAR_PARA & MYDR.Item("TABLE_NAME"))
            Loop

            MYDR.Close()
            MYCN.Close()
        Catch ex As Exception
            arrReturn.Clear()
        End Try

        Return arrReturn
    End Function

    Public Function SwitchTriggerSettings(ByVal bSwitch As Boolean, ByVal bWriteLog As Boolean) As Boolean
        Dim bReturn As Boolean = False

        Try
            If isCloudSyncEnabled() Then
                If bWriteLog Then
                    WritetoLog("", "")
                End If

                If bSwitch Then
                    bReturn = SwitchTriggers(bSwitch, bWriteLog)
                Else
                    bReturn = SwitchTriggers(bSwitch, bWriteLog)
                End If
            Else
                bReturn = True
            End If
        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Function SwitchTriggers(ByVal bSwitch As Boolean, ByVal bWriteLog As Boolean) As Boolean
        Dim bReturn As Boolean = False
        Dim sSQL As String
        Dim boolInt As Integer = 0

        If bSwitch Then
            boolInt = 1
        End If


        For Each thisDatabase As String In arDPosSchemas
            sSQL = "UPDATE mysql_trigger SET trigger_enabled=" & boolInt

            If bWriteLog Then
                If bSwitch Then
                    WritetoLog("Enabling trigger settings: " & thisDatabase.ToString & "...", "")
                Else
                    WritetoLog("Disabling trigger settings: " & thisDatabase.ToString & "...", "")
                End If
            End If

            Dim sProcess As String

            sProcess = ProcessMySQL(sSQL, GetConnectionStringMY(thisDatabase))

            If sProcess <> "" Then
                If bWriteLog Then
                    If bSwitch Then
                        ReWritetoLog("Enabling trigger settings: " & thisDatabase.ToString, "FAILED")
                    Else
                        ReWritetoLog("Disabling trigger settings: " & thisDatabase.ToString, "FAILED")
                    End If
                    WritetoLog(sProcess, "")
                End If

                Return bReturn
            Else
                If bWriteLog Then
                    If bSwitch Then
                        ReWritetoLog("Enabling trigger settings: " & thisDatabase.ToString, "SUCCESS")
                    Else
                        ReWritetoLog("Disabling trigger settings: " & thisDatabase.ToString, "SUCCESS")
                    End If
                End If
            End If
        Next

        bReturn = True

        Return bReturn
    End Function

    Public Function CompressFiles(ByVal sType As String, ByVal sFilesToAdd As Chilkat.StringArray, ByVal sZipFilePath As String, ByVal bSecured As Boolean) As String
        Dim sReturn As Boolean = False

        sReturn = CompressFiles(sType, sFilesToAdd, sZipFilePath, "Data", bSecured)

        Return sReturn
    End Function

    Public Function CompressFiles(ByVal sType As String, ByVal sFilesToAdd As Chilkat.StringArray, ByVal sZipFilePath As String, ByVal sPrefixZipFileName As String, ByVal bSecured As Boolean) As String
        Dim success As Boolean

        If sFilesToAdd.Count <> 0 Then
            Dim sZipFile As String = sZipFilePath & sPrefixZipFileName & " " & Date.Today.Year & Date.Today.Month.ToString.PadLeft(2, "0") & Date.Today.Day.ToString.PadLeft(2, "0") & ".zip"
            Dim iFileCount As Integer = 2

            While File.Exists(sZipFile)
                If File.Exists(sZipFile) Then
                    sZipFile = sZipFilePath & sPrefixZipFileName & " " & Date.Today.Year & Date.Today.Month.ToString.PadLeft(2, "0") & Date.Today.Day.ToString.PadLeft(2, "0") & "-" & iFileCount & ".zip"
                    iFileCount += 1
                End If
            End While

            Try
                'Dim zip As Package = ZipPackage.Open(sZipFile, IO.FileMode.OpenOrCreate, IO.FileAccess.ReadWrite)
                Dim zip As New Chilkat.Zip
                success = zip.UnlockComponent(ChilkatZipLicenseKey)

                If success Then
                    'create a new zip
                    success = zip.NewZip(sZipFile)

                    If bSecured Then
                        zip.Encryption = 4
                        zip.EncryptKeyLength = 128
                        zip.EncryptPassword = sMySQLDPosPass
                    End If

                    zip.AppendMultiple(sFilesToAdd, False)
                    zip.PasswordProtect = False

                    'write the zip file
                    success = zip.WriteZipAndClose
                End If

                If (Not success) Then
                    MsgBox(zip.LastErrorText)
                    Exit Try
                End If


                'delete the .BAK files now because they are in the zip.
                If sType = "Data" Then
                    Dim x As Integer
                    For x = 0 To (sFilesToAdd.Count - 1)
                        If IO.File.Exists(sFilesToAdd.GetString(x)) Then
                            IO.File.Delete(sFilesToAdd.GetString(x))
                        End If
                    Next
                End If

                Return sZipFile


            Catch ex As Exception

            End Try

        End If

        Return ""

    End Function

    Public Function BackupDatabase(ByVal sType As String, ByVal valPath As String) As String

        Application.DoEvents()

        Dim sName As String = ""

        Try
            sName = valPath & "\" & sType & ".BAK"
            Dim constring As String = GetConnectionStringMY(sType)
            ' Important Additional Connection Options
            'constring += ";charset=utf8;convertzerodatetime=true;"
            constring += ";convertzerodatetime=true;"

            Dim MYCON As New MySql.Data.MySqlClient.MySqlConnection
            Dim MYCMD As New MySql.Data.MySqlClient.MySqlCommand
            Dim mb As New MySql.Data.MySqlClient.MySqlBackup


            If MYCON.State = ConnectionState.Open Then
                MYCON.Close()
            End If

            MYCON.ConnectionString = constring
            MYCON.Open()
            MYCMD.Connection = MYCON
            mb.Command = MYCMD
            mb.ExportInfo.ExportRows = False
            mb.ExportToFile(sName)

            MYCMD.Dispose()
            MYCON.Close()


        Catch ex As Exception
            sName = ""
        End Try

        Return sName
    End Function

    Public Function BackupDatabaseMSSQL(ByVal sType As String, ByVal valPath As String, ByRef sDBerror As String) As String

        Application.DoEvents()

        Dim sName As String = ""

        Try
            sName = valPath & "\" & sType & ".BAK"
            Dim constring As String = GetConnectionStringMS(sType)
            Dim sProcess As String
            sProcess = ProcessMSSERVER("BACKUP DATABASE " & sType & " TO DISK = '" & sName & "'", constring)

            If sProcess <> "" Then
                sDBerror = sProcess
                sName = ""
            End If

        Catch ex As Exception
            sDBerror = ex.Message
            sName = ""
        End Try

        Return sName
    End Function

    Public Function DeleteDefaultBackups() As Boolean
        Dim bReturn As Boolean = False
        Dim x As Integer
        Dim sDeleteFiles As New ArrayList
        Dim sBAKFile As String = sDatabaseLocation & "\BACKUPS\"

        Try
            sDeleteFiles.Add(sBAKFile & "DeliverITSQL.BAK")
            sDeleteFiles.Add(sBAKFile & "StreetsSQL.BAK")
            sDeleteFiles.Add(sBAKFile & "DPosSysSQL.BAK")
            sDeleteFiles.Add(sBAKFile & "TimeClockSQL.BAK")
            sDeleteFiles.Add(sBAKFile & "StockSQL.BAK")

            For Each thistable As String In arDPOSTables
                Dim splitInfo As String()
                splitInfo = thistable.Split(CHAR_PARA)
                sDeleteFiles.Add(sBAKFile & splitInfo(0).ToString.ToLower & "_" & splitInfo(1).ToString.ToLower & ".BAK")
            Next

            sDeleteFiles.Add(sBAKFile & "deliveritsql_mysql_dposcredentials.BAK")
            sDeleteFiles.Add(sBAKFile & "deliveritsql_mysql_trigger.BAK")
            sDeleteFiles.Add(sBAKFile & "dpossyssql_mysql_trigger.BAK")
            sDeleteFiles.Add(sBAKFile & "stocksql_mysql_trigger.BAK")
            sDeleteFiles.Add(sBAKFile & "streetssql_mysql_trigger.BAK")
            sDeleteFiles.Add(sBAKFile & "timeclocksql_mysql_trigger.BAK")

            For x = 0 To (sDeleteFiles.Count - 1)
                If IO.File.Exists(sDeleteFiles.Item(x)) Then
                    IO.File.Delete(sDeleteFiles.Item(x))
                End If
            Next

            bReturn = True
        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Function GetAllFilesInZipbyDatabaseName(ByVal sDatabaseName As String, ByVal sZipFile As String) As ArrayList
        Dim arrResult As New ArrayList

        Dim zip As New Chilkat.Zip()
        zip.UnlockComponent(ChilkatZipLicenseKey)

        Dim success As Boolean
        success = zip.OpenZip(sZipFile)

        Try
            Dim zipEntry As Chilkat.ZipEntry = zip.FirstEntry

            While Not zipEntry Is Nothing
                If zipEntry.FileName.StartsWith(sDatabaseName.ToLower & "_") Then
                    arrResult.Add(zipEntry.FileName)
                End If
                zipEntry = zipEntry.NextEntry
            End While
        Catch ex As Exception

        End Try

        Return arrResult
    End Function
End Module
