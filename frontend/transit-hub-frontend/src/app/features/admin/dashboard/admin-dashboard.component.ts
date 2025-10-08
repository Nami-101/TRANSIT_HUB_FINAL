import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DashboardService } from '../../../services/dashboard.service';
import { NotificationService } from '../../../services/notification.service';
import { DashboardMetrics } from '../../../models/admin.dto';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="admin-dashboard p-4 sm:p-6">
      <div class="dashboard-header mb-6 sm:mb-8">
        <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center space-y-4 sm:space-y-0">
          <div>
            <h1 class="text-2xl sm:text-3xl font-bold text-gray-800">Admin Dashboard</h1>
            <p class="text-sm sm:text-base text-gray-600 mt-1 sm:mt-2">Welcome to Transit Hub Administration</p>
          </div>
          <button mat-raised-button color="primary" (click)="refreshMetrics()" [disabled]="loading" class="w-full sm:w-auto">
            <mat-icon>refresh</mat-icon>
            <span class="ml-2">Refresh</span>
          </button>
        </div>
      </div>

      <div *ngIf="loading" class="flex justify-center py-12">
        <mat-spinner></mat-spinner>
      </div>

      <div *ngIf="!loading" class="dashboard-content">
        <!-- Stats Cards -->
        <div class="stats-grid grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6 mb-6 sm:mb-8">
          <mat-card class="stat-card revenue-card">
            <mat-card-content class="flex items-center justify-between">
              <div>
                <h3 class="text-lg font-semibold text-gray-700">Total Revenue</h3>
                <p class="text-3xl font-bold text-green-600">₹{{ metrics.totalRevenue | number }}</p>
                <p class="text-sm text-gray-500">All time earnings</p>
              </div>
              <div class="text-4xl text-green-500">💰</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card bookings-card">
            <mat-card-content class="flex items-center justify-between">
              <div>
                <h3 class="text-lg font-semibold text-gray-700">Total Bookings</h3>
                <p class="text-3xl font-bold text-blue-600">{{ metrics.totalBookings }}</p>
                <p class="text-sm text-gray-500">{{ metrics.activeBookings }} active</p>
              </div>
              <div class="text-4xl text-blue-500">🎫</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card trains-card">
            <mat-card-content class="flex items-center justify-between">
              <div>
                <h3 class="text-lg font-semibold text-gray-700">Total Trains</h3>
                <p class="text-3xl font-bold text-purple-600">{{ metrics.totalTrains }}</p>
                <p class="text-sm text-gray-500">Available trains</p>
              </div>
              <div class="text-4xl text-purple-500">🚂</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card users-card">
            <mat-card-content class="flex items-center justify-between">
              <div>
                <h3 class="text-lg font-semibold text-gray-700">Total Users</h3>
                <p class="text-3xl font-bold text-orange-600">{{ metrics.totalUsers }}</p>
                <p class="text-sm text-gray-500">Registered users</p>
              </div>
              <div class="text-4xl text-orange-500">👥</div>
            </mat-card-content>
          </mat-card>
        </div>

        <!-- Quick Actions -->
        <div class="quick-actions">
          <h2 class="text-xl sm:text-2xl font-bold text-gray-800 mb-4">Quick Actions</h2>
          <div class="grid grid-cols-2 sm:grid-cols-3 gap-3 sm:gap-4">
            <button mat-raised-button color="primary" routerLink="/admin/trains" class="action-btn">
              <mat-icon>train</mat-icon>
              Manage Trains
            </button>
            <button mat-raised-button color="accent" routerLink="/admin/stations" class="action-btn">
              <mat-icon>location_on</mat-icon>
              Manage Stations
            </button>

            <button mat-raised-button routerLink="/admin/reports" class="action-btn">
              <mat-icon>assessment</mat-icon>
              Reports
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .stat-card {
      transition: transform 0.2s ease-in-out;
      cursor: pointer;
    }
    .stat-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 25px rgba(0,0,0,0.15);
    }
    .action-btn {
      height: 50px;
      display: flex;
      flex-direction: column;
      gap: 4px;
      font-size: 0.875rem;
    }
    
    @media (min-width: 640px) {
      .action-btn {
        height: 60px;
        gap: 8px;
        font-size: 1rem;
      }
    }
    
    @media (max-width: 640px) {
      .stat-card mat-card-content {
        padding: 12px !important;
      }
      
      .stat-card h3 {
        font-size: 0.875rem;
      }
      
      .stat-card p {
        font-size: 1.5rem;
      }
    }
    .revenue-card { border-left: 4px solid #10b981; }
    .bookings-card { border-left: 4px solid #3b82f6; }
    .trains-card { border-left: 4px solid #8b5cf6; }
    .users-card { border-left: 4px solid #f59e0b; }
  `]
})
export class AdminDashboardComponent implements OnInit {
  loading = true;
  metrics: DashboardMetrics = {
    totalUsers: 0,
    totalTrains: 0,
    totalFlights: 0,
    totalBookings: 0,
    totalStations: 0,
    totalAirports: 0,
    totalRevenue: 0,
    activeBookings: 0
  };

  constructor(
    private dashboardService: DashboardService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadMetrics();
  }

  loadMetrics(): void {
    this.loading = true;
    this.dashboardService.getDashboardMetrics().subscribe({
      next: (data) => {
        this.metrics = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard metrics:', error);
        this.notificationService.showError('Failed to load dashboard metrics');
        this.loading = false;
      }
    });
  }

  refreshMetrics(): void {
    this.loadMetrics();
  }
}