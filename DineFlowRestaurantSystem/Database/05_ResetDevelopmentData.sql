USE DineFlowDB;
GO

BEGIN TRANSACTION;

-- Delete review/feedback data first because it depends on Orders and Customers
IF OBJECT_ID('dbo.Feedback', 'U') IS NOT NULL
BEGIN
    DELETE FROM Feedback;
END

-- Delete order item data before orders
IF OBJECT_ID('dbo.OrderItems', 'U') IS NOT NULL
BEGIN
    DELETE FROM OrderItems;
END

-- Delete order history
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL
BEGIN
    DELETE FROM Orders;
END

-- Delete menu items before menu categories
IF OBJECT_ID('dbo.MenuItems', 'U') IS NOT NULL
BEGIN
    DELETE FROM MenuItems;
END

-- Delete menu categories
IF OBJECT_ID('dbo.MenuCategories', 'U') IS NOT NULL
BEGIN
    DELETE FROM MenuCategories;
END

-- Reset customer wallet for testing
IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL
BEGIN
    UPDATE Customers
    SET WalletBalance = 100.00;
END

-- Reset identity values
IF OBJECT_ID('dbo.Feedback', 'U') IS NOT NULL
BEGIN
    DBCC CHECKIDENT ('Feedback', RESEED, 0);
END

IF OBJECT_ID('dbo.OrderItems', 'U') IS NOT NULL
BEGIN
    DBCC CHECKIDENT ('OrderItems', RESEED, 0);
END

IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL
BEGIN
    DBCC CHECKIDENT ('Orders', RESEED, 0);
END

IF OBJECT_ID('dbo.MenuItems', 'U') IS NOT NULL
BEGIN
    DBCC CHECKIDENT ('MenuItems', RESEED, 0);
END

IF OBJECT_ID('dbo.MenuCategories', 'U') IS NOT NULL
BEGIN
    DBCC CHECKIDENT ('MenuCategories', RESEED, 0);
END

COMMIT TRANSACTION;
GO