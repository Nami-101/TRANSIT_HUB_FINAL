import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { AdminService } from '../../../services/admin.service';
import { NotificationService } from '../../../services/notification.service';
import { AdminTrain, CreateTrain, UpdateTrain, Station } from '../../../models/admin.dto';
import { ApiResponse } from '../../../models/auth.dto';
import { ConfirmDialogComponent } from '../../../components/confirm-dialog.component';

interface CoachConfig {
  class: string;
  coachCount: number;
  seatsPerCoach: number;
  priceMultiplier: number;
}

@Component({
  selector: 'app-manage-trains',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatCardModule,
    MatSlideToggleModule,
    MatCheckboxModule
  ],
  template: `
    <div class="p-3 sm:p-6">
      <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center mb-6 space-y-3 sm:space-y-0">
        <h1 class="text-2xl sm:text-3xl font-bold text-gray-900">🚂 Manage Trains</h1>
        <button mat-raised-button color="primary" (click)="openCreateDialog()" class="w-full sm:w-auto">
          <mat-icon>add</mat-icon>
          Add New Train
        </button>
      </div>

      <div *ngIf="loading" class="flex justify-center items-center h-64">
        <mat-spinner></mat-spinner>
      </div>

      <div *ngIf="!loading">
        <div class="block sm:hidden space-y-4">
          <mat-card *ngFor="let train of trains" class="p-4">
            <div class="flex justify-between items-start mb-3">
              <div>
                <h3 class="font-bold text-lg">{{ train.trainNumber }}</h3>
                <p class="text-gray-600">{{ train.trainName }}</p>
              </div>
              <span [class]="train.isActive ? 'text-green-600 bg-green-100' : 'text-red-600 bg-red-100'" 
                    class="px-2 py-1 rounded-full text-xs font-medium">
                {{ train.isActive ? 'Active' : 'Inactive' }}
              </span>
            </div>
            <div class="text-sm text-gray-700 mb-3">
              <div class="flex items-center">
                <mat-icon class="text-base mr-1">train</mat-icon>
                {{ train.sourceStationName }} → {{ train.destinationStationName }}
              </div>
            </div>
            <div class="flex justify-end space-x-2">
              <button mat-icon-button color="primary" (click)="openEditDialog(train)">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="deleteTrain(train.trainId)">
                <mat-icon>delete</mat-icon>
              </button>
              <button *ngIf="!train.isActive" mat-icon-button style="color: #dc2626" (click)="removeTrain(train.trainId)">
                <mat-icon>delete_forever</mat-icon>
              </button>
            </div>
          </mat-card>
        </div>

        <mat-card class="hidden sm:block">
          <div class="overflow-x-auto">
            <table mat-table [dataSource]="trains" class="w-full">
              <ng-container matColumnDef="trainNumber">
                <th mat-header-cell *matHeaderCellDef class="font-semibold">Train Number</th>
                <td mat-cell *matCellDef="let train">{{ train.trainNumber }}</td>
              </ng-container>
              <ng-container matColumnDef="trainName">
                <th mat-header-cell *matHeaderCellDef class="font-semibold">Train Name</th>
                <td mat-cell *matCellDef="let train">{{ train.trainName }}</td>
              </ng-container>
              <ng-container matColumnDef="route">
                <th mat-header-cell *matHeaderCellDef class="font-semibold">Route</th>
                <td mat-cell *matCellDef="let train">
                  {{ train.sourceStationName }} → {{ train.destinationStationName }}
                </td>
              </ng-container>
              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef class="font-semibold">Status</th>
                <td mat-cell *matCellDef="let train">
                  <span [class]="train.isActive ? 'text-green-600 bg-green-100' : 'text-red-600 bg-red-100'" 
                        class="px-2 py-1 rounded-full text-xs font-medium">
                    {{ train.isActive ? 'Active' : 'Inactive' }}
                  </span>
                </td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef class="font-semibold">Actions</th>
                <td mat-cell *matCellDef="let train">
                  <button mat-icon-button color="primary" (click)="openEditDialog(train)" matTooltip="Edit">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button mat-icon-button color="warn" (click)="deleteTrain(train.trainId)" matTooltip="Delete">
                    <mat-icon>delete</mat-icon>
                  </button>
                  <button *ngIf="!train.isActive" mat-icon-button style="color: #dc2626" (click)="removeTrain(train.trainId)" matTooltip="Remove Permanently">
                    <mat-icon>delete_forever</mat-icon>
                  </button>
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>
          </div>
        </mat-card>
      </div>

      <div *ngIf="showDialog" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4 overflow-y-auto" 
           (click)="$event.target === $event.currentTarget && closeDialog()">
        <div class="bg-white rounded-lg w-full max-w-lg max-h-[90vh] flex flex-col my-auto" 
             (click)="$event.stopPropagation()">
          <div class="sticky top-0 bg-white border-b px-6 py-4 rounded-t-lg">
            <div class="flex justify-between items-center">
              <h2 class="text-xl font-bold text-gray-900">
                {{ isEditMode ? '✏️ Edit Train' : '🚂 Add New Train' }}
              </h2>
              <button mat-icon-button (click)="closeDialog()">
                <mat-icon>close</mat-icon>
              </button>
            </div>
          </div>

          <div class="px-6 py-4 overflow-y-auto flex-1">
            <form [formGroup]="trainForm" (ngSubmit)="saveTrain()" class="space-y-4">
              
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <mat-form-field appearance="outline" class="w-full">
                  <mat-label>Train Number</mat-label>
                  <input matInput formControlName="trainNumber" placeholder="12345">
                  <mat-error *ngIf="trainForm.get('trainNumber')?.hasError('required')">
                    Required
                  </mat-error>
                </mat-form-field>

                <mat-form-field appearance="outline" class="w-full">
                  <mat-label>Train Name</mat-label>
                  <input matInput formControlName="trainName" placeholder="Rajdhani Express">
                  <mat-error *ngIf="trainForm.get('trainName')?.hasError('required')">
                    Required
                  </mat-error>
                </mat-form-field>
              </div>

              <div class="bg-orange-50 border border-orange-200 rounded-lg p-4">
                <h3 class="font-semibold text-orange-800 mb-3 flex items-center">
                  <mat-icon class="mr-2 text-orange-600">route</mat-icon>
                  Route
                </h3>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <mat-form-field appearance="outline">
                    <mat-label>From</mat-label>
                    <mat-select formControlName="sourceStationId">
                      <mat-option *ngFor="let station of stations" [value]="station.stationID">
                        {{ station.stationName }} ({{ station.stationCode }})
                      </mat-option>
                    </mat-select>
                    <mat-error>Required</mat-error>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>To</mat-label>
                    <mat-select formControlName="destinationStationId">
                      <mat-option *ngFor="let station of stations" [value]="station.stationID">
                        {{ station.stationName }} ({{ station.stationCode }})
                      </mat-option>
                    </mat-select>
                    <mat-error>Required</mat-error>
                  </mat-form-field>
                </div>
              </div>

              <div *ngIf="!isEditMode" class="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h3 class="font-semibold text-blue-800 mb-3 flex items-center">
                  <mat-icon class="mr-2 text-blue-600">schedule</mat-icon>
                  Timing
                </h3>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <mat-form-field appearance="outline">
                    <mat-label>Departure</mat-label>
                    <input matInput type="time" formControlName="departureTime">
                    <mat-error>Required</mat-error>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Arrival</mat-label>
                    <input matInput type="time" formControlName="arrivalTime">
                    <mat-error>Required</mat-error>
                  </mat-form-field>
                </div>
              </div>

              <div *ngIf="!isEditMode" class="bg-green-50 border border-green-200 rounded-lg p-4">
                <h3 class="font-semibold text-green-800 mb-3 flex items-center">
                  <mat-icon class="mr-2 text-green-600">currency_rupee</mat-icon>
                  Classes & Pricing
                </h3>
                
                <mat-form-field appearance="outline" class="w-full mb-4">
                  <mat-label>Base Fare (₹)</mat-label>
                  <input matInput type="number" formControlName="basePrice" placeholder="1200">
                  <mat-error *ngIf="trainForm.get('basePrice')?.hasError('required')">
                    Required
                  </mat-error>
                  <mat-error *ngIf="trainForm.get('basePrice')?.hasError('min')">
                    Must be greater than 0
                  </mat-error>
                </mat-form-field>

                <div class="space-y-2">
                  <label class="text-sm font-medium text-gray-700">Available Classes:</label>
                  <div class="grid grid-cols-2 gap-2">
                    <mat-checkbox [(ngModel)]="availableClasses.SL" [ngModelOptions]="{standalone: true}">
                      Sleeper (SL)
                    </mat-checkbox>
                    <mat-checkbox [(ngModel)]="availableClasses.AC3" [ngModelOptions]="{standalone: true}">
                      AC 3 Tier (3A)
                    </mat-checkbox>
                    <mat-checkbox [(ngModel)]="availableClasses.AC2" [ngModelOptions]="{standalone: true}">
                      AC 2 Tier (2A)
                    </mat-checkbox>
                    <mat-checkbox [(ngModel)]="availableClasses.AC1" [ngModelOptions]="{standalone: true}">
                      AC 1 Tier (1A)
                    </mat-checkbox>
                  </div>
                </div>
              </div>

              <div *ngIf="isEditMode" class="flex items-center justify-between p-3 bg-gray-50 rounded">
                <span class="font-medium">Train Status</span>
                <mat-slide-toggle formControlName="isActive">Active</mat-slide-toggle>
              </div>

            </form>
          </div>

          <div class="sticky bottom-0 bg-white border-t px-6 py-4 rounded-b-lg">
            <div class="flex flex-col sm:flex-row justify-between items-center space-y-3 sm:space-y-0">
              <!-- Advanced Config temporarily disabled -->
              <!-- <button *ngIf="!isEditMode" type="button" mat-stroked-button (click)="openAdvancedConfig()" 
                      class="w-full sm:w-auto order-2 sm:order-1">
                <mat-icon>settings</mat-icon>
                Advanced Config
              </button> -->
              
              <div class="flex space-x-3 w-full sm:w-auto order-1 sm:order-2">
                <button type="button" mat-button (click)="closeDialog()" class="flex-1 sm:flex-none">
                  Cancel
                </button>
                <button type="submit" mat-raised-button color="primary" (click)="saveTrain()"
                        [disabled]="trainForm.invalid || saving" class="flex-1 sm:flex-none">
                  <mat-spinner *ngIf="saving" diameter="16" class="mr-2"></mat-spinner>
                  {{ saving ? 'Saving...' : (isEditMode ? 'Update' : 'Create') }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Advanced Configuration Modal -->
      <div *ngIf="showAdvancedModal" class="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center z-[60] p-4 overflow-y-auto"
           (click)="$event.target === $event.currentTarget && closeAdvancedConfig()">
        <div class="bg-white rounded-lg w-full max-w-2xl max-h-[85vh] flex flex-col" 
             (click)="$event.stopPropagation()">
          
          <div class="sticky top-0 bg-gradient-to-r from-orange-500 to-orange-600 text-white px-6 py-4 rounded-t-lg">
            <div class="flex justify-between items-center">
              <h2 class="text-xl font-bold flex items-center">
                <mat-icon class="mr-2">settings</mat-icon>
                ⚙️ Advanced Coach Configuration
              </h2>
              <button mat-icon-button (click)="closeAdvancedConfig()" class="text-white">
                <mat-icon>close</mat-icon>
              </button>
            </div>
            <p class="text-orange-100 text-sm mt-2">Customize coaches and pricing</p>
          </div>

          <div class="px-6 py-6 overflow-y-auto flex-1">
            <div class="space-y-6">
              
              <div class="bg-gray-50 rounded-lg p-4">
                <h3 class="font-semibold text-gray-800 mb-4 flex items-center">
                  <mat-icon class="mr-2 text-orange-600">train</mat-icon>
                  Add Coach Configuration
                </h3>
                
                <div class="grid grid-cols-1 gap-4 mb-4">
                  <mat-form-field appearance="outline">
                    <mat-label>Class</mat-label>
                    <mat-select [(value)]="selectedClass">
                      <mat-option value="Sleeper">Sleeper (SL)</mat-option>
                      <mat-option value="AC3">AC 3 Tier (3A)</mat-option>
                      <mat-option value="AC2">AC 2 Tier (2A)</mat-option>
                      <mat-option value="AC1">AC 1 Tier (1A)</mat-option>
                      <mat-option value="General">General</mat-option>
                    </mat-select>
                  </mat-form-field>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                  <mat-form-field appearance="outline">
                    <mat-label>Number of Coaches</mat-label>
                    <input matInput type="number" [(ngModel)]="coachCount" min="1" max="10" [ngModelOptions]="{standalone: true}">
                    <mat-hint>1-10 coaches</mat-hint>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Seats per Coach</mat-label>
                    <input matInput type="number" [(ngModel)]="seatsPerCoach" min="16" max="78" [ngModelOptions]="{standalone: true}">
                    <mat-hint>SL:72, 3A:78, 2A:64, 1A:18</mat-hint>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Price Multiplier</mat-label>
                    <input matInput type="number" [(ngModel)]="priceMultiplier" min="1" max="5" step="0.1" [ngModelOptions]="{standalone: true}">
                    <mat-hint>1.0-5.0x base price</mat-hint>
                  </mat-form-field>
                </div>

                <button type="button" mat-raised-button color="primary" (click)="addCoachConfig()" class="w-full md:w-auto">
                  <mat-icon>add</mat-icon>
                  Add Configuration
                </button>
              </div>

              <div *ngIf="coachConfigs.length > 0" class="bg-white border rounded-lg">
                <div class="px-4 py-3 border-b bg-gray-50 rounded-t-lg">
                  <h4 class="font-semibold text-gray-800 flex items-center">
                    <mat-icon class="mr-2 text-green-600">check_circle</mat-icon>
                    Custom Configurations ({{ coachConfigs.length }})
                  </h4>
                </div>
                <div class="p-4 space-y-3">
                  <div *ngFor="let config of coachConfigs; let i = index" 
                       class="flex flex-col md:flex-row md:items-center justify-between bg-gray-50 p-4 rounded-lg border">
                    <div class="flex-1 mb-2 md:mb-0">
                      <div class="flex flex-wrap items-center gap-2 text-sm">
                        <span class="bg-blue-100 text-blue-800 px-2 py-1 rounded font-medium">{{ config.class }}</span>
                        <span class="bg-gray-100 text-gray-800 px-2 py-1 rounded">{{ config.coachCount }} coaches</span>
                        <span class="bg-gray-100 text-gray-800 px-2 py-1 rounded">{{ config.seatsPerCoach }} seats each</span>
                        <span class="bg-green-100 text-green-800 px-2 py-1 rounded font-medium">₹{{ (trainForm.get('basePrice')?.value || 0) * config.priceMultiplier | number:'1.0-0' }}</span>
                      </div>
                    </div>
                    <button type="button" mat-icon-button color="warn" (click)="removeCoachConfig(i)" class="ml-2">
                      <mat-icon>delete</mat-icon>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="sticky bottom-0 bg-white border-t px-6 py-4 rounded-b-lg">
            <div class="flex justify-between items-center">
              <div class="text-sm text-gray-600">
                {{ coachConfigs.length }} custom configuration(s) added
              </div>
              <button type="button" mat-raised-button color="primary" (click)="closeAdvancedConfig()">
                <mat-icon>check</mat-icon>
                Done
              </button>
            </div>
          </div>
        </div>
      </div>
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

    input[type="time"]::-webkit-calendar-picker-indicator {
      opacity: 1 !important;
      display: block !important;
      cursor: pointer;
    }

    @media (max-width: 640px) {
      :host ::ng-deep .mat-mdc-form-field {
        margin-bottom: 8px;
      }
      
      :host ::ng-deep .mat-mdc-card {
        margin: 8px 0;
        border-radius: 8px;
      }
    }

    :host ::ng-deep .mat-primary {
      --mdc-filled-button-container-color: #ff6600;
      --mdc-outlined-button-outline-color: #ff6600;
    }

    .space-y-4 > * + * {
      margin-top: 1rem;
    }

    .overflow-y-auto::-webkit-scrollbar {
      width: 4px;
    }
    
    .overflow-y-auto::-webkit-scrollbar-track {
      background: #f1f1f1;
    }
    
    .overflow-y-auto::-webkit-scrollbar-thumb {
      background: #c1c1c1;
      border-radius: 2px;
    }

    .fixed.inset-0 {
      touch-action: none;
      animation: fadeIn 0.2s ease-out;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }
  `]
})
export class ManageTrainsComponent implements OnInit, OnDestroy {
  trains: AdminTrain[] = [];
  stations: Station[] = [];
  displayedColumns: string[] = ['trainNumber', 'trainName', 'route', 'status', 'actions'];
  loading = true;
  saving = false;
  showDialog = false;
  isEditMode = false;
  currentTrainId: number | null = null;
  showAdvancedConfig = false;
  showAdvancedModal = false;

