import { Component, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './shared/components/navbar/navbar.component';
import { SplashScreenComponent } from './components/splash-screen.component';
import { AuthService } from './services/auth.service';
import { NotificationService } from './services/notification.service';
import { SplashService } from './services/splash.service';
import { Subject, takeUntil } from 'rxjs';
import { MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, NavbarComponent, SplashScreenComponent, MatSnackBarModule],
  template: `
    <app-splash-screen *ngIf="showSplash$ | async"></app-splash-screen>
    <div class="app-container" *ngIf="!(showSplash$ | async)">
      <app-navbar></app-navbar>
      <main class="main-content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styleUrl: './app.css'
})
export class App implements OnInit, OnDestroy {
  title = 'Transit Hub';
  private destroy$ = new Subject<void>();
  showSplash$!: any;

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private splashService: SplashService
  ) {}

  ngOnInit(): void {
    // Initialize splash screen observable
    this.showSplash$ = this.splashService.showSplash$;
    
    // Hide splash screen after delay
    this.splashService.hideSplash();
    
    // Start SignalR connection when user is authenticated
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        if (user) {
          this.notificationService.startConnection();
        } else {
          this.notificationService.stopConnection();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.notificationService.stopConnection();
  }
}
