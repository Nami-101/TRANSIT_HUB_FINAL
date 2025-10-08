import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, timeout } from 'rxjs';
import { StationDto } from '../models/station.dto';
import { AirportDto } from '../models/airport.dto';
import { LookupDataDto } from '../models/lookup-data.dto';

@Injectable({
  providedIn: 'root'
})
export class LookupService {
  private readonly baseUrl = 'http://localhost:5000/api'; // Backend API URL

  constructor(private http: HttpClient) { }

  fetchStations(): Observable<StationDto[]> {
    return this.http.get<StationDto[]>(`${this.baseUrl}/test/stations`).pipe(
      timeout(10000) // 10 second timeout
    );
  }

  fetchAirports(): Observable<AirportDto[]> {
    return this.http.get<AirportDto[]>(`${this.baseUrl}/test/airports`).pipe(
      timeout(10000) // 10 second timeout
    );
  }

  fetchLookupData(): Observable<LookupDataDto> {
    return this.http.get<LookupDataDto>(`${this.baseUrl}/test/lookup`).pipe(
      timeout(10000) // 10 second timeout
    );
  }
}