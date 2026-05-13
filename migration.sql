IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250217213529_InitialCreate'
)
BEGIN
    CREATE TABLE [Attractions] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Attractions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250217213529_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250217213529_InitialCreate', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    EXEC sp_rename N'[Attractions].[Id]', N'AttractionId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    ALTER TABLE [Attractions] ADD [ImageUrl] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    CREATE TABLE [TourGuides] (
        [TourGuideId] uniqueidentifier NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [PhoneNumber] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Bio] nvarchar(max) NOT NULL,
        [IsAvailable] bit NOT NULL,
        CONSTRAINT [PK_TourGuides] PRIMARY KEY ([TourGuideId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] uniqueidentifier NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [PhoneNumber] nvarchar(max) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] int NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    CREATE TABLE [Tours] (
        [TourId] uniqueidentifier NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [MaxParticipants] int NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        [IsAvailable] bit NOT NULL,
        [TourGuideId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Tours] PRIMARY KEY ([TourId]),
        CONSTRAINT [FK_Tours_TourGuides_TourGuideId] FOREIGN KEY ([TourGuideId]) REFERENCES [TourGuides] ([TourGuideId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    CREATE TABLE [Bookings] (
        [BookingId] uniqueidentifier NOT NULL,
        [TourId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [BookingDate] datetime2 NOT NULL,
        [NumberOfPeople] int NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        CONSTRAINT [PK_Bookings] PRIMARY KEY ([BookingId]),
        CONSTRAINT [FK_Bookings_Tours_TourId] FOREIGN KEY ([TourId]) REFERENCES [Tours] ([TourId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Bookings_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    CREATE TABLE [TourAttraction] (
        [TourId] uniqueidentifier NOT NULL,
        [AttractionId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_TourAttraction] PRIMARY KEY ([TourId], [AttractionId]),
        CONSTRAINT [FK_TourAttraction_Attractions_AttractionId] FOREIGN KEY ([AttractionId]) REFERENCES [Attractions] ([AttractionId]) ON DELETE CASCADE,
        CONSTRAINT [FK_TourAttraction_Tours_TourId] FOREIGN KEY ([TourId]) REFERENCES [Tours] ([TourId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    CREATE INDEX [IX_Bookings_TourId] ON [Bookings] ([TourId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    CREATE INDEX [IX_Bookings_UserId] ON [Bookings] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    CREATE INDEX [IX_TourAttraction_AttractionId] ON [TourAttraction] ([AttractionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    CREATE INDEX [IX_Tours_TourGuideId] ON [Tours] ([TourGuideId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250221082730_Added new Models to Database'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250221082730_Added new Models to Database', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250225202804_Removed Custom User Class'
)
BEGIN
    ALTER TABLE [Bookings] DROP CONSTRAINT [FK_Bookings_Users_UserId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250225202804_Removed Custom User Class'
)
BEGIN
    DROP TABLE [Users];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250225202804_Removed Custom User Class'
)
BEGIN
    DROP INDEX [IX_Bookings_UserId] ON [Bookings];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250225202804_Removed Custom User Class'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bookings]') AND [c].[name] = N'UserId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Bookings] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Bookings] DROP COLUMN [UserId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250225202804_Removed Custom User Class'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250225202804_Removed Custom User Class', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250309125257_Added other Models to the Database'
)
BEGIN
    ALTER TABLE [Bookings] ADD [PaymentId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250309125257_Added other Models to the Database'
)
BEGIN
    CREATE TABLE [Payments] (
        [PaymentId] uniqueidentifier NOT NULL,
        [BookingId] uniqueidentifier NOT NULL,
        [PaymentMethod] nvarchar(max) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentDate] datetime2 NOT NULL,
        [IsSuccessful] bit NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([PaymentId]),
        CONSTRAINT [FK_Payments_Bookings_BookingId] FOREIGN KEY ([BookingId]) REFERENCES [Bookings] ([BookingId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250309125257_Added other Models to the Database'
)
BEGIN
    CREATE TABLE [Reviews] (
        [ReviewId] uniqueidentifier NOT NULL,
        [TourId] uniqueidentifier NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [Comment] nvarchar(max) NOT NULL,
        [Rating] int NOT NULL,
        [ReviewDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Reviews] PRIMARY KEY ([ReviewId]),
        CONSTRAINT [FK_Reviews_Tours_TourId] FOREIGN KEY ([TourId]) REFERENCES [Tours] ([TourId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250309125257_Added other Models to the Database'
)
BEGIN
    CREATE INDEX [IX_Payments_BookingId] ON [Payments] ([BookingId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250309125257_Added other Models to the Database'
)
BEGIN
    CREATE INDEX [IX_Reviews_TourId] ON [Reviews] ([TourId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250309125257_Added other Models to the Database'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250309125257_Added other Models to the Database', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250313104026_Removed PaymentId from Booking'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bookings]') AND [c].[name] = N'PaymentId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Bookings] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Bookings] DROP COLUMN [PaymentId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250313104026_Removed PaymentId from Booking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250313104026_Removed PaymentId from Booking', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250402113324_ Removed Review and Attraction'
)
BEGIN
    DROP TABLE [Reviews];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250402113324_ Removed Review and Attraction'
)
BEGIN
    DROP TABLE [TourAttraction];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250402113324_ Removed Review and Attraction'
)
BEGIN
    DROP TABLE [Attractions];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250402113324_ Removed Review and Attraction'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250402113324_ Removed Review and Attraction', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250504103505_AddStatusUpdatedAtToBooking'
)
BEGIN
    ALTER TABLE [Bookings] ADD [StatusUpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250504103505_AddStatusUpdatedAtToBooking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250504103505_AddStatusUpdatedAtToBooking', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510120000_AddPaymentStatusLifecycle'
)
BEGIN
    ALTER TABLE [Payments] ADD [ProviderReference] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510120000_AddPaymentStatusLifecycle'
)
BEGIN
    ALTER TABLE [Payments] ADD [Status] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510120000_AddPaymentStatusLifecycle'
)
BEGIN
    UPDATE Payments SET Status = CASE WHEN IsSuccessful = 1 THEN 2 ELSE 0 END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510120000_AddPaymentStatusLifecycle'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'IsSuccessful');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Payments] DROP COLUMN [IsSuccessful];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510120000_AddPaymentStatusLifecycle'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260510120000_AddPaymentStatusLifecycle', N'9.0.3');
END;

COMMIT;
GO

