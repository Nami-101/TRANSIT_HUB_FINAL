import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { HttpClient } from '@angular/common/http';

export interface Seat {
  number: number;
  isOccupied: boolean;
  isSelected: boolean;
  type: 'window' | 'aisle' | 'middle';
  row: number;
  position: 'A' | 'B' | 'C' | 'D';
}

@Component({
  selector: 'app-seat-map',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatChipsModule
  ],
  template: `
    <div class="seat-map-container">
      <!-- Header -->
      <div class="seat-map-header">
        <div class="train-info">
          <mat-icon class="train-icon">train</mat-icon>
          <div>
            <h2 class="coach-title">Coach {{ currentCoach }}</h2>
            <p class="coach-subtitle">{{ totalSeats }} seats per coach</p>
          </div>
        </div>
        <div class="selection-info">
          <span class="selected-count">{{ selectedSeats.length }}/{{ maxSeats }} selected</span>
        </div>
      </div>

      <!-- Coach Navigation -->
      <div class="coach-navigation" *ngIf="totalCoaches > 1">
        <button mat-icon-button (click)="previousCoach()" [disabled]="currentCoach === 1">
          <mat-icon>chevron_left</mat-icon>
        </button>
        <span class="coach-info">Coach {{ currentCoach }} of {{ totalCoaches }}</span>
        <button mat-icon-button (click)="nextCoach()" [disabled]="currentCoach === totalCoaches">
          <mat-icon>chevron_right</mat-icon>
        </button>
      </div>

      <!-- Legend -->
      <div class="seat-legend">
        <div class="legend-item">
          <div class="seat-demo available"></div>
          <span>Available</span>
        </div>
        <div class="legend-item">
          <div class="seat-demo occupied"></div>
          <span>Occupied</span>
        </div>
        <div class="legend-item">
          <div class="seat-demo selected"></div>
          <span>Selected</span>
        </div>
        <div class="legend-item">
          <mat-icon class="window-icon">visibility</mat-icon>
          <span>Window</span>
        </div>
      </div>

      <!-- Coach Layout -->
      <div class="coach-container">
        <div class="coach-body">
          <!-- Front of train indicator -->
          <div class="train-direction">
            <mat-icon>arrow_forward</mat-icon>
            <span>Front of Train</span>
          </div>

          <!-- Seat Grid -->
          <div class="seat-grid">
            <div *ngFor="let row of seatRows; let rowIndex = index" class="seat-row">
              <div class="row-number">{{ row.rowNumber }}</div>
              
              <!-- Left side seats (A, B) -->
              <div class="seat-section left">
                <div *ngFor="let seat of row.leftSeats" 
                     class="seat-container"
                     [class.window]="seat.type === 'window'"
                     [class.aisle]="seat.type === 'aisle'">
                  <div class="seat"
                       [class.available]="!seat.isOccupied && !seat.isSelected"
                       [class.occupied]="seat.isOccupied"
                       [class.selected]="seat.isSelected"
                       [class.pulse]="seat.isSelected"
                       [matTooltip]="getSeatTooltip(seat)"
                       (click)="toggleSeat(seat)"
                       [style.animation-delay]="(rowIndex * 50) + 'ms'">
                    <div class="seat-number">{{ seat.number }}</div>
                    <div class="seat-position">{{ seat.position }}</div>
                    <mat-icon *ngIf="seat.type === 'window'" class="seat-icon">visibility</mat-icon>
                    <mat-icon *ngIf="seat.isSelected" class="selected-icon">check_circle</mat-icon>
                  </div>
                </div>
              </div>

              <!-- Aisle -->
              <div class="aisle"></div>

              <!-- Right side seats (C, D) -->
              <div class="seat-section right">
                <div *ngFor="let seat of row.rightSeats" 
                     class="seat-container"
                     [class.window]="seat.type === 'window'"
                     [class.aisle]="seat.type === 'aisle'">
                  <div class="seat"
                       [class.available]="!seat.isOccupied && !seat.isSelected"
                       [class.occupied]="seat.isOccupied"
                       [class.selected]="seat.isSelected"
                       [class.pulse]="seat.isSelected"
                       [matTooltip]="getSeatTooltip(seat)"
                       (click)="toggleSeat(seat)"
                       [style.animation-delay]="(rowIndex * 50 + 25) + 'ms'">
                    <div class="seat-number">{{ seat.number }}</div>
                    <div class="seat-position">{{ seat.position }}</div>
                    <mat-icon *ngIf="seat.type === 'window'" class="seat-icon">visibility</mat-icon>
                    <mat-icon *ngIf="seat.isSelected" class="selected-icon">check_circle</mat-icon>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Back of train indicator -->
          <div class="train-direction back">
            <span>Back of Train</span>
            <mat-icon>arrow_back</mat-icon>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="action-buttons">
        <button mat-button (click)="clearSelection()" [disabled]="selectedSeats.length === 0">
          <mat-icon>clear</mat-icon>
          Clear Selection
        </button>
        <button mat-raised-button color="primary" 
                (click)="confirmSelection()" 
                [disabled]="selectedSeats.length === 0"
                class="confirm-btn">
          <mat-icon>check</mat-icon>
          Confirm Seats ({{ selectedSeats.length }})
        </button>
      </div>
    </div>
  `,
  styles: [`
    .seat-map-container {
      max-width: 800px;
      margin: 0 auto;
      padding: 20px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 20px;
      box-shadow: 0 20px 40px rgba(0,0,0,0.1);
      color: white;
      width: 100%;
      box-sizing: border-box;
    }

    .seat-map-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      padding: 20px;
      background: rgba(255,255,255,0.1);
      border-radius: 15px;
      backdrop-filter: blur(10px);
    }

    .train-info {
      display: flex;
      align-items: center;
      gap: 15px;
    }

    .train-icon {
      font-size: 2.5rem;
      color: #ffd700;
      animation: trainMove 2s ease-in-out infinite alternate;
    }

    @keyframes trainMove {
      0% { transform: translateX(0); }
      100% { transform: translateX(10px); }
    }

    .coach-title {
      font-size: 1.5rem;
      font-weight: bold;
      margin: 0;
      text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
    }

    .coach-subtitle {
      margin: 0;
      opacity: 0.8;
      font-size: 0.9rem;
    }

    .selection-info {
      text-align: right;
    }

    .selected-count {
      font-size: 1.2rem;
      font-weight: bold;
      color: #ffd700;
    }

    .seat-legend {
      display: flex;
      justify-content: center;
      gap: 30px;
      margin-bottom: 30px;
      flex-wrap: wrap;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 0.9rem;
    }

    .seat-demo {
      width: 20px;
      height: 20px;
      border-radius: 6px;
      border: 2px solid rgba(255,255,255,0.3);
    }

    .seat-demo.available {
      background: linear-gradient(145deg, #4CAF50, #45a049);
      box-shadow: 0 4px 8px rgba(76,175,80,0.3);
    }

    .seat-demo.occupied {
      background: linear-gradient(145deg, #f44336, #d32f2f);
      box-shadow: 0 4px 8px rgba(244,67,54,0.3);
    }

    .seat-demo.selected {
      background: linear-gradient(145deg, #ffd700, #ffb300);
      box-shadow: 0 4px 8px rgba(255,215,0,0.4);
    }

    .window-icon {
      color: #87CEEB;
      font-size: 1.2rem;
    }

    .coach-container {
      background: rgba(255,255,255,0.1);
      border-radius: 20px;
      padding: 30px;
      backdrop-filter: blur(10px);
      border: 2px solid rgba(255,255,255,0.2);
    }

    .coach-body {
      background: linear-gradient(180deg, #2c3e50, #34495e);
      border-radius: 15px;
      padding: 20px;
      box-shadow: inset 0 4px 8px rgba(0,0,0,0.3);
    }

    .train-direction {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
      padding: 15px;
      background: rgba(255,215,0,0.2);
      border-radius: 10px;
      margin-bottom: 20px;
      font-weight: bold;
      color: #ffd700;
    }

    .train-direction.back {
      margin-top: 20px;
      margin-bottom: 0;
    }

    .seat-grid {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .seat-row {
      display: flex;
      align-items: center;
      gap: 15px;
      animation: slideInUp 0.6s ease-out forwards;
      opacity: 0;
      transform: translateY(20px);
    }

    @keyframes slideInUp {
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .row-number {
      width: 30px;
      text-align: center;
      font-weight: bold;
      color: #ffd700;
      font-size: 0.9rem;
    }

    .seat-section {
      display: flex;
      gap: 8px;
    }

    .aisle {
      width: 40px;
      background: linear-gradient(90deg, transparent, rgba(255,255,255,0.1), transparent);
      border-radius: 20px;
      position: relative;
    }

    .aisle::after {
      content: '';
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      width: 2px;
      height: 80%;
      background: rgba(255,255,255,0.3);
      border-radius: 1px;
    }

    .seat-container {
      position: relative;
    }

    .seat {
      width: 50px;
      height: 50px;
      border-radius: 12px;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      position: relative;
      border: 2px solid transparent;
      font-size: 0.7rem;
      font-weight: bold;
    }

    .seat.available {
      background: linear-gradient(145deg, #4CAF50, #45a049);
      box-shadow: 0 6px 12px rgba(76,175,80,0.3);
      border-color: rgba(255,255,255,0.2);
    }

    .seat.available:hover {
      transform: translateY(-3px) scale(1.05);
      box-shadow: 0 8px 16px rgba(76,175,80,0.4);
      border-color: rgba(255,255,255,0.4);
    }

    .seat.occupied {
      background: linear-gradient(145deg, #f44336, #d32f2f);
      box-shadow: 0 4px 8px rgba(244,67,54,0.3);
      cursor: not-allowed;
      opacity: 0.7;
    }

    .seat.selected {
      background: linear-gradient(145deg, #ffd700, #ffb300);
      box-shadow: 0 8px 16px rgba(255,215,0,0.4);
      transform: translateY(-2px);
      border-color: rgba(255,255,255,0.6);
    }

    .seat.pulse {
      animation: pulse 1.5s ease-in-out infinite;
    }

    @keyframes pulse {
      0%, 100% { box-shadow: 0 8px 16px rgba(255,215,0,0.4); }
      50% { box-shadow: 0 8px 20px rgba(255,215,0,0.6), 0 0 20px rgba(255,215,0,0.3); }
    }

    .seat-number {
      font-size: 0.8rem;
      font-weight: bold;
      line-height: 1;
    }

    .seat-position {
      font-size: 0.6rem;
      opacity: 0.8;
      line-height: 1;
    }

    .seat-icon {
      position: absolute;
      top: -8px;
      right: -8px;
      font-size: 1rem;
      color: #87CEEB;
      background: rgba(0,0,0,0.5);
      border-radius: 50%;
      padding: 2px;
    }

    .selected-icon {
      position: absolute;
      top: -8px;
      right: -8px;
      font-size: 1.2rem;
      color: #4CAF50;
      background: white;
      border-radius: 50%;
      animation: checkmark 0.3s ease-out;
    }

    @keyframes checkmark {
      0% { transform: scale(0) rotate(180deg); }
      100% { transform: scale(1) rotate(0deg); }
    }

    .action-buttons {
      display: flex;
      justify-content: space-between;
      gap: 15px;
      margin-top: 30px;
      padding: 20px;
      background: rgba(255,255,255,0.1);
      border-radius: 15px;
      backdrop-filter: blur(10px);
    }

    .confirm-btn {
      background: linear-gradient(145deg, #4CAF50, #45a049) !important;
      color: white !important;
      font-weight: bold;
      padding: 12px 24px;
      border-radius: 25px;
      box-shadow: 0 6px 12px rgba(76,175,80,0.3);
      transition: all 0.3s ease;
    }

    .confirm-btn:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 8px 16px rgba(76,175,80,0.4);
    }

    .confirm-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .coach-navigation {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 20px;
      margin-bottom: 20px;
      padding: 15px;
      background: rgba(255,255,255,0.1);
      border-radius: 15px;
      backdrop-filter: blur(10px);
    }

    .coach-info {
      font-weight: bold;
      color: #ffd700;
      font-size: 1.1rem;
    }

    /* Mobile Responsive */
    @media (max-width: 768px) {
      .seat-map-container {
        padding: 10px;
        margin: 5px;
        max-width: 100%;
        overflow-x: hidden;
      }
      
      .seat-map-header {
        flex-direction: column;
        gap: 10px;
        text-align: center;
        padding: 15px;
      }
      
      .train-icon {
        font-size: 2rem;
      }
      
      .coach-title {
        font-size: 1.2rem;
      }
      
      .seat-legend {
        gap: 10px;
        justify-content: space-around;
      }
      
      .legend-item {
        font-size: 0.8rem;
        gap: 5px;
      }
      
      .seat-demo {
        width: 16px;
        height: 16px;
      }
      
      .coach-container {
        padding: 15px;
      }
      
      .coach-body {
        padding: 15px;
      }
      
      .seat-grid {
        gap: 8px;
      }
      
      .seat-row {
        gap: 8px;
      }
      
      .row-number {
        width: 25px;
        font-size: 0.8rem;
      }
      
      .seat-section {
        gap: 4px;
      }
      
      .aisle {
        width: 25px;
      }
      
      .seat {
        width: 35px;
        height: 35px;
        font-size: 0.6rem;
        border-radius: 8px;
      }
      
      .seat-number {
        font-size: 0.7rem;
      }
      
      .seat-position {
        font-size: 0.5rem;
      }
      
      .seat-icon {
        font-size: 0.8rem;
        top: -6px;
        right: -6px;
      }
      
      .selected-icon {
        font-size: 1rem;
        top: -6px;
        right: -6px;
      }
      
      .train-direction {
        padding: 10px;
        font-size: 0.9rem;
      }
      
      .action-buttons {
        flex-direction: column;
        gap: 10px;
        padding: 15px;
      }
      
      .confirm-btn {
        padding: 10px 20px;
        font-size: 0.9rem;
      }
      
      .coach-navigation {
        padding: 10px;
      }
      
      .coach-info {
        font-size: 1rem;
      }
    }
    
    @media (max-width: 480px) {
      .seat-map-container {
        padding: 8px;
        margin: 2px;
      }
      
      .seat-map-header {
        padding: 10px;
      }
      
      .coach-title {
        font-size: 1.1rem;
      }
      
      .selected-count {
        font-size: 1rem;
      }
      
      .seat-legend {
        gap: 8px;
        flex-wrap: wrap;
      }
      
      .legend-item {
        font-size: 0.7rem;
        min-width: 70px;
      }
      
      .coach-container {
        padding: 10px;
      }
      
      .coach-body {
        padding: 10px;
      }
      
      .seat-grid {
        gap: 6px;
      }
      
      .seat-row {
        gap: 6px;
      }
      
      .row-number {
        width: 20px;
        font-size: 0.7rem;
      }
      
      .seat-section {
        gap: 3px;
      }
      
      .aisle {
        width: 20px;
      }
      
      .seat {
        width: 30px;
        height: 30px;
        font-size: 0.5rem;
        border-radius: 6px;
      }
      
      .seat-number {
        font-size: 0.6rem;
      }
      
      .seat-position {
        font-size: 0.4rem;
      }
      
      .seat-icon {
        font-size: 0.7rem;
        top: -4px;
        right: -4px;
        padding: 1px;
      }
      
      .selected-icon {
        font-size: 0.9rem;
        top: -4px;
        right: -4px;
      }
      
      .train-direction {
        padding: 8px;
        font-size: 0.8rem;
      }
      
      .action-buttons {
        padding: 10px;
        gap: 8px;
      }
      
      .confirm-btn {
        padding: 8px 16px;
        font-size: 0.8rem;
      }
    }
    
    /* Landscape mobile optimization */
    @media (max-width: 768px) and (orientation: landscape) {
      .seat-map-container {
        padding: 5px;
      }
      
      .seat-map-header {
        flex-direction: row;
        padding: 10px;
      }
      
      .coach-container {
        padding: 10px;
      }
      
      .seat {
        width: 32px;
        height: 32px;
      }
    }
  `]
})
export class SeatMapComponent implements OnInit {
  @Input() scheduleId: number = 0;
  @Input() trainClass: string = '';
  @Input() maxSeats: number = 6;
  @Output() seatsSelected = new EventEmitter<number[]>();
  @Output() coachChanged = new EventEmitter<number>();

