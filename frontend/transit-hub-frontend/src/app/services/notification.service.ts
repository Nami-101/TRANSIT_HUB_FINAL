import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

export interface Notification {
  notificationID: number;
  title: string;
  message: string;
  type: string;
  status: string;
  createdAt: string;
  relatedBookingID?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = 'http://localhost:5000/api/notification';
  private hubConnection: signalR.HubConnection | null = null;
  
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();
  
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {}

  startConnection(): void {
    if (this.hubConnection) {
      return;
    }

    const token = localStorage.getItem('token');
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5000/notificationHub', {
        accessTokenFactory: () => token || ''
      })
      .build();

    this.hubConnection.start()
      .then(() => {
        console.log('✅ SignalR Connected successfully');
        this.joinUserGroup();
        this.setupEventHandlers();
      })
      .catch(err => {
        console.error('❌ SignalR connection failed:', err);
      });
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
    }
  }

  private joinUserGroup(): void {
    if (this.hubConnection) {
      const userEmail = this.authService.getCurrentUserEmail();
      console.log('👥 Current user email for SignalR:', userEmail);
      if (userEmail) {
        this.hubConnection.invoke('JoinUserGroup', userEmail)
          .then(() => console.log('✅ Joined user group:', userEmail))
          .catch(err => console.error('❌ Failed to join user group:', err));
      } else {
        console.warn('⚠️ No user email found for SignalR group');
      }
    }
  }

  private setupEventHandlers(): void {
    if (this.hubConnection) {
      console.log('🔊 Setting up SignalR event handlers');
      
      this.hubConnection.on('NewNotification', (notification: any) => {
        console.log('🔔 Received new notification via SignalR:', notification);
        const current = this.notificationsSubject.value;
        console.log('📊 Current notifications count:', current.length);
        
        const newNotification: Notification = {
          notificationID: notification.id,
          title: notification.title,
          message: notification.message,
          type: notification.type,
          status: 'Unread',
          createdAt: notification.createdAt
        };
        
        this.notificationsSubject.next([newNotification, ...current]);
        console.log('📊 Updated notifications count:', this.notificationsSubject.value.length);
        
        // Update unread count immediately
        const currentCount = this.unreadCountSubject.value;
        this.unreadCountSubject.next(currentCount + 1);
        console.log('🔴 Updated unread count:', currentCount + 1);
        
        this.snackBar.open(notification.title, 'View', { duration: 5000 });
      });

      this.hubConnection.on('NotificationRead', () => {
        console.log('📜 Notification marked as read via SignalR');
        this.loadUnreadCount();
      });
      
      console.log('✅ SignalR event handlers setup complete');
    }
  }

  getNotifications(limit: number = 10): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.apiUrl}?limit=${limit}`);
  }

  getUnreadCount(): Observable<{count: number}> {
    return this.http.get<{count: number}>(`${this.apiUrl}/unread-count`);
  }

  markAllAsRead(): Observable<any> {
    return this.http.post(`${this.apiUrl}/mark-all-read`, {});
  }

  loadNotifications(): void {
    this.getNotifications(50).subscribe({
      next: (notifications) => {
        this.notificationsSubject.next(notifications);
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
        this.notificationsSubject.next([]);
      }
    });
  }

  loadUnreadCount(): void {
    this.getUnreadCount().subscribe({
      next: (response) => {
        this.unreadCountSubject.next(response.count);
      },
      error: (error) => {
        console.error('Error loading unread count:', error);
        this.unreadCountSubject.next(0);
      }
    });
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'confirmed': return 'check_circle';
      case 'cancelled': return 'cancel';
      case 'waitlist': return 'schedule';
      case 'payment': return 'payment';
      default: return 'notifications';
    }
  }

  getNotificationColor(type: string): string {
    switch (type) {
      case 'confirmed': return 'text-green-600';
      case 'cancelled': return 'text-red-600';
      case 'waitlist': return 'text-yellow-600';
      case 'payment': return 'text-blue-600';
      default: return 'text-gray-600';
    }
  }

  showSuccess(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }
}