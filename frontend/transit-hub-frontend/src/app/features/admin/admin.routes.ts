import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { AdminDashboardComponent } from './dashboard/admin-dashboard.component';
import { ManageTrainsComponent } from './manage-trains/manage-trains.component';
import { ManageFlightsComponent } from './manage-flights/manage-flights.component';
import { ManageUsersComponent } from './manage-users/manage-users.component';
import { ManageStationsComponent } from './manage-stations/manage-stations.component';
import { ReportsComponent } from './reports/reports.component';

export const adminRoutes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: AdminDashboardComponent },
      { path: 'trains', component: ManageTrainsComponent },
      { path: 'stations', component: ManageStationsComponent },
      { path: 'flights', component: ManageFlightsComponent },
      { path: 'users', component: ManageUsersComponent },
      { path: 'reports', component: ReportsComponent }
    ]
  }
];