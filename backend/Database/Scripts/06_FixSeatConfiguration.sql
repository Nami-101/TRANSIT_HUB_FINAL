-- Fix seat configuration and reset all seats to available
-- This script addresses the seat count and availability issues

-- First, let's clear all existing bookings and reset seats
DELETE FROM TrainBookingPassengers;
DELETE FROM TrainBookings;
DELETE FROM TrainSeats;
DELETE FROM TrainCoaches;

-- Update TrainSchedules with correct seat counts based on IRCTC standards
UPDATE TrainSchedules 
SET TotalSeats = CASE 
    WHEN TrainClassID = 1 THEN 72  -- Sleeper Class (SL)
    WHEN TrainClassID = 2 THEN 64  -- AC 3 Tier (3A) - CORRECTED
    WHEN TrainClassID = 3 THEN 46  -- AC 2 Tier (2A) - CORRECTED
    WHEN TrainClassID = 4 THEN 18  -- AC 1st Class (1A)
    WHEN TrainClassID = 5 THEN 72  -- Chair Car (CC)
    WHEN TrainClassID = 6 THEN 72  -- Second Sitting (2S)
    WHEN TrainClassID = 7 THEN 16  -- First Class (FC)
    ELSE 72
END,
AvailableSeats = CASE 
    WHEN TrainClassID = 1 THEN 72  -- Sleeper Class (SL)
    WHEN TrainClassID = 2 THEN 64  -- AC 3 Tier (3A) - CORRECTED
    WHEN TrainClassID = 3 THEN 46  -- AC 2 Tier (2A) - CORRECTED
    WHEN TrainClassID = 4 THEN 18  -- AC 1st Class (1A)
    WHEN TrainClassID = 5 THEN 72  -- Chair Car (CC)
    WHEN TrainClassID = 6 THEN 72  -- Second Sitting (2S)
    WHEN TrainClassID = 7 THEN 16  -- First Class (FC)
    ELSE 72
END;

-- Create a stored procedure to initialize coaches with correct seat counts
IF OBJECT_ID('sp_InitializeTrainCoaches', 'P') IS NOT NULL
    DROP PROCEDURE sp_InitializeTrainCoaches;
GO

