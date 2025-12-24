# TaskHub API

A RESTful API for task management built with ASP.NET Core. TaskHub provides a simple and efficient way to manage tasks with full CRUD operations.

## Features

- Create, read, update, and delete tasks
- Track due dates for tasks
- Mark tasks as completed
- SQLite database for data persistence
- Swagger/OpenAPI documentation
- Serilog file logging with a custom request logging middleware

## Technologies

- .NET 10
- ASP.NET Core
- Entity Framework Core
- SQLite
- Serilog
- Swashbuckle (Swagger)

## Project layout

Source lives under the `src` directory:

```
TaskHub/
├── src/
│   ├── Controllers/
│   │   └── TaskController.cs
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   ├── Infrastructure/
│   │   └── Logging/
│   │       ├── RequestLoggingMiddleware.cs
│   │       └── RequestLog.cs
│   ├── Models/
│   │   ├── Db/
│   │   │   └── TaskEntity.cs
│   │   └── Dto/
│   ├── Program.cs
│   └── appsettings.json
├── tests/
└── README.md
```

## Prerequisites

- .NET 10 SDK
- dotnet-ef tool (for migrations) — install with:

```
dotnet tool install --global dotnet-ef
```

## Getting started

All commands below assume you are in the repository root unless noted; many CLI commands should be run from the `src` folder.

1. Enter the `src` folder:

```
cd src
```

2. Restore dependencies:

```
dotnet restore
```

3. Apply database migrations and create the SQLite database:

```
dotnet ef database update
```

4. Run the application:

```
dotnet run
```

## Running tests

From the repository root:

```
dotnet test
```

## Configuration

The primary configuration file is `src/appsettings.json`. Example connection string:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=TaskHub.db"
  }
}
```

The app uses SQLite; the database file `TaskHub.db` will be created by EF Core when migrations are applied.

## Logging

Serilog is configured and writes rotating log files to the `logs` folder by default (`logs/log-.txt`). The project includes a `RequestLoggingMiddleware` in `src/Infrastructure/Logging` that emits structured request/response logs.

If you want to suppress specific framework logs such as the "Executing endpoint '...'" messages, adjust logging overrides in `appsettings.json` or Serilog configuration. Example Serilog override (in code) used by the project:

- `MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)`

Or set the category in `appsettings.json` under `Logging:LogLevel` for the specific category such as `Microsoft.AspNetCore.Routing` or `Microsoft.AspNetCore.Routing.EndpointMiddleware`.

## Swagger

When running in the Development environment, Swagger UI is enabled at `/swagger`.

## API Endpoints

Base route: `/api/tasks`

- GET `/api/tasks` — list tasks
- GET `/api/tasks/{id}` — get a task
- POST `/api/tasks` — create a task
- PUT `/api/tasks/{id}` — update a task
- DELETE `/api/tasks/{id}` — delete a task

## Notes

- Development commands that act on the project (migrations, run) are easiest when executed from `src`.
- The repository includes a `tests` project; run tests with `dotnet test`.