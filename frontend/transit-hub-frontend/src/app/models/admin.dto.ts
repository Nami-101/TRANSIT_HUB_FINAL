// Dashboard Metrics
export interface DashboardMetrics {
  totalUsers: number;
  totalTrains: number;
  totalFlights: number;
  totalBookings: number;
  totalStations: number;
  totalAirports: number;
  totalRevenue: number;
  activeBookings: number;
}

// User Management
export interface AdminUser {
  id: string;
  name: string;
  email: string;
  roles: string[];
  emailConfirmed: boolean;
  createdAt: Date;
  isActive: boolean;
}

// Train Management
export interface AdminTrain {
  trainId: number;
  trainNumber: string;
  trainName: string;
  sourceStationId: number;
  sourceStationName: string;
  destinationStationId: number;
  destinationStationName: string;
  departureTime: string;
  arrivalTime: string;
  basePrice: number;
  isActive: boolean;
  createdAt: Date;
}

export interface CreateTrain {
  trainNumber: string;
  trainName: string;
  sourceStationId: number;
  destinationStationId: number;
  departureTime: string;
  arrivalTime: string;
  basePrice: number;
  selectedClassIds: number[];
}

export interface UpdateTrain {
  trainName: string;
  sourceStationId: number;
  destinationStationId: number;
  departureTime: string;
  arrivalTime: string;
  basePrice: number;
  isActive: boolean;
}

// Flight Management
export interface AdminFlight {
  flightId: number;
  flightNumber: string;
  airline: string;
  sourceAirportId: number;
  sourceAirportName: string;
  destinationAirportId: number;
  destinationAirportName: string;
  departureTime: string;
  arrivalTime: string;
  basePrice: number;
  isActive: boolean;
  createdAt: Date;
}

export interface CreateFlight {
  flightNumber: string;
  airline: string;
  sourceAirportId: number;
  destinationAirportId: number;
  departureTime: string;
  arrivalTime: string;
  basePrice: number;
}

export interface UpdateFlight {
  airline: string;
  sourceAirportId: number;
  destinationAirportId: number;
  departureTime: string;
  arrivalTime: string;
  basePrice: number;
  isActive: boolean;
}

// Booking Reports
export interface BookingReport {
  bookingId: number;
  bookingReference: string;
  userName: string;
  userEmail: string;
  transportType: string;
  transportNumber: string;
  route: string;
  bookingDate: Date;
  travelDate: Date;
  amount: number;
  status: string;
  paymentStatus: string;
}

// Lookup Data (matching SearchDTOs structure)
export interface Station {
  stationID: number;
  stationName: string;
  city: string;
  state: string;
  stationCode: string;
}

export interface Airport {
  airportID: number;
  airportName: string;
  city: string;
  state: string;
  code: string;
}

// Coach Management
export interface CoachConfig {
  class: string;
  quota: string;
  coachCount: number;
  seatsPerCoach: number;
  priceMultiplier: number;
}

export interface AddCoachesRequest {
  coachConfigs: CoachConfig[];
}

// Station Management
export interface CreateStation {
  stationName: string;
  stationCode: string;
  city: string;
  state: string;
}

export interface UpdateStation {
  stationName: string;
  stationCode: string;
  city: string;
  state: string;
}

// Coach Management
export interface Coach {
  coachId: number;
  trainId: number;
  coachNumber: string;
  trainClassId: number;
  trainClassName: string;
  totalSeats: number;
  availableSeats: number;
  baseFare: number;
  seats: Seat[];
}

export interface CreateCoach {
  coachNumber: string;
  trainClassId: number;
  totalSeats: number;
  baseFare: number;
}

export interface UpdateCoach {
  coachNumber: string;
  trainClassId: number;
  totalSeats: number;
  baseFare: number;
}

export interface Seat {
  seatId: number;
  coachId: number;
  seatNumber: string;
  seatType: string;
  isAvailable: boolean;
  isWindowSeat: boolean;
}