CREATE DEFINER=`root`@`localhost` PROCEDURE `UpdateCustomerSummary`(IN sdate CHAR(8), IN ivalue DOUBLE, IN icustomerId INT, IN addOrder INT)
BEGIN
	DECLARE numOfOrders INT;
	DECLARE totValOrders DOUBLE;
	DECLARE aveValOrders DOUBLE;

	SELECT COUNT(*) from deliveritsql.tblCustomerSummary WHERE CustomerID=icustomerId INTO @SEL_CUSTOMER;
	
	IF @SEL_CUSTOMER > 0 
	THEN
			SELECT TotalNumberOrders, TotalValueOrders FROM deliveritsql.tblCustomerSummary WHERE CustomerID=icustomerId INTO @numOfOrders, @totValOrders;
            
            SET @numOfOrders = @numOfOrders + addOrder;
			
			IF @numOfOrders < 1
			THEN
				DELETE FROM deliveritsql.tblcustomerSummary WHERE CustomerID=icustomerId;
			ELSE
				SET @totValOrders = @totValOrders + ivalue;
				SET @aveValOrders = @totValOrders/@numOfOrders;
				
				UPDATE deliveritsql.tblCustomerSummary
				SET TotalNumberOrders=@numOfOrders, TotalValueOrders=@totValOrders, AverageOrderValue=@aveValOrders, LastOrderDate=sdate
				WHERE CustomerID=icustomerId;
			END IF;
	ELSE
		INSERT INTO deliveritsql.tblCustomerSummary (CustomerID,FirstOrderDate,LastOrderDate,ModifiedDate,TotalNumberOrders,TotalValueOrders,AverageOrderValue)
		VALUES (icustomerId,sdate,sdate,sdate,1,ivalue,ivalue);
	END IF;
	
END