import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

interface ProfileData {
  userID: number;
  name: string;
  email: string;
  phone: string;
  age: number;
  avatarId: number | null;
  isSeniorCitizen: boolean;
  createdAt: string;
}

interface UpdateProfileRequest {
  name: string;
  phone: string;
  age: number;
  avatarId: number | null;
}

interface UpdateProfileResponse {
  success: boolean;
  message: string;
  profile?: ProfileData;
}

interface AvatarOption {
  id: number;
  name: string;
  emoji: string;
  color: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private apiUrl = 'http://localhost:5000/api/profile';

  constructor(private http: HttpClient) {}

  getProfile(): Observable<ProfileData> {
    return this.http.get<ProfileData>(this.apiUrl);
  }

  updateProfile(request: UpdateProfileRequest): Observable<UpdateProfileResponse> {
    return this.http.put<UpdateProfileResponse>(this.apiUrl, request);
  }

  getAvatars(): Observable<AvatarOption[]> {
    return this.http.get<AvatarOption[]>(`${this.apiUrl}/avatars`);
  }
}