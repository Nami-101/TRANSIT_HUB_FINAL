import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatToolbarModule
  ],
  template: `
    <div class="admin-layout h-screen flex">
      <!-- Sidebar -->
      <mat-sidenav-container class="flex-1">
        <mat-sidenav mode="side" opened class="w-64 bg-gray-900 text-white">
          <div class="p-4">
            <h2 class="text-xl font-bold text-white mb-6">Admin Panel</h2>
            
            <mat-nav-list>
              <a mat-list-item routerLink="/admin/dashboard" routerLinkActive="active-nav-item">
                <mat-icon matListItemIcon class="text-white">dashboard</mat-icon>
                <span matListItemTitle>Dashboard</span>
              </a>
              
              <a mat-list-item routerLink="/admin/trains" routerLinkActive="active-nav-item">
                <mat-icon matListItemIcon class="text-white">train</mat-icon>
                <span matListItemTitle>Manage Trains</span>
              </a>
              
              <a mat-list-item routerLink="/admin/stations" routerLinkActive="active-nav-item">
                <mat-icon matListItemIcon class="text-white">location_on</mat-icon>
                <span matListItemTitle>Manage Stations</span>
              </a>
              
              <a mat-list-item routerLink="/admin/flights" routerLinkActive="active-nav-item">
                <mat-icon matListItemIcon class="text-white">flight</mat-icon>
                <span matListItemTitle>Manage Flights</span>
              </a>
              
              <a mat-list-item routerLink="/admin/users" routerLinkActive="active-nav-item">
                <mat-icon matListItemIcon class="text-white">people</mat-icon>
                <span matListItemTitle>Manage Users</span>
              </a>
              
              <a mat-list-item routerLink="/admin/reports" routerLinkActive="active-nav-item">
                <mat-icon matListItemIcon class="text-white">assessment</mat-icon>
                <span matListItemTitle>Reports</span>
              </a>
              
              
            </mat-nav-list>
          </div>
        </mat-sidenav>
        
        <!-- Main Content -->
        <mat-sidenav-content class="bg-gray-50">
          <router-outlet></router-outlet>
        </mat-sidenav-content>
      </mat-sidenav-container>
    </div>
  `,
  styles: [`
    .admin-layout {
      height: 100vh;
    }

    :host ::ng-deep .mat-mdc-list-item {
      color: white !important;
      border-radius: 8px;
      margin: 4px 0;
    }

    :host ::ng-deep .mat-mdc-list-item:hover {
      background-color: rgba(255, 255, 255, 0.1) !important;
    }

    :host ::ng-deep .active-nav-item {
      background-color: rgba(59, 130, 246, 0.5) !important;
      color: white !important;
    }

    :host ::ng-deep .mat-mdc-list-item-icon {
      color: white !important;
    }

    :host ::ng-deep .mat-sidenav {
      border-right: none !important;
    }

    :host ::ng-deep .mat-sidenav-container {
      background-color: transparent !important;
    }
  `]
})
export class AdminLayoutComponent {
  constructor(private router: Router) { }
}