import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-otp-sending-animation',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="otp-animation-overlay">
      <div class="otp-animation-content">
        <div class="email-animation">
          <mat-icon class="email-icon">email</mat-icon>
          <div class="sending-dots">
            <div class="dot"></div>
            <div class="dot"></div>
            <div class="dot"></div>
          </div>
          <mat-icon class="phone-icon">smartphone</mat-icon>
        </div>
        <h2 class="animation-title">Sending OTP</h2>
        <p class="animation-subtitle">Please wait while we send the verification code to your email</p>
        <div class="progress-bar">
          <div class="progress-fill"></div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .otp-animation-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw;
      height: 100vh;
      background: rgba(0, 0, 0, 0.8);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10000;
      animation: fadeIn 0.3s ease-out;
    }

    .otp-animation-content {
      background: white;
      padding: 3rem 2rem;
      border-radius: 20px;
      text-align: center;
      max-width: 400px;
      width: 90%;
      box-shadow: 0 20px 40px rgba(0,0,0,0.3);
      animation: slideInUp 0.5s ease-out;
    }

    .email-animation {
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 2rem;
      position: relative;
    }

    .email-icon {
      font-size: 3rem;
      width: 3rem;
      height: 3rem;
      color: #667eea;
      animation: pulse 2s infinite;
    }

    .phone-icon {
      font-size: 2.5rem;
      width: 2.5rem;
      height: 2.5rem;
      color: #764ba2;
      animation: bounce 2s infinite;
    }

    .sending-dots {
      display: flex;
      gap: 0.5rem;
      margin: 0 1.5rem;
      align-items: center;
    }

    .dot {
      width: 8px;
      height: 8px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 50%;
      animation: travel 2s infinite;
    }

    .dot:nth-child(2) {
      animation-delay: 0.3s;
    }

    .dot:nth-child(3) {
      animation-delay: 0.6s;
    }

    .animation-title {
      font-size: 1.8rem;
      font-weight: 500;
      color: #333;
      margin: 0 0 0.5rem 0;
      animation: fadeInUp 0.8s ease-out 0.3s both;
    }

    .animation-subtitle {
      color: #666;
      margin: 0 0 2rem 0;
      line-height: 1.5;
      animation: fadeInUp 0.8s ease-out 0.5s both;
    }

    .progress-bar {
      width: 100%;
      height: 4px;
      background: #e0e0e0;
      border-radius: 2px;
      overflow: hidden;
      animation: fadeInUp 0.8s ease-out 0.7s both;
    }

    .progress-fill {
      height: 100%;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 2px;
      animation: progressFill 2.5s ease-out;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes slideInUp {
      from {
        opacity: 0;
        transform: translateY(50px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    @keyframes fadeInUp {
      from {
        opacity: 0;
        transform: translateY(20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    @keyframes pulse {
      0%, 100% {
        transform: scale(1);
        opacity: 1;
      }
      50% {
        transform: scale(1.1);
        opacity: 0.8;
      }
    }

    @keyframes bounce {
      0%, 100% {
        transform: translateY(0);
      }
      50% {
        transform: translateY(-10px);
      }
    }

    @keyframes travel {
      0% {
        transform: translateX(-20px);
        opacity: 0;
      }
      50% {
        opacity: 1;
      }
      100% {
        transform: translateX(20px);
        opacity: 0;
      }
    }

    @keyframes progressFill {
      from {
        width: 0%;
      }
      to {
        width: 100%;
      }
    }

    @media (max-width: 480px) {
      .otp-animation-content {
        padding: 2rem 1.5rem;
      }
      
      .email-icon {
        font-size: 2.5rem;
        width: 2.5rem;
        height: 2.5rem;
      }
      
      .phone-icon {
        font-size: 2rem;
        width: 2rem;
        height: 2rem;
      }
      
      .animation-title {
        font-size: 1.5rem;
      }
    }
  `]
})
export class OtpSendingAnimationComponent implements OnInit {
  @Output() animationComplete = new EventEmitter<void>();

  ngOnInit() {
    // Complete animation after 3 seconds
    setTimeout(() => {
      this.animationComplete.emit();
    }, 3000);
  }
}