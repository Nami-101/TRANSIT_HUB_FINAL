import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LoginRequest, ApiResponse, LoginResponse } from '../../../models/auth.dto';
import { Inject } from '@angular/core';

@Component({
  selector: 'app-login',
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
    MatDialogModule,
    MatSelectModule,
    RouterModule
  ],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 py-6 sm:py-12 px-4 sm:px-6 lg:px-8">
      <div class="max-w-md w-full space-y-6 sm:space-y-8">
        <div>
          <div class="flex justify-center mb-4 sm:mb-6">
            <img src="transithub-logo.png.jpeg" alt="Transit Hub" class="h-12 sm:h-16 w-auto">
          </div>
          <h2 class="mt-4 sm:mt-6 text-center text-2xl sm:text-3xl font-extrabold text-gray-900">
            Sign in to Transit-Hub
          </h2>
          <p class="mt-2 text-center text-sm text-gray-600">
            Or
            <a routerLink="/auth/register" class="font-medium text-indigo-600 hover:text-indigo-500">
              create a new account
            </a>
          </p>
        </div>

        <mat-card class="p-4 sm:p-6">
          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="space-y-4">
            
            <!-- Email Field -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Email</mat-label>
              <input matInput 
                     type="email" 
                     formControlName="email"
                     placeholder="Enter your email"
                     [class.border-red-500]="isFieldInvalid('email')">
              <mat-icon matSuffix>email</mat-icon>
              <mat-error *ngIf="loginForm.get('email')?.hasError('required')">
                Email is required
              </mat-error>
              <mat-error *ngIf="loginForm.get('email')?.hasError('email')">
                Please enter a valid email
              </mat-error>
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
                      (click)="hidePassword = !hidePassword"
                      [attr.aria-label]="'Hide password'"
                      [attr.aria-pressed]="hidePassword">
                <mat-icon>{{hidePassword ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
              <mat-error *ngIf="loginForm.get('password')?.hasError('required')">
                Password is required
              </mat-error>
              <mat-error *ngIf="loginForm.get('password')?.hasError('minlength')">
                Password must be at least 6 characters
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
                    class="w-full py-2 sm:py-3 text-sm sm:text-base"
                    [disabled]="loginForm.invalid || isLoading">
              <mat-spinner *ngIf="isLoading" diameter="20" class="mr-2"></mat-spinner>
              {{ isLoading ? 'Signing in...' : 'Sign In' }}
            </button>

            <!-- Forgot Password Link -->
            <div class="text-center mt-4">
              <a routerLink="/auth/forgot-password" class="text-sm text-indigo-600 hover:text-indigo-500">
                Forgot your password?
              </a>
            </div>

          </form>
        </mat-card>

        <div class="text-center">
          <p class="text-sm text-gray-600">
            Don't have an account?
            <a routerLink="/auth/register" class="font-medium text-indigo-600 hover:text-indigo-500">
              Sign up now
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
    
    /* Hide duplicate password visibility icons */
    .mat-mdc-form-field input[type="password"]::-ms-reveal,
    .mat-mdc-form-field input[type="password"]::-webkit-credentials-auto-fill-button {
      display: none !important;
    }
    
    .mat-mdc-form-field input[type="text"]::-ms-reveal,
    .mat-mdc-form-field input[type="text"]::-webkit-credentials-auto-fill-button {
      display: none !important;
    }
    
    @media (max-width: 640px) {
      .mat-mdc-form-field {
        margin-bottom: 8px;
      }
      
      .mat-mdc-text-field-wrapper {
        min-height: 48px;
      }
    }
  `]
})
export class LoginComponent {
  loginForm: FormGroup;
  hidePassword = true;
  isLoading = false;
  errorMessage = '';

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private dialog: MatDialog
  ) {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      const loginRequest: LoginRequest = this.loginForm.value;

      this.authService.login(loginRequest).subscribe({
        next: (response: ApiResponse<LoginResponse>) => {
          this.isLoading = false;
          if (response.success && response.data) {
            if (response.data.requiresRoleSelection) {
              // Show role selection dialog for admin
              this.showRoleSelectionDialog(loginRequest, response.data.availableRoles);
            } else {
              // Regular user login - redirect
              const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/search';
              this.router.navigate([returnUrl]);
            }
          } else {
            this.errorMessage = response.message || 'Login failed. Please try again.';
          }
        },
        error: (error: any) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'An error occurred during login. Please try again.';
          console.error('Login error:', error);
        }
      });
    }
  }

  private showRoleSelectionDialog(credentials: LoginRequest, availableRoles: string[]): void {
    const dialogRef = this.dialog.open(RoleSelectionDialogComponent, {
      width: '400px',
      disableClose: true,
      data: { roles: availableRoles }
    });

    dialogRef.afterClosed().subscribe(selectedRole => {
      if (selectedRole) {
        this.loginWithSelectedRole(credentials, selectedRole);
      }
    });
  }

  private loginWithSelectedRole(credentials: LoginRequest, selectedRole: string): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.loginWithRole({ ...credentials, selectedRole }).subscribe({
      next: (response: ApiResponse<LoginResponse>) => {
        this.isLoading = false;
        if (response.success) {
          // Redirect based on selected role
          const returnUrl = selectedRole === 'Admin' ? '/admin' : '/search';
          this.router.navigate([returnUrl]);
        } else {
          this.errorMessage = response.message || 'Login failed. Please try again.';
        }
      },
      error: (error: any) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'An error occurred during login. Please try again.';
        console.error('Role-based login error:', error);
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }
}

@Component({
  selector: 'role-selection-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatDialogModule
  ],
  template: `
    <div class="p-6">
      <h2 class="text-xl font-bold mb-4">Select Your Role</h2>
      <p class="text-gray-600 mb-6">Choose how you want to access the platform:</p>
      
      <form [formGroup]="roleForm" (ngSubmit)="onConfirm()">
        <mat-form-field appearance="fill" class="w-full mb-4">
          <mat-label>Select Role</mat-label>
          <mat-select formControlName="selectedRole">
            <mat-option *ngFor="let role of data.roles" [value]="role">
              {{ role === 'Admin' ? 'Admin - Manage Platform' : 'User - Book Tickets' }}
            </mat-option>
          </mat-select>
        </mat-form-field>
        
        <div class="flex justify-end gap-3">
          <button mat-button type="button" (click)="onCancel()">Cancel</button>
          <button mat-raised-button color="primary" type="submit" [disabled]="roleForm.invalid">
            Continue
          </button>
        </div>
      </form>
    </div>
  `
})
export class RoleSelectionDialogComponent {
  roleForm: FormGroup;

  constructor(
    private dialogRef: MatDialogRef<RoleSelectionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { roles: string[] },
    private formBuilder: FormBuilder
  ) {
    this.roleForm = this.formBuilder.group({
      selectedRole: ['', Validators.required]
    });
  }

  onConfirm(): void {
    if (this.roleForm.valid) {
      this.dialogRef.close(this.roleForm.value.selectedRole);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}