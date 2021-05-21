Imports System.IO
Imports System.Text

Module modIni
    Public Function myINIExist(ByVal sFullPath As String) As Boolean
        Dim bReturn As Boolean = False

        If System.IO.File.Exists(sFullPath) = True Then
            bReturn = True
        Else
            bReturn = False
        End If

        Return bReturn
    End Function

    Public Function WriteToIni(ByVal sData As String, ByVal sFilter As String, ByVal sPath As String) As Boolean

        Dim sIniFilePath As String = sPath
        Dim iIndex As Integer = 0
        Dim bFound As Boolean = False
        Dim bReturn = False

        Try
            Dim lines() As String = System.IO.File.ReadAllLines(sIniFilePath)

            For iIndex = 0 To lines.Count - 1
                If lines(iIndex).StartsWith(sFilter) Then
                    ' The line was located.
                    bFound = True
                    Exit For
                End If
            Next

            If bFound = True Then
                ' Update the line.
                lines(iIndex) = sData
                lines.ToString.Replace(vbCrLf & vbCrLf, vbCrLf)
                File.WriteAllLines(sIniFilePath, lines)
            Else
                ' The line was not located, directly write the line.
                FileOpen(1, sIniFilePath, OpenMode.Append)
                PrintLine(1, sData)
                FileClose(1)
            End If

            bReturn = True
        Catch ex As Exception

            myMsgBox("WriteToIni" & vbCrLf & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bReturn
    End Function

    Public Sub LogDataImportDetails(ByVal sTableName As String, ByVal sErrorMsg As String, ByVal sColumns As String, ByVal sExportCMM As String, ByVal sImportCMM As String)
        Try
            Dim sLogMsg As New StringBuilder

            sLogMsg.Append(vbCrLf)
            sLogMsg.Append("Table Name: " & sTableName & vbCrLf)
            sLogMsg.Append("Time: " & DateTime.Now.ToString & vbCrLf)
            sLogMsg.Append("Error: " & sErrorMsg & vbCrLf & vbCrLf)
            sLogMsg.Append("Column Headers: " & vbCrLf & sColumns & vbCrLf & vbCrLf)
            sLogMsg.Append("Export Command: " & vbCrLf & sExportCMM & vbCrLf & vbCrLf)
            sLogMsg.Append("Import Command: " & vbCrLf & sImportCMM & vbCrLf & vbCrLf)

            LogDataImport(sLogMsg.ToString)
        Catch e As Exception
            'Ignore showerror errors
        End Try

    End Sub

    Public Sub LogDataImportDetails2(ByVal sTableName As String, ByVal arrErrorMsg As ArrayList, ByVal sColumns As String, ByVal sExportCMM As String, ByVal arrImportCMM As ArrayList)
        Try
            Dim sLogMsg As New StringBuilder

            sLogMsg.Append(vbCrLf)
            sLogMsg.Append("Table Name: " & sTableName & vbCrLf)
            sLogMsg.Append("Time: " & DateTime.Now.ToString & vbCrLf)
            sLogMsg.Append("Error: " & vbCrLf)

            For Each thiserrmsg As String In arrErrorMsg
                sLogMsg.Append(thiserrmsg)
            Next

            sLogMsg.Append(vbCrLf & vbCrLf)
            sLogMsg.Append("Column Headers: " & vbCrLf & sColumns & vbCrLf & vbCrLf)
            sLogMsg.Append("Export Command: " & vbCrLf & sExportCMM & vbCrLf & vbCrLf)
            sLogMsg.Append("Import Command: " & vbCrLf)

            For Each thisimportmsg As String In arrImportCMM
                sLogMsg.Append(thisimportmsg)
            Next

            sLogMsg.Append(vbCrLf & vbCrLf)

            LogDataImport(sLogMsg.ToString)
        Catch e As Exception
            'Ignore showerror errors
        End Try

    End Sub

    Public Sub LogDataImportDetails2(ByVal sDatabase As String, ByVal sTableName As String, ByVal sImportCMM As String, ByVal sErrorMsg As String)
        Try
            Dim sLogMsg As New StringBuilder

            sLogMsg.Append(vbCrLf)
            sLogMsg.Append("Database Name: " & sDatabase & vbCrLf)
            sLogMsg.Append("Table Name: " & sTableName & vbCrLf)
            sLogMsg.Append("Time: " & DateTime.Now.ToString & vbCrLf)
            sLogMsg.Append("Error: " & sErrorMsg & vbCrLf & vbCrLf)
            sLogMsg.Append("Import Command: " & vbCrLf & sImportCMM & vbCrLf & vbCrLf)

            LogDataImport(sLogMsg.ToString)
        Catch e As Exception
            'Ignore showerror errors
        End Try

    End Sub

    Public Sub LogDataImport(ByVal sWrite As String)
        Try
            Dim sFileName As String

            sFileName = sDatabaseLocation & "\ImportDataLogs.txt"

            Try
                'Save error to server Errors.txt in Database location
                FileOpen(1, sFileName, OpenMode.Append)
                PrintLine(1, sWrite)
                FileClose(1)
            Catch e As Exception

            End Try

        Catch e As Exception
            'Ignore showerror errors
        End Try

    End Sub

    Public Function CreateBackupLogFiles(ByVal sFullPath As String, ByVal lSize As Long) As Boolean
        Dim bReturn As Boolean = True
        Try
            Dim sBakFile As String = Path.GetDirectoryName(sFullPath) & "\" & Path.GetFileNameWithoutExtension(sFullPath) & "bak.txt"

            Dim chkLog As New FileInfo(sFullPath)
            Dim iLogSize As Long = chkLog.Length

            If System.IO.File.Exists(sFullPath) Then
                If iLogSize > lSize Then    ' If the file length is > the parameter copy it to the backup log and delete it
                    If System.IO.File.Exists(sBakFile) Then
                        System.IO.File.Delete(sBakFile)
                    End If
                    System.IO.File.Copy(sFullPath, sBakFile)
                    System.IO.File.Delete(sFullPath)
                End If
            End If

        Catch ex As Exception
            bReturn = False
        End Try

        Return bReturn
    End Function

End Module
