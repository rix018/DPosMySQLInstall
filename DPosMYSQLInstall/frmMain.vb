Imports System.Web.Script.Serialization
Imports MySql.Data.MySqlClient
Imports System.Data.SqlClient
Imports System.IO
Imports System.Text
Imports System.Threading
Imports Microsoft.Win32
Imports System.Management
Imports System.ComponentModel
Imports System.Windows
Imports System.ServiceProcess
Imports DPosSecurity.modSecurity
Imports System.Security.AccessControl
Imports System.Net
Imports System.Security.Cryptography
Imports System.Net.NetworkInformation
Imports System.Collections.Specialized
Imports Renci.SshNet

Public Class frmMain
    Dim iCurrentStep As Integer
    Dim StepByStep As Boolean
    Dim bDoAnyBtnEvents As Boolean         'other Button functions like Generating Schemas and Tables, Import Data, and Dropping Tables under Advanced User Mode will not be executed if it was flagged as False
    Dim bRepaintDatagridView As Boolean
    Dim bSwitchToggle As Boolean
    Dim iCount As Integer = 1
    Public MyProgress As Double
    Dim arrRstBak As New ArrayList
    Dim bCloudEnabled As Boolean


    Sub frmInitialize()

        Control.CheckForIllegalCrossThreadCalls = False

        If Process.GetProcessesByName _
            (Process.GetCurrentProcess.ProcessName).Length > 1 Then
            Try
                myMsgBox("Another Instance of DPOS MySQL Install is already running", "Multiple Instances Forbidden", myMsgBoxDisplay.OkOnly)

                ExitProgram()
                Exit Sub
            Catch ex As Exception
            End Try
        End If

        If IsMySQLServerInstalled() Then
            Dim sServiceStatus As String
            Dim mysqlservice As New ServiceController("MySQL57")

            sServiceStatus = mysqlservice.Status
            If sServiceStatus <> 4 Then
                SwitchMySQLServerService(True)
            End If
        End If

        iLogonFrmPosX = Me.Left + Me.Width / 2
        iLogonFrmPosY = Me.Top + Me.Height / 2

        iCurrentStep = AppStepProcess.START
        MyProgress = 0

        SwitchStepstoButtons(False)
        bSwitchToggle = True
        bDoAnyBtnEvents = True
        bCloudEnabled = False

        Me.DataGridView1.Columns("Column1").Width = 370
        Me.DataGridView1.Columns("Column2").Width = 100

        If Debugger.IsAttached Then
            bDemo = True
        Else
            bDemo = False
        End If

        'loading required locations
        sDatabaseLocation = GetDatabaseLocation(1)
        sProgramLocation = GetProgramLocation(1)

        ResetAllImportOptions()
        'fills the arrays for schemas and tables(includes the schema where the table should be placed, its table name, and the column headers along with its data type). This will be used on Creating Schemas and Tables on MySQL Server
        FillAllArrayList()

        sMySQLRootPass = DecryptString(GetInfoFromIni("ROOT-USER", GetDatabaseLocationDPOS(1) & "\Dpos.ini", False))
        sMySQLDPosPass = DecryptString(GetInfoFromIni("DPOS-USER", GetDatabaseLocationDPOS(1) & "\Dpos.ini", False))

        If sMySQLRootPass = "" Or sMySQLDPosPass = "" Then
            'uncomment this on release
            'pass ClientID here??
            sJSonDataMySQLCreds = GetJSonfromWebService("http://api.deliverit.com.au/dpos-mysql.php", "GET", 10000)   'url for now

            If sJSonDataMySQLCreds = "" Or sJSonDataMySQLCreds = "{""root_password"":"""",""dpos_password"":""""}" Then
                sJSonDataMySQLCreds = "{""root_password"":""" & EncryptString("1234@home") & """,""dpos_password"":""" & EncryptString("dpos@99") & """}"
            End If

            sMySQLRootPass = DecryptString(GetJSONdata(sJSonDataMySQLCreds, "root_password"))
            sMySQLDPosPass = DecryptString(GetJSONdata(sJSonDataMySQLCreds, "dpos_password"))

            If CreateDPosIni() = False Then
                'Do Not do anything for now
            End If

            System.Threading.Thread.Sleep(2000)
            WriteToIni("ROOT-USER=" & EncryptString(sMySQLRootPass), "ROOT-USER=", GetDatabaseLocationDPOS(1) & "\Dpos.ini")
            System.Threading.Thread.Sleep(2000)
            WriteToIni("DPOS-USER=" & EncryptString(sMySQLDPosPass), "DPOS-USER=", GetDatabaseLocationDPOS(1) & "\Dpos.ini")
            System.Threading.Thread.Sleep(2000)

            sMySQLRootPass = DecryptString(GetInfoFromIni("ROOT-USER", GetDatabaseLocationDPOS(1) & "\Dpos.ini", False))
            sMySQLDPosPass = DecryptString(GetInfoFromIni("DPOS-USER", GetDatabaseLocationDPOS(1) & "\Dpos.ini", False))
        End If

        If My.Computer.Info.OSFullName.ToString.Contains("Windows 7") = True Then
            Me.lblUserTypeStatus.Location = New Point(473, 63)
        Else
            Me.lblUserTypeStatus.Location = New Point(483, 63)
        End If

        CurrentStep(iCurrentStep)
        If Debugger.IsAttached Then
            'myMsgBox(sDatabaseLocation, "", myMsgBoxDisplay.OkOnly)
            'myMsgBox(sProgramLocation, "", myMsgBoxDisplay.OkOnly)
            'myMsgBox(sMySQLRootPass, "Root Pass", myMsgBoxDisplay.OkOnly)
            'myMsgBox(sMySQLDPosUser, "Dpos user", myMsgBoxDisplay.OkOnly)
            'myMsgBox(sMySQLDPosPass, "Dpos pass", myMsgBoxDisplay.OkOnly)
            'myMsgBox(Application.StartupPath, "", myMsgBoxDisplay.OkOnly)
            'myMsgBox(bDemo, "", myMsgBoxDisplay.OkOnly)

            'If myMsgBox("Yes No", "Test", myMsgBoxDisplay.YesNo) = DialogResult.OK Then
            '    myMsgBox("You chose Yes", "Test", myMsgBoxDisplay.OkOnly)
            'Else
            '    myMsgBox("You chose No", "Test", myMsgBoxDisplay.OkOnly)
            'End If
            Me.Button1.Visible = True
        End If

    End Sub
    Private Sub frmMySQLDPosInstall_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        frmInitialize()
    End Sub

#Region "Button Events"
    Private Sub btnNextStep_Click(sender As Object, e As EventArgs) Handles btnNextStep.Click

        If iCurrentStep - 1 = AppStepProcess.IMPORTDATACONFIG AndAlso ValidateDataImport() = False Then
            Exit Sub
        End If

        If iCurrentStep - 1 = AppStepProcess.INSTALLDPOSCONFIG AndAlso ValidateDPosInstaller() = False Then
            Exit Sub
        End If

        ImportOptionsAndInstallBtnEnabled(False)

        If Me.btnNextStep.Text = "EXECUTE" Then
            Me.btnNextStep.Text = "NEXT"
        End If

        If iCurrentStep = AppStepProcess.DONE Then
            ExitProgram()
        ElseIf iCurrentStep = AppStepProcess.FAILED Then
            ExitProgram()
        ElseIf iCurrentStep = AppStepProcess.START Then
            MyProgress = 0
            myReportProgressBar(MyProgress)

            Me.DataGridView1.Rows.Clear()

            iCurrentStep += 1
        End If

        CurrentStep(iCurrentStep)

        ImportOptionsAndInstallBtnEnabled(True)
    End Sub

    Private Sub btnMySQLInstall_Click(sender As Object, e As EventArgs) Handles btnMySQLInstall.Click

        Application.DoEvents()

        If IsMySQLServerInstalled() = 1 Then
            myMsgBox("MySQL Server is already installed", "MySQL DPos Install and Data Import", myMsgBoxDisplay.OkOnly)
            Exit Sub
        End If

        Try
            If bDoAnyBtnEvents = True Then
                bDoAnyBtnEvents = False
                ImportOptionsAndInstallBtnEnabled(False)
                MySQLInstall()
                ImportOptionsAndInstallBtnEnabled(True)
                bDoAnyBtnEvents = True     'will revert to True after the execution of the event so that other buttons will be functionable again
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub btnAddDposUser_Click(sender As Object, e As EventArgs) Handles btnAddDposUser.Click

        Application.DoEvents()

        If IsMySQLServerInstalled() = 0 Then
            myMsgBox("MySQL Server is not yet installed", "Please Install MySQL Server", myMsgBoxDisplay.OkOnly)
            Exit Sub
        End If

        Try
            Dim srvcService As New ServiceController("MySQL57")

            If srvcService.Status.Equals(ServiceControllerStatus.Stopped) Or srvcService.Status.Equals(ServiceControllerStatus.StopPending) Then
                Try
                    srvcService.Start()
                    System.Threading.Thread.Sleep(1000)
                Catch ex As Exception
                    myMsgBox("MySQL57 service is not started. Please start the service first in Control Panel->Administrative Tools->Services before opening DPos MySQL Install.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    Exit Sub
                End Try
            End If
        Catch ex As Exception

        End Try

        Try
            If bDoAnyBtnEvents = True Then
                bDoAnyBtnEvents = False
                ImportOptionsAndInstallBtnEnabled(False)
                AddDposUser()
                ImportOptionsAndInstallBtnEnabled(True)
                bDoAnyBtnEvents = True
            End If
        Catch ex As Exception
            ImportOptionsAndInstallBtnEnabled(True)
            bDoAnyBtnEvents = True
        End Try
    End Sub

    Private Sub btnAddSchemas_Click(sender As Object, e As EventArgs) Handles btnAddSchemas.Click

        Application.DoEvents()

        If IsMySQLServerInstalled() = 0 Then
            myMsgBox("MySQL Server is not yet installed", "Please Install MySQL Server", myMsgBoxDisplay.OkOnly)
            Exit Sub
        End If

        Try
            If bDoAnyBtnEvents = True And ValidateImportOption() = True Then
                bDoAnyBtnEvents = False
                ImportOptionsAndInstallBtnEnabled(False)
                AddSchemasTables()
                ImportOptionsAndInstallBtnEnabled(True)
                bDoAnyBtnEvents = True
            End If
        Catch ex As Exception
            ImportOptionsAndInstallBtnEnabled(True)
            bDoAnyBtnEvents = True
        End Try
    End Sub

    Private Sub btnImportMStoMY_Click(sender As Object, e As EventArgs) Handles btnImportMStoMY.Click

        Application.DoEvents()

        If IsMySQLServerInstalled() = 0 Then
            myMsgBox("MySQL Server is not yet installed", "Please Install MySQL Server", myMsgBoxDisplay.OkOnly)
            Exit Sub
        End If

        Try
            If bDoAnyBtnEvents = True AndAlso ValidateDataImport() = True Then
                bDoAnyBtnEvents = False
                ImportOptionsAndInstallBtnEnabled(False)
                ImportData()
                bCloudEnabled = isCloudSyncEnabled()
                'WriteToIni("MYSQL-TRIGGERS=", "MYSQL-TRIGGERS=", GetDatabaseLocationDPOS(1) & "\cloud.ini")
                If bCloudEnabled = True Then
                    myMsgBox("Please open DPOSCloudSync to initialize for the first time before running DPos to avoid further problems", "Import Data Completed", 1)
                End If
                MyProgress += 5
                myReportProgressBar(MyProgress)
                ImportOptionsAndInstallBtnEnabled(True)
                bDoAnyBtnEvents = True
            End If
        Catch ex As Exception
            ImportOptionsAndInstallBtnEnabled(True)
            bDoAnyBtnEvents = True
        End Try
    End Sub

    Private Sub btnDropTablesDPosUser_Click(sender As Object, e As EventArgs) Handles btnDropTablesDPosUser.Click

        Application.DoEvents()

        If IsMySQLServerInstalled() = 0 Then
            myMsgBox("MySQL Server is not yet installed", "Please Install MySQL Server", myMsgBoxDisplay.OkOnly)
            Exit Sub
        End If

        Try
            Dim srvcService As New ServiceController("MySQL57")

            If srvcService.Status.Equals(ServiceControllerStatus.Stopped) Or srvcService.Status.Equals(ServiceControllerStatus.StopPending) Then
                Try
                    srvcService.Start()
                    System.Threading.Thread.Sleep(1000)
                Catch ex As Exception
                    myMsgBox("MySQL57 service is not started. Please start the service first in Control Panel->Administrative Tools->Services before opening DPos MySQL Install.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    Exit Sub
                End Try
            End If
        Catch ex As Exception

        End Try

        Try
            If bDoAnyBtnEvents = True Then
                bDoAnyBtnEvents = False
                ImportOptionsAndInstallBtnEnabled(False)
                DropAllUsersTablesAndSchemas()
                ImportOptionsAndInstallBtnEnabled(True)
                bDoAnyBtnEvents = True
            End If
        Catch ex As Exception
            ImportOptionsAndInstallBtnEnabled(True)
            bDoAnyBtnEvents = True
        End Try
    End Sub

    Private Sub btnInstallDPos_Click(sender As Object, e As EventArgs) Handles btnInstallDPos.Click
        Application.DoEvents()

        Try
            If bDoAnyBtnEvents = True AndAlso ValidateDPosInstaller() = True Then
                bDoAnyBtnEvents = False
                ImportOptionsAndInstallBtnEnabled(False)
                DPosInstaller()
                ImportOptionsAndInstallBtnEnabled(True)
                bDoAnyBtnEvents = True
            End If
        Catch ex As Exception
            ImportOptionsAndInstallBtnEnabled(True)
            bDoAnyBtnEvents = True
        End Try
    End Sub

    Private Sub btnChangeDPosDatabaseType1_Click(sender As Object, e As EventArgs)
        Application.DoEvents()

        Try
            If bDoAnyBtnEvents = True Then
                bDoAnyBtnEvents = False
                ImportOptionsAndInstallBtnEnabled(False)
                If WriteDatabaseType("1") = True Then
                    myMsgBox("DPos Database setting has been set to MySQL", "Rewrite DPos.ini - SUCCESS", myMsgBoxDisplay.OkOnly)
                Else
                    myMsgBox("Error on rewriting DPos.ini", "Rewrite DPos.ini - FAILED", myMsgBoxDisplay.OkOnly)
                End If
                ImportOptionsAndInstallBtnEnabled(True)
                bDoAnyBtnEvents = True
            End If
        Catch ex As Exception
            ImportOptionsAndInstallBtnEnabled(True)
            bDoAnyBtnEvents = True
        End Try
    End Sub

    Private Sub btnChangeDPosDatabaseType0_Click(sender As Object, e As EventArgs)
        Application.DoEvents()

        Try
            If bDoAnyBtnEvents = True Then
                bDoAnyBtnEvents = False
                ImportOptionsAndInstallBtnEnabled(False)
                If WriteDatabaseType("0") = True Then
                    myMsgBox("DPos Database setting has been set to MSSERVER", "DPos.ini", myMsgBoxDisplay.OkOnly)
                Else
                    myMsgBox("Error on rewriting DPos.ini", "Rewrite DPos.ini - FAILED", myMsgBoxDisplay.OkOnly)
                End If
                ImportOptionsAndInstallBtnEnabled(True)
                bDoAnyBtnEvents = True
            End If
        Catch ex As Exception
            myMsgBox("ReWriteSecureLocPriv" & vbCrLf & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try
    End Sub

    Private Sub btnRestoreCurrent_Click(sender As Object, e As EventArgs) Handles btnRestoreCurrent.Click
        Application.DoEvents()

        If IsMySQLServerInstalled() = 0 Then
            myMsgBox("MySQL Server is not yet installed", "Please Install MySQL Server", myMsgBoxDisplay.OkOnly)
            Exit Sub
        End If

        If ValidateBackupCurrentMySQLDB() = False Then
            Exit Sub
        End If

        Try
            If bDoAnyBtnEvents = True Then
                bDoAnyBtnEvents = False
                ImportOptionsAndInstallBtnEnabled(False)
                BackupMySQLDB(Me.txtBackupPath.Text)
                ImportOptionsAndInstallBtnEnabled(True)
                bDoAnyBtnEvents = True
            End If
        Catch ex As Exception

        End Try
    End Sub
#End Region

#Region "Button Functions"
    Public Sub MySQLInstall()

        Application.DoEvents()

        'Clears the Work logs, if advanced user 
        If StepByStep = False Then
            Me.DataGridView1.Rows.Clear()
            MyProgress = 0
            Me.bwMySQLInstall.ReportProgress(MyProgress)
            'myReportProgressBar(MyProgress)
        End If

        WritetoLog("Check if MySQL is installed...", "")

        If IsMySQLServerInstalled() = 0 Then
            'If MySQL is not yet installed 

            'crm3486 - MySQL Server Install rework
            'MySQL Server will be silently installed through MySQL Installer Console
            'the app wont be needing to open the MySQL Community
            ReWritetoLog("Check if MySQL was installed", "")

            WritetoLog("MySQL is not yet avaiable.", "")
            WritetoLog("Installing MySQL Server 5.7.22", "")
            If bDemo = True Then
                WritetoLog("WARNING: PLEASE DO NOT CLOSE COMMAND PROMPT", "")
            End If

            If StepByStep = False Then
                MyProgress += 10
            Else
                MyProgress += 3
            End If
            myReportProgressBar(MyProgress)

            Dim bStopper As Boolean = False

            If MySQLServerSilentInstall() = False Then
                iCurrentStep = AppStepProcess.FAILED

                If StepByStep = False Then
                    MyProgress = 100
                    myReportProgressBar(MyProgress)
                Else
                    MyProgress = 100
                    myReportProgressBar(MyProgress)
                End If

                bStopper = True
            End If

            'check if MySQL was installed properly
            'loops until the bstopper flagged as true
            Do While bStopper = False
                If CheckInitialConnectionNoLog() = True Then
                    WritetoLog("MySQL Server has been installed", "")

                    If StepByStep = False Then
                        MyProgress += 80
                        myReportProgressBar(MyProgress)
                    Else
                        MyProgress += 29
                        myReportProgressBar(MyProgress)
                    End If

                    'test connection goes here
                    If CheckInitialConnection() = True Then
                        System.Threading.Thread.Sleep(10000)

                        'no need to rewrite the my.ini file
                        WritetoLog("Installing MySQL has been completed", "")
                        iCurrentStep += 1

                    Else
                        WritetoLog("Installing MySQL has been failed", "")

                        If StepByStep = True Then
                            WritetoLog("Other Process will not be executed", "")
                        End If

                        MyProgress = 100
                        myReportProgressBar(MyProgress)

                        iCurrentStep = AppStepProcess.FAILED
                    End If

                    If StepByStep = False Then
                        MyProgress += 10
                        myReportProgressBar(MyProgress)
                    Else
                        MyProgress += 3
                        myReportProgressBar(MyProgress)
                    End If

                    bStopper = True

                End If
            Loop

        Else

            'If MySQL is already installed 
            ReWritetoLog("Check if MySQL was installed", "")
            WritetoLog("MySQL is already installed", "")

            If StepByStep = False Then
                MyProgress += 90
                myReportProgressBar(MyProgress)
            Else
                MyProgress += 32
                myReportProgressBar(MyProgress)
            End If

            'test connection goes here
            If CheckInitialConnection() = True Then
                System.Threading.Thread.Sleep(10000)

                If ReWriteSecureLocPriv() = False Then
                    WritetoLog("Installing MySQL has been failed", "")

                    If StepByStep = True Then
                        WritetoLog("Other Process will not be executed", "")
                    End If

                    MyProgress = 100
                    myReportProgressBar(MyProgress)

                    iCurrentStep = AppStepProcess.FAILED
                Else
                    'goes to the next step if succeeded
                    iCurrentStep += 1

                    If StepByStep = False Then
                        MyProgress += 10
                        myReportProgressBar(MyProgress)
                    Else
                        MyProgress += 3
                        myReportProgressBar(MyProgress)
                    End If
                End If
            Else
                'will cancel the whole process if it didn't get through the test
                iCurrentStep = AppStepProcess.FAILED

                If StepByStep = True Then
                    WritetoLog("Other Process will not be executed", "")
                End If

                MyProgress = 100
                myReportProgressBar(MyProgress)
            End If

        End If

        DeleteAppShortcut()

        If StepByStep Then
            WritetoLog("", "")
            WritetoLog("Click [NEXT] to proceed", "")
        End If

        If iCurrentStep = AppStepProcess.FAILED Then
            CurrentStep(iCurrentStep)
        End If
    End Sub

    Public Function MySQLServerSilentInstall() As Boolean
        Dim bReturn As Boolean = False
        Dim sCommand As String = ""
        Dim bStatusTransferDataFiles As Boolean

        Application.DoEvents()

        sCommand = BuildMySQLInstallCommandString(sMySQLRootPass).ToString

        WritetoLog("Configuring option file(my.ini) and MySQL data directory...", "")
        bStatusTransferDataFiles = MySQLTransferMySQLDataFiles()

        If bStatusTransferDataFiles AndAlso sCommand <> "" Then
            ReWritetoLog("Configuring option file(my.ini) and data directory", "SUCCESS")
            myCMDShellExecute("echo Installing MySQL Server 5.7.22&&echo Please do not close this prompt. This prompt will be automatically close after the procedure&&" & sCommand & "&&exit")

            WritetoLog("Installing MySQL Server 5.7.22...", "")
            'to do find mysql server
            While IsMySQLServerInstalled() = 0

            End While

            'waits 45 seconds
            System.Threading.Thread.Sleep(45000)

            ReWritetoLog("Installing MySQL Server 5.7.22", "SUCCESS")

            WritetoLog("Cleaning MySQL data directory...", "")

            Try
                If Directory.Exists("C:\ProgramData\MySQL\MySQL Server 5.7\Data\mysql") Then Directory.Delete("C:\ProgramData\MySQL\MySQL Server 5.7\Data\mysql", True)

                While Directory.Exists("C:\ProgramData\MySQL\MySQL Server 5.7\Data\mysql")

                End While

                If Directory.Exists("C:\ProgramData\MySQL\MySQL Server 5.7\Data\performance_schema") Then Directory.Delete("C:\ProgramData\MySQL\MySQL Server 5.7\Data\performance_schema", True)

                While Directory.Exists("C:\ProgramData\MySQL\MySQL Server 5.7\Data\performance_schema")

                End While

                If Directory.Exists("C:\ProgramData\MySQL\MySQL Server 5.7\Data\sys") Then Directory.Delete("C:\ProgramData\MySQL\MySQL Server 5.7\Data\sys", True)

                While Directory.Exists("C:\ProgramData\MySQL\MySQL Server 5.7\Data\sys")

                End While

                If Directory.Exists("C:\ProgramData\MySQL\MySQL Server 5.7\Data\test") Then Directory.Delete("C:\ProgramData\MySQL\MySQL Server 5.7\Data\test", True)

                While Directory.Exists("C:\ProgramData\MySQL\MySQL Server 5.7\Data\test")

                End While

                ReWritetoLog("Cleaning MySQL data directory", "SUCCESS")
            Catch ex As Exception
                ReWritetoLog("Cleaning MySQL data directory", "FAILED")
                Return False
            End Try

            WritetoLog("Install MySQL57 service and initializing MySQL Server...", "")
            myCMDShellExecute("echo Please do not close this prompt&&c:&&cd ""C:\Program Files\MySQL\MySQL Server 5.7\bin""&&mysqld.exe --install MySQL57 --defaults-file=""C:\ProgramData\MySQL\MySQL Server 5.7\Data\my.ini""&&mysqld.exe --initialize-insecure --basedir=""C:\ProgramData\MySQL\MySQL Server 5.7"" --datadir=""C:\ProgramData\MySQL\MySQL Server 5.7\Data\Data""&&net start MySQL57&&echo You can now close this prompt&&pause&&exit")

            'to do change dword mysql57 service
            'change DWORD ObjectName "NT AUTHORITY\NetworkService"

            ReWritetoLog("Install MySQL57 service and initializing MySQL Server", "SUCCESS")

            'waiting until service is fully installed
            Dim isServiceOk As Boolean = False

            While isServiceOk = False
                Try
                    Dim sServiceStatus As String
                    Dim mysqlservice As New ServiceController("MySQL57")
                    sServiceStatus = mysqlservice.Status

                    'wait for service to be created
                    While sServiceStatus = Nothing
                        sServiceStatus = mysqlservice.Status
                    End While

                    Do While sServiceStatus <> 4
                        Dim mysqlservice2 As New ServiceController("MySQL57")
                        sServiceStatus = mysqlservice2.Status
                    Loop

                    isServiceOk = True
                Catch ex As Exception

                End Try
            End While

            ProcessMySQL("ALTER USER 'root'@'localhost' identified by '" & sMySQLRootPass & "'", "server=localhost; user id=root; Connect Timeout=10000;SslMode=none")

            bReturn = True

        Else
            ReWritetoLog("Configuring option file(my.ini) and data directory", "FAILED")
            bReturn = False
        End If

        Return bReturn
    End Function

    Public Function MySQLTransferMySQLDataFiles() As Boolean
        Dim bReturn As Boolean = False

        Try
            If Directory.Exists("C:\ProgramData\MySQL") Then
                'removes Uploads folder that prevents the app from deleting MySQL folder on ProgramData
                AddPermissionOnDirectory("C:\ProgramData\MySQL")
                DeleteDirectory("C:\ProgramData\MySQL")
                System.Threading.Thread.Sleep(2000)
            End If

            Directory.CreateDirectory("C:\ProgramData\MySQL\MySQL Server 5.7")

            CopyDirectory(sProgramLocation & "\MySQL Server 5.7\", "C:\ProgramData\MySQL\MySQL Server 5.7")

            System.Threading.Thread.Sleep(2000)

            AddPermissionOnDirectory("C:\ProgramData\MySQL\MySQL Server 5.7")

            bReturn = True
        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Function AddPermissionOnDirectory(ByVal sPath As String) As Boolean
        Dim bReturn As Boolean = False

        Try
            Dim sMyComputerUser As String = "USERS"

            Dim FolderInfo As IO.DirectoryInfo = New IO.DirectoryInfo(sPath)
            Dim FolderAcl As New DirectorySecurity

            FolderAcl = FolderInfo.GetAccessControl


            FolderAcl.AddAccessRule(New FileSystemAccessRule(sMyComputerUser, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow))
            FolderAcl.AddAccessRule(New FileSystemAccessRule(sMyComputerUser, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Allow))
            FolderInfo.SetAccessControl(FolderAcl)

            bReturn = True

        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Function AddPermissionOnFile(ByVal sPath As String) As Boolean
        Dim bReturn As Boolean = False

        Try
            Dim sMyComputerUser As String = My.Computer.Name & "\Users"

            Dim myFileInfo As IO.FileInfo = New IO.FileInfo(sPath)
            Dim myFileAcl As FileSecurity = myFileInfo.GetAccessControl

            myFileAcl.AddAccessRule(New FileSystemAccessRule(sMyComputerUser, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow))
            myFileInfo.SetAccessControl(myFileAcl)

            bReturn = True

        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Private Shared Sub CopyDirectory(ByVal sourcePath As String, ByVal destPath As String)
        If Not Directory.Exists(destPath) Then
            Directory.CreateDirectory(destPath)
        End If

        For Each dirFiles As String In Directory.GetFiles(sourcePath)
            Dim dest As String = Path.Combine(destPath, Path.GetFileName(dirFiles))
            File.Copy(dirFiles, dest)
        Next

        For Each folder As String In Directory.GetDirectories(sourcePath)
            Dim dest As String = Path.Combine(destPath, Path.GetFileName(folder))
            CopyDirectory(folder, dest)
        Next
    End Sub

    Private Shared Sub DeleteDirectory(ByVal sourcePath As String)
        If Directory.Exists(sourcePath) Then
            For Each dirFiles As String In Directory.GetFiles(sourcePath)
                File.Delete(dirFiles)
            Next

            For Each folder As String In Directory.GetDirectories(sourcePath)
                DeleteDirectory(folder)
            Next

            Directory.Delete(sourcePath)
        End If
    End Sub

    Public Function myCMDShellExecute(ByVal sCommand As String) As Boolean
        Dim bReturn As Boolean = False
        Dim objShell = CreateObject("Shell.Application")

        Try
            If bDemo = True Then
                If (My.Computer.Info.OSFullName.ToString.Contains("Vista") = True) Or (My.Computer.Info.OSFullName.ToString.Contains("Windows 7") = True) Or (My.Computer.Info.OSFullName.ToString.Contains("Windows 10") = True) Or (My.Computer.Info.OSFullName.ToString.Contains("Windows 8") = True) Then
                    objShell.ShellExecute("cmd.exe", "/k " & sCommand, "", "runas", 1)
                Else
                    objShell.ShellExecute("cmd.exe", "/k " & sCommand, "", "", 1)
                End If
            Else
                If (My.Computer.Info.OSFullName.ToString.Contains("Vista") = True) Or (My.Computer.Info.OSFullName.ToString.Contains("Windows 7") = True) Or (My.Computer.Info.OSFullName.ToString.Contains("Windows 10") = True) Or (My.Computer.Info.OSFullName.ToString.Contains("Windows 8") = True) Then
                    objShell.ShellExecute("cmd.exe", "/k " & sCommand, "", "runas", 1)
                Else
                    objShell.ShellExecute("cmd.exe", "/k " & sCommand, "", "", 1)
                End If
            End If

            bReturn = True
        Catch ex As Exception
            myMsgBox("myCMDShellExecute: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bReturn
    End Function

    Public Function BuildMySQLInstallCommandString(ByVal sPassword As String) As StringBuilder
        Dim sReturn As New StringBuilder
        Dim sComputerOS As String = ""

        Try
            sReturn.Clear()
            sReturn.Append("ECHO ON&&cls&&")
            If Environment.Is64BitOperatingSystem Then
                sReturn.Append("msiexec /i """ & sProgramLocation & "\mysql-5.7.22-winx64.msi"" /qn ACTION=INSTALL EXECUTEACTION=INSTALL INSTALLLEVEL=3 WixUI_InstallMode=InstallTypical")
            Else
                sReturn.Append("msiexec /i """ & sProgramLocation & "\mysql-5.7.22-win32.msi"" /qn ACTION=INSTALL EXECUTEACTION=INSTALL INSTALLLEVEL=3 WixUI_InstallMode=InstallTypical")
            End If
        Catch ex As Exception

        End Try

        Return sReturn
    End Function

    Public Function CheckInitialConnection() As Boolean
        Dim bReturn As Boolean = False
        Dim MYCN As New MySqlConnection

        Application.DoEvents()

        WritetoLog("Testing MySQL Connection...", "")

        MYCN.ConnectionString = GetConnectionStringMY("")
        If MYDBOpen(MYCN) = True Then
            MYCN.Close()
            bReturn = True
            ReWritetoLog("Testing MySQL Connection", "")
            WritetoLog("Successfully connected to MySQL Server", "")
        Else
            ReWritetoLog("Testing MySQL Connection", "")
            WritetoLog("Failed to connect MySQL Server", "")
        End If

        Return bReturn
    End Function

    Public Function CheckInitialConnectionNoLog() As Boolean
        Dim bReturn As Boolean = False
        Dim MYCN As New MySqlConnection

        Application.DoEvents()
        Try
            MYCN.ConnectionString = GetConnectionStringMY("")
            If MYDBOpenNoReport(MYCN) = True Then
                MYCN.Close()
                bReturn = True
            End If
        Catch ex As Exception
            bReturn = False
        End Try

        Return bReturn
    End Function

    Public Function isMySQLInstallerRunning() As Boolean
        Dim bReturn As Boolean = False

        For Each thisProcess As Process In Process.GetProcesses(".")
            If thisProcess.MainWindowTitle.ToString = "MySQL Installer" Then
                bReturn = True
                Exit For
            End If
        Next

        Return bReturn
    End Function

    Public Function ReWriteSecureLocPriv() As Boolean
        Dim bReturn As Boolean = False
        Dim myINILoc As String

        Try
            WritetoLog("", "")
            WritetoLog("MySQL my.ini configuration for DPOS has been started", "")
            'Locate inifile
            myINILoc = LocateMyIni()

            'Stop service
            WritetoLog("Stopping MySQL Service...", "")

            If SwitchMySQLServerService(False) = True Then
                ReWritetoLog("Stopping MySQL Service", "SUCCESS")
            Else
                ReWritetoLog("Stopping MySQL Service", "FAILED")
                Return bReturn
            End If

            WritetoLog("Locating my.ini file on MySQL Server...", "")
            If myINILoc = "ERROR" Then
                ReWritetoLog("Locating my.ini file on MySQL Server", "FAILED")
                WritetoLog("Error on Locating MySQL my.ini file", "")

                WritetoLog("Starting MySQL Service...", "")
                If SwitchMySQLServerService(True) = True Then
                    ReWritetoLog("Starting MySQL Service", "SUCCESS")
                Else
                    ReWritetoLog("Starting MySQL Service", "FAILED")
                End If

                Return bReturn
            Else
                ReWritetoLog("Locating my.ini file on MySQL Server", "SUCCESS")
            End If

            'check if inifile exist, edit my.ini
            WritetoLog("Configuring my.ini file...", "")
            If myINIExist(myINILoc & "my.ini") Then
                'edit my.ini file
                AddPermissionOnFile(myINILoc & "my.ini")

                System.Threading.Thread.Sleep(2000)

                If WriteToIni("secure-file-priv=""""", "secure-file-priv=""", myINILoc & "my.ini") Then
                    ReWritetoLog("Configuring my.ini file", "SUCCESS")
                Else
                    ReWritetoLog("Configuring my.ini file", "FAILED")
                    WritetoLog("Error on configuring my.ini file", "")

                    WritetoLog("Starting MySQL Service...", "")
                    If SwitchMySQLServerService(True) = True Then
                        ReWritetoLog("Starting MySQL Service", "SUCCESS")
                    Else
                        ReWritetoLog("Starting MySQL Service", "FAILED")
                    End If

                    Return bReturn
                End If
            Else
                ReWritetoLog("Configuring my.ini file", "FAILED")
                WritetoLog("Cannot find MySQL my.ini file", "")
            End If

            System.Threading.Thread.Sleep(2000)

            'Start Service
            WritetoLog("Starting MySQL Service...", "")
            If SwitchMySQLServerService(True) = True Then
                ReWritetoLog("Starting MySQL Service", "SUCCESS")
            Else
                ReWritetoLog("Starting MySQL Service", "FAILED")
                Return bReturn
            End If

            bReturn = True
        Catch ex As Exception
            myMsgBox("ReWriteSecureLocPriv" & vbCrLf & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            'do nothing
        End Try

        Return bReturn
    End Function

    ''' <summary>
    ''' Restarts MySQL Service
    ''' </summary>
    Public Function SwitchMySQLServerService(ByVal bSwitch As Boolean) As Boolean
        Dim bReturn As Boolean = False
        'Dim mysqlservice As New ServiceController("MSSQLSERVER")
        Dim mysqlservice As New ServiceController("MySQL57")

        Try
            'throws an exception whenever i stop/start the service using vb.net
            'and won't let me access to stop/start the service without admin credentials
            'mysqlservice.Stop()
            'mysqlservice.WaitForStatus(ServiceControllerStatus.Stopped)
            'mysqlservice.Start()

            'so i control the service using batch files and run them
            If bSwitch = False Then
                'RunBatFileAs(sProgramLocation & "\MySQLServiceStop.bat")
                myCMDShellExecute("net stop MySQL57&&exit")
                Dim sServiceStatus As String

                sServiceStatus = mysqlservice.Status

                Do While sServiceStatus <> 1
                    Dim mysqlservice2 As New ServiceController("MySQL57")
                    sServiceStatus = mysqlservice2.Status
                Loop

                System.Threading.Thread.Sleep(2000)
            Else
                'RunBatFileAs(sProgramLocation & "\MySQLServiceStart.bat")
                myCMDShellExecute("net start MySQL57&&exit")
                Dim sServiceStatus As String

                sServiceStatus = mysqlservice.Status

                Do While sServiceStatus <> 4
                    Dim mysqlservice2 As New ServiceController("MySQL57")
                    sServiceStatus = mysqlservice2.Status
                Loop

                System.Threading.Thread.Sleep(2000)
            End If

            bReturn = True
        Catch ex As Exception
            If bSwitch = True Then
                myMsgBox("Starting MySQL Service" & vbCrLf & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            Else
                myMsgBox("Closing MySQL Service" & vbCrLf & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If
        End Try

        Return bReturn
    End Function

    Public Function isAppRunning(ByVal sProcess) As Boolean
        Dim bReturn As Boolean = False

        For Each thisProcess As Process In Process.GetProcesses(".")
            If thisProcess.MainWindowTitle.ToString = "MySQLServiceStop.bat" Then
                bReturn = True
                Exit For
            End If
        Next

        Return bReturn
    End Function

    Public Function RunBatFileAs(ByVal sFile As String) As Boolean
        Dim bReturn As Boolean = False

        Dim procInfo As New ProcessStartInfo()

        Try
            procInfo.WindowStyle = ProcessWindowStyle.Normal
            procInfo.UseShellExecute = True
            procInfo.FileName = sFile
            procInfo.WorkingDirectory = ""
            If bDemo = False Then
                procInfo.CreateNoWindow = True
            End If

            If (My.Computer.Info.OSFullName.ToString.Contains("Vista") = True) Or
                   (My.Computer.Info.OSFullName.ToString.Contains("Windows 7") = True) Or (My.Computer.Info.OSFullName.ToString.Contains("Windows 10") = True) Or (My.Computer.Info.OSFullName.ToString.Contains("Windows 8") = True) Then

                procInfo.Verb = "runas"

            End If

            Process.Start(procInfo)

            bReturn = True
        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Function LocateMyIni() As String
        Dim sReturn As String = ""
        Dim sSQL As String

        Try
            sSQL = "SELECT @@datadir as MySQLDataLoc"
            sReturn = GetFieldMySQL(sSQL, GetConnectionStringMY(""), "MySQLDataLoc")
            If sReturn = "ERROR" Then
                Return sReturn
            End If

            sReturn = sReturn.Substring(0, Len(sReturn) - 5)
        Catch ex As Exception
            myMsgBox("Locate Ini File: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            sReturn = "ERROR"
        End Try

        Return sReturn
    End Function

    Public Sub AddDposUser()
        Dim sSQL As String

        Application.DoEvents()

        If StepByStep = False Then
            Me.DataGridView1.Rows.Clear()
            MyProgress = 0
            'Me.bwAddDPosUser.ReportProgress(MyProgress)
            myReportProgressBar(MyProgress)

            If IsMySQLServerInstalled() = 0 Then
                myMsgBox("MySQL Server is not yet installed on this computer", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                Exit Sub
            End If
        End If

        If Me.DataGridView1.Rows.Count > 1 Then
            WritetoLog("", "")
        End If

        WritetoLog("Adding DPOS User Credentials ...", "")

        Try
            sSQL = "DROP USER IF exists 'dpos'@'%'; " &
                   "CREATE USER IF NOT EXISTS 'dpos'@'%' IDENTIFIED BY '" & sMySQLDPosPass & "'; " &
                   "GRANT ALL PRIVILEGES ON * . * TO 'dpos'@'%'; " &
                   "DROP USER IF exists 'root'@'%'; " &
                   "CREATE USER IF NOT EXISTS 'root'@'%' IDENTIFIED BY '" & sMySQLRootPass & "'; " &
                   "GRANT ALL PRIVILEGES ON * . * TO 'root'@'%' WITH GRANT OPTION;"

            ProcessMySQL(sSQL, GetConnectionStringMY(""))

            ReWritetoLog("Adding DPOS User Credentials", "SUCCESS")

            If StepByStep = False Then
                'If DPosMySQLUserstoDB() = False Then
                'Exit Sub
                'End If
            End If

            iCurrentStep += 1
        Catch ex As Exception
            ReWritetoLog("Adding DPOS User Credentials", "FAILED")
            WritetoLog(ex.ToString, "")
            If StepByStep = True Then
                WritetoLog("Other Process will not be executed", "")
            End If

            iCurrentStep = AppStepProcess.FAILED
        End Try

        DeleteAppShortcut()

        If StepByStep Then
            WritetoLog("", "")
            WritetoLog("Click [NEXT] to proceed", "")
        End If

        If StepByStep = False Then
            MyProgress += 100
            'Me.bwAddDPosUser.ReportProgress(MyProgress)
            myReportProgressBar(MyProgress)
        Else
            MyProgress += 5
            'Me.bwStepByStep.ReportProgress(MyProgress)
            myReportProgressBar(MyProgress)
        End If

        If iCurrentStep = AppStepProcess.FAILED Then
            CurrentStep(iCurrentStep)
        End If
    End Sub

    Public Sub AddSchemasTables()
        Dim thisTableDetails As String()
        'use char_para to split the details
        'thisTableDetails(0) - Schema
        'thisTableDetails(1) - Table
        'thisTableDetails(2) - Columns
        Dim thisIndexDetails As String()
        Dim thisFKDetails As String()

        Application.DoEvents()

        If (Me.rbtnImportBakFile.Checked = True Or Me.rbtnImportDumpFile.Checked = True) And StepByStep = True Then
            WritetoLog("Creating Schemas and Tables is not available for this Import Option", "")

            If StepByStep = True Then
                WritetoLog("This operation will be skipped", "")
                MyProgress += 20
            Else
                MyProgress = 100
            End If

            'Me.bwStepByStep.ReportProgress(MyProgress)
            myReportProgressBar(MyProgress)

            iCurrentStep += 1
            GoTo thenextstep
        End If

        If StepByStep = False Then
            Me.DataGridView1.Rows.Clear()
            MyProgress = 0
            'Me.bwAddDPosSchemasAndTables.ReportProgress(MyProgress)
            myReportProgressBar(MyProgress)

            If IsMySQLServerInstalled() = 0 Then
                myMsgBox("MySQL Server is not yet installed on this computer", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                Exit Sub
            End If
        End If

        If Me.DataGridView1.Rows.Count > 1 Then
            WritetoLog("", "")
        End If

        WritetoLog("Creating Schemas has been started", "")

        For Each thisSchema In arDPosSchemas
            Dim sError As String = ""
            sError = CreateSchemas(thisSchema, True)

            System.Threading.Thread.Sleep(2000)

            If sError <> "" Then
                WritetoLog("Creating Schemas cancelled", "")

                If StepByStep = True Then
                    WritetoLog("Other Process will not be executed", "")
                End If

                MyProgress = 100
                myReportProgressBar(MyProgress)

                iCurrentStep = AppStepProcess.FAILED
                GoTo thenextstep
            End If

            If StepByStep = False Then
                MyProgress += 54 / arDPosSchemas.Count
                'Me.bwAddDPosSchemasAndTables.ReportProgress(MyProgress)
                myReportProgressBar(MyProgress)
            Else
                MyProgress += (20 * 0.54) / arDPosSchemas.Count
                'Me.bwStepByStep.ReportProgress(MyProgress)
                myReportProgressBar(MyProgress)
            End If
        Next

        WritetoLog("Creating Schemas Done", "")

        WritetoLog("", "")

        WritetoLog("Creating Tables has been started", "")

        For Each thisTable In arDPOSTables
            Dim sError As String = ""
            thisTableDetails = thisTable.split(CHAR_PARA)
            If thisTableDetails(1).ToLower <> "audit" Then      'skips the creation of audit table
                sError = CreateTable(thisTableDetails(1).ToLower, thisTableDetails(2), thisTableDetails(0), True)

                System.Threading.Thread.Sleep(2000)

                If sError <> "" Then
                    WritetoLog("Creating Tables cancelled", "")

                    If StepByStep = True Then
                        WritetoLog("Other Process will not be executed", "")
                    End If

                    MyProgress = 100
                    myReportProgressBar(MyProgress)

                    iCurrentStep = AppStepProcess.FAILED
                    CurrentStep(iCurrentStep)

                    Exit Sub
                End If

                If StepByStep = False Then
                    MyProgress += 46 / (arDPOSTables.Count - 1)
                Else
                    MyProgress += (20 * 0.46) / (arDPOSTables.Count - 1)
                End If

                'Me.bwAddDPosSchemasAndTables.ReportProgress(MyProgress)
                myReportProgressBar(MyProgress)
            End If
        Next

        WritetoLog("Creating Tables Done", "")

        WritetoLog("", "")

        WritetoLog("Creating Index has been started", "")

        For Each thisIndex In arDPOSIndex
            thisIndexDetails = thisIndex.split(CHAR_PARA)
            CreateMYSQLIndex(thisIndexDetails(0), thisIndexDetails(1), thisIndexDetails(2), True)
        Next

        WritetoLog("Creating Index Done", "")

        WritetoLog("", "")

        WritetoLog("Creating Foreign Keys has been started", "")

        For Each thisFK In arDPOSFKs
            thisFKDetails = thisFK.split(CHAR_PARA)
            'use char_para to split the details
            'thisTableDetails(0) - Schema
            'thisTableDetails(1) - Table
            'thisTableDetails(2) - Columns
            CreateMySQLFKs(thisFKDetails(0), thisFKDetails(1), thisFKDetails(2), thisFKDetails(3), thisFKDetails(4), thisFKDetails(5), True)
        Next

        WritetoLog("Creating Foreign Keys Done", "")

        If StepByStep = False Then
            WritetoLog("", "")
            WritetoLog("Creating UpdateCustomerSummary Stored Procedure...", "")

            If CheckIfSPExist("UpdateCustomerSummary", "deliveritsql") = False Then
                Dim sString As New System.Text.StringBuilder
                sString.Append(My.Resources.CreateSPUpdateCustomerSummaryMYSQL)

                If ProcessSQLforSPCreationMySQL(sString.ToString, GetConnectionStringMY("DeliveritSQL")) = False Then
                    ReWritetoLog("Creating UpdateCustomerSummary Stored Procedure", "FAILED")
                Else
                    ReWritetoLog("Creating UpdateCustomerSummary Stored Procedure", "SUCCESS")
                End If
            Else
                WritetoLog("Creating UpdateCustomerSummary Stored Procedure", "SUCCESS")
                WritetoLog("Stored Procedure is already created", "")
            End If
        End If

        'If DPosMySQLUserstoDB() = False Then
        'Exit Sub
        'End If

        iCurrentStep += 1

thenextstep:
        DeleteAppShortcut()
        CurrentStep(iCurrentStep)
    End Sub

    Public Function DPosMySQLUserstoDB() As Boolean
        Dim bReturn As Boolean = False

        If CreateDPosMySQLUsers() = False Then
            If StepByStep = True Then
                WritetoLog("Other Process will not be executed", "")
            End If

            MyProgress = 100
            myReportProgressBar(MyProgress)

            iCurrentStep = AppStepProcess.FAILED
            CurrentStep(iCurrentStep)

            Return bReturn
        End If

        bReturn = True

        Return bReturn
    End Function

    Public Sub ImportData()
        Dim sResult As String = ""
        Dim thisTableDetails As String()
        Dim thisIndexDetails As String()

        Dim arrFailedImportTables As New ArrayList

        Application.DoEvents()

        If StepByStep = False Then
            Me.DataGridView1.Rows.Clear()
            MyProgress = 0
            myReportProgressBar(MyProgress)

            If IsMySQLServerInstalled() = 0 Then
                myMsgBox("MySQL Server is not yet installed on this computer", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                Exit Sub
            End If
        End If

        Try
            'check if ImportDataLogs.txt exceeds more than 5mb. creates a backup file and creates a new backup file
            CreateBackupLogFiles(sDatabaseLocation & "\ImportDataLogs.txt", (1024 * 1024) * 5)

            LogDataImport("=== Migration Started ===" & vbCrLf & "Computer Name: " & My.Computer.Name & vbCrLf & "Date/Time " & DateTime.Now.ToString)
        Catch ex As Exception

        End Try

        If Me.rbtnImportFile.Checked = True Then
#Region "LOCAL MSSERVER"
            LogDataImport("Import Option: Local SQL Server")
            If Me.DataGridView1.Rows.Count > 1 Then
                WritetoLog("", "")
            End If

            arrFailedImportTables.Clear()

            WritetoLog("Importing Data from MSSERVER has been started", "")

            WritetoLog("", "")

            'If false or individual execution, the app will create schemas and tables needed for the datas that will be imported to the database in case all of the said requirements were dropped previously.
            If StepByStep = False Then
                If Not CreateSchemasForImport() Then
                    LogDataImport(vbCrLf & "Create Tables and Schemas: FAILED")
                    LogDataImport("=== Migration aborted ===" & vbCrLf & vbCrLf & vbCrLf)
                    MyProgress = 100
                    myReportProgressBar(MyProgress)

                    iCurrentStep = AppStepProcess.FAILED
                    CurrentStep(iCurrentStep)
                    Exit Sub
                Else
                    LogDataImport(vbCrLf & "Create Tables and Schemas: SUCCESS")
                End If
            End If

            'DB backup
            Dim bSuccessBackup As Boolean
            Dim sErrorMsg As String = ""
            bSuccessBackup = BackupMSSQLDB("", sErrorMsg)

            If bSuccessBackup = False Then
                Dim bSkippedBackup As DialogResult
                bSkippedBackup = myMsgBox("Auto backup current Local SQL Server Data has encountered an error during backup. " & vbCrLf & "Do you still want to proceed or stop import process?", "Local SQL Backup Failed", myMsgBoxDisplay.CustomYesNo, "PROCEED", "STOP")

                If bSkippedBackup = DialogResult.OK Then
                    LogDataImport(vbCrLf & "Auto backup current Local SQL Server Data: Failed" & vbCrLf & "Error: " & sErrorMsg & vbCrLf & "Time: " & DateTime.Now.ToString & vbCrLf & "The user decided to proceed the process")
                Else
                    If StepByStep = True Then
                        WritetoLog("Other Process will not be executed", "")
                    End If

                    MyProgress = 100
                    myReportProgressBar(MyProgress)

                    LogDataImport(vbCrLf & "Auto backup current Local SQL Server Data: Failed" & vbCrLf & "Error: " & sErrorMsg & vbCrLf & "Time: " & DateTime.Now.ToString & vbCrLf & vbCrLf & "=== Migration Aborted ===" & vbCrLf & vbCrLf & vbCrLf)

                    iCurrentStep = AppStepProcess.FAILED
                    CurrentStep(iCurrentStep)
                    Exit Sub
                End If
            Else
                LogDataImport(vbCrLf & "Auto backup current Local SQL Server Data: Success" & vbCrLf & "Time: " & DateTime.Now.ToString)
            End If

            sErrorMsg = ""

            WritetoLog("", "")
            WritetoLog("Performing Database Upgrade...", "")
            If DatabaseUpgrade(DatabaseType.MSSERVER) = False Then
                LogDataImport(vbCrLf & "Failed to perform Database upgrade" & vbCrLf & vbCrLf & "=== Migration Aborted ===" & vbCrLf & vbCrLf & vbCrLf)
                ReWritetoLog("Performing Database Upgrade", "FAILED")
                If StepByStep = True Then
                    WritetoLog("Other Process will not be executed", "")
                End If

                MyProgress = 100
                myReportProgressBar(MyProgress)

                iCurrentStep = AppStepProcess.FAILED
                CurrentStep(iCurrentStep)
                Exit Sub
            Else
                ReWritetoLog("Performing Database Upgrade", "SUCCESS")
            End If

            arDPOSTables.Sort()

            'Creation of Audit table if the table does exist on MSSERVER(for Cloud Sync Enabled DPos)
            WritetoLog("Check Audit table if existing in MSSERVER", "")
            Dim bAuditExist As Boolean

            bAuditExist = CheckIfDBTableExist("Audit", "DeliveritSQL", DatabaseType.MSSERVER)

            If bAuditExist = True Then
                WritetoLog("Audit table exist on MSSERVER", "")

                For Each sTableName In arDPOSTables
                    Dim sError As String = ""
                    thisTableDetails = sTableName.split(CHAR_PARA)

                    If thisTableDetails(1).ToLower = "audit" Then
                        sError = CreateTable(thisTableDetails(1).ToLower, thisTableDetails(2), thisTableDetails(0), True)
                        If sError <> "" Then
                            LogDataImport(vbCrLf & "Creating Audit Table Failed" & vbCrLf & "Error: " & sError & vbCrLf & vbCrLf & "=== Migration Aborted ===" & vbCrLf & vbCrLf & vbCrLf)
                            WritetoLog("Other Process will not be executed", "")

                            MyProgress = 100
                            myReportProgressBar(MyProgress)

                            iCurrentStep = AppStepProcess.FAILED
                            CurrentStep(iCurrentStep)
                            Exit Sub
                        Else
                            Exit For
                        End If
                    End If
                Next
            Else
                WritetoLog("Audit table does not exist on MSSERVER", "")
                WritetoLog("The application will not create the table", "")
            End If

            WritetoLog("", "")

            'Import SQL Data
            For Each sTableName In arDPOSTables
                thisTableDetails = sTableName.split(CHAR_PARA)

                If thisTableDetails(1) = "Audit" Then
                    If bAuditExist = True Then
                        sResult = ExportMSSERVERData(thisTableDetails(1), thisTableDetails(0))

                        If sResult = "ERROR" Then
                            'once sResult was flagged as ERROR the application will ask the user to skip the table or stop the import operation

                            Dim bSkipped As DialogResult

                            bSkipped = myMsgBox("Table " & thisTableDetails(1) & " has encountered an error during import/export. " & vbCrLf & "Do you still want to proceed or stop import process?", "Skip " & thisTableDetails(1) & " table", myMsgBoxDisplay.CustomYesNo, "PROCEED", "STOP")

                            If bSkipped = DialogResult.OK Then

                                arrFailedImportTables.Add(thisTableDetails(1))

                                'continue to the next table
                                If StepByStep = False Then
                                    MyProgress += 95 / arDPOSTables.Count
                                Else
                                    MyProgress += 25 / arDPOSTables.Count
                                End If
                                'Me.bwImportData.ReportProgress(MyProgress)
                                myReportProgressBar(MyProgress)
                            Else
                                'stop the whole process
                                MyProgress = 100
                                myReportProgressBar(MyProgress)

                                LogDataImport(vbCrLf & "=== Migration Aborted ===" & vbCrLf & vbCrLf & vbCrLf)
                                WritetoLog("Import Data has been cancelled", "")
                                WritetoLog("Check the Import Scripts and Logs on this location ", "")
                                WritetoLog(sDatabaseLocation & "\ImportDataLogs.txt", "")

                                If StepByStep = True Then
                                    WritetoLog("Following operations will be cancelled", "")
                                    iCurrentStep = AppStepProcess.FAILED
                                    CurrentStep(iCurrentStep)
                                End If
                                Exit Sub
                            End If
                        Else
                            If StepByStep = False Then
                                MyProgress += 95 / arDPOSTables.Count
                            Else
                                MyProgress += 25 / arDPOSTables.Count
                            End If
                            'Me.bwImportData.ReportProgress(MyProgress)
                            myReportProgressBar(MyProgress)
                        End If
                    Else
                        If StepByStep = False Then
                            MyProgress += 95 / arDPOSTables.Count
                        Else
                            MyProgress += 25 / arDPOSTables.Count
                        End If
                        myReportProgressBar(MyProgress)
                    End If
                Else
                    sResult = ExportMSSERVERData(thisTableDetails(1), thisTableDetails(0))

                    If sResult = "ERROR" Then
                        'once sResult was flagged as ERROR the application will ask the user to skip the table or stop the import operation

                        Dim bSkipped As DialogResult

                        bSkipped = myMsgBox("Table " & thisTableDetails(1) & " has encountered an error during import/export. " & vbCrLf & "Do you still want to proceed or stop import process?", "Skip " & thisTableDetails(1) & " table", myMsgBoxDisplay.CustomYesNo, "PROCEED", "STOP")

                        If bSkipped = DialogResult.OK Then

                            arrFailedImportTables.Add(thisTableDetails(1))

                            'continue to the next table
                            If StepByStep = False Then
                                MyProgress += 95 / arDPOSTables.Count
                            Else
                                MyProgress += 25 / arDPOSTables.Count
                            End If
                            'Me.bwImportData.ReportProgress(MyProgress)
                            myReportProgressBar(MyProgress)
                        Else
                            'stop the whole process
                            MyProgress = 100
                            myReportProgressBar(MyProgress)

                            LogDataImport(vbCrLf & "=== Migration Aborted ===" & vbCrLf & vbCrLf & vbCrLf)
                            WritetoLog("Import Data has been cancelled", "")
                            WritetoLog("Check the Import Scripts and Logs on this location ", "")
                            WritetoLog(sDatabaseLocation & "\ImportDataLogs.txt", "")

                            If StepByStep = True Then
                                WritetoLog("Following operations will be cancelled", "")
                                iCurrentStep = AppStepProcess.FAILED
                                CurrentStep(iCurrentStep)
                            End If
                            Exit Sub
                        End If
                    Else
                        If StepByStep = False Then
                            MyProgress += 95 / arDPOSTables.Count
                        Else
                            MyProgress += 25 / arDPOSTables.Count
                        End If
                        myReportProgressBar(MyProgress)
                    End If
                End If
            Next

            If arrFailedImportTables.Count <> 0 Then
                LogDataImport("Location: " & sDatabaseLocation & "\ImportDataLogs.txt" & vbCrLf & "Skipped Table(s):" & vbCrLf)
                WritetoLog("Importing Data has been completed with import errors", "")
                WritetoLog("", "")
                WritetoLog("Please edit CSV files of the following tables", "")
                WritetoLog("and import them manually to MySQL ", "")
                WritetoLog("Check the Import Scripts and Logs on this location ", "")
                WritetoLog(sDatabaseLocation & "\ImportDataLogs.txt", "")
                WritetoLog("", "")
                For Each thistables In arrFailedImportTables
                    WritetoLog(thistables.ToString, "")
                    LogDataImport(thistables.ToString)
                Next
                LogDataImport(vbCrLf & "=== Migration completed with import error(s) ===" & vbCrLf & vbCrLf & vbCrLf)
            Else
                LogDataImport("=== Migration completed without import errors ===" & vbCrLf & vbCrLf & vbCrLf)
                WritetoLog("Importing Data has been completed without any import errors", "")
            End If
#End Region
        ElseIf Me.rbtnImportDumpFile.Checked = True Then
#Region "SQL DUMP FILE"
            Dim sSQL As New StringBuilder
            Dim sProcessSQL As String
            Dim iTimeout As Integer = 2147483

            Dim myFileInfo As New FileInfo(Me.txtDumpFile.Text)
            Dim myFileSize As Long = myFileInfo.Length

            If myCInt(myFileSize) > 100000 Then
                iTimeout = myCInt(myFileSize) / 1000000
                iTimeout *= 1000000
            Else
                iTimeout = 2147483
            End If

            iTimeout = 0

            If Me.DataGridView1.Rows.Count > 1 Then
                WritetoLog("", "")
            End If

            WritetoLog("Importing Data from SQL Dump has been started", "")

            'drops all database
            WritetoLog("Droping all existing DPos Schemas...", "")
            Dim sDropsSQL As String = ""
            For Each sschemas In arDPosSchemas
                sDropsSQL = "DROP database if exists " & sschemas
                ProcessMySQL(sDropsSQL, GetConnectionStringMY(""))
            Next
            ReWritetoLog("Drops all existing DPos Schemas", "SUCCESS")

            ''set max_allowed_packet
            WritetoLog("Configuring max_allowed_packet...", "")
            Dim sSetMaxPacketsSQL As String = ""
            sSetMaxPacketsSQL = "SET GLOBAL max_allowed_packet=1024*1024*1024;"
            sProcessSQL = ProcessMySQL(sSetMaxPacketsSQL, GetConnectionStringMY(""), iTimeout)
            If sProcessSQL <> "" Then
                ReWritetoLog("Configuring max_allowed_packet", "FAILED")
                WritetoLog(sProcessSQL, "")
                WritetoLog("Importing Data has been cancelled", "")

                MyProgress = 100
                myReportProgressBar(MyProgress)

                If StepByStep = True Then
                    WritetoLog("Following operations will be cancelled", "")
                    iCurrentStep = AppStepProcess.FAILED
                    CurrentStep(iCurrentStep)
                End If

                Exit Sub
            Else
                ReWritetoLog("Configuring max_allowed_packet", "SUCCESS")
            End If

            'Import SQL dump 
            Try
                sSQL.Clear()
                WritetoLog("Executing Script from Dump File to MySQL...", "")
                sProcessSQL = ProcessMySQL(File.ReadAllText(Me.txtDumpFile.Text), GetConnectionStringMY(""), iTimeout)

                If sProcessSQL <> "" Then
                    ReWritetoLog("Executing Script from Dump File to MySQL", "FAILED")
                    WritetoLog(sProcessSQL, "")
                    WritetoLog("Importing Data has been cancelled", "")

                    MyProgress = 100
                    myReportProgressBar(MyProgress)

                    If StepByStep = True Then
                        WritetoLog("Following operations will be cancelled", "")
                        iCurrentStep = AppStepProcess.FAILED
                        CurrentStep(iCurrentStep)
                    End If

                    Exit Sub
                Else
                    ReWritetoLog("Executing Script from Dump File to MySQL", "SUCCESS")
                    WritetoLog("Importing SQL Dump File has been completed", "")

                    If StepByStep = False Then
                        MyProgress = 95
                    Else
                        MyProgress += 25
                    End If

                    myReportProgressBar(MyProgress)
                End If

                WritetoLog("", "")
                WritetoLog("Create MySQL Index", "")
                For Each thisIndex In arDPOSIndex
                    thisIndexDetails = thisIndex.split(CHAR_PARA)
                    CreateMYSQLIndex(thisIndexDetails(0), thisIndexDetails(1), thisIndexDetails(2), True)
                Next

            Catch ex As Exception
                ReWritetoLog("Executing Script from Dump File to MySQL", "FAILED")
                WritetoLog("Unable to read SQL file", "")
                WritetoLog(ex.Message, "")
                WritetoLog("Importing Data has been cancelled", "")

                MyProgress = 100
                myReportProgressBar(MyProgress)

                If StepByStep = True Then
                    WritetoLog("Following operations will be cancelled", "")
                    iCurrentStep = AppStepProcess.FAILED
                    CurrentStep(iCurrentStep)
                End If

                Exit Sub
            End Try
#End Region
        ElseIf Me.rbtnImportBakFile.Checked = True Then
#Region "RESTORE FILE"
            Dim iArrayCount As Double = 0

            iArrayCount = arrRstBak.Count

            arrFailedImportTables.Clear()

            Dim sArrRstBakToLog As String = ""

            For Each thisArrRstBak As String In arrRstBak
                If sArrRstBakToLog <> "" Then sArrRstBakToLog &= ", "
                sArrRstBakToLog &= thisArrRstBak
            Next

            LogDataImport("Import Option: DPos MySQL Restore File. (" & sArrRstBakToLog & ")")

            If Directory.Exists(sDatabaseLocation & "\ImportfromDPOSRestore") Then
                Directory.Delete(sDatabaseLocation & "\ImportfromDPOSRestore", True)
            End If

            If Me.DataGridView1.Rows.Count > 1 Then
                WritetoLog("", "")
            End If

            WritetoLog("Importing Data from DPos MySQL Restore File has been started", "")

            'set max_allowed_packet
            WritetoLog("Configuring max_allowed_packet...", "")
            Dim sSetMaxPacketsSQL As String = ""
            Dim sSetMaxPacketProcess As String = ""
            sSetMaxPacketsSQL = "SET GLOBAL max_allowed_packet=1024*1024*1024;"
            sSetMaxPacketProcess = ProcessMySQL(sSetMaxPacketsSQL, GetConnectionStringMY(""))
            If sSetMaxPacketProcess <> "" Then
                LogDataImport("Configuring max_allowed_packet: FAILED")
                LogDataImport("ERROR: " & sSetMaxPacketProcess)
                ReWritetoLog("Configuring max_allowed_packet", "FAILED")
                WritetoLog(sSetMaxPacketProcess, "")
                WritetoLog("Importing Data has been cancelled", "")

                MyProgress = 100
                myReportProgressBar(MyProgress)

                If StepByStep = True Then
                    WritetoLog("Following operations will be cancelled", "")
                    iCurrentStep = AppStepProcess.FAILED
                    CurrentStep(iCurrentStep)
                End If

                LogDataImport("=== Migration aborted ===" & vbCrLf & vbCrLf & vbCrLf)

                Exit Sub
            Else
                LogDataImport("Configuring max_allowed_packet: SUCCESS")
                ReWritetoLog("Configuring max_allowed_packet", "SUCCESS")
            End If

            'Importing BAK File

            ExtractZipFile(Me.txtBakFile.Text, "ImportfromDPOSRestore", arrRstBak, True, sMySQLDPosPass)

            System.Threading.Thread.Sleep(2000)

            For Each thisBAKfile In arrRstBak
                Dim CreateSchemasMsg2 As String
                CreateSchemasMsg2 = CreateSchemas(thisBAKfile.ToString.ToLower, True)

                If CreateSchemasMsg2 <> "" Then
                    If myMsgBox("Importing database [" & thisBAKfile & "] to MySQL Server has encountered an error during creating database. " & vbCrLf & "Do you still want to proceed or stop import process?", "Importing Data from DPos Restore Failed", myMsgBoxDisplay.CustomYesNo, "PROCEED", "STOP") = DialogResult.OK Then
                        LogDataImport(vbCrLf & "Failed to Create Database: " & thisBAKfile.ToString)
                        LogDataImport(vbCrLf & "ERROR: " & CreateSchemasMsg2)

                        System.Threading.Thread.Sleep(2000)

                        MyProgress += 25 / iArrayCount
                        myReportProgressBar(MyProgress)

                        Exit For
                    Else
                        LogDataImport(vbCrLf & "Failed to Create Database: " & thisBAKfile.ToString)
                        LogDataImport(vbCrLf & "ERROR: " & CreateSchemasMsg2 & vbCrLf)

                        System.Threading.Thread.Sleep(2000)

                        MyProgress = 100
                        myReportProgressBar(MyProgress)

                        If StepByStep = True Then
                            WritetoLog("Following operations will be cancelled", "")
                            iCurrentStep = AppStepProcess.FAILED
                            CurrentStep(iCurrentStep)
                        End If

                        LogDataImport("=== Migration aborted ===" & vbCrLf & vbCrLf & vbCrLf)

                        Exit Sub
                    End If
                End If

                WritetoLog("Importing Data from " & thisBAKfile.ToString & " to MySQL", "")
                Dim sProcess As String

                System.Threading.Thread.Sleep(2000)

                sProcess = RestoreDatabase(thisBAKfile, sDatabaseLocation & "\ImportfromDPOSRestore\")
                If sProcess = "" Then
                    ReWritetoLog("Import Database Structure to MySQL: " & thisBAKfile, "SUCCESS")
                Else
                    If myMsgBox("Importing database [" & thisBAKfile & "] to MySQL Server has encountered an error during importing. " & vbCrLf & "Do you still want to proceed or stop import process?", "Importing Data from DPos Restore Failed", myMsgBoxDisplay.CustomYesNo, "PROCEED", "STOP") = DialogResult.OK Then
                        LogDataImport(vbCrLf & "Failed to Import Database: " & thisBAKfile.ToString)
                        LogDataImport(vbCrLf & "ERROR: " & sProcess)
                        ReWritetoLog("Import Database Structure to MySQL: " & thisBAKfile, "SKIPPED")
                        WritetoLog(sProcess, "")

                        System.Threading.Thread.Sleep(2000)

                        MyProgress += 25 / iArrayCount
                        myReportProgressBar(MyProgress)

                        Exit For
                    Else
                        LogDataImport(vbCrLf & "Failed to Import Database: " & thisBAKfile.ToString)
                        LogDataImport(vbCrLf & "ERROR: " & sProcess & vbCrLf)

                        ReWritetoLog("Import Database Structure to MySQL: " & thisBAKfile, "FAILED")
                        WritetoLog(sProcess, "")

                        System.Threading.Thread.Sleep(2000)

                        MyProgress = 100
                        myReportProgressBar(MyProgress)

                        If StepByStep = True Then
                            WritetoLog("Following operations will be cancelled", "")
                            iCurrentStep = AppStepProcess.FAILED
                            CurrentStep(iCurrentStep)
                        End If

                        LogDataImport("=== Migration aborted ===" & vbCrLf & vbCrLf & vbCrLf)

                        Exit Sub
                    End If
                End If

                WritetoLog("Import CSV Data files of " & thisBAKfile.ToString, "")

                Dim arrDatabaseStructure As New ArrayList
                arrDatabaseStructure = FillDatabaseStructureMySQL(thisBAKfile.ToString.ToLower)
                WritetoLog("Disabling all indexes under " & thisBAKfile & "...", "")
                ChangeIndexSettingByDatabase(thisBAKfile, False)

                For Each thisTable As String In arrDatabaseStructure
                    Dim thisCSVFileInfo As String() = thisTable.Split(CHAR_PARA)

                    Dim sCSVFile As String
                    sCSVFile = thisCSVFileInfo(0).ToString & "_" & thisCSVFileInfo(1).ToString & ".BAK"

                    WritetoLog("Importing CSV Data File: " & sCSVFile & "...", "")

                    If CheckFileIfExistInZip(sCSVFile, Me.txtBakFile.Text) Then
                        Dim sProcessImportCSV As Boolean
                        Dim sError As String = ""
                        Dim bBreakFiles As Boolean = False
                        Dim arrFilesToImport As New ArrayList

                        arrFilesToImport.Clear()
                        bBreakFiles = BreakDownRestoreFilesMySQL(sDatabaseLocation & "\ImportfromDPOSRestore\", thisCSVFileInfo(0).ToString & "_" & thisCSVFileInfo(1).ToString, ".BAK", thisCSVFileInfo(1).ToString, True, True, arrFilesToImport)

                        If bBreakFiles Then
                            'calculate percentage if bulk restore files
                            Dim iBrFiles As Integer = arrFilesToImport.Count
                            Dim iPercentPerFile As Integer = 100 / iBrFiles
                            Dim iProgress As Integer = iPercentPerFile
                            Dim arrSkippedmsg As New ArrayList
                            Dim bError As Boolean = False

                            arrSkippedmsg.Clear()

                            For Each thisfile As String In arrFilesToImport

                                sProcessImportCSV = ImportCSVFileNoHeaders(sDatabaseLocation & "\ImportfromDPOSRestore\" & thisfile, thisCSVFileInfo(1).ToString, thisCSVFileInfo(0).ToString, 10000, sError)

                                If sProcessImportCSV Then
                                    If arrFilesToImport.Count <= 1 Then
                                        ReWritetoLog("Importing CSV Data File: " & thisfile, "SUCCESS")
                                    Else
                                        If myCInt(arrFilesToImport.LastIndexOf(thisfile)) <> iBrFiles - 1 Then
                                            ReWritetoLog("Importing CSV Data File: " & thisCSVFileInfo(0).ToString & "_" & thisCSVFileInfo(1).ToString & ".BAK" & " ... " & iProgress.ToString & "%", "SUCCESS")
                                        Else
                                            If Not bError Then
                                                ReWritetoLog("Importing CSV Data File: " & thisCSVFileInfo(0).ToString & "_" & thisCSVFileInfo(1).ToString & ".BAK" & " ... 100%", "SUCCESS")
                                            Else
                                                ReWritetoLog("Importing CSV Data File: " & thisCSVFileInfo(0).ToString & "_" & thisCSVFileInfo(1).ToString & ".BAK" & " ... " & iProgress.ToString & "%", "SUCCESS")
                                            End If
                                        End If

                                        iProgress += iPercentPerFile

                                        If iProgress > 100 Then iProgress = 100
                                    End If
                                Else
                                    bError = False
                                    If myMsgBox("Importing database [" & thisBAKfile & "] to MySQL Server has encountered an error during importing. " & vbCrLf & "Do you still want to proceed or stop import process?", "Importing Data from DPos Restore Failed", myMsgBoxDisplay.CustomYesNo, "PROCEED", "STOP") = DialogResult.OK Then
                                        If arrFilesToImport.Count <= 1 Then
                                            ReWritetoLog("Importing CSV Data File: " & thisfile, "SKIPPED")
                                            WritetoLog("ERROR: " & sProcessImportCSV, "")
                                        Else
                                            ReWritetoLog("Importing CSV Data File: " & thisCSVFileInfo(0).ToString & "_" & thisCSVFileInfo(1).ToString & ".BAK" & " ... " & iProgress.ToString & "%", "SKIPPED")

                                            arrSkippedmsg.Add(thisfile & "- " & sProcessImportCSV)
                                            'WritetoLog("ERROR: " & sProcessImportCSV, "")
                                        End If
                                    Else
                                        If arrFilesToImport.Count <= 1 Then
                                            ReWritetoLog("Importing CSV Data File: " & thisCSVFileInfo(0).ToString & "_" & thisCSVFileInfo(1).ToString & ".BAK", "FAILED")
                                        Else
                                            ReWritetoLog("Importing CSV Data File: " & thisCSVFileInfo(0).ToString & "_" & thisCSVFileInfo(1).ToString & ".BAK" & " ... " & iProgress.ToString & "%", "FAILED")
                                        End If

                                        WritetoLog("ERROR: " & sProcessImportCSV, "")

                                        If Directory.Exists(sDatabaseLocation & "\ImportfromDPOSRestore") Then
                                            Directory.Delete(sDatabaseLocation & "\ImportfromDPOSRestore", True)
                                        End If

                                        System.Threading.Thread.Sleep(2000)

                                        MyProgress = 100
                                        myReportProgressBar(MyProgress)

                                        If StepByStep = True Then
                                            WritetoLog("Following operations will be cancelled", "")
                                            iCurrentStep = AppStepProcess.FAILED
                                            CurrentStep(iCurrentStep)
                                        End If

                                        LogDataImport("=== Migration aborted ===" & vbCrLf & vbCrLf & vbCrLf)

                                        Exit Sub
                                    End If
                                End If
                            Next

                            If myCInt(arrFilesToImport.Count) >= 1 AndAlso myCInt(arrSkippedmsg.Count) > 0 Then
                                WritetoLog("ERROR(S): ", "")
                                For Each thisskipmsg As String In arrSkippedmsg
                                    WritetoLog(thisskipmsg, "")
                                Next
                            End If
                        Else
                            LogDataImportDetails2(thisCSVFileInfo(0).ToString, thisCSVFileInfo(1).ToString, "N/A", "The app failed to breakdown the file specified - " & sCSVFile)

                            If myMsgBox("Importing database [" & thisBAKfile & "] to MySQL Server has encountered an error during importing. " & vbCrLf & "Do you still want to proceed or stop import process?", "Importing Data from DPos Restore Failed", myMsgBoxDisplay.CustomYesNo, "PROCEED", "STOP") = DialogResult.OK Then
                                arrFailedImportTables.Add(sCSVFile)
                                ReWritetoLog("Importing CSV Data File: " & sCSVFile, "SKIPPED")
                                WritetoLog("ERROR: The app failed to breakdown the file specified - " & sCSVFile, "")
                            Else
                                ReWritetoLog("Importing CSV Data File: " & sCSVFile, "FAILED")
                                WritetoLog("ERROR: The app failed to breakdown the file specified - " & sCSVFile, "")

                                System.Threading.Thread.Sleep(2000)

                                MyProgress = 100
                                myReportProgressBar(MyProgress)

                                If StepByStep = True Then
                                    WritetoLog("Following operations will be cancelled", "")
                                    iCurrentStep = AppStepProcess.FAILED
                                    CurrentStep(iCurrentStep)
                                End If

                                LogDataImport("=== Migration aborted ===" & vbCrLf & vbCrLf & vbCrLf)

                                Exit Sub
                            End If
                        End If
                    Else
                        LogDataImportDetails2(thisCSVFileInfo(0).ToString, thisCSVFileInfo(1).ToString, "N/A", "The app cannot find the file specified - " & sCSVFile)

                        If myMsgBox("Importing database [" & thisBAKfile & "] to MySQL Server has encountered an error during importing. " & vbCrLf & "Do you still want to proceed or stop import process?", "Importing Data from DPos Restore Failed", myMsgBoxDisplay.CustomYesNo, "PROCEED", "STOP") = DialogResult.OK Then
                            arrFailedImportTables.Add(sCSVFile)
                            ReWritetoLog("Importing CSV Data File: " & sCSVFile, "SKIPPED")
                            WritetoLog("ERROR: The app cannot find the file specified - " & sCSVFile, "")
                        Else
                            ReWritetoLog("Importing CSV Data File: " & sCSVFile, "FAILED")
                            WritetoLog("ERROR: The app cannot find the file specified - " & sCSVFile, "")

                            System.Threading.Thread.Sleep(2000)

                            MyProgress = 100
                            myReportProgressBar(MyProgress)

                            If StepByStep = True Then
                                WritetoLog("Following operations will be cancelled", "")
                                iCurrentStep = AppStepProcess.FAILED
                                CurrentStep(iCurrentStep)
                            End If

                            LogDataImport("=== Migration aborted ===" & vbCrLf & vbCrLf & vbCrLf)

                            Exit Sub
                        End If
                    End If
                Next

                ChangeIndexSettingByDatabase(thisBAKfile, True)

                If StepByStep = False Then
                    MyProgress += 95 / iArrayCount
                Else
                    MyProgress += 25 / iArrayCount
                End If

                myReportProgressBar(MyProgress)
            Next

            MySql.Data.MySqlClient.MySqlConnection.ClearAllPools()

            'If Directory.Exists(sDatabaseLocation & "\ImportfromDPOSRestore") Then
            '    Directory.Delete(sDatabaseLocation & "\ImportfromDPOSRestore", True)
            'End If

            MyProgress += 5

            myReportProgressBar(MyProgress)

            WritetoLog("Importing Data has been completed", "")

            If arrFailedImportTables.Count = 0 Then
                LogDataImport("=== Migration completed without import errors ===" & vbCrLf & vbCrLf & vbCrLf)
            Else
                WritetoLog("Importing Data has been completed with import errors", "")
                WritetoLog("", "")
                WritetoLog("Please edit CSV files of the following files", "")
                WritetoLog("and import them manually to MySQL ", "")
                WritetoLog("Check the Import Scripts and Logs on this location ", "")
                WritetoLog(sDatabaseLocation & "\ImportDataLogs.txt", "")
                WritetoLog("", "")
                LogDataImport("Location: " & sDatabaseLocation & "\ImportDataLogs.txt" & vbCrLf & "Skipped Table(s):" & vbCrLf)
                For Each thistables In arrFailedImportTables
                    WritetoLog(thistables.ToString, "")
                    LogDataImport(thistables.ToString)
                Next
                LogDataImport(vbCrLf & "=== Migration completed with import error(s) ===" & vbCrLf & vbCrLf & vbCrLf)
            End If
#End Region
        Else
#Region "CLOUD DATA"
            arrFailedImportTables.Clear()
            LogDataImport("Import Option: Import from Cloud")
            LogDataImport(vbCrLf & "Cloud Setup")
            LogDataImport(" Client ID: " & sRetClientID)
            LogDataImport(" Cloud URL: " & sRetCloudURL)
            LogDataImport(" SSH Credentials URL: " & sRetSSH)
            'If false or individual execution, the app will create schemas and tables needed for the datas that will be imported to the database in case all of the said requirements were dropped previously.
            If StepByStep = False Then
                If Not CreateSchemasForImport() Then
                    LogDataImport("Create Tables and Schemas: FAILED" & vbCrLf)
                    LogDataImport("=== Migration aborted ===" & vbCrLf & vbCrLf & vbCrLf)
                    MyProgress = 100
                    myReportProgressBar(MyProgress)

                    iCurrentStep = AppStepProcess.FAILED
                    CurrentStep(iCurrentStep)
                    Exit Sub
                Else
                    LogDataImport(vbCrLf & "Create Tables and Schemas: OK")
                End If
            Else
                WritetoLog("", "")
            End If

            WritetoLog("Performing Database Upgrade...", "")
            If Not DatabaseUpgradeCloud() Then
                LogDataImport(vbCrLf & "Failed to perform Database upgrade" & vbCrLf & vbCrLf & "=== Migration Aborted ===" & vbCrLf & vbCrLf & vbCrLf)
                ReWritetoLog("Performing Database Upgrade", "FAILED")
                If StepByStep = True Then
                    WritetoLog("Other Process will not be executed", "")
                End If

                MyProgress = 100
                myReportProgressBar(MyProgress)

                iCurrentStep = AppStepProcess.FAILED
                CurrentStep(iCurrentStep)
                Exit Sub
            Else
                ReWritetoLog("Performing Database Upgrade", "SUCCESS")
            End If

            WritetoLog("", "")
            WritetoLog("Importing CSV from downloaded zip file has been started.", "")

            If Not ImportfromCloud(restore, arrFailedImportTables) Then
                LogDataImport("=== Migration Failed ===" & vbCrLf & vbCrLf & vbCrLf)

                MyProgress = 100
                myReportProgressBar(MyProgress)

                If StepByStep Then
                    WritetoLog("Following operations will be cancelled", "")
                End If

                iCurrentStep = AppStepProcess.FAILED
                CurrentStep(iCurrentStep)
                Exit Sub
            Else
                If arrFailedImportTables.Count > 0 Then
                    LogDataImport("Location: " & sDatabaseLocation & "\ImportDataLogs.txt" & vbCrLf & "Skipped Table(s):" & vbCrLf)
                    For Each thistables In arrFailedImportTables
                        LogDataImport(thistables.ToString)
                    Next
                    LogDataImport(vbCrLf & "=== Migration completed with import error(s) ===" & vbCrLf & vbCrLf & vbCrLf)
                Else
                    LogDataImport("=== Migration Completed without import error(s) ===" & vbCrLf & vbCrLf & vbCrLf)
                End If
            End If
#End Region
        End If

        If (StepByStep = False And rbtnImportFile.Checked = False) Or (StepByStep = True And rbtnImportFile.Checked = False) Then
            'If DPosMySQLUserstoDB() = False Then
            'Exit Sub
            'End If
        End If

        DeleteAppShortcut()
        iCurrentStep += 1
        CurrentStep(iCurrentStep)
    End Sub

    Public Function GetRestoreFromCloud(ByVal oThisMSGList As clsRestore) As Boolean
        Dim bReturn As Boolean = True
        Dim restore As New clsRestore
        Dim messageList As New clsMessageList

        Application.DoEvents()

        Try
            'Download all the restore files from cloud
            If DownloadRestoreFilesV2(oThisMSGList) Then
                If oThisMSGList.ExtractZIP(oThisMSGList.CsvDirectory & oThisMSGList.ZipFileName, oThisMSGList.CsvDirectory) Then
                    'Set the extracted folder name.
                    oThisMSGList.ExtractedFolderName = oThisMSGList.ZipFileName.Replace(".zip", "\")

                    System.Threading.Thread.Sleep(5000)

                    oThisMSGList.ProcessExtractedZIPFile(oThisMSGList.CsvDirectory & oThisMSGList.ExtractedFolderName)
                Else
                    myMsgBox("A problem was encountered while extracting the zip file. Restore will not continue.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    bReturn = False
                End If
            Else
                bReturn = False
            End If

            messageList.AddToList("ClientID", GenerateClientID(sRetClientID))
            messageList.AddToList("RestoreOutFiles", "Complete")
            SendMessage(messageList.SerializeList, GetURL("COMMUNICATE", sRetCloudURL))
        Catch ex As Exception
            bReturn = False
        End Try

        Return bReturn
    End Function

    Public Function ImportfromCloud(ByVal oThisMSGList As clsRestore, ByRef thisFailedImports As ArrayList) As Boolean
        Dim bReturn As Boolean = True
        Dim restore As New clsRestore
        Dim messageList As New clsMessageList

        Application.DoEvents()

        Try
            'Download all the restore files from cloud
            If ImportV2(oThisMSGList, thisFailedImports) = False Then
                WritetoLog("Importing Data Failed", "")
            Else
                If thisFailedImports.Count > 0 Then
                    LogDataImport("Import Data: OK with import errors")
                    WritetoLog("Importing Data has been completed with import errors", "")
                    WritetoLog("", "")
                    WritetoLog("Please edit CSV files of the following tables", "")
                    WritetoLog("and import them manually to MySQL ", "")
                    WritetoLog("Check the Import Scripts and Logs on this location ", "")
                    WritetoLog(sDatabaseLocation & "\ImportDataLogs.txt", "")
                    WritetoLog("", "")
                    For Each thistables In thisFailedImports
                        WritetoLog(thistables.ToString, "")
                        LogDataImport(thistables.ToString)
                    Next
                Else
                    LogDataImport("Import Data: OK")
                    WritetoLog("Importing Data Success", "")
                End If
            End If

            WritetoLog("", "")
            WritetoLog("Importing Data from Cloud Completed", "")
        Catch ex As Exception
            bReturn = False
        Finally

        End Try

        Return bReturn
    End Function

    ''' <summary>
    ''' This function will browse all the contents of the zip file and import them directly to the database.
    ''' </summary>
    ''' <returns>true if the csv files has been successfully imported.</returns>
    Public Function ImportV2(ByVal oThisMSGList As clsRestore, ByRef thisFailedImports As ArrayList) As Boolean

        Dim bOK As Boolean = True
        Dim sTableName As String = ""
        Dim sHeaders As String = ""
        Dim bReplace As Boolean = False

        Try
            ' Browse the contents of the zip file.
            For Each sTableName In oThisMSGList.CsvDirectoryContents

                WritetoLog("Importing CSV file: " & sTableName & "...", "")

                sHeaders = GetTableColumnNames(sTableName)

                ' Check if the file is not empty.
                If IsFileEmpty(oThisMSGList.CsvDirectory & oThisMSGList.ExtractedFolderName & sTableName & ".csv") = False Then
                    ' The file is not empty. We can process the csv file.

                    ' Backups data in Cloud Sync. I'm not sure if im going to include backup here since this is a fresh db

                    ' Truncating data inside the table will not be executed as well

                    'Compare all columns of the csv file and the columns created database and get the available columns needed for import
                    sHeaders = GetComparedColumns(sHeaders, oThisMSGList.CsvDirectory & oThisMSGList.ExtractedFolderName & sTableName & ".csv")

                    If sHeaders <> "" Then
                        ' Import the csv file to the table.
                        If ImportCSVDataV2(oThisMSGList.CsvDirectory & oThisMSGList.ExtractedFolderName & sTableName & ".csv", sTableName, sHeaders, False) = False Then
                            If myMsgBox("A problem was encountered while restoring " & sTableName & ".", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.CustomYesNo, "PROCEED", "STOP") = DialogResult.OK Then
                                If StepByStep Then
                                    MyProgress += 20 / oThisMSGList.CsvDirectoryContents.Count
                                Else
                                    MyProgress += 90 / oThisMSGList.CsvDirectoryContents.Count
                                End If

                                myReportProgressBar(MyProgress)

                                ReWritetoLog("Importing CSV file: " & sTableName & "", "SKIPPED")

                                thisFailedImports.Add(sTableName)
                            Else
                                bOK = False

                                ' Delete the contents of the table in case there are rows that were inserted.
                                TruncateDataFromTable(sTableName)

                                'WriteToRestoreLog("Restore from Cloud: Import failed for " & sTableName & ".")
                                ReWritetoLog("Importing CSV file: " & sTableName & "", "FAILED")
                                Exit For
                            End If
                        Else
                            If StepByStep Then
                                MyProgress += 20 / oThisMSGList.CsvDirectoryContents.Count
                            Else
                                MyProgress += 90 / oThisMSGList.CsvDirectoryContents.Count
                            End If

                            myReportProgressBar(MyProgress)

                            ReWritetoLog("Importing CSV file: " & sTableName & "", "SUCCESS")
                        End If

                    Else
                        ' There was a problem with headers generation.
                        bOK = False

                        'WriteToRestoreLog("Restore from Cloud: Import failed for " & sTableName & ".")
                        myMsgBox("A problem was encountered while generating headers for " & sTableName & ".", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)

                        ReWritetoLog("Importing CSV file: " & sTableName & "", "FAILED")
                        Exit For
                    End If
                Else
                    If StepByStep Then
                        MyProgress += 20 / oThisMSGList.CsvDirectoryContents.Count
                    Else
                        MyProgress += 90 / oThisMSGList.CsvDirectoryContents.Count
                    End If

                    myReportProgressBar(MyProgress)

                    ReWritetoLog("Importing CSV file: " & sTableName & "", "SUCCESS")
                End If
            Next

        Catch ex As Exception
            myMsgBox("ImportV2" & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK

    End Function

    Public Function GetCloudINIValue(ByVal sHeaders As String, ByVal sClientID As String, ByVal sURL As String) As String

        Dim message As New clsMessage
        Dim serializer As New JavaScriptSerializer()
        Dim sData As String = ""
        Dim sReturn As String = ""

        Try
            ' Set the maximum length for the serializer to avoid errors on bulk data.
            serializer.MaxJsonLength = iSerializerLength

            ' Request the ini file from the server.
            message = RequestIniFile(sClientID, sURL)

            If message.Value = "Store Not Available." Then
                sReturn = message.Value
                myMsgBox(message.Value, "Import from Cloud Setup Error", myMsgBoxDisplay.OkOnly)
            Else
                Dim iniList = serializer.Deserialize(Of List(Of clsIni))(message.Value)

                If message.Value <> "" Then
                    For Each item In iniList
                        ' Write each line to the ini file.
                        If item.Header = sHeaders Then
                            sReturn = item.Value
                            Exit For
                        End If
                    Next

                    Dim sCloudIni As String = GetDatabaseLocationDPOS(1) & "\cloud.ini"

                    If myINIExist(sCloudIni) Then
                        File.Delete(sCloudIni)
                        System.Threading.Thread.Sleep(2000)
                    End If

                    File.Create(sCloudIni).Close()
                    System.Threading.Thread.Sleep(2000)

                    For Each item In iniList
                        If item.Header <> "MYSQL-TRIGGERS" Then
                            WriteToIni(item.Header & "=" & item.Value, item.Header, sCloudIni)
                        End If
                    Next
                Else
                    message.Value = "CannotGenerateCloudINI"
                End If
            End If
        Catch ex As Exception
            myMsgBox("GetCloudINIValue" & vbCrLf & ex.Message, "Import from Cloud Setup Error", myMsgBoxDisplay.OkOnly)
        End Try

        Return sReturn
    End Function

    Public Function RequestIniFile(ByVal sClientID As String, ByVal sURL As String) As clsMessage

        Dim message As New clsMessage
        Dim messageList As New clsMessageList
        Dim sResponse As String = ""
        Dim serializer As New JavaScriptSerializer()

        Try
            ' Set the maximum length for the serializer to avoid errors on bulk data.
            serializer.MaxJsonLength = iSerializerLength

            messageList.AddToList("ClientID", GenerateClientID(sClientID))
            messageList.AddToList("IniFile", "Download")

            sResponse = SendMessage(messageList.SerializeList, GetURL("COMMUNICATE", sURL))

            ' Check if the response in not nothing.
            If sResponse <> "" Then

                ' Check if the string is in JSON format.
                If IsJSON(sResponse) = True Then

                    ' Deserialize the response.
                    Dim itemList = serializer.Deserialize(Of List(Of clsMessage))(sResponse)

                    For Each item In itemList
                        message = item
                        Exit For
                    Next
                Else
                    If sResponse = "CONNERROR" Then
                        message.Value = ""
                        ' Connection error.
                        myMsgBox("Request ini file: A connection error has occured - " & sResponse, "Cloud", myMsgBoxDisplay.OkOnly)
                    Else
                        ' The response is not in json format.
                        message.Value = ""
                        myMsgBox("Request ini file: The response received is not in json format - " & sResponse, "Cloud", myMsgBoxDisplay.OkOnly)
                    End If
                End If
            End If

        Catch ex As Exception
            message.Value = ""
            MsgBox("RequestIniFile" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return message

    End Function

    Public Function ValidateBackupCurrentMySQLDB() As Boolean

        If Me.txtBackupPath.Text <> "" AndAlso Directory.Exists(Me.txtBackupPath.Text) = False Then
            myMsgBox("Incorrect Backup Filepath", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)

            Return False
        End If

        Return True
    End Function

    Public Function ValidateDataImport() As Boolean
        arrRstBak.Clear()

        Try
            Dim srvcService As New ServiceController("MySQL57")

            If srvcService.Status.Equals(ServiceControllerStatus.Stopped) Or srvcService.Status.Equals(ServiceControllerStatus.StopPending) Then
                Try
                    srvcService.Start()
                    System.Threading.Thread.Sleep(1000)
                Catch ex As Exception
                    myMsgBox("MySQL57 service is not started. Please start the service first in Control Panel->Administrative Tools->Services before opening DPos MySQL Install.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    Return False
                    Exit Function
                End Try
            End If
        Catch ex As Exception

        End Try

        If Me.rbtnImportDumpFile.Checked Then
            If Me.txtDumpFile.Text = "" Then
                myMsgBox("No Dump file selected to Import", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                Return False
                Exit Function
            End If

            If Len(Me.txtDumpFile.Text) < 4 OrElse Me.txtDumpFile.Text.Substring(Len(Me.txtDumpFile.Text) - 4, 4) <> ".sql" Then
                myMsgBox("Invalid Dump File", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                Return False
                Exit Function
            End If

            If File.Exists(Me.txtDumpFile.Text) = False Then
                myMsgBox("The Dump file selected does not exist", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                Return False
                Exit Function
            End If
        ElseIf Me.rbtnImportBakFile.Checked Then
            If Me.txtBakFile.Text = "" Then
                myMsgBox("No DPos Restore file selected to Import", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                Return False
                Exit Function
            End If

            If Len(Me.txtBakFile.Text) < 4 OrElse Me.txtBakFile.Text.Substring(Len(Me.txtBakFile.Text) - 4, 4) <> ".zip" Then
                myMsgBox("Invalid Restore File", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                Return False
                Exit Function
            End If

            If File.Exists(Me.txtBakFile.Text) = False Then
                myMsgBox("The Restore file selected does not exist", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                Return False
                Exit Function
            End If

            If Me.chkRstDeliverit.Checked Then arrRstBak.Add("DeliverITSQL")
            If Me.chkRstDPosSys.Checked Then arrRstBak.Add("DPosSysSQL")
            If Me.chkRstStock.Checked Then arrRstBak.Add("StockSQL")
            If Me.chkRstStreets.Checked Then arrRstBak.Add("StreetsSQL")
            If Me.chkRstTimeClock.Checked Then arrRstBak.Add("TimeClockSQL")

            If arrRstBak.Count = 0 Then
                myMsgBox("No Database selected to Import", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                Return False
                Exit Function
            End If

        ElseIf Me.rbtnImportFile.Checked Then
            Try
                Dim srvcService As New ServiceController("MSSQLSERVER")

                If srvcService.Status.Equals(ServiceControllerStatus.Stopped) Or srvcService.Status.Equals(ServiceControllerStatus.StopPending) Then
                    Try
                        srvcService.Start()
                        System.Threading.Thread.Sleep(1000)
                    Catch ex As Exception
                        myMsgBox("MSSQLSERVER service is not started. Please start the service first in Control Panel->Administrative Tools->Services before opening DPos MySQL Install.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                        Return False
                        Exit Function
                    End Try
                End If
            Catch ex As Exception

            End Try

            Dim sDBVersion As String
            sDBVersion = GetFieldMSSQL("Select SettingValue from tblSettings where Setting='Version'", GetConnectionStringMS("DeliveritSQL"), "SettingValue")

            If Not myCInt(sDBVersion.Replace(".", "")) >= myCInt(sDPOSSQLVersion.Replace(".", "")) Then
                myMsgBox("The loaded DPos Database version should be atleast v" & sDPOSSQLVersion & " or higher on your SQL Server in order to use this option. You must upgrade the Database by running DPos v" & sDPOSSQLVersion, "DPos SQL database version is not compatible", 1)
                Return False
                Exit Function
            End If

            ConfigureCMDShell()             'configure xp_cmdshell for MSSERVER
            DropExistingEscapeFunction()    'drop escapefunction if exist
            AddEscapeFunction()             'add the escapefunction
        ElseIf Me.rbtnImportCloud.Checked Then
            Try
                If Not ValImportfromCloud() Then
                    Return False
                    Exit Function
                End If
            Catch ex As Exception

            End Try
        End If

        Return True
    End Function
    Public Function ValidateImportOption() As Boolean
        Dim bReturn As Boolean = False

        If Me.rbtnImportFile.Checked = False Then
            myMsgBox("This feature is only available for Importing Data from local MSSERVER", "Please check Import Option", myMsgBoxDisplay.OkOnly)
        Else
            bReturn = True
        End If

        Return bReturn
    End Function

    Public Function ValImportfromCloud() As Boolean
        Dim bReturn As Boolean = True

        Try
            Dim thisCloudSetup As New frmCloudSetup

            With thisCloudSetup
                If bDemo Then
                    .txtCLUser.Text = "dsoft"
                    .txtCLPass.Text = "Dsoft@99"

                    'staging
                    '.txtClientID.Text = "1515"
                    '.txtCloudUrl.Text = "http://cloudstaging.deliverit.com.au/sync/service/v2/"
                    '.txtSSHURL.Text = "http://staging-api.deliverit.com.au/v2/get-login-details.php"

                    'live
                    .txtClientID.Text = "1393"
                    .txtCloudUrl.Text = "http://cloud.deliverit.com.au/sync/"
                    .txtSSHURL.Text = "http://api.deliverit.com.au/get-login-details.php"
                    .txtVersion.Text = "02.19.01"
                Else
                    'Textboxes will be set as default. Users can change the value to set the URLs
                    .txtCloudUrl.Text = "http://cloud.deliverit.com.au/sync/"
                    .txtSSHURL.Text = "http://api.deliverit.com.au/get-login-details.php"
                    .txtVersion.Text = "02.19.02"
                End If
                .Top = iLogonFrmPosY - .Height / 2
                .Left = iLogonFrmPosX - .Width / 2
                .ShowDialog()
            End With

            If thisCloudSetup.DialogResult = DialogResult.OK Then
                If StepByStep Then
                    Me.btnNextStep.Enabled = False
                Else
                    bDoAnyBtnEvents = False
                End If

                Dim sGetCloudINIVal As String = GetCloudINIValue("INITIAL-SYNC", sRetClientID, sRetCloudURL)

                If sGetCloudINIVal = "OK" Then
                    'download the restore file from Cloud after clicking OK button from the form
                    restore = New clsRestore
                    restore.UserName = sRetUser
                    restore.Password = sRetPass

                    restore.MessageList.AddToList("ClientID", GenerateClientID(sRetClientID))
                    restore.MessageList.AddToList("UserName", restore.UserName)
                    restore.MessageList.AddToList("Password", restore.Password)
                    If bRetRestoreAll Then
                        restore.MessageList.AddToList("RestoreOutfiles", "All")
                    Else
                        restore.StartDate = sRetStartDate
                        restore.EndDate = sRetEndDate

                        restore.MessageList.AddToList("RestoreOutfiles", "DateRange")
                        restore.MessageList.AddToList("StartDate", restore.StartDate)
                    End If
                    restore.MessageList.AddToList("DposVersion", sRetVersion)
                    restore.MessageList.AddToList("DposTables", GenerateJSONForTables())

                    If Not GetRestoreFromCloud(restore) Then
                        bReturn = False
                    End If
                ElseIf sGetCloudINIVal = "Store Not Available." Then
                    bReturn = False
                ElseIf sGetCloudINIVal = "CannotGenerateCloudINI" Then
                    myMsgBox("The client is cannot generate cloud ini file.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    bReturn = False
                Else
                    myMsgBox("The client is not valid for cloud restore. " & vbCrLf & "The client has no data on cloud.", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    bReturn = False
                End If

                If StepByStep Then
                    Me.btnNextStep.Enabled = True
                Else
                    bDoAnyBtnEvents = True
                End If
            Else
                bReturn = False
            End If
        Catch ex As Exception
            myMsgBox("ValImportfromCloud: " & ex.Message, "Import from Cloud Setup Error", myMsgBoxDisplay.OkOnly)
            bReturn = False
        End Try

        Return bReturn
    End Function

    Public Sub DPosInstaller()
        WritetoLog("", "")
        WritetoLog("Installation of DPos application has been started", "")
        WritetoLog("Check if DPos is already installed and updated...", "")

        If isThisSoftwareInstalled("DPos", True) = True Then
            If StepByStep = False Then
                MyProgress += 5
            Else
                MyProgress += 5
            End If

            ReWritetoLog("DPos is already installed", "")
            WritetoLog("Checking if the installed DPos is updated", "")
            If isDPosUpdated() = True Then
                'skip this
                WritetoLog("The installed DPos is updated", "")
                WritetoLog("The application will skip this operation", "")
            Else
                WritetoLog("The installed DPos is not updated", "")
                WritetoLog("Uninstalling DPos...", "")

                'get the uninstallstring then execute the process
                Dim sUninstallkey As String = GetDPosUninstallString()
                Dim NewProcess As New Process()
                Dim sSplitFilename As String()
                Dim sSplitArgs As String()

                sSplitFilename = sUninstallkey.Split("/".ToCharArray)
                sSplitArgs = sUninstallkey.Split("{".ToCharArray)

                NewProcess.StartInfo.FileName = sSplitFilename(0)
                NewProcess.StartInfo.Arguments = "/x{" & sSplitArgs(1)
                NewProcess.StartInfo.UseShellExecute = False
                NewProcess.Start()

                'wait until DPos has been uninstalled
                Do While isThisSoftwareInstalled("DPos", True) <> False
                    'do nothing
                Loop

                ReWritetoLog("Uninstalling DPos completed", "")

                System.Threading.Thread.Sleep(10000)

                'after uninstall run the msi provided
                InstallUpdatedDpos()
            End If
        Else
            WritetoLog("DPos is not yet installed on this computer", "")

            InstallUpdatedDpos()

            If StepByStep = False Then
                MyProgress += 5
            Else
                MyProgress += 5
            End If
        End If

        If StepByStep = False Then
            MyProgress = 100
        Else
            MyProgress += 5
        End If
        myReportProgressBar(MyProgress)

        WriteDatabaseType("1")

        'get setting EnabledCloud
        'bCloudEnabled = isCloudSyncEnabled()

        DeleteAppShortcut()

        If StepByStep Then
            WritetoLog("", "")
            WritetoLog("Click [NEXT] to proceed", "")
        End If

        iCurrentStep += 1
    End Sub

    Public Sub InstallUpdatedDpos()
        WritetoLog("Installing the updated version of DPos", "")

        'opens the installer
        Process.Start(Me.txtDPosInstaller.Text)

        'wait until Dpos has been installed
        Do While isThisSoftwareInstalled("DPos", True) <> True
            'do nothing
        Loop

        If isDPosUpdated() = True Then
            WritetoLog("The version of the installed DPos is updated", "")
        Else
            WritetoLog("The version of the installed DPos is not updated", "")
        End If

        WritetoLog("Installing the updated version of DPos completed", "")
    End Sub

    Public Function GetDPosUninstallString() As String
        Dim HKLMPath As RegistryKey = My.Computer.Registry.LocalMachine
        Dim sReturn As String = ""
        Dim iReturn As Integer = 0

        Try
            Dim MySQLRegKey As RegistryKey = HKLMPath.OpenSubKey("SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall")

            Dim iSubkeyCount As Integer = MySQLRegKey.SubKeyCount
            Dim cnt As Integer

            For cnt = 1 To iSubkeyCount
                Dim thisSubkey As RegistryKey = MySQLRegKey.OpenSubKey(MySQLRegKey.GetSubKeyNames(cnt))

                Dim thisDisplayName As String = thisSubkey.GetValue("DisplayName")
                If thisDisplayName = "DPos" Then
                    sReturn = thisSubkey.GetValue("UninstallString")
                End If
            Next

            MySQLRegKey = HKLMPath.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")
            iSubkeyCount = MySQLRegKey.SubKeyCount
            cnt = 1

            For cnt = 1 To iSubkeyCount
                Dim thisSubkey As RegistryKey = MySQLRegKey.OpenSubKey(MySQLRegKey.GetSubKeyNames(cnt))

                Dim thisDisplayName As String = thisSubkey.GetValue("DisplayName")
                If thisDisplayName = "DPos" Then
                    sReturn = thisSubkey.GetValue("UninstallString")
                End If
            Next

        Catch ex As Exception
            'do not do anything
        End Try

        Return sReturn
    End Function

    Public Function ValidateDPosInstaller() As Boolean
        If Me.txtDPosInstaller.Text = "" Then
            myMsgBox("No DPos Installer file selected to Install", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            Return False
            Exit Function
        End If

        If Len(Me.txtDPosInstaller.Text) < 4 OrElse Me.txtDPosInstaller.Text.Substring(Len(Me.txtDPosInstaller.Text) - 4, 4) <> ".msi" Then
            myMsgBox("Invalid DPos Installer File", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            Return False
            Exit Function
        End If

        If File.Exists(Me.txtDPosInstaller.Text) = False Then
            myMsgBox("The DPos Installer file selected does not exist", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            Return False
            Exit Function
        End If

        Return True
    End Function

    Public Sub DPosCloudSyncApp()

        Application.DoEvents()

        WritetoLog("", "")
        WritetoLog("Checking EnableCloud setting on the imported Database", "")

        If isCloudSyncEnabled() Then
            MyProgress += 1
            myReportProgressBar(MyProgress)

            WritetoLog("EnableCloud setting was set to 'Y'", "")
            WritetoLog("Proceeding to generate triggers for DPos MySQL Database", "")

            WritetoLog("Opening Cloud Sync App...", "")

            If OpenCloudSync() Then
                MyProgress += 1
                myReportProgressBar(MyProgress)

                ReWritetoLog("Opening Cloud Sync App", "SUCCESS")

                'wait for cloudsyncapp to finish
                WritetoLog("Waiting for Cloud Sync App to initialize for the first time...", "")
                If ReadCloudIni() = False Then
                    ReWritetoLog("Waiting for Cloud Sync App to initialize for the first time", "FAILED")
                    iCurrentStep = AppStepProcess.FAILED
                Else
                    ReWritetoLog("Waiting for Cloud Sync App to initialize for the first time", "SUCCESS")
                    WritetoLog("MySQL Triggers for DPos MySQL Database has been created", "")
                    myMsgBox("Cloud Sync App has been successfully initialize and the MySQL Triggers has been created", "Cloud Sync App", myMsgBoxDisplay.OkOnly)
                    iCurrentStep += 1
                End If
            Else
                ReWritetoLog("Opening Cloud Sync App", "FAILED")
                iCurrentStep = AppStepProcess.FAILED
            End If
        Else
            WritetoLog("EnableCloud setting was set to 'N'", "")
            WritetoLog("Generating Triggers for DPos MySQL Database will not proceed", "")

            MyProgress += 2

            iCurrentStep += 1
        End If

        MyProgress += 3
        myReportProgressBar(MyProgress)

        CurrentStep(iCurrentStep)
    End Sub

    Public Function ReadCloudIni() As Boolean

        Application.DoEvents()

        Dim bReturn As Boolean = False
        Try
            Dim cloud_ini_file As String = GetDatabaseLocationDPOS(1) & "\cloud.ini"
            Dim cloud_ini_file_read As String = GetDatabaseLocationDPOS(1) & "\cloud_read.ini"

            Do While myINIExist(cloud_ini_file) = False

            Loop

            If File.Exists(cloud_ini_file_read) Then
                File.Delete(cloud_ini_file_read)
                System.Threading.Thread.Sleep(2000)
            End If

            File.Copy(cloud_ini_file, cloud_ini_file_read)
            System.Threading.Thread.Sleep(2000)

            'reads the cloud.ini copy if the property is present
            'MYSQL-TRIGGERS=OK will tell if the triggers for dpos tables were completed
            Do While GetInfoFromIni("MYSQL-TRIGGERS", cloud_ini_file_read, False) <> "OK"
                'deletes the copy and create another one
                If File.Exists(cloud_ini_file_read) Then
                    File.Delete(cloud_ini_file_read)
                    System.Threading.Thread.Sleep(2000)
                End If

                File.Copy(cloud_ini_file, cloud_ini_file_read)
                System.Threading.Thread.Sleep(2000)
            Loop

            'deletes the copy
            If File.Exists(cloud_ini_file_read) Then
                File.Delete(cloud_ini_file_read)
                System.Threading.Thread.Sleep(2000)
            End If

            bReturn = True
        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Sub DropAllUsersTablesAndSchemas()
        Dim sSQL As String
        Dim sErrorLog As String = ""

        Me.DataGridView1.Rows.Clear()
        MyProgress = 0
        'Me.bwDropDposSchemasTables.ReportProgress(0)
        myReportProgressBar(MyProgress)

        Try
            WritetoLog("Removing DPos Credentials on DPos MySQL Server...", "")
            sSQL = "DROP USER IF exists 'dpos'@'%'"

            sErrorLog = ProcessMySQL(sSQL, GetConnectionStringMY(""))

            If sErrorLog <> "" Then
                ReWritetoLog("Removing DPos Credentials on DPos MySQL Server", "FAILED")
            Else
                ReWritetoLog("Removing DPos Credentials on DPos MySQL Server", "SUCCESS")
            End If

            MyProgress += 5
            myReportProgressBar(MyProgress)

            sErrorLog = ""
            WritetoLog("Droping All Existing DPos Databases...", "") 'All Existence Denied

            For Each sschemas In arDPosSchemas
                WritetoLog("Dropping DPos Database(" & sschemas & ") from DPos MySQL Server...", "")

                sSQL = "DROP database if exists " & sschemas

                sErrorLog = ProcessMySQL(sSQL, GetConnectionStringMY(""))

                If sErrorLog <> "" Then
                    ReWritetoLog("Dropping DPos Database(" & sschemas & ") from DPos MySQL Server", "FAILED")
                Else
                    ReWritetoLog("Dropping DPos Database(" & sschemas & ") from DPos MySQL Server", "SUCCESS")
                End If

                MyProgress += 95 / arDPosSchemas.Count
                myReportProgressBar(MyProgress)

                System.Threading.Thread.Sleep(1000)
            Next
        Catch ex As Exception
            myMsgBox("DropAllUsersTablesAndSchemas" & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        WritetoLog("", "")
        WritetoLog("DONE", "")
        myMsgBox("Deleted", "Drop All Tables and DPos User", myMsgBoxDisplay.OkOnly)
    End Sub

    Private Function BackupMSSQLDB(ByVal sPath As String, ByRef sDBError As String) As Boolean
        Dim bReturn As Boolean = False

        Application.DoEvents()

        'MyProgress = 0
        'myReportProgressBar(MyProgress)

        Try
            Dim sFilesToAdd As New Chilkat.StringArray
            Dim bOK As Boolean = True
            Dim sMsg As String = "Nothing selected"
            Dim sDataZipFile As String = ""
            Dim sProgramZipFile As String = ""
            Dim sBackupPath As String = ""
            Dim sZipPath As String = ""
            Dim sPrefixZipFileName As String = ""

            Dim sRestaurantChain As String = myCStr(GetFieldMSSQL("SELECT * FROM tblSettings where Setting='RestaurantChain'", GetConnectionStringMS("DeliveritSQL"), "SettingValue"))
            If sRestaurantChain = "" Then
                sRestaurantChain = "Data"
            End If

            Dim sDBVersion As String = myCStr(GetFieldMSSQL("SELECT * FROM tblSettings where Setting='Version'", GetConnectionStringMS("DeliveritSQL"), "SettingValue").ToString.Replace(".", ""))
            If sDBVersion = "" Then
                sDBVersion = sDPOSSQLVersion.Replace(".", "")
            End If

            If CInt(sDBVersion) < 21800 Then
                Dim sDBChecktbls As Integer = myCInt(GetFieldMSSQL("Select count(TABLE_NAME) as CountTBL from INFORMATION_SCHEMA.TABLES where TABLE_NAME='tblSubCategoryPrinters' or TABLE_NAME='tblPayments'", GetConnectionStringMS("DeliveritSQL"), "CountTBL").ToString)
                Dim sDBCheckcols As Integer = myCInt(GetFieldMSSQL("Select count(COLUMN_NAME) as CountCOL from INFORMATION_SCHEMA.COLUMNS where (TABLE_NAME='tblSubCategory' and (COLUMN_NAME='Printers' or COLUMN_NAME='PrintBold' or COLUMN_NAME='ShopPrint' or COLUMN_NAME='PickupPrint' or COLUMN_NAME='DeliveryPrint' or COLUMN_NAME='TablePrint' or COLUMN_NAME='Highlight')) or (TABLE_NAME='tblOrderHeaders' and (COLUMN_NAME='TenderTypeID' or COLUMN_NAME='AmountPaid'))", GetConnectionStringMS("DeliveritSQL"), "CountCOL").ToString)

                If sDBChecktbls > 0 Or sDBCheckcols < 9 Then
                    sDBVersion = "021800"
                End If
            End If

            sPrefixZipFileName = sRestaurantChain & " v" & sDBVersion

            If sPath = "" Then
                sZipPath = sDatabaseLocation & "\MSSQLBACKUP\"
                sBackupPath = sDatabaseLocation & "\MSSQLBACKUP\" & sPrefixZipFileName & " " & Date.Today.Year & Date.Today.Month.ToString.PadLeft(2, "0") & Date.Today.Day.ToString.PadLeft(2, "0") & "\"
            Else
                If sPath.EndsWith("\") = False Then
                    sPath &= "\"
                End If

                sZipPath = sPath
                sBackupPath = sPath & sPrefixZipFileName & " " & Date.Today.Year & Date.Today.Month.ToString.PadLeft(2, "0") & Date.Today.Day.ToString.PadLeft(2, "0") & "\"
            End If

            WritetoLog("", "")
            WritetoLog("Backup Current MSSQL Data has been started", "")

            If CreateBackupDirectory(sZipPath, sBackupPath) Then
                System.Threading.Thread.Sleep(2000)

                For Each thisDB As String In arDPosSchemas
                    Dim sBackupDatabaseFile As String = ""
                    Dim sErrorMsg As String = ""

                    WritetoLog("Back up Database: " & thisDB.ToString & "...", "")
                    sBackupDatabaseFile = BackupDatabaseMSSQL(thisDB.ToString, sBackupPath, sErrorMsg)

                    If sBackupDatabaseFile <> "" Then
                        ReWritetoLog("Back up Database: " & thisDB.ToString, "SUCCESS")
                        sFilesToAdd.Append(sBackupDatabaseFile)
                    Else
                        sDBError = thisDB.ToString & " - " & sErrorMsg
                        ReWritetoLog("Back up Database: " & thisDB.ToString, "FAILED")
                        WritetoLog("Error on back up database", "")
                        WritetoLog("Other Process will not executed", "")
                        WritetoLog("Backup DID NOT WORK", "")
                        'MyProgress = 100
                        'myReportProgressBar(MyProgress)

                        Return bReturn
                    End If
                Next

                WritetoLog("Compressing backup files...", "")
                sDataZipFile = CompressFiles("Data", sFilesToAdd, sZipPath, sPrefixZipFileName)
                If sDataZipFile <> "" Then
                    ReWritetoLog("Compressing backup files", "SUCCESS")
                Else
                    sDBError = "Failed to Compress Backup Data"
                    ReWritetoLog("Compressing backup files", "FAILED")
                    WritetoLog("Backup DID NOT WORK", "")
                    'MyProgress = 100
                    'myReportProgressBar(MyProgress)
                    Return bReturn
                End If

                If Directory.Exists(sBackupPath) Then
                    Directory.Delete(sBackupPath, True)
                End If

                WritetoLog("MSSERVER Database backup finished OK", "")

                bReturn = True
            Else
                sDBError = "Creating Backup Directory has been failed"
                WritetoLog("Backup DID NOT WORK", "")
                'MyProgress = 100
                'myReportProgressBar(MyProgress)
                Return bReturn
            End If

        Catch ex As Exception
            sDBError = ex.Message
        End Try

        Return bReturn
    End Function

    Private Sub BackupMySQLDB(ByVal sPath As String)

        Application.DoEvents()

        MyProgress = 0
        myReportProgressBar(MyProgress)

        Try
            Dim sFilesToAdd As New Chilkat.StringArray
            Dim bOK As Boolean = True
            Dim sMsg As String = "Nothing selected"
            Dim sDataZipFile As String = ""
            Dim sProgramZipFile As String = ""
            Dim sBackupPath As String = ""
            Dim sZipPath As String = ""

            If sPath = "" Then
                sZipPath = sDatabaseLocation & "\BACKUPS\"
                sBackupPath = sDatabaseLocation & "\BACKUPS\Data " & Date.Today.Year & Date.Today.Month.ToString.PadLeft(2, "0") & Date.Today.Day.ToString.PadLeft(2, "0") & "\"
            Else
                If sPath.EndsWith("\") = False Then
                    sPath &= "\"
                End If

                sZipPath = sPath
                sBackupPath = sPath & "Data " & Date.Today.Year & Date.Today.Month.ToString.PadLeft(2, "0") & Date.Today.Day.ToString.PadLeft(2, "0") & "\"
            End If

            WritetoLog("Backup Current MySQL Data has been started", "")

            If CreateBackupDirectory(sZipPath, sBackupPath) Then
                System.Threading.Thread.Sleep(2000)

                If SwitchTriggerSettings(False, True) = False Then
                    WritetoLog("Other Process will not executed", "")
                    WritetoLog("", "")
                    WritetoLog("Backup DID NOT WORK", "")
                    MyProgress = 100
                    myReportProgressBar(MyProgress)
                    Exit Sub
                End If

                For Each thisDB As String In arDPosSchemas
                    Dim sBackupDatabaseFile As String = ""

                    WritetoLog("", "")

                    WritetoLog("Back up Database: " & thisDB.ToString & "...", "")
                    sBackupDatabaseFile = BackupDatabase(thisDB.ToString, sBackupPath)

                    If sBackupDatabaseFile <> "" Then
                        ReWritetoLog("Back up Database: " & thisDB.ToString, "SUCCESS")

                        sFilesToAdd.Append(sBackupDatabaseFile)
                        Dim arrTableList As ArrayList
                        arrTableList = FillDatabaseStructureMySQL(thisDB.ToString)

                        For Each thisTableInfo As String In arrTableList
                            Dim splitInfo As String()
                            Dim sExportfile As String = ""
                            splitInfo = thisTableInfo.Split(CHAR_PARA)
                            WritetoLog("Creating CSV file: " & splitInfo(1).ToString & "...", "")
                            sExportfile = ExportBackupCSVMySQL(splitInfo(0).ToString, splitInfo(1).ToString, sBackupPath)

                            If sExportfile <> "" Then
                                ReWritetoLog("Creating CSV file: " & splitInfo(1).ToString, "SUCCESS")

                                System.Threading.Thread.Sleep(2000)

                                sFilesToAdd.Append(sExportfile.ToString)
                            Else
                                ReWritetoLog("Creating CSV file: " & splitInfo(1).ToString, "FAILED")
                                WritetoLog("Other Process will not executed", "")
                                WritetoLog("", "")
                                WritetoLog("Backup DID NOT WORK", "")
                                MyProgress = 100
                                myReportProgressBar(MyProgress)
                                Exit Sub
                            End If

                        Next
                    Else
                        ReWritetoLog("Back up Database: " & thisDB.ToString, "FAILED")
                        WritetoLog("Error on back up database", "")
                        WritetoLog("Other Process will not executed", "")
                        WritetoLog("", "")
                        WritetoLog("Backup DID NOT WORK", "")
                        MyProgress = 100
                        myReportProgressBar(MyProgress)
                        Exit Sub
                    End If

                    MyProgress += 95 / arDPosSchemas.Count
                    myReportProgressBar(MyProgress)
                Next

                WritetoLog("", "")
                WritetoLog("Compressing backup files...", "")
                sDataZipFile = CompressFiles("Data", sFilesToAdd, sZipPath)
                If sDataZipFile <> "" Then
                    ReWritetoLog("Compressing backup files", "SUCCESS")
                    MyProgress += 5
                    myReportProgressBar(MyProgress)
                Else
                    ReWritetoLog("Compressing backup files", "FAILED")
                    WritetoLog("", "")
                    WritetoLog("Backup DID NOT WORK", "")
                    MyProgress = 100
                    myReportProgressBar(MyProgress)
                    Exit Sub
                End If

                If Directory.Exists(sBackupPath) Then
                    Directory.Delete(sBackupPath, True)
                End If

                If SwitchTriggerSettings(True, True) = False Then
                    'do nothing for now
                End If


                WritetoLog("", "")
                WritetoLog("Database backup finished OK", "")
            Else
                WritetoLog("", "")
                WritetoLog("Backup DID NOT WORK", "")
                MyProgress = 100
                myReportProgressBar(MyProgress)
                Exit Sub
            End If

        Catch ex As Exception

            MyProgress = 100
            myReportProgressBar(MyProgress)
        End Try

    End Sub

#End Region

#Region "UI Behaviour"
    Private Sub tgSwitchUserType_CheckedChanged(sender As Object, e As EventArgs) Handles tgSwitchUserType.CheckedChanged
        If bSwitchToggle = True AndAlso Me.tgSwitchUserType.Checked = True Then
            Me.tgSwitchUserType.Checked = False
            MyProgress = 0
            Me.lblProgress.Text = "Progress"
            Me.ProgressBar1.Value = 0

            Dim thisLogon As New frmLogon
            thisLogon.Top = iLogonFrmPosY - thisLogon.Height / 2
            thisLogon.Left = iLogonFrmPosX - thisLogon.Width / 2
            thisLogon.ShowDialog()

            If thisLogon.DialogResult = DialogResult.OK Then
                Me.DataGridView1.Rows.Clear()

                SwitchStepstoButtons(True)
                ImportOptionsAndInstallBtnEnabled(True)
                ResetAllImportOptions()

                Me.lblUserTypeStatus.Text = "Switch to Basic Installation/Step by Step"
                'My.Computer.Info.OSFullName.ToString.Contains("Windows 7") = True
                If My.Computer.Info.OSFullName.ToString.Contains("Windows 7") = True Then
                    Me.lblUserTypeStatus.Location = New Point(430, 63)
                Else
                    Me.lblUserTypeStatus.Location = New Point(444, 63)
                End If


                bSwitchToggle = False
                Me.tgSwitchUserType.Checked = True
                bSwitchToggle = True

                iLogonFrmPosX = Me.Left + Me.Width / 2
                iLogonFrmPosY = Me.Top + Me.Height / 2

                myDisplayOptions(0)
            Else
                Me.lblUserTypeStatus.Text = "Switch to Advanced Installation"

                If My.Computer.Info.OSFullName.ToString.Contains("Windows 7") = True Then
                    Me.lblUserTypeStatus.Location = New Point(473, 63)
                Else
                    Me.lblUserTypeStatus.Location = New Point(483, 63)
                End If

                bSwitchToggle = True
                Me.tgSwitchUserType.Checked = False

                iLogonFrmPosX = Me.Left + Me.Width / 2
                iLogonFrmPosY = Me.Top + Me.Height / 2

                myDisplayOptions(1)
            End If
        Else
            Me.DataGridView1.Rows.Clear()
            MyProgress = 0
            Me.lblProgress.Text = "Progress"
            Me.ProgressBar1.Value = 0

            If bSwitchToggle = True Then
                SwitchStepstoButtons(False)
                ImportOptionsAndInstallBtnEnabled(True)
                ResetAllImportOptions()
                myDisplayOptions(1)

                Me.lblUserTypeStatus.Text = "Switch to Advanced Installation"
                If My.Computer.Info.OSFullName.ToString.Contains("Windows 7") = True Then
                    Me.lblUserTypeStatus.Location = New Point(473, 63)
                Else
                    Me.lblUserTypeStatus.Location = New Point(483, 63)
                End If

                iLogonFrmPosX = Me.Left + Me.Width / 2
                iLogonFrmPosY = Me.Top + Me.Height / 2

                iCurrentStep = AppStepProcess.START
                CurrentStep(iCurrentStep)
            End If
        End If
    End Sub

    Private Sub frmMain_Move(sender As Object, e As EventArgs) Handles Me.Move
        iLogonFrmPosX = Me.Left + Me.Width / 2
        iLogonFrmPosY = Me.Top + Me.Height / 2
    End Sub

    Public Sub ImportOptionsAndInstallBtnEnabled(ByVal bSwitch As Boolean)
        Me.rbtnImportFile.Enabled = bSwitch

        Me.rbtnImportDumpFile.Enabled = bSwitch
        If rbtnImportDumpFile.Checked And bSwitch Then
            Me.txtDumpFile.Enabled = bSwitch
            Me.btnBrowseDumpFile.Enabled = bSwitch
        Else
            Me.txtDumpFile.Enabled = False
            Me.btnBrowseDumpFile.Enabled = False
        End If

        Me.rbtnImportBakFile.Enabled = bSwitch
        If rbtnImportBakFile.Checked And bSwitch Then
            Me.txtBakFile.Enabled = bSwitch
            Me.btnBrowseBAKFile.Enabled = bSwitch
            Me.chkRstDeliverit.Enabled = bSwitch
            Me.chkRstDPosSys.Enabled = bSwitch
            Me.chkRstStock.Enabled = bSwitch
            Me.chkRstStreets.Enabled = bSwitch
            Me.chkRstTimeClock.Enabled = bSwitch
        Else
            Me.txtBakFile.Enabled = False
            Me.btnBrowseBAKFile.Enabled = False
            Me.chkRstDeliverit.Enabled = False
            Me.chkRstDPosSys.Enabled = False
            Me.chkRstStock.Enabled = False
            Me.chkRstStreets.Enabled = False
            Me.chkRstTimeClock.Enabled = False
        End If

        Me.btnBrowseDPosInstaller.Enabled = bSwitch
        Me.txtDPosInstaller.Enabled = bSwitch

        Me.txtBackupPath.Enabled = bSwitch
        Me.btnBrowseBackupPath.Enabled = bSwitch

        Me.rbtnImportCloud.Enabled = bSwitch
        Me.btnNextStep.Enabled = bSwitch

        If bSwitch = True Then
            Me.btnNextStep.BackColor = Color.FromArgb(52, 152, 219)
        Else
            Me.btnNextStep.BackColor = Color.Silver
        End If

        If StepByStep = False Then
            Me.tgSwitchUserType.Enabled = bSwitch
        End If
    End Sub

    Public Sub ResetAllImportOptions()
        Me.rbtnImportFile.Enabled = True
        Me.rbtnImportFile.Checked = True

        Me.rbtnImportDumpFile.Enabled = True
        Me.txtDumpFile.Enabled = False
        Me.btnBrowseDumpFile.Enabled = False

        Me.rbtnImportBakFile.Enabled = True
        Me.txtDumpFile.Enabled = False
        Me.btnBrowseBAKFile.Enabled = False
        Me.chkRstDeliverit.Enabled = False
        Me.chkRstDPosSys.Enabled = False
        Me.chkRstStock.Enabled = False
        Me.chkRstStreets.Enabled = False
        Me.chkRstTimeClock.Enabled = False

        Me.rbtnImportCloud.Enabled = True

        Me.txtDPosInstaller.Text = sProgramLocation & "\DPos MSI\DPosUpgradeV030000.msi"
    End Sub

    Public Sub rbtnImportDumpFile_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnImportDumpFile.CheckedChanged
        Me.txtDumpFile.Enabled = Me.rbtnImportDumpFile.Checked
        Me.btnBrowseDumpFile.Enabled = Me.rbtnImportDumpFile.Checked

        Me.txtDumpFile.Text = ""
    End Sub

    Public Sub btnBrowseDumpFile_Click(sender As Object, e As EventArgs) Handles btnBrowseDumpFile.Click

        Me.OpenFileDialog1.Title = "Select SQL File"
        Me.OpenFileDialog1.FileName = "*.sql"

        If Me.OpenFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            txtDumpFile.Text = Me.OpenFileDialog1.FileName
        End If
    End Sub

    Public Sub rbtnImportBakFile_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnImportBakFile.CheckedChanged
        Me.txtBakFile.Enabled = Me.rbtnImportBakFile.Checked
        Me.btnBrowseBAKFile.Enabled = Me.rbtnImportBakFile.Checked
        Me.chkRstDeliverit.Enabled = Me.rbtnImportBakFile.Checked
        Me.chkRstDPosSys.Enabled = Me.rbtnImportBakFile.Checked
        Me.chkRstStock.Enabled = Me.rbtnImportBakFile.Checked
        Me.chkRstStreets.Enabled = Me.rbtnImportBakFile.Checked
        Me.chkRstTimeClock.Enabled = Me.rbtnImportBakFile.Checked

        Me.chkRstDeliverit.Checked = Me.rbtnImportBakFile.Checked
        Me.chkRstDPosSys.Checked = Me.rbtnImportBakFile.Checked
        Me.chkRstStock.Checked = Me.rbtnImportBakFile.Checked
        Me.chkRstStreets.Checked = Me.rbtnImportBakFile.Checked
        Me.chkRstTimeClock.Checked = Me.rbtnImportBakFile.Checked

        Me.txtBakFile.Text = ""
    End Sub

    Public Sub btnBrowseBAKFile_Click(sender As Object, e As EventArgs) Handles btnBrowseBAKFile.Click
        Me.OpenFileDialog1.Title = "Select DPos Restore File"
        Me.OpenFileDialog1.FileName = "*.zip"

        If Me.OpenFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            Me.txtBakFile.Text = Me.OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub btnBrowseDPosInstaller_Click(sender As Object, e As EventArgs) Handles btnBrowseDPosInstaller.Click
        Me.OpenFileDialog1.Title = "Locate DPos Installer File"
        Me.OpenFileDialog1.FileName = "*.msi"

        If Me.OpenFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            Me.txtDPosInstaller.Text = Me.OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub btnBrowseBackupPath_Click(sender As Object, e As EventArgs) Handles btnBrowseBackupPath.Click
        If Me.FolderBrowserDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            Me.txtBackupPath.Text = Me.FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Public Sub myDisplayOptions(ByVal bSwitch As Integer)

        Select Case bSwitch
            Case 1  'Guides and Logs
                Me.lblStepTitle.Visible = True
                Me.lblStepDesc.Visible = True

                'Me.chkOpenCloudSyncApp.Visible = False

                Me.lblLog.Visible = True
                Me.lblLog.Location = New Point(235, 205)
                Me.DataGridView1.Visible = True
                Me.DataGridView1.Location = New Point(235, 221)
                Me.DataGridView1.Width = 453
                Me.DataGridView1.Height = 249

                Me.grpDPosInstaller.Visible = False

                Me.grpImportData.Visible = False
            Case 2  'Guides and Import data
                Me.lblStepTitle.Visible = True
                Me.lblStepDesc.Visible = True

                'Me.chkOpenCloudSyncApp.Visible = False

                Me.lblLog.Visible = False
                Me.DataGridView1.Visible = False

                Me.grpImportData.Visible = True
                Me.grpImportData.Location = New Point(235, 205)

                Me.grpDPosInstaller.Visible = False
            Case 3  'Guides and Install Dpos
                Me.lblStepTitle.Visible = True
                Me.lblStepDesc.Visible = True

                'Me.chkOpenCloudSyncApp.Visible = False

                Me.lblLog.Visible = False
                Me.DataGridView1.Visible = False

                Me.grpImportData.Visible = False

                Me.grpDPosInstaller.Visible = True
                Me.grpDPosInstaller.Location = New Point(235, 205)
            Case 4 'Guides and Logs with opening CloudSync option
                Me.lblStepTitle.Visible = True
                Me.lblStepDesc.Visible = True

                'Me.chkOpenCloudSyncApp.Visible = True
                'Me.chkOpenCloudSyncApp.Location = New Point(240, 214)

                Me.lblLog.Visible = True
                Me.lblLog.Location = New Point(235, 255)
                Me.DataGridView1.Visible = True
                Me.DataGridView1.Location = New Point(235, 271)
                Me.DataGridView1.Width = 453
                Me.DataGridView1.Height = 199

                Me.grpDPosInstaller.Visible = False

                Me.grpImportData.Visible = False
            Case Else   'No Guides, show all options (for advance users)
                Me.lblStepTitle.Visible = False
                Me.lblStepDesc.Visible = False

                'Me.chkOpenCloudSyncApp.Visible = False

                Me.lblLog.Visible = True
                Me.lblLog.Location = New Point(235, 375)
                Me.DataGridView1.Visible = True
                Me.DataGridView1.Location = New Point(235, 389)
                Me.DataGridView1.Width = 453
                Me.DataGridView1.Height = 91

                Me.grpImportData.Visible = True
                Me.grpImportData.Location = New Point(235, 90)

                Me.grpDPosInstaller.Visible = True
                Me.grpDPosInstaller.Location = New Point(235, 294)
        End Select

    End Sub

    Public Sub SwitchStepstoButtons(ByVal thisValue As Boolean)
        If thisValue = True Then
            Me.pnlBasicCtrl.Visible = False
            Me.pnlAdvanceCtrl.Visible = True
            Me.btnNextStep.Visible = False

            bDoAnyBtnEvents = True
            StepByStep = False
        Else
            Me.pnlBasicCtrl.Visible = True
            Me.pnlAdvanceCtrl.Visible = False
            Me.btnNextStep.Visible = True

            StepByStep = True
            iCurrentStep = AppStepProcess.START
            ClearAllSteps()
            HighlightStep(iCurrentStep)
        End If
    End Sub

    Private Sub frmMain_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If Me.btnNextStep.Text = "CLOSE" Then
            myMsgBox("Please click [Close] button", "Action not Allowed", myMsgBoxDisplay.OkOnly)
            e.Cancel = True
        End If
    End Sub
#End Region

#Region "UI Progress"
    Public Sub CurrentStep(ByVal StepNo As Double)
        Dim ind As Integer

        Application.DoEvents()

        'will skip this one if not advanced user
        If StepByStep = True Then
            'clears the check of all steps(UI)
            ClearAllSteps()

            'marks all steps with check before current step(UI)
            Dim thisStepNo As Double
            If StepNo = AppStepProcess.FAILED Then
                thisStepNo = AppStepProcess.DONE
            Else
                thisStepNo = StepNo
            End If

            For ind = 1 To thisStepNo - 1
                StepMarkasChecked(ind)
            Next ind

            'Highlight the current step(UI)
            HighlightStep(StepNo)

            'Highlight the current step(UI)
            StepDialogues(StepNo)

            If iCurrentStep <> AppStepProcess.START And iCurrentStep <> AppStepProcess.FAILED And iCurrentStep <> AppStepProcess.DONE Then
                Me.tgSwitchUserType.Enabled = False
            Else
                Me.tgSwitchUserType.Enabled = True
            End If

            'Executes Function depending on the value of the argument
            If iCurrentStep = AppStepProcess.FAILED Then              'if a process fails set 0
                If StepByStep Then
                    WritetoLog("", "")
                    WritetoLog("Setup Failed. Click [CLOSE] to abort", "")
                End If

                ResetAllImportOptions()
                ImportOptionsAndInstallBtnEnabled(True)
                myDisplayOptions(1)
                Me.btnNextStep.Text = "CLOSE"
            ElseIf iCurrentStep = AppStepProcess.START Then '1
                Me.btnNextStep.Text = "EXECUTE"
                myDisplayOptions(1)
            ElseIf iCurrentStep = AppStepProcess.INSTALLMYSQL Then '2
                MySQLInstall()
            ElseIf iCurrentStep = AppStepProcess.ADDDPOSUSER Then '3
                AddDposUser()
            ElseIf iCurrentStep = AppStepProcess.IMPORTDATACONFIG Then '4
                myDisplayOptions(2)
                iCurrentStep += 1
            ElseIf iCurrentStep = AppStepProcess.ADDSCHEMAANDTABLES Then '5
                myDisplayOptions(1)
                AddSchemasTables()
            ElseIf iCurrentStep = AppStepProcess.IMPORTDATA Then '6
                ImportData()
            ElseIf iCurrentStep = AppStepProcess.INSTALLDPOSCONFIG Then '7
                myDisplayOptions(3)
                iCurrentStep += 1
            ElseIf iCurrentStep = AppStepProcess.INSTALLDPOS Then '8
                myDisplayOptions(1)
                DPosInstaller()
            ElseIf iCurrentStep = AppStepProcess.OPENCLOUDSYNC Then '9 new
                myDisplayOptions(1)
                'crm3487
                DPosCloudSyncApp()
            ElseIf iCurrentStep = AppStepProcess.DONE Then '10 former 9
                If StepByStep Then
                    WritetoLog("", "")
                    WritetoLog("Setup Completed. Click [CLOSE] to close the application", "")
                End If
                ResetAllImportOptions()
                ImportOptionsAndInstallBtnEnabled(True)
                'If bCloudEnabled = True Then
                '    myDisplayOptions(4)
                'Else
                '    myDisplayOptions(1)
                'End If
                myDisplayOptions(1)
                Me.btnNextStep.Text = "CLOSE"
            End If
        End If
    End Sub
    Public Sub StepDialogues(ByVal iStepNo As Double)
        If iStepNo = 0 Then
            Me.lblStepTitle.Text = "Setup Failed"
            Me.lblStepDesc.Text = "Click [Close] to abort"
        ElseIf iStepNo = 1 Then
            Me.lblStepTitle.Text = "Welcome to MySQL and Data Import Client"
            Me.lblStepDesc.Text = "This app will guide you though the steps required to install" & vbCrLf & "MySQL Server and Databases required for DPos" & vbCrLf & vbCrLf & "Click [Execute] to proceed"
        ElseIf iStepNo = 2 Then
            Me.lblStepTitle.Text = "MySQL Server Install"
            Me.lblStepDesc.Text = "This application is now installing MySQL Server if it was not installed in your computer" & vbCrLf & vbCrLf & "Click [Next] after finish installing"
        ElseIf iStepNo = 3 Then
            Me.lblStepTitle.Text = "Add DPos Credentials"
            Me.lblStepDesc.Text = "This application is now adding the required DPos Credentials for MySQL" & vbCrLf & vbCrLf & "Click [Next] after the operation"
        ElseIf iStepNo = 4 Then
            Me.lblStepTitle.Text = "Setting Import Data"
            Me.lblStepDesc.Text = "Please select Import Data Options" & vbCrLf & vbCrLf & "Click [Next] to proceed"
        ElseIf iStepNo = 5 Then
            Me.lblStepTitle.Text = "Generating DPos Schemas and Tables"
            Me.lblStepDesc.Text = "This application is now creating Databases and Tables for MySQL Server" & vbCrLf & vbCrLf & "The application will automatically proceed to the next step after this operation"
        ElseIf iStepNo = 6 Then
            Me.lblStepTitle.Text = "Import Data"
            If Me.rbtnImportFile.Checked = True Then                'MSSERVER DATA to MySQL
                Me.lblStepDesc.Text = "This application is now importing data from local SQL Server using INFILE/OUTFILE" & vbCrLf & vbCrLf & "Click [Next] after the operation"
            ElseIf Me.rbtnImportDumpFile.Checked = True Then        'SQL Dump File
                Me.lblStepDesc.Text = "This application is now importing data though SQL Dump File" & vbCrLf & vbCrLf & "Click [Next] after the operation"
            ElseIf Me.rbtnImportBakFile.Checked = True Then          'Import from DPos MySQL Restore File
                Me.lblStepDesc.Text = "This application is now importing data using an existing MySQL DPos backup" & vbCrLf & vbCrLf & "Click [Next] after the operation"
            Else
                Me.lblStepDesc.Text = "This application is now importing data from DPos Cloud" & vbCrLf & vbCrLf & "Click [Next] after the operation"
            End If
        ElseIf iStepNo = 7 Then
            Me.lblStepTitle.Text = "Setup DPos Install"
            Me.lblStepDesc.Text = "Please browse the DPos Installer File." & vbCrLf & "Make sure it is updated to DPos MySQL version" & vbCrLf & vbCrLf & "Click [Next] to proceed"
        ElseIf iStepNo = 8 Then
            Me.lblStepTitle.Text = "Installing DPos"
            Me.lblStepDesc.Text = "This application is now installing the MySQL version of DPos if" & vbCrLf & "it was not installed in your computer or not updated" & vbCrLf & vbCrLf & "Click [Next] after the installation"
        ElseIf iStepNo = 9 Then
            Me.lblStepTitle.Text = "Opening Cloud Sync App"
            Me.lblStepDesc.Text = "Opens Cloud Sync App to generate Triggers on MySQL Server" & vbCrLf & "if the [EnableCloud] setting of the imported database is enabled" & vbCrLf & vbCrLf & "Click [Next] after the operation"
        ElseIf iStepNo = 10 Then
            Me.lblStepTitle.Text = "Setup Completed"
            Me.lblStepDesc.Text = "Click [Close] to close the application"
            'If bCloudEnabled = True Then
            '    Me.lblStepDesc.Text = "EnabledCloud option is enabled on the imported data and needs to create the triggers" & vbCrLf & "required for the MySQL database by initializing DPOSCloudSync for the first time" & vbCrLf & vbCrLf & "Please tick the checkbox below if you want to open DPos CloudSyncApp" & vbCrLf & "after closing this application then click [Close]"
            'Else

            'End If
        End If
    End Sub
    Public Sub ClearAllSteps()
        Me.lblStep2.BackColor = Color.FromArgb(52, 152, 219)
        Me.lblStep3.BackColor = Color.FromArgb(52, 152, 219)
        Me.lblStep4.BackColor = Color.FromArgb(52, 152, 219)
        Me.lblStep5.BackColor = Color.FromArgb(52, 152, 219)
        Me.lblStep6.BackColor = Color.FromArgb(52, 152, 219)
        Me.lblStep7.BackColor = Color.FromArgb(52, 152, 219)
        Me.lblStep8.BackColor = Color.FromArgb(52, 152, 219)
        Me.lblStep9.BackColor = Color.FromArgb(52, 152, 219)
        Me.lblStep10.BackColor = Color.FromArgb(52, 152, 219)

        Me.lblStep2.ForeColor = Color.Silver
        Me.lblStep3.ForeColor = Color.Silver
        Me.lblStep4.ForeColor = Color.Silver
        Me.lblStep5.ForeColor = Color.Silver
        Me.lblStep6.ForeColor = Color.Silver
        Me.lblStep7.ForeColor = Color.Silver
        Me.lblStep8.ForeColor = Color.Silver
        Me.lblStep9.ForeColor = Color.Silver
        Me.lblStep10.BackColor = Color.Silver

        Me.lblStep2.Image = Nothing
        Me.lblStep3.Image = Nothing
        Me.lblStep4.Image = Nothing
        Me.lblStep5.Image = Nothing
        Me.lblStep6.Image = Nothing
        Me.lblStep7.Image = Nothing
        Me.lblStep8.Image = Nothing
        Me.lblStep9.Image = Nothing
        Me.lblStep10.BackColor = Nothing
    End Sub
    Public Sub StepMarkasChecked(ByVal StepNo As Double)
        If StepNo = 2 Then
            Me.lblStep2.Image = My.Resources.ok
            Me.lblStep2.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 3 Then
            Me.lblStep3.Image = My.Resources.ok
            Me.lblStep3.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 4 Then
            Me.lblStep4.Image = My.Resources.ok
            Me.lblStep4.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 5 Then
            Me.lblStep5.Image = My.Resources.ok
            Me.lblStep5.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 6 Then
            Me.lblStep6.Image = My.Resources.ok
            Me.lblStep6.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 7 Then
            Me.lblStep7.Image = My.Resources.ok
            Me.lblStep7.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 8 Then
            Me.lblStep8.Image = My.Resources.ok
            Me.lblStep8.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 9 Then
            Me.lblStep9.Image = My.Resources.ok
            Me.lblStep9.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 10 Then
            Me.lblStep10.Image = My.Resources.ok
            Me.lblStep10.ForeColor = Color.FromArgb(255, 255, 255)
        End If
    End Sub
    Public Sub HighlightStep(ByVal StepNo As Double)
        If StepNo = 2 Then
            Me.lblStep2.BackColor = Color.FromArgb(41, 128, 185)
            Me.lblStep2.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 3 Then
            Me.lblStep3.BackColor = Color.FromArgb(41, 128, 185)
            Me.lblStep3.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 4 Then
            Me.lblStep4.BackColor = Color.FromArgb(41, 128, 185)
            Me.lblStep4.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 5 Then
            Me.lblStep5.BackColor = Color.FromArgb(41, 128, 185)
            Me.lblStep5.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 6 Then
            Me.lblStep6.BackColor = Color.FromArgb(41, 128, 185)
            Me.lblStep6.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 7 Then
            Me.lblStep7.BackColor = Color.FromArgb(41, 128, 185)
            Me.lblStep7.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 8 Then
            Me.lblStep8.BackColor = Color.FromArgb(41, 128, 185)
            Me.lblStep8.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 9 Then
            Me.lblStep9.BackColor = Color.FromArgb(41, 128, 185)
            Me.lblStep9.ForeColor = Color.FromArgb(255, 255, 255)
        ElseIf StepNo = 10 Then
            Me.lblStep10.BackColor = Color.FromArgb(41, 128, 185)
            Me.lblStep10.ForeColor = Color.FromArgb(255, 255, 255)
        End If
    End Sub

#End Region

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'CreateDPosMySQLUsers()
        '
        Try
            'If isThisSoftwareInstalled("Microsoft Visual C++ 2013 Redistributable", False) = True Then

            'End If
            'Dim sErrorImportData As String = ""

            'ImportCSVFile("C:\DPos\MySQLInstall\Export\OUTFILES\tblSettingsTouch.txt", "tblSettingsTouch", "deliveritsql", 3000, sErrorImportData)


        Catch ex As Exception

        End Try
    End Sub

    Public Function CreateSchemasForImport() As Boolean
        Dim thisTableDetails As String()
        Dim thisIndexDetails As String()
        Dim thisFKDetails As String()
        Dim bReturn As Boolean = False

        WritetoLog("Preparing the required DPos Schemas and Tables", "")

        For Each thisSchema In arDPosSchemas
            Dim CreateSchemasMsg As String
            CreateSchemasMsg = CreateSchemas(thisSchema, True)
            If CreateSchemasMsg <> "" Then
                LogDataImport(vbCrLf & "Failed to create schemas: " & thisSchema & vbCrLf & "Error: " & CreateSchemasMsg & vbCrLf & vbCrLf & "=== Migration Aborted ===" & vbCrLf & vbCrLf & vbCrLf)
                WritetoLog("Preparing the required DPos Schemas and Tables failed", "")
                Return bReturn
                Exit Function
            End If
        Next

        WritetoLog("", "")

        For Each thisTable In arDPOSTables
            thisTableDetails = thisTable.split(CHAR_PARA)
            If thisTableDetails(1).ToLower <> "audit" Then
                Dim CreateTableAuditMsg As String
                CreateTableAuditMsg = CreateTable(thisTableDetails(1).ToLower, thisTableDetails(2), thisTableDetails(0), True)
                If CreateTableAuditMsg <> "" Then
                    LogDataImport(vbCrLf & "Failed to create table: " & thisTableDetails(1).ToLower & vbCrLf & "Error: " & CreateTableAuditMsg & vbCrLf & vbCrLf & "=== Migration Aborted ===" & vbCrLf & vbCrLf & vbCrLf)
                    WritetoLog("Preparing the required DPos Schemas and Tables failed", "")
                    Return bReturn
                    Exit Function
                End If
            End If
        Next

        WritetoLog("", "")

        For Each thisIndex In arDPOSIndex
            Dim CreateMySQLIndexMsg As String
            thisIndexDetails = thisIndex.split(CHAR_PARA)
            CreateMySQLIndexMsg = CreateMYSQLIndex(thisIndexDetails(0), thisIndexDetails(1), thisIndexDetails(2), True)
            If CreateMySQLIndexMsg <> "" Then
                LogDataImport(vbCrLf & "Failed to create Index: " & thisIndexDetails(0) & vbCrLf & vbCrLf & "Error: " & CreateMySQLIndexMsg & vbCrLf & vbCrLf & "=== Migration Aborted ===" & vbCrLf & vbCrLf & vbCrLf)
                WritetoLog("Preparing the required DPos Schemas and Tables failed", "")
                Return bReturn
                Exit Function
            End If
        Next

        WritetoLog("", "")

        WritetoLog("Creating Foreign Keys has been started", "")

        For Each thisFK In arDPOSFKs
            Dim CreateMySQLFKMsg As String
            thisFKDetails = thisFK.split(CHAR_PARA)
            CreateMySQLFKMsg = CreateMySQLFKs(thisFKDetails(0), thisFKDetails(1), thisFKDetails(2), thisFKDetails(3), thisFKDetails(4), thisFKDetails(5), True)
            If CreateMySQLFKMsg <> "" Then
                LogDataImport(vbCrLf & "Failed to create Foreign Key: " & thisFKDetails(0) & "." & thisFKDetails(1) & "(" & thisFKDetails(2) & ")" & vbCrLf & "Reference: " & thisFKDetails(3) & "." & thisFKDetails(4) & "(" & thisFKDetails(5) & ")" & vbCrLf & vbCrLf & "Error: " & CreateMySQLFKMsg & vbCrLf & vbCrLf & "=== Migration Aborted ===" & vbCrLf & vbCrLf & vbCrLf)
                WritetoLog("Preparing the required DPos Schemas and Tables failed", "")
                Return bReturn
                Exit Function
            End If
        Next

        WritetoLog("Preparing the required Schemas and Tables completed", "")

        WritetoLog("", "")

        bReturn = True

        Return bReturn
    End Function



End Class