  seatRows: any[] = [];
  selectedSeats: number[] = [];
  currentCoach: number = 1;
  totalCoaches: number = 1;
  totalSeats: number = 0;
  coaches: any[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadCoachLayout();
  }

  async loadCoachLayout() {
    try {
      const response: any = await this.http.get(
        `http://localhost:5000/api/booking/coach-layout/${this.scheduleId}/${this.trainClass}`
      ).toPromise();
      
      this.coaches = response.coaches || [];
      this.totalCoaches = this.coaches.length;
      
      if (this.coaches.length > 0) {
        this.currentCoach = 1;
        this.generateSeatLayout();
      }
    } catch (error) {
      console.error('Error loading coach layout:', error);
    }
  }

  generateSeatLayout() {
    if (!this.coaches[this.currentCoach - 1]) return;
    
    const coach = this.coaches[this.currentCoach - 1];
    this.totalSeats = coach.totalSeats;
    this.seatRows = [];
    
    const seats = coach.seats || [];
    let seatIndex = 0;
    
    // Group seats into rows of 4
    for (let row = 1; seatIndex < seats.length; row++) {
      const leftSeats = [];
      const rightSeats = [];
      
      // Left side seats (A, B)
      if (seatIndex < seats.length) {
        leftSeats.push(this.createSeatFromData(seats[seatIndex], 'A', 'window'));
        seatIndex++;
      }
      if (seatIndex < seats.length) {
        leftSeats.push(this.createSeatFromData(seats[seatIndex], 'B', 'aisle'));
        seatIndex++;
      }
      
      // Right side seats (C, D)
      if (seatIndex < seats.length) {
        rightSeats.push(this.createSeatFromData(seats[seatIndex], 'C', 'aisle'));
        seatIndex++;
      }
      if (seatIndex < seats.length) {
        rightSeats.push(this.createSeatFromData(seats[seatIndex], 'D', 'window'));
        seatIndex++;
      }
      
      if (leftSeats.length > 0 || rightSeats.length > 0) {
        this.seatRows.push({
          rowNumber: row,
          leftSeats,
          rightSeats
        });
      }
    }
  }

