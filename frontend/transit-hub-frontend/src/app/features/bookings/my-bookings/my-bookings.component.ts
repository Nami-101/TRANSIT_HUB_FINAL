import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { BookingService } from '../../../services/booking.service';
import { NotificationService } from '../../../services/notification.service';
import { CancelDialogComponent } from '../cancel-dialog.component';
import { RemoveAllDialogComponent } from '../../../components/remove-all-dialog.component';
import { RemoveBookingDialogComponent } from '../../../components/remove-booking-dialog.component';
import { BookingStatusToggleComponent } from '../../../components/booking-status-toggle.component';
import jsPDF from 'jspdf';

interface BookingDetails {
  bookingId: number;
  bookingReference: string;
  status: string;
  coachNumber?: number;
  seatNumbers: number[];
  totalAmount: number;
  paymentStatus: string;
  waitlistPosition?: number;
  selectedQuota?: string;
  trainName: string;
  trainNumber: string;
  route: string;
  travelDate: string;
  departureTime: string;
  arrivalTime: string;
  trainClass?: string;
  passengers: PassengerDetails[];
}

interface PassengerDetails {
  name: string;
  age: number;
  gender: string;
  seatNumber?: number;
}

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatTabsModule,
    BookingStatusToggleComponent
  ],
  template: `
    <div class="container mx-auto px-4 py-4 sm:py-6">
      <div class="flex items-center justify-between mb-4 sm:mb-6">
        <h1 class="text-2xl sm:text-3xl font-bold text-gray-800">My Bookings</h1>
        <mat-icon class="text-3xl sm:text-4xl text-blue-600">confirmation_number</mat-icon>
      </div>

      <div *ngIf="loading" class="flex justify-center py-8">
        <mat-spinner></mat-spinner>
      </div>

      <!-- Booking Status Toggle -->
      <div *ngIf="!loading" class="flex justify-center mb-4 sm:mb-6">
        <app-booking-status-toggle 
          [value]="currentStatus" 
          (valueChange)="onStatusChange($event)">
        </app-booking-status-toggle>
      </div>

      <!-- Bookings Display -->
      <div *ngIf="!loading">
        <!-- Upcoming Bookings -->
        <div *ngIf="currentStatus === 'upcoming'">
          <div *ngIf="upcomingBookings.length === 0" class="text-center py-8 sm:py-12">
            <mat-icon class="text-4xl sm:text-6xl text-gray-400 mb-4">schedule</mat-icon>
            <h2 class="text-lg sm:text-xl text-gray-600 mb-2">No upcoming bookings</h2>
            <p class="text-sm sm:text-base text-gray-500">You haven't made any future train bookings yet.</p>
          </div>
          <div *ngIf="upcomingBookings.length > 0" class="space-y-4">
            <mat-card *ngFor="let booking of upcomingBookings" class="booking-card">
              <ng-container *ngTemplateOutlet="bookingCardTemplate; context: { booking: booking, showActions: true }"></ng-container>
            </mat-card>
          </div>
        </div>

        <!-- Completed Bookings -->
        <div *ngIf="currentStatus === 'completed'">
          <div *ngIf="completedBookings.length === 0" class="text-center py-12">
            <mat-icon class="text-6xl text-gray-400 mb-4">check_circle</mat-icon>
            <h2 class="text-xl text-gray-600 mb-2">No completed journeys</h2>
            <p class="text-gray-500">Your completed train journeys will appear here.</p>
          </div>
          <div *ngIf="completedBookings.length > 0">
            <div class="flex justify-end mb-4">
              <button mat-button color="warn" (click)="removeAllBookings('completed')">
                <mat-icon>clear_all</mat-icon>
                Remove All
              </button>
            </div>
            <div class="space-y-4">
              <mat-card *ngFor="let booking of completedBookings" class="booking-card">
                <ng-container *ngTemplateOutlet="bookingCardTemplate; context: { booking: booking, showActions: false }"></ng-container>
              </mat-card>
            </div>
          </div>
        </div>

        <!-- Cancelled Bookings -->
        <div *ngIf="currentStatus === 'cancelled'">
          <div *ngIf="cancelledBookings.length === 0" class="text-center py-12">
            <mat-icon class="text-6xl text-gray-400 mb-4">cancel</mat-icon>
            <h2 class="text-xl text-gray-600 mb-2">No cancelled bookings</h2>
            <p class="text-gray-500">Your cancelled bookings will appear here.</p>
          </div>
          <div *ngIf="cancelledBookings.length > 0">
            <div class="flex justify-end mb-4">
              <button mat-button color="warn" (click)="removeAllBookings('cancelled')">
                <mat-icon>clear_all</mat-icon>
                Remove All
              </button>
            </div>
            <div class="space-y-4">
              <mat-card *ngFor="let booking of cancelledBookings" class="booking-card">
                <ng-container *ngTemplateOutlet="bookingCardTemplate; context: { booking: booking, showActions: false }"></ng-container>
              </mat-card>
            </div>
          </div>
        </div>
      </div>

      <!-- Booking Card Template -->
      <ng-template #bookingCardTemplate let-booking="booking" let-showActions="showActions">
          <mat-card-header class="pb-4">
            <div class="flex flex-col sm:flex-row sm:justify-between sm:items-start w-full space-y-3 sm:space-y-0">
              <div class="flex-1">
                <mat-card-title class="text-base sm:text-lg font-semibold mb-1">
                  {{ booking.trainNumber }} - {{ booking.trainName }}
                </mat-card-title>
                <mat-card-subtitle class="text-sm text-gray-600 mb-2">
                  {{ booking.route }}
                </mat-card-subtitle>
                <div class="flex flex-col sm:flex-row sm:items-center gap-2 sm:gap-4 text-xs sm:text-sm text-gray-700">
                  <div class="flex items-center gap-1">
                    <mat-icon class="text-sm">schedule</mat-icon>
                    <span>{{ booking.departureTime }} - {{ booking.arrivalTime }}</span>
                  </div>
                  <div class="flex items-center gap-1">
                    <mat-icon class="text-sm">calendar_today</mat-icon>
                    <span>{{ booking.travelDate }}</span>
                  </div>
                </div>
                <div class="mt-2 text-xs text-gray-500">
                  PNR: {{ booking.bookingReference }}
                  <span *ngIf="booking.trainClass" class="ml-2 px-2 py-1 bg-green-100 text-green-800 rounded text-xs">
                    {{ booking.trainClass }}
                  </span>
                  <span *ngIf="booking.selectedQuota && booking.selectedQuota !== 'General'" class="ml-2 px-2 py-1 bg-blue-100 text-blue-800 rounded text-xs">
                    {{ getQuotaLabel(booking.selectedQuota) }}
                  </span>
                </div>
              </div>
              <div class="flex flex-row sm:flex-col items-start sm:items-end gap-2 sm:gap-2">
                <mat-chip [class]="getStatusClass(booking.status)" class="text-xs">
                  {{ booking.status }}
                </mat-chip>
                <span class="text-base sm:text-lg font-bold text-green-600">
                  ₹{{ booking.totalAmount }}
                </span>
              </div>
            </div>
          </mat-card-header>

          <mat-card-content>
            <div class="mb-4">
              <h3 class="font-semibold text-gray-700 mb-2">Passengers</h3>
              <div class="grid grid-cols-1 gap-2">
                <div *ngFor="let passenger of booking.passengers; let i = index" 
                     class="flex flex-col sm:flex-row sm:justify-between sm:items-center p-2 bg-gray-50 rounded space-y-1 sm:space-y-0">
                  <div>
                    <span class="font-medium text-sm sm:text-base">{{ passenger.name }}</span>
                    <span class="text-xs sm:text-sm text-gray-600 ml-0 sm:ml-2 block sm:inline">
                      ({{ passenger.age }}{{ passenger.gender.charAt(0) }})
                    </span>
                  </div>
                  <div class="text-xs sm:text-sm font-medium">
                    <span *ngIf="passenger.seatNumber" class="text-blue-600">
                      Seat {{ passenger.seatNumber }}
                    </span>
                    <span *ngIf="!passenger.seatNumber && booking.status === 'Confirmed'" class="text-orange-600">
                      Seat pending
                    </span>
                    <span *ngIf="booking.status === 'Waitlisted'" class="text-orange-600">
                      WL {{ booking.waitlistPosition || 0 }}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <div *ngIf="booking.status === 'Confirmed' && booking.coachNumber" class="mb-4">
              <h3 class="font-semibold text-gray-700 mb-2">Seat Details</h3>
              <div class="flex items-center gap-4 p-3 bg-green-50 rounded">
                <mat-icon class="text-green-600">airline_seat_recline_normal</mat-icon>
                <div>
                  <p class="font-medium">Coach {{ booking.coachNumber }}</p>
                  <p class="text-sm text-gray-600">
                    {{ booking.passengers.length }} seat(s): {{ getPassengerSeats(booking).join(', ') }}
                  </p>
                  <p class="text-xs text-gray-500 mt-1">
                    Total passengers: {{ booking.passengers.length }}
                  </p>
                </div>
              </div>
            </div>

            <div *ngIf="booking.status === 'Waitlisted'" class="mb-4">
              <div class="flex items-center gap-4 p-3 rounded" 
                   [ngClass]="isCompleted(booking) ? 'bg-red-50' : 'bg-orange-50'">
                <mat-icon [class]="isCompleted(booking) ? 'text-red-600' : 'text-orange-600'">
                  {{ isCompleted(booking) ? 'cancel' : 'hourglass_empty' }}
                </mat-icon>
                <div>
                  <p class="font-medium" [class]="isCompleted(booking) ? 'text-red-700' : 'text-orange-700'">
                    {{ isCompleted(booking) ? 'Waitlist Expired' : 'Waitlist Position: ' + booking.waitlistPosition }}
                  </p>
                  <p class="text-sm text-gray-600">
                    <span *ngIf="isCompleted(booking)">Journey date passed - Waitlist no longer valid</span>
                    <span *ngIf="!isCompleted(booking)">You'll be notified when seats become available</span>
                  </p>
                </div>
              </div>
            </div>
          </mat-card-content>

          <mat-card-actions class="flex flex-col sm:flex-row justify-end gap-2">
            <!-- Upcoming bookings actions -->
            <div *ngIf="showActions" class="flex flex-col sm:flex-row gap-2">
              <button mat-button color="primary" (click)="downloadTicket(booking)" class="w-full sm:w-auto text-sm">
                <mat-icon class="text-sm sm:text-base">download</mat-icon>
                <span class="hidden sm:inline ml-1">Download Ticket</span>
                <span class="sm:hidden ml-1">Download</span>
              </button>
              <button mat-button color="warn" 
                      *ngIf="isCancellationAllowed(booking)" 
                      (click)="cancelBooking(booking)"
                      class="w-full sm:w-auto text-sm">
                <mat-icon class="text-sm sm:text-base">cancel</mat-icon>
                <span class="ml-1">Cancel</span>
              </button>
              <span *ngIf="!isCancellationAllowed(booking)" 
                    class="text-xs sm:text-sm text-gray-500 px-2 sm:px-3 py-1 text-center">
                <mat-icon class="text-sm mr-1">schedule</mat-icon>
                <span class="hidden sm:inline">Cannot cancel (< 30 min)</span>
                <span class="sm:hidden">Cannot cancel</span>
              </span>
            </div>
            
            <!-- Completed/Cancelled bookings actions -->
            <div *ngIf="!showActions" class="flex flex-col sm:flex-row gap-2">
              <button mat-button color="primary" 
                      *ngIf="booking.status === 'Confirmed'"
                      (click)="downloadTicket(booking)"
                      class="w-full sm:w-auto text-sm">
                <mat-icon class="text-sm sm:text-base">download</mat-icon>
                <span class="hidden sm:inline ml-1">Download Ticket</span>
                <span class="sm:hidden ml-1">Download</span>
              </button>
              <button mat-button color="accent" 
                      (click)="removeBooking(booking)"
                      class="w-full sm:w-auto text-sm">
                <mat-icon class="text-sm sm:text-base">delete_outline</mat-icon>
                <span class="hidden sm:inline ml-1">Remove from list</span>
                <span class="sm:hidden ml-1">Remove</span>
              </button>
            </div>
          </mat-card-actions>
      </ng-template>
    </div>
  `,
  styles: [`
    .booking-card {
      transition: transform 0.2s ease-in-out;
    }
    .booking-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    }
    .status-confirmed { background-color: #d4edda; color: #155724; }
    .status-waitlisted { background-color: #fff3cd; color: #856404; }
    .status-cancelled { background-color: #f8d7da; color: #721c24; }
  `]
})
export class MyBookingsComponent implements OnInit {
  bookings: BookingDetails[] = [];
  loading = true;
  currentStatus: 'upcoming' | 'completed' | 'cancelled' = 'upcoming';

