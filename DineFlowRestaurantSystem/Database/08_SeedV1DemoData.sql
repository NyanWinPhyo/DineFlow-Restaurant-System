USE DineFlowDB;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    /* =========================================================
       Demo user IDs
       ========================================================= */

    DECLARE @AdminUserID INT;

    SELECT @AdminUserID = UserID
    FROM Users
    WHERE LoginID = 'admin01';

    IF @AdminUserID IS NULL
    BEGIN
        THROW 50001, 'Demo admin account admin01 was not found.', 1;
    END;


    /* =========================================================
       Categories
       ========================================================= */

    INSERT INTO MenuCategories (CategoryName, IsActive)
    VALUES
        ('Main Course', 1),
        ('Rice & Noodles', 1),
        ('Drinks', 1),
        ('Desserts', 1);


    /* =========================================================
       Menu Items
       ========================================================= */

    DECLARE @MainCourseID INT;
    DECLARE @RiceNoodlesID INT;
    DECLARE @DrinksID INT;
    DECLARE @DessertsID INT;

    SELECT @MainCourseID = CategoryID
    FROM MenuCategories
    WHERE CategoryName = 'Main Course';

    SELECT @RiceNoodlesID = CategoryID
    FROM MenuCategories
    WHERE CategoryName = 'Rice & Noodles';

    SELECT @DrinksID = CategoryID
    FROM MenuCategories
    WHERE CategoryName = 'Drinks';

    SELECT @DessertsID = CategoryID
    FROM MenuCategories
    WHERE CategoryName = 'Desserts';


    INSERT INTO MenuItems
    (
        CategoryID,
        ItemName,
        Description,
        Price,
        IsAvailable,
        ImagePath,
        CreatedByUserID
    )
    VALUES
    (
        @MainCourseID,
        'Chicken Chop',
        'Grilled chicken served with crispy fries and black pepper sauce.',
        18.90,
        1,
        '/uploads/menu-items/chicken-chop.webp',
        @AdminUserID
    ),
    (
        @RiceNoodlesID,
        'Egg Fried Rice',
        'Wok-fried rice with egg and mixed vegetables.',
        11.90,
        1,
        '/uploads/menu-items/egg-fried-rice.webp',
        @AdminUserID
    ),
    (
        @RiceNoodlesID,
        'Spaghetti Bolognese',
        'Spaghetti served with a rich beef and tomato sauce.',
        17.90,
        1,
        '/uploads/menu-items/spaghetti-bolognese.webp',
        @AdminUserID
    ),
    (
        @DrinksID,
        'Lemon Iced Tea',
        'Refreshing iced tea with fresh lemon and light sweetness.',
        5.90,
        1,
        '/uploads/menu-items/lemon-iced-tea.webp',
        @AdminUserID
    ),
    (
        @DessertsID,
        'Chocolate Brownie',
        'Warm chocolate brownie with a soft and rich centre.',
        8.90,
        1,
        '/uploads/menu-items/chocolate-brownie.webp',
        @AdminUserID
    );


    /* =========================================================
       Ingredients
       ========================================================= */

    INSERT INTO Ingredients
    (
        IngredientName,
        Unit,
        CurrentStock,
        ReorderLevel,
        CostPerUnit,
        IsActive
    )
    VALUES
        ('Chicken Breast',       'g',   5000.00, 1000.00, 0.0250, 1),
        ('Potato',               'g',   4000.00,  800.00, 0.0050, 1),
        ('Black Pepper Sauce',   'ml',   300.00,  500.00, 0.0120, 1),
        ('Cooking Oil',          'ml',  3000.00,  500.00, 0.0060, 1),
        ('Rice',                 'g',   8000.00, 1500.00, 0.0040, 1),
        ('Egg',                  'pcs',   60.00,   12.00, 0.6000, 1),
        ('Mixed Vegetables',     'g',   3000.00,  500.00, 0.0080, 1),
        ('Spaghetti',            'g',   4000.00,  800.00, 0.0080, 1),
        ('Minced Beef',          'g',   3000.00,  700.00, 0.0300, 1),
        ('Tomato Sauce',         'ml',  3000.00,  600.00, 0.0080, 1),
        ('Tea Bag',              'pcs',   80.00,   20.00, 0.2500, 1),
        ('Lemon',                'pcs',   40.00,   10.00, 0.8000, 1),
        ('Sugar',                'g',   5000.00, 1000.00, 0.0030, 1),
        ('Brownie Mix',          'g',   3000.00,  600.00, 0.0140, 1),
        ('Butter',               'g',   2000.00,  400.00, 0.0120, 1);


    /* =========================================================
       Recipe IDs
       ========================================================= */

    DECLARE @ChickenChopID INT;
    DECLARE @EggFriedRiceID INT;
    DECLARE @SpaghettiID INT;
    DECLARE @LemonTeaID INT;
    DECLARE @BrownieID INT;

    SELECT @ChickenChopID = MenuItemID
    FROM MenuItems
    WHERE ItemName = 'Chicken Chop';

    SELECT @EggFriedRiceID = MenuItemID
    FROM MenuItems
    WHERE ItemName = 'Egg Fried Rice';

    SELECT @SpaghettiID = MenuItemID
    FROM MenuItems
    WHERE ItemName = 'Spaghetti Bolognese';

    SELECT @LemonTeaID = MenuItemID
    FROM MenuItems
    WHERE ItemName = 'Lemon Iced Tea';

    SELECT @BrownieID = MenuItemID
    FROM MenuItems
    WHERE ItemName = 'Chocolate Brownie';


    /* Chicken Chop */

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @ChickenChopID, IngredientID, 180
    FROM Ingredients
    WHERE IngredientName = 'Chicken Breast';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @ChickenChopID, IngredientID, 120
    FROM Ingredients
    WHERE IngredientName = 'Potato';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @ChickenChopID, IngredientID, 40
    FROM Ingredients
    WHERE IngredientName = 'Black Pepper Sauce';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @ChickenChopID, IngredientID, 20
    FROM Ingredients
    WHERE IngredientName = 'Cooking Oil';


    /* Egg Fried Rice */

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @EggFriedRiceID, IngredientID, 180
    FROM Ingredients
    WHERE IngredientName = 'Rice';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @EggFriedRiceID, IngredientID, 1
    FROM Ingredients
    WHERE IngredientName = 'Egg';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @EggFriedRiceID, IngredientID, 50
    FROM Ingredients
    WHERE IngredientName = 'Mixed Vegetables';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @EggFriedRiceID, IngredientID, 15
    FROM Ingredients
    WHERE IngredientName = 'Cooking Oil';


    /* Spaghetti Bolognese */

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @SpaghettiID, IngredientID, 120
    FROM Ingredients
    WHERE IngredientName = 'Spaghetti';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @SpaghettiID, IngredientID, 100
    FROM Ingredients
    WHERE IngredientName = 'Minced Beef';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @SpaghettiID, IngredientID, 100
    FROM Ingredients
    WHERE IngredientName = 'Tomato Sauce';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @SpaghettiID, IngredientID, 10
    FROM Ingredients
    WHERE IngredientName = 'Cooking Oil';


    /* Lemon Iced Tea */

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @LemonTeaID, IngredientID, 1
    FROM Ingredients
    WHERE IngredientName = 'Tea Bag';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @LemonTeaID, IngredientID, 0.50
    FROM Ingredients
    WHERE IngredientName = 'Lemon';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @LemonTeaID, IngredientID, 20
    FROM Ingredients
    WHERE IngredientName = 'Sugar';


    /* Chocolate Brownie */

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @BrownieID, IngredientID, 120
    FROM Ingredients
    WHERE IngredientName = 'Brownie Mix';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @BrownieID, IngredientID, 1
    FROM Ingredients
    WHERE IngredientName = 'Egg';

    INSERT INTO MenuItemIngredients
        (MenuItemID, IngredientID, QuantityRequired)
    SELECT @BrownieID, IngredientID, 20
    FROM Ingredients
    WHERE IngredientName = 'Butter';


    /* =========================================================
       Initial stock transaction history
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
        IngredientID,
        'Restock',
        CurrentStock,
        CostPerUnit,
        CurrentStock * CostPerUnit,
        'Initial V1 demo stock',
        'Setup',
        NULL,
        @AdminUserID
    FROM Ingredients;


    COMMIT TRANSACTION;

    PRINT 'DineFlow V1 demo data seeded successfully.';
END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO