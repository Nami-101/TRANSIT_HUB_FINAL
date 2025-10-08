// Login DTOs
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: Date;
  user: UserInfo;
  requiresRoleSelection: boolean;
  availableRoles: string[];
}

// Registration DTOs
export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  phoneNumber?: string;
  dateOfBirth?: Date;
}

// User Information
export interface UserInfo {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

// Generic API Response
export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data?: T;
  errors: string[];
}

// Auth Guard Response
export interface AuthState {
  isAuthenticated: boolean;
  user: UserInfo | null;
  token: string | null;
}