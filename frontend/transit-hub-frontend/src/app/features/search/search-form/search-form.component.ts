import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Observable, forkJoin } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';

import { TrainService } from '../../../services/train.service';
import { FlightService } from '../../../services/flight.service';
import { LookupService } from '../../../services/lookup.service';

import { StationDto } from '../../../models/station.dto';
import { AirportDto } from '../../../models/airport.dto';
import { LookupDataDto, TrainClassDto, TrainQuotaTypeDto, FlightClassDto } from '../../../models/lookup-data.dto';
import { TrainSearchDto, TrainSearchResultDto } from '../../../models/train-search.dto';
import { FlightSearchDto, FlightSearchResultDto } from '../../../models/flight-search.dto';
import { AnimatedToggleComponent } from '../../../components/animated-toggle.component';

// Common interface for location items
interface LocationItem {
  id: number;
  name: string;
  code: string;
  city: string;
  state: string;
  type: 'station' | 'airport';
}

@Component({
  selector: 'app-search-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatIconModule,
    AnimatedToggleComponent
  ],
  templateUrl: './search-form.component.html',
  styleUrls: ['./search-form.component.css']
})
export class SearchFormComponent implements OnInit {
  @Output() searchResults = new EventEmitter<TrainSearchResultDto[] | FlightSearchResultDto[]>();
  @Output() searchModeChange = new EventEmitter<'train' | 'flight'>();
  @Output() searchStart = new EventEmitter<void>();

  searchForm: FormGroup;
  searchMode: 'train' | 'flight' = 'train';
  
  stations: StationDto[] = [];
  airports: AirportDto[] = [];
  trainClasses: TrainClassDto[] = [];
  trainQuotaTypes: TrainQuotaTypeDto[] = [];
  flightClasses: FlightClassDto[] = [];
  
  isLoading = false;
  minDate = new Date();

  constructor(
    private fb: FormBuilder,
    private trainService: TrainService,
    private flightService: FlightService,
    private lookupService: LookupService
  ) {
    this.searchForm = this.createForm();
  }

