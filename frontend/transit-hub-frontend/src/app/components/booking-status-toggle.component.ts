import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-booking-status-toggle',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toggle-container">
      <div class="sliding-indicator" [style.transform]="getIndicatorPosition()"></div>
      <button 
        class="toggle-option" 
        [class.active]="value === 'upcoming'"
        (click)="onToggle('upcoming')">
        <span class="desktop-text">📅 Upcoming</span>
        <span class="mobile-text">📅 Up</span>
      </button>
      <button 
        class="toggle-option" 
        [class.active]="value === 'completed'"
        (click)="onToggle('completed')">
        <span class="desktop-text">✅ Completed</span>
        <span class="mobile-text">✅ Done</span>
      </button>
      <button 
        class="toggle-option" 
        [class.active]="value === 'cancelled'"
        (click)="onToggle('cancelled')">
        <span class="desktop-text">❌ Cancelled</span>
        <span class="mobile-text">❌ Cancel</span>
      </button>
    </div>
  `,
  styles: [`
    .toggle-container {
      position: relative;
      display: flex;
      width: 480px;
      height: 56px;
      background: #f5f5f5;
      border-radius: 28px;
      padding: 4px;
      margin: 0 auto;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .sliding-indicator {
      position: absolute;
      width: calc(33.33% - 3px);
      height: calc(100% - 8px);
      background: #2196f3;
      border-radius: 24px;
      top: 4px;
      left: 4px;
      transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      box-shadow: 0 2px 8px rgba(33, 150, 243, 0.3);
    }

    .toggle-option {
      flex: 1;
      border: none;
      background: transparent;
      color: #666;
      font-size: 0.95rem;
      font-weight: 500;
      cursor: pointer;
      border-radius: 24px;
      transition: color 0.3s ease;
      z-index: 2;
      position: relative;
    }

    .toggle-option.active {
      color: white;
      font-weight: 600;
    }

    .toggle-option:hover:not(.active) {
      color: #333;
    }
    
    .desktop-text {
      display: inline;
    }
    
    .mobile-text {
      display: none;
    }
    
    @media (max-width: 768px) {
      .toggle-container {
        width: calc(100% - 24px);
        max-width: 480px;
        height: 52px;
        margin: 0 12px;
      }
      
      .toggle-option {
        font-size: 0.9rem;
        padding: 0 12px;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
      
      .desktop-text {
        display: none;
      }
      
      .mobile-text {
        display: inline;
      }
    }
    
    @media (max-width: 480px) {
      .toggle-container {
        width: calc(100% - 20px);
        height: 48px;
        margin: 0 10px;
      }
      
      .toggle-option {
        font-size: 0.85rem;
        padding: 0 8px;
      }
    }
  `]
})
export class BookingStatusToggleComponent {
  @Input() value: 'upcoming' | 'completed' | 'cancelled' = 'upcoming';
  @Output() valueChange = new EventEmitter<'upcoming' | 'completed' | 'cancelled'>();

  onToggle(newValue: 'upcoming' | 'completed' | 'cancelled') {
    if (this.value !== newValue) {
      this.value = newValue;
      this.valueChange.emit(newValue);
    }
  }

  getIndicatorPosition(): string {
    switch (this.value) {
      case 'upcoming': return 'translateX(0%)';
      case 'completed': return 'translateX(calc(100% + 4px))';
      case 'cancelled': return 'translateX(calc(200% + 8px))';
      default: return 'translateX(0%)';
    }
  }
}