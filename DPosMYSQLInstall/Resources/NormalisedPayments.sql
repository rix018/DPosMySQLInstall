BEGIN TRAN T1;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tblPayments') 
BEGIN 
	CREATE TABLE tblPayments( 
	RecID int identity(1,1), 
	OrderID int, 
	TenderTypeID int, 
	AmountPaid money, 
	CONSTRAINT tblPayments_PK PRIMARY KEY(RecID,OrderID)) 
END;

;WITH tmp(OrderID, Test, TenderTypeID, Test2, AmountPaid) AS
(
    SELECT
        OrderID,
        LEFT(CAST(TenderTypeID AS VARCHAR(MAX)), CHARINDEX(',', TenderTypeID + ',') - 1),
        STUFF(TenderTypeID, 1, CHARINDEX(',', TenderTypeID + ','), ''),
        LEFT(CAST(AmountPaid AS VARCHAR(MAX)), CHARINDEX(',', AmountPaid + ',') - 1),
        STUFF(AmountPaid, 1, CHARINDEX(',', AmountPaid + ','), '')
    FROM tblOrderHeaders
    UNION all

    SELECT
        OrderID,
        LEFT(CAST(TenderTypeID AS VARCHAR(MAX)), CHARINDEX(',', TenderTypeID + ',') - 1),
        STUFF(TenderTypeID, 1, CHARINDEX(',', TenderTypeID + ','), ''),
        LEFT(CAST(AmountPaid AS VARCHAR(MAX)), CHARINDEX(',', AmountPaid + ',') - 1),
        STUFF(AmountPaid, 1, CHARINDEX(',', AmountPaid + ','), '')
    FROM tmp
    WHERE
        TenderTypeID > ''
)

INSERT INTO tblPayments
	(OrderID, TenderTypeID, AmountPaid)
SELECT
	OrderID,
	Test,
	Test2
FROM tmp
WHERE Test <> ''
ORDER BY OrderID;

COMMIT TRAN T1;