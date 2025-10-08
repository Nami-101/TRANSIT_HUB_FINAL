import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  constructor(private snackBar: MatSnackBar) {}

  showSuccess(message: string, duration: number = 3000) {
    this.snackBar.open(message, 'Close', {
      duration,
      panelClass: ['success-toast'],
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }

  showError(message: string, duration: number = 4000) {
    this.snackBar.open(message, 'Close', {
      duration,
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }

  showWarning(message: string, duration: number = 3500) {
    this.snackBar.open(message, 'Close', {
      duration,
      panelClass: ['warning-toast'],
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }
}