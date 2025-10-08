import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { NotificationService } from '../../../services/notification.service';
import { ApiResponse } from '../../../models/auth.dto';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    RouterModule
  ],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div class="max-w-md w-full space-y-8">
        <div>
          <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Verify Your Email
          </h2>
          <p class="mt-2 text-center text-sm text-gray-600">
            We've sent a 6-digit OTP to
            <span class="font-medium text-indigo-600">{{ email }}</span>
          </p>
        </div>

        <mat-card class="p-6">
          <form [formGroup]="verifyForm" (ngSubmit)="onSubmit()" class="space-y-4">
            
            <!-- OTP Field -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Enter OTP</mat-label>
              <input matInput 
                     type="text" 
                     formControlName="otp"
                     placeholder="Enter 6-digit OTP"
                     maxlength="6"
                     [class.border-red-500]="isFieldInvalid('otp')">
              <mat-icon matSuffix>security</mat-icon>
              <mat-error *ngIf="verifyForm.get('otp')?.hasError('required')">
                OTP is required
              </mat-error>
              <mat-error *ngIf="verifyForm.get('otp')?.hasError('pattern')">
                OTP must be 6 digits
              </mat-error>
            </mat-form-field>

            <!-- Submit Button -->
            <button mat-raised-button 
                    color="primary" 
                    type="submit"
                    class="w-full py-3"
                    [disabled]="verifyForm.invalid || isLoading">
              <mat-spinner *ngIf="isLoading" diameter="20" class="mr-2"></mat-spinner>
              {{ isLoading ? 'Verifying...' : 'Verify Email' }}
            </button>

            <!-- Resend OTP -->
            <div class="text-center mt-4">
              <p class="text-sm text-gray-600">
                Didn't receive the OTP?
                <button type="button" 
                        mat-button 
                        color="primary"
                        [disabled]="resendLoading || resendCooldown > 0"
                        (click)="resendOtp()">
                  <mat-spinner *ngIf="resendLoading" diameter="16" class="mr-1"></mat-spinner>
                  {{ resendCooldown > 0 ? 'Resend in ' + resendCooldown + 's' : 'Resend OTP' }}
                </button>
              </p>
            </div>

          </form>
        </mat-card>

        <div class="text-center">
          <p class="text-sm text-gray-600">
            Wrong email address?
            <a routerLink="/auth/register" class="font-medium text-indigo-600 hover:text-indigo-500">
              Register again
            </a>
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .mat-mdc-form-field {
      width: 100%;
    }
    
    /* Fix text cutoff issues */
    .mat-mdc-text-field-wrapper {
      min-height: 56px;
    }
    
    .mat-mdc-form-field-input-control {
      height: auto;
      line-height: normal;
    }
    
    .mat-mdc-form-field .mat-mdc-form-field-input-control input {
      padding: 16px 0 8px 0;
      line-height: 1.5;
      text-align: center;
      font-size: 1.5rem;
      letter-spacing: 0.5rem;
    }

    :host ::ng-deep .success-snackbar {
      background-color: #4caf50 !important;
      color: white !important;
    }

    :host ::ng-deep .error-snackbar {
      background-color: #f44336 !important;
      color: white !important;
    }

    :host ::ng-deep .info-snackbar {
      background-color: #2196f3 !important;
      color: white !important;
    }

    :host ::ng-deep .warning-snackbar {
      background-color: #ff9800 !important;
      color: white !important;
    }
  `]
})
export class VerifyEmailComponent implements OnInit {
  verifyForm: FormGroup;
  isLoading = false;
  resendLoading = false;
  resendCooldown = 0;
  email = '';

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.verifyForm = this.formBuilder.group({
      otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]]
    });
  }

  ngOnInit(): void {
    // Get email from query params
    this.email = this.route.snapshot.queryParams['email'] || '';
    if (!this.email) {
      this.notificationService.showError('Email parameter is missing. Please register again.');
      this.router.navigate(['/auth/register']);
    }
  }

  onSubmit(): void {
    if (this.verifyForm.valid && this.email) {
      this.isLoading = true;
      const otp = this.verifyForm.get('otp')?.value;

      this.authService.verifyEmail(this.email, otp).subscribe({
        next: (response: ApiResponse) => {
          this.isLoading = false;
          if (response.success) {
            this.notificationService.showSuccess(response.message || 'Email verified successfully!');
            // Redirect to login after 2 seconds
            setTimeout(() => {
              this.router.navigate(['/auth/login']);
            }, 2000);
          } else {
            this.notificationService.showError(response.message || 'Verification failed. Please try again.');
          }
        },
        error: (error: any) => {
          this.isLoading = false;
          const errorMessage = error.error?.message || 'An error occurred during verification. Please try again.';
          this.notificationService.showError(errorMessage);
        }
      });
    }
  }

  resendOtp(): void {
    if (this.email && this.resendCooldown === 0) {
      this.resendLoading = true;

      this.authService.resendOtp(this.email).subscribe({
        next: (response: ApiResponse) => {
          this.resendLoading = false;
          if (response.success) {
            this.notificationService.showSuccess(response.message || 'OTP sent successfully!');
            this.startResendCooldown();
          } else {
            this.notificationService.showError(response.message || 'Failed to resend OTP. Please try again.');
          }
        },
        error: (error: any) => {
          this.resendLoading = false;
          const errorMessage = error.error?.message || 'Failed to resend OTP. Please try again.';
          this.notificationService.showError(errorMessage);
        }
      });
    }
  }

  private startResendCooldown(): void {
    this.resendCooldown = 60; // 60 seconds cooldown
    const interval = setInterval(() => {
      this.resendCooldown--;
      if (this.resendCooldown <= 0) {
        clearInterval(interval);
      }
    }, 1000);
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.verifyForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }
}