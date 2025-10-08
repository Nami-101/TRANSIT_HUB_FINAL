import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-splash-screen',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <div class="splash-container" [class.fade-out]="fadeOut">
      <div class="splash-content">
        <div class="logo-container">
          <mat-icon class="logo-icon">directions_bus</mat-icon>
          <h1 class="logo-text">Transit Hub</h1>
          <p class="tagline">Your Journey Begins Here</p>
        </div>
        <div class="animation-dots">
          <div class="dot"></div>
          <div class="dot"></div>
          <div class="dot"></div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .splash-container {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 9999;
      opacity: 1;
      transition: opacity 0.5s ease-out;
    }

    .splash-container.fade-out {
      opacity: 0;
      pointer-events: none;
    }

    .splash-content {
      text-align: center;
      color: white;
      animation: fadeInUp 0.8s ease-out;
    }

    .logo-container {
      margin-bottom: 3rem;
    }

    .logo-icon {
      font-size: 4rem;
      width: 4rem;
      height: 4rem;
      margin-bottom: 1rem;
      animation: bounceIn 1.2s ease-out;
    }

    .logo-text {
      font-size: 2.5rem;
      font-weight: 300;
      margin: 0;
      letter-spacing: 2px;
      animation: slideInFromLeft 1s ease-out 0.3s both;
    }

    .tagline {
      font-size: 1.1rem;
      margin: 0.5rem 0 0 0;
      opacity: 0.9;
      font-weight: 300;
      animation: slideInFromRight 1s ease-out 0.6s both;
    }

    .animation-dots {
      display: flex;
      justify-content: center;
      gap: 0.5rem;
      margin-top: 2rem;
    }

    .dot {
      width: 12px;
      height: 12px;
      background: rgba(255, 255, 255, 0.8);
      border-radius: 50%;
      animation: wave 1.5s ease-in-out infinite;
    }

    .dot:nth-child(2) {
      animation-delay: 0.2s;
    }

    .dot:nth-child(3) {
      animation-delay: 0.4s;
    }

    @keyframes fadeInUp {
      from {
        opacity: 0;
        transform: translateY(30px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    @keyframes bounceIn {
      0% {
        opacity: 0;
        transform: scale(0.3) rotate(-180deg);
      }
      50% {
        opacity: 1;
        transform: scale(1.1) rotate(-90deg);
      }
      100% {
        opacity: 1;
        transform: scale(1) rotate(0deg);
      }
    }

    @keyframes slideInFromLeft {
      0% {
        opacity: 0;
        transform: translateX(-100px);
      }
      100% {
        opacity: 1;
        transform: translateX(0);
      }
    }

    @keyframes slideInFromRight {
      0% {
        opacity: 0;
        transform: translateX(100px);
      }
      100% {
        opacity: 1;
        transform: translateX(0);
      }
    }

    @keyframes wave {
      0%, 100% {
        transform: translateY(0) scale(1);
        opacity: 0.8;
      }
      50% {
        transform: translateY(-15px) scale(1.2);
        opacity: 1;
      }
    }

    @media (max-width: 768px) {
      .logo-icon {
        font-size: 3rem;
        width: 3rem;
        height: 3rem;
      }
      
      .logo-text {
        font-size: 2rem;
      }
    }
  `]
})
export class SplashScreenComponent implements OnInit {
  fadeOut = false;

  ngOnInit() {
    setTimeout(() => {
      this.fadeOut = true;
    }, 2500);
  }
}