import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { TrainSearchResultDto, TrainClassAvailabilityDto } from '../../../models/train-search.dto';
import { FlightSearchResultDto } from '../../../models/flight-search.dto';
import { BookingDialogComponent } from '../../../components/booking-dialog.component';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './search-results.component.html',
  styleUrls: ['./search-results.component.css']
})
export class SearchResultsComponent implements OnInit {
  @Input() results: (TrainSearchResultDto | FlightSearchResultDto)[] = [];
  @Input() searchMode: 'train' | 'flight' = 'train';

  constructor(private dialog: MatDialog, private authService: AuthService) {}

  ngOnInit() {
  }

  onBook(result: TrainSearchResultDto | FlightSearchResultDto) {
    if (this.searchMode === 'train' && this.isTrainResult(result)) {
      const dialogRef = this.dialog.open(BookingDialogComponent, {
        width: '900px',
        maxWidth: '95vw',
        maxHeight: '90vh',
        panelClass: 'mobile-dialog',
        data: {
          trainID: result.trainID,
          scheduleID: result.scheduleID,
          trainNumber: result.trainNumber,
          trainName: result.trainName,
          sourceStation: result.sourceStation,
          destinationStation: result.destinationStation,
          travelDate: result.travelDate,
          fare: result.fare,
          trainClass: result.trainClass,
          totalSeats: result.totalSeats
        }
      });

      dialogRef.afterClosed().subscribe(bookingResult => {
        if (bookingResult?.success) {
          // Booking successful
        }
      });
    } else if (this.searchMode === 'flight' && this.isFlightResult(result)) {
      // Flight booking not implemented yet
    }
  }

  formatTime(dateTime: string): string {
    return new Date(dateTime).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    });
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      day: '2-digit',
      month: 'short'
    });
  }

  formatDuration(minutes: number): string {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours}h ${mins}m`;
  }

  getAvailabilityClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'available':
        return 'text-green-600 bg-green-100';
      case 'limited':
        return 'text-yellow-600 bg-yellow-100';
      case 'waitlist':
        return 'text-red-600 bg-red-100';
      case 'sold out':
        return 'text-gray-600 bg-gray-100';
      case 'departed':
        return 'text-red-600 bg-red-100';
      default:
        return 'text-gray-600 bg-gray-100';
    }
  }

  isTrainDeparted(result: TrainSearchResultDto): boolean {
    const now = new Date();
    const travelDate = new Date(result.travelDate);
    const departureTime = new Date(result.departureTime);
    
    // Combine travel date with departure time
    const departureDateTime = new Date(travelDate);
    departureDateTime.setHours(departureTime.getHours(), departureTime.getMinutes(), 0, 0);
    
    return departureDateTime < now;
  }

  getBookingButtonText(result: TrainSearchResultDto): string {
    if (this.isTrainDeparted(result)) {
      return 'Already Departed';
    }
    if (result.availabilityStatus === 'Sold Out') {
      return 'Sold Out';
    }
    return 'Book Now';
  }

  isBookingDisabled(result: TrainSearchResultDto): boolean {
    return this.isTrainDeparted(result) || result.availabilityStatus === 'Sold Out';
  }

  isTrainResult(result: any): result is TrainSearchResultDto {
    return 'trainNumber' in result;
  }

  isFlightResult(result: any): result is FlightSearchResultDto {
    return 'flightNumber' in result;
  }

  isAdmin(): boolean {
    return this.authService.hasRole('Admin');
  }
}