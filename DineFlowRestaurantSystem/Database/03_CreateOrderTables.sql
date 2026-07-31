USE DineFlowDB;
GO

IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE Orders (
        OrderID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
        TotalAmount DECIMAL(10,2) NOT NULL,
        OrderStatus VARCHAR(30) NOT NULL DEFAULT 'Pending',
        PaymentStatus VARCHAR(30) NOT NULL DEFAULT 'Paid',

        FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
    );
END
GO

IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE OrderItems (
        OrderItemID INT IDENTITY(1,1) PRIMARY KEY,
        OrderID INT NOT NULL,
        MenuItemID INT NOT NULL,
        ItemName VARCHAR(150) NOT NULL,
        UnitPrice DECIMAL(10,2) NOT NULL,
        Quantity INT NOT NULL,
        LineTotal DECIMAL(10,2) NOT NULL,

        FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
        FOREIGN KEY (MenuItemID) REFERENCES MenuItems(MenuItemID)
    );
END
GO

SELECT * FROM Orders;
SELECT * FROM OrderItems;