import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule, MatMenuTrigger } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { RouterModule } from '@angular/router';
import { NotificationService, Notification } from '../services/notification.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule,
    RouterModule
  ],
  template: `
    <button mat-icon-button [matMenuTriggerFor]="notificationMenu" #menuTrigger="matMenuTrigger" class="notification-bell">
      <mat-icon 
        [matBadge]="unreadCount" 
        [matBadgeHidden]="unreadCount === 0"
        matBadgeColor="warn"
        matBadgeSize="small"
        aria-hidden="false"
        [attr.aria-label]="'Notifications' + (unreadCount > 0 ? ', ' + unreadCount + ' unread' : '')">
        notifications
      </mat-icon>
    </button>

    <mat-menu #notificationMenu="matMenu" class="notification-menu">
      <div class="notification-header" (click)="$event.stopPropagation()">
        <h3>Notifications</h3>
        <button mat-button color="primary" (click)="markAllAsRead()" *ngIf="unreadCount > 0">
          Mark all read
        </button>
      </div>
      
      <div class="notification-list" (click)="$event.stopPropagation()">
        <div *ngIf="notifications.length === 0" class="no-notifications">
          <mat-icon>notifications_none</mat-icon>
          <p>No notifications yet</p>
        </div>
        
        <div *ngFor="let notification of notifications" 
             class="notification-item"
             [class.unread]="notification.status === 'Unread'"
             routerLink="/notifications"
             (click)="menuTrigger.closeMenu()">
          <div class="notification-icon">
            <mat-icon [class]="getNotificationColor(notification.type)">
              {{ getNotificationIcon(notification.type) }}
            </mat-icon>
          </div>
          <div class="notification-content">
            <h4>{{ notification.title }}</h4>
            <p>{{ notification.message }}</p>
            <span class="notification-time">{{ formatTime(notification.createdAt) }}</span>
          </div>
        </div>
      </div>
      
      <div class="notification-footer" (click)="$event.stopPropagation()">
        <button mat-button routerLink="/notifications" color="primary" (click)="menuTrigger.closeMenu()">
          View All Notifications
        </button>
      </div>
    </mat-menu>
  `,
  styles: [`
    .notification-bell {
      margin-right: 8px;
    }

    .notification-menu {
      width: 350px;
      max-width: 90vw;
    }

    .notification-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px;
      border-bottom: 1px solid #e0e0e0;
    }

    .notification-header h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 500;
    }

    .notification-list {
      max-height: 400px;
      overflow-y: auto;
    }

    .no-notifications {
      text-align: center;
      padding: 32px 16px;
      color: #666;
    }

    .no-notifications mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 8px;
      opacity: 0.5;
    }

    .notification-item {
      display: flex;
      padding: 12px 16px;
      border-bottom: 1px solid #f0f0f0;
      cursor: pointer;
      transition: background-color 0.2s;
    }

    .notification-item:hover {
      background-color: #f5f5f5;
    }

    .notification-item.unread {
      background-color: #e3f2fd;
    }

    .notification-icon {
      margin-right: 12px;
      flex-shrink: 0;
    }

    .notification-content {
      flex: 1;
      min-width: 0;
    }

    .notification-content h4 {
      margin: 0 0 4px 0;
      font-size: 14px;
      font-weight: 500;
      color: #333;
    }

    .notification-content p {
      margin: 0 0 4px 0;
      font-size: 13px;
      color: #666;
      line-height: 1.4;
      overflow: hidden;
      text-overflow: ellipsis;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
    }

    .notification-time {
      font-size: 11px;
      color: #999;
    }

    .notification-footer {
      padding: 8px 16px;
      border-top: 1px solid #e0e0e0;
      text-align: center;
    }

    .text-green-600 { color: #16a34a; }
    .text-red-600 { color: #dc2626; }
    .text-yellow-600 { color: #ca8a04; }
    .text-blue-600 { color: #2563eb; }
    .text-gray-600 { color: #4b5563; }
  `]
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  @ViewChild('menuTrigger') menuTrigger!: MatMenuTrigger;
  notifications: Notification[] = [];
  unreadCount = 0;
  private destroy$ = new Subject<void>();

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications.slice(0, 5); // Show only 5 in dropdown
      });

    this.notificationService.unreadCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => {
        this.unreadCount = count;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notificationService.loadNotifications();
        this.notificationService.loadUnreadCount();
      },
      error: (error) => {
        console.error('Error marking notifications as read:', error);
      }
    });
  }

  getNotificationIcon(type: string): string {
    return this.notificationService.getNotificationIcon(type);
  }

  getNotificationColor(type: string): string {
    return this.notificationService.getNotificationColor(type);
  }

  formatTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  }
}