import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../services/auth.service';
import { UserInfo, ApiResponse } from '../../models/auth.dto';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class=\"dashboard-container\">
      <h1>Welcome to Transit Hub Dashboard</h1>
      
      <div class=\"dashboard-grid\">
        <mat-card class=\"dashboard-card\">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>person</mat-icon>
              User Profile
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <ng-container *ngIf=\"currentUser$ | async as user; else noUser\">
              <p><strong>Name:</strong> {{ user.firstName }} {{ user.lastName }}</p>
              <p><strong>Email:</strong> {{ user.email }}</p>
              <p><strong>Roles:</strong> {{ user.roles.join(', ') }}</p>
            </ng-container>
            <ng-template #noUser>
              <p>Loading user information...</p>
            </ng-template>
          </mat-card-content>
        </mat-card>

        <mat-card class=\"dashboard-card\">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>confirmation_number</mat-icon>
              Recent Bookings
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>No recent bookings found.</p>
            <p>Start by searching for routes and making your first booking!</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color=\"primary\" (click)=\"navigateToSearch()\">
              <mat-icon>search</mat-icon>
              Find Routes
            </button>
          </mat-card-actions>
        </mat-card>

        <mat-card class=\"dashboard-card\">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>notifications</mat-icon>
              Notifications
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>Welcome to Transit Hub! 🎉</p>
            <p>Your JWT authentication is working perfectly.</p>
            <p>You can now start booking your transit tickets.</p>
          </mat-card-content>
        </mat-card>

        <mat-card class=\"dashboard-card\">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>analytics</mat-icon>
              Quick Stats
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class=\"stats-grid\">
              <div class=\"stat-item\">
                <span class=\"stat-number\">0</span>
                <span class=\"stat-label\">Total Trips</span>
              </div>
              <div class=\"stat-item\">
                <span class=\"stat-number\">0</span>
                <span class=\"stat-label\">Active Bookings</span>
              </div>
              <div class=\"stat-item\">
                <span class=\"stat-number\">₹0</span>
                <span class=\"stat-label\">Total Spent</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 2rem 0;
    }

    h1 {
      color: #333;
      margin-bottom: 2rem;
      text-align: center;
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 1.5rem;
      margin-top: 2rem;
    }

    .dashboard-card {
      height: fit-content;
    }

    .dashboard-card mat-card-header {
      margin-bottom: 1rem;
    }

    .dashboard-card mat-card-title {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .dashboard-card mat-card-content p {
      margin-bottom: 0.5rem;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1rem;
      text-align: center;
    }

    .stat-item {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .stat-number {
      font-size: 1.5rem;
      font-weight: bold;
      color: #3f51b5;
    }

    .stat-label {
      font-size: 0.875rem;
      color: #666;
    }

    @media (max-width: 768px) {
      .dashboard-container {
        padding: 1rem 0;
      }

      .dashboard-grid {
        grid-template-columns: 1fr;
        gap: 1rem;
      }

      .stats-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  currentUser$: Observable<UserInfo | null>;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    // Load current user info from API if needed
    this.authService.getCurrentUser().subscribe({
      next: (response: ApiResponse<UserInfo>) => {
        if (response.success && response.data) {
          console.log('User info loaded:', response.data);
        }
      },
      error: (error: any) => {
        console.error('Failed to load user info:', error);
      }
    });
  }

  navigateToSearch(): void {
    this.router.navigate(['/search']);
  }
}