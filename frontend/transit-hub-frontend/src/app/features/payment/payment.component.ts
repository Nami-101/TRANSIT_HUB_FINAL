import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="payment-dialog">
      <h2 mat-dialog-title class="text-center">
        <mat-icon class="text-green-600 text-3xl">payment</mat-icon>
        Complete Payment
      </h2>
      
      <mat-dialog-content class="payment-content">
        <!-- Booking Summary -->
        <div class="booking-summary mb-6 p-4 bg-blue-50 rounded-lg">
          <h3 class="font-semibold text-blue-800 mb-2">Booking Summary</h3>
          <div class="flex justify-between items-center">
            <span>{{ data.passengerCount }} Passenger(s)</span>
            <span class="font-bold text-green-600">₹{{ data.totalAmount }}</span>
          </div>
          <div class="text-sm text-gray-600 mt-1">
            {{ data.trainName }} • {{ data.travelDate }}
          </div>
        </div>

        <!-- Payment Form -->
        <form [formGroup]="paymentForm" class="payment-form">
          <div class="mb-4">
            <mat-form-field appearance="outline" class="w-full">
              <mat-label>Cardholder Name</mat-label>
              <input matInput formControlName="cardholderName" placeholder="John Doe">
              <mat-icon matSuffix>person</mat-icon>
            </mat-form-field>
          </div>

          <div class="mb-4">
            <mat-form-field appearance="outline" class="w-full">
              <mat-label>Card Number</mat-label>
              <input matInput formControlName="cardNumber" 
                     placeholder="1234 5678 9012 3456"
                     maxlength="19"
                     (input)="formatCardNumber($event)">
              <mat-icon matSuffix>credit_card</mat-icon>
            </mat-form-field>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
            <mat-form-field appearance="outline">
              <mat-label>Expiry Date</mat-label>
              <input matInput formControlName="expiryDate" 
                     placeholder="MM/YY"
                     maxlength="5"
                     (input)="formatExpiryDate($event)">
              <mat-icon matSuffix>date_range</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>CVV</mat-label>
              <input matInput formControlName="cvv" 
                     placeholder="123"
                     maxlength="3"
                     type="password">
              <mat-icon matSuffix>security</mat-icon>
            </mat-form-field>
          </div>

          <!-- Payment Method -->
          <div class="mb-6">
            <h4 class="font-semibold mb-3">Payment Method</h4>
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-2">
              <button type="button" 
                      class="payment-method-btn"
                      [class.selected]="selectedPaymentMethod === 'visa'"
                      (click)="selectPaymentMethod('visa')">
                <img src="https://upload.wikimedia.org/wikipedia/commons/4/41/Visa_Logo.png" 
                     alt="Visa" class="h-6">
              </button>
              <button type="button" 
                      class="payment-method-btn"
                      [class.selected]="selectedPaymentMethod === 'mastercard'"
                      (click)="selectPaymentMethod('mastercard')">
                <img src="https://upload.wikimedia.org/wikipedia/commons/2/2a/Mastercard-logo.svg" 
                     alt="Mastercard" class="h-6">
              </button>
              <button type="button" 
                      class="payment-method-btn"
                      [class.selected]="selectedPaymentMethod === 'rupay'"
                      (click)="selectPaymentMethod('rupay')">
                <span class="text-orange-600 font-bold">RuPay</span>
              </button>
            </div>
          </div>

          <!-- Security Notice -->
          <div class="security-notice p-3 bg-green-50 rounded-lg mb-4">
            <div class="flex items-center gap-2">
              <mat-icon class="text-green-600">security</mat-icon>
              <span class="text-sm text-green-700">
                Your payment is secured with 256-bit SSL encryption
              </span>
            </div>
          </div>
        </form>
      </mat-dialog-content>

      <mat-dialog-actions class="flex justify-between p-4">
        <button mat-button (click)="onCancel()" [disabled]="processing">
          Cancel
        </button>
        <button mat-raised-button 
                color="primary" 
                (click)="processPayment()"
                [disabled]="paymentForm.invalid || processing"
                class="px-8">
          <mat-spinner *ngIf="processing" diameter="20" class="mr-2"></mat-spinner>
          <mat-icon *ngIf="!processing">payment</mat-icon>
          {{ processing ? 'Processing...' : 'Pay ₹' + data.totalAmount }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .payment-dialog {
      min-width: 400px;
      max-width: 500px;
      width: 100%;
    }
    .payment-content {
      max-height: 600px;
      overflow-y: auto;
    }
    .payment-method-btn {
      padding: 12px;
      border: 2px solid #e5e7eb;
      border-radius: 8px;
      background: white;
      cursor: pointer;
      transition: all 0.2s;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .payment-method-btn:hover {
      border-color: #3b82f6;
    }
    .payment-method-btn.selected {
      border-color: #3b82f6;
      background-color: #eff6ff;
    }
    
    @media (max-width: 768px) {
      .payment-dialog {
        min-width: 300px;
        max-width: 100%;
      }
      
      .payment-content {
        max-height: 500px;
      }
      
      .grid-cols-2 {
        grid-template-columns: 1fr;
        gap: 12px;
      }
      
      .grid-cols-3 {
        grid-template-columns: 1fr;
        gap: 8px;
      }
      
      .payment-method-btn {
        padding: 10px;
      }
    }
    
    @media (max-width: 480px) {
      .payment-dialog {
        min-width: 280px;
      }
      
      .payment-content {
        max-height: 400px;
      }
    }
  `]
})
export class PaymentComponent {
  paymentForm: FormGroup;
  processing = false;
  selectedPaymentMethod = 'visa';

  constructor(
    private fb: FormBuilder,
    private notificationService: NotificationService,
    private dialogRef: MatDialogRef<PaymentComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.paymentForm = this.fb.group({
      cardholderName: ['', Validators.required],
      cardNumber: ['', [Validators.required, Validators.minLength(19)]],
      expiryDate: ['', [Validators.required, Validators.pattern(/^\d{2}\/\d{2}$/)]],
      cvv: ['', [Validators.required, Validators.minLength(3)]]
    });
  }

  formatCardNumber(event: any): void {
    let value = event.target.value.replace(/\s/g, '');
    let formattedValue = value.replace(/(.{4})/g, '$1 ').trim();
    if (formattedValue.length > 19) {
      formattedValue = formattedValue.substring(0, 19);
    }
    this.paymentForm.patchValue({ cardNumber: formattedValue });
  }

  formatExpiryDate(event: any): void {
    let value = event.target.value.replace(/\D/g, '');
    if (value.length >= 2) {
      value = value.substring(0, 2) + '/' + value.substring(2, 4);
    }
    this.paymentForm.patchValue({ expiryDate: value });
  }

  selectPaymentMethod(method: string): void {
    this.selectedPaymentMethod = method;
  }

  processPayment(): void {
    if (this.paymentForm.valid) {
      this.processing = true;
      
      // Simulate payment processing
      setTimeout(() => {
        this.processing = false;
        this.notificationService.showSuccess('Payment successful! 🎉');
        this.dialogRef.close({ success: true, paymentMethod: this.selectedPaymentMethod });
      }, 2000);
    }
  }

  onCancel(): void {
    this.dialogRef.close({ success: false });
  }
}