import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { Observable, Subject, interval } from 'rxjs';
import { map, takeUntil, startWith } from 'rxjs/operators';
import { AuthService } from '../../../services/auth.service';
import { NotificationService } from '../../../services/notification.service';
import { NotificationBellComponent } from '../../../components/notification-bell.component';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatSidenavModule,
    NotificationBellComponent
  ],
  template: `
    <mat-toolbar color="primary" class="navbar">
      <div class="navbar-container">
        <div class="navbar-brand">
          <a routerLink="/" class="brand-link">
            <mat-icon>directions_bus</mat-icon>
            <span class="brand-text">Transit Hub</span>
          </a>
        </div>

        <!-- Desktop Navigation -->
        <div class="navbar-nav desktop-nav">
          <ng-container *ngIf="isAuthenticated$ | async; else guestNav">
            <a routerLink="/search" mat-button routerLinkActive="active-link">
              <mat-icon>search</mat-icon>
              <span class="nav-text">Search</span>
            </a>
            <a routerLink="/bookings" mat-button routerLinkActive="active-link" *ngIf="!(isAdmin$ | async)">
              <mat-icon>confirmation_number</mat-icon>
              <span class="nav-text">Bookings</span>
            </a>
            <a routerLink="/admin" mat-button routerLinkActive="active-link" *ngIf="isAdmin$ | async">
              <mat-icon>admin_panel_settings</mat-icon>
              <span class="nav-text">Admin</span>
            </a>
            <app-notification-bell></app-notification-bell>
            <button mat-button (click)="logout()" *ngIf="isAdmin$ | async">
              <mat-icon>logout</mat-icon>
              <span class="nav-text">Logout</span>
            </button>
            <ng-container *ngIf="!(isAdmin$ | async)">
              <button mat-button [matMenuTriggerFor]="userMenu">
                <mat-icon>account_circle</mat-icon>
                <span class="nav-text">Account</span>
                <mat-icon>arrow_drop_down</mat-icon>
              </button>
              <mat-menu #userMenu="matMenu">
                <button mat-menu-item routerLink="/profile">
                  <mat-icon>person</mat-icon>
                  <span>Profile</span>
                </button>
                <button mat-menu-item (click)="logout()">
                  <mat-icon>logout</mat-icon>
                  <span>Logout</span>
                </button>
              </mat-menu>
            </ng-container>
          </ng-container>
          <ng-template #guestNav>
            <a routerLink="/auth/login" mat-button routerLinkActive="active-link">
              <mat-icon>login</mat-icon>
              <span class="nav-text">Login</span>
            </a>
            <a routerLink="/auth/register" mat-raised-button color="accent" routerLinkActive="active-link">
              <mat-icon>person_add</mat-icon>
              <span class="nav-text">Sign Up</span>
            </a>
          </ng-template>
        </div>

        <!-- Mobile Actions -->
        <div class="mobile-actions">
          <app-notification-bell *ngIf="isAuthenticated$ | async" class="mobile-notification"></app-notification-bell>
          <button mat-icon-button class="mobile-menu-btn" [matMenuTriggerFor]="mobileMenu">
            <mat-icon>menu</mat-icon>
          </button>
        </div>
        
        <!-- Mobile Menu -->
        <mat-menu #mobileMenu="matMenu" class="mobile-menu">
          <ng-container *ngIf="isAuthenticated$ | async; else mobileGuestNav">
            <a mat-menu-item routerLink="/search">
              <mat-icon>search</mat-icon>
              <span>Search</span>
            </a>
            <a mat-menu-item routerLink="/bookings" *ngIf="!(isAdmin$ | async)">
              <mat-icon>confirmation_number</mat-icon>
              <span>My Bookings</span>
            </a>
            <a mat-menu-item routerLink="/admin" *ngIf="isAdmin$ | async">
              <mat-icon>admin_panel_settings</mat-icon>
              <span>Admin</span>
            </a>
            <a mat-menu-item routerLink="/profile" *ngIf="!(isAdmin$ | async)">
              <mat-icon>person</mat-icon>
              <span>Profile</span>
            </a>
            <button mat-menu-item (click)="logout()">
              <mat-icon>logout</mat-icon>
              <span>Logout</span>
            </button>
          </ng-container>
          <ng-template #mobileGuestNav>
            <a mat-menu-item routerLink="/auth/login">
              <mat-icon>login</mat-icon>
              <span>Login</span>
            </a>
            <a mat-menu-item routerLink="/auth/register">
              <mat-icon>person_add</mat-icon>
              <span>Sign Up</span>
            </a>
          </ng-template>
        </mat-menu>
      </div>
    </mat-toolbar>
  `,
  styles: [`
    .navbar {
      position: sticky;
      top: 0;
      z-index: 1000;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .navbar-container {
      display: flex;
      justify-content: space-between;
      align-items: center;
      width: 100%;
      max-width: 1200px;
      margin: 0 auto;
      padding: 0 1rem;
    }

    .navbar-brand {
      display: flex;
      align-items: center;
    }

    .brand-link {
      display: flex;
      align-items: center;
      text-decoration: none;
      color: inherit;
      font-size: 1.25rem;
      font-weight: 500;
    }

    .brand-text {
      margin-left: 0.5rem;
    }

    .navbar-nav {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .navbar-nav a {
      display: flex;
      align-items: center;
      gap: 0.25rem;
      text-decoration: none;
    }

    .navbar-nav button {
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }

    .active-link {
      background-color: rgba(255, 255, 255, 0.1) !important;
    }

    .mobile-actions {
      display: none;
      align-items: center;
      gap: 0.5rem;
    }

    .mobile-menu-btn {
      display: none;
    }

    .mobile-notification {
      display: none;
    }

    .mobile-menu {
      margin-top: 8px;
    }
    
    .mobile-menu .mat-mdc-menu-item {
      display: flex;
      align-items: center;
      gap: 12px;
      min-height: 48px;
    }

    @media (max-width: 768px) {
      .navbar-container {
        padding: 0 0.5rem;
      }
      
      .desktop-nav {
        display: none;
      }

      .mobile-actions {
        display: flex;
      }

      .mobile-menu-btn {
        display: flex;
      }

      .mobile-notification {
        display: block;
      }

      .brand-text {
        display: none;
      }
    }



    @media (max-width: 480px) {
      .nav-text {
        display: none;
      }
    }
  `]
})
export class NavbarComponent implements OnInit, OnDestroy {
  isAuthenticated$: Observable<boolean>;
  isAdmin$: Observable<boolean>;
  private destroy$ = new Subject<void>();
  isMobileMenuOpen = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {
    // Use the auth service observables directly
    this.isAuthenticated$ = this.authService.currentUser$.pipe(
      map(user => !!user),
      takeUntil(this.destroy$)
    );

    // Check admin status based on current user
    this.isAdmin$ = this.authService.currentUser$.pipe(
      map(user => user ? this.authService.hasRole('Admin') : false),
      takeUntil(this.destroy$)
    );
  }

  ngOnInit(): void {
    // Load notifications when user is authenticated
    this.isAuthenticated$.subscribe(isAuth => {
      if (isAuth) {
        this.loadNotifications();
      }
    });
  }

  private loadNotifications(): void {
    // Small delay to ensure auth is fully loaded
    setTimeout(() => {
      this.notificationService.startConnection();
      this.notificationService.loadNotifications();
      this.notificationService.loadUnreadCount();
    }, 1000);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }



  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/auth/login']);
      },
      error: (error) => {
        console.error('Logout error:', error);
        this.router.navigate(['/auth/login']);
      }
    });
  }
}