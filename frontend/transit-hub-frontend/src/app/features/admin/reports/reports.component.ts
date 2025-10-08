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
import { MatMenuModule } from '@angular/material/menu';
import { AdminService } from '../../../services/admin.service';
import { NotificationService } from '../../../services/notification.service';
import { BookingReport } from '../../../models/admin.dto';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-reports',
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
    MatChipsModule,
    MatMenuModule
  ],
  template: `
    <div class="p-6">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold text-gray-900">Booking Reports</h1>
        <button mat-raised-button color="primary" [matMenuTriggerFor]="exportMenu">
          <mat-icon>download</mat-icon>
          Export
        </button>
        <mat-menu #exportMenu="matMenu">
          <button mat-menu-item (click)="exportCSV()">
            <mat-icon>description</mat-icon>
            <span>Export as CSV</span>
          </button>
          <button mat-menu-item (click)="exportPDF()">
            <mat-icon>picture_as_pdf</mat-icon>
            <span>Export as PDF</span>
          </button>
        </mat-menu>
      </div>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <mat-card class="p-4">
          <div class="text-center">
            <h3 class="text-lg font-semibold text-gray-700">Total Bookings</h3>
            <p class="text-3xl font-bold text-blue-600">{{ bookings.length }}</p>
          </div>
        </mat-card>
        
        <mat-card class="p-4">
          <div class="text-center">
            <h3 class="text-lg font-semibold text-gray-700">Total Revenue</h3>
            <p class="text-2xl md:text-3xl font-bold text-green-600 break-words">Rs.{{ getTotalRevenue() | number:'1.0-0' }}</p>
          </div>
        </mat-card>
        
        <mat-card class="p-4">
          <div class="text-center">
            <h3 class="text-lg font-semibold text-gray-700">Train Bookings</h3>
            <p class="text-3xl font-bold text-purple-600">{{ getTrainBookings() }}</p>
          </div>
        </mat-card>
        
        <mat-card class="p-4">
          <div class="text-center">
            <h3 class="text-lg font-semibold text-gray-700">Flight Bookings</h3>
            <p class="text-3xl font-bold text-orange-600">{{ getFlightBookings() }}</p>
          </div>
        </mat-card>
      </div>

      <!-- Search -->
      <div class="mb-6">
        <mat-form-field appearance="fill" class="w-80">
          <mat-label>Search bookings</mat-label>
          <input matInput (keyup)="applyFilter($event)" placeholder="Search by user email or booking reference">
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
      </div>

      <!-- Loading Spinner -->
      <div *ngIf="loading" class="flex justify-center items-center h-64">
        <mat-spinner></mat-spinner>
      </div>

      <!-- Bookings Table -->
      <mat-card *ngIf="!loading">
        <div class="overflow-x-auto">
          <table mat-table [dataSource]="filteredBookings" class="w-full">
            
            <!-- Booking Reference Column -->
            <ng-container matColumnDef="bookingReference">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Booking Ref</th>
              <td mat-cell *matCellDef="let booking">{{ booking.bookingReference }}</td>
            </ng-container>

            <!-- User Column -->
            <ng-container matColumnDef="user">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">User</th>
              <td mat-cell *matCellDef="let booking">
                <div>
                  <div class="font-medium">{{ booking.userName }}</div>
                  <div class="text-sm text-gray-500">{{ booking.userEmail }}</div>
                </div>
              </td>
            </ng-container>

            <!-- Transport Column -->
            <ng-container matColumnDef="transport">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Transport</th>
              <td mat-cell *matCellDef="let booking">
                <div>
                  <mat-chip [class]="booking.transportType === 'Train' ? 'bg-purple-100 text-purple-800' : 'bg-orange-100 text-orange-800'">
                    {{ booking.transportType }}
                  </mat-chip>
                  <div class="text-sm mt-1">{{ booking.transportNumber }}</div>
                </div>
              </td>
            </ng-container>

            <!-- Route Column -->
            <ng-container matColumnDef="route">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Route</th>
              <td mat-cell *matCellDef="let booking">{{ booking.route || 'N/A' }}</td>
            </ng-container>

            <!-- Booking Date Column -->
            <ng-container matColumnDef="bookingDate">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Booking Date</th>
              <td mat-cell *matCellDef="let booking">{{ formatLocalDate(booking.bookingDate) }}</td>
            </ng-container>

            <!-- Travel Date Column -->
            <ng-container matColumnDef="travelDate">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Travel Date</th>
              <td mat-cell *matCellDef="let booking">{{ formatLocalDate(booking.travelDate) }}</td>
            </ng-container>

            <!-- Amount Column -->
            <ng-container matColumnDef="amount">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Amount</th>
              <td mat-cell *matCellDef="let booking">
                <span class="font-medium text-green-600">Rs.{{ booking.amount | number:'1.0-0' }}</span>
              </td>
            </ng-container>

            <!-- Status Column -->
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef class="font-semibold">Status</th>
              <td mat-cell *matCellDef="let booking">
                <div>
                  <span [class]="getStatusClass(booking.status)" 
                        class="px-2 py-1 rounded-full text-xs font-medium">
                    {{ booking.status }}
                  </span>
                  <div class="text-xs mt-1" [class]="getPaymentStatusClass(booking.paymentStatus)">
                    Payment: {{ booking.paymentStatus }}
                  </div>
                </div>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </div>

        <!-- No Data Message -->
        <div *ngIf="filteredBookings.length === 0 && !loading" class="text-center py-8 text-gray-500">
          No bookings found
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
export class ReportsComponent implements OnInit {
  bookings: BookingReport[] = [];
  filteredBookings: BookingReport[] = [];
  displayedColumns: string[] = ['bookingReference', 'user', 'transport', 'route', 'bookingDate', 'travelDate', 'amount', 'status'];
  loading = true;

  constructor(
    private adminService: AdminService,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    this.loadBookings();
  }

  loadBookings(): void {
    this.loading = true;
    this.adminService.getBookingReports().subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success && response.data) {
          this.bookings = response.data.sort((a: BookingReport, b: BookingReport) => new Date(b.bookingDate).getTime() - new Date(a.bookingDate).getTime());
          this.filteredBookings = [...this.bookings];
        } else {
          this.notificationService.showError('Failed to load booking reports');
        }
      },
      error: (error) => {
        this.loading = false;
        console.error('Error loading booking reports:', error);
        this.notificationService.showError('Failed to load booking reports');
      }
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value.toLowerCase();
    this.filteredBookings = this.bookings.filter(booking => 
      booking.userEmail.toLowerCase().includes(filterValue) ||
      booking.bookingReference.toLowerCase().includes(filterValue) ||
      booking.userName.toLowerCase().includes(filterValue)
    );
  }

  getTotalRevenue(): number {
    return this.bookings.reduce((total, booking) => total + booking.amount, 0);
  }

  getTrainBookings(): number {
    return this.bookings.filter(booking => booking.transportType === 'Train').length;
  }

  getFlightBookings(): number {
    return this.bookings.filter(booking => booking.transportType === 'Flight').length;
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'confirmed':
        return 'text-green-600 bg-green-100';
      case 'pending':
        return 'text-yellow-600 bg-yellow-100';
      case 'cancelled':
        return 'text-red-600 bg-red-100';
      default:
        return 'text-gray-600 bg-gray-100';
    }
  }

  getPaymentStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'completed':
        return 'text-green-600';
      case 'pending':
        return 'text-yellow-600';
      case 'failed':
        return 'text-red-600';
      default:
        return 'text-gray-600';
    }
  }

  exportCSV(): void {
    const csvData = this.convertToCSV(this.filteredBookings);
    const blob = new Blob([csvData], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `booking-reports-${new Date().toISOString().split('T')[0]}.csv`;
    link.click();
    window.URL.revokeObjectURL(url);
    this.notificationService.showSuccess('CSV report exported successfully');
  }

  exportPDF(): void {
    const doc = new jsPDF();
    
    // Add title
    doc.setFontSize(20);
    doc.text('Booking Reports', 14, 22);
    
    // Add summary
    doc.setFontSize(12);
    doc.text(`Total Bookings: ${this.bookings.length}`, 14, 35);
    doc.text(`Total Revenue: Rs.${this.getTotalRevenue().toLocaleString()}`, 14, 42);
    doc.text(`Generated on: ${new Date().toLocaleDateString()}`, 14, 49);
    
    // Add table
    const tableData = this.filteredBookings.map(booking => [
      booking.bookingReference,
      booking.userName,
      booking.userEmail,
      booking.transportType,
      this.formatLocalDate(booking.bookingDate),
      `Rs.${booking.amount.toLocaleString()}`,
      booking.status
    ]);
    
    autoTable(doc, {
      head: [['Booking Ref', 'User', 'Email', 'Type', 'Date', 'Amount', 'Status']],
      body: tableData,
      startY: 60,
      styles: { fontSize: 8 },
      headStyles: { fillColor: [63, 81, 181] }
    });
    
    doc.save(`booking-reports-${new Date().toISOString().split('T')[0]}.pdf`);
    this.notificationService.showSuccess('PDF report exported successfully');
  }

  formatLocalDate(dateInput: string | Date): string {
    const date = typeof dateInput === 'string' ? new Date(dateInput) : dateInput;
    const now = new Date();
    const istOffset = 5.5 * 60 * 60 * 1000; // IST is UTC+5:30
    const localDate = new Date(date.getTime() + istOffset);
    
    return localDate.toLocaleString('en-IN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      hour12: true
    });
  }

  private convertToCSV(data: BookingReport[]): string {
    const headers = ['Booking Reference', 'User Name', 'User Email', 'Transport Type', 'Transport Number', 'Route', 'Booking Date', 'Travel Date', 'Amount', 'Status', 'Payment Status'];
    const csvRows = [headers.join(',')];
    
    data.forEach(booking => {
      const row = [
        booking.bookingReference,
        booking.userName,
        booking.userEmail,
        booking.transportType,
        booking.transportNumber,
        booking.route || 'N/A',
        this.formatLocalDate(booking.bookingDate),
        this.formatLocalDate(booking.travelDate),
        booking.amount.toString(),
        booking.status,
        booking.paymentStatus
      ];
      csvRows.push(row.join(','));
    });
    
    return csvRows.join('\n');
  }
}