  constructor(
    private bookingService: BookingService,
    private notificationService: NotificationService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  onStatusChange(status: 'upcoming' | 'completed' | 'cancelled'): void {
    this.currentStatus = status;
  }

  loadBookings(): void {
    this.loading = true;
    this.bookingService.getMyBookings().subscribe({
      next: (bookings) => {
        this.bookings = bookings;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading bookings:', error);
        this.notificationService.showError('Failed to load bookings');
        this.loading = false;
      }
    });
  }

  getStatusClass(status: string): string {
    return `status-${status.toLowerCase()}`;
  }

  getPassengerSeats(booking: BookingDetails): number[] {
    return booking.passengers
      .filter(p => p.seatNumber)
      .map(p => p.seatNumber!)
      .sort((a, b) => a - b);
  }

  getQuotaLabel(quota: string): string {
    const quotaLabels: { [key: string]: string } = {
      'Ladies': '👩 Ladies',
      'Senior': '👴 Senior',
      'Tatkal': '⚡ Tatkal'
    };
    return quotaLabels[quota] || quota;
  }

  get upcomingBookings(): BookingDetails[] {
    return this.bookings.filter(b => this.isUpcoming(b) && b.status !== 'Cancelled');
  }

  get completedBookings(): BookingDetails[] {
    return this.bookings.filter(b => this.isCompleted(b) && b.status !== 'Cancelled');
  }

  get cancelledBookings(): BookingDetails[] {
    return this.bookings.filter(b => b.status === 'Cancelled');
  }

  isUpcoming(booking: BookingDetails): boolean {
    const now = new Date();
    const travelDate = new Date(booking.travelDate);
    const [hours, minutes] = booking.departureTime.split(':').map(Number);
    const departureDateTime = new Date(travelDate);
    departureDateTime.setHours(hours, minutes, 0, 0);
    
    return departureDateTime > now;
  }

  isCompleted(booking: BookingDetails): boolean {
    const now = new Date();
    const travelDate = new Date(booking.travelDate);
    const [hours, minutes] = booking.arrivalTime.split(':').map(Number);
    const arrivalDateTime = new Date(travelDate);
    arrivalDateTime.setHours(hours, minutes, 0, 0);
    
    return arrivalDateTime <= now;
  }

  isCancellationAllowed(booking: BookingDetails): boolean {
    if (booking.status === 'Cancelled') return false;
    
    const now = new Date();
    const travelDate = new Date(booking.travelDate);
    const [hours, minutes] = booking.departureTime.split(':').map(Number);
    const departureDateTime = new Date(travelDate);
    departureDateTime.setHours(hours, minutes, 0, 0);
    
    const timeDifference = departureDateTime.getTime() - now.getTime();
    const minutesDifference = timeDifference / (1000 * 60);
    
    return minutesDifference >= 30;
  }

  cancelBooking(booking: BookingDetails): void {
    const dialogRef = this.dialog.open(CancelDialogComponent, {
      width: '400px',
      data: { bookingReference: booking.bookingReference }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.bookingService.cancelTrainBooking(booking.bookingId, result.reason).subscribe({
          next: (response) => {
            if (response.success) {
              this.notificationService.showSuccess('Booking cancelled successfully');
              this.loadBookings();
            } else {
              this.notificationService.showError(response.message || 'Failed to cancel booking');
            }
          },
          error: (error) => {
            console.error('Error cancelling booking:', error);
            this.notificationService.showError('Failed to cancel booking');
          }
        });
      }
    });
  }

