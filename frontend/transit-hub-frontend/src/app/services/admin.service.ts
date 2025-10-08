import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private readonly apiUrl = 'http://localhost:5000/api/admin';

  constructor(private http: HttpClient) {}

  // Real API calls to backend
  getAllFlights(): Observable<any> {
    return this.http.get(`${this.apiUrl}/flights`);
  }
  
  getAllAirports(): Observable<any> {
    return this.http.get(`${this.apiUrl}/airports`);
  }
  
  updateFlight(id: any, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/flights/${id}`, data);
  }
  
  createFlight(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/flights`, data);
  }
  
  deleteFlight(id: any): Observable<any> {
    return this.http.delete(`${this.apiUrl}/flights/${id}`);
  }
  
  getAllStations(): Observable<any> {
    return this.http.get(`${this.apiUrl}/stations`);
  }
  
  updateStation(id: any, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/stations/${id}`, data);
  }
  
  createStation(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/stations`, data);
  }
  
  deleteStation(id: any): Observable<any> {
    return this.http.delete(`${this.apiUrl}/stations/${id}`);
  }
  
  getAllTrains(): Observable<any> {
    return this.http.get(`${this.apiUrl}/trains`);
  }
  
  addCoachToTrain(trainId: any, data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/trains/${trainId}/coaches`, data);
  }
  
  updateTrain(id: any, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/trains/${id}`, data);
  }
  
  createTrain(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/trains`, data);
  }
  
  deleteTrain(id: any): Observable<any> {
    return this.http.delete(`${this.apiUrl}/trains/${id}`);
  }
  
  removeTrain(id: any): Observable<any> {
    return this.http.delete(`${this.apiUrl}/trains/${id}/remove`);
  }
  
  getAllUsers(): Observable<any> {
    return this.http.get(`${this.apiUrl}/users`);
  }
  
  deleteUser(id: any): Observable<any> {
    return this.http.delete(`${this.apiUrl}/users/${id}`);
  }
  
  getBookingReports(): Observable<any> {
    return this.http.get(`${this.apiUrl}/reports/bookings`);
  }
}