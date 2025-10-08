import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NotificationService, Notification } from '../../services/notification.service';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatButtonModule],
  template: `
    <div class="container mx-auto px-4 py-6">
      <div class="flex items-center justify-between mb-6">
        <h1 class="text-3xl font-bold text-gray-800">All Notifications</h1>
        <button mat-raised-button color="primary" (click)="markAllAsRead()" *ngIf="hasUnread">
          Mark All as Read
        </button>
      </div>

      <div *ngIf="notifications.length === 0" class="text-center py-12">
        <mat-icon class="text-6xl text-gray-400 mb-4">notifications_none</mat-icon>
        <h2 class="text-xl text-gray-600 mb-2">No notifications</h2>
        <p class="text-gray-500">You'll see your booking updates and other notifications here.</p>
      </div>

      <div class="space-y-4">
        <mat-card *ngFor="let notification of notifications" 
                  class="notification-card"
                  [class.unread]="notification.status === 'Unread'">
          <mat-card-content class="flex items-start space-x-4 p-4">
            <div class="notification-icon">
              <mat-icon [class]="getNotificationColor(notification.type)">
                {{ getNotificationIcon(notification.type) }}
              </mat-icon>
            </div>
            <div class="flex-1">
              <h3 class="font-semibold text-gray-900 mb-1">{{ notification.title }}</h3>
              <p class="text-gray-700 mb-2">{{ notification.message }}</p>
              <span class="text-sm text-gray-500">{{ formatTime(notification.createdAt) }}</span>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .notification-card {
      transition: transform 0.2s ease-in-out;
    }
    .notification-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    }
    .notification-card.unread {
      border-left: 4px solid #2196f3;
      background-color: #f8f9ff;
    }
    .notification-icon mat-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
    }
    .text-green-600 { color: #16a34a; }
    .text-red-600 { color: #dc2626; }
    .text-yellow-600 { color: #ca8a04; }
    .text-blue-600 { color: #2563eb; }
    .text-gray-600 { color: #4b5563; }
  `]
})
export class NotificationsComponent implements OnInit {
  notifications: Notification[] = [];
  hasUnread = false;

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadNotifications();
  }

  loadNotifications(): void {
    this.notificationService.getNotifications(50).subscribe({
      next: (notifications) => {
        this.notifications = notifications;
        this.hasUnread = notifications.some(n => n.status === 'Unread');
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
        this.notifications = [];
        this.hasUnread = false;
      }
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.loadNotifications();
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
    return date.toLocaleString();
  }
}