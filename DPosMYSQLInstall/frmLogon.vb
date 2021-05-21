Public Class frmLogon
    Dim bDragabble As Boolean
    Dim thisX As Integer
    Dim thisY As Integer
    Private Sub frmLogon_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        bDragabble = False

        iLogonFrmPosX = Me.Left + Me.Width / 2
        iLogonFrmPosY = Me.Top + Me.Height / 2
    End Sub
    Private Sub btnLogIn_Click(sender As Object, e As EventArgs) Handles btnLogIn.Click
        LogIn()
    End Sub
    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub
    Private Sub frmLogon_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
        bDragabble = True
        thisX = Windows.Forms.Cursor.Position.X - Me.Left
        thisY = Windows.Forms.Cursor.Position.Y - Me.Top

        iLogonFrmPosX = Me.Left + Me.Width / 2
        iLogonFrmPosY = Me.Top + Me.Height / 2
    End Sub
    Private Sub frmLogon_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If bDragabble = True Then
            Me.Top = Windows.Forms.Cursor.Position.Y - thisY
            Me.Left = Windows.Forms.Cursor.Position.X - thisX

            iLogonFrmPosX = Me.Left + Me.Width / 2
            iLogonFrmPosY = Me.Top + Me.Height / 2
        End If
    End Sub
    Private Sub frmLogon_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
        bDragabble = False
    End Sub
    Private Sub txtUser_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtUser.KeyPress
        If Asc(e.KeyChar) = 13 Then
            Me.txtPass.Focus()
        End If
    End Sub
    Private Sub txtPass_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtPass.KeyPress
        If Asc(e.KeyChar) = 13 Then
            LogIn()
        End If
    End Sub
    Private Sub LogIn()
        Dim sCurrentPsw As String = ""
        Dim sUserName As String = ""
        Dim sPassword As String = ""

        sCurrentPsw = GetDSoftPassword()
        sCurrentPsw = sCurrentPsw.PadLeft(3, "0")

        If Me.txtUser.Text <> "" And Me.txtPass.Text <> "" Then
            sUserName = Me.txtUser.Text.ToUpper
            sPassword = Me.txtPass.Text.ToUpper

            If sUserName = "DSOFT" Then
                If sPassword = sCurrentPsw Then
                    Me.DialogResult = Windows.Forms.DialogResult.OK
                    Me.Close()
                Else
                    myMsgBox("Either the username or password is incorrect!", "Access Denied", myMsgBoxDisplay.OkOnly)
                    Me.txtUser.Clear()
                    Me.txtPass.Clear()
                    Me.txtUser.Focus()
                End If
            Else
                myMsgBox("Either the username or password is incorrect!", "Access Denied", myMsgBoxDisplay.OkOnly)
                Me.txtUser.Clear()
                Me.txtPass.Clear()
                Me.txtUser.Focus()
            End If
        End If
    End Sub
End Class