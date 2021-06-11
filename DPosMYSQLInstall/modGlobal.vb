Imports System.Management
Imports MySql.Data.MySqlClient
Imports System.Data.SqlClient
Imports System.IO
Imports System.Text
Imports System.Threading
Imports Microsoft.Win32
Imports System.Net
Imports Newtonsoft.Json
Imports DPosSecurity.modSecurity
Imports System.Text.RegularExpressions
Imports System.Net.NetworkInformation
Imports System.Security.Cryptography
Imports System.Collections.Specialized
'Update the following if there are changes on DPos Database structure
'FillAllArayList - stores Table information(TableName, Schema Database where the Table is, and the Columns that will be created inside the table). The info will be looped on CreateSchemas, CreateTable and Import Data(OUTFILE,INFILE)
'GetExportScript - Export script for Exporting data from MS SQL Server. the order of the column should match with the columns in GetAllColumnHeaders
'GetAllColumnHeaders - self-explanatory

'Adding Index FillAllArrayList on arDPOSIndex arraylist
'                 schema                       table                            column
'arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblTableSettings" & CHAR_PARA & "TableNoID")
Module modGlobal
    Public Class clsIni

        Public Property Header As String
        Public Property Value As String

    End Class

    Public sDatabaseLocation As String
    Public sProgramLocation As String
    Public arDPosSchemas As New ArrayList
    Public arDPOSTables As New ArrayList
    Public arDPOSIndex As New ArrayList
    Public arDPOSFKs As New ArrayList
    Public bDemo As Boolean
    Public CHAR_PARA As Char = "¶"
    Public CHAR_COMMA As Char = ","
    Public Const CHAR_RBRACKET As Char = ")"
    Public Const CHAR_LBRACKET As Char = "("
    Public iLogonFrmPosX As Integer = 0
    Public iLogonFrmPosY As Integer = 0
    Public ChilkatMailLicenseKey As String = "ABRYCEMAIL_NVfSRcDJ8K8F"
    Public ChilkatZipLicenseKey As String = "BRYCEJZIP_B7JckLgw7FyG"
    Public sDestinationFolder As String
    Public sDataFileName As String

    'CONSTANTS for Settings tblSubCategoryPrinters
    Public Const SUBCAT_PRINTERS As String = "Printers"
    Public Const SUBCAT_PRINTBOLD As String = "PrintBold"
    Public Const SUBCAT_SHOPPRINT As String = "ShopPrint"
    Public Const SUBCAT_PICKUPPRINT As String = "PickUpPrint"
    Public Const SUBCAT_DELIVERYPRINT As String = "DeliveryPrint"
    Public Const SUBCAT_TABLEPRINT As String = "TablePrint"
    Public Const SUBCAT_HIGHLIGHT As String = "Highlight"

    Public Const ContentTypeJSON As String = "application/json"
    Public Const ContentTypeURLENCODED As String = "application/x-www-form-urlencoded"
    Public Const iSerializerLength As Integer = 999999999

    'Fetch Root Password for MySQL Server from the WebService
    Public sJSonDataMySQLCreds As String
    Public sMySQLRootPass As String
    Public sMySQLDPosPass As String

    'Import Cloud Credentials
    Public sRetClientID As String
    Public sRetCloudURL As String
    Public sRetSSH As String
    Public sRetUser As String
    Public sRetPass As String
    Public sRetVersion As String
    Public bRetRestoreAll As Boolean = True
    Public sRetStartDate As String
    Public sRetEndDate As String
    Public restore As New clsRestore

    'Compatible SQL Server Version
    Public sDPOSSQLVersion As String = "02.17.24"
    Public sDPOSSQLVersionLimit As String = "02.19.02"

    Public Enum AppStepProcess As Integer
        FAILED = 0
        START = 1
        INSTALLMYSQL = 2
        ADDDPOSUSER = 3
        IMPORTDATACONFIG = 4
        ADDSCHEMAANDTABLES = 5
        IMPORTDATA = 6
        INSTALLDPOSCONFIG = 7
        INSTALLDPOS = 8
        OPENCLOUDSYNC = 9
        DONE = 10
    End Enum

    Public Enum myMsgBoxDisplay As Integer
        OkOnly = 1
        YesNo = 2
        CustomYesNo = 3
    End Enum

    Public Function GetDSoftPassword() As String
        Dim i As Integer
        Dim s As String
        Dim DateDay As String = ""
        Dim DateMonth As String = ""
        Dim DateYear As String = ""
        Dim iOut As Integer
        Dim sOut As String = ""

        DateDay = Date.Today.Day
        DateDay = DateDay.PadLeft(2, "0")
        DateMonth = Date.Today.Month
        DateMonth = DateMonth.PadLeft(2, "0")
        DateYear = Date.Today.Year
        DateYear = DateYear.Substring(2, 2)
        s = DateDay & DateMonth & DateYear

        For i = 0 To 3
            iOut += myCInt(s.Substring(i, 1))
        Next
        sOut = myCStr(iOut)
        sOut = sOut.PadLeft(2, CChar("0"))

        Return sOut
    End Function

    Public Function GetJSonfromWebService(ByVal sUrl As String, ByVal sMethod As String, ByVal iTimeout As Integer) As String
        Dim sReturn As String = ""
        Dim webReq As HttpWebRequest
        Dim webRes As HttpWebResponse = Nothing
        Dim webReader As StreamReader
        Dim webServiceUrl As Uri

        'to do
        'for crm3487 - Installer: DPos User and password from web service
        'returns JSON data
        Try

            If sUrl <> "" Then
                webServiceUrl = New Uri(sUrl)

                webReq = DirectCast(WebRequest.Create(webServiceUrl), HttpWebRequest)
                webReq.Method = sMethod
                webReq.Timeout = iTimeout
                webRes = DirectCast(webReq.GetResponse(), HttpWebResponse)
                webReader = New StreamReader(webRes.GetResponseStream())
                sReturn = webReader.ReadToEnd()
                webReader.Close()
                webRes.Close()
            Else
                sReturn = ""
            End If
        Catch e As WebException
            sReturn = ""
        Catch ex As Exception
            sReturn = ""
        End Try

        Return sReturn
    End Function

    Public Function GetJSONdata(ByVal sJSon As String, ByVal sProperty As String) As String
        Dim sReturn As String = ""
        Dim JSonString As String = ""

        Try
            If sJSon = "" Then
                JSonString = ""
            Else
                JSonString = sJSon
            End If

            Dim sJsonConvert = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(JSonString)
            Dim sFilter = sJsonConvert.Item(sProperty)

            sReturn = sFilter.ToString
        Catch ex As Exception
            sReturn = ""
        End Try

        Return sReturn
    End Function

    Public Function MergeTextFiles(ByVal sFile1 As String, ByVal sFile2 As String)

        Dim bOK As Boolean = False

        Application.DoEvents()

        Try
            FileOpen(1, sFile1, OpenMode.Append)
            For Each line As String In File.ReadLines(sFile2)
                PrintLine(1, line)
            Next
            FileClose(1)

            DeleteFile(sFile2)

            bOK = True
        Catch ex As Exception
            myMsgBox("MergeTextFiles: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bOK

    End Function

    Public Function DeleteFile(ByVal sFile As String) As Boolean
        Dim bOk As Boolean = True

        Application.DoEvents()

        Try
            If File.Exists(sFile) Then
                File.Delete(sFile)

                ' Give enough time for the file to be deleted.
                Thread.Sleep(2000)
            End If

        Catch ex As Exception
            myMsgBox("DeleteFile: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOk = False
        End Try

        Return bOk

    End Function

    Public Sub FillAllArrayList()

        'Clear all arrays
        arDPosSchemas.Clear()
        arDPOSTables.Clear()
        arDPOSIndex.Clear()

        GetConnectionStringMS("")

#Region "Schema List"
        arDPosSchemas.Add("DeliverITSQL")
        arDPosSchemas.Add("DPosSysSQL")
        arDPosSchemas.Add("StockSQL")
        arDPosSchemas.Add("StreetsSQL")
        arDPosSchemas.Add("TimeClockSQL")
#End Region

#Region "DeliverITSQL Tables"
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "Audit" & CHAR_PARA & "(`RecordID` int(11) NOT NULL AUTO_INCREMENT, `Type` char(1) DEFAULT NULL, `TableName` varchar(128) DEFAULT NULL, `PK` varchar(1000) DEFAULT NULL, `Data` varchar(5000) DEFAULT NULL, `UpdateDate` datetime DEFAULT NULL, `Synced` smallint(1) NOT NULL DEFAULT 0, PRIMARY KEY (`RecordID`)) ENGINE=InnoDB DEFAULT CHARSET=latin1")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblArticles" & CHAR_PARA & "(`ArticleID` int(11) NOT NULL AUTO_INCREMENT, `Description` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `PLU` varchar(50) CHARACTER SET utf8 DEFAULT NULL, `SellShop` double DEFAULT NULL, `SellDelivery` double DEFAULT NULL, `SellTable` double DEFAULT NULL, `SellCost` decimal(10,2) DEFAULT NULL, `HideDelivery` smallint(1) DEFAULT NULL, `HidePickup` smallint(1) DEFAULT NULL, `HideTable` smallint(1) DEFAULT NULL, `CategoryID` int(11) DEFAULT NULL, `SubCategoryID` int(11) DEFAULT NULL, `DateCreated` datetime DEFAULT NULL, `LastUpdated` datetime DEFAULT NULL, `Active` smallint(1) DEFAULT NULL, `GSTExempt` smallint(1) DEFAULT NULL, `Special` smallint(1) DEFAULT NULL, `ArticleType` varchar(20) CHARACTER SET utf8 DEFAULT NULL, `UsePercent` smallint(1) DEFAULT NULL, `LoyaltyItem` smallint(1) DEFAULT NULL, `SellSpecial` double DEFAULT NULL, `Topping1` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping2` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping3` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping4` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping5` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping6` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping7` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping8` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping9` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping10` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping11` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping12` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping13` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping14` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Topping15` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `PrintZeroAmt` smallint(1) DEFAULT NULL, `StickerPrint` varchar(5) CHARACTER SET utf8 DEFAULT NULL, `UpsellItem` smallint(1) DEFAULT NULL, `ExcludeCondimentCharge` smallint(1) DEFAULT NULL, `ExcludeFromMinimum` smallint(1) DEFAULT NULL, PRIMARY KEY (`ArticleID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblAudit" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `AuditType` varchar(20) DEFAULT NULL, `StaffID` varchar(50) DEFAULT NULL, `ComputerName` varchar(50) DEFAULT NULL, `OldOrderID` int(11) DEFAULT NULL, `OrderID` int(11) DEFAULT NULL, `PLU` varchar(50) DEFAULT NULL, `ModifiedDateTime` varchar(50) DEFAULT NULL, `OldPrice` double DEFAULT NULL, `NewPrice` double DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblBalanceSheet" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `SheetDate` varchar(50) DEFAULT NULL, `Name` varchar(50) DEFAULT NULL, `ShopSales` double DEFAULT NULL, `PickUpSales` double DEFAULT NULL, `DeliverySales` double DEFAULT NULL, `TableSales` double DEFAULT NULL, `TotalSales` double DEFAULT NULL, `ExcludedSales` double DEFAULT NULL, `TotalCash` double DEFAULT NULL, `TotalPaid` double DEFAULT NULL, `TotalUnpaid` double DEFAULT NULL, `TillFloat` double DEFAULT NULL, `TotalExpenses` double DEFAULT NULL, `TotalReceipts` double DEFAULT NULL, `TotalBalance` double DEFAULT NULL, `UnderOver` double DEFAULT NULL, `Notes` varchar(255) DEFAULT NULL, `PaidComputer` varchar(50) DEFAULT NULL, `OOPaidAmt` double DEFAULT NULL, `OOPaidOnlineAmt` double DEFAULT NULL, `OOPaidInStoreAmt` double DEFAULT NULL, `OOUnpaidAmt` double DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblBalanceSheetExpenses" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `ExpenseDate` varchar(50) DEFAULT NULL, `Description` varchar(255) DEFAULT NULL, `ExpenseAmt` double DEFAULT NULL, `PaidComputer` varchar(50) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblBalanceSheetPayments" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `PaymentDate` varchar(50) DEFAULT NULL, `PaymentType` varchar(50) DEFAULT NULL, `PaymentTypeID` int(11) DEFAULT NULL, `Calc` double DEFAULT NULL, `BSUser` double DEFAULT NULL, `PaidComputer` varchar(50) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblButtons" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `ButtonID` int(11) DEFAULT NULL, `Text` varchar(25) CHARACTER SET utf8 DEFAULT NULL, `Action` varchar(50) CHARACTER SET utf8 DEFAULT NULL, `BackColour` varchar(50) CHARACTER SET utf8 DEFAULT NULL, `ForeColour` varchar(50) CHARACTER SET utf8 DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=latin1")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblCategory" & CHAR_PARA & "(`CategoryID` int(11) NOT NULL AUTO_INCREMENT, `Description` varchar(255) DEFAULT NULL, `Active` smallint(1) DEFAULT NULL, `XeroAccountCode` varchar(25) DEFAULT NULL, `XeroAccountName` varchar(255) DEFAULT NULL, `XeroGSTExempt` smallint(1) DEFAULT NULL, PRIMARY KEY (`CategoryID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblCommentsHistory" & CHAR_PARA & "(`ID` int(11) NOT NULL AUTO_INCREMENT, `CustomerID` float DEFAULT NULL, `NoteDate` varchar(20) DEFAULT NULL, `NoteText` varchar(255) DEFAULT NULL, `UserID` varchar(20) DEFAULT NULL, `Status` smallint(1) DEFAULT NULL, `CompleteDate` varchar(20) DEFAULT NULL, PRIMARY KEY (`ID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblCondimentArticles" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `SubCategoryID` int(11) DEFAULT NULL, `Price` double DEFAULT NULL, `ArticleID` int(11) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblCustomers" & CHAR_PARA & "(`CustomerID` int(11) NOT NULL AUTO_INCREMENT, `CustomerPhone` varchar(20) DEFAULT NULL, `FullName` varchar(50) DEFAULT NULL, `StreetNumber` varchar(255) DEFAULT NULL, `StreetName` varchar(50) DEFAULT NULL, `Suburb` varchar(50) DEFAULT NULL, `CommentsNoPrint` varchar(255) DEFAULT NULL, `Comments` varchar(255) DEFAULT NULL, `OldOrderValue` double DEFAULT NULL, `OldOrderNumber` varchar(25) DEFAULT NULL, `Barcode` varchar(50) DEFAULT NULL, `AutoloadPLU` varchar(50) DEFAULT NULL, `Account` smallint(1) DEFAULT NULL, `EmailUpdated` varchar(10) DEFAULT NULL, `DateJoined` varchar(10) DEFAULT NULL, `PostCode` varchar(10) DEFAULT NULL, `DisableLoyalty` smallint(1) DEFAULT NULL, `UserField1` varchar(255) DEFAULT NULL, `UserField2` varchar(255) DEFAULT NULL, `UserField3` varchar(255) DEFAULT NULL, `UserField4` varchar(255) DEFAULT NULL, `UserField5` varchar(255) DEFAULT NULL, `UserField6` varchar(255) DEFAULT NULL, `UserField7` varchar(255) DEFAULT NULL, `UserField8` varchar(255) DEFAULT NULL, `UserField9` varchar(255) DEFAULT NULL, `VIPP` smallint(1) DEFAULT NULL, `Email` varchar(255) DEFAULT NULL, `CrossStreet` varchar(255) DEFAULT NULL, `ApartmentNumber` varchar(20) DEFAULT NULL, `Floor` varchar(20) DEFAULT NULL, `BuildingName` varchar(50) DEFAULT NULL, `ExtractExclude` smallint(1) DEFAULT NULL, `Latitude` varchar(255) DEFAULT NULL, `Longitude` varchar(255) DEFAULT NULL, `UnitLevelNumber` varchar(50) DEFAULT NULL, `State` varchar(255) DEFAULT NULL, `GeoAddress` varchar(255) DEFAULT NULL, PRIMARY KEY (`CustomerID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblCustomerSummary" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `CustomerID` int(11) DEFAULT NULL, `FirstOrderDate` varchar(12) DEFAULT NULL, `LastOrderDate` varchar(12) DEFAULT NULL, `ModifiedDate` varchar(12) DEFAULT NULL, `TotalNumberOrders` int(11) DEFAULT NULL, `TotalValueOrders` double DEFAULT NULL, `AverageOrderValue` double DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblLicence" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `Setting` varchar(255) CHARACTER SET utf8 DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblOccasions" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `CustomerID` float DEFAULT NULL, `Description` varchar(255) DEFAULT NULL, `OccasionDate` varchar(20) DEFAULT NULL, `Category` varchar(20) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblOrderDetails" & CHAR_PARA & "(`OrderID` int(11) NOT NULL, `OrderItem` int(11) NOT NULL, `PLU` varchar(50) CHARACTER SET utf8 DEFAULT NULL, `Description` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `Qty` int(11) DEFAULT NULL, `UnitSell` double DEFAULT NULL, `HasSubItems` smallint(1) DEFAULT NULL, `ParentItem` int(11) DEFAULT NULL, `ParentType` int(11) DEFAULT NULL, `LoyaltyRedeemed` int(11) DEFAULT NULL, `SpecialRecID` int(11) DEFAULT NULL, `SplitItemID` int(11) DEFAULT NULL, PRIMARY KEY (`OrderID`,`OrderItem`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "(`OrderID` int(11) NOT NULL AUTO_INCREMENT, `TakenBy` varchar(20) CHARACTER SET utf8 DEFAULT NULL, `OrderDate` date DEFAULT NULL, `DeliveredBy` varchar(20) CHARACTER SET utf8 DEFAULT NULL, `LocationID` tinyint(4) DEFAULT NULL, `TenderedAmt` double DEFAULT NULL, `Comments` varchar(256) CHARACTER SET utf8 DEFAULT NULL, `TransactionStatus` int(11) DEFAULT NULL, `CustomerID` int(4) DEFAULT NULL, `PickupTime` varchar(10) CHARACTER SET utf8 DEFAULT NULL, `TransactionType` int(11) DEFAULT NULL, `CustomerName` varchar(100) CHARACTER SET utf8 DEFAULT NULL, `CustomerPhone` varchar(20) CHARACTER SET utf8 DEFAULT NULL, `StreetNumber` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `StreetName` varchar(120) CHARACTER SET utf8 DEFAULT NULL, `Suburb` varchar(100) CHARACTER SET utf8 DEFAULT NULL, `Map` varchar(50) CHARACTER SET utf8 DEFAULT NULL, `MapReference` varchar(20) CHARACTER SET utf8 DEFAULT NULL, `AddressComments` varchar(256) CHARACTER SET utf8 DEFAULT NULL, `InTime` varchar(10) CHARACTER SET utf8 DEFAULT NULL, `OutTime` varchar(10) CHARACTER SET utf8 DEFAULT NULL, `ModifiedTime` varchar(25) CHARACTER SET utf8 DEFAULT NULL, `Notes` varchar(256) CHARACTER SET utf8 DEFAULT NULL, `Paid` smallint(1) DEFAULT NULL, `NewOrderID` int(4) DEFAULT NULL, `OldOrderID` int(4) DEFAULT NULL, `ExcludeAmt` double DEFAULT NULL, `Guests` int(4) DEFAULT NULL, `LoyaltyAccrued` int(4) DEFAULT NULL, `LoyaltyRedeemed` int(4) DEFAULT NULL, `SessionID` int(11) DEFAULT NULL, `OnlineOrder` smallint(1) DEFAULT NULL, `Cashier` varchar(20) CHARACTER SET utf8 DEFAULT NULL, `TableNo` varchar(15) CHARACTER SET utf8 DEFAULT NULL, `CommentsNoPrint` varchar(256) CHARACTER SET utf8 DEFAULT NULL, `StatusComments` varchar(256) CHARACTER SET utf8 DEFAULT NULL, `ComputerName` varchar(25) CHARACTER SET utf8 DEFAULT NULL, `MakeDoneTime` varchar(10) CHARACTER SET utf8 DEFAULT NULL, `TableDesc` varchar(20) CHARACTER SET utf8 DEFAULT NULL, `OnlineRecID` varchar(50) CHARACTER SET utf8 DEFAULT NULL, `DeliveryZone` varchar(10) CHARACTER SET utf8 DEFAULT NULL, `CrossStreet` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `ApartmentNumber` varchar(20) CHARACTER SET utf8 DEFAULT NULL, `Floor` varchar(20) CHARACTER SET utf8 DEFAULT NULL, `BuildingName` varchar(50) CHARACTER SET utf8 DEFAULT NULL, `WebAmountPaid` varchar(10) CHARACTER SET utf8 DEFAULT NULL, `IsAsap` smallint(1) DEFAULT NULL, `CCTransactionNumber` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `CCNumber` varchar(4) DEFAULT NULL, `CCExpiryDate` varchar(10) DEFAULT NULL, `PaidComputer` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `PaidDate` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `ChangeAmount` varchar(255) CHARACTER SET utf8 DEFAULT NULL, `OnlinePaymentMethod` varchar(10) CHARACTER SET utf8 DEFAULT NULL, `OnlineOrderSource` varchar(50) DEFAULT NULL, `SessionName` varchar(50) CHARACTER SET utf8 DEFAULT NULL, `AccountNumber` int(4) DEFAULT NULL, `TransactionID` varchar(20) CHARACTER SET utf8 DEFAULT NULL, `YelloOrderID` varchar(100) DEFAULT NULL, `LOKETransactionID` varchar(50) DEFAULT NULL, `LOKEReferenceID` varchar(50) DEFAULT NULL, `OOTransactionID` varchar(100) DEFAULT NULL, `UnitLevelNumber` varchar(50) DEFAULT NULL, `State` varchar(255) DEFAULT NULL, `PostCode` varchar(50) DEFAULT NULL, `GeoAddress` varchar(255) DEFAULT NULL, `GeoLat` varchar(50) DEFAULT NULL, `GeoLng` varchar(50) DEFAULT NULL, `RequestedDate` varchar(20) DEFAULT NULL, `PosReferenceID` varchar(50) DEFAULT NULL, `MakeStartTime` varchar(10) DEFAULT NULL, PRIMARY KEY (`OrderID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblPayments" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `OrderID` int(11) NOT NULL, `TenderTypeID` int(11) DEFAULT NULL, `AmountPaid` double DEFAULT NULL, PRIMARY KEY (`RecID`,`OrderID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblPaymentSessions" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `OrderID` int(11) DEFAULT NULL, `PLU` varchar(50) CHARACTER SET latin1 DEFAULT NULL, `SplitItemID` int(11) DEFAULT NULL, `PaidAmount` double DEFAULT NULL, `ItemAmount` double DEFAULT NULL, `Adjustment` double DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblPrinters" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `PrinterID` varchar(50) DEFAULT NULL, `PrinterName` varchar(50) DEFAULT NULL, `PrinterUNC` varchar(255) DEFAULT NULL, `DocketType` varchar(50) DEFAULT NULL, `PrinterType` varchar(50) DEFAULT NULL, `PrintItems` varchar(50) DEFAULT NULL, `Comments` varchar(255) DEFAULT NULL, `Alarm` smallint(1) DEFAULT NULL, `Interactive` smallint(1) DEFAULT NULL, `PrinterUNC2` varchar(255) DEFAULT NULL, `PrinterType2` varchar(50) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblProfile" & CHAR_PARA & "(`ProfileID` int(11) NOT NULL AUTO_INCREMENT, `ProfileDescription` varchar(20) DEFAULT NULL, PRIMARY KEY (`ProfileID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblProfileExclusion" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `ProfileID` varchar(20) DEFAULT NULL, `Exclusion` varchar(50) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblPublicHolidays" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `StartDate` varchar(10) DEFAULT NULL, `EndDate` varchar(10) DEFAULT NULL, `SurchargePLU` varchar(20) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblRedemptions" & CHAR_PARA & "(`OrderID` int(11) NOT NULL, `CustomerID` int(11) NOT NULL, `PointsRedeemed` float DEFAULT NULL, `Description` varchar(255) DEFAULT NULL, `OrderDate` varchar(20) DEFAULT NULL, PRIMARY KEY (`OrderID`,`CustomerID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblSettings" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `Setting` varchar(50) DEFAULT NULL, `DefaultValue` varchar(255) DEFAULT NULL, `Comments` varchar(255) DEFAULT NULL, `Category` varchar(20) DEFAULT NULL, `SettingValue` varchar(255) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblSettingsLocal" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `ComputerName` varchar(50) DEFAULT NULL, `Setting` varchar(50) DEFAULT NULL, `SettingValue` varchar(255) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblSettingsTouch" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `MenuID` varchar(50) DEFAULT NULL, `BtnID` int(11) DEFAULT NULL, `Description` varchar(50) DEFAULT NULL, `BtnType` varchar(20) DEFAULT NULL, `TypeID` varchar(50) DEFAULT NULL, `ForeColor` varchar(20) DEFAULT NULL, `BackColor` varchar(20) DEFAULT NULL, `Active` smallint(1) DEFAULT NULL, `Message` varchar(255) DEFAULT NULL, `PopUp` varchar(50) DEFAULT NULL, `BackgroundImage` varchar(50) DEFAULT NULL, `ThemeForeColor` varchar(50) DEFAULT NULL, `Hidden` smallint(1) DEFAULT NULL, `TouchMultiplePopUps` smallint(1) DEFAULT NULL, `SubItem` smallint(1) DEFAULT NULL, `StartDate` varchar(10) DEFAULT NULL, `EndDate` varchar(10) DEFAULT NULL, `Repeating` varchar(10) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblSpecialsArticles" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `ArticleID` int(11) NOT NULL, `PLU` varchar(50) DEFAULT NULL, `Description` varchar(50) DEFAULT NULL, `Priority` int(11) DEFAULT NULL, `SubCategoryID` int(11) DEFAULT NULL, `SubMenu` varchar(50) DEFAULT NULL, `IgnoreSpecialPrice` smallint(1) DEFAULT NULL, `CountAsDealItem` smallint(1) DEFAULT NULL, `IncludeInTotal` int(11) DEFAULT NULL, `ChargeItem` smallint(1) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblSpecialsSettings" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `PLU` varchar(50) DEFAULT NULL, `ButtonColour` varchar(50) DEFAULT NULL, `ButtonText` varchar(255) DEFAULT NULL, `FontColour` varchar(50) DEFAULT NULL, `SortNumber` int(11) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblStaff" & CHAR_PARA & "(`StaffID` varchar(20) NOT NULL, `FirstName` varchar(50) DEFAULT NULL, `LastName` varchar(50) DEFAULT NULL, `Comments` text, `Comments1` text, `Psw` varchar(20) DEFAULT NULL, `ProfileID` varchar(50) DEFAULT NULL, `StartDate` varchar(10) DEFAULT NULL, `PhoneNumber` varchar(20) DEFAULT NULL, `MobileNumber` varchar(20) DEFAULT NULL, `DeliveryRate` double DEFAULT NULL, `HourlyRate` double DEFAULT NULL, `Active` smallint(1) DEFAULT NULL, `LeftIndexFinger` varchar(4000) DEFAULT NULL, `RightIndexFinger` varchar(4000) DEFAULT NULL, `DailyRate` double DEFAULT NULL, `Shift` varchar(50) DEFAULT NULL, `Pincode` varchar(20) DEFAULT NULL, `AppEnabled` smallint(1) DEFAULT NULL, PRIMARY KEY (`StaffID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        'arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblSubCategory" & CHAR_PARA & "(`SubCategoryID` int(11) NOT NULL AUTO_INCREMENT, `CategoryID` int(11) DEFAULT NULL, `Description` varchar(255) DEFAULT NULL, `Printers` varchar(50) DEFAULT NULL, `PrintBold` varchar(50) DEFAULT NULL, `Active` smallint(1) DEFAULT NULL, `Exclude` smallint(1) DEFAULT NULL, `ShopPrint` varchar(50) DEFAULT NULL, `PickUpPrint` varchar(50) DEFAULT NULL, `DeliveryPrint` varchar(50) DEFAULT NULL, `TablePrint` varchar(50) DEFAULT NULL, `Highlight` varchar(50) DEFAULT NULL, `PrintPriority` int(11) DEFAULT NULL, `DisableHalfHalf` smallint(1) DEFAULT NULL, `BackColour` varchar(20) DEFAULT NULL, `HighlightAdditionalItems` smallint(1) DEFAULT NULL, `ExcludeFromDiscount` smallint(1) DEFAULT NULL, `ExcludeFromRepeat` smallint(1) DEFAULT NULL, `EnablePercent` smallint(1) DEFAULT NULL, `Discount` double DEFAULT NULL, `DisplayPriority` int(11) DEFAULT NULL, `ExtrasColour` varchar(50) DEFAULT NULL, PRIMARY KEY (`SubCategoryID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblSubCategory" & CHAR_PARA & "(`SubCategoryID` int(11) NOT NULL AUTO_INCREMENT, `CategoryID` int(11) DEFAULT NULL, `Description` varchar(255) DEFAULT NULL, `Active` smallint(1) DEFAULT NULL, `Exclude` smallint(1) DEFAULT NULL, `PrintPriority` int(11) DEFAULT NULL, `DisableHalfHalf` smallint(1) DEFAULT NULL, `BackColour` varchar(20) DEFAULT NULL, `HighlightAdditionalItems` smallint(1) DEFAULT NULL, `ExcludeFromDiscount` smallint(1) DEFAULT NULL, `ExcludeFromRepeat` smallint(1) DEFAULT NULL, `EnablePercent` smallint(1) DEFAULT NULL, `Discount` double DEFAULT NULL, `DisplayPriority` int(11) DEFAULT NULL, `ExtrasColour` varchar(50) DEFAULT NULL, PRIMARY KEY (`SubCategoryID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblSubCategoryPrinters" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `SubCategoryID` int(11) DEFAULT NULL, `PrinterNumber` varchar(255) DEFAULT NULL, `Setting` varchar(50) DEFAULT NULL, `Value` varchar(50) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblTable" & CHAR_PARA & "(`TableNoID` varchar(15) NOT NULL, `Status` int(11) DEFAULT NULL, `CurrentSession` int(11) DEFAULT NULL, `Guests` int(11) DEFAULT NULL, `CustomerID` int(11) DEFAULT NULL, `MergeTableRec` varchar(50) DEFAULT NULL, `SplitTable` varchar(50) DEFAULT NULL, `Description` varchar(50) DEFAULT NULL, `SessionDate` varchar(10) DEFAULT NULL, `ModifiedTime` datetime DEFAULT NULL, `TableSession` varchar(255) DEFAULT NULL, PRIMARY KEY (`TableNoID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblTableMenu" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `MenuID` varchar(50) DEFAULT NULL, `MenuPosition` int(11) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=latin1")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblTableSettings" & CHAR_PARA & "(`btnID` varchar(20) NOT NULL, `TopPos` float DEFAULT NULL, `LeftPos` float DEFAULT NULL, `Width` float DEFAULT NULL, `Height` float DEFAULT NULL, `Active` smallint(1) DEFAULT NULL, `MenuID` varchar(20) DEFAULT NULL, `ForeColor` varchar(20) DEFAULT NULL, `BackColor` varchar(20) DEFAULT NULL, `Type` varchar(10) DEFAULT NULL, `TableNoID` varchar(15) DEFAULT NULL, `Description` varchar(255) DEFAULT NULL, PRIMARY KEY (`btnID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblTendertypes" & CHAR_PARA & "(`TenderTypeID` int(11) NOT NULL AUTO_INCREMENT, `TenderType` varchar(50) DEFAULT NULL, `EFTPOSAddress` varchar(50) DEFAULT NULL, `SurchargePLU` varchar(50) DEFAULT NULL, `Hidden` smallint(1) DEFAULT NULL, `Expense` smallint(1) DEFAULT NULL, `XeroAccountCode` varchar(25) DEFAULT NULL, `XeroAccountName` varchar(225) DEFAULT NULL, `SaveAsUnpaid` smallint(1) DEFAULT '0', PRIMARY KEY (`TenderTypeID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblTillCash" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `TillDate` varchar(50) DEFAULT NULL, `FiveCents` int(11) DEFAULT NULL, `TenCents` int(11) DEFAULT NULL, `TwentyCents` int(11) DEFAULT NULL, `FiftyCents` int(11) DEFAULT NULL, `OneDollar` int(11) DEFAULT NULL, `TwoDollars` int(11) DEFAULT NULL, `FiveDollars` int(11) DEFAULT NULL, `TenDollars` int(11) DEFAULT NULL, `TwentyDollars` int(11) DEFAULT NULL, `FiftyDollars` int(11) DEFAULT NULL, `HundredDollars` int(11) DEFAULT NULL, `PaidComputer` varchar(50) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblTransactionStatusTypes" & CHAR_PARA & "(`TransactionStatusID` int(11) NOT NULL AUTO_INCREMENT, `TransactionStatus` varchar(50) DEFAULT NULL, PRIMARY KEY (`TransactionStatusID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblTransactionTypes" & CHAR_PARA & "(`TransactionTypeID` int(11) NOT NULL AUTO_INCREMENT, `TransactionType` varchar(50) DEFAULT NULL, PRIMARY KEY (`TransactionTypeID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblUnsavedOrderDetails" & CHAR_PARA & "(`UnsavedItemID` int(11) NOT NULL AUTO_INCREMENT, `UnsavedOrderID` int(11) DEFAULT NULL, `PLU` varchar(50) DEFAULT NULL, `Description` varchar(100) DEFAULT NULL, `Price` double DEFAULT NULL, `Qty` int(11) DEFAULT NULL, `DeleteDateTime` varchar(50) DEFAULT NULL, `Action` varchar(10) DEFAULT NULL, PRIMARY KEY (`UnsavedItemID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblUnsavedOrderHeaders" & CHAR_PARA & "(`UnsavedOrderID` int(11) NOT NULL AUTO_INCREMENT, `OrderDate` varchar(10) DEFAULT NULL, `StaffID` varchar(50) DEFAULT NULL, `TransactionType` int(11) DEFAULT NULL, `ComputerName` varchar(50) DEFAULT NULL, `CustomerID` int(11) DEFAULT NULL, `CustomerPhone` varchar(50) DEFAULT NULL, `CustomerName` varchar(100) DEFAULT NULL, `StreetNumber` varchar(255) DEFAULT NULL, `StreetName` varchar(50) DEFAULT NULL, `Suburb` varchar(50) DEFAULT NULL, `UnitLevelNumber` varchar(50) DEFAULT NULL, `State` varchar(255) DEFAULT NULL, `GeoAddress` varchar(255) DEFAULT NULL, `PostCode` varchar(50) DEFAULT NULL, `GeoLat` varchar(50) DEFAULT NULL, `GeoLng` varchar(50) DEFAULT NULL, PRIMARY KEY (`UnsavedOrderID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("deliveritsql" & CHAR_PARA & "tblUpgradeLogs" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `UpgradeType` varchar(20) DEFAULT NULL, `OldVersion` varchar(20) DEFAULT NULL, `NewVersion` varchar(20) DEFAULT NULL, `Message` varchar(250) DEFAULT NULL, `UpgradeDateTime` varchar(50) DEFAULT NULL, `Auto` smallint(1) DEFAULT NULL, `MACAddress` varchar(50) DEFAULT NULL, `MachineName` varchar(50) DEFAULT NULL, `IPAddress` varchar(50) DEFAULT NULL, `Source` varchar(200) DEFAULT NULL, `Files` varchar(100) DEFAULT NULL, `OSInfo` varchar(100) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
#End Region

#Region "DPosSysSQL Tables"
        arDPOSTables.Add("dpossyssql" & CHAR_PARA & "tblPrinterSettings" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT,`Printer` varchar(40) NOT NULL,`Setting` varchar(100) NOT NULL, `SettingValue` varchar(100) NOT NULL, `Comments` varchar(510) NOT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
#End Region

#Region "StockSQL Tables"
        arDPOSTables.Add("stocksql" & CHAR_PARA & "tblAreas" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `Code` varchar(40) DEFAULT NULL, `Description` varchar(510) DEFAULT NULL, `SortOrder` varchar(20) DEFAULT NULL, `Active` smallint(1) NOT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("stocksql" & CHAR_PARA & "tblItems" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `Code` varchar(40) DEFAULT NULL, `Description` varchar(510) DEFAULT NULL, `Unit` varchar(100) DEFAULT NULL, `Area` varchar(40) DEFAULT NULL, `Supplier` varchar(40) DEFAULT NULL, `HoldQty` double DEFAULT NULL, `Active` smallint(1) NOT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("stocksql" & CHAR_PARA & "tblItemSuppliers" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `Item` varchar(40) DEFAULT NULL, `Supplier` varchar(40) DEFAULT NULL, `SupplierCode` varchar(100) DEFAULT NULL, `SupplierPrice` double DEFAULT NULL, `SupplierUnit` varchar(100) DEFAULT NULL, `Multiplier` int(11) DEFAULT NULL, `OrderMultiple` int(11) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("stocksql" & CHAR_PARA & "tblSuppliers" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `Code` varchar(40) DEFAULT NULL, `SupName` varchar(100) DEFAULT NULL, `Address` varchar(510) DEFAULT NULL, `Phone` varchar(40) DEFAULT NULL, `Fax` varchar(40) DEFAULT NULL, `Email` varchar(200) DEFAULT NULL, `OrderType` varchar(40) DEFAULT NULL, `OrderFrq` varchar(40) DEFAULT NULL, `Comments` varchar(510) DEFAULT NULL, `Active` smallint(1) NOT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("stocksql" & CHAR_PARA & "tblTransactions" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `ItemCode` varchar(40) DEFAULT NULL, `TransDate` varchar(20) DEFAULT NULL, `TransType` varchar(40) DEFAULT NULL, `Supplier` varchar(40) DEFAULT NULL, `Unit` varchar(100) DEFAULT NULL, `Price` double DEFAULT NULL, `Qty` double DEFAULT NULL, `AdjQty` double DEFAULT NULL, `Received` smallint(1) NOT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
#End Region

#Region "StreetsSQL Tables"
        arDPOSTables.Add("streetssql" & CHAR_PARA & "tblStreets" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `Street` varchar(120) DEFAULT NULL, `Suburb` varchar(120) DEFAULT NULL, `Map` varchar(40) DEFAULT NULL, `MapRef` varchar(40) DEFAULT NULL, `PostCode` varchar(20) DEFAULT NULL, `Other` varchar(100) DEFAULT NULL, `Comments` varchar(510) DEFAULT NULL, `DeliveryZone` varchar(100) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
#End Region

#Region "TimeClockSQL Tables"
        arDPOSTables.Add("timeclocksql" & CHAR_PARA & "tblClockings" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `StaffID` varchar(40) DEFAULT NULL, `ClockingDate` varchar(16) DEFAULT NULL, `InTime` varchar(8) DEFAULT NULL, `OutTime` varchar(8) DEFAULT NULL, `ElapsedMins` varchar(20) DEFAULT NULL, `CostCentre` varchar(40) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("timeclocksql" & CHAR_PARA & "tblClockingsLogs" & CHAR_PARA & "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `UserID` varchar(50) DEFAULT NULL, `StaffID` varchar(50) DEFAULT NULL, `ModifiedField` varchar(50) DEFAULT NULL, `OriginalValue` varchar(100) DEFAULT NULL, `NewValue` varchar(100) DEFAULT NULL, `ModifiedDateTime` varchar(50) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("timeclocksql" & CHAR_PARA & "tblCostCentres" & CHAR_PARA & "(`CostCentre` varchar(40) NOT NULL, `Description` varchar(100) DEFAULT NULL, PRIMARY KEY (`CostCentre`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        arDPOSTables.Add("timeclocksql" & CHAR_PARA & "tblPayroll" & CHAR_PARA & "(`StaffID` varchar(40) NOT NULL, `CostCentre` varchar(40) DEFAULT NULL, `PayrollID` varchar(100) DEFAULT NULL, PRIMARY KEY (`StaffID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
#End Region

#Region "DPos Index"
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "OrderDate")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "CustomerID")
        'arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "TransactionType")
        'arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "TransactionStatus")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "CustomerPhone")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "CustomerName")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "Suburb")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "StreetName")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "StreetNumber")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "TableNo")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "TableDesc")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "DeliveredBy")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "SessionID")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "RequestedDate")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "PickupTime")

        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCategory" & CHAR_PARA & "XeroAccountCode")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCategory" & CHAR_PARA & "XeroAccountName")

        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblPayments" & CHAR_PARA & "OrderID")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblPayments" & CHAR_PARA & "TenderTypeID")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblPayments" & CHAR_PARA & "AmountPaid")

        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCustomers" & CHAR_PARA & "CustomerPhone")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCustomers" & CHAR_PARA & "FullName")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCustomers" & CHAR_PARA & "Suburb")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCustomers" & CHAR_PARA & "StreetName")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCustomers" & CHAR_PARA & "StreetNumber")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCustomers" & CHAR_PARA & "BarCode")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCustomers" & CHAR_PARA & "Latitude")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCustomers" & CHAR_PARA & "Longitude")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCustomers" & CHAR_PARA & "Account")

        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblOrderDetails" & CHAR_PARA & "PLU")

        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblArticles" & CHAR_PARA & "PLU")
        'arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblArticles" & CHAR_PARA & "SubCategoryID")
        'arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblArticles" & CHAR_PARA & "CategoryID")

        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCondimentArticles" & CHAR_PARA & "ArticleID")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblCondimentArticles" & CHAR_PARA & "SubCategoryID")

        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblSettingsTouch" & CHAR_PARA & "BtnID")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblSettingsTouch" & CHAR_PARA & "MenuID")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblSettingsTouch" & CHAR_PARA & "StartDate")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblSettingsTouch" & CHAR_PARA & "EndDate")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblSettingsTouch" & CHAR_PARA & "Repeating")

        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblSpecialsArticles" & CHAR_PARA & "PLU")

        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblTable" & CHAR_PARA & "Status")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblTable" & CHAR_PARA & "SessionDate")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblTable" & CHAR_PARA & "CurrentSession")
        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblTable" & CHAR_PARA & "ModifiedTime")

        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblSettings" & CHAR_PARA & "Setting")

        arDPOSIndex.Add("deliveritsql" & CHAR_PARA & "tblTableSettings" & CHAR_PARA & "TableNoID")
#End Region

#Region "Foreign Keys"
        '              schema                       table                       Column                     RefSchema                    RefTable                    RefColumn
        arDPOSFKs.Add("deliveritsql" & CHAR_PARA & "tblArticles" & CHAR_PARA & "CategoryID" & CHAR_PARA & "deliveritsql" & CHAR_PARA & "tblCategory" & CHAR_PARA & "CategoryID")
        arDPOSFKs.Add("deliveritsql" & CHAR_PARA & "tblArticles" & CHAR_PARA & "SubCategoryID" & CHAR_PARA & "deliveritsql" & CHAR_PARA & "tblSubCategory" & CHAR_PARA & "SubCategoryID")

        arDPOSFKs.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "TransactionStatus" & CHAR_PARA & "deliveritsql" & CHAR_PARA & "tblTransactionStatusTypes" & CHAR_PARA & "TransactionStatusID")
        arDPOSFKs.Add("deliveritsql" & CHAR_PARA & "tblOrderHeaders" & CHAR_PARA & "TransactionType" & CHAR_PARA & "deliveritsql" & CHAR_PARA & "tblTransactionTypes" & CHAR_PARA & "TransactionTypeID")

        arDPOSFKs.Add("deliveritsql" & CHAR_PARA & "tblSpecialsArticles" & CHAR_PARA & "ArticleID" & CHAR_PARA & "deliveritsql" & CHAR_PARA & "tblArticles" & CHAR_PARA & "ArticleID")

        arDPOSFKs.Add("deliveritsql" & CHAR_PARA & "tblSubCategory" & CHAR_PARA & "CategoryID" & CHAR_PARA & "deliveritsql" & CHAR_PARA & "tblCategory" & CHAR_PARA & "CategoryID")
#End Region
    End Sub

    Public Function ChangeIndexSetting(ByVal sDB As String, ByVal sTable As String, ByVal bSetting As Boolean) As Boolean
        Dim bReturn As Boolean
        Dim sSQL As String
        Dim sProcess As String

        If bSetting = True Then
            sSQL = "ALTER TABLE `" & sDB & "`.`" & sTable & "` ENABLE KEYS"
        Else
            sSQL = "ALTER TABLE `" & sDB & "`.`" & sTable & "` DISABLE KEYS"
        End If

        sProcess = ProcessMySQL(sSQL, GetConnectionStringMY(sDB))

        Return bReturn
    End Function

    Public Function ChangeIndexSettingByDatabase(ByVal sDB As String, ByVal bSetting As Boolean) As Boolean
        Dim bReturn As Boolean = False
        Dim thisarrTables As ArrayList
        thisarrTables = FillDatabaseStructureMySQL(sDB)

        For Each thisTable As String In thisarrTables
            Dim thisTableInfo As String()
            thisTableInfo = thisTable.Split(CHAR_PARA)
            If ChangeIndexSetting(sDB, thisTableInfo(1), bSetting) = True Then
                bReturn = True
            Else
                Exit For
            End If
        Next

        Return bReturn
    End Function

    Public Function GetAllColumnScriptInTableArraylist(ByVal sTableName As String, ByVal sDBName As String) As String
        'arDPOSTables.Add("timeclocksql" & CHAR_PARA & "tblPayroll" & CHAR_PARA & "(`StaffID` varchar(40) NOT NULL, `CostCentre` varchar(40) DEFAULT NULL, `PayrollID` varchar(100) DEFAULT NULL, PRIMARY KEY (`StaffID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8")
        For Each thisTable As String In arDPOSTables
            Dim thisTableInfo As String()

            thisTableInfo = thisTable.Split(CHAR_PARA)

            If sDBName = thisTableInfo(0) AndAlso sTableName = thisTableInfo(1) Then
                Return thisTableInfo(2)
            End If
        Next

        Return ""
    End Function

    Public Function GetAllColumnHeaders(ByVal sTableName As String, ByVal sSchema As String) As String
        Dim sReturn As New StringBuilder

        Application.DoEvents()

        Select Case sTableName
            Case "Audit"
                sReturn.Append("RecordID,Type,TableName,PK,Data,UpdateDate,Synced")
            Case "tblAreas"
                sReturn.Append("RecID,Code,Description,SortOrder,Active")
            Case "tblArticles"
                sReturn.Append("ArticleID,Description,PLU,SellShop,SellDelivery,SellTable,SellCost,HideDelivery,HidePickup,HideTable,CategoryID,SubCategoryID," _
                    & "DateCreated,LastUpdated,Active,GSTExempt,Special,ArticleType,UsePercent,LoyaltyItem,SellSpecial,Topping1,Topping2,Topping3,Topping4,Topping5,Topping6,Topping7," _
                    & "Topping8,Topping9,Topping10,Topping11,Topping12,Topping13,Topping14,Topping15,PrintZeroAmt,StickerPrint,UpsellItem,ExcludeCondimentCharge,ExcludeFromMinimum")
            Case "tblAudit"
                sReturn.Append("RecID,AuditType,StaffID,ComputerName,OldOrderID,OrderID,PLU,ModifiedDateTime,OldPrice,NewPrice")
            Case "tblBalanceSheet"
                sReturn.Append("RecID,SheetDate,Name,ShopSales,PickUpSales,DeliverySales,TableSales,TotalSales,ExcludedSales,TotalCash,TotalPaid,TotalUnpaid," _
                                    & "TillFloat,TotalExpenses,TotalReceipts,TotalBalance,UnderOver,Notes,PaidComputer,OOPaidAmt,OOPaidOnlineAmt,OOPaidInStoreAmt,OOUnpaidAmt")
            Case "tblBalanceSheetExpenses"
                sReturn.Append("RecID,ExpenseDate,Description,ExpenseAmt,PaidComputer")
            Case "tblBalanceSheetPayments"
                sReturn.Append("RecID,PaymentDate,PaymentType,PaymentTypeID,Calc,BSUser,PaidComputer")
            Case "tblButtons"
                sReturn.Append("RecID,ButtonID,Text,Action,BackColour,ForeColour")
            Case "tblCategory"
                sReturn.Append("CategoryID,Description,Active,XeroAccountCode,XeroAccountName,XeroGSTExempt")
            Case "tblClockings"
                sReturn.Append("RecID,StaffID,ClockingDate,InTime,OutTime,ElapsedMins,CostCentre")
            Case "tblClockingsLogs"
                sReturn.Append("RecID,UserID,StaffID,ModifiedField,OriginalValue,NewValue,ModifiedDateTime")
            Case "tblCommentsHistory"
                sReturn.Append("ID,CustomerID,NoteDate,NoteText,UserID,Status,CompleteDate")
            Case "tblCondimentArticles"
                sReturn.Append("RecID,SubCategoryID,Price,ArticleID")
            Case "tblCostCentres"
                sReturn.Append("CostCentre,Description")
            Case "tblCustomers"
                sReturn.Append("CustomerID,CustomerPhone,FullName,StreetNumber,StreetName,Suburb,CommentsNoPrint,Comments,OldOrderValue,OldOrderNumber,Barcode,AutoLoadPLU," _
                                    & "Account,EmailUpdated,DateJoined,PostCode,DisableLoyalty,UserField1,UserField2,UserField3,UserField4,UserField5,UserField6,UserField7,UserField8," _
                                    & "UserField9,VIPP,Email,CrossStreet,ApartmentNumber,Floor,BuildingName,ExtractExclude,Latitude,Longitude,UnitLevelNumber,State,GeoAddress")
            Case "tblCustomerSummary"
                sReturn.Append("RecID,CustomerID,FirstOrderDate,LastOrderDate,ModifiedDate,TotalNumberOrders,TotalValueOrders,AverageOrderValue")
            Case "tblItems"
                sReturn.Append("RecID,Code,Description,Unit,Area,Supplier,HoldQty,Active")
            Case "tblItemSuppliers"
                sReturn.Append("RecID,Item,Supplier,SupplierCode,SupplierPrice,Multiplier,OrderMultiple")
            Case "tblLicence"
                sReturn.Append("RecID,Setting")
            Case "tblOccasions"
                sReturn.Append("RecID,CustomerID,Description,OccasionDate,Category")
            Case "tblOrderDetails"
                sReturn.Append("OrderID,OrderItem,PLU,Description,Qty,UnitSell,HasSubItems,ParentItem,ParentType,LoyaltyRedeemed,SpecialRecID,SplitItemID")
            Case "tblOrderHeaders"
                sReturn.Append("OrderID,TakenBy,OrderDate,DeliveredBy,LocationID,TenderedAmt,Comments,TransactionStatus,CustomerID,PickupTime,TransactionType,CustomerName," _
                                    & "CustomerPhone,StreetNumber,StreetName,Suburb,Map,MapReference,AddressComments,InTime,OutTime,ModifiedTime,Notes,Paid,NewOrderID,OldOrderID,ExcludeAmt," _
                                    & "Guests,LoyaltyAccrued,LoyaltyRedeemed,SessionID,OnlineOrder,Cashier,TableNo,CommentsNoPrint,StatusComments,ComputerName,MakeDoneTime,TableDesc," _
                                    & "OnlineRecID,DeliveryZone,CrossStreet,ApartmentNumber,Floor,BuildingName,WebAmountPaid,IsAsap,CCTransactionNumber,CCNumber,CCExpiryDate,PaidComputer,PaidDate," _
                                    & "ChangeAmount,OnlinePaymentMethod,OnlineOrderSource,SessionName,AccountNumber,TransactionID,YelloOrderID,LOKETransactionID," _
                                    & "LOKEReferenceID,OOTransactionID,UnitLevelNumber,State,PostCode,GeoAddress,GeoLat,GeoLng,RequestedDate," _
                                    & "PosReferenceID,MakeStartTime")

            Case "tblPayments"
                sReturn.Append("RecID,OrderID,TenderTypeID,AmountPaid")
            Case "tblPaymentSessions"
                sReturn.Append("RecID,OrderID,PLU,SplitItemID,PaidAmount,ItemAmount,Adjustment")
            Case "tblPayroll"
                sReturn.Append("StaffID,CostCentre,PayrollID")
            Case "tblPrinters"
                sReturn.Append("RecID,PrinterID,PrinterName,PrinterUNC,DocketType,PrinterType,PrintItems,Comments,Alarm,Interactive,PrinterUNC2,PrinterType2")
            Case "tblPrinterSettings"
                sReturn.Append("RecID,Printer,Setting,SettingValue,Comments")
            Case "tblProfile"
                sReturn.Append("ProfileID,ProfileDescription")
            Case "tblProfileExclusion"
                sReturn.Append("RecID,ProfileID,Exclusion")
            Case "tblPublicHolidays"
                sReturn.Append("RecID,StartDate,EndDate,SurchargePLU")
            Case "tblRedemptions"
                sReturn.Append("OrderID,CustomerID,PointsRedeemed,Description,OrderDate")
            Case "tblSettings"
                sReturn.Append("RecID,Setting,DefaultValue,Comments,Category,SettingValue")
            Case "tblSettingsLocal"
                sReturn.Append("RecID,ComputerName,Setting,SettingValue")
            Case "tblSettingsTouch"
                sReturn.Append("RecID,MenuID,BtnID,Description,BtnType,TypeID,ForeColor,BackColor,Active,Message,PopUp,BackgroundImage,ThemeForeColor,Hidden,TouchMultiplePopUps,SubItem,StartDate,EndDate,Repeating")
            Case "tblSpecialsArticles"
                sReturn.Append("RecID,ArticleID,PLU,Description,Priority,SubCategoryID,SubMenu,IgnoreSpecialPrice,CountAsDealItem,IncludeInTotal,ChargeItem")
            Case "tblSpecialsSettings"
                sReturn.Append("RecID,PLU,ButtonColour,ButtonText,FontColour,SortNumber")
            Case "tblStaff"
                sReturn.Append("StaffID,FirstName,LastName,Comments,Comments1,Psw,ProfileID,StartDate,PhoneNumber,MobileNumber,DeliveryRate,HourlyRate,Active,LeftIndexFinger,RightIndexFinger," _
                                    & "DailyRate,Shift,Pincode,AppEnabled")
            Case "tblStreets"
                sReturn.Append("RecID,Street,Suburb,Map,MapRef,PostCode,Other,Comments,DeliveryZone")
            'Case "tblSubCategory"
            '    sReturn.Append("SubCategoryID,CategoryID,Description,Printers,PrintBold,Active,Exclude,ShopPrint,PickUpPrint,DeliveryPrint,TablePrint,Highlight,PrintPriority,DisableHalfHalf," _
            '        & "BackColour,HighlightAdditionalItems,ExcludeFromDiscount,ExcludeFromRepeat,EnablePercent,Discount,DisplayPriority,ExtrasColour")
            Case "tblSubCategory"
                sReturn.Append("SubCategoryID,CategoryID,Description,Active,Exclude,PrintPriority,DisableHalfHalf,BackColour,HighlightAdditionalItems,ExcludeFromDiscount,ExcludeFromRepeat,EnablePercent,Discount,DisplayPriority,ExtrasColour")
            Case "tblSubCategoryPrinters"
                sReturn.Append("RecID,SubCategoryID,PrinterNumber,Setting,Value")
            Case "tblSuppliers"
                sReturn.Append("RecID,Code,SupName,Address,Phone,Fax,Email,OrderType,OrderFrq,Comments,Active")
            Case "tblTable"
                sReturn.Append("TableNoID,Status,CurrentSession,Guests,CustomerID,MergeTableRec,SplitTable,Description,SessionDate,ModifiedTime,TableSession")
            Case "tblTableSettings"
                sReturn.Append("btnID,TopPos,LeftPos,Width,Height,Active,MenuID,ForeColor,BackColor,Type,TableNoID,Description")
            Case "tblTendertypes"
                sReturn.Append("TenderTypeID,TenderType,EFTPOSAddress,SurchargePLU,Hidden,Expense,XeroAccountCode,XeroAccountName,SaveAsUnpaid")
            Case "tblTillCash"
                sReturn.Append("RecID,TillDate,FiveCents,TenCents,TwentyCents,FiftyCents,OneDollar,TwoDollars,FiveDollars,TenDollars,TwentyDollars,FiftyDollars,HundredDollars,PaidComputer")
            Case "tblTransactions"
                sReturn.Append("RecID,ItemCode,TransDate,TransType,Supplier,Unit,Price,Qty,AdjQty,Received")
            Case "tblTransactionStatusTypes"
                sReturn.Append("TransactionStatusID,TransactionStatus")
            Case "tblTransactionTypes"
                sReturn.Append("TransactionTypeID,TransactionType")
            Case "tblUnsavedOrderDetails"
                sReturn.Append("UnsavedItemID,UnsavedOrderID,PLU,Description,Price,Qty,DeleteDateTime,Action")
            Case "tblUnsavedOrderHeaders"
                sReturn.Append("UnsavedOrderID,OrderDate,StaffID,TransactionType,ComputerName,CustomerID,CustomerPhone,CustomerName,StreetNumber,StreetName,Suburb,UnitLevelNumber," _
                                    & "State,PostCode,GeoAddress,GeoLat,GeoLng")
            Case "tblUpgradeLogs"
                sReturn.Append("RecID,UpgradeType,OldVersion,NewVersion,Message,UpgradeDateTime,Auto,MACAddress,MachineName,IPAddress,Source,Files,OSInfo")
            Case "tblTableMenu"
                sReturn.Append("RecID,MenuID,MenuPosition")
        End Select

        Return sReturn.ToString
    End Function

    Public Function GetExportScript(ByVal sTablename As String, ByVal sDestination As String, ByVal sFileName As String) As String
        Dim sScript As String = ""

        Application.DoEvents()

        Try
            Select Case sTablename
                Case "Audit"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecordID, CHAR(34) + Type + CHAR(34), CHAR(34) + TableName + CHAR(34), CHAR(34) + PK + CHAR(34), DeliveritSQL.dbo.fn_Esc(Data), (CASE WHEN CONVERT(varchar, UpdateDate, 121) IS NULL THEN ''NULL'' ELSE CHAR(34) + CONVERT(varchar, UpdateDate, 121) + CHAR(34) END), CHAR(34) + CONVERT(varchar(10),ISNULL(Synced,0)) + CHAR(34) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[Audit] where Synced!=1"" queryout """ & sDestination & sFileName & """ -c -T -t"",""'"
                Case "tblAreas"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + Code + char(34), DeliveritSQL.dbo.fn_Esc(Description), char(34) + SortOrder + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(Active,0)) + char(34) FROM [StockSQL].[dbo].[tblAreas]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblArticles"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT ArticleID, DeliveritSQL.dbo.fn_Esc(Description), char(34) + PLU + char(34), char(34) + CONVERT(varchar(20), ISNULL(SellShop,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(SellDelivery,0)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(SellTable,0)) + char(34), CONVERT(varchar(20), ISNULL(SellCost,0)), char(34) + CONVERT(varchar(10), ISNULL(HideDelivery,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(HidePickup,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(HideTable,0)) + char(34), char(34) + CONVERT(varchar(11), ISNULL(CategoryID,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(11), ISNULL(SubCategoryID,0)) + char(34), (CASE WHEN CONVERT(varchar, DateCreated, 121) IS NULL THEN ''NULL'' ELSE CHAR(34) + CONVERT(varchar, DateCreated, 121) + CHAR(34) END), (CASE WHEN CONVERT(varchar, LastUpdated, 121) IS NULL THEN ''NULL'' ELSE CHAR(34) + CONVERT(varchar, LastUpdated, 121) + CHAR(34) END), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(Active,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(GSTExempt,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(Special,0)) + char(34), "
                    sScript &= "char(34) + ArticleType + char(34), char(34) + CONVERT(varchar(10), ISNULL(UsePercent,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(LoyaltyItem,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(SellSpecial,0)) + char(34), char(34) + Topping1 + char(34), char(34) + Topping2 + char(34), char(34) + Topping3 + char(34), "
                    sScript &= "char(34) + Topping4 + char(34), char(34) + Topping5 + char(34), char(34) + Topping6 + char(34), char(34) + Topping7 + char(34), char(34) + Topping8 + char(34), "
                    sScript &= "char(34) + Topping9 + char(34), char(34) + Topping10 + char(34), char(34) + Topping11 + char(34), char(34) + Topping12 + char(34), char(34) + Topping13 + char(34), "
                    sScript &= "char(34) + Topping14 + char(34), char(34) + Topping15 + char(34), char(34) + CONVERT(varchar(10), ISNULL(PrintZeroAmt,0)) + char(34), char(34) + StickerPrint + char(34), ISNULL(UpsellItem, 0), ISNULL(ExcludeCondimentCharge, 0), ISNULL(ExcludeFromMinimum, 0) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblArticles]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblAudit"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + AuditType + char(34), char(34) + StaffID + char(34), char(34) + ComputerName + char(34), OldOrderID, OrderID, char(34) + PLU + char(34), "
                    sScript &= "char(34) + ModifiedDateTime + char(34), ISNULL(OldPrice,0), ISNULL(NewPrice,0) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblAudit]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblBalanceSheet"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + SheetDate + char(34), char(34) + Name + char(34), char(34) + CONVERT(varchar(20), ISNULL(ShopSales,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(PickUpSales,0)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(DeliverySales,0)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(TableSales,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(TotalSales,0)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(ExcludedSales,0)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(TotalCash,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(TotalPaid,0)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(TotalUnpaid,0)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(TillFloat,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(TotalExpenses,0)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(TotalReceipts,0)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(TotalBalance,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(UnderOver,0)) + char(34), char(34) + Notes + char(34), char(34) + PaidComputer + char(34), ISNULL(OOPaidAmt,0), "
                    sScript &= "ISNULL(OOPaidOnlineAmt,0), ISNULL(OOPaidInStoreAmt,0), ISNULL(OOUnpaidAmt,0) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblBalanceSheet]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblBalanceSheetExpenses"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + ExpenseDate + char(34), char(34) + Description + char(34), char(34) + CONVERT(varchar(20), ISNULL(ExpenseAmt,0)) + char(34), "
                    sScript &= "char(34) + PaidComputer + char(34) FROM [DeliveritSQL].[dbo].[tblBalanceSheetExpenses]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblBalanceSheetPayments"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + PaymentDate + char(34), char(34) + PaymentType + char(34), PaymentTypeID, char(34) + CONVERT(varchar(20), Calc) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), BSUser) + char(34), char(34) + PaidComputer + char(34) FROM [DeliveritSQL].[dbo].[tblBalanceSheetPayments]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblButtons"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, ButtonID, DeliveritSQL.dbo.fn_Esc(Text), char(34) + Action + char(34), char(34) + BackColour + char(34), char(34) + ForeColour + char(34) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblButtons]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblCategory"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT CategoryID, char(34) + Description + char(34), char(34) + CONVERT(varchar(10), ISNULL(Active,0)) + char(34), char(34) + XeroAccountCode + char(34), "
                    sScript &= "char(34) + XeroAccountName + char(34), ISNULL(XeroGSTExempt,0) FROM [DeliveritSQL].[dbo].[tblCategory]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblClockings"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + StaffID + char(34), char(34) + ClockingDate + char(34), char(34) + InTime + char(34), char(34) + OutTime + char(34), "
                    sScript &= "char(34) + ElapsedMins + char(34), char(34) + CostCentre + char(34) FROM [TimeClockSQL].[dbo].[tblClockings]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblClockingsLogs"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + UserID + char(34), char(34) + StaffID + char(34), char(34) + ModifiedField + char(34), char(34) + OriginalValue + char(34), "
                    sScript &= "char(34) + NewValue + char(34), char(34) + ModifiedDateTime + char(34) FROM [TimeClockSQL].[dbo].[tblClockingsLogs]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblCommentsHistory"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT ID, CustomerID, char(34) + NoteDate + char(34), DeliveritSQL.dbo.fn_Esc(NoteText), char(34) + UserID + char(34), char(34) + CONVERT(varchar(10), ISNULL(Status,0)) + char(34), "
                    sScript &= "char(34) + CompleteDate + char(34) FROM [DeliveritSQL].[dbo].[tblCommentsHistory]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblCondimentArticles"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID,SubCategoryID, char(34) + CONVERT(varchar(20), ISNULL(Price,0)) + char(34), ISNULL(ArticleID,0) FROM [DeliveritSQL].[dbo].[tblCondimentArticles]"""
                    sScript &= " queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblCostCentres"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT char(34) + CostCentre + char(34), char(34) + Description + char(34) FROM [TimeClockSQL].[dbo].[tblCostCentres]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblCustomers"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT CustomerID, char(34) + CustomerPhone + char(34), char(34) + FullName + char(34), char(34) + StreetNumber + char(34), char(34) + StreetName + char(34), "
                    sScript &= "char(34) + Suburb + char(34), DeliveritSQL.dbo.fn_Esc(CommentsNoPrint), DeliveritSQL.dbo.fn_Esc(Comments), char(34) + CONVERT(varchar(20), ISNULL(OldOrderValue,0)) + char(34), "
                    sScript &= "char(34) + ISNULL(OldOrderNumber,0) + char(34), char(34) + Barcode + char(34), char(34) + AutoLoadPLU + char(34), char(34) + CONVERT(varchar(10), ISNULL(Account,0)) + char(34), "
                    sScript &= "char(34) + EmailUpdated + char(34), char(34) + DateJoined + char(34), char(34) + PostCode + char(34), char(34) + CONVERT(varchar(10), ISNULL(DisableLoyalty,0)) + char(34), "
                    sScript &= "char(34) + UserField1 + char(34), char(34) + UserField2 + char(34), char(34) + UserField3 + char(34), char(34) + UserField4 + char(34), char(34) + UserField5 + char(34), "
                    sScript &= "char(34) + UserField6 + char(34), char(34) + UserField7 + char(34), char(34) + UserField8 + char(34), char(34) + UserField9 + char(34), char(34) + CONVERT(varchar(10), ISNULL(VIPP,0)) + char(34), "
                    sScript &= "char(34) + Email + char(34), char(34) + CrossStreet + char(34), char(34) + ApartmentNumber + char(34), char(34) + Floor + char(34), char(34) + BuildingName + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(ExtractExclude,0)) + char(34), char(34) + Latitude + char(34), char(34) + Longitude + char(34), "
                    sScript &= "char(34) + UnitLevelNumber + char(34), char(34) + State + char(34), char(34) + GeoAddress + char(34) FROM [DeliveritSQL].[dbo].[tblCustomers]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblCustomerSummary"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, ISNULL(CustomerID,0), char(34) + FirstOrderDate + char(34), char(34) + LastOrderDate + char(34), "
                    sScript &= "char(34) + ModifiedDate + char(34), ISNULL(TotalNumberOrders, 0), ISNULL(TotalValueOrders, 0), ISNULL(AverageOrderValue, 0) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblCustomerSummary]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblItems"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + Code + char(34), char(34) + Description + char(34), char(34) + Unit + char(34), char(34) + Area + char(34), "
                    sScript &= "char(34) + Supplier + char(34), char(34) + CONVERT(varchar(20), ISNULL(HoldQty,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(Active,0)) + char(34) "
                    sScript &= "FROM [StockSQL].[dbo].[tblItems]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblItemSuppliers"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + Item + char(34), char(34) + Supplier + char(34), char(34) + SupplierCode + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(SupplierPrice,0)) + char(34), ISNULL(Multiplier,0), ISNULL(OrderMultiple,0) FROM [StockSQL].[dbo].[tblItemSuppliers]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblLicence"
                    'sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + Setting + char(34), DeleteFlag = 0, ClientID = " & sClientIdInt & " FROM [DeliveritSQL].[dbo].[tblLicence]"" queryout " & sDestination & sFileName & " -w -T -t"",""'"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, DeliveritSQL.dbo.fn_Esc(Setting) FROM [DeliveritSQL].[dbo].[tblLicence]"" queryout " & sDestination & sFileName & " -w -T -t"",""'"
                Case "tblOccasions"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, ISNULL(CustomerID,0), char(34) + Description + char(34), char(34) + OccasionDate + char(34), char(34) + Category + char(34) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblOccasions]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblOrderDetails"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT tblOrderDetails.OrderID, ISNULL(OrderItem,0), char(34) + PLU + char(34), DeliveritSQL.dbo.fn_Esc(Description), ISNULL(Qty,0), char(34) + CONVERT(varchar(20), ISNULL(UnitSell,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(HasSubItems,0)) + char(34), ISNULL(ParentItem,0), ISNULL(ParentType,0), ISNULL(tblOrderDetails.LoyaltyRedeemed,0), ISNULL(SpecialRecID,0), ISNULL(SplitItemID,0) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblOrderDetails], [DeliveritSQL].[dbo].[tblOrderHeaders] where tblOrderDetails.OrderID = tblOrderHeaders.OrderID"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblOrderHeaders"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT OrderID, char(34) + TakenBy + char(34), (CASE WHEN CONVERT(varchar, OrderDate, 121) IS NULL THEN ''NULL'' ELSE CHAR(34) + CONVERT(varchar, OrderDate, 121) + CHAR(34) END), char(34) + DeliveredBy + char(34), ISNULL(LocationID,0), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(TenderedAmt,0)) + char(34), DeliveritSQL.dbo.fn_Esc(Comments), ISNULL(TransactionStatus,0), ISNULL(CustomerID,0), "
                    sScript &= "char(34) + PickupTime + char(34), ISNULL(TransactionType,0), char(34) + CustomerName + char(34), char(34) + CustomerPhone + char(34), char(34) + StreetNumber + char(34), "
                    sScript &= "char(34) + StreetName + char(34), char(34) + Suburb + char(34), char(34) + Map + char(34), char(34) + MapReference + char(34), DeliveritSQL.dbo.fn_Esc(AddressComments), "
                    sScript &= "char(34) + InTime + char(34), char(34) + OutTime + char(34), char(34) + ModifiedTime + char(34), DeliveritSQL.dbo.fn_Esc(Notes), char(34) + CONVERT(varchar(10), ISNULL(Paid,0)) + char(34), "
                    sScript &= "ISNULL(NewOrderID,0), ISNULL(OldOrderID,0), char(34) + CONVERT(varchar(20), ISNULL(ExcludeAmt,0)) + char(34), ISNULL(Guests,0), ISNULL(LoyaltyAccrued,0), ISNULL(LoyaltyRedeemed,0), ISNULL(SessionID,0), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(OnlineOrder,0)) + char(34), char(34) + Cashier + char(34), char(34) + TableNo + char(34), DeliveritSQL.dbo.fn_Esc(CommentsNoPrint), "
                    sScript &= "DeliveritSQL.dbo.fn_Esc(StatusComments), char(34) + ComputerName + char(34), char(34) + MakeDoneTime + char(34), char(34) + TableDesc + char(34), "
                    sScript &= "char(34) + OnlineRecID + char(34), char(34) + DeliveryZone + char(34), char(34) + CrossStreet + char(34), char(34) + ApartmentNumber + char(34), char(34) + Floor + char(34), "
                    sScript &= "char(34) + BuildingName + char(34), char(34) + WebAmountPaid + char(34), char(34) + CONVERT(varchar(10), ISNULL(IsAsap,0)) + char(34), char(34) + CCTransactionNumber + char(34), "
                    sScript &= "char(34) + CCNumber + char(34), char(34) + CCExpiryDate + char(34), char(34) + PaidComputer + char(34), char(34) + PaidDate + char(34), char(34) + ChangeAmount + char(34), "
                    sScript &= "char(34) + OnlinePaymentMethod + char(34), char(34) + OnlineOrderSource + char(34), char(34) + SessionName + char(34), ISNULL(AccountNumber,0), char(34) + TransactionID + char(34), "
                    sScript &= "char(34) + YelloOrderID + char(34), char(34) + LOKETransactionID + char(34), char(34) + LOKEReferenceID + char(34), "
                    sScript &= "char(34) + OOTransactionID + char(34), char(34) + UnitLevelNumber + char(34), char(34) + State + char(34), char(34) + PostCode + char(34), char(34) + GeoAddress + char(34), char(34) + GeoLat + char(34), char(34) + GeoLng + char(34), char(34) + RequestedDate + char(34), "
                    sScript &= "char(34) + PosReferenceID + char(34), char(34) + MakeStartTime + char(34) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblOrderHeaders]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblPayments"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, OrderID, ISNULL(TenderTypeID,0), ISNULL(AmountPaid,0) FROM [DeliveritSQL].[dbo].[tblPayments]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblPaymentSessions"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, ISNULL(OrderID,0), char(34) + PLU + char(34), ISNULL(SplitItemID,0), char(34) + CONVERT(varchar(20), ISNULL(PaidAmount,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(ItemAmount,0)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(Adjustment,0)) + char(34) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblPaymentSessions]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblPayroll"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT char(34) + StaffID + char(34), char(34) + CostCentre + char(34), char(34) + PayrollID + char(34) "
                    sScript &= "FROM [TimeClockSQL].[dbo].[tblPayroll]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblPrinters"
                    'sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + PrinterID + char(34), char(34) + PrinterName + char(34), char(34) + PrinterUNC + char(34), "
                    'sScript &= "char(34) + DocketType + char(34), char(34) + PrinterType + char(34), char(34) + PrintItems + char(34), char(34) + Comments + char(34), "
                    'sScript &= "char(34) + CONVERT(varchar(10), Alarm) + char(34), char(34) + CONVERT(varchar(10), Interactive) + char(34), DeleteFlag = 0, ClientID = " & sClientIdInt & " "
                    'sScript &= "FROM [DeliveritSQL].[dbo].[tblPrinters]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, DeliveritSQL.dbo.fn_Esc(PrinterID), DeliveritSQL.dbo.fn_Esc(PrinterName), DeliveritSQL.dbo.fn_Esc(PrinterUNC), "
                    sScript &= "char(34) + DocketType + char(34), DeliveritSQL.dbo.fn_Esc(PrinterType), char(34) + PrintItems + char(34), DeliveritSQL.dbo.fn_Esc(Comments), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(Alarm,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(Interactive,0)) + char(34), DeliveritSQL.dbo.fn_Esc(PrinterUNC2), DeliveritSQL.dbo.fn_Esc(PrinterType2) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblPrinters]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblPrinterSettings"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + Printer + char(34), char(34) + Setting + char(34), char(34) + SettingValue + char(34), char(34) + Comments + char(34) "
                    sScript &= "FROM [DPosSysSQL].[dbo].[tblPrinterSettings]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblProfile"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT ProfileID, char(34) + ProfileDescription + char(34) FROM [DeliveritSQL].[dbo].[tblProfile]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblProfileExclusion"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + ProfileID + char(34), char(34) + Exclusion + char(34) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblProfileExclusion]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblPublicHolidays"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + StartDate + char(34), char(34) + EndDate + char(34), char(34) + SurchargePLU + char(34) FROM [DeliveritSQL].[dbo].[tblPublicHolidays]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblRedemptions"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT OrderID, ISNULL(CustomerID,0), ISNULL(PointsRedeemed,0), char(34) + Description + char(34), char(34) + OrderDate + char(34) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblRedemptions]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblSettings"
                    'sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + Setting + char(34), char(34) + DefaultValue + char(34), char(34) + Comments + char(34), "
                    'sScript &= "char(34) + Category + char(34), char(34) + SettingValue + char(34), DeleteFlag = 0, ClientID = " & sClientIdInt & " FROM [DeliveritSQL].[dbo].[tblSettings]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + Setting + char(34), DeliveritSQL.dbo.fn_Esc(DefaultValue), DeliveritSQL.dbo.fn_Esc(Comments), "
                    sScript &= "char(34) + Category + char(34), DeliveritSQL.dbo.fn_Esc(SettingValue) FROM [DeliveritSQL].[dbo].[tblSettings]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblSettingsLocal"
                    'sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + ComputerName + char(34), char(34) + Setting + char(34), char(34) + SettingValue + char(34), "
                    'sScript &= "DeleteFlag = 0, ClientID = " & sClientIdInt & " FROM [DeliveritSQL].[dbo].[tblSettingsLocal]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, DeliveritSQL.dbo.fn_Esc(ComputerName), char(34) + Setting + char(34), DeliveritSQL.dbo.fn_Esc(SettingValue) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblSettingsLocal]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblSettingsTouch"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + MenuID + char(34), ISNULL(BtnID,0), char(34) + Description + char(34), char(34) + BtnType + char(34), "
                    sScript &= "char(34) + TypeID + char(34), char(34) + ForeColor + char(34), char(34) + BackColor + char(34), ISNULL(Active,0), char(34) + Message + char(34), char(34) + PopUp + char(34), "
                    sScript &= "char(34) + BackgroundImage + char(34), char(34) + ThemeForeColor + char(34), ISNULL(Hidden,0), char(34) + CONVERT(varchar(10), ISNULL(TouchMultiplePopUps,0)) + char(34), "
                    sScript &= "ISNULL(SubItem,0), char(34) + StartDate + char(34), char(34) + EndDate + char(34),  char(34) + Repeating + char(34) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblSettingsTouch]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblSpecialsArticles"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, ISNULL(ArticleID,0), char(34) + PLU + char(34), DeliveritSQL.dbo.fn_Esc(Description), ISNULL(Priority,0), SubCategoryID = 0, "
                    sScript &= "char(34) + SubMenu + char(34), char(34) + CONVERT(varchar(10), ISNULL(IgnoreSpecialPrice,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(CountAsDealItem,0)) + char(34), "
                    sScript &= "ISNULL(IncludeInTotal,0), "
                    sScript &= "ISNULL(ChargeItem, 0) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblSpecialsArticles]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblStaff"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT char(34) + StaffID + char(34), char(34) + FirstName + char(34), char(34) + LastName + char(34), "
                    sScript &= "DeliveritSQL.dbo.fn_Esc(Comments), DeliveritSQL.dbo.fn_Esc(Comments1), char(34) + Psw + char(34), char(34) + ProfileID + char(34), char(34) + StartDate + char(34), "
                    sScript &= "char(34) + PhoneNumber + char(34), char(34) + MobileNumber + char(34), char(34) + CONVERT(varchar(20), ISNULL(DeliveryRate,0.00)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(HourlyRate,0.00)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(Active,0)) + char(34), char(34) + LeftIndexFinger + char(34), "
                    sScript &= "char(34) + RightIndexFinger + char(34), char(34) + CONVERT(varchar(20), ISNULL(DailyRate,0.00)) + char(34), char(34) + Shift + char(34), char(34) + Pincode + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(AppEnabled,0)) + char(34) FROM [DeliveritSQL].[dbo].[tblStaff]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblStreets"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + Street + char(34), char(34) + Suburb + char(34), char(34) + Map + char(34), "
                    sScript &= "char(34) + MapRef + char(34), char(34) + PostCode + char(34), char(34) + Other + char(34), DeliveritSQL.dbo.fn_Esc(Comments), "
                    sScript &= "char(34) + DeliveryZone + char(34) FROM [StreetsSQL].[dbo].[tblStreets]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblSubCategory"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT SubCategoryID, ISNULL(CategoryID,0), char(34) + Description + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(Active,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(Exclude,0)) + char(34), "
                    sScript &= "ISNULL(PrintPriority,0), char(34) + CONVERT(varchar(10), ISNULL(DisableHalfHalf,0)) + char(34), "
                    sScript &= "char(34) + BackColour + char(34), char(34) + CONVERT(varchar(10), ISNULL(HighlightAdditionalItems,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(ExcludeFromDiscount,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(ExcludeFromRepeat,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(EnablePercent,0)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(Discount,0.00)) + char(34), "
                    sScript &= "ISNULL(DisplayPriority,0), char(34) + ExtrasColour + char(34) FROM [DeliveritSQL].[dbo].[tblSubCategory]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblSubCategoryPrinters"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, ISNULL(SubCategoryID,0), char(34) + PrinterNumber + char(34), char(34) + Setting + char(34), char(34) + Value + char(34) FROM [DeliveritSQL].[dbo].[tblSubCategoryPrinters]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblSuppliers"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + Code + char(34), char(34) + SupName + char(34), DeliveritSQL.dbo.fn_Esc(Address), char(34) + Phone + char(34), "
                    sScript &= "char(34) + Fax + char(34), char(34) + Email + char(34), char(34) + OrderType + char(34), char(34) + OrderFrq + char(34), DeliveritSQL.dbo.fn_Esc(Comments), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(Active,0)) + char(34) FROM [StockSQL].[dbo].[tblSuppliers]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblSpecialsSettings"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + PLU + char(34), char(34) + ButtonColour + char(34), char(34) + ButtonText + char(34), char(34) + FontColour + char(34), "
                    sScript &= "ISNULL(SortNumber,0) FROM [DeliveritSQL].[dbo].[tblSpecialsSettings]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblTable"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT char(34) + TableNoID + char(34), ISNULL(Status,0), ISNULL(CurrentSession,0), ISNULL(Guests,0), ISNULL(CustomerID,0), char(34) + MergeTableRec + char(34), "
                    sScript &= "char(34) + SplitTable + char(34), char(34) + Description + char(34), char(34) + SessionDate + char(34), (CASE WHEN CONVERT(varchar, ModifiedTime, 121) IS NULL THEN ''NULL'' ELSE CHAR(34) + CONVERT(varchar, ModifiedTime, 121) + CHAR(34) END), "
                    sScript &= "char(34) + TableSession + char(34) FROM [DeliveritSQL].[dbo].[tblTable]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblTableSettings"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT char(34) + btnID + char(34), ISNULL(TopPos,0), ISNULL(LeftPos,0), ISNULL(Width,100), ISNULL(Height,100), char(34) + CONVERT(varchar(10), ISNULL(Active,0)) + char(34), char(34) + MenuID + char(34), "
                    sScript &= "char(34) + ForeColor + char(34), char(34) + BackColor + char(34), char(34) + Type + char(34), char(34) + TableNoID + char(34), char(34) + Description + char(34) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblTableSettings]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblTendertypes"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT TenderTypeID, char(34) + TenderType + char(34), char(34) + EFTPOSAddress + char(34), char(34) + SurchargePLU + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(Hidden,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(Expense,0)) + char(34), char(34) + XeroAccountCode + char(34), char(34) + XeroAccountName + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(10), ISNULL(SaveAsUnpaid,0)) + char(34) FROM [DeliveritSQL].[dbo].[tblTenderTypes]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblTillCash"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + TillDate + char(34), ISNULL(FiveCents,0), ISNULL(TenCents,0), ISNULL(TwentyCents,0), ISNULL(FiftyCents,0), ISNULL(OneDollar,0), ISNULL(TwoDollars,0), ISNULL(FiveDollars,0), "
                    sScript &= "ISNULL(TenDollars,0), ISNULL(TwentyDollars,0), ISNULL(FiftyDollars,0), ISNULL(HundredDollars,0), char(34) + PaidComputer + char(34) FROM [DeliveritSQL].[dbo].[tblTillCash]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblTransactions"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + ItemCode + char(34), char(34) + TransDate + char(34), char(34) + TransType + char(34), "
                    sScript &= "char(34) + Supplier + char(34), char(34) + Unit + char(34), char(34) + CONVERT(varchar(20), ISNULL(Price,0.00)) + char(34), char(34) + CONVERT(varchar(20), ISNULL(Qty,0)) + char(34), "
                    sScript &= "char(34) + CONVERT(varchar(20), ISNULL(AdjQty,0)) + char(34), char(34) + CONVERT(varchar(10), ISNULL(Received,0)) + char(34) FROM [StockSQL].[dbo].[tblTransactions]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblTransactionStatusTypes"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT TransactionStatusID, char(34) + TransactionStatus + char(34) FROM [DeliveritSQL].[dbo].[tblTransactionStatusTypes]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblTransactionTypes"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT TransactionTypeID, char(34) + TransactionType + char(34) FROM [DeliveritSQL].[dbo].[tblTransactionTypes]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblUnsavedOrderDetails"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT UnsavedItemID, ISNULL(UnsavedOrderID,0), char(34) + PLU + char(34), DeliveritSQL.dbo.fn_Esc(Description), char(34) + CONVERT(varchar(20), ISNULL(Price,0)) + char(34), ISNULL(Qty,0), "
                    sScript &= "char(34) + DeleteDateTime + char(34), char(34) + Action + char(34) FROM [DeliveritSQL].[dbo].[tblUnsavedOrderDetails]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblUnsavedOrderHeaders"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT UnsavedOrderID, char(34) + OrderDate + char(34), char(34) + StaffID + char(34), ISNULL(TransactionType,0), char(34) + ComputerName + char(34), "
                    sScript &= "ISNULL(CustomerID,0), DeliveritSQL.dbo.fn_Esc(CustomerPhone), DeliveritSQL.dbo.fn_Esc(CustomerName), char(34) + StreetNumber + char(34), char(34) + StreetName + char(34), char(34) + Suburb + char(34), "
                    sScript &= "char(34) + UnitLevelNumber + char(34), char(34) + State + char(34), char(34) + PostCode + char(34), char(34) + GeoAddress + char(34), char(34) + GeoLat + char(34), char(34) + GeoLng + char(34) "
                    sScript &= "FROM [DeliveritSQL].[dbo].[tblUnsavedOrderHeaders]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblUpgradeLogs"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + UpgradeType + char(34), char(34) + OldVersion + char(34), char(34) + NewVersion + char(34), DeliveritSQL.dbo.fn_Esc(Message), "
                    sScript &= "char(34) + UpgradeDateTime + char(34), char(34) + CONVERT(varchar(10), ISNULL(Auto,0)) + char(34), char(34) + MACAddress + char(34), char(34) + MachineName + char(34), "
                    sScript &= "char(34) + IPAddress + char(34), char(34) + Source + char(34), char(34) + Files + char(34), char(34) + OSInfo + char(34) FROM [DeliveritSQL].[dbo].[tblUpgradeLogs]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
                Case "tblTableMenu"
                    sScript = "exec master..xp_cmdshell 'bcp ""SELECT RecID, char(34) + MenuID + char(34), ISNULL(MenuPosition,0) FROM [DeliveritSQL].[dbo].[tblTableMenu]"" queryout " & sDestination & sFileName & " -c -T -t"",""'"
            End Select
        Catch ex As Exception

        End Try

        Return sScript
    End Function

    Public Function ExportMSSERVERData(ByVal sTableName As String, ByVal sSchema As String) As String
        Dim sSQL As String = ""
        Dim sSQLGetExportCommand As String = ""
        Dim sSQLGetCreateTableCommand As String = ""
        Dim sSQLGetColumnHeaders As String = ""
        Dim sSQLGetImportCommand As String = ""
        Dim sErrorImportData As String = ""
        Dim sResult As String = "OK"
        'Dim iRowCount As Integer = 0

        sDestinationFolder = sDatabaseLocation & "\OUTFILES\"
        sDataFileName = "temp_" & sTableName & ".txt"

        Application.DoEvents()

        WritetoLog("Exporting " & sTableName & " Data from MSSERVER...", "")

        'checking directory if exist
        If Directory.Exists(sDestinationFolder) = False Then
            Directory.CreateDirectory(sDestinationFolder)
        End If

        CleanDatabase(sTableName, GetConnectionStringMS(sSchema))

        System.Threading.Thread.Sleep(2000)

        sSQL = GetExportScript(sTableName, sDestinationFolder, sDataFileName)

        'get the errors first
        sSQLGetExportCommand = sSQL
        sSQLGetColumnHeaders = GetAllColumnHeaders(sTableName, GetConnectionStringMS(sSchema))
        sSQLGetCreateTableCommand = "USE " & sSchema & ";" & vbCrLf & "DROP TABLE If EXISTS " & sTableName & ";" & vbCrLf & "CREATE TABLE " & sTableName & " " & GetAllColumnScriptInTableArraylist(sTableName, sSchema)

        System.Threading.Thread.Sleep(1000)

        If Not WriteColumnHeaders(sDestinationFolder & sTableName, sTableName, sSchema) Then
            sErrorImportData = "Writing Column Headers " & sTableName & " failed"
            LogDataImportDetails(sTableName, sErrorImportData, sSQLGetColumnHeaders, sSQLGetExportCommand, sSQLGetImportCommand)
            sResult = "ERROR"
            ReWritetoLog("Exporting " & sTableName & " Data from MSSERVER", "FAILED")
            Return sResult
        End If

        System.Threading.Thread.Sleep(2000)

        'setting the command timeout for ProcessMSSERVER
        Dim iTime As Integer
        iTime = 2147483

        If sTableName = "tblStaff" Then
            ' Change the column type of tblStaff.Comments and tblStaff.Comments1.
            Dim bReplicateSuccess As Boolean
            bReplicateSuccess = ReplicateTblStaff()
            If bReplicateSuccess = False Then
                sErrorImportData = "Error on Replicating tblStaff on MSSERVER"
                LogDataImportDetails(sTableName, sErrorImportData, sSQLGetColumnHeaders, sSQLGetExportCommand, sSQLGetImportCommand)
                sResult = "ERROR"
                ReWritetoLog("Exporting " & sTableName & " Data from MSSERVER", "FAILED")
                WritetoLog("Error on Replicating tblStaff on MSSERVER", "FAILED")
                Return sResult
            End If
        End If

        If sSQL <> "" Then
            Dim WriteProcessMSSERVER As String
            WriteProcessMSSERVER = ProcessMSSERVER(sSQL, sSchema, iTime)

            If WriteProcessMSSERVER <> "" Then
                sErrorImportData = WriteProcessMSSERVER
                LogDataImportDetails(sTableName, sErrorImportData, sSQLGetColumnHeaders, sSQLGetExportCommand, sSQLGetImportCommand)
                sResult = "ERROR"
                ReWritetoLog("Exporting " & sTableName & " Data from MSSERVER", "FAILED")
                WritetoLog(WriteProcessMSSERVER, "FAILED")
                Return sResult
            Else
                If File.Exists(sDestinationFolder & sDataFileName) = False Then
                    sErrorImportData = "Data File for " & sTableName & " does not exist. File Location: " & sDestinationFolder & sDataFileName
                    LogDataImportDetails(sTableName, sErrorImportData, sSQLGetColumnHeaders, sSQLGetExportCommand, sSQLGetImportCommand)
                    sResult = "ERROR"
                    WritetoLog("Data File for " & sTableName & " does not exist", "")
                    WritetoLog("File Location: " & sDestinationFolder & sDataFileName, "")
                    Return sResult
                End If

                ReWritetoLog("Exporting " & sTableName & " Data from MSSERVER", "SUCCESS")

                If File.Exists(sDestinationFolder & sTableName & ".txt") Then
                    If Not MergeTextFiles(sDestinationFolder & sTableName & ".txt", sDestinationFolder & sDataFileName) Then
                        sErrorImportData = "Merge File Failed" & sTableName
                        LogDataImportDetails(sTableName, sErrorImportData, sSQLGetColumnHeaders, sSQLGetExportCommand, sSQLGetImportCommand)
                        sResult = "ERROR"
                        WritetoLog("Exporting " & sTableName & " Data from MSSERVER", "FAILED")
                        Return sResult
                    End If

                    'Breaks down files into chunks
                    Dim bBreakFiles As Boolean = False
                    Dim arrFilesToImport As New ArrayList

                    arrFilesToImport.Clear()
                    bBreakFiles = BreakDownRestoreFilesMySQL(sDestinationFolder, sTableName, ".txt", sTableName, True, True, arrFilesToImport)

                    System.Threading.Thread.Sleep(2000)

                    If bBreakFiles Then
                        Dim bOk As Boolean = True
                        Dim bMySQLWarning As Boolean = False
                        Dim arrMySQLWarning As New ArrayList
                        Dim arrMySQLImportScript As New ArrayList

                        arrMySQLWarning.Clear()
                        arrMySQLImportScript.Clear()

                        For Each thisFile As String In arrFilesToImport
                            sSQLGetImportCommand = "SET autocommit=0; SET unique_checks=0; SET foreign_key_checks=0;" & vbCrLf & "LOAD DATA LOCAL INFILE '" & Replace(sDestinationFolder & thisFile, "\", "\\") & "' " & vbCrLf & "INTO TABLE " & sSchema.ToLower & "." & sTableName.ToLower & " " & vbCrLf & "FIELDS TERMINATED BY ',' ENCLOSED BY '""' LINES TERMINATED BY '\r\n' " & vbCrLf & "IGNORE 1 LINES(" & sSQLGetColumnHeaders & ");" & vbCrLf & "SET foreign_key_checks=1; SET unique_checks=1; SET autocommit=1; commit;"

                            'Import CSV to MySQL Server
                            If arrFilesToImport.Count <= 1 Then
                                If ImportCSVFile(sDestinationFolder & thisFile, sTableName, sSchema, iTime, sErrorImportData) = False Then
                                    If arrMySQLImportScript.Count = 0 Then
                                        arrMySQLImportScript.Add("DELETE from " & sSchema.ToLower & "." & sTableName.ToLower & "; " & vbCrLf)
                                    End If

                                    bOk = False
                                    arrMySQLWarning.Add(thisFile & ": " & sErrorImportData & vbCrLf)
                                    arrMySQLImportScript.Add(sSQLGetImportCommand)
                                End If
                            Else
                                If ImportCSVFile(sDestinationFolder & thisFile, sTableName, sSchema, iTime, thisFile, sErrorImportData) = False Then
                                    If arrMySQLImportScript.Count = 0 Then
                                        arrMySQLImportScript.Add("DELETE from " & sSchema.ToLower & "." & sTableName.ToLower & "; " & vbCrLf)
                                    End If

                                    bOk = False
                                    arrMySQLWarning.Add(thisFile & ": " & sErrorImportData & vbCrLf)
                                    arrMySQLImportScript.Add(sSQLGetImportCommand)
                                End If
                            End If

                            System.Threading.Thread.Sleep(2000)
                        Next

                        'compare both db
                        If bOk Then
                            Dim iMSSQLrows As String = "0"
                            Dim iMYSQLrows As String = "0"
                            If CompareRowCountSQLMySQL(sTableName, sSchema, iMSSQLrows, iMYSQLrows) = False Then
                                sErrorImportData = "Mismatched number of rows. MSSQL(" & iMSSQLrows & ") MYSQL(" & iMYSQLrows & ")"
                                LogDataImportDetails2(sTableName, arrMySQLWarning, sSQLGetColumnHeaders, sSQLGetExportCommand, arrMySQLImportScript)
                                WritetoLog("Mismatched number of rows", "")
                                sResult = "ERROR"
                                Return sResult
                            End If
                        Else
                            LogDataImportDetails2(sTableName, arrMySQLWarning, sSQLGetColumnHeaders, sSQLGetExportCommand, arrMySQLImportScript)
                            sResult = "ERROR"
                        End If
                    Else
                        sResult = "ERROR"
                    End If
                Else
                    sErrorImportData = "Column Header File for " & sTableName & " does not exist. File Location: " & sDestinationFolder & sTableName
                    LogDataImportDetails(sTableName, sErrorImportData, sSQLGetColumnHeaders, sSQLGetExportCommand, sSQLGetImportCommand)
                    WritetoLog("Column Header File for " & sTableName & " does not exist", "")
                    WritetoLog("File Location: " & sDestinationFolder & sTableName, "")
                    sResult = "ERROR"

                    Return sResult
                End If
            End If
        Else
            sErrorImportData = "Export Script for " & sTableName & " is not available for this application"
            LogDataImportDetails(sTableName, sErrorImportData, sSQLGetColumnHeaders, sSQLGetExportCommand, sSQLGetImportCommand)
            ReWritetoLog("Exporting " & sTableName & " Data from MSSERVER", "FAILED")
            WritetoLog("Export Script for " & sTableName & " is not available for this application", "")

            sResult = "ERROR"
            Return sResult
        End If

        Return sResult
    End Function
    Public Function ImportCSVFile(ByVal sFile1 As String, ByVal sTable As String, ByVal sSchemas As String, ByVal iTime As Integer, ByRef sErrorMsg As String) As Boolean
        Dim bOK As Boolean = True

        Try
            bOK = ImportCSVFile(sFile1, sTable, sSchemas, iTime, "", sErrorMsg)
        Catch ex As Exception
            bOK = False
        End Try

        Return bOK
    End Function
    Public Function ImportCSVFile(ByVal sFile1 As String, ByVal sTable As String, ByVal sSchemas As String, ByVal iTime As Integer, ByVal sFilename As String, ByRef sErrorMsg As String) As Boolean
        Dim bOK As Boolean = True
        Dim sSql As String
        Dim sInFile As String
        Dim sProcessSQL As String = ""
        Dim sProcessSQLWarnings As String = ""

        Application.DoEvents()

        sInFile = Replace(sFile1, "\", "\\")

        If sFilename = "" Then
            WritetoLog("Importing " & sTable & " Data to MySQL...", "")
        Else
            WritetoLog("Importing " & sTable & " Data to MySQL: " & sFilename & " ...", "")
        End If

        Try
            sSql = "SET autocommit=0; "
            sSql &= "SET unique_checks=0; "
            sSql &= "SET foreign_key_checks=0; "
            sSql &= "SET max_error_count=1; "
            sSql &= "LOAD DATA LOCAL INFILE '" & sInFile & "' "
            sSql &= "INTO TABLE " & sSchemas.ToLower & "." & sTable.ToLower & " "
            sSql &= "FIELDS TERMINATED BY ',' ENCLOSED BY '""' LINES TERMINATED BY '\r\n' "
            sSql &= "IGNORE 1 LINES(" & GetAllColumnHeaders(sTable, GetConnectionStringMS(sSchemas)) & "); "
            sSql &= "SHOW WARNINGS; "
            sSql &= "commit; "
            sSql &= "SET max_error_count=64; "
            sSql &= "SET foreign_key_checks=1; "
            sSql &= "SET unique_checks=1; "
            sSql &= "SET autocommit=1; "

            Dim thisTimeOut As Integer = 90
            If sTable = "tblOrderHeaders" OrElse sTable = "tblOrderDetails" OrElse sTable = "tblCustomers" OrElse sTable = "Audit" OrElse sTable = "tblPayments" Then
                thisTimeOut = iTime
            End If

            sProcessSQL = GetFieldMySQLReader1Item(sSql, GetConnectionStringMY(sSchemas), "Message")

            If sProcessSQL <> "" Then
                bOK = False
                sErrorMsg = sProcessSQL
                If sFilename = "" Then
                    ReWritetoLog("Importing " & sTable & " Data to MySQL", "FAILED")
                Else
                    ReWritetoLog("Importing " & sTable & " Data to MySQL: " & sFilename, "FAILED")
                End If
                WritetoLog(sProcessSQL, "")
            Else
                If sFilename = "" Then
                    ReWritetoLog("Importing " & sTable & " Data to MySQL", "SUCCESS")
                Else
                    ReWritetoLog("Importing " & sTable & " Data to MySQL: " & sFilename, "SUCCESS")
                End If
            End If

        Catch ex As Exception
            bOK = False
            If sFilename = "" Then
                ReWritetoLog("Importing " & sTable & " Data to MySQL", "FAILED")
            Else
                ReWritetoLog("Importing " & sTable & " Data to MySQL: " & sFilename, "FAILED")
            End If
            WritetoLog(ex.ToString, "")
        End Try

        Return bOK
    End Function

    Public Function VerifyEncryptedTextonCSV(ByVal sFile As String, ByVal sPassword As String) As Boolean
        Dim bReturn As Boolean = False
        Dim sDecryptedText As String = ""

        Try
            Dim thisline As String = File.ReadLines(sFile).First()

            sDecryptedText = DPosSecurity.modSecurity.DecryptString(thisline)

            Dim sTrimmed As String
            sTrimmed = sDecryptedText.Substring(8, Len(sDecryptedText) - 16)

            'password validation
            If sPassword = sTrimmed Then
                bReturn = True
            End If

        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Function CheckFileIfExistInZip(ByVal sFileName As String, ByVal sZipFile As String) As Boolean
        Dim bReturn As Boolean = False

        Dim zip As New Chilkat.Zip()
        zip.UnlockComponent(ChilkatZipLicenseKey)

        Dim success As Boolean
        success = zip.OpenZip(sZipFile)

        Try
            Dim zipEntry As Chilkat.ZipEntry = zip.FirstEntry

            While Not zipEntry Is Nothing
                If zipEntry.FileName.ToString = sFileName Then
                    bReturn = True
                    Exit While
                End If
                zipEntry = zipEntry.NextEntry
            End While
        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Function ImportCSVFileNoHeaders(ByVal sFile1 As String, ByVal sTable As String, ByVal sSchemas As String, ByVal iTime As Integer, ByRef sMyError As String) As Boolean
        Dim bOK As Boolean = True
        Dim sSql As String = ""
        Dim sInFile As String
        Dim sProcessSQL As String = ""

        Application.DoEvents()

        sInFile = Replace(sFile1, "\", "\\")

        Try
            If VerifyEncryptedTextonCSV(sFile1, sMySQLDPosPass) Then
                'sSql = "SET autocommit=0;"
                sSql = "SET unique_checks=0;"
                sSql &= "SET foreign_key_checks=0;"
                sSql &= "LOAD DATA LOCAL INFILE '" & sInFile & "' "
                sSql &= "INTO TABLE " & sSchemas.ToLower & "." & sTable.ToLower & " "
                sSql &= "FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '""' LINES TERMINATED BY '\r\n' "
                sSql &= "IGNORE 1 lines;"
                sSql &= "SET foreign_key_checks=1;"
                sSql &= "SET unique_checks=1;"
                'sSql &= "SET autocommit=1;"
                'sSql &= "commit;"

                Dim myFileInfo As New FileInfo(sFile1)
                Dim myFileSize As Long = myFileInfo.Length
                Dim thisTimeOut As Integer = 100000
                If myCInt(myFileSize) > 10000000 Then
                    thisTimeOut = myCInt(myFileSize) / 10000000
                    thisTimeOut *= 100000
                Else
                    thisTimeOut = 100000
                End If

                sProcessSQL = ProcessMySQL(sSql, GetConnectionStringMY(sSchemas), thisTimeOut)

                If sProcessSQL <> "" Then
                    sMyError = sProcessSQL
                    LogDataImportDetails2(sSchemas, sTable, sSql, sMyError)
                    bOK = False
                End If
            Else
                sMyError = "The data file did not passed the validation"
                LogDataImportDetails2(sSchemas, sTable, sSql, sMyError)
                bOK = False
            End If

        Catch ex As Exception
            bOK = False
        End Try

        Return bOK
    End Function

    Public Function CompareRowCountSQLMySQL(ByVal sTable As String, ByVal sDatabase As String, ByRef iRowsMSSQL As String, ByRef iRowsMYSQL As String) As Boolean
        Dim bResult As Boolean = False

        WritetoLog("Row Count Compare - " & sTable & "", "")

        If sTable = "Audit" Then
            iRowsMSSQL = GetFieldMSSQL("SELECT COUNT(*) as MSRowCount from " & sTable & " where Synced!=1", GetConnectionStringMS(sDatabase), "MSRowCount")
            iRowsMYSQL = GetFieldMySQL("SELECT COUNT(*) as MYRowCount from " & sTable & " where Synced!=1", GetConnectionStringMY(sDatabase), "MYRowCount")
        Else
            iRowsMSSQL = GetFieldMSSQL("SELECT COUNT(*) as MSRowCount from " & sTable, GetConnectionStringMS(sDatabase), "MSRowCount")
            iRowsMYSQL = GetFieldMySQL("SELECT COUNT(*) as MYRowCount from " & sTable, GetConnectionStringMY(sDatabase), "MYRowCount")
        End If


        If iRowsMSSQL <> "ERROR" And iRowsMYSQL <> "ERROR" Then
            If iRowsMSSQL = iRowsMYSQL Then
                WritetoLog("MSSQL(" & iRowsMSSQL & ") MYSQL(" & iRowsMYSQL & ") ", "OK")
                bResult = True
            Else
                WritetoLog("MSSQL(" & iRowsMSSQL & ") MYSQL(" & iRowsMYSQL & ") ", "MISMATCH")
            End If
        Else
            WritetoLog("MSSQL(" & iRowsMSSQL & ") MYSQL(" & iRowsMYSQL & ") ", "MISMATCH")
        End If

        Return bResult
    End Function

    Public Function GetProgramLocation(ByVal arg As Integer) As String
        Dim sThisPath As String = ""
        Dim arguments As [String]() = Environment.GetCommandLineArgs()

        If bDemo = True Then
            sThisPath = "C:\VBNET\DPosMySQLInstall\DPosMYSQLInstall\Installers"
        Else
            'do not do anything for now

            sThisPath = Application.StartupPath & "\Installers"
        End If

        Return sThisPath
    End Function

    Public Function GetDatabaseLocation(ByVal arg As Integer) As String

        Dim sDBPath As String = ""
        Dim arguments As [String]() = Environment.GetCommandLineArgs()


        ' Changed from arguments.Length = 1 to bDemo = true
        If bDemo = True Then
            sDBPath = "C:\VBNet\DPosMySQLInstall\DPosMYSQLInstall\Export"
        Else
            'do not do anything for now
            Try
                sDBPath = Application.StartupPath & "\Export"

            Catch ex As Exception
                myMsgBox("GetDatabaseLocation: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End Try

        End If

        Return sDBPath
    End Function

    ''' <summary>
    ''' Breaks down Restore Data files. Stores a list of files 
    ''' </summary>
    ''' <param name="sFileLocation">Directory where the Restore Data files belong to</param>
    ''' <param name="sFileName">Self-explanatory</param>
    ''' <param name="sFileExt">File Extension no need to add period</param>
    ''' <param name="sTableName">Database Table of the Data file. This will determine if we need to adjust the maxrow.</param>
    ''' <param name="bReplace">True - Max lines will be 25000 by default, False - Max lines will be 20000 by default.</param>
    ''' <param name="arRestoreFiles">List of files</param>
    ''' <returns>Return if breaking down in to multiple files was successful for validation purposes.</returns>
    Public Function BreakDownRestoreFilesMySQL(ByVal sFileLocation As String, ByVal sFileName As String, ByVal sFileExt As String, ByVal sTableName As String, ByVal bCopyHeader As Boolean, ByVal bReplace As Boolean, ByRef arRestoreFiles As ArrayList) As Boolean
        Dim bReturn As Boolean = True
        Dim iMaxRow As Integer = 5000
        Dim sHeader As String = ""
        Dim iLineCount As Integer = 0

        Try
            Application.DoEvents()
            'Get the number of rows
            iLineCount = File.ReadAllLines(sFileLocation & sFileName & sFileExt).Count()

            If bCopyHeader Then
                sHeader = File.ReadLines(sFileLocation & sFileName & sFileExt).First()
            End If

            If bReplace Then
                iMaxRow = 5000
            End If

            'Decrease bulk size for these tables.
            If sTableName = "tblcustomers" OrElse sTableName = "tblorderdetails" Then
                If bReplace Then
                    iMaxRow = 3000
                Else
                    iMaxRow = 2500
                End If
            ElseIf sTableName = "tblorderheaders" Then
                iMaxRow = 2500
            End If

            'Divides file by maxrow
            If iLineCount > iMaxRow Then
                Using sr As StreamReader = New StreamReader(sFileLocation & sFileName & sFileExt)
                    Dim iNumberTag As Integer = 1
                    Dim newFileName As String = ""

                    While Not sr.EndOfStream
                        Dim i As Integer = 0
                        newFileName = sFileName & iNumberTag.ToString & sFileExt

                        If File.Exists(sFileLocation & newFileName) Then
                            File.Delete(sFileLocation & newFileName)
                        End If

                        Using sw As StreamWriter = New StreamWriter(sFileLocation & newFileName)

                            If bCopyHeader AndAlso iNumberTag > 1 Then
                                'The first file will have the headers for this one
                                sw.WriteLine(sHeader)
                            End If

                            While Not sr.EndOfStream AndAlso i <= iMaxRow
                                sw.WriteLine(sr.ReadLine)
                                i += 1
                            End While

                            sw.Flush()
                        End Using

                        arRestoreFiles.Add(newFileName)
                        iNumberTag += 1
                    End While
                End Using
            Else
                'Original untouched file will be added to the array for import. if not divided
                arRestoreFiles.Add(sFileName & sFileExt)
            End If
        Catch ex As Exception
            arRestoreFiles.Clear()
            bReturn = False
        End Try

        Return bReturn
    End Function

    Public Function GetDatabaseLocationDPOS(ByVal arg As Integer) As String

        Dim sDBPath As String = ""
        Dim arguments As [String]() = Environment.GetCommandLineArgs()

        ' Changed from arguments.Length = 1 to bDemo = true
        If bDemo = True Then
            sDBPath = "C:\VBNet\DPOS\Database"
        Else
            Try
                ' The client app is called by DPos.
                If arguments.Length > 1 Then
                    ' Use the second argument in arguments().
                    sDBPath = arguments(1)
                    'MsgBox("DPos called the app: " & sDBPath)
                Else
                    ' The app is either opened via double click.

                    Dim mngmtClass As New ManagementClass("Win32_Process")
                    Dim bFound As Boolean = False

                    ' Find the DPos process from the list of running processes.
                    For Each item As ManagementObject In mngmtClass.GetInstances()

                        If item("Name").Equals("DPos.exe") Then

                            Try
                                Dim commandLine As [String] = DirectCast(item("CommandLine"), [String])     ' Get the command line arguments.

                                ' UAC problems, please take note.
                                If commandLine <> "" Then
                                    Dim iLastIndexOfQuotation As Integer = commandLine.LastIndexOf("""")        ' Get the index of the last quotation mark.
                                    Dim appArguments As String = commandLine
                                    appArguments = appArguments.Insert(iLastIndexOfQuotation + 1, ",")          ' Insert a comma so we can split the argument into an array.

                                    Dim sDPosArguments As String() = appArguments.Split(CHAR_COMMA)             ' Split the argument to an array.

                                    sDBPath = sDPosArguments(arg).Trim                                          ' Trim any whitespace.

                                    bFound = True
                                Else
                                    bFound = False
                                End If

                            Catch ex As Exception
                                bFound = False
                            End Try

                            'MsgBox("DPos running: " & sDBPath)
                            Exit For
                        End If
                    Next

                    ' The DPos app is not running, use a default location.
                    If bFound = False Then
                        sDBPath = "C:\DPos"
                        'MsgBox("DPos not running: " & sDBPath)
                    End If

                End If

                'sDBPath = arguments(arg)
                'sDBPath = "C:\DPos"

            Catch ex As Exception
                MsgBox("GetDatabaseLocation" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            End Try

        End If

        Return sDBPath

    End Function

    Public Function myMsgBox(ByVal sMessage As String, ByVal sTitle As String, ByVal iType As Integer) As DialogResult
        Dim drReturn As DialogResult = DialogResult.Cancel

        Dim sCustomYes As String = "YES"
        Dim sCustomNo As String = "NO"

        drReturn = myMsgBox(sMessage, sTitle, iType, sCustomYes, sCustomNo)

        Return drReturn
    End Function

    ''' <summary>
    ''' Show customed made msgbox
    ''' </summary>
    ''' <param name="sMessage">goes to Messagebox</param>
    ''' <param name="sTitle">goes to Title Bar</param>
    ''' <param name="iType">Message box controls 1 - OK only, 2 - Yes or No, 3 - CustomYESNO</param>
    Public Function myMsgBox(ByVal sMessage As String, ByVal sTitle As String, ByVal iType As Integer, ByVal CustomYes As String, ByVal CustomNo As String) As DialogResult
        Dim myMessageBox As New frmMessageBox
        Dim iTimes As Integer = 1
        Dim modiMessageTxtH As Integer = 0
        Dim iModal As Integer = 0
        Dim drReturn As DialogResult = DialogResult.Cancel

        With myMessageBox
            .lblTitleBar.Text = sTitle
            .lblMessage.Text = sMessage
            .msgboxType = iType

            sMessage = Replace(sMessage, vbCrLf, CHAR_PARA)

            'does not return 0 if divisible by 41
            modiMessageTxtH = Len(sMessage) Mod 41

            'stores how many times should the lblmessage get bigger
            'characters per line = 41
            iTimes = Len(sMessage) \ 41 'use backslash if you want to get the product without the remainder

            'if the length of the sMessage is not divisible by 41, add times
            If modiMessageTxtH <> 0 Then
                iTimes += 1
            End If

            'linebreaks
            Dim ind As Integer
            Dim sChar As String

            For ind = 0 To Len(sMessage) - 1
                sChar = sMessage.Substring(ind, 1)

                If sChar = CHAR_PARA Then
                    iTimes += 1
                End If
            Next

            'adjusts the lblmessage.height
            'new lblmessage height = height per line * iTimes
            .lblMessage.Height = 22 * iTimes

            'repositioning messagebox controls and size adjustment
            .Height = .lblMessage.Height + .btnOK.Height + .pnlTitleBar.Height + 19
            .btnOK.Location = New Point(112, .Height - 30)
            .btnYN_Yes.Location = New Point(61, .Height - 30)
            .btnYN_No.Location = New Point(160, .Height - 30)
            .btnYN_Yes.Text = CustomYes
            .btnYN_No.Text = CustomNo

            .Top = iLogonFrmPosY - .Height / 2
            .Left = iLogonFrmPosX - .Width / 2

            .ShowDialog()

            drReturn = .DialogResult
        End With

        Return drReturn
    End Function

    Public Sub ExitProgram()

        GC.Collect()

        End

        If (Application.MessageLoop) Then
            ' Use this since we are a WinForms app.
            Application.Exit()
        Else
            ' Use this since we are a console app.
            Environment.Exit(1)
        End If

    End Sub

    Public Function myCStr(ByVal i As Object) As String
        Dim sOut As String
        Try
            If i Is Nothing OrElse IsDBNull(i) Then
                Return ""
            End If
            sOut = CStr(i)
            If sOut Is Nothing OrElse IsDBNull(sOut) Then
                sOut = ""
            End If
        Catch
            sOut = ""
        End Try
        Return sOut
    End Function

    Public Function myCInt(ByVal o As Object) As Integer
        Dim iOut As Integer
        If IsDBNull(o) Then
            Return 0
        End If
        If o Is Nothing Then
            Return 0
        End If
        If o.ToString = "" Then
            Return 0
        End If
        Try
            iOut = CInt(o)
        Catch
            iOut = 0
        End Try
        Return iOut
    End Function

    Public Function myCBool(ByVal b As Object) As Boolean
        Dim bVal As Boolean = False

        If b Is Nothing Then
            Return bVal
        End If

        If myCStr(b).Trim = "" Then
            Return bVal
        End If

        Try
            bVal = CBool(b)
        Catch ex As Exception
        End Try

        Return bVal

    End Function

    Public Function IsMySQLServerInstalled() As Integer
        Dim HKCUPath As RegistryKey = My.Computer.Registry.CurrentUser
        Dim iReturn As Integer = 0

        Try
            Dim MySQLRegKey As RegistryKey = HKCUPath.OpenSubKey("Software\MySQL AB\MySQL Server 5.7")              'HKEY_CURRENT_USER path of MySQL Server

            iReturn = CInt(MySQLRegKey.GetValue("installed", 0))                                                    '1/0 - DWORD that determines if mySQL Server was installed. 1 - installed 0 - not installed

        Catch ex As Exception
            'do not do anything
        End Try

        Return iReturn
    End Function

    Public Function IsSQLServerInstalled() As Integer
        Dim HKCUPath As RegistryKey = My.Computer.Registry.CurrentUser
        Dim iReturn As Integer = 0

        Try
            Dim MySQLRegKey As RegistryKey = HKCUPath.OpenSubKey("Software\MySQL AB\MySQL Server 5.7")              'HKEY_CURRENT_USER path of MySQL Server

            iReturn = CInt(MySQLRegKey.GetValue("installed", 0))                                                    '1/0 - DWORD that determines if mySQL Server was installed. 1 - installed 0 - not installed

        Catch ex As Exception
            'do not do anything
        End Try

        Return iReturn
    End Function

    Public Function isDPosInstalled() As Boolean
        Dim HKLMPath As RegistryKey = My.Computer.Registry.LocalMachine

        Try
            Dim MySQLRegKey As RegistryKey = HKLMPath.OpenSubKey("SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall")

            Dim iSubkeyCount As Integer = MySQLRegKey.SubKeyCount
            Dim cnt As Integer

            For cnt = 1 To iSubkeyCount
                Dim thisSubkey As RegistryKey = MySQLRegKey.OpenSubKey(MySQLRegKey.GetSubKeyNames(cnt))

                Dim thisDisplayName As String = thisSubkey.GetValue("DisplayName")
                If thisDisplayName = "DPos" Then
                    Return True
                End If
            Next

            MySQLRegKey = HKLMPath.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")
            iSubkeyCount = MySQLRegKey.SubKeyCount
            cnt = 1

            For cnt = 1 To iSubkeyCount
                Dim thisSubkey As RegistryKey = MySQLRegKey.OpenSubKey(MySQLRegKey.GetSubKeyNames(cnt))

                Dim thisDisplayName As String = thisSubkey.GetValue("DisplayName")
                If thisDisplayName = "DPos" Then
                    Return True
                End If
            Next


        Catch ex As Exception
            'do not do anything
        End Try

        Return False
    End Function

    Public Function isThisSoftwareInstalled(ByVal sDisplayName As String, ByVal bExactProgramName As Boolean) As Boolean
        Dim b32 As Boolean = False
        Dim b64 As Boolean = False

        b32 = isThisSoftwareInstalled(sDisplayName, "SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", RegistryView.Default, bExactProgramName)
        b64 = isThisSoftwareInstalled(sDisplayName, "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", RegistryView.Registry64, bExactProgramName)

        If b32 Or b64 Then
            Return True
        End If

        Return False
    End Function

    Public Function isThisSoftwareInstalled(ByVal sDisplayName As String, ByVal RegisKey As String, ByRef RegisView As RegistryView, ByVal bExactProgramName As Boolean) As Boolean
        'Dim HKLMPath As RegistryKey

        Try
            Using HKLMPath As RegistryKey = Microsoft.Win32.RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegisView)
                Using key As RegistryKey = Registry.LocalMachine.OpenSubKey(RegisKey)
                    For Each subkey_name In key.GetSubKeyNames
                        Using subkey As RegistryKey = key.OpenSubKey(subkey_name)
                            Dim thisDisplayName As String = subkey.GetValue("DisplayName")
                            If bExactProgramName Then
                                If thisDisplayName = sDisplayName Then
                                    Return True
                                End If
                            Else
                                Dim bMatched As Boolean = thisDisplayName Like sDisplayName & "*"

                                If bMatched Then
                                    Return True
                                End If
                            End If
                        End Using
                    Next
                End Using
            End Using
        Catch ex As Exception
            'do not do anything
        End Try

        Return False
    End Function

    Public Function CheckDatabaseIfExistMS(ByVal sSchema As String) As Boolean
        Dim bReturn As Boolean = False
        Dim sSQL As String

        sSQL = "SELECT Count(NAME) as RecCount from sys.databases WHERE NAME!='master' and NAME!='tempdb' and NAME!='model' and NAME!='msdb' and NAME='" & sSchema & "'"

        If GetFieldMSSQL(sSQL, GetConnectionStringMS("master"), "RecCount") = "1" Then
            bReturn = True
        End If

        Return bReturn
    End Function

    Public Function CheckIfDBTableExistMS(ByVal sTable As String, ByVal sDatabase As String) As Boolean
        Dim sSQL As String
        Dim bExists As Boolean = False
        Dim sConn As String = ""

        Try
            If sDatabase = "" Then
                sDatabase = "DeliveritSQL"
                sConn = GetConnectionStringMS(sDatabase)
            Else
                sConn = GetConnectionStringMS(sDatabase)
            End If

            sSQL = "SELECT COUNT(*) as ColCount FROM sys.objects " &
                    "WHERE object_id = OBJECT_ID(N'[dbo].[" & sTable & "]') " &
                    "AND type in (N'U')"

            bExists = myCBool(GetFieldMSSQL(sSQL, GetConnectionStringMS(sDatabase), "ColCount"))

        Catch ex As Exception

        End Try

        Return bExists

    End Function

    Public Function CheckIfDBTableExistMY(ByVal sTable As String, ByVal sDatabase As String) As Boolean
        Dim sSQL As String
        Dim bExists As Boolean = False
        Dim sConn As String = ""

        Try
            If sDatabase = "" Then
                sDatabase = "DeliveritSQL"
                sConn = GetConnectionStringMY(sDatabase)
            Else
                sConn = GetConnectionStringMY(sDatabase)
            End If

            sSQL = "SELECT COUNT(*) as result " &
                    "FROM information_schema.tables " &
                    "WHERE table_schema = '" & sDatabase & "' " &
                    "AND table_name = '" & sTable & "' " &
                    "LIMIT 1"

            bExists = myCBool(GetFieldMSSQL(sSQL, GetConnectionStringMY(sDatabase), "ColCount"))

        Catch ex As Exception

        End Try

        Return bExists

    End Function

    Public Function CheckIfFKExistandGetConstraintMS(ByVal sDatabaseName As String, ByVal sTableName As String, ByVal sColumnName As String, ByVal bNoLock As Boolean, ByRef ConstraintName As String, ByRef sError As String) As Boolean
        Dim bReturn As Boolean = False
        Dim MSCON As New SqlClient.SqlConnection
        Dim MSCMD As New SqlClient.SqlCommand
        Dim MSDR As SqlClient.SqlDataReader

        Dim sSQL As String

        Try
            sSQL = "SELECT INFORMATION_SCHEMA.KEY_COLUMN_USAGE.CONSTRAINT_NAME as thisConstraintName FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS "
            If bNoLock Then
                sSQL &= "with (NOLOCK) "
            End If
            sSQL &= "LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ON INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_NAME = INFORMATION_SCHEMA.KEY_COLUMN_USAGE.CONSTRAINT_NAME "
            sSQL &= "WHERE INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_CATALOG='" & sDatabaseName & "' AND "
            sSQL &= "INFORMATION_SCHEMA.KEY_COLUMN_USAGE.TABLE_NAME='" & sTableName & "' AND "
            sSQL &= "INFORMATION_SCHEMA.KEY_COLUMN_USAGE.COLUMN_NAME='" & sColumnName & "' AND "
            sSQL &= "INFORMATION_SCHEMA.TABLE_CONSTRAINTS.CONSTRAINT_TYPE = 'FOREIGN KEY'"

            MSCON.ConnectionString = GetConnectionStringMS(sDatabaseName)
            MSDBOpen(MSCON)
            MSCMD.Connection = MSCON
            MSCMD.CommandText = sSQL

            If bNoLock Then
                MSDR = MyExecuteReaderMS(MSCMD, CommandBehavior.SingleRow)
            Else
                MSDR = MyExecuteReaderMS(MSCMD)
            End If

            If IsNothing(MSDR) = False Then
                If MSDR.HasRows Then
                    MSDR.Read()
                    Try
                        ConstraintName = myDRStringMS(MSDR, "thisConstraintName")
                        bReturn = True
                    Catch ex As Exception
                        sError = "CheckIfFKExistandGetConstraintMS: Failed to load constraint name"
                        bReturn = False
                    End Try
                End If

                MSDR.Close()
            Else
                sError = "CheckIfFKExistandGetConstraintMS: Failed to check if foreign key is existing"
            End If

            MSCMD.Dispose()
        Catch ex As Exception
            'ShowError(ex)
            sError = "CheckIfFKExistandGetConstraintMS: " & ex.ToString
        Finally
            MSCON.Close()
        End Try

        Return bReturn
    End Function

    Public Function isDPosUpdated() As Boolean
        Dim HKLMPath As RegistryKey = My.Computer.Registry.LocalMachine

        Try
            Dim MySQLRegKey As RegistryKey = HKLMPath.OpenSubKey("SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall")

            Dim iSubkeyCount As Integer = MySQLRegKey.SubKeyCount
            Dim cnt As Integer

            For cnt = 1 To iSubkeyCount
                Dim thisSubkey As RegistryKey = MySQLRegKey.OpenSubKey(MySQLRegKey.GetSubKeyNames(cnt))

                Dim thisDisplayName As String = thisSubkey.GetValue("DisplayName")
                Dim thisDisplayVersion As String = Replace(thisSubkey.GetValue("DisplayVersion"), ".", "")
                If thisDisplayName = "DPos" Then
                    If CInt(thisDisplayVersion) >= 21800 Then
                        Return True
                    End If
                End If
            Next

            MySQLRegKey = HKLMPath.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")
            iSubkeyCount = MySQLRegKey.SubKeyCount
            cnt = 1

            For cnt = 1 To iSubkeyCount
                Dim thisSubkey As RegistryKey = MySQLRegKey.OpenSubKey(MySQLRegKey.GetSubKeyNames(cnt))

                Dim thisDisplayName As String = thisSubkey.GetValue("DisplayName")
                Dim thisDisplayVersion As String = Replace(thisSubkey.GetValue("DisplayVersion"), ".", "")
                If thisDisplayName = "DPos" Then
                    If CInt(thisDisplayVersion) >= 21800 Then
                        Return True
                    End If
                End If
            Next

        Catch ex As Exception
            'do not do anything
        End Try

        Return False
    End Function

    Public Sub DeleteAppShortcut()
        If IsMySQLServerInstalled() = 1 AndAlso (isThisSoftwareInstalled("DPos", True) = True And isDPosUpdated() = True) Then
            DeleteFile(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory) & "\DPos MySQL Install.lnk")
            DeleteFile(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) & "\DPos MySQL Install.lnk")
            DeleteFile(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\DPos MySQL Install.lnk")
        End If
    End Sub

    Public Function GetDesktopLocation() As String
        Dim sLocation As String = ""
        Dim HKCUPath As RegistryKey = My.Computer.Registry.CurrentUser

        Try
            Dim DesktopLocationRegKey As RegistryKey = HKCUPath.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders")
            sLocation = DesktopLocationRegKey.GetValue("Desktop")

        Catch ex As Exception
            'do not do anything
        End Try

        Return sLocation
    End Function

    ''' <summary>
    ''' Replicates the tblStaff to fix the issues with the triggers wherein some columns
    ''' are not recorded by the triggers.
    ''' </summary>
    Public Function ReplicateTblStaff() As Boolean
        Dim bReturn As Boolean = False
        Dim sSQL As String = ""
        Dim sProcess As String = ""
        Dim bProcess As Boolean

        Try
            ' Change the column type of tblStaff.Comments and tblStaff.Comments1.
            bProcess = ChangeColumnType()
            If bProcess = False Then
                Return bReturn
            End If

            ' Insert the current rows to a new table with the current table structure.
            sSQL = "SELECT * INTO [DeliveritSQL].[dbo].tblStaff2 FROM [DeliveritSQL].[dbo].[tblStaff]"
            sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS("DeliveritSQL"))
            If sProcess <> "" Then
                Return bReturn
            End If

            ' Check if the new table exists.
            If CheckIfDBTableExist("tblStaff2", "DeliveritSQL", DatabaseType.MSSERVER) = True Then
                ' Drop the old table.
                sSQL = "DROP TABLE tblStaff"
                sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS("DeliveritSQL"))
                If sProcess <> "" Then
                    Return bReturn
                End If

                ' Rename the new table to tblStaff.
                sSQL = "sp_rename 'tblStaff2', 'tblStaff';"
                sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS("DeliveritSQL"))
                If sProcess <> "" Then
                    Return bReturn
                End If

                ' Add a primary key to the new table.
                sSQL = "ALTER TABLE tblStaff ADD CONSTRAINT tblStaff_PK PRIMARY KEY CLUSTERED (StaffID);"
                sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS("DeliveritSQL"))
                If sProcess <> "" Then
                    Return bReturn
                End If
            End If

            bReturn = True
        Catch ex As Exception
            myMsgBox("ReplicateTblStaff" & vbCrLf & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bReturn
    End Function

    ''' <summary>
    ''' This function changes the column type of Comments and Comments1 in tblStaff from ntext to nvarchar.
    ''' SQL triggers cannot use ntext columns, so we need to change the column type to nvarchar.
    ''' </summary>
    Public Function ChangeColumnType() As Boolean
        Dim bReturn As Boolean = False
        ' This function changes the column type of Comments and Comments1 in tblStaff from ntext to nvarchar.
        ' SQL triggers cannot use ntext columns, so we need to change the column type to nvarchar.

        Dim sSQL As String = ""
        Dim sProcess As String
        Try
            sSQL = "alter table tblStaff alter column Comments nvarchar(255) null"
            sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS("DeliveritSQL"))
            If sProcess <> "" Then
                Return bReturn
            End If

            sSQL = "alter table tblStaff alter column Comments1 nvarchar(255) null"
            sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS("DeliveritSQL"))
            If sProcess <> "" Then
                Return bReturn
            End If

            bReturn = True
        Catch ex As Exception
            myMsgBox("ChangeColumnType" & vbCrLf & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bReturn
    End Function

    Public Function RestoreDatabase(ByVal sType As String, ByVal sPath As String) As String
        Dim sReturn As String = ""

        Application.DoEvents()

        Try
            'calculate timeout depending on file size
            Dim iTime As Integer
            Dim myFileInfo As New FileInfo(sPath & sType & ".BAK")
            Dim myFileSize As Long = myFileInfo.Length

            If myCInt(myFileSize) > 10000000 Then
                iTime = myCInt(myFileSize) / 10000000
                iTime *= 100000
            Else
                iTime = 100000
            End If


            Dim constring As String = GetConnectionStringMY(sType.ToLower)
            ' Important Additional Connection Options
            constring += ";convertzerodatetime=true;"

            Dim MYCON As New MySqlConnection
            Dim MYCMD As New MySqlCommand
            Dim MYBAK As New MySqlBackup

            MYCON.ConnectionString = constring
            MYDBOpen(MYCON)

            MYCMD.Connection = MYCON
            MYCMD.CommandTimeout = iTime
            MYBAK.Command = MYCMD

            MYBAK.ImportFromFile(sPath & sType & ".BAK")

            MYCMD.Dispose()
            MYCON.Close()

            sReturn = ""

        Catch ex As Exception
            sReturn = ex.Message
        End Try

        Return sReturn
    End Function

    Public Function CreateSchemas(ByVal sSchema As String, ByVal bWriteLog As Boolean) As String
        Dim MYCN As New MySqlConnection
        Dim MYCM As New MySqlCommand
        Dim sSQL As String
        Dim sProcessSQL As String = ""
        Dim sReturn As String = ""

        Application.DoEvents()

        If bWriteLog = True Then
            WritetoLog("Creating Database: " & sSchema & " ...", "")
        End If

        Try
            sSQL = "DROP database if exists " & sSchema
            ProcessMySQL(sSQL, GetConnectionStringMY(""))
            If sProcessSQL <> "" Then
                If bWriteLog = True Then
                    ReWritetoLog("Creating Database: " & sSchema, "FAILED")
                    WritetoLog(sProcessSQL, "")
                Else
                    myMsgBox("CreateSchemas: " & sProcessSQL, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                End If

                Return sProcessSQL
            End If

            sSQL = "CREATE DATABASE IF NOT EXISTS " & sSchema
            sProcessSQL = ProcessMySQL(sSQL, GetConnectionStringMY(""))

            If sProcessSQL <> "" Then
                If bWriteLog = True Then
                    ReWritetoLog("Creating Database: " & sSchema, "FAILED")
                    WritetoLog(sProcessSQL, "")
                Else
                    myMsgBox("CreateSchemas: " & sProcessSQL, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                End If

                sReturn = sProcessSQL
            Else
                If bWriteLog = True Then
                    ReWritetoLog("Creating Database: " & sSchema, "SUCCESS")
                End If
            End If
        Catch ex As Exception
            If bWriteLog = True Then
                ReWritetoLog("Creating Database: " & sSchema, "FAILED")
                WritetoLog(ex.ToString, "")
            Else
                myMsgBox("CreateSchemas: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            sReturn = "ERROR"
        End Try

        Return sReturn
    End Function

    Public Function CreateTable(ByVal sTable As String, ByVal sRows As String, ByVal sSchema As String, ByVal bWriteLog As Boolean) As String
        Dim MYCN As New MySqlConnection
        Dim MYCM As New MySqlCommand
        Dim sSQL As String
        Dim sProcessSQL As String = ""
        Dim bExecuted As Boolean = False
        Dim sReturn As String = ""

        Application.DoEvents()

        If bWriteLog = True Then
            WritetoLog("Creating Table: " & sTable & " ...", "")
        End If

        Try
            sSQL = "USE " & sSchema & ";"
            sSQL &= "DROP TABLE IF EXISTS " & sTable & ";"
            sSQL &= "CREATE TABLE " & sTable & " " & sRows

            sProcessSQL = ProcessMySQL(sSQL, GetConnectionStringMY(sSchema))

            If sProcessSQL <> "" Then
                If bWriteLog = True Then
                    ReWritetoLog("Creating Table: " & sTable, "FAILED")
                    WritetoLog(sProcessSQL, "")
                Else
                    myMsgBox("CreateTable: " & sProcessSQL, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                End If

                sReturn = sProcessSQL
            Else
                If bWriteLog = True Then
                    ReWritetoLog("Creating Table: " & sTable, "SUCCESS")
                End If
            End If

        Catch ex As Exception
            If bWriteLog = True Then
                ReWritetoLog("Creating Table: " & sTable, "FAILED")
                WritetoLog(ex.ToString, "")
            Else
                myMsgBox("CreateTable: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            sReturn = "ERROR"
        End Try

        Return sReturn
    End Function

    Public Function CreateMYSQLIndex(ByVal sSchema As String, ByVal sTable As String, ByVal sColumn As String, ByVal bWriteLog As Boolean) As String
        Dim MYCN As New MySqlConnection
        Dim MYCM As New MySqlCommand
        Dim sSQL As String
        Dim sProcessSQL As String = ""
        Dim bExecuted As Boolean = False
        Dim sReturn As String = ""

        Application.DoEvents()

        If bWriteLog = True Then
            WritetoLog("Creating Index " & sTable & "(" & sColumn & ") ...", "")
        End If

        Try
            'scans if index exist
            sSQL = "USE sys; "
            sSQL &= "SELECT IF(EXISTS(SELECT 1 from information_schema.STATISTICS where INDEX_NAME = '" & sTable & "_" & sColumn & "_IDX' and TABLE_NAME='" & sTable & "' and INDEX_SCHEMA='" & sSchema & "'),1,0) as result;"

            If GetFieldMySQL(sSQL, GetConnectionStringMY(""), "result") = "1" Then
                'if index exist drop index
                sSQL = "ALTER TABLE `" & sSchema & "`.`" & sTable & "` "
                sSQL &= "DROP INDEX " & sTable & "_" & sColumn & "_IDX;"
                sProcessSQL = ProcessMySQL(sSQL, GetConnectionStringMY(""))
            End If

            sSQL = "USE " & sSchema & "; "
            sSQL &= "CREATE INDEX " & sTable & "_" & sColumn & "_IDX ON " & sTable & "(" & sColumn & ")"

            sProcessSQL = ProcessMySQL(sSQL, GetConnectionStringMY(sSchema))

            If sProcessSQL <> "" Then
                If bWriteLog = True Then
                    ReWritetoLog("Creating Index " & sTable & "(" & sColumn & ")", "FAILED")
                    WritetoLog(sProcessSQL, "")
                Else
                    myMsgBox("CreateTable: " & sProcessSQL, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                End If

                sReturn = sProcessSQL
            Else
                If bWriteLog = True Then
                    ReWritetoLog("Creating Index " & sTable & "(" & sColumn & ")", "SUCCESS")
                End If
            End If

        Catch ex As Exception
            If bWriteLog = True Then
                ReWritetoLog("Creating Index " & sTable & "(" & sColumn & ")", "FAILED")
                WritetoLog(ex.ToString, "")
            Else
                myMsgBox("CreateIndex: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            sReturn = "ERROR"
        End Try

        Return sReturn
    End Function

    Public Function CreateMySQLFKs(ByVal sSchema As String, ByVal sTable As String, ByVal sColumn As String, ByVal sRefSchema As String, ByVal sRefTable As String, ByVal sRefColumn As String, ByVal bWriteLog As Boolean) As String
        Dim MYCN As New MySqlConnection
        Dim MYCM As New MySqlCommand
        Dim sSQL As String
        Dim sProcessSQL As String = ""
        Dim bExecuted As Boolean = False
        Dim sReturn As String = ""

        Application.DoEvents()

        If bWriteLog = True Then
            WritetoLog("Creating Foreign Key " & sTable & "(" & sColumn & ") ...", "")
        End If

        Try
            'scans if index exist
            sSQL = "USE sys; "
            sSQL &= "SELECT IF(EXISTS(SELECT c.*, pk.Constraint_TYPE FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk, INFORMATION_SCHEMA.KEY_COLUMN_USAGE c WHERE c.TABLE_SCHEMA='" & sSchema & "' AND c.TABLE_NAME='" & sTable & "' AND c.COLUMN_NAME='" & sColumn & "' AND c.CONSTRAINT_NAME='" & sTable & "_" & sColumn & "_FK' AND c.TABLE_NAME = pk.TABLE_NAME AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME AND pk.CONSTRAINT_TYPE='FOREIGN KEY' LIMIT 1),1,0) as result;"

            If GetFieldMySQL(sSQL, GetConnectionStringMY(""), "result") = "1" Then
                'if index exist drop index
                sSQL = "ALTER TABLE `" & sSchema & "`.`" & sTable & "` "
                sSQL &= "DROP FOREIGN KEY " & sColumn & ";"
                sProcessSQL = ProcessMySQL(sSQL, GetConnectionStringMY(""))
            End If

            sSQL = "SET FOREIGN_KEY_CHECKS=0; "
            sSQL &= "USE " & sSchema & "; "
            'sSQL &= "ALTER TABLE " & sTable & "_" & sColumn & "_IDX ON " & sTable & "(" & sColumn & ")"

            sSQL &= "ALTER TABLE `" & sSchema & "`.`" & sTable & "` "
            sSQL &= "ADD Constraint `" & sTable & "_" & sColumn & "_FK` "
            sSQL &= "FOREIGN KEY(`" & sColumn & "`) "
            sSQL &= "REFERENCES `" & sRefSchema & "`.`" & sRefTable & "` (`" & sRefColumn & "`) "
            sSQL &= "On DELETE NO ACTION On UPDATE NO ACTION; "
            sSQL &= "SET FOREIGN_KEY_CHECKS=1;"

            sProcessSQL = ProcessMySQL(sSQL, GetConnectionStringMY(sSchema))

            If sProcessSQL <> "" Then
                If bWriteLog = True Then
                    ReWritetoLog("Creating Foreign Key " & sTable & "(" & sColumn & ")", "FAILED")
                    WritetoLog(sProcessSQL, "")
                Else
                    myMsgBox("CreateMySQLFKs: " & sProcessSQL, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                End If

                sReturn = sProcessSQL
            Else
                If bWriteLog = True Then
                    ReWritetoLog("Creating Foreign Key " & sTable & "(" & sColumn & ")", "SUCCESS")
                End If
            End If

        Catch ex As Exception
            If bWriteLog = True Then
                ReWritetoLog("Creating Foreign Key " & sTable & "(" & sColumn & ")", "FAILED")
                WritetoLog(ex.ToString, "")
            Else
                myMsgBox("CreateMySQLFKs: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            sReturn = "ERROR"
        End Try

        Return sReturn
    End Function

    Public Function ExtractZipFile(ByVal sZipFile As String, ByVal sExtractFolder As String, ByVal files As ArrayList, ByVal bEnc As Boolean, ByVal sPass As String) As String

        Application.DoEvents()

        If sExtractFolder.StartsWith("\") = False Then
            sExtractFolder = "\" & sExtractFolder
        End If

        If sExtractFolder.EndsWith("\") = False Then
            sExtractFolder = sExtractFolder & "\"
        End If

        sExtractFolder = sDatabaseLocation & sExtractFolder

        If Directory.Exists(sExtractFolder) = False Then
            Directory.CreateDirectory(sExtractFolder)
        End If

        Dim zip As New Chilkat.Zip()
        zip.UnlockComponent(ChilkatZipLicenseKey)

        Dim success As Boolean
        success = zip.OpenZip(sZipFile)
        If bEnc Then
            zip.DecryptPassword = sPass
        End If

        'Check zip if it has the correct files
        Try
            Dim zipEntry As Chilkat.ZipEntry = zip.FirstEntry
            Dim zipFiles As New ArrayList
            Dim iMatch As Integer = 0

            While Not zipEntry Is Nothing
                zipFiles.Add(zipEntry.FileName)
                zipEntry = zipEntry.NextEntry
            End While

            For x As Integer = 0 To files.Count - 1
                Dim sSelected As String = files.Item(x).ToString & ".BAK"
                For y As Integer = 0 To zipFiles.Count - 1
                    If sSelected.ToUpper = zipFiles.Item(y).ToString.ToUpper Then
                        iMatch += 1
                        Exit For
                    End If
                Next
                If (x + 1) <> iMatch Then
                    myMsgBox("ExtractZipFile: Cannot find '" & sSelected & "' on zip file", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    WritetoLog("Cannot find '" & sSelected & "' on zip file", "")
                    Return ""
                End If
            Next

        Catch ex As Exception
            'ignore when error
        End Try

        'Create the extract folder, or clear it first.
        Dim directoryInfo As New DirectoryInfo(sExtractFolder)
        If directoryInfo.Exists Then
            '           directoryInfo.Delete(True)
        End If
        directoryInfo.Create()

        ' Unzip to a specific directory:
        Dim count As Integer
        count = zip.Unzip(sExtractFolder)
        If (count = -1) Then
            MsgBox(zip.LastErrorText)
            Return ""
            'Else
            'MsgBox("Unzipped " + Str(count) + " files and directories")
        End If

        Return sExtractFolder
    End Function

    Public Function CleanDatabase(ByVal sTable As String, ByVal sConnstring As String) As Boolean

        Dim bOK As Boolean = True

        Application.DoEvents()

        Try

            Select Case sTable

                Case "tblOrderHeaders"

                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE tblOrderHeaders ")
                    sql.AppendLine("SET CustomerName = REPLACE(CustomerName, '\', '') ")
                    sql.AppendLine("WHERE CustomerName LIKE '%\%' ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE tblOrderHeaders ")
                    sql.AppendLine("SET StreetNumber = REPLACE(StreetNumber, '\', '/') ")
                    sql.AppendLine("WHERE StreetNumber LIKE '%\%' ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE tblOrderHeaders ")
                    sql.AppendLine("SET StreetName = REPLACE(StreetName, '\', '/') ")
                    sql.AppendLine("WHERE StreetName LIKE '%\%' ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE tblOrderHeaders ")
                    sql.AppendLine("SET TableNo = REPLACE(TableNo, '\', '') ")
                    sql.AppendLine("WHERE TableNo LIKE '%\%' ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE tblOrderHeaders ")
                    sql.AppendLine("SET Comments = REPLACE(Comments, '\', '') ")
                    sql.AppendLine("WHERE Comments LIKE '%\%' ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If


                Case "tblCustomers"

                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE tblCustomers ")
                    sql.AppendLine("SET FullName = REPLACE(FullName, '\', '/') ")
                    sql.AppendLine("WHERE FullName LIKE '%\%' ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE tblCustomers ")
                    sql.AppendLine("SET StreetNumber = REPLACE(StreetNumber, '\', '/') ")
                    sql.AppendLine("WHERE StreetNumber LIKE '%\%' ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE tblCustomers ")
                    sql.AppendLine("SET StreetName = REPLACE(StreetName, '\', '/') ")
                    sql.AppendLine("WHERE StreetName LIKE '%\%' ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE tblCustomers ")
                    sql.AppendLine("SET CustomerPhone = REPLACE(CustomerPhone, '\', '/') ")
                    sql.AppendLine("WHERE CustomerPhone LIKE '%\%' ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                Case "tblBalanceSheet"

                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE tblBalanceSheet ")
                    sql.AppendLine("SET OOPaidAmt = 0 ")
                    sql.AppendLine("WHERE OOPaidAmt = NULL ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE tblBalanceSheet ")
                    sql.AppendLine("SET OOPaidOnlineAmt = 0 ")
                    sql.AppendLine("WHERE OOPaidOnlineAmt = NULL or OOPaidOnlineAmt = ''")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE tblBalanceSheet ")
                    sql.AppendLine("SET OOUnpaidAmt = 0 ")
                    sql.AppendLine("WHERE OOUnpaidAmt = NULL or OOUnpaidAmt = ''")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                Case "tblSpecialsArticles"
                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE tblSpecialsArticles ")
                    sql.AppendLine("SET Description = REPLACE(Description, '\', '') ")
                    sql.AppendLine("WHERE Description LIKE '%\%' ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                Case "tblStreets"

                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE tblStreets ")
                    sql.AppendLine("SET Street = REPLACE(Street, '\', '/') ")
                    sql.AppendLine("WHERE Street LIKE '%\%' ")

                    If ProcessMSSERVER(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

            End Select

        Catch ex As Exception
            myMsgBox("CleanDatabase: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK

    End Function

    Public Function CleanDatabaseMYSQL(ByVal sTable As String, ByVal sConnstring As String) As Boolean
        Dim bOK As Boolean = True

        Application.DoEvents()

        Try
            Select Case sTable
                Case "Audit"

                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE Audit SET Synced = 1 WHERE Synced = 49")

                    If ProcessMySQL(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE Audit SET Synced = 0 WHERE Synced = 48")

                    If ProcessMySQL(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                Case "tblCondimentArticles"

                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE tblCondimentArticles SET ArticleID = null WHERE ArticleID = 0")

                    If ProcessMySQL(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                Case "tblOrderDetails"

                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE tblOrderDetails SET SpecialRecID  = null WHERE SpecialRecID = 0")

                    If ProcessMySQL(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE tblOrderDetails SET SplitItemID = null WHERE SplitItemID = 0")

                    If ProcessMySQL(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                Case "tblOrderHeaders"

                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE tblOrderHeaders SET AccountNumber = null WHERE AccountNumber = 0")

                    If ProcessMySQL(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                Case "tblSpecialsArticles"

                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE tblSpecialsArticles ")
                    sql.AppendLine("SET IncludeInTotal = null ")
                    sql.AppendLine("WHERE IncludeInTotal = 0")

                    If ProcessMySQL(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                Case "tblSubCategory"

                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE tblSubCategory SET SubCategoryID = null WHERE SubCategoryID = 0")

                    If ProcessMySQL(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                    sql = New StringBuilder

                    sql.AppendLine("UPDATE tblSubCategory SET DisplayPriority = null WHERE DisplayPriority = 0")

                    If ProcessMySQL(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

                Case "tblTable"

                    Dim sql As New StringBuilder

                    sql.AppendLine("UPDATE tblTable SET CustomerID = null WHERE CustomerID = 0")

                    If ProcessMySQL(sql.ToString, sConnstring) <> "" Then
                        bOK = False
                        myMsgBox("CleanDatabase: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    End If

            End Select
        Catch ex As Exception
            myMsgBox("CleanDatabase: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK
    End Function

    Public Function ProcessMySQL(ByVal mysSQL As String, ByVal sConnString As String) As String
        Dim MYCN As New MySqlConnection
        Dim MYCM As New MySqlCommand
        Dim sResult As String = ""

        Application.DoEvents()


        Try
            MYCN.ConnectionString = sConnString
            MYDBOpen(MYCN)

            MYCM.Connection = MYCN
            MYCM.CommandText = mysSQL
            sResult = MyExecuteNonQueryMY(MYCM)

        Catch ex As Exception
            sResult = ex.Message
        Finally
            MYCN.Close()
        End Try

        Return sResult
    End Function

    Public Function ProcessMySQL(ByVal mysSQL As String, ByVal sConnString As String, ByVal iTime As Integer) As String
        Dim MYCN As New MySqlConnection
        Dim MYCM As New MySqlCommand
        Dim sResult As String = ""

        Application.DoEvents()

        Try
            MYCN.ConnectionString = sConnString
            MYDBOpen(MYCN)

            MYCM.Connection = MYCN
            MYCM.CommandText = mysSQL
            MYCM.CommandTimeout = iTime
            sResult = MyExecuteNonQueryMY(MYCM)

        Catch ex As Exception
            sResult = ex.Message
        Finally
            MYCN.Close()
        End Try

        Return sResult
    End Function

    Public Function ProcessMSSERVER(ByVal mysSQL As String, ByVal sConnString As String) As String
        Dim MSCN As New SqlConnection
        Dim MSCM As New SqlCommand
        Dim sResult As String = ""

        Application.DoEvents()

        Try
            MSCN.ConnectionString = sConnString
            MSDBOpen(MSCN)

            MSCM.Connection = MSCN
            MSCM.CommandText = mysSQL
            sResult = MyExecuteNonQueryMS(MSCM)

        Catch ex As Exception
            sResult = ex.Message
        Finally
            MSCN.Close()
        End Try

        Return sResult
    End Function

    Public Function ProcessMSSERVER(ByVal mysSQL As String, ByVal sConnString As String, ByVal iTime As Integer) As String
        Dim MSCN As New SqlConnection
        Dim MSCM As New SqlCommand
        Dim sResult As String = ""

        Application.DoEvents()

        Try
            MSCN.ConnectionString = GetConnectionStringMS(sConnString)
            MSDBOpen(MSCN)

            MSCM.Connection = MSCN
            MSCM.CommandText = mysSQL
            MSCM.CommandTimeout = iTime
            sResult = MyExecuteNonQueryMS(MSCM)

        Catch ex As Exception
            sResult = ex.Message
            MsgBox(sResult)
        Finally
            MSCN.Close()
        End Try

        Return sResult
    End Function

    Public Function ProcessSQLTransactMS(ByVal sSQL As String, ByRef MSCMD As SqlClient.SqlCommand, ByRef sError As String) As Boolean
        Dim bReturn As Boolean = True
        Try
            MSCMD.CommandText = sSQL

            Try
                Dim sProcess As String = ""

                sProcess = MyExecuteNonQueryMS(MSCMD)

                If sProcess <> "" Then
                    bReturn = False
                End If

            Catch ex As Exception
                bReturn = False
            End Try

        Catch ex As Exception
            bReturn = False
        End Try

        Return bReturn
    End Function

    Public Function ReadMSSERVER(ByVal mysSQL As String, ByVal sConnString As String) As Long        'Returns Rows Affected
        Dim MSCN As New SqlConnection
        Dim MSCM As New SqlCommand
        Dim MSAD As SqlDataAdapter
        Dim DS As New DataSet
        Dim iResult As Long

        Application.DoEvents()

        DS.Clear()

        Try
            MSCN.ConnectionString = sConnString
            MSDBOpen(MSCN)
            MSAD = New SqlDataAdapter(mysSQL, MSCN)
            MSAD.Fill(DS)

            iResult = DS.Tables(0).Rows.Count

        Catch ex As Exception
            myMsgBox("ReadMSSERVER: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        Finally
            MSCN.Close()
        End Try

        Return iResult
    End Function

    Public Function GetFieldMySQL(ByVal mysSQL As String, ByVal sConnString As String, ByVal sField As String) As String        'Returns Rows Affected
        Dim MYCN As New MySqlConnection
        Dim MYCM As New MySqlCommand
        Dim MYDR As MySqlDataReader
        Dim sResult As String = ""

        Application.DoEvents()

        Try
            MYCN.ConnectionString = sConnString
            MYDBOpen(MYCN)

            MYCM.Connection = MYCN
            MYCM.CommandText = mysSQL
            MYDR = MYCM.ExecuteReader

            Do While MYDR.Read
                sResult = MYDR.Item(sField).ToString
            Loop

        Catch ex As Exception
            sResult = "ERROR"
        Finally
            MYCN.Close()
        End Try

        Return sResult
    End Function

    'Works like scalar. This is for SHOW WARNINGS command
    Public Function GetFieldMySQLReader1Item(ByVal mysSQL As String, ByVal sConnString As String, ByVal sField As String) As String        'Returns Rows Affected
        Dim MYCN As New MySqlConnection
        Dim MYCM As New MySqlCommand
        Dim MYDR As MySqlDataReader
        Dim sReturn As String = ""

        Application.DoEvents()

        Try
            MYCN.ConnectionString = sConnString
            MYDBOpen(MYCN)

            MYCM.Connection = MYCN
            MYCM.CommandText = mysSQL
            MYDR = MYCM.ExecuteReader

            MYDR.Read()
            If MYDR.HasRows Then
                sReturn = "MYSQL WARNING: " & MYDR.Item(sField).ToString
            End If

        Catch ex As Exception
            sReturn = ex.Message
        Finally
            MYCN.Close()
        End Try

        Return sReturn
    End Function

    Public Function GetFieldMSSQL(ByVal mssSQL As String, ByVal sConnString As String, ByVal sField As String) As String        'Returns Rows Affected
        Dim MSCN As New SqlConnection
        Dim MSCM As New SqlCommand
        Dim MSDR As SqlDataReader
        Dim sResult As String = ""

        Application.DoEvents()

        Try
            MSCN.ConnectionString = sConnString
            MSDBOpen(MSCN)

            MSCM.Connection = MSCN
            MSCM.CommandText = mssSQL
            MSDR = MSCM.ExecuteReader

            Do While MSDR.Read
                sResult = MSDR.Item(sField).ToString
            Loop

        Catch ex As Exception
            sResult = "ERROR"
        Finally
            MSCN.Close()
        End Try

        Return sResult
    End Function

    Public Function GetConnectionStringMY(ByVal sDatabase As String) As String
        Dim sReturn As String = "server=localhost; user id=root; password=" + sMySQLRootPass + "; Connect Timeout=10000;SslMode=none"

        Application.DoEvents()

        If sDatabase <> "" Then
            sReturn = "server=localhost; user id=root; password=" + sMySQLRootPass + "; database=" + sDatabase + "; Connect Timeout=10000;SslMode=none"
        End If

        Return sReturn
    End Function

    Public Function GetConnectionStringMS(ByVal sDatabase As String) As String
        Dim sServerName As String = ""
        Dim sReturn As String

        Application.DoEvents()

        sServerName = My.Computer.Name
        sReturn = "Data Source=" & sServerName & ";Integrated Security=False;User ID=dpos;Password=dpos99"

        If sDatabase <> "" Then
            sReturn = "Data Source=" & sServerName & ";Integrated Security=False;Initial Catalog=" + sDatabase + ";User ID=dpos;Password=dpos99"
        End If

        Return sReturn
    End Function

    Public Function MYDBOpen(ByVal sConn As MySqlConnection) As Boolean
        Dim bResult As Boolean = False

        Application.DoEvents()

        Try
            If sConn.State = ConnectionState.Open Then sConn.Close()
            sConn.Open()

            'oks
            bResult = True
        Catch ex As Exception
            myMsgBox("MYDBOpen: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bResult
    End Function

    Public Function MYDBOpenNoReport(ByVal sConn As MySqlConnection) As Boolean
        Dim bResult As Boolean = False

        Application.DoEvents()

        Try
            If sConn.State = ConnectionState.Open Then sConn.Close()
            sConn.Open()
            bResult = True
        Catch ex As Exception

        End Try

        Return bResult
    End Function

    Public Function MSDBOpen(ByVal sConn As SqlConnection) As Boolean
        Dim bResult As Boolean = False

        Application.DoEvents()

        Try
            If sConn.State = ConnectionState.Open Then sConn.Close()
            sConn.Open()
            bResult = True
        Catch ex As Exception
            myMsgBox("MSDBOpen: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bResult
    End Function

    Public Function MSDBOpenNoReport(ByVal sConn As SqlConnection) As Boolean
        Dim bResult As Boolean = False

        Application.DoEvents()

        Try
            If sConn.State = ConnectionState.Open Then sConn.Close()
            sConn.Open()
            bResult = True
        Catch ex As Exception

        End Try

        Return bResult
    End Function

    Public Function MyExecuteNonQueryMY(ByVal thisCommand As MySqlCommand) As String
        Dim iRows As String = ""

        Application.DoEvents()

        Try
            iRows = thisCommand.ExecuteNonQuery()
            iRows = ""
        Catch ex As Exception
            myMsgBox("MyExecuteNonQuery: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            iRows = ex.Message
        End Try

        Return iRows
    End Function

    Public Function MyExecuteNonQueryMS(ByVal thisCommand As SqlCommand) As String
        Dim iRows As String = ""

        Application.DoEvents()

        Try
            iRows = thisCommand.ExecuteNonQuery()
            iRows = ""
        Catch ex As Exception
            myMsgBox("MyExecuteNonQueryMS: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            iRows = ex.Message
        End Try

        Return iRows
    End Function

    Public Function MyExecuteReaderMS(ByRef SQLCommand As SqlCommand) As SqlDataReader
        Dim SQLDataReader As SqlDataReader

        Try
            SQLDataReader = SQLCommand.ExecuteReader()
            Return SQLDataReader
        Catch ex As Exception
            myMsgBox("MyExecuteReaderMS: " & ex.Message & vbCrLf & SQLCommand.CommandText, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return Nothing
    End Function

    Public Function MyExecuteReaderMS(ByRef SQLCommand As SqlCommand, ByRef Behavior As CommandBehavior) As SqlDataReader
        Dim SQLDataReader As SqlClient.SqlDataReader

        Try
            SQLDataReader = SQLCommand.ExecuteReader(Behavior)
            Return SQLDataReader
        Catch ex As Exception
            myMsgBox("MyExecuteReaderMS: " & ex.Message & vbCrLf & SQLCommand.CommandText, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return Nothing
    End Function

    Public Function MyExecuteScalarMS(ByRef SQLCommand As SqlCommand) As Object
        Dim Test As Object = Nothing
        Try
            Test = SQLCommand.ExecuteScalar()
        Catch ex As Exception
            myMsgBox("MyExecuteScalarMS: " & ex.Message & vbCrLf & SQLCommand.CommandText, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return Test
    End Function

    Public Function MyExecuteScalarMY(ByRef SQLCommand As MySqlCommand, ByRef bSuccess As Boolean) As Object
        Dim Test As Object = Nothing
        bSuccess = True
        Try
            ConvertQueryBoolToInt(SQLCommand.CommandText)
            Test = SQLCommand.ExecuteScalar()
        Catch ex As Exception
            bSuccess = False
            myMsgBox("MyExecuteScalarMY: " & ex.Message & vbCrLf & SQLCommand.CommandText, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return Test
    End Function

    Public Sub ConvertQueryBoolToInt(ByRef sSQL As String)

        Try
            sSQL = Regex.Replace(sSQL, "'false'", "0", RegexOptions.IgnoreCase)
            sSQL = Regex.Replace(sSQL, "'true'", "1", RegexOptions.IgnoreCase)
        Catch ex As Exception
            MsgBox("ConvertQueryBoolToInt" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

    End Sub

    Public Function DropExistingEscapeFunction() As Boolean
        Dim bOK As Boolean = True
        Dim sql As New StringBuilder

        Try
            sql.AppendLine("IF EXISTS(SELECT * from sys.objects where name='fn_Esc')")
            sql.AppendLine("BEGIN")
            sql.AppendLine("DROP FUNCTION [dbo].[fn_Esc]")
            sql.AppendLine("END")

            If ProcessMSSERVER(sql.ToString, GetConnectionStringMS("DeliverITSQL")) <> "" Then
                bOK = False
                myMsgBox("DropExistingEscapeFunction: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            If ProcessMSSERVER(sql.ToString, GetConnectionStringMS("DPosSysSQL")) <> "" Then
                bOK = False
                myMsgBox("DropExistingEscapeFunction: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            If ProcessMSSERVER(sql.ToString, GetConnectionStringMS("StockSQL")) <> "" Then
                bOK = False
                myMsgBox("DropExistingEscapeFunction: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            If ProcessMSSERVER(sql.ToString, GetConnectionStringMS("StreetsSQL")) <> "" Then
                bOK = False
                myMsgBox("DropExistingEscapeFunction: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            If ProcessMSSERVER(sql.ToString, GetConnectionStringMS("TimeClockSQL")) <> "" Then
                bOK = False
                myMsgBox("DropExistingEscapeFunction: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

        Catch ex As Exception
            myMsgBox("DropExistingEscapeFunction: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK
    End Function

    Public Function WriteColumnHeaders(ByVal sFileName As String, ByVal sTableName As String, ByVal sSchema As String) As Boolean

        Application.DoEvents()

        Dim bOK As Boolean = False
        Dim sHeaders As New StringBuilder

        Try
            sHeaders.Append(GetAllColumnHeaders(sTableName, GetConnectionStringMS(sSchema)))

            'removes header file if exist
            DeleteFile(sFileName & ".txt")

            FileOpen(1, sFileName & ".txt", OpenMode.Append)
            PrintLine(1, sHeaders.ToString)
            FileClose(1)

            bOK = True
        Catch ex As Exception
            myMsgBox("WriteColumnHeaders: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bOK

    End Function

    Public Function AddEscapeFunction() As Boolean

        Dim bOK As Boolean = True
        Dim sql As New StringBuilder

        Try
            sql.AppendLine("CREATE FUNCTION [dbo].[fn_Esc] (@Str nvarchar(max))")
            sql.AppendLine("RETURNS nvarchar(max) AS")
            sql.AppendLine("BEGIN")
            sql.AppendLine("DECLARE @Result nvarchar(max)")
            sql.AppendLine("SET @Result = REPLACE(@Str, '\', '\\')")
            sql.AppendLine("SET @Result = REPLACE(@Result, CHAR(0) , '\0')	")
            sql.AppendLine("SET @Result = REPLACE(@Result, CHAR(8) , '\b')")
            sql.AppendLine("SET @Result = REPLACE(@Result, CHAR(9), '\t')")
            'sql.AppendLine("SET @Result = REPLACE(@Result, CHAR(10), '\n')")
            'sql.AppendLine("SET @Result = REPLACE(@Result, CHAR(13), '\r')")
            sql.AppendLine("SET @Result = REPLACE(@Result, CHAR(26), '\Z')")
            sql.AppendLine("SET @Result = REPLACE(@Result, '""', '\""')")
            sql.AppendLine("SET @Result = char(34) + @Result + char(34)")
            sql.AppendLine("RETURN @Result")
            sql.AppendLine("END")

            If ProcessMSSERVER(sql.ToString, GetConnectionStringMS("DeliverITSQL")) <> "" Then
                bOK = False
                myMsgBox("AddEscapeFunction: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            If ProcessMSSERVER(sql.ToString, GetConnectionStringMS("DPosSysSQL")) <> "" Then
                bOK = False
                myMsgBox("AddEscapeFunction: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            If ProcessMSSERVER(sql.ToString, GetConnectionStringMS("StockSQL")) <> "" Then
                bOK = False
                myMsgBox("AddEscapeFunction: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            If ProcessMSSERVER(sql.ToString, GetConnectionStringMS("StreetsSQL")) <> "" Then
                bOK = False
                myMsgBox("AddEscapeFunction: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

            If ProcessMSSERVER(sql.ToString, GetConnectionStringMS("TimeClockSQL")) <> "" Then
                bOK = False
                myMsgBox("AddEscapeFunction: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

        Catch ex As Exception
            myMsgBox("AddEscapeFunction: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK

    End Function

    Public Function ConfigureCMDShell() As Boolean

        Dim bOK As Boolean = True
        Dim sql As New StringBuilder

        Try
            sql.AppendLine("USE [master];")
            sql.AppendLine("EXEC sp_configure 'show advanced options', 1;")
            sql.AppendLine("RECONFIGURE;")
            sql.AppendLine("EXEC sp_configure 'xp_cmdshell', 1;")
            sql.AppendLine("RECONFIGURE;")

            If ProcessMSSERVER(sql.ToString, GetConnectionStringMS("master")) <> "" Then
                bOK = False
                myMsgBox("ConfigureCMDShell: Unable to execute script!", "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If

        Catch ex As Exception
            myMsgBox("ConfigureCMDShell: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK

    End Function

    Public Function CreateDPosMySQLUsers() As Boolean
        Dim bReturn As Boolean = False

        Dim sError As String = ""
        WritetoLog("", "")
        WritetoLog("Creating Table for DPos MySQL Users...", "")
        sError = CreateTable("mysql_dposcredentials", "(`RecID` int(11) NOT NULL AUTO_INCREMENT, `DPosMySQLUser` varchar(255) DEFAULT NULL, `DPosMySQLPass` varchar(255) DEFAULT NULL, PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8", "deliveritsql", False)

        If sError <> "" Then
            ReWritetoLog("Creating Table for DPos MySQL Users", "FAILED")
            WritetoLog(sError, "")

            Return bReturn
        End If

        sError = ""

        Dim sSQL As String = ""
        Dim sProcess As String = ""
        WritetoLog("Inserting DPos Root Credentials to Table...", "")
        sSQL = "INSERT INTO mysql_dposcredentials (DPosMySQLUser,DPosMySQLPass) VALUES ('root','" & EncryptString(sMySQLRootPass) & "')"
        sProcess = ProcessMySQL(sSQL, GetConnectionStringMY("deliveritsql"))

        If sProcess <> "" Then
            ReWritetoLog("Inserting DPos Root Credentials to Table", "FAILED")
            WritetoLog(sError, "")

            Return bReturn
        Else
            ReWritetoLog("Inserting DPos Root Credentials to Table", "SUCCESS")
        End If

        WritetoLog("Inserting DPos User Credentials to Table...", "")
        sSQL = "INSERT INTO mysql_dposcredentials (DPosMySQLUser,DPosMySQLPass) VALUES ('dpos','" & EncryptString(sMySQLDPosPass) & "')"
        sProcess = ProcessMySQL(sSQL, GetConnectionStringMY("deliveritsql"))

        If sProcess <> "" Then
            ReWritetoLog("Inserting DPos User Credentials to Table", "FAILED")
            WritetoLog(sError, "")

            Return bReturn
        Else
            ReWritetoLog("Inserting DPos User Credentials to Table", "SUCCESS")
        End If

        bReturn = True

        Return bReturn
    End Function

    Public Function WriteDatabaseType(ByVal sDBType As String) As Boolean
        Dim bReturn As Boolean = False
        Dim sDPosINILoc As String

        sDPosINILoc = GetDatabaseLocationDPOS(1) & "\Dpos.ini"

        Try
            If CreateDPosIni() = True Then
                WriteToIni("DATABASE-TYPE=" & sDBType, "DATABASE-TYPE=", sDPosINILoc)
                System.Threading.Thread.Sleep(2000)
            End If

            If sMySQLDPosPass <> "" And sMySQLRootPass <> "" Then
                WriteToIni("ROOT-USER=" & EncryptString(sMySQLRootPass), "ROOT-USER=", sDPosINILoc)
                System.Threading.Thread.Sleep(2000)
                WriteToIni("DPOS-USER=" & EncryptString(sMySQLDPosPass), "DPOS-USER=", sDPosINILoc)
                System.Threading.Thread.Sleep(2000)
            End If

            bReturn = True
        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Function OpenCloudSync() As Boolean
        Dim bReturn As Boolean = False
        Dim sDPosCloudSyncLoc As String

        If bDemo = True Then
            sDPosCloudSyncLoc = "C:\VBNET\DPos\Program\DPOSCloudSync.exe"
        Else
            sDPosCloudSyncLoc = GetDatabaseLocationDPOS(1) & "\Program\DPOSCloudSync.exe"
        End If

        Try
            If WriteDatabaseType(1) = False Then
                Return bReturn
            End If

            If WriteToIni("MYSQL-TRIGGERS=", "MYSQL-TRIGGERS=", GetDatabaseLocationDPOS(1) & "\cloud.ini") = False Then
                Return bReturn
            End If

            If Process.GetProcessesByName("DPOSCloudSync").Length > 0 Then
                ' Cloud app is already running.
            Else
                Try
                    Dim startInfo As New ProcessStartInfo

                    startInfo.FileName = sDPosCloudSyncLoc
                    startInfo.Arguments = GetDatabaseLocationDPOS(1)
                    Process.Start(startInfo)
                Catch ex As Exception
                    myMsgBox("Client App not found", "CloudSync", myMsgBoxDisplay.OkOnly)
                End Try

            End If

            bReturn = True
        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Function CreateDPosIni() As Boolean
        Dim bReturn As Boolean = False
        Dim sDPosINILoc As String

        If Directory.Exists(GetDatabaseLocationDPOS(1)) = False Then
            Directory.CreateDirectory(GetDatabaseLocationDPOS(1))
        End If

        sDPosINILoc = GetDatabaseLocationDPOS(1) & "\Dpos.ini"

        Try
            If myINIExist(sDPosINILoc) = False Then
                File.Create(sDPosINILoc).Close()
                System.Threading.Thread.Sleep(2000)
            End If

            WriteToIni("DATABASE-TYPE=1", "DATABASE-TYPE=", sDPosINILoc)
            System.Threading.Thread.Sleep(2000)

            bReturn = True
        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Sub WritetoLog(ByVal sOutputText As String, ByVal sStatus As String)
        Dim iGetLastRow As DataGridViewRow

        Application.DoEvents()

        Try
            frmMain.DataGridView1.Rows.Add(sOutputText, sStatus)

            iGetLastRow = frmMain.DataGridView1.Rows(frmMain.DataGridView1.Rows.Count - 1)
            iGetLastRow.Selected = True
            frmMain.DataGridView1.FirstDisplayedScrollingRowIndex = frmMain.DataGridView1.Rows.Count - 1
        Catch ex As Exception
            myMsgBox("WriteToLog: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try
    End Sub

    Public Sub ReWritetoLog(ByVal sOutputText As String, ByVal sStatus As String)
        Dim iGetLastRow As DataGridViewRow

        Application.DoEvents()

        Try
            iGetLastRow = frmMain.DataGridView1.Rows(frmMain.DataGridView1.Rows.Count - 2)
            frmMain.DataGridView1.Rows.Remove(iGetLastRow)
            frmMain.DataGridView1.Rows.Add(sOutputText, sStatus)

            iGetLastRow = frmMain.DataGridView1.Rows(frmMain.DataGridView1.Rows.Count - 2)
            iGetLastRow.Selected = True
            frmMain.DataGridView1.FirstDisplayedScrollingRowIndex = frmMain.DataGridView1.Rows.Count - 2
        Catch ex As Exception
            myMsgBox("ReWritetoLog: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try
    End Sub

    Public Sub myReportProgressBar(ByVal iReport As Integer)
        Application.DoEvents()

        If iReport > 100 Then
            frmMain.ProgressBar1.Value = 100
            frmMain.lblProgress.Text = "Progress ... 100%"
        Else
            frmMain.ProgressBar1.Value = iReport
            frmMain.lblProgress.Text = "Progress ... " & iReport & "%"
        End If
    End Sub

    Public Function GetInfoFromIni(ByVal sFilter As String, ByVal sIniFileName As String, ByVal bShowError As Boolean) As String

        Dim sInfo As String = ""
        Dim iniItem As New clsIni
        Dim sIniList As New ArrayList

        Try
            ' Get the list of settings from the ini file.
            sIniList = ReadIniFile(sFilter, sIniFileName, bShowError)

            ' Browse the contents of the ini file.
            For Each iniItem In sIniList
                ' Check if this is the info that we are looking for.
                If iniItem.Header = sFilter Then
                    sInfo = iniItem.Value
                    Exit For
                End If
            Next

        Catch ex As Exception
            myMsgBox("GetInfoFromIni: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return sInfo

    End Function

    Public Function ReadIniFile(ByVal sFilter As String, ByVal sIniFileName As String, ByVal bShowError As Boolean) As ArrayList

        Dim iniContents As New ArrayList

        Try
            ' Check if the file exists in the directory.
            If CheckIfIniFileExists(sIniFileName) = True Then
                Dim lineList As New List(Of Object)(File.ReadAllLines(sIniFileName))

                ' Check if the ini file has any contents.
                If lineList.Count <> 0 Then

                    ' Loop through the ini file.
                    For Each line In lineList

                        Dim iniSetting As New clsIni
                        iniSetting.Header = GetField(line.ToString, "=", 1)
                        iniSetting.Value = line.ToString.Substring(Len(sFilter & "="), Len(line.ToString) - Len(sFilter & "="))
                        iniContents.Add(iniSetting)

                    Next

                End If
            End If

        Catch ex As Exception
            If bShowError = True Then
                myMsgBox("ReadIniFile: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            End If
        End Try

        Return iniContents

    End Function

    Public Function CheckIfIniFileExists(ByVal sIniFileName As String) As Boolean

        Dim bExists As Boolean = False

        Try
            ' Check if the file exists.
            If File.Exists(sIniFileName) Then
                bExists = True
            Else
                bExists = False
            End If

        Catch ex As Exception
            myMsgBox("CheckIfIniFileExists: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bExists

    End Function

    Public Function GetNumeric(ByVal value As String) As Integer

        Dim integerVal As String = String.Empty
        Dim collection As MatchCollection = Regex.Matches(value, "\d+")

        For Each m As Match In collection
            integerVal += m.ToString()
        Next

        Return Convert.ToInt32(integerVal)

    End Function

    Function GetField(ByVal sRec As String, ByVal sDelim As Char, ByVal sCount As Integer) As String
        If sRec = "" Then
            'nothing to check
            Return ""
            Exit Function
        End If

        Dim iCharCnt, iItemCnt, iRecLength As Integer
        Dim sTemp As String = ""
        Dim sOut As String = ""
        Try
            iRecLength = sRec.Length
            Do
                Do While sRec.Substring(iCharCnt, 1) <> sDelim
                    sTemp = sTemp & sRec.Substring(iCharCnt, 1)
                    iCharCnt += 1
                    If iRecLength = iCharCnt Then Exit Do
                Loop
                iCharCnt += 1
                iItemCnt += 1
                If iItemCnt = sCount Then
                    sOut = sTemp
                Else
                    sTemp = ""
                End If
            Loop Until iCharCnt >= iRecLength
        Catch ex As Exception
            myMsgBox("GetField: " & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return sOut
    End Function

    Public Function isCloudSyncEnabled() As String
        Dim bReturn As Boolean = False

        Try
            If GetFieldMySQL("Select * from tblsettings where setting='EnableCloud'", GetConnectionStringMY("deliveritsql"), "SettingValue") = "Y" Then
                bReturn = True
            End If
        Catch ex As Exception
            'Do nothing
        End Try

        Return bReturn
    End Function

    Public Function CreateBackupDirectory(ByVal sDirZipPath As String, ByVal sDirBackupFilePath As String) As Boolean
        Dim bReturn As Boolean = False

        Try
            WritetoLog("Creating Backup Directory", "")
            If Directory.Exists(sDirZipPath) = False Then
                Directory.CreateDirectory(sDirZipPath)
                System.Threading.Thread.Sleep(2000)
            End If

            If Directory.Exists(sDirBackupFilePath) Then
                Directory.Delete(sDirBackupFilePath, True)
            End If

            Directory.CreateDirectory(sDirBackupFilePath)
            WritetoLog("Backup Directory has been created", "")

            bReturn = True
        Catch ex As Exception
            Directory.CreateDirectory(sDirBackupFilePath)
            WritetoLog("Backup Directory has been failed", "")
        End Try

        Return bReturn
    End Function

    Public Function IsJSON(ByVal sJSON As String) As Boolean

        Dim bIsJSON As Boolean = False

        Try
            ' Trim the json string from unnecessary spaces in the start and end.
            sJSON = sJSON.Trim

            ' Check if the string is a json string.
            If sJSON.StartsWith("{") = True Or sJSON.StartsWith("[") = True Then
                bIsJSON = True
            Else
                bIsJSON = False
            End If

        Catch ex As Exception
            MsgBox("IsJSON" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
            bIsJSON = False
        End Try

        Return bIsJSON

    End Function

    ''' <summary>
    ''' Generates the clientid from the user's machine mac address and the store id stored in the 
    ''' DPos settings database (ClientID setting).
    ''' </summary>
    ''' <returns>the clientid to be used for sending data to the server.</returns>
    Public Function GenerateClientID(ByVal sStoreID As String) As String

        Dim sMacAddress As String
        Dim sMacAddressSha As String
        Dim sClientIDMD5 As String
        Dim sFirstThreeChar As String
        Dim sGeneratedID As String = ""

        Try
            ' The mac address of the pc.
            sMacAddress = GetMacAddress()

            ' Generate a sha256 hash of the mac address.
            sMacAddressSha = GetSHA256(sMacAddress)

            ' Generate an md5 hash of the clientid.
            sClientIDMD5 = GetMd5Hash(sStoreID)

            ' Extract the first three characters of the md5 hash client id for salting. 
            sFirstThreeChar = sClientIDMD5.Substring(0, 3)

            ' Combine the first three characters of the md5 hash client id to the sha256 hash of the mac address and generate a sha256 hash.
            sGeneratedID = GetSHA256(sFirstThreeChar & sMacAddressSha)

        Catch ex As Exception
            MsgBox("GenerateClientID" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return sGeneratedID

    End Function

    ''' <summary>
    ''' This function gets the mac address of the current computer.
    ''' </summary>
    ''' <returns>a string containing the mac address of the current computer.</returns>
    Public Function GetMacAddress() As String

        Dim sMacAddress As String = ""

        Try
            ' Browse through each NIC. (wireless, ethernet, etc.)
            For Each nic As System.Net.NetworkInformation.NetworkInterface In System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                ' Only get the ethernet type and check if it is operational status is up.
                If nic.NetworkInterfaceType = Net.NetworkInformation.NetworkInterfaceType.Ethernet And nic.Name = "Local Area Connection" And nic.OperationalStatus = Net.NetworkInformation.OperationalStatus.Up Then
                    sMacAddress = nic.GetPhysicalAddress.ToString
                    Exit For
                End If

            Next

            ' The ethernet adapter is not available. Use whatever adapter is ipenabled(running).
            If sMacAddress = "" Then
                sMacAddress = GetMacAddressOfIPEnabled()
            End If

            ' There is no active adapter. Get the mac address of the ethernet adapter.
            If sMacAddress = "" Then
                For Each nic As System.Net.NetworkInformation.NetworkInterface In System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                    ' Only get the ethernet type and check if it is operational status is up.
                    If nic.NetworkInterfaceType = Net.NetworkInformation.NetworkInterfaceType.Ethernet And nic.Name = "Local Area Connection" Then
                        sMacAddress = nic.GetPhysicalAddress.ToString
                        Exit For
                    End If

                Next
            End If

            ' There is no ethernet adapter detected. Get the first adapter from the list. Last resort.
            If sMacAddress = "" Then
                Dim nics() As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces()
                sMacAddress = nics(1).GetPhysicalAddress.ToString
            End If

        Catch ex As Exception
            MsgBox("GetMacAddress" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return sMacAddress

    End Function

    ''' <summary>
    ''' This function gets the mac address of the current adapter that is enabled when the ethernet adapter is not present.
    ''' </summary>
    ''' <returns>a string containing the mac address of the current computer.</returns>
    Public Function GetMacAddressOfIPEnabled() As String

        Dim oWMIService As Object
        Dim oColAdapters As Object
        Dim oObjAdapter As Object
        Dim sMacAddress As String = ""

        Try

            oWMIService = GetObject("winmgmts:" & "!\\.\root\cimv2")
            oColAdapters = oWMIService.ExecQuery("Select * from Win32_NetworkAdapterConfiguration Where IPEnabled = True")

            For Each oObjAdapter In oColAdapters
                'MsgBox("MAC address formatted: " & oObjAdapter.MACAddress & vbNewLine & "MAC address unformatted: " & Replace(oObjAdapter.MACAddress, ":", ""))
                sMacAddress = Replace(oObjAdapter.MACAddress, ":", "")
                Exit For
            Next

            oObjAdapter = Nothing
            oColAdapters = Nothing
            oWMIService = Nothing

        Catch ex As Exception
            MsgBox("GetMacAddressOfIPEnabled" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return sMacAddress

    End Function

    ''' <summary>
    ''' Hashes the given string using SHA256 encryption.
    ''' </summary>
    ''' <param name="sData">The string to be hashed.</param>
    ''' <returns>the string hashed using SHA256 encryption.</returns>
    Public Function GetSHA256(ByVal sData As String) As String

        Dim sResult As String = ""
        Dim sha = SHA256.Create
        Dim bytesToHash() As Byte = System.Text.Encoding.ASCII.GetBytes(sData)

        Try
            bytesToHash = sha.ComputeHash(bytesToHash)

            For Each b As Byte In bytesToHash
                sResult += b.ToString("x2")
            Next

            ' Dispose the object to avoid memory leaks.
            sha.Dispose()

        Catch ex As Exception
            MsgBox("GetSHA256" & vbCrLf & ex.ToString, MsgBoxStyle.Critical)
        End Try

        Return sResult

    End Function

    Public Function GetMd5Hash(ByVal sData As String)

        Dim sHashedString As String = ""
        Dim md5Hash As MD5 = MD5.Create()
        Dim data As Byte() = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(sData))     ' Convert the input string to a byte array and compute the hash. 

        Try
            ' Create a new Stringbuilder to collect the bytes and create a string. 
            Dim sBuilder As New StringBuilder()

            ' Loop through each byte of the hashed data and format each one as a hexadecimal string. 
            Dim iCount As Integer
            For iCount = 0 To data.Length - 1
                sBuilder.Append(data(iCount).ToString("x2"))
            Next iCount

            sHashedString = sBuilder.ToString

            ' Dispose the object to avoid memory leaks.
            md5Hash.Dispose()

        Catch ex As Exception
            MsgBox("GetMd5Hash" & vbCrLf & ex.ToString)
        End Try

        Return sHashedString

    End Function

    Public Function CheckForInternetConnection() As Boolean

        Try
            Using client = New WebClient()
                Using stream = client.OpenRead("http://www.google.com")
                    Return True
                End Using
            End Using
        Catch
            Return False
        End Try

    End Function

    Public Function AppendParameters(ByVal sURL As String, ByVal prmList As StringDictionary) As String

        Dim sNewURL As String = sURL
        Try

            If prmList Is Nothing Then
                Return sURL
            End If

            Dim bFirst As Boolean = True
            If sURL.Contains("?") Then
                bFirst = False
            End If
            For Each itemKey As DictionaryEntry In prmList
                Dim sKey As String = itemKey.Key
                Dim sValue As String = itemKey.Value
                If bFirst Then
                    sNewURL &= "?" & sKey & "=" & sValue
                    bFirst = False
                Else
                    sNewURL &= "&" & sKey & "=" & sValue
                End If
            Next
        Catch ex As Exception

        End Try

        Return sNewURL

    End Function
End Module