  ngOnInit() {
    this.loadData();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      sourceID: ['', Validators.required],
      destinationID: ['', Validators.required],
      travelDate: ['', Validators.required],
      trainClassID: [''],
      flightClassID: ['']
    });
  }

  private loadData() {
    this.isLoading = true;
    console.log('Starting to load data from backend...');
    
    // Load stations from train service
    this.trainService.getStations().subscribe({
      next: (stations) => {
        console.log('Stations loaded:', stations?.length || 0, 'items');
        this.stations = stations || [];
        this.loadTrainData();
      },
      error: (error) => {
        console.error('Error loading stations:', error);
        this.isLoading = false;
      }
    });
  }

  private loadTrainData() {
    // Load train classes and quotas
    forkJoin({
      classes: this.trainService.getTrainClasses(),
      quotas: this.trainService.getQuotaTypes(),
      airports: this.lookupService.fetchAirports()
    }).subscribe({
      next: (data) => {
        console.log('Raw train classes data:', data.classes);
        console.log('Raw quotas data:', data.quotas);
        this.trainClasses = data.classes || [];
        this.trainQuotaTypes = data.quotas || [];
        this.airports = data.airports || [];
        this.isLoading = false;
        console.log('Train classes loaded:', this.trainClasses.length);
        console.log('Quotas loaded:', this.trainQuotaTypes.length);
        console.log('All data loaded successfully');
      },
      error: (error) => {
        console.error('Error loading train data:', error);
        this.isLoading = false;
      }
    });
  }



  onSearchModeToggle(mode: 'train' | 'flight' | any) {
    const newMode = typeof mode === 'string' ? mode : mode.value;
    if (this.searchMode !== newMode) {
      this.searchMode = newMode;
      this.searchModeChange.emit(newMode);
      this.searchForm.patchValue({
        sourceID: '',
        destinationID: '',
        trainClassID: '',
        flightClassID: ''
      });
    }
  }

  onSubmit() {
    if (this.searchForm.valid) {
      this.searchStart.emit();
      
      const formValue = this.searchForm.value;
      // Format date as YYYY-MM-DD string only (no time component)
      const travelDate = new Date(formValue.travelDate);
      const formattedDate = travelDate.getFullYear() + '-' + 
        String(travelDate.getMonth() + 1).padStart(2, '0') + '-' + 
        String(travelDate.getDate()).padStart(2, '0');
      
      console.log('Original date:', formValue.travelDate);
      console.log('Formatted date:', formattedDate);
      
      if (this.searchMode === 'train') {
        this.searchTrains(formValue, formattedDate);
      } else {
        this.searchFlights(formValue, formattedDate);
      }
    } else {
      this.markFormGroupTouched();
    }
  }

  private searchTrains(formValue: any, travelDate: string) {
    // Get station names from IDs
    const sourceStation = this.stations.find(s => s.stationID === formValue.sourceID);
    const destStation = this.stations.find(s => s.stationID === formValue.destinationID);
    
    const searchDto: TrainSearchDto = {
      sourceStation: sourceStation?.stationName || '',
      destinationStation: destStation?.stationName || '',
      travelDate: new Date(travelDate),
      passengerCount: 1,
      trainClass: formValue.trainClassID || undefined,
      quota: undefined
    };

    console.log('Sending train search request:', searchDto);

    this.trainService.searchTrains(searchDto).subscribe({
      next: (results) => {
        console.log('Train search results:', results);
        this.searchResults.emit(results);
      },
      error: (error) => {
        console.error('Error searching trains:', error);
        this.searchResults.emit([]);
      }
    });
  }

  private searchFlights(formValue: any, travelDate: string) {
    const searchDto: FlightSearchDto = {
      sourceAirportID: formValue.sourceID,
      destinationAirportID: formValue.destinationID,
      travelDate: travelDate,
      passengerCount: 1,
      flightClassID: formValue.flightClassID || undefined
    };

    this.flightService.searchFlights(searchDto).subscribe({
      next: (results) => {
        this.searchResults.emit(results);
      },
      error: (error) => {
        console.error('Error searching flights:', error);
        this.searchResults.emit([]);
      }
    });
  }

  private markFormGroupTouched() {
    Object.keys(this.searchForm.controls).forEach(key => {
      const control = this.searchForm.get(key);
      control?.markAsTouched();
    });
  }

  private _sourceList: LocationItem[] = [];
  private _destinationList: LocationItem[] = [];
  private lastSearchMode: 'train' | 'flight' = 'train';

  get sourceList(): LocationItem[] {
    if (this.searchMode !== this.lastSearchMode || this._sourceList.length === 0) {
      this.updateLocationLists();
    }
    return this._sourceList;
  }

  get destinationList(): LocationItem[] {
    if (this.searchMode !== this.lastSearchMode || this._destinationList.length === 0) {
      this.updateLocationLists();
    }
    return this._destinationList;
  }

  private updateLocationLists() {
    this.lastSearchMode = this.searchMode;
    
    if (this.searchMode === 'train') {
      this._sourceList = this.stations.map(station => ({
        id: station.stationID,
        name: station.stationName,
        code: station.stationCode,
        city: station.city,
        state: station.state,
        type: 'station' as const
      }));
      this._destinationList = [...this._sourceList];
    } else {
      this._sourceList = this.airports.map(airport => ({
        id: airport.airportID,
        name: airport.airportName,
        code: airport.code,
        city: airport.city,
        state: airport.state,
        type: 'airport' as const
      }));
      this._destinationList = [...this._sourceList];
    }
  }

  getDisplayName(item: LocationItem): string {
    return `${item.name} (${item.code}) - ${item.city}`;
  }

  getItemId(item: LocationItem): number {
    return item.id;
  }

  reverseStations(): void {
    const sourceValue = this.searchForm.get('sourceID')?.value;
    const destinationValue = this.searchForm.get('destinationID')?.value;
    
    this.searchForm.patchValue({
      sourceID: destinationValue,
      destinationID: sourceValue
    });
  }
}