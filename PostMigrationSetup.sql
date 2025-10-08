-- =============================================
-- Transit-Hub Post-Migration Setup Script
-- =============================================
-- Run this script AFTER running 'dotnet ef database update'
-- This includes all manual database fixes and enhancements
-- Safe to run multiple times (includes existence checks)
-- =============================================

USE TransitHubDB;
GO

PRINT '===================================================='
PRINT 'Starting Transit-Hub Post-Migration Setup...'
PRINT '===================================================='

-- =============================================
-- Step 1: Add TrainClass column to WaitlistQueue
-- =============================================
PRINT 'Step 1: Adding TrainClass support to WaitlistQueue...'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WaitlistQueue]') AND name = 'TrainClass')
BEGIN
    ALTER TABLE WaitlistQueue ADD TrainClass NVARCHAR(10) NULL;
    PRINT '✅ TrainClass column added to WaitlistQueue'
END
ELSE
BEGIN
    PRINT '✅ TrainClass column already exists in WaitlistQueue'
END

-- =============================================
-- Step 2: Add Waitlist Priority Columns
-- =============================================
PRINT 'Step 2: Adding waitlist priority columns...'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WaitlistQueue]') AND name = 'Position')
BEGIN
    ALTER TABLE [WaitlistQueue] ADD [Position] int NOT NULL DEFAULT 1;
    PRINT '✅ Position column added to WaitlistQueue'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WaitlistQueue]') AND name = 'QueuedAt')
BEGIN
    ALTER TABLE [WaitlistQueue] ADD [QueuedAt] datetime2 NOT NULL DEFAULT GETUTCDATE();
    PRINT '✅ QueuedAt column added to WaitlistQueue'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WaitlistQueue]') AND name = 'ConfirmedAt')
BEGIN
    ALTER TABLE [WaitlistQueue] ADD [ConfirmedAt] datetime2 NULL;
    PRINT '✅ ConfirmedAt column added to WaitlistQueue'
END

-- =============================================
-- Step 3: Backfill TrainClass for existing records
-- =============================================
PRINT 'Step 3: Backfilling TrainClass data...'

UPDATE wq
SET TrainClass = tc.ClassName
FROM WaitlistQueue wq
INNER JOIN Bookings b ON wq.BookingID = b.BookingID
INNER JOIN TrainSchedules ts ON b.TrainScheduleID = ts.ScheduleID
INNER JOIN TrainClasses tc ON ts.TrainClassID = tc.TrainClassID
WHERE wq.TrainClass IS NULL
AND wq.IsActive = 1;

PRINT '✅ TrainClass backfill completed'

-- =============================================
-- Step 4: Clean up WaitlistQueue table
-- =============================================
PRINT 'Step 4: Cleaning up WaitlistQueue table...'

-- Drop foreign key constraints if they exist
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_WaitlistQueue_FlightSchedules_FlightScheduleID')
BEGIN
    ALTER TABLE [WaitlistQueue] DROP CONSTRAINT [FK_WaitlistQueue_FlightSchedules_FlightScheduleID];
    PRINT '✅ Dropped FlightSchedules foreign key'
END

-- Drop indexes if they exist
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WaitlistQueue_FlightScheduleID')
BEGIN
    DROP INDEX [IX_WaitlistQueue_FlightScheduleID] ON [WaitlistQueue];
    PRINT '✅ Dropped FlightScheduleID index'
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WaitlistQueue_QueuePosition')
BEGIN
    DROP INDEX [IX_WaitlistQueue_QueuePosition] ON [WaitlistQueue];
    PRINT '✅ Dropped QueuePosition index'
END

-- Drop unused columns
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WaitlistQueue]') AND name = 'FlightScheduleID')
BEGIN
    ALTER TABLE [WaitlistQueue] DROP COLUMN [FlightScheduleID];
    PRINT '✅ Dropped FlightScheduleID column'
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WaitlistQueue]') AND name = 'ScheduleType')
BEGIN
    ALTER TABLE [WaitlistQueue] DROP COLUMN [ScheduleType];
    PRINT '✅ Dropped ScheduleType column'
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WaitlistQueue]') AND name = 'QueuePosition')
BEGIN
    ALTER TABLE [WaitlistQueue] DROP COLUMN [QueuePosition];
    PRINT '✅ Dropped QueuePosition column'
END

-- =============================================
-- Step 5: Create/Update Stored Procedures
-- =============================================
PRINT 'Step 5: Creating stored procedures...'

-- Waitlist position procedure with TrainClass support
CREATE OR ALTER PROCEDURE sp_GetNextWaitlistPosition
    @TrainScheduleID INT,
    @TrainClass NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @WaitlistCount INT;
        
        -- Get the total count of active waitlist entries for specific class
        SELECT @WaitlistCount = COUNT(*)
        FROM WaitlistQueue 
        WHERE TrainScheduleID = @TrainScheduleID 
        AND TrainClass = @TrainClass
        AND IsActive = 1;
        
        SELECT @WaitlistCount as NextPosition;
        
    END TRY
    BEGIN CATCH
        SELECT 0 as NextPosition;
    END CATCH
