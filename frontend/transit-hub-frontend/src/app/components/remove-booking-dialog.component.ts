import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-remove-booking-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="p-6">
      <div class="flex items-center mb-4">
        <mat-icon class="text-blue-500 mr-3 text-3xl">cleaning_services</mat-icon>
        <h2 class="text-xl font-semibold text-gray-800">Clean Up Booking</h2>
      </div>
      
      <div class="mb-6">
        <p class="text-gray-600 mb-2">
          Remove booking <strong>{{ data.bookingReference }}</strong> from your list?
        </p>
        <p class="text-sm text-gray-500">
          🧹 Keep your booking history neat and organized!
        </p>
      </div>
      
      <div class="flex justify-end gap-3">
        <button mat-button (click)="onCancel()" class="text-gray-600">
          Keep it
        </button>
        <button mat-raised-button color="primary" (click)="onConfirm()">
          <mat-icon class="mr-1">delete_outline</mat-icon>
          Remove
        </button>
      </div>
    </div>
  `
})
export class RemoveBookingDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<RemoveBookingDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { bookingReference: string }
  ) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}