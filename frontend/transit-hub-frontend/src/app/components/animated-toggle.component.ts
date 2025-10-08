import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-animated-toggle',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toggle-container">
      <div class="sliding-indicator" [style.transform]="getIndicatorPosition()"></div>
      <button 
        class="toggle-option" 
        [class.active]="value === 'train'"
        (click)="onToggle('train')">
        🚂 Train
      </button>
      <button 
        class="toggle-option" 
        [class.active]="value === 'flight'"
        (click)="onToggle('flight')">
        ✈️ Flight
      </button>
    </div>
  `,
  styles: [`
    .toggle-container {
      position: relative;
      display: flex;
      width: 240px;
      height: 44px;
      background: #f5f5f5;
      border-radius: 22px;
      padding: 3px;
      margin: 0 auto;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .sliding-indicator {
      position: absolute;
      width: calc(50% - 2px);
      height: calc(100% - 6px);
      background: #1976d2;
      border-radius: 19px;
      top: 3px;
      left: 3px;
      transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      box-shadow: 0 2px 8px rgba(25, 118, 210, 0.3);
    }

    .toggle-option {
      flex: 1;
      border: none;
      background: transparent;
      color: #666;
      font-size: 0.85rem;
      font-weight: 500;
      cursor: pointer;
      border-radius: 19px;
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
  `]
})
export class AnimatedToggleComponent {
  @Input() value: 'train' | 'flight' = 'train';
  @Output() valueChange = new EventEmitter<'train' | 'flight'>();

  onToggle(newValue: 'train' | 'flight') {
    if (this.value !== newValue) {
      this.value = newValue;
      this.valueChange.emit(newValue);
    }
  }

  getIndicatorPosition(): string {
    return this.value === 'train' ? 'translateX(0%)' : 'translateX(calc(100% + 4px))';
  }
}