Public Class clsArticle

    Private zArticleID As String
    Private zDescription As String
    Private zPLU As String
    Private zSellShop As Decimal
    Private zSellDelivery As Decimal
    Private zSellTable As Decimal
    Private zSellSpecial As Decimal
    Private zCategoryID As String
    Private zSubCategoryID As String
    Private zGSTExempt As Boolean
    Private zSpecial As Boolean
    Private zUsePercent As Boolean
    Private zArticleType As String
    Private zExclude As Boolean
    Private zSubMenu As String
    Private zLoyaltyItem As Boolean
    Private zPrintZeroAmt As Boolean
    Private zHideDelivery As Boolean
    Private zHidePickup As Boolean
    Private zHideTable As Boolean
    Private zIgnoreSpecialPrice As Boolean
    Private zCountAsDealItem As Boolean
    Private zIncludeInTotal As Integer
    Private zNoOfDealItems As Integer
    Private zSpecialRecID As Integer
    Private zStickerPrint As String


    Public Sub New()
        MyBase.new()

    End Sub


    Public Property ArticleID() As String
        Get
            Return zArticleID
        End Get
        Set(ByVal Value As String)
            zArticleID = Value
        End Set
    End Property

    Public Property Description() As String
        Get
            Return zDescription
        End Get
        Set(ByVal Value As String)
            zDescription = Value
        End Set
    End Property

    Public Property PLU() As String
        Get
            Return zPLU
        End Get
        Set(ByVal Value As String)
            zPLU = Value
        End Set
    End Property

    Public Property SellShop() As Decimal
        Get
            Return zSellShop
        End Get
        Set(ByVal Value As Decimal)
            zSellShop = Value
        End Set
    End Property

    Public Property SellDelivery() As Decimal
        Get
            Return zSellDelivery
        End Get
        Set(ByVal Value As Decimal)
            zSellDelivery = Value
        End Set
    End Property

    Public Property SellTable() As Decimal
        Get
            Return zSellTable
        End Get
        Set(ByVal Value As Decimal)
            zSellTable = Value
        End Set
    End Property
    Public Property SellSpecial() As Decimal
        Get
            Return zSellSpecial
        End Get
        Set(ByVal Value As Decimal)
            zSellSpecial = Value
        End Set
    End Property

    Public Property CategoryID() As String
        Get
            Return zCategoryID
        End Get
        Set(ByVal Value As String)
            zCategoryID = Value
        End Set
    End Property

    Public Property SubCategoryID() As String
        Get
            Return zSubCategoryID
        End Get
        Set(ByVal Value As String)
            zSubCategoryID = Value
        End Set
    End Property

    Public Property GSTExempt() As Boolean
        Get
            Return zGSTExempt
        End Get
        Set(ByVal Value As Boolean)
            zGSTExempt = Value
        End Set
    End Property

    Public Property Special() As Boolean
        Get
            Return zSpecial
        End Get
        Set(ByVal Value As Boolean)
            zSpecial = Value
        End Set
    End Property

    Public Property UsePercent() As Boolean
        Get
            Return zUsePercent
        End Get
        Set(ByVal Value As Boolean)
            zUsePercent = Value
        End Set
    End Property

    Public Property ArticleType() As String
        Get
            Return zArticleType
        End Get
        Set(ByVal Value As String)
            zArticleType = Value
        End Set
    End Property

    Public Property Exclude() As Boolean
        Get
            Return zExclude
        End Get
        Set(ByVal Value As Boolean)
            zExclude = Value
        End Set
    End Property

    Public Property SubMenu() As String
        Get
            Return zSubMenu
        End Get
        Set(ByVal Value As String)
            zSubMenu = Value
        End Set
    End Property

    Public Property LoyaltyItem() As Boolean
        Get
            Return zLoyaltyItem
        End Get
        Set(ByVal Value As Boolean)
            zLoyaltyItem = Value
        End Set
    End Property

    Public Property PrintZeroAmt() As Boolean
        Get
            Return zPrintZeroAmt
        End Get
        Set(ByVal Value As Boolean)
            zPrintZeroAmt = Value
        End Set
    End Property

    Public Property HideDelivery() As Boolean
        Get
            Return zHideDelivery
        End Get
        Set(ByVal Value As Boolean)
            zHideDelivery = Value
        End Set
    End Property

    Public Property HidePickup() As Boolean
        Get
            Return zHidePickup
        End Get
        Set(ByVal Value As Boolean)
            zHidePickup = Value
        End Set
    End Property

    Public Property HideTable() As Boolean
        Get
            Return zHideTable
        End Get
        Set(ByVal Value As Boolean)
            zHideTable = Value
        End Set
    End Property

    Public Property IgnoreSpecialPrice() As Boolean
        Get
            Return zIgnoreSpecialPrice
        End Get
        Set(ByVal Value As Boolean)
            zIgnoreSpecialPrice = Value
        End Set
    End Property

    Public Property CountAsDealItem() As Boolean
        Get
            Return zCountAsDealItem
        End Get
        Set(ByVal Value As Boolean)
            zCountAsDealItem = Value
        End Set
    End Property

    Public Property IncludeInTotal() As Integer
        Get
            Return zIncludeInTotal
        End Get
        Set(ByVal Value As Integer)
            zIncludeInTotal = Value
        End Set
    End Property

    Public Property NoOfDealItems() As Integer
        Get
            Return zNoOfDealItems
        End Get
        Set(ByVal Value As Integer)
            zNoOfDealItems = Value
        End Set
    End Property

    Public Property SpecialRecID() As Integer
        Get
            Return zSpecialRecID
        End Get
        Set(ByVal Value As Integer)
            zSpecialRecID = Value
        End Set
    End Property

    Public Property StickerPrint() As String
        Get
            Return zStickerPrint
        End Get
        Set(ByVal Value As String)
            zStickerPrint = Value
        End Set
    End Property

    Public Function Clone() As clsArticle

        Return DirectCast(Me.MemberwiseClone(), clsArticle)

    End Function

End Class
