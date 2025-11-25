CREATE TABLE [dbo].[OrderItem] (
    [Id]                 UNIQUEIDENTIFIER NOT NULL,
    [ProductId]          UNIQUEIDENTIFIER NOT NULL,
    [Quantity]           INT              NOT NULL,
    [ItemPrice]          DECIMAL (18, 2)  NOT NULL,
    [ProductDescription] NVARCHAR (500)   NULL,
    [CustomerComments]   NVARCHAR (500)   NULL,
    [State]              INT              NOT NULL,
    [OrderId]            UNIQUEIDENTIFIER NOT NULL
);
GO

ALTER TABLE [dbo].[OrderItem]
    ADD CONSTRAINT [PK_OrderItem] PRIMARY KEY CLUSTERED ([Id] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderItem_OrderId]
    ON [dbo].[OrderItem]([OrderId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_OrderItem_State]
    ON [dbo].[OrderItem]([State] ASC);
GO

ALTER TABLE [dbo].[OrderItem]
    ADD CONSTRAINT [FK_OrderItem_Order_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Order] ([Id]) ON DELETE CASCADE;
GO

