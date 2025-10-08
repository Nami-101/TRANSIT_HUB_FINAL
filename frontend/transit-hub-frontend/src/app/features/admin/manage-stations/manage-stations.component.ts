import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { AdminService } from '../../../services/admin.service';
import { NotificationService } from '../../../services/notification.service';
import { Station, CreateStation, UpdateStation } from '../../../models/admin.dto';
import { ConfirmDialogComponent } from '../../../components/confirm-dialog.component';

@Component({
  selector: 'app-manage-stations',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatDialogModule
  ],
  template: `
    <div class="p-6">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold text-gray-900">🚉 Manage Stations</h1>
        <button mat-raised-button color="primary" (click)="openCreateDialog()">
          <mat-icon>add</mat-icon>
          Add Station
        </button>
      </div>

      <div *ngIf="loading" class="flex justify-center items-center h-64">
        <mat-spinner></mat-spinner>
      </div>

      <mat-card *ngIf="!loading">
        <table mat-table [dataSource]="stations" class="w-full">
          <ng-container matColumnDef="stationName">
            <th mat-header-cell *matHeaderCellDef>Station Name</th>
            <td mat-cell *matCellDef="let station">{{ station.stationName }}</td>
          </ng-container>
          <ng-container matColumnDef="stationCode">
            <th mat-header-cell *matHeaderCellDef>Code</th>
            <td mat-cell *matCellDef="let station">{{ station.stationCode }}</td>
          </ng-container>
          <ng-container matColumnDef="city">
            <th mat-header-cell *matHeaderCellDef>City</th>
            <td mat-cell *matCellDef="let station">{{ station.city }}</td>
          </ng-container>
          <ng-container matColumnDef="state">
            <th mat-header-cell *matHeaderCellDef>State</th>
            <td mat-cell *matCellDef="let station">{{ station.state }}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let station">
              <button mat-icon-button color="primary" (click)="openEditDialog(station)">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="deleteStation(station.stationID)">
                <mat-icon>delete</mat-icon>
              </button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </mat-card>

      <!-- Station Dialog -->
      <div *ngIf="showDialog" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
        <div class="bg-white rounded-lg w-full max-w-md p-6">
          <h2 class="text-xl font-bold mb-4">{{ isEditMode ? 'Edit Station' : 'Add Station' }}</h2>
          
          <form [formGroup]="stationForm" (ngSubmit)="saveStation()" class="space-y-4">
            <mat-form-field appearance="outline" class="w-full">
              <mat-label>Station Name</mat-label>
              <input matInput formControlName="stationName" placeholder="New Delhi Railway Station">
              <mat-error>Required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="w-full">
              <mat-label>Station Code</mat-label>
              <input matInput formControlName="stationCode" placeholder="NDLS">
              <mat-error>Required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="w-full">
              <mat-label>City</mat-label>
              <input matInput formControlName="city" placeholder="New Delhi">
              <mat-error>Required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="w-full">
              <mat-label>State</mat-label>
              <input matInput formControlName="state" placeholder="Delhi">
              <mat-error>Required</mat-error>
            </mat-form-field>

            <div class="flex justify-end space-x-3 pt-4">
              <button type="button" mat-button (click)="closeDialog()">Cancel</button>
              <button type="submit" mat-raised-button color="primary" [disabled]="stationForm.invalid || saving">
                {{ saving ? 'Saving...' : (isEditMode ? 'Update' : 'Create') }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `
})
export class ManageStationsComponent implements OnInit {
  stations: Station[] = [];
  displayedColumns = ['stationName', 'stationCode', 'city', 'state', 'actions'];
  loading = true;
  saving = false;
  showDialog = false;
  isEditMode = false;
  currentStationId: number | null = null;

  stationForm: FormGroup;

  constructor(
    private adminService: AdminService,
    private notificationService: NotificationService,
    private formBuilder: FormBuilder,
    private dialog: MatDialog
  ) {
    this.stationForm = this.formBuilder.group({
      stationName: ['', [Validators.required]],
      stationCode: ['', [Validators.required]],
      city: ['', [Validators.required]],
      state: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadStations();
  }

  loadStations(): void {
    this.loading = true;
    this.adminService.getAllStations().subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success && response.data) {
          this.stations = response.data;
        }
      },
      error: (error) => {
        this.loading = false;
        this.notificationService.showError('Failed to load stations');
      }
    });
  }

  openCreateDialog(): void {
    this.isEditMode = false;
    this.currentStationId = null;
    this.stationForm.reset();
    this.showDialog = true;
  }

  openEditDialog(station: Station): void {
    this.isEditMode = true;
    this.currentStationId = station.stationID;
    this.stationForm.patchValue({
      stationName: station.stationName,
      stationCode: station.stationCode,
      city: station.city,
      state: station.state
    });
    this.showDialog = true;
  }

  closeDialog(): void {
    this.showDialog = false;
    this.stationForm.reset();
  }

  saveStation(): void {
    if (this.stationForm.invalid) return;

    this.saving = true;
    const formValue = this.stationForm.value;

    if (this.isEditMode && this.currentStationId) {
      this.adminService.updateStation(this.currentStationId, formValue).subscribe({
        next: (response) => {
          this.saving = false;
          if (response.success) {
            this.notificationService.showSuccess('Station updated successfully');
            this.closeDialog();
            this.loadStations();
          }
        },
        error: () => {
          this.saving = false;
          this.notificationService.showError('Failed to update station');
        }
      });
    } else {
      this.adminService.createStation(formValue).subscribe({
        next: (response) => {
          this.saving = false;
          if (response.success) {
            this.notificationService.showSuccess('Station created successfully');
            this.closeDialog();
            this.loadStations();
          }
        },
        error: () => {
          this.saving = false;
          this.notificationService.showError('Failed to create station');
        }
      });
    }
  }

  deleteStation(stationId: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Station',
        message: 'Are you sure you want to delete this station? This action cannot be undone.'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminService.deleteStation(stationId).subscribe({
          next: (response) => {
            if (response.success) {
              this.notificationService.showSuccess('Station deleted successfully');
              this.loadStations();
            } else {
              this.notificationService.showError(response.message || 'Failed to delete station');
            }
          },
          error: () => {
            this.notificationService.showError('Failed to delete station');
          }
        });
      }
    });
  }
}