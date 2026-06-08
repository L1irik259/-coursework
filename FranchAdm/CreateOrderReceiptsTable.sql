-- Запустить в SQL Server Management Studio на базе FranchiseDB
-- Создание таблицы для хранения PDF-чеков заявок

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'OrderReceipts'
)
BEGIN
    CREATE TABLE [dbo].[OrderReceipts] (
        [ReceiptId]         INT IDENTITY(1,1) NOT NULL,
        [OrderId]           INT              NOT NULL,
        [FileName]          NVARCHAR(255)    NOT NULL,
        [FileData]          VARBINARY(MAX)   NOT NULL,
        [GeneratedAt]       DATETIME         NOT NULL CONSTRAINT [DF_OrderReceipts_GeneratedAt] DEFAULT GETDATE(),
        [GeneratedByUserId] INT              NOT NULL,

        CONSTRAINT [PK_OrderReceipts]       PRIMARY KEY CLUSTERED ([ReceiptId] ASC),
        CONSTRAINT [FK_OrderReceipts_Orders] FOREIGN KEY ([OrderId])
            REFERENCES [dbo].[Orders] ([OrderId]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderReceipts_Users]  FOREIGN KEY ([GeneratedByUserId])
            REFERENCES [dbo].[Users] ([UserId])
    );

    PRINT 'Таблица OrderReceipts успешно создана.';
END
ELSE
BEGIN
    PRINT 'Таблица OrderReceipts уже существует.';
END
