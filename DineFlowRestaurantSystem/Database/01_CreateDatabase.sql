CREATE DATABASE DineFlowDB;
GO

USE DineFlowDB;
GO

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    LoginID VARCHAR(50) NOT NULL UNIQUE,
    Username VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(30) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY,
    WalletBalance DECIMAL(10,2) NOT NULL DEFAULT 0,
    FOREIGN KEY (CustomerID) REFERENCES Users(UserID)
);
GO

INSERT INTO Users (LoginID, Username, PasswordHash, Role)
VALUES
('admin01', 'System Admin', 'admin123', 'Admin'),
('manager01', 'Restaurant Manager', 'manager123', 'Manager'),
('chef01', 'Main Chef', 'chef123', 'Chef'),
('cust01', 'Alice Tan', 'cust123', 'Customer');
GO

INSERT INTO Customers (CustomerID, WalletBalance)
VALUES
(4, 100.00);
GO