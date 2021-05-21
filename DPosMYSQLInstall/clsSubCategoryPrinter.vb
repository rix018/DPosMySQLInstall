Public Class clsSubCategoryPrinter
    Private zPrinterNumber As String
    Private zPrinterSetting As String
    Private zPrinterValue As String
    Private zSubCategoryID As Integer
    Public Sub New()
        MyBase.New()
    End Sub

    Public Property PrinterNumber() As String
        Get
            Return zPrinterNumber
        End Get
        Set(ByVal Value As String)
            zPrinterNumber = Value
        End Set
    End Property

    Public Property PrinterSetting() As String
        Get
            Return zPrinterSetting
        End Get
        Set(ByVal Value As String)
            zPrinterSetting = Value
        End Set
    End Property

    Public Property PrinterValue() As String
        Get
            Return zPrinterValue
        End Get
        Set(ByVal Value As String)
            zPrinterValue = Value
        End Set
    End Property

    Public Property SubCategoryID() As Integer
        Get
            Return zSubCategoryID
        End Get
        Set(ByVal Value As Integer)
            zSubCategoryID = Value
        End Set
    End Property
End Class
