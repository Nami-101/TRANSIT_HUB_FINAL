import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators, FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { BookingService, BookingRequest } from '../services/booking.service';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { PaymentComponent } from '../features/payment/payment.component';
import { SeatMapComponent } from './seat-map/seat-map.component';

@Component({
  selector: 'app-booking-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    SeatMapComponent
  ],
  template: `
    <h2 mat-dialog-title>Book Train Ticket</h2>
    <mat-dialog-content class="dialog-content">
      <div class="mb-4 p-4 bg-blue-50 rounded">
        <h3 class="font-semibold">{{ data.trainNumber }} - {{ data.trainName }}</h3>
        <p>{{ data.sourceStation }} → {{ data.destinationStation }}</p>
        <p>Fare: ₹{{ data.fare }} per person</p>
      </div>

      <form [formGroup]="bookingForm">
        <div formArrayName="passengers">
          <div *ngFor="let passenger of passengers.controls; let i = index" 
               [formGroupName]="i" class="passenger-form mb-4 p-4 border rounded">
            <h4 class="font-semibold mb-2">Passenger {{ i + 1 }}</h4>
            
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
              <mat-form-field appearance="fill">
                <mat-label>Name</mat-label>
                <input matInput formControlName="name" placeholder="Full Name">
              </mat-form-field>

              <mat-form-field appearance="fill">
                <mat-label>Age</mat-label>
                <input matInput type="number" formControlName="age" placeholder="Age">
              </mat-form-field>

              <mat-form-field appearance="fill">
                <mat-label>Gender</mat-label>
                <mat-select formControlName="gender">
                  <mat-option value="Male">Male</mat-option>
                  <mat-option value="Female">Female</mat-option>
                  <mat-option value="Other">Other</mat-option>
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="fill">
                <mat-label>Seat Preference</mat-label>
                <mat-select formControlName="seatPreference">
                  <mat-option value="Window">Window</mat-option>
                  <mat-option value="Aisle">Aisle</mat-option>
                  <mat-option value="Any">Any</mat-option>
                </mat-select>
              </mat-form-field>
            </div>
          </div>
        </div>

        <button type="button" mat-stroked-button (click)="addPassenger()" class="mb-4">
          <mat-icon>add</mat-icon>
          Add Passenger
        </button>

        <!-- Quota Selection -->
        <div class="quota-section mb-4 p-4 border rounded">
          <h3 class="font-semibold mb-2">Select Quota</h3>
          <mat-form-field appearance="fill" class="w-full">
            <mat-label>Booking Quota</mat-label>
            <mat-select [(value)]="selectedQuota" (selectionChange)="onQuotaChange()">
              <mat-option *ngFor="let quota of quotas" 
                         [value]="quota.value" 
                         [disabled]="!isQuotaEligible(quota.value)">
                {{ quota.label }}
                <span *ngIf="!isQuotaEligible(quota.value)" class="text-red-500 text-xs">
                  (Not eligible)
                </span>
              </mat-option>
            </mat-select>
          </mat-form-field>
          <div class="text-sm text-gray-600 mt-2">
            <div *ngIf="selectedQuota === 'Ladies'">👩 Ladies quota: Only for female passengers</div>
            <div *ngIf="selectedQuota === 'Senior'">👴 Senior quota: At least one passenger must be 58+</div>
            <div *ngIf="selectedQuota === 'Tatkal'">⚡ Tatkal: Premium booking with higher fare</div>
          </div>
        </div>

        <!-- Seat Selection -->
        <div class="seat-selection-section mb-4">
          <h3 class="font-semibold mb-2">Select Seats (Optional)</h3>
          <div class="flex items-center gap-4 mb-4">
            <label class="flex items-center gap-2">
              <input type="checkbox" [(ngModel)]="autoAssignSeats" [ngModelOptions]="{standalone: true}">
              <span>Auto-assign seats</span>
            </label>
          </div>
          
          <div *ngIf="!autoAssignSeats" class="seat-map-wrapper">
            <app-seat-map 
              [scheduleId]="data.scheduleID"
              [trainClass]="data.trainClass"
              [maxSeats]="passengers.length"
              (seatsSelected)="onSeatsSelected($event)"
              (coachChanged)="onCoachChanged($event)">
            </app-seat-map>
            

          </div>
        </div>

        <div class="total-section p-4 rounded">
          <h3 class="font-semibold text-green-700">Total: ₹{{ getTotalAmount() }}</h3>
          <p class="text-sm text-gray-600">Base fare: ₹{{ getBaseFare() }} × {{ passengers.length }} passengers</p>
          <p *ngIf="getDiscount() !== 0" class="text-sm" [class.text-green-600]="getDiscount() > 0" [class.text-red-600]="getDiscount() < 0">
            {{ getDiscount() > 0 ? 'Discount' : 'Premium' }}: {{ getDiscount() > 0 ? '-' : '+' }}₹{{ Math.abs(getDiscountAmount()) }}
          </p>
          <p *ngIf="selectedSeats.length > 0" class="text-sm text-green-600 mt-1">
            🎯 Selected seats: {{ selectedSeats.join(', ') }}
          </p>
          <p *ngIf="autoAssignSeats" class="text-sm text-blue-600 mt-1">
            🎲 Seats will be auto-assigned
          </p>
        </div>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" 
              [disabled]="bookingForm.invalid || isLoading"
              (click)="onBook()">
        {{ isLoading ? 'Booking...' : 'Book Now' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
      max-width: 100vw;
      box-sizing: border-box;
    }
    
    .dialog-content {
      max-height: 45vh;
      overflow-y: auto;
      padding-right: 8px;
    }
    
    .seat-map-wrapper {
      max-height: 400px;
      overflow-y: auto;
      border-radius: 10px;
      margin: 15px 0;
    }
    
    .seat-selection-section {
      border: 2px solid #e3f2fd;
      border-radius: 10px;
      padding: 15px;
      background: linear-gradient(135deg, #f8f9ff 0%, #e8f4fd 100%);
    }
    
    .pulse-btn {
      animation: pulseGlow 1.5s ease-in-out infinite;
    }
    
    @keyframes pulseGlow {
      0%, 100% { box-shadow: 0 0 5px rgba(33, 150, 243, 0.5); }
      50% { box-shadow: 0 0 20px rgba(33, 150, 243, 0.8), 0 0 30px rgba(33, 150, 243, 0.4); }
    }
    
    .passenger-form {
      background: linear-gradient(135deg, #fff 0%, #f8f9fa 100%);
      border: 1px solid #e9ecef;
      transition: all 0.3s ease;
    }
    
    .passenger-form:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }
    
    .total-section {
      background: linear-gradient(135deg, #e8f5e8 0%, #f0f8f0 100%);
      border: 2px solid #4caf50;
    }
    
    .quota-section {
      background: linear-gradient(135deg, #fff3e0 0%, #fce4ec 100%);
      border: 2px solid #ff9800;
    }
    
    /* Mobile Responsive */
    @media (max-width: 768px) {
      .seat-map-wrapper {
        max-height: 350px;
        margin: 12px 0;
      }
      
      .seat-selection-section {
        padding: 10px;
        margin-bottom: 15px;
      }
      
      .passenger-form {
        margin-bottom: 15px;
        padding: 15px;
      }
      
      .grid {
        grid-template-columns: 1fr;
        gap: 12px;
      }
      
      .quota-section {
        padding: 15px;
        margin-bottom: 15px;
      }
      
      .total-section {
        padding: 15px;
      }
    }
    
    @media (max-width: 480px) {
      .seat-map-wrapper {
        max-height: 300px;
        margin: 10px 0;
      }
      
      .seat-selection-section {
        padding: 8px;
      }
      
      .passenger-form {
        padding: 12px;
        margin-bottom: 12px;
      }
      
      .quota-section {
        padding: 12px;
      }
      
      .total-section {
        padding: 12px;
      }
    }
  `]
})
export class BookingDialogComponent {
  bookingForm: FormGroup;
  isLoading = false;
  autoAssignSeats = true;
  selectedSeats: number[] = [];
  selectedQuota = 'General';
  manualSeats = '';
  quotas = [
    { value: 'General', label: 'General', discount: 0 },
    { value: 'Ladies', label: 'Ladies (10% off)', discount: 0.1 },
    { value: 'Senior', label: 'Senior Citizen (20% off)', discount: 0.2 },
    { value: 'Tatkal', label: 'Tatkal (+50%)', discount: -0.5 }
  ];

