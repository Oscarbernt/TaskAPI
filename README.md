# TaskHub API

A RESTful API for task management built with ASP.NET Core. TaskHub provides a simple and efficient way to manage tasks with full CRUD operations.

## Features

- Create, read, update, and delete tasks
- Track due dates for tasks
- Mark tasks as completed
- SQLite database for data persistence
- Swagger/OpenAPI documentation

## Technologies

- **.NET 10.0** - Framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 10.0** - ORM
- **SQLite** - Database
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- A code editor (Visual Studio, Rider, VS Code, etc.)

## Getting Started

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd TaskHub
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Apply database migrations:
```bash
dotnet ef database update
```

### Running the Application

Run the application using one of the following methods:

**Using .NET CLI:**
```bash
dotnet run
```

**Using Visual Studio/Rider:**
- Press F5 or click the Run button

The API will be available at:
- HTTP: `http://localhost:5067`
- HTTPS: `https://localhost:7193`

### Swagger Documentation

When running in Development mode, Swagger UI is automatically available at:
- `https://localhost:7193/swagger` (HTTPS)
- `http://localhost:5067/swagger` (HTTP)

## API Endpoints

All endpoints are prefixed with `/api/tasks`

### Get All Tasks
```http
GET /api/tasks
```

**Response:** Returns a list of all tasks

### Create Task
```http
POST /api/tasks
Content-Type: application/json

{
  "title": "Complete project",
  "description": "Finish the TaskHub API project",
  "dueDate": "2024-12-31T23:59:59Z"
}
```

**Response:** Returns the created task with generated ID and timestamps

### Update Task
```http
PUT /api/tasks/{id}
Content-Type: application/json

{
  "title": "Updated title",
  "description": "Updated description",
  "dueDate": "2024-12-31T23:59:59Z",
  "isCompleted": true
}
```

**Response:** Returns the updated task

### Delete Task
```http
DELETE /api/tasks/{id}
```

**Response:** 204 No Content

## Data Models

### TaskEntity
- `Id` (int) - Primary key
- `Title` (string, required) - Task title
- `Description` (string, required) - Task description
- `DueDate` (DateTime, required) - Task due date
- `IsCompleted` (bool) - Completion status
- `CreatedAt` (DateTime) - Creation timestamp
- `UpdatedAt` (DateTime) - Last update timestamp

## Project Structure

```
TaskHub/
├── Controllers/
│   └── TaskController.cs      # API endpoints
├── Data/
│   └── ApplicationDbContext.cs # EF Core DbContext
├── Models/
│   ├── Db/
│   │   └── TaskEntity.cs      # Database entity
│   └── Dto/
│       ├── TaskCreateRequest.cs
│       ├── TaskResponse.cs
│       └── TaskUpdateRequest.cs
├── Migrations/                # EF Core migrations
├── Program.cs                 # Application entry point
├── appsettings.json          # Configuration
└── TaskHub.csproj            # Project file
```

## Database

The application uses SQLite with a database file named `TaskHub.db` in the project root. The database is automatically created when you run migrations.

### Creating Migrations

To create a new migration after making model changes:
```bash
dotnet ef migrations add <MigrationName>
```

### Applying Migrations

To apply pending migrations:
```bash
dotnet ef database update
```

## Configuration

Database connection string can be configured in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=TaskHub.db"
  }
}
```

## Development

### Environment

The application uses the `Development` environment by default when running locally. Swagger UI is only enabled in Development mode.

### Logging

Logging is configured in `appsettings.json`. Default log level is `Information` for the application and `Warning` for Microsoft.AspNetCore.