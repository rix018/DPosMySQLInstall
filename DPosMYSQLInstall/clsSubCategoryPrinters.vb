Public Class clsSubCategoryPrinters
    Inherits ArrayList

    Public Sub New()
        MyBase.New()

    End Sub

    Public Overloads Sub Add(ByVal thisRec As clsSubCategoryPrinter)
        MyBase.Add(thisRec)
    End Sub
End Class
