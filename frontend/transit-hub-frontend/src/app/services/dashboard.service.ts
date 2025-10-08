import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { DashboardMetrics } from '../models/admin.dto';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly apiUrl = 'http://localhost:5000/api/admin';

  constructor(private http: HttpClient) {}

  getDashboardMetrics(): Observable<DashboardMetrics> {
    return this.http.get<{success: boolean, data: DashboardMetrics}>(`${this.apiUrl}/dashboard-metrics`)
      .pipe(
        map(response => response.data)
      );
  }
}