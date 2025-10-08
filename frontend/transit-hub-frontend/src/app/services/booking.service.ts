import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateBookingRequest {
  scheduleId: number;
  passengerCount: number;
  passengers: PassengerInfo[];
}

export interface PassengerInfo {
  name: string;
  age: number;
  gender: string;
}

export interface BookingResponse {
  success: boolean;
  message: string;
  data?: {
    bookingId: number;
    pnr: string;
    status: string;
    seatAllocations: SeatAllocation[];
  };
}

export interface SeatAllocation {
  passengerId: number;
  passengerName: string;
  coachNumber: string;
  seatNumber: number;
  status: string;
  priority: number;
}

export interface UserBooking {
  bookingId: number;
  bookingReference: string;
  bookingType: string;
  status: string;
  totalAmount: number;
  bookingDate: string;
  passengerCount: number;
  paymentStatus: string;
}

export interface BookingDetails {
  bookingId: number;
  bookingReference: string;
  status: string;
  totalAmount: number;
  paymentStatus: string;
  trainName: string;
  trainNumber: string;
  route: string;
  travelDate: string;
  departureTime: string;
  arrivalTime: string;
  passengers: PassengerDetails[];
  seatNumbers: number[];
  coachNumber?: number;
  waitlistPosition?: number;
  selectedQuota?: string;
}

export interface PassengerDetails {
  name: string;
  age: number;
  gender: string;
  seatNumber?: number;
}

export interface CancelBookingRequest {
  bookingId: number;
  reason?: string;
}

export interface CancelBookingResponse {
  success: boolean;
  message: string;
  refundAmount?: number;
}

export interface BookingRequest {
  TrainId: number;
  ScheduleId: number;
  TravelDate: string;
  Passengers: PassengerInfo[];
  PassengerCount: number;
  AutoAssignSeats: boolean;
  PreferredSeats: number[];
  SelectedQuota: string;
}

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private readonly API_URL = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  // Train booking methods
  createTrainBooking(bookingData: any): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${this.API_URL}/booking/train`, bookingData);
  }

  createFlightBooking(bookingData: any): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${this.API_URL}/booking/flight`, bookingData);
  }

  getUserBookings(userId: number): Observable<UserBooking[]> {
    return this.http.get<UserBooking[]>(`${this.API_URL}/booking/user/${userId}`);
  }

  getBookingDetails(bookingId: number, userId: number): Observable<BookingDetails> {
    return this.http.get<BookingDetails>(`${this.API_URL}/booking/${bookingId}/user/${userId}`);
  }

  cancelBooking(request: CancelBookingRequest): Observable<CancelBookingResponse> {
    return this.http.post<CancelBookingResponse>(`${this.API_URL}/booking/${request.bookingId}/cancel`, request);
  }

  // Train booking specific methods
  createBooking(request: CreateBookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${this.API_URL}/trainbooking/book`, request);
  }

  getMyBookings(): Observable<BookingDetails[]> {
    return this.http.get<BookingDetails[]>(`${this.API_URL}/trainbooking/my-bookings`);
  }

  getBookingById(bookingId: number): Observable<BookingDetails> {
    return this.http.get<BookingDetails>(`${this.API_URL}/trainbooking/${bookingId}`);
  }

  cancelTrainBooking(bookingId: number, reason?: string): Observable<CancelBookingResponse> {
    return this.http.post<CancelBookingResponse>(`${this.API_URL}/trainbooking/cancel`, {
      BookingId: bookingId,
      Reason: reason
    });
  }

  getCoachLayout(trainId: number, travelDate: string): Observable<any> {
    return this.http.get(`${this.API_URL}/trainbooking/coach-layout/${trainId}/${travelDate}`);
  }

  getWaitlistInfo(scheduleId: number): Observable<any> {
    return this.http.get(`${this.API_URL}/trainbooking/waitlist/${scheduleId}`);
  }

  bookTrain(request: BookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${this.API_URL}/trainbooking/book`, request);
  }

  removeBooking(bookingId: number): Observable<any> {
    return this.http.delete(`${this.API_URL}/trainbooking/${bookingId}`);
  }

  removeMultipleBookings(bookingIds: number[]): Observable<any> {
    return this.http.post(`${this.API_URL}/trainbooking/remove-multiple`, { bookingIds });
  }
}