  removeBooking(booking: BookingDetails): void {
    const dialogRef = this.dialog.open(RemoveBookingDialogComponent, {
      width: '400px',
      data: { bookingReference: booking.bookingReference }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.bookingService.removeBooking(booking.bookingId).subscribe({
          next: () => {
            this.bookings = this.bookings.filter(b => b.bookingId !== booking.bookingId);
            this.notificationService.showSuccess('Booking removed permanently');
          },
          error: (error) => {
            console.error('Error removing booking:', error);
            this.notificationService.showError('Failed to remove booking');
          }
        });
      }
    });
  }

  removeAllBookings(type: 'completed' | 'cancelled'): void {
    const bookingsToRemove = type === 'completed' ? this.completedBookings : this.cancelledBookings;
    const count = bookingsToRemove.length;
    
    const dialogRef = this.dialog.open(RemoveAllDialogComponent, {
      width: '400px',
      data: { type, count }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        const idsToRemove = bookingsToRemove.map(b => b.bookingId);
        this.bookingService.removeMultipleBookings(idsToRemove).subscribe({
          next: () => {
            this.bookings = this.bookings.filter(b => !idsToRemove.includes(b.bookingId));
            this.notificationService.showSuccess(`${count} ${type} bookings removed permanently`);
          },
          error: (error) => {
            console.error('Error removing bookings:', error);
            this.notificationService.showError('Failed to remove bookings');
          }
        });
      }
    });
  }

  downloadTicket(booking: BookingDetails): void {
    const doc = new jsPDF();
    
    // Header
    doc.setFontSize(20);
    doc.text('TRANSIT HUB', 105, 20, { align: 'center' });
    doc.setFontSize(16);
    doc.text('E-TICKET', 105, 30, { align: 'center' });
    
    // Booking Details
    doc.setFontSize(12);
    doc.text(`PNR: ${booking.bookingReference}`, 20, 50);
    doc.text(`Status: ${booking.status}`, 120, 50);
    
    // Train Details
    doc.text(`Train: ${booking.trainNumber} - ${booking.trainName}`, 20, 65);
    doc.text(`Route: ${booking.route}`, 20, 75);
    doc.text(`Date: ${booking.travelDate}`, 20, 85);
    doc.text(`Departure: ${booking.departureTime}`, 20, 95);
    doc.text(`Arrival: ${booking.arrivalTime}`, 120, 95);
    doc.text(`Class: ${booking.trainClass || 'Sleeper'}`, 20, 105);
    
    // Passenger Details
    doc.text('PASSENGER DETAILS:', 20, 125);
    let yPos = 135;
    booking.passengers.forEach((passenger, index) => {
      const seatInfo = passenger.seatNumber ? `Seat ${passenger.seatNumber}` : 
                      booking.status === 'Waitlisted' ? `WL ${booking.waitlistPosition || 0}` : 'Seat Pending';
      doc.text(`${index + 1}. ${passenger.name} (${passenger.age}${passenger.gender.charAt(0)}) - ${seatInfo}`, 25, yPos);
      yPos += 10;
    });
    
    // Amount
    doc.text(`Total Amount: Rs.${booking.totalAmount}`, 20, yPos + 10);
    
    // Footer
    doc.setFontSize(10);
    doc.text('Please carry a valid ID proof during travel.', 105, 280, { align: 'center' });
    doc.text(`Generated on: ${new Date().toLocaleString()}`, 105, 290, { align: 'center' });
    
    doc.save(`ticket-${booking.bookingReference}.pdf`);
    this.notificationService.showSuccess('Ticket downloaded successfully!');
  }
}