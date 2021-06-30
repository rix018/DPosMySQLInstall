Imports System.IO
Imports MySql.Data

Module modUpgradeDatabase
    Public sDatabaseType As Integer
    Public arSettings As ArrayList

    Public Enum DatabaseType As Integer
        MSSERVER = 2
        MYSQL = 1
    End Enum

    Public Function DatabaseUpgradeCloud() As Boolean
        Dim bReturn As Boolean = True

        Try
            If CheckIfSPExist("UpdateCustomerSummary", "deliveritsql") = False Then
                Dim sString As New System.Text.StringBuilder
                sString.Append(My.Resources.CreateSPUpdateCustomerSummaryMYSQL)

                If bReturn Then bReturn = ProcessSQLforSPCreationMySQL(sString.ToString, GetConnectionStringMY("DeliveritSQL"))
            End If
        Catch ex As Exception

        End Try

        Return bReturn
    End Function

    Public Function DatabaseUpgrade(ByVal DBType As Integer) As Boolean
        Dim bReturn As Boolean = True

        Try
            sDatabaseType = 0
            sDatabaseType = DBType

#Region "New Table: tblSubCategoryPrinters"
            If CheckIfDBTableExist("tblSubCategoryPrinters", "DeliveritSQL", sDatabaseType) = False Then
                If bReturn Then bReturn = CreateTableSubCategoryPrinters()
                If bReturn Then bReturn = AddPrintersToSubCategoryPrinters("")
                If sDatabaseType = DatabaseType.MSSERVER Then
                    If bReturn Then bReturn = DBfieldRemoveMS("tblSubCategory", "Printers", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMS("tblSubCategory", "PrintBold", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMS("tblSubCategory", "ShopPrint", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMS("tblSubCategory", "PickupPrint", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMS("tblSubCategory", "DeliveryPrint", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMS("tblSubCategory", "TablePrint", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMS("tblSubCategory", "Highlight", "DeliveritSQL")
                Else
                    If bReturn Then bReturn = DBfieldRemoveMY("tblSubCategory", "Printers", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMY("tblSubCategory", "PrintBold", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMY("tblSubCategory", "ShopPrint", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMY("tblSubCategory", "PickupPrint", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMY("tblSubCategory", "DeliveryPrint", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMY("tblSubCategory", "TablePrint", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMY("tblSubCategory", "Highlight", "DeliveritSQL")
                End If

            End If
#End Region

#Region "New Table: tblPayments"
            If CheckIfDBTableExist("tblPayments", "DeliveritSQL", sDatabaseType) = False Then
                '1st line is for 4983 - Migration Tool: Upgrade Database issue on creating tblPayment
                If bReturn Then bReturn = CleanTenderTypeID(sDatabaseType)
                If bReturn Then bReturn = CreateTablePayments()
                If sDatabaseType = DatabaseType.MSSERVER Then
                    If bReturn Then bReturn = DBfieldRemoveMS("tblOrderHeaders", "TenderTypeID", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMS("tblOrderHeaders", "AmountPaid", "DeliveritSQL")
                Else
                    If bReturn Then bReturn = DBfieldRemoveMY("tblOrderHeaders", "TenderTypeID", "DeliveritSQL")
                    If bReturn Then bReturn = DBfieldRemoveMY("tblOrderHeaders", "AmountPaid", "DeliveritSQL")
                End If
            End If
#End Region

#Region "SP UpdateCustomerSummary on MySQL"
            If CheckIfSPExist("UpdateCustomerSummary", "deliveritsql") = False Then
                Dim sString As New System.Text.StringBuilder
                sString.Append(My.Resources.CreateSPUpdateCustomerSummaryMYSQL)

                If bReturn Then bReturn = ProcessSQLforSPCreationMySQL(sString.ToString, GetConnectionStringMY("DeliveritSQL"))
            End If
#End Region

#Region "DeliveritSQL - tblOrderHeaders. New Column: PosReferenceID"
            If sDatabaseType = DatabaseType.MSSERVER Then
                If bReturn Then bReturn = DBfieldAddMS("DeliveritSQL", "tblOrderHeaders", "PosReferenceID", "NVARCHAR(50)", False, False, True, True)
            Else
                If bReturn Then bReturn = DBfieldAddMY("DeliveritSQL", "tblOrderHeaders", "PosReferenceID", "NVARCHAR(50)", False, False, True, True)
            End If
#End Region

#Region "RebuildtblOrderHeaders on MSSERVER"
            If sDatabaseType = DatabaseType.MSSERVER Then
                If bReturn Then bReturn = RebuildtblOrderHeadersMS()
                If bReturn Then bReturn = RecreatetblOrderHeadersTriggerMS()
            End If
#End Region

#Region "DeliveritSQL - tblOrderHeaders. New Column: MakeStartTime"
            If sDatabaseType = DatabaseType.MSSERVER Then
                If bReturn Then bReturn = DBfieldAddMS("DeliveritSQL", "tblOrderHeaders", "MakeStartTime", "NVARCHAR(10)", False, False, True, True)
            Else
                If bReturn Then bReturn = DBfieldAddMY("DeliveritSQL", "tblOrderHeaders", "MakeStartTime", "NVARCHAR(10)", False, False, True, True)
            End If
#End Region

#Region "DeliveritSQL - tblSettingsTouch. New Column: SubItem"
            If sDatabaseType = DatabaseType.MSSERVER Then
                If bReturn Then bReturn = DBfieldAddMS("DeliveritSQL", "tblSettingsTouch", "SubItem", "NVARCHAR(10)", False, False, True, True)
            Else
                If bReturn Then bReturn = DBfieldAddMY("DeliveritSQL", "tblSettingsTouch", "SubItem", "NVARCHAR(10)", False, False, True, True)
            End If
#End Region

#Region "DeliveritSQL - tblSettingsTouch. New Columns: StartDate, EndDate, Repeating. tblSpecialsArticles. New Column: ChargeItem"
            If sDatabaseType = DatabaseType.MSSERVER Then
                If bReturn Then bReturn = DBfieldAddMS("DeliveritSQL", "tblSettingsTouch", "StartDate", "NVARCHAR(10)", False, False, True, True)
                If bReturn Then bReturn = DBfieldAddMS("DeliveritSQL", "tblSettingsTouch", "EndDate", "NVARCHAR(10)", False, False, True, True)
                If bReturn Then bReturn = DBfieldAddMS("DeliveritSQL", "tblSettingsTouch", "Repeating", "NVARCHAR(10)", False, False, True, True)
                If bReturn Then bReturn = DBfieldAddMS("DeliveritSQL", "tblSpecialsArticles", "ChargeItem", "NVARCHAR(10)", False, False, True, True)
            Else
                If bReturn Then bReturn = DBfieldAddMY("DeliveritSQL", "tblSettingsTouch", "StartDate", "NVARCHAR(10)", False, False, True, True)
                If bReturn Then bReturn = DBfieldAddMY("DeliveritSQL", "tblSettingsTouch", "EndDate", "NVARCHAR(10)", False, False, True, True)
                If bReturn Then bReturn = DBfieldAddMY("DeliveritSQL", "tblSettingsTouch", "Repeating", "NVARCHAR(10)", False, False, True, True)
                If bReturn Then bReturn = DBfieldAddMY("DeliveritSQL", "tblSpecialsArticles", "ChargeItem", "NVARCHAR(10)", False, False, True, True)
            End If
#End Region

#Region "DeliveritSQL - tblUnsavedOrderHeaders. Modified Column: StreetName"
            If sDatabaseType = DatabaseType.MSSERVER Then
                If bReturn Then
                    If ProcessMSSERVER("ALTER TABLE tblUnsavedOrderHeaders ALTER COLUMN StreetNumber nvarchar(255)", GetConnectionStringMS("DeliveritSQL")) <> "" Then bReturn = False
                End If
            Else
                If bReturn Then
                    If ProcessMySQL("SET foreign_key_checks=0; ALTER TABLE tblUnsavedOrderHeaders CHANGE COLUMN `StreetNumber` `StreetNumber` VARCHAR(255) NULL DEFAULT NULL; SET foreign_key_checks=1; commit;", GetConnectionStringMY("DeliveritSQL")) <> "" Then bReturn = False
                End If
            End If
#End Region

#Region "DeliveritsQL - tblOrderHeaders. Added Index: tblOrderHeaders(PickupTime)"
            If sDatabaseType = DatabaseType.MSSERVER Then
                If bReturn Then bReturn = CreateIndexMS("tblOrderHeaders", "PickupTime", "DeliveritSQL")
            Else
                If bReturn Then bReturn = CreateIndexMY("tblOrderHeaders", "PickupTime", "DeliveritSQL")
            End If
#End Region

#Region "DeliveritsQL - tblArticles. New Column: ExcludeCondimentCharge"
            If sDatabaseType = DatabaseType.MSSERVER Then
                If bReturn Then bReturn = DBfieldAddMS("DeliveritSQL", "tblArticles", "ExcludeCondimentCharge", "bit", False, False, True, True)
            Else
                If bReturn Then bReturn = DBfieldAddMY("DeliveritSQL", "tblArticles", "ExcludeCondimentCharge", "bit", False, False, True, True)
            End If
#End Region

#Region "DeliveritSQL - New Table tblPublic Holidays (02.20.09)"
            If bReturn Then bReturn = CreateTablePublicHolidays()
#End Region

#Region "DeliveritsQL - tblArticles. New Column: ExcludeFromMinimum (02.20.10)"
            If sDatabaseType = DatabaseType.MSSERVER Then
                If bReturn Then bReturn = DBfieldAddMS("DeliveritSQL", "tblArticles", "ExcludeFromMinimum", "bit", False, False, True, True)
            Else
                If bReturn Then bReturn = DBfieldAddMY("DeliveritSQL", "tblArticles", "ExcludeFromMinimum", "bit", False, False, True, True)
            End If
#End Region

#Region "DeliveritSQL - New Table tblBarcodeItems (02.20.10)"
            If bReturn Then bReturn = CreateTableBarcodeItems()
#End Region


        Catch ex As Exception
            myMsgBox("DatabaseUpgrade: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bReturn = False
        End Try

        Return bReturn
    End Function

    Public Function CleanTenderTypeID(ByVal DBType As Integer) As Boolean
        Dim sSQL As String = ""
        Dim sResult As String = ""
        Dim bOK As Boolean = False
        Dim iEFTPOSTenderID As Integer = 0
        Dim sDBError As String

        Try
            If DBType = DatabaseType.MSSERVER Then
                iEFTPOSTenderID = AddTenderType("EFTPOS", DBType)

                If iEFTPOSTenderID = 0 Then
                    Return False
                End If

                sSQL = "UPDATE tblOrderHeaders "
                sSQL &= "SET TenderTypeID = REPLACE(TenderTypeID, 'btneftpos', '" & iEFTPOSTenderID.ToString & "') "
                sSQL &= "WHERE TenderTypeID like '%btneftpos%' "

                sDBError = ProcessMSSERVER(sSQL, "DeliveritSQL", 60)

                If sDBError <> "" Then
                    bOK = False
                Else
                    bOK = True
                End If

            Else
                bOK = True ' to be done.
            End If

        Catch ex As Exception
            'to be done

        End Try

        Return bOK

    End Function

    Public Function AddTenderType(ByVal sTender As String, ByVal DBType As Integer) As Integer
        Dim iTenderID As Integer = 0
        Dim sSQL As String = ""

        Try
            iTenderID = myCInt(GetTenderTypeID(sTender, DBType))

            If iTenderID = 0 Then
                sSQL = "INSERT INTO tblTenderTypes "
                sSQL &= "(TenderType, EFTPOSAddress, SurchargePLU, XeroAccountName, XeroAccountCode, Hidden, Expense, SaveAsUnpaid) "
                sSQL &= "VALUES "
                sSQL &= "('" & sTender & "', '', '', '', '', 0, 0, 0) "
                sSQL &= "SELECT SCOPE_IDENTITY() AS TenderTypeID "

                If DBType = DatabaseType.MSSERVER Then
                    iTenderID = myCInt(GetFieldMSSQL(sSQL, GetConnectionStringMS("DeliveritSQL"), "TenderTypeID"))
                Else
                    'Not necessarily add codes for mysql transaction but i added anyway for future purposes
                    iTenderID = myCInt(GetFieldMySQL(sSQL, GetConnectionStringMY("DeliveritSQL"), "TenderTypeID"))
                End If
            End If

        Catch ex As Exception
            'to be done
        End Try

        Return iTenderID

    End Function

    Public Function GetTenderTypeID(ByVal sType As String, ByVal DBType As Integer) As String
        Dim sSQL As String
        Dim sID As String = "0"

        Try
            sSQL = "SELECT TenderTypeID "
            sSQL &= "FROM tblTendertypes "
            sSQL &= "WHERE TenderType = '" & sType & "'"

            If DBType = DatabaseType.MSSERVER Then
                sID = myCStr(GetFieldMSSQL(sSQL, GetConnectionStringMS("DeliveritSQL"), "TenderTypeID"))
            Else
                'Just in case you still need this function for MySQL
                sID = myCStr(GetFieldMySQL(sSQL, GetConnectionStringMY("DeliveritSQL"), "TenderTypeID"))
            End If

            sID = myCStr(myCInt(sID))

        Catch ex As Exception
            'to be done
        End Try

        Return sID

    End Function

    Public Function CheckIfDBTableExist(ByVal sTable As String, ByVal sDatabase As String, ByVal DBType As Integer) As Boolean
        Dim sSQL As String
        Dim bExists As Boolean = False

        Try
            If DBType = DatabaseType.MSSERVER Then
                sSQL = "SELECT COUNT(*) as TableCount FROM sys.objects " &
                    "WHERE object_id = OBJECT_ID(N'[dbo].[" & sTable & "]') " &
                    "AND type in (N'U')"

                bExists = myCBool(GetFieldMSSQL(sSQL, GetConnectionStringMS(sDatabase), "TableCount"))
            Else
                sSQL = "SELECT COUNT(*) as TableCount " &
                    "FROM information_schema.tables " &
                    "WHERE table_schema = '" & sDatabase & "' " &
                    "AND table_name = '" & sTable & "' " &
                    "LIMIT 1"

                bExists = myCBool(GetFieldMySQL(sSQL, GetConnectionStringMY(sDatabase), "TableCount"))
            End If

        Catch ex As Exception
            myMsgBox("CheckIfDBTableExist: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bExists

    End Function
    Public Function CreateTablePayments() As Boolean

        Dim bReturn As Boolean = False

        bReturn = ExecuteSQLFromResources(My.Resources.NormalisedPayments, "DeliveritSQL", 150)

        Return bReturn

    End Function

    Public Function ExecuteSQLFromResources(ByVal file As String, ByVal database As String, ByVal iTime As Integer) As Boolean

        Dim bReturn As Boolean = False

        Try
            Dim sString As New System.Text.StringBuilder

            sString.Append(file)

            If ProcessMSSERVER(sString.ToString, "DeliveritSQL", iTime) <> "" Then
            Else
                bReturn = True
            End If

        Catch ex As Exception
            myMsgBox("ExecuteSQLFromResources:" & ex.Message, "", myMsgBoxDisplay.OkOnly)
        End Try

        Return bReturn

    End Function

    Public Function CreateTableSubCategoryPrinters() As Boolean
        Dim bReturn As Boolean = False

        Try
            If sDatabaseType = DatabaseType.MSSERVER Then
                bReturn = CreateTableSubCategoryPrintersMS()
            Else
                bReturn = CreateTableSubCategoryPrintersMY()
            End If
        Catch ex As Exception

        End Try

        Return bReturn
    End Function
    Public Function CreateTableSubCategoryPrintersMS() As Boolean
        Dim sql As String
        Dim bReturn As Boolean = False
        Dim sProcess As String

        sql = "IF NOT EXISTS "
        sql &= "(SELECT * FROM INFORMATION_SCHEMA.TABLES "
        sql &= "WHERE TABLE_NAME = 'tblSubCategoryPrinters') "
        sql &= "BEGIN "
        sql &= "CREATE TABLE tblSubCategoryPrinters( "
        sql &= "RecID int identity(1,1) CONSTRAINT tblSubCategoryPrinters_PK PRIMARY KEY, "
        sql &= "SubCategoryID int, "
        sql &= "PrinterNumber NVARCHAR(50), "
        sql &= "Setting NVARCHAR(50), "
        sql &= "Value NVARCHAR(50)) "
        sql &= "END "

        Try
            sProcess = ProcessMSSERVER(sql, GetConnectionStringMS("DeliveritSQL"))
            If sProcess <> "" Then
                Return bReturn
            End If

            sql = "DELETE FROM tblSubCategoryPrinters"
            sProcess = ProcessMSSERVER(sql, GetConnectionStringMS("DeliveritSQL"))
            If sProcess <> "" Then
                Return bReturn
            End If

            bReturn = True
        Catch ex As Exception
            myMsgBox("CreateTableSubCategoryPrintersMS: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bReturn

    End Function

    Public Function CreateTableSubCategoryPrintersMY() As Boolean
        Dim sql As String
        Dim bReturn As Boolean = False
        Dim sProcess As String

        'to do: Change query
        sql = "IF NOT EXISTS "
        sql &= "(SELECT * FROM INFORMATION_SCHEMA.TABLES "
        sql &= "WHERE TABLE_NAME = 'tblSubCategoryPrinters') "
        sql &= "BEGIN "
        sql &= "CREATE TABLE tblSubCategoryPrinters( "
        sql &= "RecID int identity(1,1) CONSTRAINT tblSubCategoryPrinters_PK PRIMARY KEY, "
        sql &= "SubCategoryID int, "
        sql &= "PrinterNumber NVARCHAR(50), "
        sql &= "Setting NVARCHAR(50), "
        sql &= "Value NVARCHAR(50)) "
        sql &= "END "

        Try
            sProcess = ProcessMySQL(sql, GetConnectionStringMS("DeliveritSQL"))
            If sProcess <> "" Then
                Return bReturn
            End If

            sql = "DELETE FROM tblSubCategoryPrinters"
            sProcess = ProcessMySQL(sql, GetConnectionStringMS("DeliveritSQL"))
            If sProcess <> "" Then
                Return bReturn
            End If

            bReturn = True
        Catch ex As Exception
            myMsgBox("CreateTableSubCategoryPrintersMY: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bReturn

    End Function

    Public Function CreateTablePublicHolidays() As Boolean

        Dim sql As String
        Dim bReturn As Boolean = False
        Dim sProcess As String = ""

        If sDatabaseType = DatabaseType.MSSERVER Then
            sql = "IF NOT EXISTS "
            sql &= "(SELECT * FROM INFORMATION_SCHEMA.TABLES "
            sql &= "WHERE TABLE_NAME = 'tblPublicHolidays') "
            sql &= "BEGIN "
            sql &= "CREATE TABLE tblPublicHolidays( "
            sql &= "RecID int identity(1,1) CONSTRAINT tblPublicHolidays_PK PRIMARY KEY, "
            sql &= "StartDate NVARCHAR(10), "
            sql &= "EndDate NVARCHAR(10), "
            sql &= "SurchargePLU NVARCHAR(20)) "
            sql &= "END "
        Else
            sql = "USE deliveritsql;"
            sql &= "DROP TABLE IF EXISTS tblPublicHolidays;"
            sql &= "CREATE TABLE tblPublicHolidays ("
            sql &= "`RecID` int(11) Not NULL AUTO_INCREMENT, "
            sql &= "`StartDate` varchar(10) DEFAULT NULL, "
            sql &= "`EndDate` varchar(10) DEFAULT NULL, "
            sql &= "`SurchargePLU` varchar(20) DEFAULT NULL, "
            sql &= "PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8"
        End If

        Try
            If sDatabaseType = DatabaseType.MSSERVER Then
                sProcess = ProcessMSSERVER(sql, GetConnectionStringMS("DeliveritSQL"))
                If sProcess <> "" Then
                    Return bReturn
                End If
            Else
                sProcess = ProcessMySQL(sql, GetConnectionStringMS("DeliveritSQL"))
                If sProcess <> "" Then
                    Return bReturn
                End If
            End If

            bReturn = True
        Catch ex As Exception
            myMsgBox("CreateTablePublicHolidays: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bReturn
    End Function

    Public Function CreateTableBarcodeItems() As Boolean

        Dim sql As String
        Dim bReturn As Boolean = False
        Dim sProcess As String = ""

        If sDatabaseType = DatabaseType.MSSERVER Then
            sql = "IF NOT EXISTS "
            sql &= "(SELECT * FROM INFORMATION_SCHEMA.TABLES "
            sql &= "WHERE TABLE_NAME = 'tblBarcodeItems') "
            sql &= "BEGIN "
            sql &= "CREATE TABLE tblBarcodeItems( "
            sql &= "RecID int identity(1,1) CONSTRAINT tblBarcodeItems_PK PRIMARY KEY, "
            sql &= "Code NVARCHAR(100), "
            sql &= "PLU NVARCHAR(50)) "
            sql &= "END "
        Else
            sql = "USE deliveritsql;"
            sql &= "DROP TABLE IF EXISTS tblBarcodeItems;"
            sql &= "CREATE TABLE tblBarcodeItems ("
            sql &= "`RecID` int(11) Not NULL AUTO_INCREMENT, "
            sql &= "`Code` varchar(100) DEFAULT NULL, "
            sql &= "`PLU` varchar(50) DEFAULT NULL, "
            sql &= "PRIMARY KEY (`RecID`)) ENGINE=InnoDB DEFAULT CHARSET=utf8"
        End If

        Try
            If sDatabaseType = DatabaseType.MSSERVER Then
                sProcess = ProcessMSSERVER(sql, GetConnectionStringMS("DeliveritSQL"))
                If sProcess <> "" Then
                    Return bReturn
                End If
            Else
                sProcess = ProcessMySQL(sql, GetConnectionStringMS("DeliveritSQL"))
                If sProcess <> "" Then
                    Return bReturn
                End If
            End If

            bReturn = True
        Catch ex As Exception
            myMsgBox("CreateTableBarcodeItems: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try

        Return bReturn
    End Function

    Public Function AddPrintersToSubCategoryPrinters(ByVal iFilter As String) As Boolean
        Dim bReturn As Boolean = False

        If sDatabaseType = DatabaseType.MSSERVER Then
            bReturn = AddPrintersToSubCategoryPrintersMS(iFilter)
        Else
            bReturn = AddPrintersToSubCategoryPrintersMY(iFilter)
        End If

        Return bReturn
    End Function
    Public Function AddPrintersToSubCategoryPrintersMS(ByVal iFilter As String) As Boolean

        Dim bReturn As Boolean = False
        Dim subcategoryPrinters As New clsSubCategoryPrinters
        Dim subcategoryPrinter As New clsSubCategoryPrinter
        Dim sqlString As String = ""
        Dim sProcess As String = ""

        Try
            subcategoryPrinters = GetAllSubCategoryPrintersFromSubCatMS(iFilter)
            For Each subcategoryPrinter In subcategoryPrinters
                sqlString = "INSERT INTO tblSubCategoryPrinters(SubCategoryID, PrinterNumber, Setting, Value) "
                sqlString &= "VALUES ('" & subcategoryPrinter.SubCategoryID & "','" & subcategoryPrinter.PrinterNumber & "','" & subcategoryPrinter.PrinterSetting & "','" & subcategoryPrinter.PrinterValue & "')"

                sProcess = ProcessMSSERVER(sqlString, GetConnectionStringMS("DeliveritSQL"))
                If sProcess <> "" Then
                    Return bReturn
                End If

            Next
            bReturn = True
        Catch ex As Exception
            myMsgBox("AddPrintersToSubCategoryPrintersMS: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try
        Return bReturn
    End Function

    Public Function AddPrintersToSubCategoryPrintersMY(ByVal iFilter As String) As Boolean

        Dim bReturn As Boolean = False
        Dim subcategoryPrinters As New clsSubCategoryPrinters
        Dim subcategoryPrinter As New clsSubCategoryPrinter
        Dim sqlString As String = ""
        Dim sProcess As String = ""

        Try
            subcategoryPrinters = GetAllSubCategoryPrintersFromSubCatMY(iFilter)
            For Each subcategoryPrinter In subcategoryPrinters
                sqlString = "INSERT INTO tblSubCategoryPrinters(SubCategoryID, PrinterNumber, Setting, Value) "
                sqlString &= "VALUES ('" & subcategoryPrinter.SubCategoryID & "','" & subcategoryPrinter.PrinterNumber & "','" & subcategoryPrinter.PrinterSetting & "','" & subcategoryPrinter.PrinterValue & "')"

                sProcess = ProcessMySQL(sqlString, GetConnectionStringMY("DeliveritSQL"))
                If sProcess <> "" Then
                    Return bReturn
                End If

            Next
            bReturn = True
        Catch ex As Exception
            myMsgBox("AddPrintersToSubCategoryPrintersMY: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        End Try
        Return bReturn
    End Function

    Public Function GetAllSubCategoryPrintersFromSubCatMS(ByVal iFilter As String) As clsSubCategoryPrinters
        Dim CN As New SqlClient.SqlConnection(GetConnectionStringMS("DeliveritSQL"))
        Dim cmd As New SqlClient.SqlCommand
        Dim dr As SqlClient.SqlDataReader
        Dim thisSubCategoryPrinter As clsSubCategoryPrinter
        Dim sqlString As String = ""
        Dim SubCategoryPrinterArray As New clsSubCategoryPrinters

        Dim iSubcategoryID As Integer = 0
        Dim iPrinterNumber As Integer = 0
        Dim iPrinterSettings As Integer = 0
        Dim sSetting As String = ""

        Try

            Dim arrayPrintersSettings As New ArrayList
            arrayPrintersSettings.Clear()
            arrayPrintersSettings.Add(SUBCAT_PRINTERS)
            arrayPrintersSettings.Add(SUBCAT_PRINTBOLD)
            arrayPrintersSettings.Add(SUBCAT_SHOPPRINT)
            arrayPrintersSettings.Add(SUBCAT_PICKUPPRINT)
            arrayPrintersSettings.Add(SUBCAT_DELIVERYPRINT)
            arrayPrintersSettings.Add(SUBCAT_TABLEPRINT)
            arrayPrintersSettings.Add(SUBCAT_HIGHLIGHT)

            If iFilter = "" Then
                sqlString = "SELECT * FROM tblSubCategory ORDER BY SubCategoryID asc"
            Else
                sqlString = "SELECT * FROM tblSubCategory where SubCategoryID=" & iFilter & " ORDER BY SubCategoryID asc"
            End If

            MSDBOpen(CN)
            cmd.Connection = CN
            cmd.CommandText = sqlString
            dr = cmd.ExecuteReader

            If dr.HasRows Then
                Do While dr.Read()
                    iSubcategoryID = myDRStringMS(dr, "SubCategoryID")

                    Dim arrayPrinterSettingsVal As New ArrayList
                    arrayPrinterSettingsVal.Clear()
                    arrayPrinterSettingsVal.Add(myDRStringMS(dr, "Printers"))
                    arrayPrinterSettingsVal.Add(myDRStringMS(dr, "PrintBold"))
                    arrayPrinterSettingsVal.Add(myDRStringMS(dr, "ShopPrint"))
                    arrayPrinterSettingsVal.Add(myDRStringMS(dr, "PickUpPrint"))
                    arrayPrinterSettingsVal.Add(myDRStringMS(dr, "DeliveryPrint"))
                    arrayPrinterSettingsVal.Add(myDRStringMS(dr, "TablePrint"))
                    arrayPrinterSettingsVal.Add(myDRStringMS(dr, "Highlight"))

                    For iPrinterSettings = 0 To arrayPrinterSettingsVal.Count - 1
                        If arrayPrinterSettingsVal(iPrinterSettings) <> "" Then
                            Dim thisPrinterSetting As String()
                            thisPrinterSetting = arrayPrinterSettingsVal(iPrinterSettings).ToString.Split(",")

                            For iPrinterNumber = 0 To thisPrinterSetting.Count - 1
                                thisSubCategoryPrinter = New clsSubCategoryPrinter
                                thisSubCategoryPrinter.SubCategoryID = iSubcategoryID
                                thisSubCategoryPrinter.PrinterNumber = "Printer" & (iPrinterNumber + 1)
                                thisSubCategoryPrinter.PrinterSetting = arrayPrintersSettings(iPrinterSettings)
                                If thisPrinterSetting(iPrinterNumber).ToString = "" Then
                                    thisSubCategoryPrinter.PrinterValue = "N"
                                Else
                                    thisSubCategoryPrinter.PrinterValue = thisPrinterSetting(iPrinterNumber).ToString
                                End If
                                SubCategoryPrinterArray.Add(thisSubCategoryPrinter)
                            Next
                        End If
                    Next
                Loop
            End If

        Catch ex As Exception
            myMsgBox("GetAllSubCategoryPrintersFromSubCatMS: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        Finally
            CN.Close()
        End Try

        Return SubCategoryPrinterArray
    End Function
    Public Function GetAllSubCategoryPrintersFromSubCatMY(ByVal iFilter As String) As clsSubCategoryPrinters
        Dim CN As New MySqlClient.MySqlConnection(GetConnectionStringMY("DeliveritSQL"))
        Dim cmd As New MySqlClient.MySqlCommand
        Dim dr As MySqlClient.MySqlDataReader
        Dim thisSubCategoryPrinter As clsSubCategoryPrinter
        Dim sqlString As String = ""
        Dim SubCategoryPrinterArray As New clsSubCategoryPrinters

        Dim iSubcategoryID As Integer = 0
        Dim iPrinterNumber As Integer = 0
        Dim iPrinterSettings As Integer = 0
        Dim sSetting As String = ""

        Try

            Dim arrayPrintersSettings As New ArrayList
            arrayPrintersSettings.Clear()
            arrayPrintersSettings.Add(SUBCAT_PRINTERS)
            arrayPrintersSettings.Add(SUBCAT_PRINTBOLD)
            arrayPrintersSettings.Add(SUBCAT_SHOPPRINT)
            arrayPrintersSettings.Add(SUBCAT_PICKUPPRINT)
            arrayPrintersSettings.Add(SUBCAT_DELIVERYPRINT)
            arrayPrintersSettings.Add(SUBCAT_TABLEPRINT)
            arrayPrintersSettings.Add(SUBCAT_HIGHLIGHT)

            If iFilter = "" Then
                sqlString = "SELECT * FROM tblSubCategory ORDER BY SubCategoryID asc"
            Else
                sqlString = "SELECT * FROM tblSubCategory where SubCategoryID=" & iFilter & " ORDER BY SubCategoryID asc"
            End If

            MYDBOpen(CN)
            cmd.Connection = CN
            cmd.CommandText = sqlString
            dr = cmd.ExecuteReader

            If dr.HasRows Then
                Do While dr.Read()
                    iSubcategoryID = myDRStringMY(dr, "SubCategoryID")

                    Dim arrayPrinterSettingsVal As New ArrayList
                    arrayPrinterSettingsVal.Clear()
                    arrayPrinterSettingsVal.Add(myDRStringMY(dr, "Printers"))
                    arrayPrinterSettingsVal.Add(myDRStringMY(dr, "PrintBold"))
                    arrayPrinterSettingsVal.Add(myDRStringMY(dr, "ShopPrint"))
                    arrayPrinterSettingsVal.Add(myDRStringMY(dr, "PickUpPrint"))
                    arrayPrinterSettingsVal.Add(myDRStringMY(dr, "DeliveryPrint"))
                    arrayPrinterSettingsVal.Add(myDRStringMY(dr, "TablePrint"))
                    arrayPrinterSettingsVal.Add(myDRStringMY(dr, "Highlight"))

                    For iPrinterSettings = 0 To arrayPrinterSettingsVal.Count - 1
                        If arrayPrinterSettingsVal(iPrinterSettings) <> "" Then
                            Dim thisPrinterSetting As String()
                            thisPrinterSetting = arrayPrinterSettingsVal(iPrinterSettings).ToString.Split(",")

                            For iPrinterNumber = 0 To thisPrinterSetting.Count - 1
                                thisSubCategoryPrinter = New clsSubCategoryPrinter
                                thisSubCategoryPrinter.SubCategoryID = iSubcategoryID
                                thisSubCategoryPrinter.PrinterNumber = "Printer" & (iPrinterNumber + 1)
                                thisSubCategoryPrinter.PrinterSetting = arrayPrintersSettings(iPrinterSettings)
                                If thisPrinterSetting(iPrinterNumber).ToString = "" Then
                                    thisSubCategoryPrinter.PrinterValue = "N"
                                Else
                                    thisSubCategoryPrinter.PrinterValue = thisPrinterSetting(iPrinterNumber).ToString
                                End If
                                SubCategoryPrinterArray.Add(thisSubCategoryPrinter)
                            Next
                        End If
                    Next
                Loop
            End If

        Catch ex As Exception
            myMsgBox("GetAllSubCategoryPrintersFromSubCatMY: " & ex.ToString, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        Finally
            CN.Close()
        End Try

        Return SubCategoryPrinterArray
    End Function

    Public Function DBfieldRemoveMS(ByVal tblName As String, ByVal tblColumn As String, ByVal sSchema As String) As Boolean
        Dim sSQL As String
        Dim bOK As Boolean = True
        Dim sProcess As String = ""

        sSQL = "ALTER Table " & tblName & " "
        sSQL &= "DROP Column " & tblColumn

        If ProcessMSSERVER(sSQL, GetConnectionStringMS(sSchema)) <> "" Then
            bOK = False
        End If

        Return bOK

    End Function

    Public Function DBfieldRemoveMY(ByVal tblName As String, ByVal tblColumn As String, ByVal sSchema As String) As Boolean
        Dim sSQL As String
        Dim bOK As Boolean = True

        sSQL = "ALTER Table " & tblName & " "
        sSQL &= "DROP Column " & tblColumn

        If ProcessMySQL(sSQL, GetConnectionStringMY(sSchema)) <> "" Then
            bOK = False
        End If

        Return bOK

    End Function

    Public Function DBfieldAddMS(ByVal sDB As String, ByVal sTblName As String, ByVal sTblColumn As String, ByVal sTblType As String, ByVal bCreateIndex As Boolean, ByVal bIndexUnique As Boolean, ByVal bNew As Boolean, ByVal bCreateTrigger As Boolean) As Boolean
        Dim sSQL As String
        Dim bOK As Boolean = True
        Dim sColumnsfound As String
        Dim bFieldExists As Boolean = False
        Dim bNewField As Boolean = False
        Dim sProcess As String = ""

        Try
            bFieldExists = CheckIfDBColumnPresentMS("DeliveritSQL", sTblColumn, sTblName, GetConnectionStringMS(sDB))

            If bNew = True And bFieldExists = True Then
                bNewField = False
            ElseIf bNew = False And bFieldExists = False Then
                bNewField = True
            Else
                bNewField = bNew
            End If

            If bNewField Then
                sSQL = "SELECT COL_LENGTH('" & sTblName & "','" & sTblColumn & "') as col_length"
                sColumnsfound = GetFieldMSSQL(sSQL, GetConnectionStringMS(sDB), "col_length")

                If sColumnsfound <> "ERROR" Then
                    If sColumnsfound = "255" Or (sTblType = "bit" And sColumnsfound = "1") Then
                        'Column already exists do nothing
                        bOK = True
                        Return bOK
                    End If
                Else
                    'Fails to execute command return false
                    myMsgBox("DBfieldAddMS: Error on adding " & sTblColumn & " from " & sDB & " - " & sTblName, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    bOK = False
                    Return bOK
                End If

                sProcess = ""
                sSQL = "ALTER Table " & sTblName & " "
                sSQL &= "ADD " & sTblColumn & " " & sTblType
                sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS(sDB))

                If sProcess <> "" Then
                    'Fails to execute command return false
                    myMsgBox("DBfieldAddMS: Error on adding " & sTblColumn & " from " & sDB & " - " & sTblName, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    bOK = False
                    Return bOK
                End If

                ' Added this code to default settings of added BOOLEAN types to zero (For ExtractExclude)
                ' This is because the default NULL will cause exceptions to a getSetting() if not 0 or 1
                If sTblType = "bit" Then
                    sProcess = ""
                    sSQL = "UPDATE " & sTblName & " SET " & sTblColumn & " = 0"
                    sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS(sDB))

                    If sProcess <> "" Then
                        'Fails to execute command return false
                        myMsgBox("DBfieldAddMS: Error on adding " & sTblColumn & " from " & sDB & " - " & sTblName, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                        bOK = False
                        Return bOK
                    End If
                End If

                If sTblType = "DATETIME" Then
                    sProcess = ""
                    sSQL = "UPDATE " & sTblName & " SET " & sTblColumn & " = '" & GetTableModifiedTime(DatabaseType.MSSERVER) & "'"
                    sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS(sDB))

                    If sProcess <> "" Then
                        'Fails to execute command return false
                        myMsgBox("DBfieldAddMS: Error on adding " & sTblColumn & " from " & sDB & " - " & sTblName, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                        bOK = False
                        Return bOK
                    End If
                End If

                If bCreateIndex Then
                    sProcess = ""
                    If bIndexUnique Then
                        sSQL = "CREATE UNIQUE Index " & sTblName & "_" & sTblColumn & "_IDX"
                    Else
                        sSQL = "CREATE Index " & sTblName & "_" & sTblColumn & "_IDX"
                    End If
                    sSQL &= " ON " & sTblName & " (" & sTblColumn & ")"

                    sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS(sDB))
                    If sProcess <> "" Then
                        'Fails to execute command return false
                        myMsgBox("DBfieldAddMS: Error on adding " & sTblColumn & " from " & sDB & " - " & sTblName, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                        bOK = False
                        Return bOK
                    End If
                End If
            End If

            bOK = True
        Catch ex As Exception
            'Fails to execute command return false
            myMsgBox("DBfieldAddMS: Error on adding " & sTblColumn & " from " & sDB & " - " & sTblName & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK
    End Function

    Public Function CreateIndexMS(ByVal sTable As String, ByVal sColumn As String, ByVal sDatabase As String)
        Dim sSQL As String = ""
        Dim sProcess As String = ""
        Dim bOK As Boolean = True

        Try
            If CheckIfIndexExistsMS(sTable, sColumn, sDatabase) Then
                sSQL = "DROP Index " & sTable & "_" & sColumn & "_IDX"
                sSQL &= " On " & sTable
                sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS(sDatabase))
                If sProcess <> "" Then
                    Return False
                End If
                sProcess = ""
            End If

            sSQL = "CREATE Index " & sTable & "_" & sColumn & "_IDX"
            sSQL &= " ON " & sTable & " (" & sColumn & ")"
            sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS(sDatabase))

            If sProcess <> "" Then
                Return False
            End If

        Catch ex As Exception
            myMsgBox("CreateIndexMS: Error on adding " & sColumn & " from " & sTable & " - " & sDatabase & " index" & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK

    End Function

    Public Function CheckIfIndexExistsMS(ByVal sTable As String, ByVal sColumn As String, ByVal sDatabase As String) As Boolean
        Dim sSQL As String = ""
        Dim sResult As String = ""
        Dim bOK As Boolean = False

        Try
            sSQL = "SELECT * FROM sys.indexes WHERE name = '" & sTable & "_" & sColumn & "_IDX'"
            sResult = GetFieldMSSQL(sSQL, GetConnectionStringMS(sDatabase), "name")

            If sResult = sTable & "_" & sColumn & "_IDX" Then
                bOK = True
            End If

        Catch ex As Exception
            'ShowError(ex)
            myMsgBox("CheckIfIndexExistsMS: Error on checking " & sColumn & " from " & sTable & " - " & sDatabase & " index if exist" & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK

    End Function

    Public Function CreateIndexMY(ByVal sTable As String, ByVal sColumn As String, ByVal sDatabase As String)
        Dim sSQL As String = ""
        Dim sProcess As String = ""
        Dim bOK As Boolean = True

        Try
            If CheckIfIndexExistsMY(sTable, sColumn, sDatabase) Then
                sSQL = "DROP Index " & sTable & "_" & sColumn & "_IDX"
                sSQL &= " On " & sTable
                sProcess = ProcessMySQL(sSQL, GetConnectionStringMY(sDatabase))
                If sProcess <> "" Then
                    Return False
                End If
                sProcess = ""
            End If

            sSQL = "CREATE Index " & sTable & "_" & sColumn & "_IDX"
            sSQL &= " ON " & sTable & " (" & sColumn & ")"
            sProcess = ProcessMySQL(sSQL, GetConnectionStringMY(sDatabase))

            If sProcess <> "" Then
                Return False
            End If

        Catch ex As Exception
            myMsgBox("CreateIndexMY: Error on adding " & sColumn & " from " & sTable & " - " & sDatabase & " index" & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK

    End Function

    Public Function CheckIfIndexExistsMY(ByVal sTable As String, ByVal sColumn As String, ByVal sDatabase As String) As Boolean
        Dim sSQL As String = ""
        Dim sResult As String = ""
        Dim bOK As Boolean = False

        Try
            sSQL = "SHOW Index FROM " & sTable & " WHERE Key_name = '" & sTable & "_" & sColumn & "_IDX'"
            sResult = GetFieldMySQL(sSQL, GetConnectionStringMS(sDatabase), "Key_name")

            If sResult = sTable & "_" & sColumn & "_IDX" Then
                bOK = True
            End If

        Catch ex As Exception
            'ShowError(ex)
            myMsgBox("CheckIfIndexExistsMY: Error on checking " & sColumn & " from " & sTable & " - " & sDatabase & " index if exist" & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK

    End Function

    Public Function DBfieldAddMY(ByVal sDB As String, ByVal sTblName As String, ByVal sTblColumn As String, ByVal sTblType As String, ByVal bCreateIndex As Boolean, ByVal bIndexUnique As Boolean, ByVal bNew As Boolean, ByVal bCreateTrigger As Boolean) As Boolean
        Dim bOK As Boolean = True
        Dim sSQL As String
        Dim bFieldExists As Boolean = False
        Dim bNewField As Boolean = False
        Dim sProcess As String = ""

        Try
            bFieldExists = CheckIfDBColumnPresentMY("DeliveritSQL", sTblColumn, sTblName, GetConnectionStringMY(sDB))

            If bNew = True And bFieldExists = True Then
                bNewField = False
            ElseIf bNew = False And bFieldExists = False Then
                bNewField = True
            Else
                bNewField = bNew
            End If

            sTblType = sTblType.ToLower

            If bNewField Then
                sProcess = ""
                If sTblType = "bit" Then
                    sTblType = "smallint(1)"
                ElseIf sTblType = "datetime" Then
                    sTblType = "Date"
                ElseIf sTblType = "money" Then
                    sTblType = "decimal(10,2)"
                ElseIf sTblType.Contains("NVARCHAR") Then
                    Dim sDelimStart As String = "("
                    Dim sDelimEnd As String = ")" 'Second delimiting word
                    Dim sDatatypeLength As String = (sTblType.Substring(InStr(sTblType, sDelimStart) + sDelimStart.Length - 1, sTblType.Length - sDelimStart.Length - sDelimEnd.Length))
                    Dim stempDataType As String = "varchar(" & CInt(sDatatypeLength) & ")"
                    sTblType = stempDataType
                End If
                sSQL = "ALTER Table " & sTblName & " "
                sSQL &= "ADD " & sTblColumn & " " & sTblType
                sProcess = ProcessMySQL(sSQL, GetConnectionStringMY(sDB))

                If sProcess <> "" Then
                    'Fails to execute command return false
                    myMsgBox("DBfieldAddMY: Error on adding " & sTblColumn & " from " & sDB & " - " & sTblName, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                    bOK = False
                    Return bOK
                End If

                ' Added this code to default settings of added BOOLEAN types to zero (For ExtractExclude)
                ' This is because the default NULL will cause exceptions to a getSetting() if not 0 or 1
                If sTblType = "smallint(1)" Then
                    sProcess = ""
                    sSQL = "UPDATE " & sTblName & " SET " & sTblColumn & " = 0"
                    sProcess = ProcessMySQL(sSQL, GetConnectionStringMY(sDB))

                    If sProcess <> "" Then
                        'Fails to execute command return false
                        myMsgBox("DBfieldAddMY: Error on adding " & sTblColumn & " from " & sDB & " - " & sTblName, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                        bOK = False
                        Return bOK
                    End If
                End If

                If sTblType = "Date" Then
                    sProcess = ""
                    sSQL = "UPDATE " & sTblName & " SET " & sTblColumn & " = '" & GetTableModifiedTime(DatabaseType.MYSQL) & "'"
                    sProcess = ProcessMySQL(sSQL, GetConnectionStringMY(sDB))

                    If sProcess <> "" Then
                        'Fails to execute command return false
                        myMsgBox("DBfieldAddMY: Error on adding " & sTblColumn & " from " & sDB & " - " & sTblName, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                        bOK = False
                        Return bOK
                    End If
                End If

                If bCreateIndex Then
                    sProcess = ""
                    If bIndexUnique Then
                        sSQL = "CREATE UNIQUE Index " & sTblName & "_" & sTblColumn & "_IDX"
                    Else
                        sSQL = "CREATE Index " & sTblName & "_" & sTblColumn & "_IDX"
                    End If
                    sSQL &= " ON " & sTblName & " (" & sTblColumn & ")"

                    If sProcess <> "" Then
                        'Fails to execute command return false
                        myMsgBox("DBfieldAddMY: Error on adding " & sTblColumn & " from " & sDB & " - " & sTblName, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
                        bOK = False
                        Return bOK
                    End If
                End If
            End If

            bOK = True
        Catch ex As Exception
            'Fails to execute command return false
            myMsgBox("DBfieldAddMY: Error on adding " & sTblColumn & " from " & sDB & " - " & sTblName & vbCrLf & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bOK = False
        End Try

        Return bOK

    End Function

    Public Function CheckIfDBColumnPresentMS(ByVal sDatabase As String, ByVal sField As String, ByVal sTable As String, ByVal sConnString As String) As Boolean
        Dim sSQL As String
        Dim bPresent As Boolean = False

        Try
            sSQL = "SELECT CASE WHEN EXISTS ("
            sSQL &= "SELECT 1 FROM sys.columns "
            sSQL &= "WHERE Name = N'" & sField & "' "
            sSQL &= "AND Object_ID = Object_ID(N'" & sDatabase & ".dbo." & sTable & "')) "
            sSQL &= "THEN CAST(1 AS BIT) "
            sSQL &= "ELSE CAST(0 AS BIT) END as isColumnExist"

            bPresent = myCBool(GetFieldMSSQL(sSQL, sConnString, "isColumnExist"))
        Catch ex As Exception

        End Try

        Return bPresent

    End Function

    Public Function CheckIfDBColumnPresentMY(ByVal sDatabase As String, ByVal sField As String, ByVal sTable As String, ByVal sConnString As String) As Boolean
        Dim sSQL As String
        Dim bPresent As Boolean = False

        Try
            sSQL = "SELECT IF(EXISTS("
            sSQL &= "SELECT 1 FROM information_schema.COLUMNS  "
            sSQL &= "WHERE TABLE_SCHEMA = '" & sDatabase & "' "
            sSQL &= "AND TABLE_NAME = '" & sTable & "' "
            sSQL &= "AND COLUMN_NAME = '" & sField & "'),1,0) as isColumnExist"

            bPresent = myCBool(GetFieldMySQL(sSQL, sConnString, "isColumnExist"))
        Catch ex As Exception

        End Try

        Return bPresent

    End Function

    Public Function GetTableModifiedTime(ByVal iDatabaseType As Integer) As String
        Dim sTime As String = ""

        Try
            If sDatabaseType = DatabaseType.MSSERVER Then
                sTime = GetFieldMSSQL("select Convert(varchar,GETDATE(),9) as thisTimeToday", GetConnectionStringMS(""), "thisTime")
            Else
                sTime = GetFieldMySQL("select DATE_FORMAT(NOW(), '%Y-%m-%d %H:%i:%s.%f') as thisTimeToday", GetConnectionStringMS(""), "thisTime")
            End If
        Catch ex As Exception
            sTime = ""
        End Try

        Return sTime

    End Function

    Function myDRStringMS(ByVal dr As SqlClient.SqlDataReader, ByVal sField As String) As String
        Dim sData As String = ""

        If Not IsDBNull(dr.Item(sField)) Then
            sData = Trim(dr.Item(sField))
        End If

        Return sData
    End Function

    Function myDRStringMY(ByVal dr As MySqlClient.MySqlDataReader, ByVal sField As String) As String
        Dim sData As String = ""

        If Not IsDBNull(dr.Item(sField)) Then
            sData = Trim(dr.Item(sField))
        End If

        Return sData
    End Function

    Public Function ProcessSQLforSPCreationMySQL(ByVal sSQL As String, ByVal sDatabase As String) As Boolean
        Dim MYCON As New MySqlClient.MySqlConnection()
        Dim MYSCR As New MySqlClient.MySqlScript()

        MYCON.ConnectionString = sDatabase
        MYSCR = New MySqlClient.MySqlScript(MYCON, sSQL.ToString)
        MYSCR.Delimiter = "$$"
        Try
            MYSCR.Execute()
        Catch ex As MySqlClient.MySqlException
            Return False
        Catch ex1 As Exception
            Return False
        Finally
            MYSCR.Delimiter = ";"
            If MYCON.State = 1 Then
                MYCON.Close()
            End If
        End Try

        Return True
    End Function

    Public Function RebuildtblOrderHeadersMS() As Boolean
        Dim bReturn As Boolean = False

        Dim MSCON As New SqlClient.SqlConnection
        Dim MSCMD As New SqlClient.SqlCommand
        Dim MSTRN As SqlClient.SqlTransaction
        Dim btrnOK As Boolean = True
        Dim sSQL As String
        Dim sProcess As String = ""

        Dim sFKTransactionType As Boolean = False
        Dim sFKTransactionStatus As Boolean = False
        Dim sFKTransactionTypeName As String = ""
        Dim sFKTransactionStatusName As String = ""
        Dim iOldRowsCnt As Integer = 0
        Dim iNewRowsCnt As Integer = 0

        Try
            If CheckIfDBTableExistMS("tblOrderHeaders2", "DeliveritSQL") Then

                sSQL = "DROP TABLE tblOrderHeaders2"

                sProcess = ProcessMSSERVER(sSQL, "")
                If sProcess <> "" Then
                    Return bReturn
                End If
            End If

            iOldRowsCnt = myCInt(GetFieldMSSQL("SELECT COUNT(OrderID) as OrderCount from tblOrderHeaders", GetConnectionStringMS("DeliveritSQL"), "OrderCount"))

            'check constraints on tblOrderHeaders
            sProcess = ""
            sFKTransactionStatus = CheckIfFKExistandGetConstraintMS("DeliveritSQL", "tblOrderHeaders", "TransactionStatus", False, sFKTransactionStatusName, sProcess)
            If sProcess <> "" Then
                bReturn = False
                Return bReturn
            End If

            sProcess = ""
            sFKTransactionType = CheckIfFKExistandGetConstraintMS("DeliveritSQL", "tblOrderHeaders", "TransactionType", False, sFKTransactionTypeName, sProcess)
            If sProcess <> "" Then
                bReturn = False
                Return bReturn
            End If
            sProcess = ""

            'Transaction Start
            MSCON.ConnectionString = GetConnectionStringMS("DeliveritSQL")
            MSDBOpen(MSCON)
            MSTRN = MSCON.BeginTransaction
            MSCMD.Transaction = MSTRN
            MSCMD.Connection = MSCON
            MSCMD.CommandTimeout = 180

            Try
                'execute the process as long as btrnOK is true

                'drops TransactionStatus foreign key if exist
                If sFKTransactionStatus Then
                    btrnOK = DropForeignKeyMS("tblOrderHeaders", sFKTransactionStatusName, MSCMD)
                Else
                    btrnOK = True
                End If

                'drops TransactionType foreign key if exist
                If btrnOK Then
                    If sFKTransactionType Then
                        btrnOK = DropForeignKeyMS("tblOrderHeaders", sFKTransactionTypeName, MSCMD)
                    Else
                        btrnOK = True
                    End If
                End If

                'We're going to transfer the rows to another tblOrderHeaders
                If btrnOK Then
                    sProcess = ""
                    sSQL = "SELECT * INTO tblOrderHeaders2 from tblOrderHeaders"
                    btrnOK = ProcessSQLTransactMS(sSQL, MSCMD, sProcess)
                End If

                'compare row counts before dropping the table
                If btrnOK Then
                    sProcess = ""
                    btrnOK = ComparetblOrderHeadersRows(iOldRowsCnt, MSCMD, sProcess)
                    If btrnOK Then
                        If sProcess <> "" Then
                            btrnOK = False
                        End If
                    End If
                End If

                'drop tblOrderHeaders
                If btrnOK Then
                    sProcess = ""
                    sSQL = "DROP TABLE tblOrderHeaders"
                    btrnOK = ProcessSQLTransactMS(sSQL, MSCMD, sProcess)
                End If

                'renames tblOrderHeaders2 to tblOrderHeaders
                If btrnOK Then
                    sProcess = ""
                    sSQL = "exec sp_rename 'tblOrderHeaders2', 'tblOrderHeaders'"
                    btrnOK = ProcessSQLTransactMS(sSQL, MSCMD, sProcess)
                End If

                'creates PrimaryKey, Indices and FKs
                If btrnOK Then
                    sProcess = ""
                    'PK
                    sSQL = "ALTER TABLE tblOrderHeaders ADD CONSTRAINT tblOrderHeaders_PK PRIMARY KEY CLUSTERED (OrderID);"
                    'Index
                    sSQL &= "CREATE Index tblOrderHeaders_OrderDate_IDX ON tblOrderHeaders(OrderDate);"
                    sSQL &= "CREATE Index tblOrderHeaders_CustomerID_IDX ON tblOrderHeaders(CustomerID);"
                    sSQL &= "CREATE Index tblOrderHeaders_TransactionType_IDX ON tblOrderHeaders(TransactionType);"
                    sSQL &= "CREATE Index tblOrderHeaders_TransactionStatus_IDX ON tblOrderHeaders(TransactionStatus);"
                    sSQL &= "CREATE Index tblOrderHeaders_CustomerPhone_IDX ON tblOrderHeaders(CustomerPhone);"
                    sSQL &= "CREATE Index tblOrderHeaders_CustomerName_IDX ON tblOrderHeaders(CustomerName);"
                    sSQL &= "CREATE Index tblOrderHeaders_Suburb_IDX ON tblOrderHeaders(Suburb);"
                    sSQL &= "CREATE Index tblOrderHeaders_StreetName_IDX ON tblOrderHeaders(StreetName);"
                    sSQL &= "CREATE Index tblOrderHeaders_StreetNumber_IDX ON tblOrderHeaders(StreetNumber);"
                    sSQL &= "CREATE Index tblOrderHeaders_TableNo_IDX ON tblOrderHeaders(TableNo);"
                    sSQL &= "CREATE Index tblOrderHeaders_TableDesc_IDX ON tblOrderHeaders(TableDesc);"
                    sSQL &= "CREATE Index tblOrderHeaders_DeliveredBy_IDX ON tblOrderHeaders(DeliveredBy);"
                    'FKs
                    sSQL &= "ALTER TABLE tblOrderHeaders WITH NOCHECK ADD FOREIGN KEY (TransactionStatus) REFERENCES tblTransactionStatusTypes(TransactionStatusID);"
                    sSQL &= "ALTER TABLE tblOrderHeaders WITH NOCHECK ADD FOREIGN KEY (TransactionType) REFERENCES tblTransactionTypes(TransactionTypeID);"
                    btrnOK = ProcessSQLTransactMS(sSQL, MSCMD, sProcess)
                End If

            Catch ex As Exception
                btrnOK = False
            End Try

            'Commit or rollback?
            If btrnOK Then
                MSTRN.Commit()
            Else
                Try
                    MSTRN.Rollback()
                Catch ex As Exception

                End Try
                bReturn = False
            End If

            bReturn = btrnOK

        Catch ex As Exception
            myMsgBox("RebuildtblOrderHeadersMS:" & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        Finally
            MSCON.Close()
        End Try

        Return bReturn
    End Function

    Public Function DropForeignKeyMS(ByVal sTableName As String, ByVal sConstraintName As String, ByRef MSCMD As SqlClient.SqlCommand) As Boolean
        Dim bReturn As Boolean = True
        Dim thisConstraint As String = ""
        Dim sSQL As String
        Dim sError As String = ""

        Try
            sSQL = "ALTER TABLE [dbo].[" & sTableName & "] DROP CONSTRAINT [" & sConstraintName & "]"
            bReturn = ProcessSQLTransactMS(sSQL, MSCMD, sError)
        Catch ex As Exception
            bReturn = False
        End Try

        Return bReturn
    End Function

    Public Function ComparetblOrderHeadersRows(ByVal iOldRowCount As Integer, ByRef MSCMD As SqlClient.SqlCommand, ByRef sError As String) As Boolean
        Dim bReturn As Boolean = True
        Dim sSQL As String

        Dim iNewRowsCount As Integer = 0

        Try
            sSQL = "SELECT COUNT(*) as NewRowsCount from tblOrderHeaders2"
            MSCMD.CommandText = sSQL
            iNewRowsCount = myCInt(MyExecuteScalarMS(MSCMD))

            If iNewRowsCount <> iOldRowCount Then
                bReturn = False
            End If

        Catch ex As Exception
            bReturn = False
        End Try

        Return bReturn
    End Function

    Public Function RecreatetblOrderHeadersTriggerMS() As Boolean

        Dim bReturn As Boolean = True
        Dim bTblExist As Boolean = True
        Dim sProcess As String = ""

        Try
            bTblExist = CheckIfDBTableExistMS("Audit", "DeliveritSQL")

            If bTblExist Then
                bReturn = CreateTriggerMSSQL("tblOrderHeaders", "DeliveritSQL", sProcess)
            End If
        Catch ex As Exception
            bReturn = False
        End Try

        Return bReturn
    End Function

    Public Function CreateTriggerMSSQL(ByVal sTableName As String, ByVal sDatabaseName As String, ByRef sError As String) As Boolean
        Dim sProcess As String = ""
        Dim bReturn As Boolean = True
        ' This function builds the query string for a table and executes it.

        Try
            Dim sSQL As String = ""

            ' Build the query for the database trigger.
            sSQL &= " CREATE  TRIGGER [dbo].[" & sTableName & "_Trigger] ON [dbo].[" & sTableName & "] FOR INSERT, DELETE, UPDATE AS SET NOCOUNT ON "
            sSQL &= " declare @JSONDATA varchar(4000),@templateQuery nvarchar(max),@outputFieldValue varchar(max),@outputFieldName varchar(max),@outputConcat varchar(max)"
            sSQL &= " ,@dataType varchar(20),@tempFieldname varchar(50),@templateQuery2 nvarchar(max),@outputPKValue varchar(max),@headersString varchar(255),"
            sSQL &= " @insertJsonSQl varchar(max),@PrimaryKey varchar(50),@PrimaryKeyName varchar(50) DECLARE @bit INT ,@field INT ,@maxfield INT ,@char INT ,@fieldname VARCHAR(128) "
            sSQL &= " ,@TableName VARCHAR(128) ,@PKCols VARCHAR(1000) ,@sql VARCHAR(2000), @UpdateDate VARCHAR(21) ,@UserName VARCHAR(128) ,@Type CHAR(1) ,@PKSelect VARCHAR(1000)"
            sSQL &= " SELECT @TableName = '" & sTableName & "' "
            sSQL &= "  SELECT @UserName = SYSTEM_USER , @UpdateDate = CONVERT(VARCHAR(8), GETDATE(), 112)  + ' ' + CONVERT(VARCHAR(12), GETDATE(), 114) IF EXISTS "
            sSQL &= " (SELECT * FROM inserted) IF EXISTS (SELECT * FROM deleted) SELECT @Type = 'U' ELSE SELECT @Type = 'I' ELSE SELECT @Type = 'D'"
            sSQL &= " SELECT * INTO #ins FROM inserted SELECT * INTO #del FROM deleted SELECT @PKCols = COALESCE(@PKCols + ' and', ' on')  + ' i.' + c.COLUMN_NAME + ' = d.' "
            sSQL &= " + c.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk , INFORMATION_SCHEMA.KEY_COLUMN_USAGE c WHERE pk.TABLE_NAME = @TableName AND CONSTRAINT_TYPE = 'PRIMARY KEY'"
            sSQL &= " AND c.TABLE_NAME = pk.TABLE_NAME AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME SELECT @PKSelect = COALESCE(@PKSelect+'+','') + '''' + COLUMN_NAME  "
            sSQL &= " + '=''+convert(varchar(100), coalesce(i.' + COLUMN_NAME +',d.' + COLUMN_NAME + '))+'','''  FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk , "
            sSQL &= " INFORMATION_SCHEMA.KEY_COLUMN_USAGE c WHERE pk.TABLE_NAME = @TableName AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND c.TABLE_NAME = pk.TABLE_NAME AND "
            sSQL &= " c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME SELECT @PrimaryKeyName = COLUMN_NAME FROM    INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk , INFORMATION_SCHEMA.KEY_COLUMN_USAGE c"
            sSQL &= " WHERE  pk.TABLE_NAME = @TableName AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND  c.TABLE_NAME = pk.TABLE_NAME AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME"
            sSQL &= " IF @PKCols IS NULL BEGIN RAISERROR('no PK on table %s', 16, -1, @TableName) RETURN END IF EXISTS (SELECT * FROM inserted) BEGIN IF EXISTS (SELECT * FROM deleted)"
            sSQL &= " BEGIN SELECT @Type = 'U' CREATE TABLE #Temp ( Id int not null identity(1,1), Data nvarchar(100) ); SELECT @templateQuery "
            sSQL &= "  = 'insert #Temp (Data) select convert(varchar(1000),' + @PrimaryKeyName + ') from #ins' exec(@templateQuery) Declare @Id2 nvarchar(100) Declare @dataPK2 nvarchar(100)"
            sSQL &= "  Set @dataPK2 = (select top 1 Data from #Temp) while exists (select * from #Temp) begin select top 1 @dataPK2 = Data from #Temp select top 1 @Id2 = Id from #Temp"
            sSQL &= "  SELECT  @field = 0, @maxfield = MAX(ORDINAL_POSITION) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName set @JSONDATA  = '{'; set @outputConcat = ''"
            sSQL &= "   WHILE @field < @maxfield BEGIN SELECT @field = MIN(ORDINAL_POSITION)FROM INFORMATION_SCHEMA.COLUMNS  WHERE TABLE_NAME = @TableName AND ORDINAL_POSITION > @field"
            sSQL &= "   SELECT @bit = (@field - 1 )% 8 + 1 SELECT @bit = POWER(2,@bit - 1) SELECT @char = ((@field - 1) / 8) + 1 IF SUBSTRING(COLUMNS_UPDATED(),@char, 1) & @bit > 0OR @Type "
            sSQL &= "   IN ('I','D') BEGIN SELECT @fieldname = COLUMN_NAME  FROM INFORMATION_SCHEMA.COLUMNS  WHERE TABLE_NAME = @TableName  AND ORDINAL_POSITION = @field"
            sSQL &= "   if @TableName = 'tblStaff' and @fieldname = 'StaffID'  begin set @templateQuery2 =  'select @fieldValue = ' + @PKSelect  +  ' from #ins i full outer join #del d' + @PKCols "
            sSQL &= "   + ' where i.' + @fieldname + ' <> d.' + @fieldname + ' or (i.' + @fieldname + ' is null and  d.' + @fieldname + ' is not null)' + ' or (i.' + @fieldname "
            sSQL &= "  + ' is not null and  d.'  + @fieldname + ' is null)' exec sp_executesql @templateQuery2, N'@fieldValue varchar(max) out', @outputPKValue out  set @PrimaryKey = @outputPKValue"
            sSQL &= "  set @templateQuery =  'select @fieldValue = convert(varchar(1000),i.' + @fieldname + ')' + ',@fieldNameTemp = '''+ @fieldname + '''' +  ' from #ins i full outer join #del d '"
            sSQL &= "  + @PKCols + ' where '  + ' i.' + @fieldname + ' <> d.' + @fieldname + ' or (i.' + @fieldname + ' is null and  d.' + @fieldname + ' is not null)' + ' or (i.' "
            sSQL &= "  + @fieldname + ' is not null and  d.'  + @fieldname + ' is null) order by d.' + @fieldname + ' DESC'  end else begin"
            sSQL &= "  set @templateQuery2 =  'select @fieldValue = ' + @PKSelect  +  ' from #ins i full outer join #del d' + @PKCols + ' where i.' + @PrimaryKeyName + '=''' + @dataPK2 "
            sSQL &= "  + ''' and ('  + ' i.' + @fieldname + ' <> d.' + @fieldname + ' or (i.' + @fieldname + ' is null and  d.' + @fieldname + ' is not null)' + ' or (i.' + @fieldname "
            sSQL &= "  + ' is not null and  d.'  + @fieldname + ' is null))' exec sp_executesql @templateQuery2, N'@fieldValue varchar(max) out', @outputPKValue out "
            sSQL &= "  set @PrimaryKey = @outputPKValue set @templateQuery =  'select @fieldValue = convert(varchar(1000),i.' + @fieldname + ')' + ',@fieldNameTemp = ''' "
            sSQL &= "  + @fieldname + ''''  +  ' from #ins i full outer join #del d' + @PKCols + ' where i.' + @PrimaryKeyName + '=''' + @dataPK2 + ''' and (' + ' i.' + @fieldname + ' <> d.' "
            sSQL &= "  + @fieldname + ' or (i.' + @fieldname + ' is null and  d.' + @fieldname + ' is not null)' + ' or (i.' + @fieldname + ' is not null and  d.'  + @fieldname + ' is null))'"
            sSQL &= "   end set @outputFieldValue = '' set @outputFieldName = '' exec sp_executesql @templateQuery, N'@fieldValue varchar(max) out, @fieldNameTemp varchar(max) out', @outputFieldValue "
            sSQL &= "   out , @outputFieldName out  if @outputFieldValue is null begin set @outputFieldValue = '' end select @dataType = DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE "
            sSQL &= "  TABLE_NAME = @TableName AND  COLUMN_NAME = @outputFieldName if @outputFieldName != '' begin  if @dataType = 'float' or @dataType = 'int' or @dataType = 'money' or @dataType = 'real' "
            sSQL &= "  begin if @outputFieldValue != '' begin set @outputConcat = '""' + @outputFieldName + '"":' + @outputFieldValue + ',' end else begin set @tempFieldname "
            sSQL &= "  = (SELECT TOP 1 @fieldname from deleted) set @outputConcat = '""' + @tempFieldname + '"":0,' end end else if @dataType = 'varchar' or @dataType = 'nvarchar' "
            sSQL &= "  or @dataType = 'datetime'  begin if @outputFieldValue != '' begin set @outputConcat = '""' + @outputFieldName + '"":""' + @outputFieldValue + '"",' end else begin"
            sSQL &= "  set @tempFieldname = (SELECT TOP 1 @fieldname from deleted) set @outputConcat = '""' + @tempFieldname + '"":"""",' end end else if @dataType = 'bit'  begin if @outputFieldValue != ''"
            sSQL &= "  begin if @outputFieldValue = 0 set @outputConcat = '""' + @outputFieldName + '"":false,' else set @outputConcat = '""' + @outputFieldName + '"":true,' end else begin"
            sSQL &= "  set @tempFieldname = (SELECT TOP 1 @fieldname from deleted) set @outputConcat = '""' + @tempFieldname + '"":false,' end end set @JSONDATA += @outputConcat end	END END"
            sSQL &= "  if @outputConcat != '' begin set @headersString = '""DBAction"":""'+ @Type + '"",""ClientID"":"""",""StoreID"":"""",""TableName"":""' + @TableName + '"",""PrimaryKey"":""'+ @outputPKValue "
            sSQL &= "  +'"",""Resend"":false}' set @JSONDATA += @headersString set @insertJsonSQl = 'insert [DeliveritSQL].[dbo].[Audit] (Type,TableName,PK,Data,UpdateDate) Values (''' + @Type "
            sSQL &= "  + ''',''' + @TableName + ''',''' + @PrimaryKey + ''',''' + @JSONDATA + ''',''' + @UpdateDate + ''')' exec(@insertJsonSQL) end delete #Temp where Id = @Id2 end"
            sSQL &= "  drop table #Temp END ELSE BEGIN SELECT @Type = 'I' BEGIN SELECT  @field = 0, @maxfield = MAX(ORDINAL_POSITION) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName"
            sSQL &= "  set @JSONDATA  = '{'; WHILE @field < @maxfield BEGIN SELECT @field = MIN(ORDINAL_POSITION)FROM INFORMATION_SCHEMA.COLUMNS  WHERE TABLE_NAME = @TableName "
            sSQL &= "  AND ORDINAL_POSITION > @field SELECT @bit = (@field - 1 )% 8 + 1 SELECT @bit = POWER(2,@bit - 1) SELECT @char = ((@field - 1) / 8) + 1 "
            sSQL &= "  IF SUBSTRING(COLUMNS_UPDATED(),@char, 1) & @bit > 0OR @Type IN ('I','D') BEGIN SELECT @fieldname = COLUMN_NAME  FROM INFORMATION_SCHEMA.COLUMNS WHERE "
            sSQL &= "  TABLE_NAME = @TableName  AND ORDINAL_POSITION = @field set @templateQuery = 'select @fieldValue = convert(varchar(1000),i.' + @fieldname + ')'"
            sSQL &= "  + ',@fieldNameTemp = '''+ @fieldname + '''' +  ' from #ins i full outer join #del d' + @PKCols + ' where i.' + @fieldname + ' <> d.' + @fieldname + ' or (i.' "
            sSQL &= "  + @fieldname + ' is null and  d.' + @fieldname + ' is not null)' + ' or (i.' + @fieldname + ' is not null and  d.'  + @fieldname + ' is null)'"
            sSQL &= "  set @outputFieldValue = '' set @outputFieldName = '' exec sp_executesql @templateQuery, N'@fieldValue varchar(max) out, @fieldNameTemp varchar(max) out',  @outputFieldValue "
            sSQL &= " out , @outputFieldName out if @outputFieldValue is null begin set @outputFieldValue = '' end select @dataType = DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS"
            sSQL &= " WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @outputFieldName if @outputFieldName != ' ' begin  if @dataType = 'float' or @dataType = 'int' or @dataType = 'money' "
            sSQL &= " or @dataType = 'real'  begin if @outputFieldValue != '' begin set @outputConcat = '""' + @outputFieldName + '"":' + @outputFieldValue + ',' end else begin"
            sSQL &= "  set @tempFieldname = (SELECT @fieldname from inserted) set @outputConcat = '""' + @tempFieldname + '"":0,' end end else if @dataType = 'varchar' or "
            sSQL &= " @dataType = 'nvarchar' or @dataType = 'datetime'  begin if @outputFieldValue != '' begin set @outputConcat = '""' + @outputFieldName + '"":""' + @outputFieldValue + '"",'"
            sSQL &= "  end else begin set @tempFieldname = (SELECT @fieldname from inserted) set @outputConcat = '""' + @tempFieldname + '"":"""",' end end"
            sSQL &= " else if @dataType = 'bit' begin if @outputFieldValue != '' begin if @outputFieldValue = 0  set @outputConcat = '""' + @outputFieldName + '"":false,' else"
            sSQL &= " set @outputConcat = '""' + @outputFieldName + '"":true,' end else begin set @tempFieldname = (SELECT @fieldname from inserted) set @outputConcat = '""' "
            sSQL &= " + @tempFieldname + '"":false,' end end set @JSONDATA += @outputConcat end END END if @outputConcat != '' begin set @outputPKValue = ''"
            sSQL &= " set @templateQuery2 =  'select @fieldValue = ' + @PKSelect +  ' from #ins i full outer join #del d' + @PKCols + ' where i.' + @PrimaryKeyName + ' <> d.' "
            sSQL &= " + @PrimaryKeyName + ' or (i.' + @PrimaryKeyName + ' is null and  d.' + @PrimaryKeyName + ' is not null)' + ' or (i.' + @PrimaryKeyName + ' is not null and  d.'  "
            sSQL &= "  + @PrimaryKeyName + ' is null)' exec sp_executesql @templateQuery2, N'@fieldValue varchar(max) out', @outputPKValue out  set @headersString = '""DBAction"":""'"
            sSQL &= " + @Type + '"",""ClientID"":"""",""StoreID"":"""",""TableName"":""' + @TableName + '"",""PrimaryKey"":""'+ @outputPKValue +'"",""Resend"":false}' "
            sSQL &= " set @JSONDATA += @headersString set @insertJsonSQl = 'insert [DeliveritSQL].[dbo].[Audit] (Type,TableName,PK,Data,UpdateDate) Values (''' + @Type + ''',''' "
            sSQL &= " + @TableName + ''',''' + @outputPKValue + ''',''' + @JSONDATA + ''',''' + @UpdateDate + ''')' exec(@insertJsonSQL) end END END END ELSE BEGIN "
            sSQL &= "  SELECT @Type = 'D' SET @JSONDATA  = '{'; CREATE TABLE #TempDelete ( Id int not null identity(1,1), Data nvarchar(100) );"
            sSQL &= "  SELECT @templateQuery = 'insert #TempDelete (Data) select ' + @PKSelect +  ' from #ins i full outer join #del d' + @PKCols exec(@templateQuery)"
            sSQL &= "  Declare @Id nvarchar(100) declare @dataPK nvarchar(100) Set @dataPK = (select top 1 Data from #TempDelete) while exists (select * from #TempDelete) begin"
            sSQL &= "  select top 1 @dataPK = Data from #TempDelete select top 1 @Id = Id from #TempDelete Set @fieldname = @PrimaryKeyName if @dataPK != '' begin	"
            sSQL &= "  set @headersString = '""DBAction"":""'+ @Type + '"",""ClientID"":"""",""StoreID"":"""",""TableName"":""' + @TableName + '"",""PrimaryKey"":""'+ @dataPK +'"",""Resend"":false}'  "
            sSQL &= "  set @JSONDATA = '{' + @headersString set @insertJsonSQl =  'insert [DeliveritSQL].[dbo].[Audit] (Type,TableName,PK,Data,UpdateDate) Values (''' "
            sSQL &= "  + @Type + ''',''' + @TableName + ''',''' + @dataPK + ''',''' + @JSONDATA + ''',''' + @UpdateDate + ''')' exec(@insertJsonSQL)"
            sSQL &= " end delete #TempDelete where Id = @Id end drop table #TempDelete END"

            ' Execute the query.
            sProcess = ProcessMSSERVER(sSQL, GetConnectionStringMS(sDatabaseName))

            If sProcess <> "" Then
                bReturn = False
            End If

        Catch ex As Exception
            myMsgBox("CreateTriggerMSSQL:" & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
            bReturn = False
        End Try

        Return bReturn
    End Function

    Public Function CheckIfSPExist(ByVal SPName As String, ByVal DBName As String) As Boolean
        Dim sReturn As Boolean = False
        Dim MYCN As New MySqlClient.MySqlConnection
        Dim MYCM As New MySqlClient.MySqlCommand
        Dim MYAD As MySqlClient.MySqlDataAdapter
        Dim sSQL As String = ""
        Dim DS As New DataSet

        DS.Clear()

        Try
            sSQL = "USE sys; SHOW procedure STATUS where Name='" & SPName & "' and Db='" & DBName & "';"

            MYCN.ConnectionString = GetConnectionStringMY("")
            MYDBOpen(MYCN)
            MYAD = New MySqlClient.MySqlDataAdapter(sSQL, MYCN)
            MYAD.Fill(DS)

            If DS.Tables(0).Rows.Count = 1 Then
                sReturn = True
            End If

        Catch ex As Exception
            myMsgBox("CheckIfSPExist:" & ex.Message, "MySQL DPos Install and Data Import: ERROR", myMsgBoxDisplay.OkOnly)
        Finally
            MYCN.Close()
        End Try

        Return sReturn
    End Function
End Module


