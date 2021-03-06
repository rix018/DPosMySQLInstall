'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'This class was auto-generated by the StronglyTypedResourceBuilder
    'class via a tool like ResGen or Visual Studio.
    'To add or remove a member, edit your .ResX file then rerun ResGen
    'with the /str option, or rebuild your VS project.
    '''<summary>
    '''  A strongly-typed resource class, for looking up localized strings, etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''  Returns the cached ResourceManager instance used by this class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("DPosMYSQLInstall.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Overrides the current thread's CurrentUICulture property for all
        '''  resource lookups using this strongly typed resource class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to CREATE DEFINER=`root`@`localhost` PROCEDURE `UpdateCustomerSummary`(IN sdate CHAR(8), IN ivalue DOUBLE, IN customerId INT, IN addOrder INT)
        '''BEGIN
        '''	DECLARE numOfOrders INT;
        '''	DECLARE totValOrders DOUBLE;
        '''	DECLARE aveValOrders DOUBLE;
        '''
        '''	SELECT COUNT(*) from deliveritsql.tblCustomerSummary WHERE CustomerID=@customerId INTO @SEL_CUSTOMER;
        '''	
        '''	IF @SEL_CUSTOMER &gt; 0 
        '''	THEN
        '''			SELECT TotalNumbersOrder, TotalValueOrders FROM tblCustomerSummary WHERE CustomerID=@customerId INTO @numOfOrders, @totValOrders;
        '''	 [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property CreateSPUpdateCustomerSummaryMYSQL() As String
            Get
                Return ResourceManager.GetString("CreateSPUpdateCustomerSummaryMYSQL", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property DPos_logo() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("DPos_logo", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Icon similar to (Icon).
        '''</summary>
        Friend ReadOnly Property DPosIcon1() As System.Drawing.Icon
            Get
                Dim obj As Object = ResourceManager.GetObject("DPosIcon1", resourceCulture)
                Return CType(obj,System.Drawing.Icon)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property lock_closed() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("lock_closed", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to BEGIN TRAN T1;
        '''
        '''IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = &apos;tblPayments&apos;) 
        '''BEGIN 
        '''	CREATE TABLE tblPayments( 
        '''	RecID int identity(1,1), 
        '''	OrderID int, 
        '''	TenderTypeID int, 
        '''	AmountPaid money, 
        '''	CONSTRAINT tblPayments_PK PRIMARY KEY(RecID,OrderID)) 
        '''END;
        '''
        ''';WITH tmp(OrderID, Test, TenderTypeID, Test2, AmountPaid) AS
        '''(
        '''    SELECT
        '''        OrderID,
        '''        LEFT(CAST(TenderTypeID AS VARCHAR(MAX)), CHARINDEX(&apos;,&apos;, TenderTypeID + &apos;,&apos;) - 1),
        '''        STUFF(TenderTypeID, 1 [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property NormalisedPayments() As String
            Get
                Return ResourceManager.GetString("NormalisedPayments", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property ok() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("ok", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property password() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("password", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized resource of type System.Drawing.Bitmap.
        '''</summary>
        Friend ReadOnly Property username() As System.Drawing.Bitmap
            Get
                Dim obj As Object = ResourceManager.GetObject("username", resourceCulture)
                Return CType(obj,System.Drawing.Bitmap)
            End Get
        End Property
    End Module
End Namespace
