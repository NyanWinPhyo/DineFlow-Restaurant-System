USE DineFlowDB;
GO

/* =========================================================
   Ingredients Master Table
   ========================================================= */

IF OBJECT_ID('dbo.Ingredients', 'U') IS NULL
BEGIN
    CREATE TABLE Ingredients (
        IngredientID INT IDENTITY(1,1) PRIMARY KEY,
        IngredientName VARCHAR(150) NOT NULL UNIQUE,
        Unit VARCHAR(30) NOT NULL,
        CurrentStock DECIMAL(10,2) NOT NULL DEFAULT 0,
        ReorderLevel DECIMAL(10,2) NOT NULL DEFAULT 0,
        CostPerUnit DECIMAL(10,4) NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL,

        CONSTRAINT CK_Ingredients_CurrentStock
            CHECK (CurrentStock >= 0),

        CONSTRAINT CK_Ingredients_ReorderLevel
            CHECK (ReorderLevel >= 0),

        CONSTRAINT CK_Ingredients_CostPerUnit
            CHECK (CostPerUnit >= 0)
    );
END
GO


/* =========================================================
   Menu Item Recipe / Ingredient Requirements
   ========================================================= */

IF OBJECT_ID('dbo.MenuItemIngredients', 'U') IS NULL
BEGIN
    CREATE TABLE MenuItemIngredients (
        MenuItemIngredientID INT IDENTITY(1,1) PRIMARY KEY,
        MenuItemID INT NOT NULL,
        IngredientID INT NOT NULL,
        QuantityRequired DECIMAL(10,2) NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_MenuItemIngredients_MenuItems
            FOREIGN KEY (MenuItemID) REFERENCES MenuItems(MenuItemID),

        CONSTRAINT FK_MenuItemIngredients_Ingredients
            FOREIGN KEY (IngredientID) REFERENCES Ingredients(IngredientID),

        CONSTRAINT UQ_MenuItemIngredients_MenuItem_Ingredient
            UNIQUE (MenuItemID, IngredientID),

        CONSTRAINT CK_MenuItemIngredients_QuantityRequired
            CHECK (QuantityRequired > 0)
    );
END
GO


/* =========================================================
   Stock Transaction History
   ========================================================= */

IF OBJECT_ID('dbo.StockTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE StockTransactions (
        StockTransactionID INT IDENTITY(1,1) PRIMARY KEY,
        IngredientID INT NOT NULL,
        TransactionType VARCHAR(30) NOT NULL,
        QuantityChange DECIMAL(10,2) NOT NULL,
        UnitCost DECIMAL(10,4) NULL,
        TotalCost DECIMAL(10,2) NULL,
        Reason VARCHAR(500) NULL,
        ReferenceType VARCHAR(50) NULL,
        ReferenceID INT NULL,
        CreatedByUserID INT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_StockTransactions_Ingredients
            FOREIGN KEY (IngredientID) REFERENCES Ingredients(IngredientID),

        CONSTRAINT FK_StockTransactions_Users
            FOREIGN KEY (CreatedByUserID) REFERENCES Users(UserID),

        CONSTRAINT CK_StockTransactions_TransactionType
            CHECK (TransactionType IN ('Restock', 'Usage', 'Adjustment', 'Waste'))
    );
END
GO