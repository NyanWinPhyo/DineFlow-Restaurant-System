USE DineFlowDB;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    /* =========================================================
       1. Clear transactional / activity data
       Keep:
       - Users
       - Customers
       - Menu Categories
       - Menu Items
       - Ingredients
       - Recipes
       ========================================================= */

    IF OBJECT_ID('dbo.Feedback', 'U') IS NOT NULL
    BEGIN
        DELETE FROM Feedback;
    END

    IF OBJECT_ID('dbo.OrderItems', 'U') IS NOT NULL
    BEGIN
        DELETE FROM OrderItems;
    END

    IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL
    BEGIN
        DELETE FROM Orders;
    END

    IF OBJECT_ID('dbo.StockTransactions', 'U') IS NOT NULL
    BEGIN
        DELETE FROM StockTransactions;
    END


    /* =========================================================
       2. Reset identities
       ========================================================= */

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

    IF OBJECT_ID('dbo.StockTransactions', 'U') IS NOT NULL
    BEGIN
        DBCC CHECKIDENT ('StockTransactions', RESEED, 0);
    END


    /* =========================================================
       3. Reset customer wallets
       ========================================================= */

    UPDATE Customers
    SET WalletBalance = 100.00;


    /* =========================================================
       4. V1 Ingredient Baseline
       ========================================================= */

    DECLARE @IngredientBaseline TABLE
    (
        IngredientName VARCHAR(150) PRIMARY KEY,
        Unit VARCHAR(30),
        CurrentStock DECIMAL(10,2),
        ReorderLevel DECIMAL(10,2),
        CostPerUnit DECIMAL(10,4)
    );

    INSERT INTO @IngredientBaseline
    (
        IngredientName,
        Unit,
        CurrentStock,
        ReorderLevel,
        CostPerUnit
    )
    VALUES
    ('Chicken Breast',       'g',   5000.00, 1000.00, 0.0250),
    ('Potato',               'g',   4000.00,  800.00, 0.0050),
    ('Black Pepper Sauce',   'ml',   300.00,  500.00, 0.0120),
    ('Cooking Oil',          'ml',  3000.00,  500.00, 0.0060),
    ('Rice',                 'g',   8000.00, 1500.00, 0.0040),
    ('Egg',                  'pcs',   60.00,   12.00, 0.6000),
    ('Mixed Vegetables',     'g',   3000.00,  500.00, 0.0080),
    ('Spaghetti',            'g',   4000.00,  800.00, 0.0080),
    ('Minced Beef',          'g',   3000.00,  700.00, 0.0300),
    ('Tomato Sauce',         'ml',  3000.00,  600.00, 0.0080),
    ('Tea Bag',              'pcs',   80.00,   20.00, 0.2500),
    ('Lemon',                'pcs',   40.00,   10.00, 0.8000),
    ('Sugar',                'g',   5000.00, 1000.00, 0.0030),
    ('Brownie Mix',          'g',   3000.00,  600.00, 0.0140),
    ('Butter',               'g',   2000.00,  400.00, 0.0120);


    /* =========================================================
       5. Safety check
       Ensure all V1 ingredients exist before changing stock
       ========================================================= */

    IF EXISTS
    (
        SELECT 1
        FROM @IngredientBaseline b
        LEFT JOIN Ingredients i
            ON i.IngredientName = b.IngredientName
        WHERE i.IngredientID IS NULL
    )
    BEGIN
        THROW 50001,
            'One or more V1 demo ingredients are missing. Add them before running this reset.',
            1;
    END


    /* =========================================================
       6. Reset ingredient master values
       ========================================================= */

    UPDATE i
    SET
        i.Unit = b.Unit,
        i.CurrentStock = b.CurrentStock,
        i.ReorderLevel = b.ReorderLevel,
        i.CostPerUnit = b.CostPerUnit,
        i.IsActive = 1,
        i.UpdatedAt = GETDATE()
    FROM Ingredients i
    INNER JOIN @IngredientBaseline b
        ON i.IngredientName = b.IngredientName;


    /* =========================================================
       7. Restore V1 categories/menu availability
       Does NOT touch photos, descriptions, prices or recipes
       ========================================================= */

    UPDATE MenuCategories
    SET IsActive = 1
    WHERE CategoryName IN
    (
        'Main Course',
        'Rice & Noodles',
        'Drinks',
        'Desserts'
    );

    UPDATE MenuItems
    SET IsAvailable = 1
    WHERE ItemName IN
    (
        'Chicken Chop',
        'Egg Fried Rice',
        'Spaghetti Bolognese',
        'Lemon Iced Tea',
        'Chocolate Brownie'
    );


    /* =========================================================
       8. Find demo admin for initial transactions
       ========================================================= */

    DECLARE @AdminUserID INT;

    SELECT @AdminUserID = UserID
    FROM Users
    WHERE LoginID = 'admin01';

    IF @AdminUserID IS NULL
    BEGIN
        THROW 50002,
            'Demo admin account admin01 could not be found.',
            1;
    END


    /* =========================================================
       9. Re-create clean opening stock history
       ========================================================= */

    INSERT INTO StockTransactions
    (
        IngredientID,
        TransactionType,
        QuantityChange,
        UnitCost,
        TotalCost,
        Reason,
        ReferenceType,
        ReferenceID,
        CreatedByUserID
    )
    SELECT
        i.IngredientID,
        'Restock',
        b.CurrentStock,
        b.CostPerUnit,
        b.CurrentStock * b.CostPerUnit,
        'Initial V1 demo stock',
        'Setup',
        NULL,
        @AdminUserID
    FROM @IngredientBaseline b
    INNER JOIN Ingredients i
        ON i.IngredientName = b.IngredientName;


    COMMIT TRANSACTION;

    PRINT 'DineFlow V1 demo activity reset completed successfully.';
END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO