import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { RegisterRequest, ApiResponse } from '../../../models/auth.dto';
import { OtpSendingAnimationComponent } from '../../../components/otp-sending-animation.component';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-register',
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
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule,
    RouterModule,
    OtpSendingAnimationComponent
  ],
  template: `
    <app-otp-sending-animation 
      *ngIf="showOtpAnimation" 
      (animationComplete)="onOtpAnimationComplete()">
    </app-otp-sending-animation>
    
    <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div class="max-w-md w-full space-y-8">
        <div>
          <div class="flex justify-center mb-6">
            <img src="transithub-logo.png.jpeg" alt="Transit Hub" class="h-16 w-auto">
          </div>
          <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Create your Transit-Hub account
          </h2>
          <p class="mt-2 text-center text-sm text-gray-600">
            Or
            <a routerLink="/auth/login" class="font-medium text-indigo-600 hover:text-indigo-500">
              sign in to your existing account
            </a>
          </p>
        </div>

        <mat-card class="p-6">
          <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="space-y-4">
            
            <!-- Name Fields -->
            <div class="flex space-x-4">
              <mat-form-field appearance="fill" class="flex-1">
                <mat-label>First Name</mat-label>
                <input matInput 
                       formControlName="firstName"
                       placeholder="First name"
                       [class.border-red-500]="isFieldInvalid('firstName')">
                <mat-error *ngIf="registerForm.get('firstName')?.hasError('required')">
                  First name is required
                </mat-error>
              </mat-form-field>

              <mat-form-field appearance="fill" class="flex-1">
                <mat-label>Last Name</mat-label>
                <input matInput 
                       formControlName="lastName"
                       placeholder="Last name"
                       [class.border-red-500]="isFieldInvalid('lastName')">
                <mat-error *ngIf="registerForm.get('lastName')?.hasError('required')">
                  Last name is required
                </mat-error>
              </mat-form-field>
            </div>

            <!-- Email Field -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Email</mat-label>
              <input matInput 
                     type="email" 
                     formControlName="email"
                     placeholder="Enter your email"
                     [class.border-red-500]="isFieldInvalid('email')">
              <mat-icon matSuffix>email</mat-icon>
              <mat-error *ngIf="registerForm.get('email')?.hasError('required')">
                Email is required
              </mat-error>
              <mat-error *ngIf="registerForm.get('email')?.hasError('email')">
                Please enter a valid email
              </mat-error>
            </mat-form-field>

            <!-- Phone Field -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Phone Number (Optional)</mat-label>
              <input matInput 
                     type="tel" 
                     formControlName="phoneNumber"
                     placeholder="Enter your phone number">
              <mat-icon matSuffix>phone</mat-icon>
            </mat-form-field>

            <!-- Date of Birth -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Date of Birth (Optional)</mat-label>
              <input matInput 
                     [matDatepicker]="picker"
                     formControlName="dateOfBirth">
              <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
              <mat-datepicker #picker></mat-datepicker>
            </mat-form-field>

            <!-- Password Field -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Password</mat-label>
              <input matInput 
                     [type]="hidePassword ? 'password' : 'text'"
                     formControlName="password"
                     placeholder="Enter your password"
                     [class.border-red-500]="isFieldInvalid('password')">
              <button mat-icon-button 
                      matSuffix 
                      type="button"
                      (click)="hidePassword = !hidePassword">
                <mat-icon>{{hidePassword ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
              <mat-error *ngIf="registerForm.get('password')?.hasError('required')">
                Password is required
              </mat-error>
              <mat-error *ngIf="registerForm.get('password')?.hasError('minlength')">
                Password must be at least 6 characters
              </mat-error>
              <mat-error *ngIf="registerForm.get('password')?.hasError('pattern')">
                Password must contain uppercase, lowercase, number and special character
              </mat-error>
            </mat-form-field>

            <!-- Confirm Password Field -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Confirm Password</mat-label>
              <input matInput 
                     [type]="hideConfirmPassword ? 'password' : 'text'"
                     formControlName="confirmPassword"
                     placeholder="Confirm your password"
                     [class.border-red-500]="isFieldInvalid('confirmPassword')">
              <button mat-icon-button 
                      matSuffix 
                      type="button"
                      (click)="hideConfirmPassword = !hideConfirmPassword">
                <mat-icon>{{hideConfirmPassword ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
              <mat-error *ngIf="registerForm.get('confirmPassword')?.hasError('required')">
                Please confirm your password
              </mat-error>
              <mat-error *ngIf="registerForm.get('confirmPassword')?.hasError('mismatch')">
                Passwords do not match
              </mat-error>
            </mat-form-field>

            <!-- Error Message -->
            <div *ngIf="errorMessage" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              {{ errorMessage }}
            </div>

            <!-- Success Message -->
            <div *ngIf="successMessage" class="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded">
              {{ successMessage }}
            </div>

            <!-- Submit Button -->
            <button mat-raised-button 
                    color="primary" 
                    type="submit"
                    class="w-full py-3"
                    [disabled]="registerForm.invalid || isLoading">
              <mat-spinner *ngIf="isLoading" diameter="20" class="mr-2"></mat-spinner>
              {{ isLoading ? 'Creating account...' : 'Create Account' }}
            </button>

          </form>
        </mat-card>
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
    }
    
    /* Ensure proper spacing for flexbox */
    .flex.space-x-4 .mat-mdc-form-field {
      margin-right: 1rem;
    }
    
    .flex.space-x-4 .mat-mdc-form-field:last-child {
      margin-right: 0;
    }
    
    /* Hide duplicate password visibility icons */
    .mat-mdc-form-field input[type="password"]::-ms-reveal,
    .mat-mdc-form-field input[type="password"]::-webkit-credentials-auto-fill-button {
      display: none !important;
    }
    
    .mat-mdc-form-field input[type="text"]::-ms-reveal,
    .mat-mdc-form-field input[type="text"]::-webkit-credentials-auto-fill-button {
      display: none !important;
    }
  `]
})
export class RegisterComponent {
  registerForm: FormGroup;
  hidePassword = true;
  hideConfirmPassword = true;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  showOtpAnimation = false;
  pendingEmail = '';

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) {
    this.registerForm = this.formBuilder.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      dateOfBirth: [''],
      password: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(100),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/)
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ mismatch: true });
      return { mismatch: true };
    }
    
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      // Prepare the request data with proper type conversion
      const formValue = this.registerForm.value;
      const registerRequest: RegisterRequest = {
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        email: formValue.email,
        password: formValue.password,
        confirmPassword: formValue.confirmPassword,
        phoneNumber: formValue.phoneNumber || undefined,
        dateOfBirth: formValue.dateOfBirth ? new Date(formValue.dateOfBirth) : undefined
      };

      console.log('Sending registration request:', registerRequest);

      this.authService.register(registerRequest).subscribe({
        next: (response: ApiResponse) => {
          this.isLoading = false;
          console.log('Registration response:', response);
          if (response.success) {
            this.successMessage = 'Account created successfully! Sending verification code...';
            this.pendingEmail = registerRequest.email;
            // Show OTP animation
            setTimeout(() => {
              this.showOtpAnimation = true;
            }, 1000);
          } else {
            this.errorMessage = response.message || 'Registration failed. Please try again.';
            if (response.errors && response.errors.length > 0) {
              this.errorMessage += ' Errors: ' + response.errors.join(', ');
            }
          }
        },
        error: (error: any) => {
          this.isLoading = false;
          console.error('Registration error:', error);
          console.error('Error status:', error.status);
          console.error('Error message:', error.error);
          
          // Check for duplicate email based on status code first
          if (error.status === 400 || error.status === 409) {
            this.errorMessage = 'This email is already registered. Please use a different email or try logging in.';
            return;
          }
          
          let errorMsg = '';
          if (error.error?.errors && Array.isArray(error.error.errors)) {
            errorMsg = error.error.errors.join(', ');
          } else if (error.error?.message) {
            errorMsg = error.error.message;
          } else if (error.error && typeof error.error === 'string') {
            errorMsg = error.error;
          } else {
            errorMsg = 'An error occurred during registration. Please try again.';
          }
          
          this.errorMessage = errorMsg || 'Registration failed. Please try again.';
        }
      });
    } else {
      // Mark all fields as touched to show validation errors
      Object.keys(this.registerForm.controls).forEach(key => {
        this.registerForm.get(key)?.markAsTouched();
      });
      this.errorMessage = 'Please fill all required fields correctly.';
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  onOtpAnimationComplete(): void {
    this.showOtpAnimation = false;
    this.router.navigate(['/auth/verify-email'], { queryParams: { email: this.pendingEmail } });
  }
}