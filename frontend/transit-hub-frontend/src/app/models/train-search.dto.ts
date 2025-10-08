export interface TrainSearchDto {
  sourceStation: string;
  destinationStation: string;
  travelDate: Date;
  trainClass?: string; // 2S, SL, 3A, FC, etc.
  quota?: string; // General, Tatkal, Ladies, Senior Citizen, etc.
  passengerCount: number;
}

export interface TrainSearchResultDto {
  trainID: number;
  trainName: string;
  trainNumber: string;
  sourceStation: string;
  sourceStationCode: string;
  destinationStation: string;
  destinationStationCode: string;
  travelDate: string;
  departureTime: string;
  arrivalTime: string;
  journeyTimeMinutes: number;
  scheduleID: number;
  trainClass: string;
  totalSeats: number;
  availableSeats: number;
  fare: number;
  availabilityStatus: string;
  availableOrWaitlistPosition: number;
}

export interface TrainClassAvailabilityDto {
  scheduleID: number;
  trainClass: string;
  totalSeats: number;
  availableSeats: number;
  fare: number;
  availabilityStatus: string; // Available, Limited, Waitlist
  availableOrWaitlistPosition: number;
}