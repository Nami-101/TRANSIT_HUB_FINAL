import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { AdminService } from '../../../services/admin.service';
import { NotificationService } from '../../../services/notification.service';
import { AdminUser } from '../../../models/admin.dto';

@Component({
  selector: 'app-manage-users',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatCardModule,
    MatChipsModule
  ],
  template: `
    <div class="p-6">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold text-gray-900">Manage Users</h1>
        <mat-form-field appearance="fill" class="w-80">
          <mat-label>Search by email</mat-label>
          <input matInput (keyup)="applyFilter($event)" placeholder="Enter email to search">
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
      </div>

      <!-- Loading Spinner -->
      <div *ngIf="loading" class="flex justify-center items-center h-64">
        <mat-spinner></mat-spinner>
      </div>

      <!-- Users Table -->
      <mat-card *ngIf="!loading">
        <div class="overflow-x-auto">
          <table mat-table [dataSource]="filteredUsers" class="w-full">
            
            <!-- Name Column -->
            <ng-container matColumnDef="name">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Name</th>
              <td mat-cell *matCellDef="let user">{{ user.name }}</td>
            </ng-container>

            <!-- Email Column -->
            <ng-container matColumnDef="email">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Email</th>
              <td mat-cell *matCellDef="let user">{{ user.email }}</td>
            </ng-container>

            <!-- Roles Column -->
            <ng-container matColumnDef="roles">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Roles</th>
              <td mat-cell *matCellDef="let user">
                <mat-chip-set>
                  <mat-chip *ngFor="let role of user.roles" 
                           [class]="role === 'Admin' ? 'bg-red-100 text-red-800' : 'bg-blue-100 text-blue-800'">
                    {{ role }}
                  </mat-chip>
                </mat-chip-set>
              </td>
            </ng-container>

            <!-- Email Status Column -->
            <ng-container matColumnDef="emailStatus">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Email Status</th>
              <td mat-cell *matCellDef="let user">
                <span [class]="user.emailConfirmed ? 'text-green-600 bg-green-100' : 'text-yellow-600 bg-yellow-100'" 
                      class="px-2 py-1 rounded-full text-xs font-medium">
                  {{ user.emailConfirmed ? 'Verified' : 'Pending' }}
                </span>
              </td>
            </ng-container>

            <!-- Status Column -->
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Status</th>
              <td mat-cell *matCellDef="let user">
                <span [class]="user.isActive ? 'text-green-600 bg-green-100' : 'text-red-600 bg-red-100'" 
                      class="px-2 py-1 rounded-full text-xs font-medium">
                  {{ user.isActive ? 'Active' : 'Deactivated' }}
                </span>
              </td>
            </ng-container>

            <!-- Actions Column -->
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Actions</th>
              <td mat-cell *matCellDef="let user">
                <button mat-icon-button 
                        color="warn" 
                        (click)="deleteUser(user.id, user.email)" 
                        matTooltip="Deactivate User"
                        [disabled]="user.roles.includes('Admin')">
                  <mat-icon>block</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </div>

        <!-- No Data Message -->
        <div *ngIf="filteredUsers.length === 0 && !loading" class="text-center py-8 text-gray-500">
          No users found
        </div>
      </mat-card>
    </div>
  `,
  styles: [`
    :host ::ng-deep .mat-mdc-form-field {
      width: 100%;
    }

    :host ::ng-deep .mat-mdc-card {
      border-radius: 12px;
      box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
    }

    :host ::ng-deep .mat-mdc-table {
      background: transparent;
    }

    :host ::ng-deep .mat-mdc-header-cell {
      font-weight: 600;
      color: #374151;
    }
  `]
})
export class ManageUsersComponent implements OnInit {
  users: AdminUser[] = [];
  filteredUsers: AdminUser[] = [];
  displayedColumns: string[] = ['name', 'email', 'roles', 'emailStatus', 'status', 'actions'];
  loading = true;

  constructor(
    private adminService: AdminService,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.adminService.getAllUsers().subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success && response.data) {
          this.users = response.data;
          this.filteredUsers = [...this.users];
        } else {
          this.notificationService.showError('Failed to load users');
        }
      },
      error: (error) => {
        this.loading = false;
        console.error('Error loading users:', error);
        this.notificationService.showError('Failed to load users');
      }
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value.toLowerCase();
    this.filteredUsers = this.users.filter(user => 
      user.email.toLowerCase().includes(filterValue) ||
      user.name.toLowerCase().includes(filterValue)
    );
  }

  deleteUser(userId: string, userEmail: string): void {
    if (confirm(`Are you sure you want to deactivate user: ${userEmail}?`)) {
      this.adminService.deleteUser(userId).subscribe({
        next: (response) => {
          if (response.success) {
            this.notificationService.showSuccess('User deactivated successfully');
            this.loadUsers();
          } else {
            this.notificationService.showError(response.message || 'Failed to deactivate user');
          }
        },
        error: (error) => {
          console.error('Error deactivating user:', error);
          this.notificationService.showError('Failed to deactivate user');
        }
      });
    }
  }
}