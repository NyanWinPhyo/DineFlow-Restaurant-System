USE DineFlowDB;
GO

IF OBJECT_ID('dbo.Feedback', 'U') IS NULL
BEGIN
    CREATE TABLE Feedback (
        FeedbackID INT IDENTITY(1,1) PRIMARY KEY,
        OrderID INT NOT NULL,
        CustomerID INT NOT NULL,
        Rating INT NOT NULL,
        Comment VARCHAR(1000) NULL,
        AdminResponse VARCHAR(1000) NULL,
        IsReviewed BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        RespondedAt DATETIME NULL,
        RespondedByUserID INT NULL,

        CONSTRAINT FK_Feedback_Orders
            FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),

        CONSTRAINT FK_Feedback_Customers
            FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),

        CONSTRAINT FK_Feedback_RespondedBy
            FOREIGN KEY (RespondedByUserID) REFERENCES Users(UserID),

        CONSTRAINT UQ_Feedback_Order
            UNIQUE (OrderID),

        CONSTRAINT CK_Feedback_Rating
            CHECK (Rating BETWEEN 1 AND 5)
    );
END
GO

SELECT * FROM Feedback;