END;
GO

PRINT '✅ sp_GetNextWaitlistPosition created/updated'

-- User registration procedure with enhanced fields
CREATE OR ALTER PROCEDURE sp_RegisterUser
    @Name NVARCHAR(100),
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(500),
    @Phone NVARCHAR(15),
    @Age INT,
    @DateOfBirth DATETIME2 = NULL,
    @Gender NVARCHAR(10) = '',
    @CreatedBy NVARCHAR(100) = 'System'
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Check if email already exists
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND IsActive = 1)
        BEGIN
            THROW 50001, 'Email already registered', 1;
        END
        
        -- Check if phone already exists
        IF EXISTS (SELECT 1 FROM Users WHERE Phone = @Phone AND IsActive = 1)
        BEGIN
            THROW 50002, 'Phone number already registered', 1;
        END
        
        -- Validate age
        IF @Age < 0 OR @Age > 120
        BEGIN
            THROW 50003, 'Invalid age. Age must be between 0 and 120', 1;
        END
        
        -- Determine senior citizen status (58+ for booking priority)
        DECLARE @IsSeniorCitizen BIT = CASE WHEN @Age >= 58 THEN 1 ELSE 0 END;
        
        -- Insert user
        DECLARE @UserID INT;
        INSERT INTO Users (Name, Email, PasswordHash, Phone, Age, DateOfBirth, Gender, IsSeniorCitizen, IsVerified, CreatedAt, CreatedBy, IsActive)
        VALUES (@Name, @Email, @PasswordHash, @Phone, @Age, @DateOfBirth, @Gender, @IsSeniorCitizen, 0, GETUTCDATE(), @CreatedBy, 1);
        
        SET @UserID = SCOPE_IDENTITY();
        
        -- Generate verification token
        DECLARE @Token NVARCHAR(500) = NEWID();
        DECLARE @ExpiryDate DATETIME2 = DATEADD(HOUR, 24, GETUTCDATE());
        
        INSERT INTO GmailVerificationTokens (UserID, Token, ExpiryDate, IsUsed, CreatedAt)
        VALUES (@UserID, @Token, @ExpiryDate, 0, GETUTCDATE());
        
        COMMIT TRANSACTION;
        
        -- Return success with user details
        SELECT 
            @UserID as UserID,
            @Token as VerificationToken,
            'User registered successfully' as Message,
            1 as Success;
            
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Log error if SystemLogs table exists
        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemLogs')
        BEGIN
            INSERT INTO SystemLogs (LogLevel, Message, StackTrace, CreatedAt)
            VALUES ('Error', ERROR_MESSAGE(), ERROR_PROCEDURE() + ' - Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10)), GETUTCDATE());
        END
        
        THROW;
    END CATCH
END;
GO

PRINT '✅ sp_RegisterUser created/updated'

-- =============================================
-- Step 6: Data Validation and Cleanup
-- =============================================
PRINT 'Step 6: Validating data integrity...'

-- Ensure TrainScheduleID has no NULL values in WaitlistQueue
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WaitlistQueue]') AND name = 'TrainScheduleID')
BEGIN
    UPDATE [WaitlistQueue] SET [TrainScheduleID] = 1 WHERE [TrainScheduleID] IS NULL;
    PRINT '✅ Fixed NULL TrainScheduleID values'
END

-- Update Priority column to use correct system (1 = Senior, 2 = Regular)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[WaitlistQueue]') AND name = 'Priority')
BEGIN
    UPDATE [WaitlistQueue] SET [Priority] = 2 WHERE [Priority] != 1 AND [Priority] != 2;
    PRINT '✅ Fixed Priority values'
END

-- =============================================
-- Step 7: Verification
-- =============================================
PRINT 'Step 7: Verifying setup...'

-- Check WaitlistQueue structure
SELECT 'WaitlistQueue Columns:' as Info;
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'WaitlistQueue'
ORDER BY ORDINAL_POSITION;

-- Check stored procedures
SELECT 'Stored Procedures Created:' as Info, COUNT(*) as Count 
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_TYPE = 'PROCEDURE' AND ROUTINE_NAME LIKE 'sp_%';

-- Check TrainClass data
SELECT 'TrainClass Distribution:' as Info;
SELECT TrainClass, COUNT(*) as Count
FROM WaitlistQueue 
WHERE TrainClass IS NOT NULL
GROUP BY TrainClass;

PRINT '===================================================='
PRINT '✅ Transit-Hub Post-Migration Setup Completed!'
PRINT '===================================================='
PRINT ''
PRINT 'Summary of changes applied:'
PRINT '• Added TrainClass column to WaitlistQueue'
PRINT '• Added waitlist priority columns (Position, QueuedAt, ConfirmedAt)'
PRINT '• Backfilled TrainClass data for existing records'
PRINT '• Cleaned up unused WaitlistQueue columns'
PRINT '• Created/updated stored procedures'
PRINT '• Validated data integrity'
PRINT ''
PRINT 'Your database is now ready for the Transit-Hub application!'
GO