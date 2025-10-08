import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ProfileService } from '../../services/profile.service';
import { AuthService } from '../../services/auth.service';

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

interface AvatarOption {
  id: number;
  name: string;
  emoji: string;
  color: string;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  profile: ProfileData | null = null;
  avatars: AvatarOption[] = [];
  isEditing = false;
  loading = false;
  profileForm: FormGroup;
  selectedAvatarId: number | null = null;

  constructor(
    private profileService: ProfileService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private authService: AuthService
  ) {
    this.profileForm = this.fb.group({
      name: ['', [Validators.required]],
      email: [{value: '', disabled: true}],
      phone: ['', [Validators.pattern(/^[6-9]\d{9}$/)]],
      age: ['', [Validators.min(1), Validators.max(120)]]
    });
  }

  ngOnInit(): void {
    this.loadProfile();
    this.loadAvatars();
  }

  private loadProfile(): void {
    this.profileService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        this.selectedAvatarId = profile.avatarId;
        this.profileForm.patchValue({
          name: profile.name,
          email: profile.email,
          phone: profile.phone,
          age: profile.age
        });
      },
      error: (error) => {
        this.snackBar.open('Failed to load profile', 'Close', { duration: 3000 });
      }
    });
  }

  private loadAvatars(): void {
    this.profileService.getAvatars().subscribe({
      next: (avatars: AvatarOption[]) => {
        this.avatars = avatars;
      },
      error: (error: any) => {
        console.error('Failed to load avatars:', error);
      }
    });
  }

  selectAvatar(avatarId: number): void {
    this.selectedAvatarId = avatarId;
  }

  toggleEdit(): void {
    this.isEditing = !this.isEditing;
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.loadProfile();
  }

  saveProfile(): void {
    if (this.profileForm.valid) {
      this.loading = true;
      const updateData = {
        name: this.profileForm.get('name')?.value,
        phone: this.profileForm.get('phone')?.value,
        age: this.profileForm.get('age')?.value,
        avatarId: this.selectedAvatarId
      };

      this.profileService.updateProfile(updateData).subscribe({
        next: () => {
          this.loading = false;
          this.isEditing = false;
          this.snackBar.open('Profile updated successfully', 'Close', { duration: 3000 });
          this.loadProfile();
        },
        error: (error) => {
          this.loading = false;
          this.snackBar.open('Failed to update profile', 'Close', { duration: 3000 });
        }
      });
    }
  }

  getDisplayAvatar(): string {
    if (this.selectedAvatarId && this.avatars.length > 0) {
      const avatar = this.avatars.find(a => a.id === this.selectedAvatarId);
      return avatar ? avatar.emoji : this.getFirstLetter();
    }
    return this.getFirstLetter();
  }

  getAvatarColor(): string {
    if (this.selectedAvatarId && this.avatars.length > 0) {
      const avatar = this.avatars.find(a => a.id === this.selectedAvatarId);
      return avatar ? avatar.color : '#2196f3';
    }
    return '#2196f3';
  }

  private getFirstLetter(): string {
    return this.profile?.name?.charAt(0).toUpperCase() || 'U';
  }

  getUserRole(): string {
    const currentUser = this.authService.currentUserValue;
    if (!currentUser) return 'User';
    
    if (this.authService.hasRole('Admin')) return 'Admin';
    if (this.authService.hasRole('Moderator')) return 'Moderator';
    return 'User';
  }
}