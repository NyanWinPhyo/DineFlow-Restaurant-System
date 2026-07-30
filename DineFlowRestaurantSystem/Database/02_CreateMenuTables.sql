USE DineFlowDB;
GO

CREATE TABLE MenuCategories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL UNIQUE,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE MenuItems (
    MenuItemID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryID INT NOT NULL,
    ItemName VARCHAR(150) NOT NULL,
    Description VARCHAR(500) NULL,
    Price DECIMAL(10,2) NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    ImagePath VARCHAR(255) NULL,
    CreatedByUserID INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    FOREIGN KEY (CategoryID) REFERENCES MenuCategories(CategoryID),
    FOREIGN KEY (CreatedByUserID) REFERENCES Users(UserID)
);
GO

INSERT INTO MenuCategories (CategoryName)
VALUES 
('Main Course'),
('Drinks'),
('Desserts'),
('Side Dishes');
GO

IF NOT EXISTS (
    SELECT 1 
    FROM MenuItems 
    WHERE CategoryID = 1 
      AND ItemName = 'Chicken Chop'
)
BEGIN
    INSERT INTO MenuItems 
    (CategoryID, ItemName, Description, Price, IsAvailable, CreatedByUserID)
    VALUES
(1, 'Chicken Chop', 'Grilled chicken served with fries and black pepper sauce.', 18.90, 1, 1);
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM MenuItems 
    WHERE CategoryID = 1 
      AND ItemName = 'Chicken Chop'
)
BEGIN
    INSERT INTO MenuItems 
    (CategoryID, ItemName, Description, Price, IsAvailable, CreatedByUserID)
    VALUES
(1, 'Fried Rice', 'Classic fried rice with egg and vegetables.', 9.90, 1, 1);
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM MenuItems 
    WHERE CategoryID = 1 
      AND ItemName = 'Chicken Chop'
)
BEGIN
    INSERT INTO MenuItems 
    (CategoryID, ItemName, Description, Price, IsAvailable, CreatedByUserID)
    VALUES
(2, 'Iced Lemon Tea', 'Cold lemon tea drink.', 4.50, 1, 1);
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM MenuItems 
    WHERE CategoryID = 1 
      AND ItemName = 'Chicken Chop'
)
BEGIN
    INSERT INTO MenuItems 
    (CategoryID, ItemName, Description, Price, IsAvailable, CreatedByUserID)
    VALUES
(3, 'Chocolate Cake', 'Slice of chocolate cake.', 7.90, 1, 1);
END
GO

SELECT * FROM MenuCategories;
SELECT * FROM MenuItems;