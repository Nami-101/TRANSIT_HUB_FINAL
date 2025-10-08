import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-cancel-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title class="text-center">
      <mat-icon class="text-red-600 text-3xl">cancel</mat-icon>
      Cancel Booking
    </h2>
    
    <mat-dialog-content class="text-center py-4">
      <p class="text-lg mb-2">Are you sure you want to cancel this booking?</p>
      <p class="font-semibold text-blue-600">PNR: {{ data.bookingReference }}</p>
      <p class="text-sm text-gray-600 mt-2">This action cannot be undone.</p>
    </mat-dialog-content>

    <mat-dialog-actions class="flex justify-center gap-4 p-4">
      <button mat-button (click)="onCancel()" class="px-6">
        Keep Booking
      </button>
      <button mat-raised-button color="warn" (click)="onConfirm()" class="px-6">
        Yes, Cancel
      </button>
    </mat-dialog-actions>
  `
})
export class CancelDialogComponent {
  constructor(
    private dialogRef: MatDialogRef<CancelDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}