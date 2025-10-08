# Transit Hub

A train booking system where users can search trains, book tickets, and manage their bookings. Admins can manage stations, trains, and view booking reports.

## What it does

- **Users** can register, search trains, book tickets, join waitlists, and manage their bookings
- **Admins** can add/edit stations and trains, view all bookings and user data
- **Real-time** seat availability and waitlist management
- **Email verification** for new accounts

## Tech Stack

- **Backend**: .NET 8 Web API with SQL Server
- **Frontend**: Angular 20 with Material Design
- **Database**: SQL Server with Entity Framework

## Setup

### Requirements
- .NET 8 SDK
- Node.js 18+
- SQL Server

### Installation

1. **Clone the repo**
```bash
git clone https://github.com/Nami-101/TRANSIT_HUB_FINAL.git
cd TRANSIT_HUB_FINAL
```

2. **Setup Database**
- Run `PostMigrationSetup.sql` in SQL Server Management Studio

3. **Backend**
```bash
cd backend
dotnet restore
# Update connection string in appsettings.json
dotnet run
```

4. **Frontend**
```bash
cd frontend/transit-hub-frontend
npm install
ng serve
```

Visit http://localhost:4200 to use the app.

## Features

### For Users
- Register and login with email verification
- Search trains by date and stations
- Book tickets with seat selection
- Join waitlist when trains are full
- View booking history
- Cancel bookings

### For Admins
- Manage railway stations
- Add and configure trains
- View all bookings and users
- Monitor system activity

## Project Structure

```
├── backend/           # .NET 8 API
├── frontend/          # Angular 20 app
├── docs/             # Documentation
└── PostMigrationSetup.sql  # Database setup
```

## Screenshots

The app includes:
- Clean login/register pages
- Train search with filters
- Interactive seat selection
- Booking management dashboard
- Admin panel for system management

## Contributing

Feel free to fork this repo and submit pull requests for any improvements.

## Author

Built by **Namith & Simran** as a full-stack web development project.

---

*A modern train booking system built with .NET and Angular*