  constructor(
    private fb: FormBuilder,
    private bookingService: BookingService,
    private authService: AuthService,
    private notificationService: NotificationService,
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<BookingDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.bookingForm = this.fb.group({
      passengers: this.fb.array([this.createPassengerForm()])
    });
  }

  get passengers() {
    return this.bookingForm.get('passengers') as FormArray;
  }

  createPassengerForm(): FormGroup {
    return this.fb.group({
      name: ['', Validators.required],
      age: ['', [Validators.required, Validators.min(1)]],
      gender: ['', Validators.required],
      seatPreference: ['Any']
    });
  }

  addPassenger() {
    this.passengers.push(this.createPassengerForm());
  }

  getTotalAmount(): number {
    const baseFare = this.passengers.length * this.data.fare;
    const quota = this.quotas.find(q => q.value === this.selectedQuota);
    const discount = quota?.discount || 0;
    return Math.round(baseFare * (1 - discount));
  }

  getBaseFare(): number {
    return this.data.fare;
  }

  getDiscount(): number {
    const quota = this.quotas.find(q => q.value === this.selectedQuota);
    return quota?.discount || 0;
  }

  getDiscountAmount(): number {
    const baseFare = this.passengers.length * this.data.fare;
    return Math.round(baseFare * Math.abs(this.getDiscount()));
  }

