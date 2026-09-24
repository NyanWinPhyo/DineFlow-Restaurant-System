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

    CONSTRAINT FK_MenuItems_MenuCategories
        FOREIGN KEY (CategoryID)
        REFERENCES MenuCategories(CategoryID),

    CONSTRAINT FK_MenuItems_Users
        FOREIGN KEY (CreatedByUserID)
        REFERENCES Users(UserID),

    CONSTRAINT UQ_MenuItems_Category_ItemName
        UNIQUE (CategoryID, ItemName)
);
GO