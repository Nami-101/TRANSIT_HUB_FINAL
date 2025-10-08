-- Reset all seats to available and clear test bookings
-- This script ensures all seats show as available in the booking interface

BEGIN TRANSACTION;

BEGIN TRY
    -- Clear all test bookings and passengers
    DELETE FROM TrainBookingPassengers;
    DELETE FROM TrainBookings;
    
    -- Reset all seats to available (not occupied)
    UPDATE TrainSeats 
    SET IsOccupied = 0, 
        BookingId = NULL, 
        PassengerId = NULL,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = 'SystemReset';
    
    -- Reset coach availability to full capacity
    UPDATE TrainCoaches 
    SET AvailableSeats = TotalSeats,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = 'SystemReset';
    
    -- Reset schedule availability to full capacity
    UPDATE TrainSchedules 
    SET AvailableSeats = TotalSeats,
        UpdatedAt = GETUTCDATE(),
        UpdatedBy = 'SystemReset';
    
    -- Get counts for verification
    DECLARE @TotalSeats INT = (SELECT COUNT(*) FROM TrainSeats);
    DECLARE @AvailableSeats INT = (SELECT COUNT(*) FROM TrainSeats WHERE IsOccupied = 0);
    DECLARE @CoachCount INT = (SELECT COUNT(*) FROM TrainCoaches);
    
    PRINT 'Seat reset completed successfully!';
    PRINT 'Total seats in system: ' + CAST(@TotalSeats AS VARCHAR(10));
    PRINT 'Available seats: ' + CAST(@AvailableSeats AS VARCHAR(10));
    PRINT 'Total coaches: ' + CAST(@CoachCount AS VARCHAR(10));
    
    -- Show seat distribution by coach
    SELECT 
        tc.TrainId,
        t.TrainName,
        tc.CoachNumber,
        tc.TotalSeats,
        tc.AvailableSeats,
        COUNT(ts.SeatId) as ActualSeats,
        COUNT(CASE WHEN ts.IsOccupied = 0 THEN 1 END) as ActualAvailable
    FROM TrainCoaches tc
    INNER JOIN Trains t ON tc.TrainId = t.TrainID
    LEFT JOIN TrainSeats ts ON tc.CoachId = ts.CoachId
    GROUP BY tc.TrainId, t.TrainName, tc.CoachNumber, tc.TotalSeats, tc.AvailableSeats
    ORDER BY tc.TrainId, tc.CoachNumber;
    
    COMMIT TRANSACTION;
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();
    
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;