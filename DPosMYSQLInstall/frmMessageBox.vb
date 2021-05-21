Imports System.Windows
Public Class frmMessageBox
    Dim bDragabble As Boolean
    Dim thisX As Integer
    Dim thisY As Integer
    Public msgboxType As Integer

    Private Sub frmMessageBox_Load(sender As Object, e As EventArgs) Handles Me.Load
        bDragabble = False

        If msgboxType = 1 Then
            Me.btnOK.Visible = True
            Me.btnYN_Yes.Visible = False
            Me.btnYN_No.Visible = False
        Else
            Me.btnOK.Visible = False
            Me.btnYN_Yes.Visible = True
            Me.btnYN_No.Visible = True
        End If
    End Sub
    Private Sub btnX_Click(sender As Object, e As EventArgs) Handles btnX.Click
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub
    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub
    Private Sub btnYN_Yes_Click(sender As Object, e As EventArgs) Handles btnYN_Yes.Click
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub
    Private Sub btnYN_No_Click(sender As Object, e As EventArgs) Handles btnYN_No.Click
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
End Class