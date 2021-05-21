Public Class clsArticles

    Inherits ArrayList

    Public Sub New()
        MyBase.new()

    End Sub

    Public Overloads Sub Add(ByVal thisRec As clsArticle)
        MyBase.Add(thisRec)
    End Sub
End Class
