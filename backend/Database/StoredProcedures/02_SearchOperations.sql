-- Transit-Hub Stored Procedures - Search Operations
-- Created: 2025-09-09

USE TransitHubDB;
GO

-- =============================================
-- Search Trains
-- =============================================
CREATE OR ALTER PROCEDURE sp_SearchTrains
    @SourceStationID INT = NULL,
    @DestinationStationID INT = NULL,
    @SourceStationCode NVARCHAR(10) = NULL,
    @DestinationStationCode NVARCHAR(10) = NULL,
    @TravelDate DATE,
    @QuotaTypeID NVARCHAR(50) = NULL,
    @TrainClassID INT = NULL,
    @PassengerCount INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate inputs
        IF @TravelDate < CAST(GETDATE() AS DATE)
        BEGIN
            THROW 50010, 'Travel date cannot be in the past', 1;
        END
        
        IF @PassengerCount <= 0 OR @PassengerCount > 6
        BEGIN
            THROW 50011, 'Passenger count must be between 1 and 6', 1;
        END
        
        -- Get station IDs from codes if provided
        IF @SourceStationCode IS NOT NULL AND @SourceStationID IS NULL
        BEGIN
            SELECT @SourceStationID = StationID FROM Stations WHERE StationCode = @SourceStationCode AND IsActive = 1;
        END
        
        IF @DestinationStationCode IS NOT NULL AND @DestinationStationID IS NULL
        BEGIN
            SELECT @DestinationStationID = StationID FROM Stations WHERE StationCode = @DestinationStationCode AND IsActive = 1;
        END
        
        -- Validate station IDs
        IF @SourceStationID IS NULL OR @DestinationStationID IS NULL
        BEGIN
            THROW 50012, 'Invalid source or destination station', 1;
        END
        
        IF @SourceStationID = @DestinationStationID
        BEGIN
            THROW 50013, 'Source and destination stations cannot be the same', 1;
        END
        
        -- Search trains with availability from TrainCoaches (new quota system)
        SELECT DISTINCT
            MIN(tc.CoachId) as ScheduleID, -- Use first coach ID as schedule ID
            t.TrainID,
            t.TrainName,
            t.TrainNumber,
            ss.StationName as SourceStation,
            ss.StationCode as SourceStationCode,
            ds.StationName as DestinationStation,
            ds.StationCode as DestinationStationCode,
            tc.TravelDate,
            t.DepartureTime,
            t.ArrivalTime,
            DATEDIFF(MINUTE, t.DepartureTime, t.ArrivalTime) as JourneyTimeMinutes,
            'General' as QuotaName, -- Default quota for display
            tc.Class as TrainClass,
            SUM(tc.TotalSeats) as TotalSeats,
            SUM(tc.AvailableSeats) as AvailableSeats,
            t.BasePrice as Fare,
            CASE 
                WHEN SUM(tc.AvailableSeats) >= @PassengerCount THEN 'Available'
                WHEN SUM(tc.AvailableSeats) > 0 THEN 'Limited'
                ELSE 'Waitlist'
            END as AvailabilityStatus,
            SUM(tc.AvailableSeats) as AvailableOrWaitlistPosition
        FROM TrainCoaches tc
        INNER JOIN Trains t ON tc.TrainId = t.TrainID
        INNER JOIN Stations ss ON t.SourceStationID = ss.StationID
        INNER JOIN Stations ds ON t.DestinationStationID = ds.StationID
        WHERE 
            t.SourceStationID = @SourceStationID
            AND t.DestinationStationID = @DestinationStationID
            AND tc.TravelDate = @TravelDate
            AND (@TrainClassID IS NULL OR tc.Class = (SELECT ClassName FROM TrainClasses WHERE TrainClassID = @TrainClassID))
            AND t.IsActive = 1
        GROUP BY 
            t.TrainID, t.TrainName, t.TrainNumber, 
            ss.StationName, ss.StationCode, 
            ds.StationName, ds.StationCode,
            tc.TravelDate, t.DepartureTime, t.ArrivalTime, 
            tc.Class, t.BasePrice
        ORDER BY t.DepartureTime;
        
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
-- Search Flights
-- =============================================
CREATE OR ALTER PROCEDURE sp_SearchFlights
    @SourceAirportID INT = NULL,
    @DestinationAirportID INT = NULL,
    @SourceAirportCode NVARCHAR(5) = NULL,
    @DestinationAirportCode NVARCHAR(5) = NULL,
    @TravelDate DATE,
    @FlightClassID INT = NULL,
    @PassengerCount INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Validate inputs
        IF @TravelDate < CAST(GETDATE() AS DATE)
        BEGIN
            THROW 50010, 'Travel date cannot be in the past', 1;
        END
        
        IF @PassengerCount <= 0 OR @PassengerCount > 6
        BEGIN
            THROW 50011, 'Passenger count must be between 1 and 6', 1;
        END
        
        -- Get airport IDs from codes if provided
        IF @SourceAirportCode IS NOT NULL AND @SourceAirportID IS NULL
        BEGIN
            SELECT @SourceAirportID = AirportID FROM Airports WHERE Code = @SourceAirportCode AND IsActive = 1;
        END
        
        IF @DestinationAirportCode IS NOT NULL AND @DestinationAirportID IS NULL
        BEGIN
            SELECT @DestinationAirportID = AirportID FROM Airports WHERE Code = @DestinationAirportCode AND IsActive = 1;
        END
        
        -- Validate airport IDs
        IF @SourceAirportID IS NULL OR @DestinationAirportID IS NULL
        BEGIN
            THROW 50014, 'Invalid source or destination airport', 1;
        END
        
        IF @SourceAirportID = @DestinationAirportID
        BEGIN
            THROW 50015, 'Source and destination airports cannot be the same', 1;
        END
        
        -- Search flights with availability
        SELECT 
            fs.ScheduleID,
            f.FlightID,
            f.FlightNumber,
            f.Airline,
            sa.AirportName as SourceAirport,
            sa.Code as SourceAirportCode,
            sa.City as SourceCity,
            da.AirportName as DestinationAirport,
            da.Code as DestinationAirportCode,
            da.City as DestinationCity,
            fs.TravelDate,
            fs.DepartureTime,
            fs.ArrivalTime,
            DATEDIFF(MINUTE, fs.DepartureTime, fs.ArrivalTime) as FlightTimeMinutes,
            fc.ClassName as FlightClass,
            fs.TotalSeats,
            fs.AvailableSeats,
            fs.Fare,
            CASE 
                WHEN fs.AvailableSeats >= @PassengerCount THEN 'Available'
                WHEN fs.AvailableSeats > 0 THEN 'Limited'
                ELSE 'Sold Out'
            END as AvailabilityStatus
        FROM FlightSchedules fs
        INNER JOIN Flights f ON fs.FlightID = f.FlightID
        INNER JOIN Airports sa ON f.SourceAirportID = sa.AirportID
        INNER JOIN Airports da ON f.DestinationAirportID = da.AirportID
        INNER JOIN FlightClasses fc ON fs.FlightClassID = fc.FlightClassID
        WHERE 
            f.SourceAirportID = @SourceAirportID
            AND f.DestinationAirportID = @DestinationAirportID
            AND fs.TravelDate = @TravelDate
            AND (@FlightClassID IS NULL OR fs.FlightClassID = @FlightClassID)
            AND fs.AvailableSeats > 0  -- Flights don't have waitlist
            AND f.IsActive = 1
            AND fs.IsActive = 1
        ORDER BY fs.DepartureTime;
        
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
-- Get All Stations
-- =============================================
CREATE OR ALTER PROCEDURE sp_GetAllStations
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            StationID,
            StationName,
            City,
            State,
            StationCode
        FROM Stations 
        WHERE IsActive = 1
        ORDER BY StationName;
        
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
-- Get All Airports
-- =============================================
CREATE OR ALTER PROCEDURE sp_GetAllAirports
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            AirportID,
            AirportName,
            City,
            State,
            Code
        FROM Airports 
        WHERE IsActive = 1
        ORDER BY AirportName;
        
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
-- Get Lookup Data
-- =============================================
CREATE OR ALTER PROCEDURE sp_GetLookupData
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Train Quota Types
        SELECT QuotaTypeID, QuotaName, Description FROM TrainQuotaTypes WHERE IsActive = 1 ORDER BY QuotaName;
        
        -- Train Classes
        SELECT TrainClassID, ClassName, Description FROM TrainClasses WHERE IsActive = 1 ORDER BY ClassName;
        
        -- Flight Classes
        SELECT FlightClassID, ClassName, Description FROM FlightClasses WHERE IsActive = 1 ORDER BY ClassName;
        
        -- Payment Modes
        SELECT PaymentModeID, ModeName, Description FROM PaymentModes WHERE IsActive = 1 ORDER BY ModeName;
        
        -- Booking Status Types
        SELECT StatusID, StatusName, Description FROM BookingStatusTypes WHERE IsActive = 1 ORDER BY StatusName;
        
    END TRY
    BEGIN CATCH
        -- Log error
        INSERT INTO SystemLogs (LogLevel, Message, StackTrace)
        VALUES ('Error', ERROR_MESSAGE(), ERROR_PROCEDURE() + ' - Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10)));
        
        THROW;
    END CATCH
END;
GO