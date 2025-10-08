-- Transit-Hub Stored Procedures - Waitlist Operations
-- Created: 2025-09-09

USE TransitHubDB;
GO

-- =============================================
-- Promote Waitlist (Auto-allocation on cancellation)
-- =============================================
CREATE OR ALTER PROCEDURE sp_PromoteWaitlist
    @TrainScheduleID INT = NULL,
    @FlightScheduleID INT = NULL,
    @AvailableSeats INT,
    @ProcessedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validate inputs
        IF @TrainScheduleID IS NULL AND @FlightScheduleID IS NULL
        BEGIN
            THROW 50032, 'Either TrainScheduleID or FlightScheduleID must be provided', 1;
        END
        
        IF @TrainScheduleID IS NOT NULL AND @FlightScheduleID IS NOT NULL
        BEGIN
            THROW 50033, 'Cannot process both train and flight schedules simultaneously', 1;
        END
        
        DECLARE @ScheduleType NVARCHAR(10) = CASE WHEN @TrainScheduleID IS NOT NULL THEN 'Train' ELSE 'Flight' END;
        DECLARE @ScheduleID INT = COALESCE(@TrainScheduleID, @FlightScheduleID);
        
        -- Get waitlisted bookings ordered by priority and queue position
        DECLARE @WaitlistCursor CURSOR;
        SET @WaitlistCursor = CURSOR FOR
        SELECT 
            wq.BookingID,
            wq.WaitlistID,
            b.TotalPassengers,
            b.UserID,
            b.BookingReference
        FROM WaitlistQueue wq
        INNER JOIN Bookings b ON wq.BookingID = b.BookingID
        WHERE 
            ((@ScheduleType = 'Train' AND wq.TrainScheduleID = @TrainScheduleID) OR
             (@ScheduleType = 'Flight' AND wq.FlightScheduleID = @FlightScheduleID))
            AND wq.IsActive = 1
            AND b.IsActive = 1
        ORDER BY wq.Priority DESC, wq.QueuePosition ASC;
        
        DECLARE @BookingID INT, @WaitlistID INT, @PassengerCount INT, @UserID INT, @BookingReference NVARCHAR(20);
        DECLARE @RemainingSeats INT = @AvailableSeats;
        DECLARE @PromotedBookings TABLE (BookingID INT, BookingReference NVARCHAR(20), UserID INT);
        
        OPEN @WaitlistCursor;
        FETCH NEXT FROM @WaitlistCursor INTO @BookingID, @WaitlistID, @PassengerCount, @UserID, @BookingReference;
        
        WHILE @@FETCH_STATUS = 0 AND @RemainingSeats > 0
        BEGIN
            -- Check if this booking can be accommodated
            IF @PassengerCount <= @RemainingSeats
            BEGIN
                -- Promote booking to confirmed
                DECLARE @ConfirmedStatusID INT;
                SELECT @ConfirmedStatusID = StatusID FROM BookingStatusTypes WHERE StatusName = 'Confirmed';
                
                UPDATE Bookings 
                SET StatusID = @ConfirmedStatusID, UpdatedBy = @ProcessedBy, UpdatedAt = GETUTCDATE()
                WHERE BookingID = @BookingID;
                
                -- Remove from waitlist
                UPDATE WaitlistQueue 
                SET IsActive = 0, UpdatedBy = @ProcessedBy, UpdatedAt = GETUTCDATE()
                WHERE WaitlistID = @WaitlistID;
                
                -- Update remaining seats
                SET @RemainingSeats = @RemainingSeats - @PassengerCount;
                
                -- Track promoted booking
                INSERT INTO @PromotedBookings (BookingID, BookingReference, UserID)
                VALUES (@BookingID, @BookingReference, @UserID);
                
                -- Create audit log
                INSERT INTO BookingAudit (BookingID, Action, ActionBy, Details, CreatedBy)
                VALUES (@BookingID, 'Waitlist Promoted', @ProcessedBy, 
                        'Booking promoted from waitlist to confirmed status', @ProcessedBy);
                
                -- Create notification
                INSERT INTO Notifications (UserID, Title, Message, Type, RelatedBookingID)
                VALUES (@UserID, 'Booking Confirmed!', 
                        'Great news! Your waitlisted booking ' + @BookingReference + ' has been confirmed due to seat availability.',
                        'Success', @BookingID);
            END
            
            FETCH NEXT FROM @WaitlistCursor INTO @BookingID, @WaitlistID, @PassengerCount, @UserID, @BookingReference;
        END
        
        CLOSE @WaitlistCursor;
        DEALLOCATE @WaitlistCursor;
        
        -- Update schedule availability
        IF @ScheduleType = 'Train'
        BEGIN
            UPDATE TrainSchedules 
            SET AvailableSeats = AvailableSeats - (@AvailableSeats - @RemainingSeats),
                UpdatedBy = @ProcessedBy,
                UpdatedAt = GETUTCDATE()
            WHERE ScheduleID = @TrainScheduleID;
        END
        ELSE
        BEGIN
            UPDATE FlightSchedules 
            SET AvailableSeats = AvailableSeats - (@AvailableSeats - @RemainingSeats),
                UpdatedBy = @ProcessedBy,
                UpdatedAt = GETUTCDATE()
            WHERE ScheduleID = @FlightScheduleID;
        END
        
        -- Update queue positions for remaining waitlisted bookings
        EXEC sp_UpdateWaitlistPositions @TrainScheduleID, @FlightScheduleID;
        
        COMMIT TRANSACTION;
        
        -- Return promoted bookings
        SELECT 
            BookingID,
            BookingReference,
            UserID,
            (@AvailableSeats - @RemainingSeats) as SeatsAllocated,
            'Waitlist promotion completed successfully' as Message,
            1 as Success
        FROM @PromotedBookings;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Log error
        INSERT INTO SystemLogs (LogLevel, Message, StackTrace)
        VALUES ('Error', ERROR_MESSAGE(), ERROR_PROCEDURE() + ' - Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10)));
        
        THROW;
    END CATCH
END;
GO

-- =============================================
-- Update Waitlist Positions
-- =============================================
CREATE OR ALTER PROCEDURE sp_UpdateWaitlistPositions
    @TrainScheduleID INT = NULL,
    @FlightScheduleID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate inputs
        IF @TrainScheduleID IS NULL AND @FlightScheduleID IS NULL
        BEGIN
            THROW 50032, 'Either TrainScheduleID or FlightScheduleID must be provided', 1;
        END
        
        -- Update queue positions for train waitlist
        IF @TrainScheduleID IS NOT NULL
        BEGIN
            WITH RankedWaitlist AS (
                SELECT 
                    WaitlistID,
                    ROW_NUMBER() OVER (ORDER BY Priority DESC, CreatedAt ASC) as NewPosition
                FROM WaitlistQueue 
                WHERE TrainScheduleID = @TrainScheduleID AND IsActive = 1
            )
            UPDATE wq
            SET QueuePosition = rw.NewPosition,
                UpdatedAt = GETUTCDATE(),
                UpdatedBy = 'System'
            FROM WaitlistQueue wq
            INNER JOIN RankedWaitlist rw ON wq.WaitlistID = rw.WaitlistID;
        END
        
        -- Update queue positions for flight waitlist
        IF @FlightScheduleID IS NOT NULL
        BEGIN
            WITH RankedWaitlist AS (
                SELECT 
                    WaitlistID,
                    ROW_NUMBER() OVER (ORDER BY Priority DESC, CreatedAt ASC) as NewPosition
                FROM WaitlistQueue 
                WHERE FlightScheduleID = @FlightScheduleID AND IsActive = 1
            )
            UPDATE wq
            SET QueuePosition = rw.NewPosition,
                UpdatedAt = GETUTCDATE(),
                UpdatedBy = 'System'
            FROM WaitlistQueue wq
            INNER JOIN RankedWaitlist rw ON wq.WaitlistID = rw.WaitlistID;
        END
        
    END TRY
    BEGIN CATCH
        -- Log error
        INSERT INTO SystemLogs (LogLevel, Message, StackTrace)
        VALUES ('Error', ERROR_MESSAGE(), ERROR_PROCEDURE() + ' - Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10)));
        
        THROW;
    END CATCH
END;
GO

-- =============================================
-- Get Next Waitlist Position
-- =============================================
CREATE OR ALTER PROCEDURE sp_GetNextWaitlistPosition
    @TrainScheduleID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @NextPosition INT;
        
        -- Get the total count of active waitlist entries
        SELECT @NextPosition = COUNT(*)
        FROM WaitlistQueue 
        WHERE TrainScheduleID = @TrainScheduleID 
        AND IsActive = 1;
        
        SELECT @NextPosition as NextPosition;
        
    END TRY
    BEGIN CATCH
        -- Log error
        INSERT INTO SystemLogs (LogLevel, Message, StackTrace)
        VALUES ('Error', ERROR_MESSAGE(), ERROR_PROCEDURE() + ' - Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10)));
        
        -- Return 0 if error
        SELECT 0 as NextPosition;
    END CATCH
END;
GO

-- =============================================
-- Get Waitlist Status
-- =============================================
CREATE OR ALTER PROCEDURE sp_GetWaitlistStatus
    @TrainScheduleID INT = NULL,
    @FlightScheduleID INT = NULL,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate inputs
        IF @TrainScheduleID IS NULL AND @FlightScheduleID IS NULL
        BEGIN
            THROW 50032, 'Either TrainScheduleID or FlightScheduleID must be provided', 1;
        END
        
        SELECT 
            wq.WaitlistID,
            wq.BookingID,
            b.BookingReference,
            b.UserID,
            u.Name as UserName,
            b.TotalPassengers,
            wq.QueuePosition,
            wq.Priority,
            wq.CreatedAt as WaitlistDate,
            CASE 
                WHEN wq.TrainScheduleID IS NOT NULL THEN 'Train'
                ELSE 'Flight'
            END as BookingType
        FROM WaitlistQueue wq
        INNER JOIN Bookings b ON wq.BookingID = b.BookingID
        INNER JOIN Users u ON b.UserID = u.UserID
        WHERE 
            ((@TrainScheduleID IS NOT NULL AND wq.TrainScheduleID = @TrainScheduleID) OR
             (@FlightScheduleID IS NOT NULL AND wq.FlightScheduleID = @FlightScheduleID))
            AND (@UserID IS NULL OR b.UserID = @UserID)
            AND wq.IsActive = 1
            AND b.IsActive = 1
        ORDER BY wq.Priority DESC, wq.QueuePosition ASC;
        
    END TRY
    BEGIN CATCH
        -- Log error
        INSERT INTO SystemLogs (LogLevel, Message, StackTrace, UserID)
        VALUES ('Error', ERROR_MESSAGE(), ERROR_PROCEDURE() + ' - Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10)), @UserID);
        
        THROW;
    END CATCH
END;
GO

-- =============================================
-- Cancel Booking and Promote Waitlist
-- =============================================
CREATE OR ALTER PROCEDURE sp_CancelBookingAndPromoteWaitlist
    @BookingID INT,
    @CancellationReason NVARCHAR(500) = NULL,
    @CancelledBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Get booking details
        DECLARE @BookingType NVARCHAR(10), @TrainScheduleID INT, @FlightScheduleID INT;
        DECLARE @TotalPassengers INT, @BookingStatus NVARCHAR(50), @UserID INT;
        
        SELECT 
            @BookingType = b.BookingType,
            @TrainScheduleID = b.TrainScheduleID,
            @FlightScheduleID = b.FlightScheduleID,
            @TotalPassengers = b.TotalPassengers,
            @BookingStatus = bs.StatusName,
            @UserID = b.UserID
        FROM Bookings b
        INNER JOIN BookingStatusTypes bs ON b.StatusID = bs.StatusID
        WHERE b.BookingID = @BookingID AND b.IsActive = 1;
        
        IF @BookingType IS NULL
        BEGIN
            THROW 50023, 'Invalid booking ID', 1;
        END
        
        -- Check if booking can be cancelled
        IF @BookingStatus = 'Cancelled'
        BEGIN
            THROW 50034, 'Booking is already cancelled', 1;
        END
        
        -- Update booking status to cancelled
        DECLARE @CancelledStatusID INT;
        SELECT @CancelledStatusID = StatusID FROM BookingStatusTypes WHERE StatusName = 'Cancelled';
        
        UPDATE Bookings 
        SET StatusID = @CancelledStatusID, UpdatedBy = @CancelledBy, UpdatedAt = GETUTCDATE()
        WHERE BookingID = @BookingID;
        
        -- Remove from waitlist if waitlisted
        IF @BookingStatus = 'Waitlisted'
        BEGIN
            UPDATE WaitlistQueue 
            SET IsActive = 0, UpdatedBy = @CancelledBy, UpdatedAt = GETUTCDATE()
            WHERE BookingID = @BookingID;
            
            -- Update queue positions
            EXEC sp_UpdateWaitlistPositions @TrainScheduleID, @FlightScheduleID;
        END
        ELSE IF @BookingStatus = 'Confirmed'
        BEGIN
            -- Release seats and promote waitlist
            IF @BookingType = 'Train'
            BEGIN
                UPDATE TrainSchedules 
                SET AvailableSeats = AvailableSeats + @TotalPassengers,
                    UpdatedBy = @CancelledBy,
                    UpdatedAt = GETUTCDATE()
                WHERE ScheduleID = @TrainScheduleID;
                
                -- Promote waitlist
                EXEC sp_PromoteWaitlist @TrainScheduleID = @TrainScheduleID, 
                                       @AvailableSeats = @TotalPassengers, 
                                       @ProcessedBy = @CancelledBy;
            END
            ELSE
            BEGIN
                UPDATE FlightSchedules 
                SET AvailableSeats = AvailableSeats + @TotalPassengers,
                    UpdatedBy = @CancelledBy,
                    UpdatedAt = GETUTCDATE()
                WHERE ScheduleID = @FlightScheduleID;
                
                -- Note: Flights typically don't have waitlist, but including for completeness
                EXEC sp_PromoteWaitlist @FlightScheduleID = @FlightScheduleID, 
                                       @AvailableSeats = @TotalPassengers, 
                                       @ProcessedBy = @CancelledBy;
            END
        END
        
        -- Create cancellation record
        INSERT INTO Cancellations (BookingID, CancellationReason, CancelledBy, CreatedBy)
        VALUES (@BookingID, @CancellationReason, @CancelledBy, @CancelledBy);
        
        -- Create audit log
        INSERT INTO BookingAudit (BookingID, Action, ActionBy, Details, CreatedBy)
        VALUES (@BookingID, 'Cancelled', @CancelledBy, 
                'Booking cancelled. Reason: ' + ISNULL(@CancellationReason, 'Not specified'), @CancelledBy);
        
        -- Create notification
        INSERT INTO Notifications (UserID, Title, Message, Type, RelatedBookingID)
        VALUES (@UserID, 'Booking Cancelled', 
                'Your booking has been cancelled successfully. If you made a payment, refund will be processed as per cancellation policy.',
                'Info', @BookingID);
        
        COMMIT TRANSACTION;
        
        SELECT 
            'Booking cancelled successfully' as Message,
            @TotalPassengers as SeatsReleased,
            1 as Success;
            
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Log error
        INSERT INTO SystemLogs (LogLevel, Message, StackTrace, UserID)
        VALUES ('Error', ERROR_MESSAGE(), ERROR_PROCEDURE() + ' - Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10)), @UserID);
        
        THROW;
    END CATCH
END;
GO