  trainForm: FormGroup;

  availableClasses = {
    SL: true,
    AC3: true,
    AC2: false,
    AC1: false
  };

  selectedClass = 'Sleeper';
  coachCount = 2;
  seatsPerCoach = 72;
  priceMultiplier = 1;
  coachConfigs: CoachConfig[] = [];

  constructor(
    private adminService: AdminService,
    private notificationService: NotificationService,
    private formBuilder: FormBuilder,
    private dialog: MatDialog
  ) {
    this.trainForm = this.formBuilder.group({
      trainNumber: ['', [Validators.required]],
      trainName: ['', [Validators.required]],
      sourceStationId: ['', [Validators.required]],
      destinationStationId: ['', [Validators.required]],
      departureTime: ['', [Validators.required]],
      arrivalTime: ['', [Validators.required]],
      basePrice: ['', [Validators.required, Validators.min(1)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadTrains();
    this.loadStations();
  }

  ngOnDestroy(): void {
    if (this.showDialog || this.showAdvancedModal) {
      this.unlockBodyScroll();
    }
  }

  loadTrains(): void {
    this.loading = true;
    this.adminService.getAllTrains().subscribe({
      next: (response: ApiResponse<AdminTrain[]>) => {
        this.loading = false;
        if (response.success && response.data) {
          this.trains = response.data;
        } else {
          this.notificationService.showError('Failed to load trains');
        }
      },
      error: () => {
        this.loading = false;
        this.notificationService.showError('Failed to load trains');
      }
    });
  }

  loadStations(): void {
    this.adminService.getAllStations().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.stations = response.data;
        }
      },
      error: () => {}
    });
  }