CREATE PROCEDURE sp_InitializeTrainCoaches
    @TrainId INT,
    @TravelDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if coaches already exist for this train and date
    IF NOT EXISTS (SELECT 1 FROM TrainCoaches WHERE TrainId = @TrainId AND TravelDate = @TravelDate)
    BEGIN
        -- Create coaches for different classes
        
        -- Sleeper Class Coaches (2 coaches, 72 seats each)
        INSERT INTO TrainCoaches (TrainId, TravelDate, CoachNumber, TotalSeats, AvailableSeats, QuotaAllocations, CreatedAt, CreatedBy, IsActive)
        VALUES 
        (@TrainId, @TravelDate, 1, 72, 72, '{"General":{"SeatNumbers":[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72]},"Ladies":{"SeatNumbers":[]},"Senior Citizen":{"SeatNumbers":[]},"Tatkal":{"SeatNumbers":[]}}', GETUTCDATE(), 'SystemInit', 1),
        (@TrainId, @TravelDate, 2, 72, 72, '{"General":{"SeatNumbers":[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72]},"Ladies":{"SeatNumbers":[]},"Senior Citizen":{"SeatNumbers":[]},"Tatkal":{"SeatNumbers":[]}}', GETUTCDATE(), 'SystemInit', 1);
        
        -- AC 3 Tier Coaches (2 coaches, 64 seats each) - CORRECTED
        INSERT INTO TrainCoaches (TrainId, TravelDate, CoachNumber, TotalSeats, AvailableSeats, QuotaAllocations, CreatedAt, CreatedBy, IsActive)
        VALUES 
        (@TrainId, @TravelDate, 3, 64, 64, '{"General":{"SeatNumbers":[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64]},"Ladies":{"SeatNumbers":[]},"Senior Citizen":{"SeatNumbers":[]},"Tatkal":{"SeatNumbers":[]}}', GETUTCDATE(), 'SystemInit', 1),
        (@TrainId, @TravelDate, 4, 64, 64, '{"General":{"SeatNumbers":[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64]},"Ladies":{"SeatNumbers":[]},"Senior Citizen":{"SeatNumbers":[]},"Tatkal":{"SeatNumbers":[]}}', GETUTCDATE(), 'SystemInit', 1);
        
        -- AC 2 Tier Coach (1 coach, 46 seats) - CORRECTED
        INSERT INTO TrainCoaches (TrainId, TravelDate, CoachNumber, TotalSeats, AvailableSeats, QuotaAllocations, CreatedAt, CreatedBy, IsActive)
        VALUES 
        (@TrainId, @TravelDate, 5, 46, 46, '{"General":{"SeatNumbers":[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46]},"Ladies":{"SeatNumbers":[]},"Senior Citizen":{"SeatNumbers":[]},"Tatkal":{"SeatNumbers":[]}}', GETUTCDATE(), 'SystemInit', 1);
        
        -- AC 1st Class Coach (1 coach, 18 seats - some have 15, some 16, some 18)
        INSERT INTO TrainCoaches (TrainId, TravelDate, CoachNumber, TotalSeats, AvailableSeats, QuotaAllocations, CreatedAt, CreatedBy, IsActive)
        VALUES 
        (@TrainId, @TravelDate, 6, 18, 18, '{"General":{"SeatNumbers":[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18]},"Ladies":{"SeatNumbers":[]},"Senior Citizen":{"SeatNumbers":[]},"Tatkal":{"SeatNumbers":[]}}', GETUTCDATE(), 'SystemInit', 1);
        
        -- First Class Coach (1 coach, 16 seats)
        INSERT INTO TrainCoaches (TrainId, TravelDate, CoachNumber, TotalSeats, AvailableSeats, QuotaAllocations, CreatedAt, CreatedBy, IsActive)
        VALUES 
        (@TrainId, @TravelDate, 7, 16, 16, '{"General":{"SeatNumbers":[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16]},"Ladies":{"SeatNumbers":[]},"Senior Citizen":{"SeatNumbers":[]},"Tatkal":{"SeatNumbers":[]}}', GETUTCDATE(), 'SystemInit', 1);
        
        -- Now create seats for each coach
        DECLARE @CoachId INT;
        DECLARE @SeatCount INT;
        DECLARE @SeatNum INT;
        
        DECLARE coach_cursor CURSOR FOR
        SELECT CoachId, TotalSeats FROM TrainCoaches 
        WHERE TrainId = @TrainId AND TravelDate = @TravelDate;
        
        OPEN coach_cursor;
        FETCH NEXT FROM coach_cursor INTO @CoachId, @SeatCount;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @SeatNum = 1;
            WHILE @SeatNum <= @SeatCount
            BEGIN
                INSERT INTO TrainSeats (CoachId, SeatNumber, IsOccupied, CreatedAt, CreatedBy, IsActive)
                VALUES (@CoachId, @SeatNum, 0, GETUTCDATE(), 'SystemInit', 1);
                
                SET @SeatNum = @SeatNum + 1;
            END
            
            FETCH NEXT FROM coach_cursor INTO @CoachId, @SeatCount;
        END
        
        CLOSE coach_cursor;
        DEALLOCATE coach_cursor;
    END
END;
GO

-- Initialize coaches for all existing trains for the next 7 days
DECLARE @TrainId INT;
DECLARE @Date DATE = CAST(GETDATE() AS DATE);
DECLARE @EndDate DATE = DATEADD(DAY, 7, @Date);

DECLARE train_cursor CURSOR FOR
SELECT TrainID FROM Trains WHERE IsActive = 1;

OPEN train_cursor;
FETCH NEXT FROM train_cursor INTO @TrainId;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @CurrentDate DATE = @Date;
    
    WHILE @CurrentDate <= @EndDate
    BEGIN
        EXEC sp_InitializeTrainCoaches @TrainId, @CurrentDate;
        SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
    END
    
    FETCH NEXT FROM train_cursor INTO @TrainId;
END

CLOSE train_cursor;
DEALLOCATE train_cursor;

PRINT 'Seat configuration fixed successfully!';
PRINT 'All seats are now available and configured with correct IRCTC standards:';
PRINT '- Sleeper Class: 72 seats per coach';
PRINT '- AC 3 Tier: 64 seats per coach (CORRECTED)';
PRINT '- AC 2 Tier: 46 seats per coach (CORRECTED)';
PRINT '- AC 1st Class: 18 seats per coach';
PRINT '- First Class: 16 seats per coach';