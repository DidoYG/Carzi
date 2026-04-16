# Carzi

Carzi is an ASP.NET Core MVC web app for tracking vehicles, trips, and vehicle-related costs (fuel fill-ups and mandatory expenses like vignettes, inspections, and TPL insurances).

## Tech Stack

- Language: **C#**
- Runtime/Framework: **.NET (`net10.0`)**
- Web: **ASP.NET Core MVC** + **Razor Views**
- Data: **Entity Framework Core** + **SQLite**
- UI: **Bootstrap**, **jQuery**
- Auth: **Cookie authentication** + **role-based authorization**
- Tests: **xUnit**

## Features

### Guest mode

- Trip cost calculator (fuel + optional temporary vignette cost).
- Uses the database tables for fuel types and vignette types.

### User mode

- Vehicles: create/edit/delete vehicles (fuel type, consumption, odometer, purchase info).
- Trips: calculate and save trip cost (needed fuel, fuel cost, optional vignette cost).
- Expenses:
  - Fuel logs (liters × price per liter = total)
  - Vignettes (validity period + cost)
  - Annual inspections (valid-until + cost)
  - TPL insurance (policy details + start/end + cost)
- Dashboard:
  - Expiry summaries per vehicle (expired / expires today / missing / valid)
  - Cost aggregates across vehicles
  - Notifications for expiring/expired items

### Admin mode

- Manage users and roles (`Admin`, `User`).
- Manage vehicle expenses (fuel types, vignette types, annual inspection types).
- Update fuel prices from the Fuelo API.

## Getting Started

### Prerequisites

- .NET SDK that supports `net10.0`

### Run locally

```bash
dotnet restore
dotnet run
```

### Database (SQLite + EF Core)

To create/update the database from migrations:

```bash
dotnet ef database update
```

Note: Admin-only pages require an `Admin` user. New registrations default to `User`, so for a fresh database you’ll need to promote a user to `Admin` (for example by updating the `Users.Role` field in SQLite).

## Configuration

### Fuel prices (Fuelo)

Admin can update fuel prices from Fuelo. Configure the API key in `appsettings.json`:

- `Fuelo:ApiKey`

If the key is missing/invalid, the update operation will simply skip failed requests.

## Tests

```bash
dotnet test
```

## Project Structure

- `Program.cs` — app startup, auth, routing
- `Controllers/` — MVC controllers (guest, user, admin)
- `Models/` — EF Core entities and view models
- `Data/ApplicationDbContext.cs` — EF Core context
- `Views/` — Razor views
- `wwwroot/` — static assets (Bootstrap/jQuery, CSS, JS)
