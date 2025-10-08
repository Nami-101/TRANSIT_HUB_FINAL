export interface VerifyEmailRequest {
  email: string;
  otp: string;
}

export interface ResendOtpRequest {
  email: string;
}

export interface VerifyEmailResponse {
  success: boolean;
  message: string;
}