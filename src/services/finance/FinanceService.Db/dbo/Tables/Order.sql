CREATE TABLE [dbo].[Order] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [Type]             INT              NOT NULL,
    [State]            INT              NOT NULL,
    [CustomerId]       UNIQUEIDENTIFIER NULL,
    [ServiceFee]       DECIMAL (18, 2)  NULL,
    [Discount]         DECIMAL (18, 2)  NULL,
    [CustomerComments] NVARCHAR (1000)  NULL,
    [CreatedAt]        DATETIME2 (7)    NOT NULL,
    [ClosedAt]         DATETIME2 (7)    NULL
);
GO

ALTER TABLE [dbo].[Order]
    ADD CONSTRAINT [FK_Order_Customer_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customer] ([Id]);
GO

CREATE NONCLUSTERED INDEX [IX_Order_CustomerId]
    ON [dbo].[Order]([CustomerId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Order_State]
    ON [dbo].[Order]([State] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Order_CreatedAt]
    ON [dbo].[Order]([CreatedAt] ASC);
GO

ALTER TABLE [dbo].[Order]
    ADD CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED ([Id] ASC);
GO

