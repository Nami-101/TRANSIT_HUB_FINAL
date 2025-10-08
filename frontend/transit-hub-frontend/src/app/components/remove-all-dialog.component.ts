import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-remove-all-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="p-6">
      <div class="flex items-center mb-4">
        <mat-icon class="text-red-500 mr-3 text-3xl">warning</mat-icon>
        <h2 class="text-xl font-semibold text-gray-800">Remove All {{ data.type | titlecase }} Bookings</h2>
      </div>
      
      <div class="mb-6">
        <p class="text-gray-600 mb-2">
          You're about to clean up <strong>{{ data.count }}</strong> {{ data.type }} bookings from your list.
        </p>
        <p class="text-sm text-gray-500">
          ✨ This will help keep your booking history organized and clutter-free!
        </p>
      </div>
      
      <div class="flex justify-end gap-3">
        <button mat-button (click)="onCancel()" class="text-gray-600">
          Cancel
        </button>
        <button mat-raised-button color="warn" (click)="onConfirm()">
          <mat-icon class="mr-1">delete_sweep</mat-icon>
          Remove All
        </button>
      </div>
    </div>
  `
})
export class RemoveAllDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<RemoveAllDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { type: string; count: number }
  ) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}