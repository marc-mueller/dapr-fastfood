CREATE TABLE [dbo].[Customer] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [FirstName]       NVARCHAR (100)   NULL,
    [LastName]        NVARCHAR (100)   NULL,
    [DeliveryAddress] NVARCHAR (MAX)   NULL,
    [InvoiceAddress]  NVARCHAR (MAX)   NULL
);
GO

ALTER TABLE [dbo].[Customer]
    ADD CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED ([Id] ASC);
GO