  createSeatFromData(seatData: any, position: 'A' | 'B' | 'C' | 'D', type: 'window' | 'aisle' | 'middle'): Seat {
    return {
      number: seatData.seatNumber,
      position,
      type,
      row: Math.ceil(seatData.seatNumber / 4),
      isOccupied: seatData.isOccupied,
      isSelected: false
    };
  }

  getSeatType(seatNumber: number, position: 'A' | 'B' | 'C' | 'D'): 'window' | 'aisle' | 'middle' {
    // A and D are always window seats
    if (position === 'A' || position === 'D') {
      return 'window';
    }
    // B and C are aisle seats
    return 'aisle';
  }

  toggleSeat(seat: Seat) {
    if (seat.isOccupied) return;

    if (seat.isSelected) {
      seat.isSelected = false;
      this.selectedSeats = this.selectedSeats.filter(s => s !== seat.number);
    } else {
      if (this.selectedSeats.length < this.maxSeats) {
        seat.isSelected = true;
        this.selectedSeats.push(seat.number);
      }
    }
    
    // Emit seats immediately when selection changes
    this.seatsSelected.emit([...this.selectedSeats]);
  }

  getSeatTooltip(seat: Seat): string {
    if (seat.isOccupied) return `Seat ${seat.number} - Occupied`;
    if (seat.isSelected) return `Seat ${seat.number} - Selected (Click to deselect)`;
    return `Seat ${seat.number} - Available (${seat.type === 'window' ? 'Window' : 'Aisle'})`;
  }

  clearSelection() {
    this.selectedSeats = [];
    this.seatRows.forEach(row => {
      [...row.leftSeats, ...row.rightSeats].forEach(seat => seat.isSelected = false);
    });
    
    // Emit empty selection when cleared
    this.seatsSelected.emit([]);
  }

  confirmSelection() {
    this.seatsSelected.emit([...this.selectedSeats]);
  }

  previousCoach() {
    if (this.currentCoach > 1) {
      this.currentCoach--;
      this.generateSeatLayout();
      this.coachChanged.emit(this.currentCoach);
    }
  }

  nextCoach() {
    if (this.currentCoach < this.totalCoaches) {
      this.currentCoach++;
      this.generateSeatLayout();
      this.coachChanged.emit(this.currentCoach);
    }
  }
}