  Math = Math;

  onCancel() {
    this.dialogRef.close();
  }

  onBook() {
    if (this.bookingForm.valid) {
      const currentUser = this.authService.currentUserValue;
      
      if (!currentUser) {
        this.notificationService.showError('Please login to book tickets');
        return;
      }

      // Open payment dialog first
      const paymentDialog = this.dialog.open(PaymentComponent, {
        width: '500px',
        maxWidth: '95vw',
        maxHeight: '90vh',
        panelClass: 'mobile-dialog',
        disableClose: true,
        data: {
          totalAmount: this.getTotalAmount(),
          passengerCount: this.passengers.length,
          trainName: this.data.trainName,
          travelDate: this.data.travelDate
        }
      });

      paymentDialog.afterClosed().subscribe(result => {
        if (result?.success) {
          this.processBooking();
        }
      });
    }
  }

  onSeatsSelected(seats: number[]) {
    this.selectedSeats = [...seats]; // Create new array to ensure change detection
  }

  onQuotaChange() {
    this.validateQuotaEligibility();
  }

  validateQuotaEligibility() {
    const passengers = this.passengers.value || [];
    
    if (this.selectedQuota === 'Ladies') {
      const hasOnlyFemales = passengers.every((p: any) => p.gender === 'Female');
      if (!hasOnlyFemales && passengers.length > 0) {
        this.selectedQuota = 'General';
      }
    }
    
    if (this.selectedQuota === 'Senior') {
      const hasElderly = passengers.some((p: any) => p.age >= 58);
      if (!hasElderly && passengers.length > 0) {
        this.selectedQuota = 'General';
      }
    }
  }

  isQuotaEligible(quota: string): boolean {
    const passengers = this.passengers.value || [];
    if (passengers.length === 0) return true;
    
    switch (quota) {
      case 'Ladies':
        return passengers.every((p: any) => p.gender === 'Female');
      case 'Senior':
        return passengers.some((p: any) => p.age >= 58);
      default:
        return true;
    }
  }



  onCoachChanged(coachNumber: number) {
    // Coach change is now handled by seat-map component
  }

  onManualSeatInput() {
    if (this.manualSeats.trim()) {
      const seats = this.manualSeats.split(',').map(s => parseInt(s.trim())).filter(n => !isNaN(n));
      this.selectedSeats = [...seats];
    } else {
      this.selectedSeats = [];
    }
  }

  private processBooking() {
    this.isLoading = true;
    
    const bookingRequest = {
      TrainId: this.data.trainID,
      ScheduleId: this.data.scheduleID,
      TravelDate: this.data.travelDate,
      Passengers: this.passengers.value,
      PassengerCount: this.passengers.length,
      AutoAssignSeats: this.autoAssignSeats,
      PreferredSeats: this.selectedSeats,
      SelectedQuota: this.selectedQuota
    };



    this.bookingService.bookTrain(bookingRequest).subscribe({
      next: (response: any) => {
        this.isLoading = false;
        if (response.success) {
          this.notificationService.showSuccess('Booking confirmed! 🎉');
          this.dialogRef.close(response);
        } else {
          this.notificationService.showError(response.message);
        }
      },
      error: (error: any) => {
        this.isLoading = false;
        this.notificationService.showError('Booking failed: ' + (error.error?.message || error.message));
        console.error('Booking error:', error);
      }
    });
  }
}