import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { AdminService } from '../../../services/admin.service';
import { NotificationService } from '../../../services/notification.service';
import { AdminFlight, CreateFlight, UpdateFlight, Airport } from '../../../models/admin.dto';

@Component({
    selector: 'app-manage-flights',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
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
        MatSlideToggleModule
    ],
    template: `
    <div class="p-6">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold text-gray-900">Manage Flights</h1>
        <button mat-raised-button color="primary" (click)="openCreateDialog()">
          <mat-icon>add</mat-icon>
          Add New Flight
        </button>
      </div>

      <!-- Loading Spinner -->
      <div *ngIf="loading" class="flex justify-center items-center h-64">
        <mat-spinner></mat-spinner>
      </div>

      <!-- Flights Table -->
      <mat-card *ngIf="!loading">
        <div class="overflow-x-auto">
          <table mat-table [dataSource]="flights" class="w-full">
            
            <!-- Flight Number Column -->
            <ng-container matColumnDef="flightNumber">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Flight Number</th>
              <td mat-cell *matCellDef="let flight">{{ flight.flightNumber }}</td>
            </ng-container>

            <!-- Airline Column -->
            <ng-container matColumnDef="airline">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Airline</th>
              <td mat-cell *matCellDef="let flight">{{ flight.airline }}</td>
            </ng-container>

            <!-- Route Column -->
            <ng-container matColumnDef="route">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Route</th>
              <td mat-cell *matCellDef="let flight">
                {{ flight.sourceAirportName }} → {{ flight.destinationAirportName }}
              </td>
            </ng-container>

            <!-- Schedule Column -->
            <ng-container matColumnDef="schedule">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Schedule</th>
              <td mat-cell *matCellDef="let flight">
                <div class="text-sm">
                  <div>Departure: {{ flight.departureTime }}</div>
                  <div>Arrival: {{ flight.arrivalTime }}</div>
                </div>
              </td>
            </ng-container>

            <!-- Price Column -->
            <ng-container matColumnDef="basePrice">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Base Price</th>
              <td mat-cell *matCellDef="let flight">₹{{ flight.basePrice | number:'1.0-0' }}</td>
            </ng-container>

            <!-- Status Column -->
            <ng-container matColumnDef="isActive">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Status</th>
              <td mat-cell *matCellDef="let flight">
                <span [class]="flight.isActive ? 'text-green-600 bg-green-100' : 'text-red-600 bg-red-100'" 
                      class="px-2 py-1 rounded-full text-xs font-medium">
                  {{ flight.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
            </ng-container>

            <!-- Actions Column -->
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Actions</th>
              <td mat-cell *matCellDef="let flight">
                <button mat-icon-button color="primary" (click)="openEditDialog(flight)" matTooltip="Edit">
                  <mat-icon>edit</mat-icon>
                </button>
                <button mat-icon-button color="warn" (click)="deleteFlight(flight.flightId)" matTooltip="Delete">
                  <mat-icon>delete</mat-icon>
                </button>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </div>

        <!-- No Data Message -->
        <div *ngIf="flights.length === 0 && !loading" class="text-center py-8 text-gray-500">
          No flights found. Click "Add New Flight" to create your first flight.
        </div>
      </mat-card>

      <!-- Create/Edit Dialog -->
      <div *ngIf="showDialog" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
        <div class="bg-white rounded-lg p-6 w-full max-w-md max-h-[90vh] overflow-y-auto">
          <h2 class="text-xl font-bold mb-4">{{ isEditMode ? 'Edit Flight' : 'Add New Flight' }}</h2>
          
          <form [formGroup]="flightForm" (ngSubmit)="saveFlight()" class="space-y-4">
            
            <!-- Flight Number -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Flight Number</mat-label>
              <input matInput formControlName="flightNumber" placeholder="e.g., FL001" [readonly]="isEditMode">
              <mat-error *ngIf="flightForm.get('flightNumber')?.hasError('required')">
                Flight number is required
              </mat-error>
            </mat-form-field>

            <!-- Airline -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Airline</mat-label>
              <input matInput formControlName="airline" placeholder="e.g., Air India">
              <mat-error *ngIf="flightForm.get('airline')?.hasError('required')">
                Airline is required
              </mat-error>
            </mat-form-field>

            <!-- Source Airport -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Source Airport</mat-label>
              <mat-select formControlName="sourceAirportId">
                <mat-option *ngFor="let airport of airports" [value]="airport.airportID">
                  {{ airport.airportName }} ({{ airport.code }})
                </mat-option>
              </mat-select>
              <mat-error *ngIf="flightForm.get('sourceAirportId')?.hasError('required')">
                Source airport is required
              </mat-error>
            </mat-form-field>

            <!-- Destination Airport -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Destination Airport</mat-label>
              <mat-select formControlName="destinationAirportId">
                <mat-option *ngFor="let airport of airports" [value]="airport.airportID">
                  {{ airport.airportName }} ({{ airport.code }})
                </mat-option>
              </mat-select>
              <mat-error *ngIf="flightForm.get('destinationAirportId')?.hasError('required')">
                Destination airport is required
              </mat-error>
            </mat-form-field>

            <!-- Departure Time -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Departure Time</mat-label>
              <input matInput type="time" formControlName="departureTime">
              <mat-error *ngIf="flightForm.get('departureTime')?.hasError('required')">
                Departure time is required
              </mat-error>
            </mat-form-field>

            <!-- Arrival Time -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Arrival Time</mat-label>
              <input matInput type="time" formControlName="arrivalTime">
              <mat-error *ngIf="flightForm.get('arrivalTime')?.hasError('required')">
                Arrival time is required
              </mat-error>
            </mat-form-field>

            <!-- Base Price -->
            <mat-form-field appearance="fill" class="w-full">
              <mat-label>Base Price (₹)</mat-label>
              <input matInput type="number" formControlName="basePrice" placeholder="e.g., 5000">
              <mat-error *ngIf="flightForm.get('basePrice')?.hasError('required')">
                Base price is required
              </mat-error>
              <mat-error *ngIf="flightForm.get('basePrice')?.hasError('min')">
                Base price must be greater than 0
              </mat-error>
            </mat-form-field>

            <!-- Active Status (only for edit) -->
            <div *ngIf="isEditMode" class="flex items-center">
              <mat-slide-toggle formControlName="isActive">
                Active
              </mat-slide-toggle>
            </div>

            <!-- Dialog Actions -->
            <div class="flex justify-end space-x-2 pt-4">
              <button type="button" mat-button (click)="closeDialog()">Cancel</button>
              <button type="submit" mat-raised-button color="primary" 
                      [disabled]="flightForm.invalid || saving">
                <mat-spinner *ngIf="saving" diameter="20" class="mr-2"></mat-spinner>
                {{ saving ? 'Saving...' : (isEditMode ? 'Update' : 'Create') }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
    styles: [`
    :host ::ng-deep .mat-mdc-form-field {
      width: 100%;
    }
  `]
})
export class ManageFlightsComponent implements OnInit {
    flights: AdminFlight[] = [];
    airports: Airport[] = [];
    displayedColumns: string[] = ['flightNumber', 'airline', 'route', 'schedule', 'basePrice', 'isActive', 'actions'];
    loading = true;
    showDialog = false;
    saving = false;
    isEditMode = false;
    currentFlightId: number | null = null;

    flightForm: FormGroup;

    constructor(
        private adminService: AdminService,
        private notificationService: NotificationService,
        private formBuilder: FormBuilder
    ) {
        this.flightForm = this.formBuilder.group({
            flightNumber: ['', [Validators.required]],
            airline: ['', [Validators.required]],
            sourceAirportId: ['', [Validators.required]],
            destinationAirportId: ['', [Validators.required]],
            departureTime: ['', [Validators.required]],
            arrivalTime: ['', [Validators.required]],
            basePrice: ['', [Validators.required, Validators.min(1)]],
            isActive: [true]
        });
    }

    ngOnInit(): void {
        this.loadFlights();
        this.loadAirports();
    }

    loadFlights(): void {
        this.loading = true;
        this.adminService.getAllFlights().subscribe({
            next: (response) => {
                this.loading = false;
                if (response.success && response.data) {
                    this.flights = response.data;
                } else {
                    this.notificationService.showError('Failed to load flights');
                }
            },
            error: (error) => {
                this.loading = false;
                console.error('Error loading flights:', error);
                this.notificationService.showError('Failed to load flights');
            }
        });
    }

    loadAirports(): void {
        this.adminService.getAllAirports().subscribe({
            next: (response) => {
                if (response.success && response.data) {
                    this.airports = response.data;
                }
            },
            error: (error) => {
                console.error('Error loading airports:', error);
            }
        });
    }

    openCreateDialog(): void {
        this.isEditMode = false;
        this.currentFlightId = null;
        this.flightForm.reset();
        this.flightForm.patchValue({ isActive: true });
        this.showDialog = true;
    }

    openEditDialog(flight: AdminFlight): void {
        this.isEditMode = true;
        this.currentFlightId = flight.flightId;
        this.flightForm.patchValue({
            flightNumber: flight.flightNumber,
            airline: flight.airline,
            sourceAirportId: flight.sourceAirportId,
            destinationAirportId: flight.destinationAirportId,
            departureTime: flight.departureTime,
            arrivalTime: flight.arrivalTime,
            basePrice: flight.basePrice,
            isActive: flight.isActive
        });
        this.showDialog = true;
    }

    closeDialog(): void {
        this.showDialog = false;
        this.flightForm.reset();
    }

    saveFlight(): void {
        if (this.flightForm.invalid) return;

        this.saving = true;
        const formValue = this.flightForm.value;

        if (this.isEditMode && this.currentFlightId) {
            // Update existing flight
            const updateData: UpdateFlight = {
                airline: formValue.airline,
                sourceAirportId: formValue.sourceAirportId,
                destinationAirportId: formValue.destinationAirportId,
                departureTime: formValue.departureTime,
                arrivalTime: formValue.arrivalTime,
                basePrice: formValue.basePrice,
                isActive: formValue.isActive
            };

            this.adminService.updateFlight(this.currentFlightId, updateData).subscribe({
                next: (response) => {
                    this.saving = false;
                    if (response.success) {
                        this.notificationService.showSuccess('Flight updated successfully');
                        this.closeDialog();
                        this.loadFlights();
                    } else {
                        this.notificationService.showError(response.message || 'Failed to update flight');
                    }
                },
                error: (error) => {
                    this.saving = false;
                    console.error('Error updating flight:', error);
                    this.notificationService.showError('Failed to update flight');
                }
            });
        } else {
            // Create new flight
            const createData: CreateFlight = {
                flightNumber: formValue.flightNumber,
                airline: formValue.airline,
                sourceAirportId: formValue.sourceAirportId,
                destinationAirportId: formValue.destinationAirportId,
                departureTime: formValue.departureTime,
                arrivalTime: formValue.arrivalTime,
                basePrice: formValue.basePrice
            };

            this.adminService.createFlight(createData).subscribe({
                next: (response) => {
                    this.saving = false;
                    if (response.success) {
                        this.notificationService.showSuccess('Flight created successfully');
                        this.closeDialog();
                        this.loadFlights();
                    } else {
                        this.notificationService.showError(response.message || 'Failed to create flight');
                    }
                },
                error: (error) => {
                    this.saving = false;
                    console.error('Error creating flight:', error);
                    this.notificationService.showError('Failed to create flight');
                }
            });
        }
    }

    deleteFlight(flightId: number): void {
        if (confirm('Are you sure you want to delete this flight?')) {
            this.adminService.deleteFlight(flightId).subscribe({
                next: (response) => {
                    if (response.success) {
                        this.notificationService.showSuccess('Flight deleted successfully');
                        this.loadFlights();
                    } else {
                        this.notificationService.showError(response.message || 'Failed to delete flight');
                    }
                },
                error: (error) => {
                    console.error('Error deleting flight:', error);
                    this.notificationService.showError('Failed to delete flight');
                }
            });
        }
    }
}