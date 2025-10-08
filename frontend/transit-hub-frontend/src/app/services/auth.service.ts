import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, RegisterRequest, UserInfo, ApiResponse } from '../models/auth.dto';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'http://localhost:5000/api/auth';
  private currentUserSubject = new BehaviorSubject<UserInfo | null>(null);
  private tokenSubject = new BehaviorSubject<string | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();
  public token$ = this.tokenSubject.asObservable();

  constructor(private http: HttpClient) {
    // Check if user is already logged in
    this.loadStoredAuthData();
  }

  /**
   * User login
   */
  login(credentials: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => {
          if (response.success && response.data && !response.data.requiresRoleSelection) {
            this.handleAuthSuccess(response.data);
          }
        })
      );
  }

  /**
   * Admin login with role selection
   */
  loginWithRole(credentials: LoginRequest & { selectedRole: string }): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/login-with-role`, {
      email: credentials.email,
      password: credentials.password,
      selectedRole: credentials.selectedRole
    }).pipe(
      tap(response => {
        if (response.success && response.data) {
          this.handleAuthSuccess(response.data);
        }
      })
    );
  }

  /**
   * User registration
   */
  register(userDetails: RegisterRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/register`, userDetails);
  }

  /**
   * User logout
   */
  logout(): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/logout`, {})
      .pipe(
        tap(() => {
          this.clearAuthData();
        })
      );
  }

  /**
   * Refresh JWT token
   */
  refreshToken(): Observable<ApiResponse<LoginResponse>> {
    const refreshToken = this.getRefreshToken();
    console.log('Attempting token refresh with:', refreshToken ? 'token present' : 'no token');
    
    if (!refreshToken) {
      console.error('No refresh token available');
      return new Observable(observer => {
        observer.error(new Error('No refresh token available'));
      });
    }
    
    return this.http.post<ApiResponse<LoginResponse>>(`${this.apiUrl}/refresh`, { refreshToken })
      .pipe(
        tap(response => {
          console.log('Refresh token response:', response);
          if (response.success && response.data) {
            this.handleAuthSuccess(response.data);
          }
        })
      );
  }

  /**
   * Get current user info
   */
  getCurrentUser(): Observable<ApiResponse<UserInfo>> {
    return this.http.get<ApiResponse<UserInfo>>(`${this.apiUrl}/me`);
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    // Check if token is expired
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp > currentTime;
    } catch {
      return false;
    }
  }

  /**
   * Check if user has specific role
   */
  hasRole(role: string): boolean {
    const token = this.getToken();
    if (!token || !this.isAuthenticated()) return false;
    
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const userRoles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      
      if (Array.isArray(userRoles)) {
        return userRoles.includes(role);
      } else if (typeof userRoles === 'string') {
        return userRoles === role;
      }
      
      return false;
    } catch {
      return false;
    }
  }

  /**
   * Get current user value
   */
  get currentUserValue(): UserInfo | null {
    return this.currentUserSubject.value;
  }

  /**
   * Get stored token
   */
  getToken(): string | null {
    return localStorage.getItem('jwt_token') || this.tokenSubject.value;
  }

  /**
   * Get stored refresh token
   */
  getRefreshToken(): string | null {
    return localStorage.getItem('refresh_token');
  }

  /**
   * Get current user ID from JWT token
   */
  getCurrentUserId(): string | null {
    const token = this.getToken();
    if (!token || !this.isAuthenticated()) return null;
    
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.sub || payload.id || null;
    } catch {
      return null;
    }
  }

  /**
   * Get current user email from JWT token
   */
  getCurrentUserEmail(): string | null {
    const token = this.getToken();
    if (!token || !this.isAuthenticated()) return null;
    
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const name = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
      const email = payload.email;
      
      // Handle array case
      if (Array.isArray(name)) {
        return name[0] || null;
      }
      if (Array.isArray(email)) {
        return email[0] || null;
      }
      
      return name || email || null;
    } catch {
      return null;
    }
  }

  /**
   * Verify email with OTP
   */
  verifyEmail(email: string, otp: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/verify-email`, { email, otp });
  }

  /**
   * Resend OTP for email verification
   */
  resendOtp(email: string): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/resend-otp`, { email });
  }

  /**
   * Forgot password
   */
  forgotPassword(request: { email: string }): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/forgot-password`, request);
  }

  /**
   * Reset password
   */
  resetPassword(request: { token: string; newPassword: string }): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/reset-password`, request);
  }

  /**
   * Handle successful authentication
   */
  private handleAuthSuccess(authData: LoginResponse): void {
    console.log('Handling auth success with data:', {
      hasToken: !!authData.token,
      hasRefreshToken: !!authData.refreshToken,
      user: authData.user?.email || 'unknown'
    });
    
    // Store tokens
    localStorage.setItem('jwt_token', authData.token);
    localStorage.setItem('refresh_token', authData.refreshToken);
    localStorage.setItem('token_expiry', authData.expiresAt.toString());
    localStorage.setItem('current_user', JSON.stringify(authData.user));

    // Update subjects
    this.tokenSubject.next(authData.token);
    this.currentUserSubject.next(authData.user);
  }

  /**
   * Load stored authentication data
   */
  private loadStoredAuthData(): void {
    const token = localStorage.getItem('jwt_token');
    const userData = localStorage.getItem('current_user');

    if (token && userData && this.isAuthenticated()) {
      this.tokenSubject.next(token);
      this.currentUserSubject.next(JSON.parse(userData));
    } else {
      this.clearAuthData();
    }
  }

  /**
   * Clear authentication data
   */
  private clearAuthData(): void {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('token_expiry');
    localStorage.removeItem('current_user');
    
    this.tokenSubject.next(null);
    this.currentUserSubject.next(null);
  }
}