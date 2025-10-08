using TransitHub.Models;

namespace TransitHub.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(TransitHubDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed TrainQuotaTypes
            if (!context.TrainQuotaTypes.Any())
            {
                var quotaTypes = new[]
                {
                    new TrainQuotaType { QuotaName = "Normal", Description = "Regular booking quota" },
                    new TrainQuotaType { QuotaName = "Tatkal", Description = "Emergency booking quota (premium charges)" },
                    new TrainQuotaType { QuotaName = "Premium Tatkal", Description = "Premium emergency booking quota" },
                    new TrainQuotaType { QuotaName = "Senior Citizen", Description = "Senior citizen quota" },
                    new TrainQuotaType { QuotaName = "Ladies", Description = "Ladies quota" },
                    new TrainQuotaType { QuotaName = "Physically Handicapped", Description = "Quota for physically handicapped passengers" }
                };
                
                await context.TrainQuotaTypes.AddRangeAsync(quotaTypes);
            }

            // Seed BookingStatusTypes
            if (!context.BookingStatusTypes.Any())
            {
                var statusTypes = new[]
                {
                    new BookingStatusType { StatusName = "Confirmed", Description = "Booking confirmed with seat allocation" },
                    new BookingStatusType { StatusName = "Waitlisted", Description = "Booking in waitlist queue" },
                    new BookingStatusType { StatusName = "Cancelled", Description = "Booking cancelled by user or system" },
                    new BookingStatusType { StatusName = "RAC", Description = "Reservation Against Cancellation" },
                    new BookingStatusType { StatusName = "Chart Prepared", Description = "Final chart prepared, no more bookings" }
                };
                
                await context.BookingStatusTypes.AddRangeAsync(statusTypes);
            }

            // Seed PaymentModes
            if (!context.PaymentModes.Any())
            {
                var paymentModes = new[]
                {
                    new PaymentMode { ModeName = "Credit Card", Description = "Payment via credit card" },
                    new PaymentMode { ModeName = "Debit Card", Description = "Payment via debit card" },
                    new PaymentMode { ModeName = "UPI", Description = "Unified Payments Interface" },
                    new PaymentMode { ModeName = "Net Banking", Description = "Internet banking payment" },
                    new PaymentMode { ModeName = "Wallet", Description = "Digital wallet payment" }
                };
                
                await context.PaymentModes.AddRangeAsync(paymentModes);
            }

            // Seed TrainClasses
            if (!context.TrainClasses.Any())
            {
                var trainClasses = new[]
                {
                    new TrainClass { ClassName = "SL", Description = "Sleeper Class" },
                    new TrainClass { ClassName = "3A", Description = "Third AC" },
                    new TrainClass { ClassName = "2A", Description = "Second AC" },
                    new TrainClass { ClassName = "1A", Description = "First AC" },
                    new TrainClass { ClassName = "CC", Description = "Chair Car" },
                    new TrainClass { ClassName = "2S", Description = "Second Sitting" },
                    new TrainClass { ClassName = "FC", Description = "First Class" }
                };
                
                await context.TrainClasses.AddRangeAsync(trainClasses);
            }

            // Seed FlightClasses
            if (!context.FlightClasses.Any())
            {
                var flightClasses = new[]
                {
                    new FlightClass { ClassName = "Economy", Description = "Economy class seating" },
                    new FlightClass { ClassName = "Premium Economy", Description = "Premium economy class" },
                    new FlightClass { ClassName = "Business", Description = "Business class seating" },
                    new FlightClass { ClassName = "First", Description = "First class seating" }
                };
                
                await context.FlightClasses.AddRangeAsync(flightClasses);
            }

            // Seed Stations
            if (!context.Stations.Any())
            {
                var stations = new[]
                {
                    new Station { StationName = "New Delhi Railway Station", City = "New Delhi", State = "Delhi", StationCode = "NDLS" },
                    new Station { StationName = "Mumbai Central", City = "Mumbai", State = "Maharashtra", StationCode = "BCT" },
                    new Station { StationName = "Howrah Junction", City = "Kolkata", State = "West Bengal", StationCode = "HWH" },
                    new Station { StationName = "Chennai Central", City = "Chennai", State = "Tamil Nadu", StationCode = "MAS" },
                    new Station { StationName = "Bangalore City Junction", City = "Bangalore", State = "Karnataka", StationCode = "SBC" },
                    new Station { StationName = "Hyderabad Deccan", City = "Hyderabad", State = "Telangana", StationCode = "HYB" },
                    new Station { StationName = "Pune Junction", City = "Pune", State = "Maharashtra", StationCode = "PUNE" },
                    new Station { StationName = "Ahmedabad Junction", City = "Ahmedabad", State = "Gujarat", StationCode = "ADI" },
                    new Station { StationName = "Jaipur Junction", City = "Jaipur", State = "Rajasthan", StationCode = "JP" },
                    new Station { StationName = "Lucknow Junction", City = "Lucknow", State = "Uttar Pradesh", StationCode = "LJN" }
                };
                
                await context.Stations.AddRangeAsync(stations);
            }

            // Seed Airports
            if (!context.Airports.Any())
            {
                var airports = new[]
                {
                    new Airport { AirportName = "Indira Gandhi International Airport", City = "New Delhi", State = "Delhi", Code = "DEL" },
                    new Airport { AirportName = "Chhatrapati Shivaji International Airport", City = "Mumbai", State = "Maharashtra", Code = "BOM" },
                    new Airport { AirportName = "Netaji Subhas Chandra Bose International Airport", City = "Kolkata", State = "West Bengal", Code = "CCU" },
                    new Airport { AirportName = "Chennai International Airport", City = "Chennai", State = "Tamil Nadu", Code = "MAA" },
                    new Airport { AirportName = "Kempegowda International Airport", City = "Bangalore", State = "Karnataka", Code = "BLR" },
                    new Airport { AirportName = "Rajiv Gandhi International Airport", City = "Hyderabad", State = "Telangana", Code = "HYD" },
                    new Airport { AirportName = "Pune Airport", City = "Pune", State = "Maharashtra", Code = "PNQ" },
                    new Airport { AirportName = "Sardar Vallabhbhai Patel International Airport", City = "Ahmedabad", State = "Gujarat", Code = "AMD" },
                    new Airport { AirportName = "Jaipur International Airport", City = "Jaipur", State = "Rajasthan", Code = "JAI" },
                    new Airport { AirportName = "Chaudhary Charan Singh International Airport", City = "Lucknow", State = "Uttar Pradesh", Code = "LKO" }
                };
                
                await context.Airports.AddRangeAsync(airports);
            }

            // Save initial data first
            await context.SaveChangesAsync();

            // Only seed trains if none exist (don't clear existing ones)
            Console.WriteLine("Checking for existing trains...");
            
            // Seed Trains (after stations are saved)
            if (!context.Trains.Any())
            {
                var trains = new[]
                {
                    // Delhi to Mumbai routes
                    new Train { TrainName = "Rajdhani Express", TrainNumber = "12001", SourceStationID = 1, DestinationStationID = 2 },
                    new Train { TrainName = "August Kranti Rajdhani", TrainNumber = "12953", SourceStationID = 1, DestinationStationID = 2 },
                    new Train { TrainName = "Golden Temple Mail", TrainNumber = "12903", SourceStationID = 1, DestinationStationID = 2 },
                    
                    // Delhi to Bangalore routes
                    new Train { TrainName = "Shatabdi Express", TrainNumber = "12002", SourceStationID = 1, DestinationStationID = 5 },
                    new Train { TrainName = "Karnataka Express", TrainNumber = "12627", SourceStationID = 1, DestinationStationID = 5 },
                    
                    // Mumbai to Kolkata routes
                    new Train { TrainName = "Duronto Express", TrainNumber = "12259", SourceStationID = 2, DestinationStationID = 3 },
                    new Train { TrainName = "Gitanjali Express", TrainNumber = "12859", SourceStationID = 2, DestinationStationID = 3 },
                    
                    // Delhi to Chennai routes
                    new Train { TrainName = "Garib Rath", TrainNumber = "12204", SourceStationID = 1, DestinationStationID = 4 },
                    new Train { TrainName = "Tamil Nadu Express", TrainNumber = "12621", SourceStationID = 1, DestinationStationID = 4 },
                    new Train { TrainName = "Grand Trunk Express", TrainNumber = "12615", SourceStationID = 1, DestinationStationID = 4 },
                    
                    // Bangalore to Hyderabad routes
                    new Train { TrainName = "Jan Shatabdi", TrainNumber = "12023", SourceStationID = 5, DestinationStationID = 6 },
                    new Train { TrainName = "Bangalore Express", TrainNumber = "17225", SourceStationID = 5, DestinationStationID = 6 },
                    
                    // Additional Bangalore routes
                    new Train { TrainName = "Bangalore Rajdhani", TrainNumber = "12429", SourceStationID = 5, DestinationStationID = 1 }, // Bangalore to Delhi
                    new Train { TrainName = "Udyan Express", TrainNumber = "16529", SourceStationID = 5, DestinationStationID = 2 }, // Bangalore to Mumbai
                    new Train { TrainName = "Shatabdi Express", TrainNumber = "12007", SourceStationID = 5, DestinationStationID = 4 }, // Bangalore to Chennai
                    
                    // Additional popular routes
                    new Train { TrainName = "Deccan Queen", TrainNumber = "12123", SourceStationID = 2, DestinationStationID = 7 }, // Mumbai to Pune
                    new Train { TrainName = "Intercity Express", TrainNumber = "12216", SourceStationID = 2, DestinationStationID = 7 }, // Mumbai to Pune
                    new Train { TrainName = "Pink City Express", TrainNumber = "12307", SourceStationID = 1, DestinationStationID = 9 }, // Delhi to Jaipur
                    new Train { TrainName = "Double Decker", TrainNumber = "12273", SourceStationID = 1, DestinationStationID = 9 }, // Delhi to Jaipur
                    new Train { TrainName = "Sabarmati Express", TrainNumber = "19019", SourceStationID = 1, DestinationStationID = 8 }, // Delhi to Ahmedabad
                    new Train { TrainName = "Ashram Express", TrainNumber = "12915", SourceStationID = 1, DestinationStationID = 8 }, // Delhi to Ahmedabad
                    new Train { TrainName = "Lucknow Mail", TrainNumber = "12229", SourceStationID = 1, DestinationStationID = 10 }, // Delhi to Lucknow
                    new Train { TrainName = "Gomti Express", TrainNumber = "12419", SourceStationID = 1, DestinationStationID = 10 } // Delhi to Lucknow
                };
                Console.WriteLine($"Adding {trains.Length} trains to database...");
                await context.Trains.AddRangeAsync(trains);
                await context.SaveChangesAsync(); // Save trains first to get their IDs
                Console.WriteLine("Trains saved to database.");
            }

            // Seed Flights (after airports are saved)
            if (!context.Flights.Any())
            {
                var flights = new[]
                {
                    new Flight { Airline = "Air India Express", FlightNumber = "AI101", SourceAirportID = 1, DestinationAirportID = 2 },
                    new Flight { Airline = "IndiGo", FlightNumber = "6E202", SourceAirportID = 1, DestinationAirportID = 5 },
                    new Flight { Airline = "SpiceJet", FlightNumber = "SG303", SourceAirportID = 2, DestinationAirportID = 3 },
                    new Flight { Airline = "Vistara", FlightNumber = "UK404", SourceAirportID = 1, DestinationAirportID = 4 },
                    new Flight { Airline = "GoAir", FlightNumber = "G8505", SourceAirportID = 5, DestinationAirportID = 6 }
                };
                await context.Flights.AddRangeAsync(flights);
            }

            await context.SaveChangesAsync();

            // Get the actual train IDs from database
            var trainIds = context.Trains.ToDictionary(t => t.TrainNumber, t => t.TrainID);

            // Seed Train Schedules (after trains are saved) - Only if none exist
            if (!context.TrainSchedules.Any())
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var schedules = new List<TrainSchedule>();

                for (int i = 0; i < 30; i++) // Next 30 days
                {
                    var travelDate = today.AddDays(i);
                    
                    // Create schedules for multiple trains with different classes
                    var trainSchedules = new[]
                    {
                        // Rajdhani Express - Delhi to Mumbai (3A, 2A, 1A)
                        new TrainSchedule { TrainID = trainIds["12001"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(16), ArrivalTime = DateTime.Today.AddDays(1).AddHours(8), QuotaTypeID = 1, TrainClassID = 2, TotalSeats = 72, AvailableSeats = 72, Fare = 1850.00m },
                        new TrainSchedule { TrainID = trainIds["12001"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(16), ArrivalTime = DateTime.Today.AddDays(1).AddHours(8), QuotaTypeID = 1, TrainClassID = 3, TotalSeats = 48, AvailableSeats = 48, Fare = 2650.00m },
                        new TrainSchedule { TrainID = trainIds["12001"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(16), ArrivalTime = DateTime.Today.AddDays(1).AddHours(8), QuotaTypeID = 1, TrainClassID = 4, TotalSeats = 24, AvailableSeats = 24, Fare = 4200.00m },
                        
                        // August Kranti Rajdhani - Delhi to Mumbai (3A, 2A, 1A)
                        new TrainSchedule { TrainID = trainIds["12953"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(17), ArrivalTime = DateTime.Today.AddDays(1).AddHours(9), QuotaTypeID = 1, TrainClassID = 2, TotalSeats = 72, AvailableSeats = 72, Fare = 1900.00m },
                        new TrainSchedule { TrainID = trainIds["12953"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(17), ArrivalTime = DateTime.Today.AddDays(1).AddHours(9), QuotaTypeID = 1, TrainClassID = 3, TotalSeats = 48, AvailableSeats = 48, Fare = 2700.00m },
                        
                        // Golden Temple Mail - Delhi to Mumbai (SL, 3A, 2A)
                        new TrainSchedule { TrainID = trainIds["12903"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(19), ArrivalTime = DateTime.Today.AddDays(1).AddHours(12), QuotaTypeID = 1, TrainClassID = 1, TotalSeats = 96, AvailableSeats = 96, Fare = 650.00m },
                        new TrainSchedule { TrainID = trainIds["12903"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(19), ArrivalTime = DateTime.Today.AddDays(1).AddHours(12), QuotaTypeID = 1, TrainClassID = 2, TotalSeats = 72, AvailableSeats = 72, Fare = 1650.00m },
                        
                        // Shatabdi Express - Delhi to Bangalore (CC, 2S)
                        new TrainSchedule { TrainID = trainIds["12002"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(6), ArrivalTime = DateTime.Today.AddHours(18), QuotaTypeID = 1, TrainClassID = 5, TotalSeats = 120, AvailableSeats = 120, Fare = 1200.00m },
                        new TrainSchedule { TrainID = trainIds["12002"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(6), ArrivalTime = DateTime.Today.AddHours(18), QuotaTypeID = 1, TrainClassID = 6, TotalSeats = 80, AvailableSeats = 80, Fare = 800.00m },
                        
                        // Karnataka Express - Delhi to Bangalore (SL, 3A, 2A)
                        new TrainSchedule { TrainID = trainIds["12627"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(21), ArrivalTime = DateTime.Today.AddDays(2).AddHours(6), QuotaTypeID = 1, TrainClassID = 1, TotalSeats = 96, AvailableSeats = 96, Fare = 750.00m },
                        new TrainSchedule { TrainID = trainIds["12627"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(21), ArrivalTime = DateTime.Today.AddDays(2).AddHours(6), QuotaTypeID = 1, TrainClassID = 2, TotalSeats = 72, AvailableSeats = 72, Fare = 1750.00m },
                        
                        // Duronto Express - Mumbai to Kolkata (3A, 2A, 1A)
                        new TrainSchedule { TrainID = trainIds["12259"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(22), ArrivalTime = DateTime.Today.AddDays(1).AddHours(18), QuotaTypeID = 1, TrainClassID = 2, TotalSeats = 72, AvailableSeats = 72, Fare = 2200.00m },
                        new TrainSchedule { TrainID = trainIds["12259"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(22), ArrivalTime = DateTime.Today.AddDays(1).AddHours(18), QuotaTypeID = 1, TrainClassID = 3, TotalSeats = 48, AvailableSeats = 48, Fare = 3200.00m },
                        
                        // Deccan Queen - Mumbai to Pune (CC, 2S)
                        new TrainSchedule { TrainID = trainIds["12123"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(7), ArrivalTime = DateTime.Today.AddHours(10), QuotaTypeID = 1, TrainClassID = 5, TotalSeats = 120, AvailableSeats = 120, Fare = 180.00m },
                        new TrainSchedule { TrainID = trainIds["12123"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(7), ArrivalTime = DateTime.Today.AddHours(10), QuotaTypeID = 1, TrainClassID = 6, TotalSeats = 80, AvailableSeats = 80, Fare = 120.00m },
                        
                        // Pink City Express - Delhi to Jaipur (SL, 3A, CC)
                        new TrainSchedule { TrainID = trainIds["12307"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(5), ArrivalTime = DateTime.Today.AddHours(10), QuotaTypeID = 1, TrainClassID = 1, TotalSeats = 96, AvailableSeats = 96, Fare = 280.00m },
                        new TrainSchedule { TrainID = trainIds["12307"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(5), ArrivalTime = DateTime.Today.AddHours(10), QuotaTypeID = 1, TrainClassID = 2, TotalSeats = 72, AvailableSeats = 72, Fare = 650.00m },
                        new TrainSchedule { TrainID = trainIds["12307"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(5), ArrivalTime = DateTime.Today.AddHours(10), QuotaTypeID = 1, TrainClassID = 5, TotalSeats = 120, AvailableSeats = 120, Fare = 450.00m },
                        
                        // Lucknow Mail - Delhi to Lucknow (SL, 3A, 2A)
                        new TrainSchedule { TrainID = trainIds["12229"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(22), ArrivalTime = DateTime.Today.AddDays(1).AddHours(6), QuotaTypeID = 1, TrainClassID = 1, TotalSeats = 96, AvailableSeats = 96, Fare = 320.00m },
                        new TrainSchedule { TrainID = trainIds["12229"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(22), ArrivalTime = DateTime.Today.AddDays(1).AddHours(6), QuotaTypeID = 1, TrainClassID = 2, TotalSeats = 72, AvailableSeats = 72, Fare = 780.00m },
                        new TrainSchedule { TrainID = trainIds["12229"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(22), ArrivalTime = DateTime.Today.AddDays(1).AddHours(6), QuotaTypeID = 1, TrainClassID = 3, TotalSeats = 48, AvailableSeats = 48, Fare = 1150.00m },
                        
                        // Gomti Express - Delhi to Lucknow (SL, 3A, CC)
                        new TrainSchedule { TrainID = trainIds["12419"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(15), ArrivalTime = DateTime.Today.AddHours(21), QuotaTypeID = 1, TrainClassID = 1, TotalSeats = 96, AvailableSeats = 96, Fare = 290.00m },
                        new TrainSchedule { TrainID = trainIds["12419"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(15), ArrivalTime = DateTime.Today.AddHours(21), QuotaTypeID = 1, TrainClassID = 2, TotalSeats = 72, AvailableSeats = 72, Fare = 720.00m },
                        new TrainSchedule { TrainID = trainIds["12419"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(15), ArrivalTime = DateTime.Today.AddHours(21), QuotaTypeID = 1, TrainClassID = 5, TotalSeats = 120, AvailableSeats = 120, Fare = 520.00m },
                        
                        // NEW BANGALORE TRAINS
                        // Bangalore Rajdhani - Bangalore to Delhi (3A, 2A, 1A)
                        new TrainSchedule { TrainID = trainIds["12429"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(20), ArrivalTime = DateTime.Today.AddDays(1).AddHours(22), QuotaTypeID = 1, TrainClassID = 2, TotalSeats = 72, AvailableSeats = 72, Fare = 2100.00m },
                        new TrainSchedule { TrainID = trainIds["12429"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(20), ArrivalTime = DateTime.Today.AddDays(1).AddHours(22), QuotaTypeID = 1, TrainClassID = 3, TotalSeats = 48, AvailableSeats = 48, Fare = 2900.00m },
                        new TrainSchedule { TrainID = trainIds["12429"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(20), ArrivalTime = DateTime.Today.AddDays(1).AddHours(22), QuotaTypeID = 1, TrainClassID = 4, TotalSeats = 24, AvailableSeats = 24, Fare = 4500.00m },
                        
                        // Udyan Express - Bangalore to Mumbai (SL, 3A, 2A)
                        new TrainSchedule { TrainID = trainIds["16529"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(14), ArrivalTime = DateTime.Today.AddDays(1).AddHours(6), QuotaTypeID = 1, TrainClassID = 1, TotalSeats = 96, AvailableSeats = 96, Fare = 850.00m },
                        new TrainSchedule { TrainID = trainIds["16529"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(14), ArrivalTime = DateTime.Today.AddDays(1).AddHours(6), QuotaTypeID = 1, TrainClassID = 2, TotalSeats = 72, AvailableSeats = 72, Fare = 1950.00m },
                        new TrainSchedule { TrainID = trainIds["16529"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(14), ArrivalTime = DateTime.Today.AddDays(1).AddHours(6), QuotaTypeID = 1, TrainClassID = 3, TotalSeats = 48, AvailableSeats = 48, Fare = 2750.00m },
                        
                        // Shatabdi Express - Bangalore to Chennai (CC, 2S)
                        new TrainSchedule { TrainID = trainIds["12007"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(6), ArrivalTime = DateTime.Today.AddHours(11), QuotaTypeID = 1, TrainClassID = 5, TotalSeats = 120, AvailableSeats = 120, Fare = 650.00m },
                        new TrainSchedule { TrainID = trainIds["12007"], TravelDate = travelDate, DepartureTime = DateTime.Today.AddHours(6), ArrivalTime = DateTime.Today.AddHours(11), QuotaTypeID = 1, TrainClassID = 6, TotalSeats = 80, AvailableSeats = 80, Fare = 450.00m }
                    };
                    
                    schedules.AddRange(trainSchedules);
                }
                
                Console.WriteLine($"Adding {schedules.Count} train schedules to database...");
                await context.TrainSchedules.AddRangeAsync(schedules);
                Console.WriteLine("Train schedules added to context.");
            }

            // Seed Flight Schedules
            if (!context.FlightSchedules.Any())
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var schedules = new List<FlightSchedule>();

                for (int i = 0; i < 30; i++) // Next 30 days
                {
                    var travelDate = today.AddDays(i);
                    
                    schedules.AddRange(new[]
                    {
                        new FlightSchedule 
                        { 
                            FlightID = 1, TravelDate = travelDate, 
                            DepartureTime = DateTime.Today.AddHours(9), 
                            ArrivalTime = DateTime.Today.AddHours(11),
                            FlightClassID = 1, 
                            TotalSeats = 180, AvailableSeats = 120, Fare = 4500.00m 
                        },
                        new FlightSchedule 
                        { 
                            FlightID = 2, TravelDate = travelDate, 
                            DepartureTime = DateTime.Today.AddHours(15), 
                            ArrivalTime = DateTime.Today.AddHours(18),
                            FlightClassID = 1, 
                            TotalSeats = 186, AvailableSeats = 95, Fare = 3800.00m 
                        }
                    });
                }
                
                await context.FlightSchedules.AddRangeAsync(schedules);
            }

            // Save all changes
            await context.SaveChangesAsync();
        }
    }
}