  openCreateDialog(): void {
    this.isEditMode = false;
    this.currentTrainId = null;
    this.showAdvancedConfig = false;
    this.trainForm.reset();
    
    this.trainForm.get('departureTime')?.setValidators([Validators.required]);
    this.trainForm.get('arrivalTime')?.setValidators([Validators.required]);
    this.trainForm.get('basePrice')?.setValidators([Validators.required, Validators.min(1)]);
    this.trainForm.get('departureTime')?.updateValueAndValidity();
    this.trainForm.get('arrivalTime')?.updateValueAndValidity();
    this.trainForm.get('basePrice')?.updateValueAndValidity();
    
    this.trainForm.patchValue({ isActive: true });
    this.resetCoachConfig();
    this.resetAvailableClasses();
    this.lockBodyScroll();
    this.showDialog = true;
  }

  resetCoachConfig(): void {
    this.selectedClass = 'Sleeper';
    this.coachCount = 2;
    this.seatsPerCoach = 72;
    this.priceMultiplier = 1;
    this.coachConfigs = [];
  }

  resetAvailableClasses(): void {
    this.availableClasses = {
      SL: true,
      AC3: true,
      AC2: false,
      AC1: false
    };
  }

  addCoachConfig(): void {
    if (!this.selectedClass || this.coachCount < 1 || this.seatsPerCoach < 1) {
      this.notificationService.showError('Please fill all coach configuration fields');
      return;
    }

    const config: CoachConfig = {
      class: this.selectedClass,
      coachCount: this.coachCount,
      seatsPerCoach: this.seatsPerCoach,
      priceMultiplier: this.priceMultiplier
    };

    this.coachConfigs.push(config);
    this.notificationService.showSuccess(`Added ${this.coachCount} ${this.selectedClass} coaches`);
  }

