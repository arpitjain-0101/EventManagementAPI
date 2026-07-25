# EventManagement API

A minimal ASP.NET Core Web API for managing events and event registrations.

## Tech stack

- `.NET 10` (`net10.0`)
- `ASP.NET Core Web API`
- `StackExchange.Redis` for persistence
- `Scalar.AspNetCore` + OpenAPI for API documentation
- `xUnit` for tests

## Features

- Create, read, update, and delete events
- Register/unregister users for events
- Capacity checks on registration
- Duplicate registration prevention
- Past-event registration prevention
- Registration payload stores and returns:
  - `userId`
  - `name`
  - `email`

## Prerequisites

Install the following before running locally:

1. **.NET 10 SDK**
2. **Redis** running locally (default: `localhost:6379`)

## Local setup

### 1) Clone and open

Open the repository in Visual Studio 2026 or any terminal.

### 2) Start Redis

You can run Redis in Docker:

```powershell
docker run --name eventmanagement-redis -p 6379:6379 -d redis:7
```

If Redis is already installed locally, ensure it is running on `localhost:6379`.

### 3) Configure connection string (optional)

Default config in `src/EventManagement.Api/appsettings.json`:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

If needed, update this value to your Redis endpoint.

### 4) Restore dependencies

From repo root (`backend`):

```powershell
dotnet restore
```

### 5) Build

```powershell
dotnet build
```

### 6) Run the API

```powershell
dotnet run --project src/EventManagement.Api/EventManagement.Api.csproj
```

Default local URL:

- `http://localhost:5073`

(From `src/EventManagement.Api/Properties/launchSettings.json`.)

## API documentation

In `Development`, OpenAPI and Scalar are enabled.

After starting the API, use:

- OpenAPI JSON: `http://localhost:5073/openapi/v1.json`
- Scalar UI: `http://localhost:5073/scalar/v1`

## CORS

A CORS policy named `frontend` is configured to allow:

- Origin: `http://localhost:5173`
- Any method and header

If your frontend uses another origin, update `Program.cs`.

## Running tests

From repo root:

```powershell
dotnet test
```

## Project structure

- `src/EventManagement.Api`
  - `Controllers` – HTTP endpoints
  - `Services` – business logic
  - `Data` – Redis repositories
  - `Contracts` – request/response DTOs
  - `Models` – domain/data models
- `tests/EventManagement.Api.Tests`
  - unit tests for registration behavior

## API endpoints

Base URL: `http://localhost:5073`

### Events

- `GET /api/events` – list events
- `GET /api/events/{id}` – get event by id
- `POST /api/events` – create event
- `PUT /api/events/{id}` – update event
- `DELETE /api/events/{id}` – delete event

`POST/PUT` body:

```json
{
  "title": "Tech Meetup",
  "description": "Monthly community meetup",
  "date": "2026-01-15T18:30:00Z",
  "maxCapacity": 100
}
```

### Registrations

- `GET /api/events/{eventId}/registrations` – list registered users (`userId`, `name`, `email`)
- `POST /api/events/{eventId}/registrations` – register a user
- `DELETE /api/events/{eventId}/registrations/{userId}` – unregister a user

`POST` body:

```json
{
  "userId": "user-123",
  "name": "Alex Doe",
  "email": "alex@example.com"
}
```

## Notes

- Deleting an event also clears registrations for that event.
- Registration count is used to enforce event capacity.
- Registration uniqueness is per `eventId + userId`.
