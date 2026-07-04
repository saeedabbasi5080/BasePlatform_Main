-- Applies OTP table + Purpose column if missing (safe to re-run).

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'EmailVerificationCodes')
BEGIN
    CREATE TABLE [EmailVerificationCodes] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Purpose] int NOT NULL CONSTRAINT [DF_EmailVerificationCodes_Purpose] DEFAULT 0,
        [CodeHash] nvarchar(500) NOT NULL,
        [ExpiresAt] datetimeoffset NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [ConsumedAt] datetimeoffset NULL,
        [AttemptCount] int NOT NULL,
        CONSTRAINT [PK_EmailVerificationCodes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmailVerificationCodes_AspNetUsers_UserId] FOREIGN KEY ([UserId])
            REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_EmailVerificationCodes_CreatedAt] ON [EmailVerificationCodes] ([CreatedAt]);
    CREATE INDEX [IX_EmailVerificationCodes_UserId_Purpose_ConsumedAt]
        ON [EmailVerificationCodes] ([UserId], [Purpose], [ConsumedAt]);
END
ELSE IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'EmailVerificationCodes') AND name = N'Purpose')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmailVerificationCodes_UserId_ConsumedAt'
               AND object_id = OBJECT_ID(N'EmailVerificationCodes'))
        DROP INDEX [IX_EmailVerificationCodes_UserId_ConsumedAt] ON [EmailVerificationCodes];

    ALTER TABLE [EmailVerificationCodes]
        ADD [Purpose] int NOT NULL CONSTRAINT [DF_EmailVerificationCodes_Purpose] DEFAULT 0;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmailVerificationCodes_UserId_Purpose_ConsumedAt'
                   AND object_id = OBJECT_ID(N'EmailVerificationCodes'))
        CREATE INDEX [IX_EmailVerificationCodes_UserId_Purpose_ConsumedAt]
            ON [EmailVerificationCodes] ([UserId], [Purpose], [ConsumedAt]);
END
GO

IF OBJECT_ID(N'__EFMigrationsHistory') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260623120000_AddEmailVerificationCode')
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20260623120000_AddEmailVerificationCode', N'10.0.9');

    IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260623160000_AddOtpPurpose')
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20260623160000_AddOtpPurpose', N'10.0.9');
END
GO

PRINT 'OTP migrations applied successfully.';
GO
