Public Class frmCloudSetup
    Dim bDragabble As Boolean
    Dim thisX As Integer
    Dim thisY As Integer
    Private Sub frmCloudSetup_Load(sender As Object, e As EventArgs) Handles Me.Load
        sRetClientID = ""
        sRetClientID = ""
        sRetSSH = ""
        sRetUser = ""
        sRetPass = ""
        sRetVersion = ""

        bDragabble = False

        Try
            Dim thisDate As Date = CDate(dtpStartDate.Value).AddDays(-90)
            dtpStartDate.Value = thisDate.ToString
        Catch ex As Exception

        End Try

        chkRestoreAll.Checked = True
    End Sub
    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Dim sPromptRet As DialogResult

        If txtCLUser.Text.Trim <> "" AndAlso txtCLPass.Text.Trim <> "" AndAlso txtClientID.Text.Trim <> "" AndAlso txtCloudUrl.Text.Trim <> "" AndAlso txtSSHURL.Text.Trim <> "" AndAlso txtVersion.Text.Trim <> "" Then
            sRetClientID = txtClientID.Text
            sRetCloudURL = txtCloudUrl.Text
            sRetSSH = txtSSHURL.Text
            sRetUser = txtCLUser.Text
            sRetPass = txtCLPass.Text
            sRetVersion = txtVersion.Text

            If chkRestoreAll.Checked Then
                bRetRestoreAll = True
                sRetStartDate = ""
                sRetEndDate = ""
            Else
                bRetRestoreAll = False
                sRetStartDate = myCStr(dtpStartDate.Value.Year) & myCStr(dtpStartDate.Value.Month).PadLeft(2, "0") & myCStr(dtpStartDate.Value.Day).PadLeft(2, "0")
                sRetEndDate = Date.Today.Year.ToString & Date.Today.Month.ToString.PadLeft(2, "0") & Date.Today.Day.ToString.PadLeft(2, "0")
            End If

            'The app warns the user that it will take for a while to download the restore files from cloud
            sPromptRet = myMsgBox("The app is going to start downloading the data from cloud after this and it might a while." & vbCrLf & "Do you want to proceed?", "Download Restore File", myMsgBoxDisplay.CustomYesNo, "YES", "NO")
            If sPromptRet = DialogResult.OK Then
                Me.DialogResult = Windows.Forms.DialogResult.OK
            Else
                Me.DialogResult = Windows.Forms.DialogResult.Cancel
            End If

            Me.Close()
        Else
            sPromptRet = myMsgBox("Please fill up all fields first" & vbCrLf & "Do you still want to fill up this form?", "All fields are required", myMsgBoxDisplay.YesNo)
            If sPromptRet = DialogResult.Cancel Then
                Me.DialogResult = Windows.Forms.DialogResult.Cancel
                Me.Close()
            End If
        End If

    End Sub
    Private Sub btnX_Click(sender As Object, e As EventArgs) Handles btnX.Click
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub
    Private Sub pnlTitleBar_MouseDown(sender As Object, e As MouseEventArgs) Handles pnlTitleBar.MouseDown
        SetAsDraggable()
    End Sub
    Private Sub pnlTitleBar_MouseUp(sender As Object, e As MouseEventArgs) Handles pnlTitleBar.MouseUp
        bDragabble = False
    End Sub
    Private Sub pnlTitleBar_MouseMove(sender As Object, e As MouseEventArgs) Handles pnlTitleBar.MouseMove
        DragThisForm()
    End Sub
    Private Sub lblTitleBar_MouseDown(sender As Object, e As MouseEventArgs) Handles lblTitleBar.MouseDown
        SetAsDraggable()
    End Sub

    Private Sub lblTitleBar_MouseMove(sender As Object, e As MouseEventArgs) Handles lblTitleBar.MouseMove
        DragThisForm()
    End Sub
    Private Sub lblTitleBar_MouseUp(sender As Object, e As MouseEventArgs) Handles lblTitleBar.MouseUp
        bDragabble = False
    End Sub
    Private Sub SetAsDraggable()
        bDragabble = True
        thisX = Windows.Forms.Cursor.Position.X - Me.Left
        thisY = Windows.Forms.Cursor.Position.Y - Me.Top
    End Sub
    Private Sub DragThisForm()
        If bDragabble = True Then
            Me.Top = Windows.Forms.Cursor.Position.Y - thisY
            Me.Left = Windows.Forms.Cursor.Position.X - thisX
        End If
    End Sub

    Private Sub chkRestoreAll_CheckedChanged(sender As Object, e As EventArgs) Handles chkRestoreAll.CheckedChanged
        If chkRestoreAll.Checked Then
            dtpStartDate.Enabled = False
        Else
            dtpStartDate.Enabled = True
        End If
    End Sub
End Class