  removeCoachConfig(index: number): void {
    this.coachConfigs.splice(index, 1);
  }

  openEditDialog(train: AdminTrain): void {
    this.isEditMode = true;
    this.currentTrainId = train.trainId;
    
    this.trainForm.get('departureTime')?.clearValidators();
    this.trainForm.get('arrivalTime')?.clearValidators();
    this.trainForm.get('basePrice')?.clearValidators();
    this.trainForm.get('departureTime')?.updateValueAndValidity();
    this.trainForm.get('arrivalTime')?.updateValueAndValidity();
    this.trainForm.get('basePrice')?.updateValueAndValidity();
    
    this.trainForm.patchValue({
      trainNumber: train.trainNumber,
      trainName: train.trainName,
      sourceStationId: train.sourceStationId,
      destinationStationId: train.destinationStationId,
      departureTime: train.departureTime,
      arrivalTime: train.arrivalTime,
      basePrice: train.basePrice,
      isActive: train.isActive
    });
    this.lockBodyScroll();
    this.showDialog = true;
  }

  closeDialog(): void {
    this.showDialog = false;
    this.showAdvancedConfig = false;
    this.showAdvancedModal = false;
    this.trainForm.reset();
    this.unlockBodyScroll();
  }

  openAdvancedConfig(): void {
    this.showAdvancedModal = true;
  }

