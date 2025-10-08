import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatCardModule, MatIconModule],
  template: `
    <div class="home-container">
      <div class="hero-section">
        <h1>Welcome to Transit Hub</h1>
        <p>Your one-stop solution for train and flight bookings</p>
        
      </div>

      <div class="features-section">
        <h2>🚀 Features</h2>
        <div class="features-grid">
          <mat-card class="feature-card">
            <mat-card-header>
              <div class="feature-emoji">🚂</div>
              <mat-card-title>Train Booking</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <p>Book train tickets with real-time availability and seat selection</p>
            </mat-card-content>
          </mat-card>

          <mat-card class="feature-card">
            <mat-card-header>
              <div class="feature-emoji">✈️</div>
              <mat-card-title>Flight Booking</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <p>🔜 COMING SOON</p>
            </mat-card-content>
          </mat-card>

          <mat-card class="feature-card">
            <mat-card-header>
              <div class="feature-emoji">📊</div>
              <mat-card-title>Admin Dashboard</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <p>Comprehensive admin panel for managing trains, stations, and bookings</p>
            </mat-card-content>
          </mat-card>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .home-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 1rem;
    }

    .hero-section {
      text-align: center;
      padding: 3rem 1rem;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border-radius: 12px;
      margin-bottom: 2rem;
    }

    .hero-section h1 {
      font-size: clamp(2rem, 5vw, 3rem);
      margin-bottom: 1rem;
      font-weight: 300;
    }

    .hero-section p {
      font-size: clamp(1rem, 3vw, 1.2rem);
      margin-bottom: 2rem;
      opacity: 0.9;
    }

    .cta-buttons {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
    }

    .cta-buttons a {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .features-section {
      text-align: center;
      padding: 0 1rem;
    }

    .features-section h2 {
      font-size: clamp(1.8rem, 4vw, 2.5rem);
      margin-bottom: 2rem;
      color: #333;
    }

    .features-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 2.5rem;
      margin-top: 3rem;
      padding: 0 1rem;
    }

    .feature-card {
      text-align: left;
      transition: transform 0.3s ease, box-shadow 0.3s ease;
      border-radius: 16px;
      padding: 1rem;
    }

    .feature-card:hover {
      transform: translateY(-8px);
      box-shadow: 0 8px 25px rgba(0,0,0,0.15);
    }

    .feature-emoji {
      font-size: 2.5rem;
      margin-bottom: 0.5rem;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 60px;
      height: 60px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 50%;
      margin-right: 1rem;
    }

    @media (max-width: 768px) {
      .home-container {
        padding: 0.5rem;
      }
      
      .hero-section {
        padding: 2rem 1rem;
        margin-bottom: 1.5rem;
      }
      
      .cta-buttons {
        flex-direction: column;
        align-items: center;
      }
      
      .features-grid {
        grid-template-columns: 1fr;
        gap: 2rem;
        padding: 0;
      }
    }

    @media (max-width: 480px) {
      .features-grid {
        grid-template-columns: 1fr;
      }
      
      .feature-card {
        margin: 0;
      }
    }
  `]
})
export class HomeComponent {}