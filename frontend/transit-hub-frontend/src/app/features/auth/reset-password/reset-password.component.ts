import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { NotificationService } from '../../../services/notification.service';

@Component({
  selector: 'app-reset-password',
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
    RouterModule
  ],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div class="max-w-md w-full space-y-8">
        <div>
          <div class="flex justify-center mb-6">
            <img src="transithub-logo.png.jpeg" alt="Transit Hub" class="h-16 w-auto">
          </div>
          <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Reset Password
          </h2>
          <p class="mt-2 text-center text-sm text-gray-600">
            Enter your new password below.
          </p>
        </div>

        <mat-card class="p-6" *ngIf="!invalidToken">
          <form [formGroup]="resetPasswordForm" (ngSubmit)="onSubmit()" class="space-y-4">
            
            <!-- New Password Field -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>New Password</mat-label>
              <input matInput 
                     [type]="hidePassword ? 'password' : 'text'"
                     formControlName="newPassword"
                     placeholder="Enter your new password"
                     [class.border-red-500]="isFieldInvalid('newPassword')">
              <button mat-icon-button 
                      matSuffix 
                      type="button"
                      (click)="hidePassword = !hidePassword">
                <mat-icon>{{hidePassword ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
              <mat-error *ngIf="resetPasswordForm.get('newPassword')?.hasError('required')">
                Password is required
              </mat-error>
              <mat-error *ngIf="resetPasswordForm.get('newPassword')?.hasError('minlength')">
                Password must be at least 6 characters
              </mat-error>
            </mat-form-field>

            <!-- Confirm Password Field -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Confirm Password</mat-label>
              <input matInput 
                     [type]="hideConfirmPassword ? 'password' : 'text'"
                     formControlName="confirmPassword"
                     placeholder="Confirm your new password"
                     [class.border-red-500]="isFieldInvalid('confirmPassword')">
              <button mat-icon-button 
                      matSuffix 
                      type="button"
                      (click)="hideConfirmPassword = !hideConfirmPassword">
                <mat-icon>{{hideConfirmPassword ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
              <mat-error *ngIf="resetPasswordForm.get('confirmPassword')?.hasError('required')">
                Please confirm your password
              </mat-error>
              <mat-error *ngIf="resetPasswordForm.hasError('passwordMismatch')">
                Passwords do not match
              </mat-error>
            </mat-form-field>

            <!-- Error Message -->
            <div *ngIf="errorMessage" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              {{ errorMessage }}
            </div>

            <!-- Submit Button -->
            <button mat-raised-button 
                    color="primary" 
                    type="submit"
                    class="w-full py-3"
                    [disabled]="resetPasswordForm.invalid || isLoading">
              <mat-spinner *ngIf="isLoading" diameter="20" class="mr-2"></mat-spinner>
              {{ isLoading ? 'Resetting...' : 'Reset Password' }}
            </button>

          </form>
        </mat-card>

        <!-- Invalid Token Message -->
        <mat-card class="p-6" *ngIf="invalidToken">
          <div class="text-center">
            <mat-icon class="text-red-500 text-6xl mb-4">error</mat-icon>
            <h3 class="text-lg font-semibold text-gray-900 mb-2">Invalid or Expired Link</h3>
            <p class="text-gray-600 mb-4">
              This password reset link is invalid or has expired. Please request a new one.
            </p>
            <button mat-raised-button color="primary" routerLink="/auth/forgot-password">
              Request New Link
            </button>
          </div>
        </mat-card>

        <div class="text-center">
          <p class="text-sm text-gray-600">
            Remember your password?
            <a routerLink="/auth/login" class="font-medium text-indigo-600 hover:text-indigo-500">
              Sign in
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
  `]
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: FormGroup;
  hidePassword = true;
  hideConfirmPassword = true;
  isLoading = false;
  errorMessage = '';
  invalidToken = false;
  private resetToken = '';

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private notificationService: NotificationService
  ) {
    this.resetPasswordForm = this.formBuilder.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    this.resetToken = this.route.snapshot.queryParams['token'];
    if (!this.resetToken) {
      this.invalidToken = true;
    }
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.resetPasswordForm.valid && this.resetToken) {
      this.isLoading = true;
      this.errorMessage = '';

      const resetRequest = {
        token: this.resetToken,
        newPassword: this.resetPasswordForm.value.newPassword
      };

      this.authService.resetPassword(resetRequest).subscribe({
        next: (response) => {
          this.isLoading = false;
          if (response.success) {
            this.notificationService.showSuccess('Password reset successfully');
            this.router.navigate(['/auth/login']);
          } else {
            this.errorMessage = response.message || 'Failed to reset password';
          }
        },
        error: (error) => {
          this.isLoading = false;
          if (error.status === 400 && error.error?.message?.includes('Invalid or expired')) {
            this.invalidToken = true;
          } else {
            this.errorMessage = error.error?.message || 'An error occurred. Please try again.';
          }
          this.notificationService.showError('Failed to reset password');
        }
      });
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.resetPasswordForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }
}