  closeAdvancedConfig(): void {
    this.showAdvancedModal = false;
  }

  saveCoachesToTrain(): void {
    if (this.coachConfigs.length === 0) {
      this.saving = false;
      this.notificationService.showError('No coaches to add');
      return;
    }

    if (!this.currentTrainId) {
      this.saving = false;
      this.notificationService.showError('No train selected');
      return;
    }
    
    const coachPromises: Promise<any>[] = [];
    
    this.coachConfigs.forEach(config => {
      for (let i = 0; i < config.coachCount; i++) {
        const createCoach = {
          coachNumber: `${config.class}-${Math.random().toString(36).substr(2, 3).toUpperCase()}`,
          trainClassId: this.getTrainClassId(config.class),
          totalSeats: config.seatsPerCoach,
          baseFare: (this.trainForm.get('basePrice')?.value || 0) * config.priceMultiplier
        };
        coachPromises.push(this.adminService.addCoachToTrain(this.currentTrainId!, createCoach).toPromise());
      }
    });

    Promise.all(coachPromises).then(() => {
      this.saving = false;
      const totalCoaches = this.coachConfigs.reduce((sum, config) => sum + config.coachCount, 0);
      const totalSeats = this.coachConfigs.reduce((sum, config) => sum + (config.coachCount * config.seatsPerCoach), 0);
      
      this.notificationService.showSuccess(
        `Train created with advanced configuration! Added ${totalCoaches} custom coaches (${totalSeats} seats) successfully!`
      );
      
      this.coachConfigs = [];
      this.closeDialog();
      this.loadTrains();
    }).catch(() => {
      this.saving = false;
      this.notificationService.showError('Failed to add advanced coaches to train');
    });
  }

  private getTrainClassId(className: string): number {
    switch (className) {
      case 'Sleeper': return 1;
      case 'AC3': return 2;
      case 'AC2': return 3;
      case 'AC1': return 4;
      case 'General': return 5;
      default: return 1;
    }
  }

  private lockBodyScroll(): void {
    document.body.style.overflow = 'hidden';
    document.body.style.paddingRight = this.getScrollbarWidth() + 'px';
  }

  private unlockBodyScroll(): void {
    document.body.style.overflow = '';
    document.body.style.paddingRight = '';
  }

  private getScrollbarWidth(): number {
    const scrollDiv = document.createElement('div');
    scrollDiv.style.cssText = 'width: 100px; height: 100px; overflow: scroll; position: absolute; top: -9999px;';
    document.body.appendChild(scrollDiv);
    const scrollbarWidth = scrollDiv.offsetWidth - scrollDiv.clientWidth;
    document.body.removeChild(scrollDiv);
    return scrollbarWidth;
  }

  saveTrain(): void {
    if (this.trainForm.invalid) {
      this.notificationService.showError('Please fill all required fields');
      return;
    }

    if (!this.isEditMode) {
      const hasSelectedClass = Object.values(this.availableClasses).some(selected => selected);
      if (!hasSelectedClass) {
        this.notificationService.showError('Please select at least one class');
        return;
      }
    }

    this.saving = true;
    const formValue = this.trainForm.value;

    if (this.isEditMode && this.currentTrainId) {
      const updateData: UpdateTrain = {
        trainName: formValue.trainName,
        sourceStationId: formValue.sourceStationId,
        destinationStationId: formValue.destinationStationId,
        departureTime: formValue.departureTime,
        arrivalTime: formValue.arrivalTime,
        basePrice: formValue.basePrice,
        isActive: formValue.isActive
      };

      this.adminService.updateTrain(this.currentTrainId, updateData).subscribe({
        next: (response: any) => {
          this.saving = false;
          if (response.success) {
            this.notificationService.showSuccess('Train updated successfully!');
            this.closeDialog();
            this.loadTrains();
          } else {
            this.notificationService.showError(response.message || 'Failed to update train');
          }
        },
        error: () => {
          this.saving = false;
          this.notificationService.showError('Failed to update train');
        }
      });
    } else {
      const selectedClassIds: number[] = [];
      if (this.availableClasses.SL) selectedClassIds.push(1);
      if (this.availableClasses.AC3) selectedClassIds.push(2);
      if (this.availableClasses.AC2) selectedClassIds.push(3);
      if (this.availableClasses.AC1) selectedClassIds.push(4);

      const createData: CreateTrain = {
        trainNumber: formValue.trainNumber,
        trainName: formValue.trainName,
        sourceStationId: formValue.sourceStationId,
        destinationStationId: formValue.destinationStationId,
        departureTime: formValue.departureTime,
        arrivalTime: formValue.arrivalTime,
        basePrice: formValue.basePrice,
        selectedClassIds: selectedClassIds
      };

      this.adminService.createTrain(createData).subscribe({
        next: (response: any) => {
          if (response.success && response.data) {
            this.currentTrainId = response.data.trainId;
            
            // Skip advanced coach configuration for now due to database issues
            this.saving = false;
            const selectedClassNames = Object.entries(this.availableClasses)
              .filter(([_, selected]) => selected)
              .map(([className, _]) => className)
              .join(', ');
            
            this.notificationService.showSuccess(
              `Train created successfully! Auto-configured with ${selectedClassNames} classes.`
            );
            this.closeDialog();
            this.loadTrains();
          } else {
            this.saving = false;
            this.notificationService.showError(response.message || 'Failed to create train');
          }
        },
        error: () => {
          this.saving = false;
          this.notificationService.showError('Failed to create train');
        }
      });
    }
  }

  deleteTrain(trainId: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Train',
        message: 'Are you sure you want to delete this train? This will mark it as inactive but preserve booking history.'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminService.deleteTrain(trainId).subscribe({
          next: (response: any) => {
            if (response.success) {
              this.notificationService.showSuccess('Train deleted successfully');
              this.loadTrains();
            } else {
              this.notificationService.showError(response.message || 'Failed to delete train');
            }
          },
          error: () => {
            this.notificationService.showError('Failed to delete train');
          }
        });
      }
    });
  }

  removeTrain(trainId: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Permanently Remove Train',
        message: 'This will PERMANENTLY remove the train and all its data from the database. This action cannot be undone. Are you absolutely sure?'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminService.removeTrain(trainId).subscribe({
          next: (response: any) => {
            if (response.success) {
              this.notificationService.showSuccess('Train permanently removed');
              this.loadTrains();
            } else {
              this.notificationService.showError(response.message || 'Failed to remove train');
            }
          },
          error: (error: any) => {
            const errorMessage = error.error?.message || 'Cannot remove an active train. Please delete it first.';
            this.notificationService.showError(errorMessage);
          }
        });
      }
    });
  }
}