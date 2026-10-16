# RaceDay API

RaceDay API is a .NET Web API project built with C# and ASP.NET Core. It uses Entity Framework Core for database access and SQL Server for data storage.

## Technologies

- C#
- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- SQL Server
- GitHub Actions for continuous integration

## Prerequisites

Before running the application, install:

- .NET 10 SDK
- Visual Studio 2022 or a compatible IDE
- SQL Server / SQL Server LocalDB
- Git

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/SinoMkosi/RaceDay.Api.git
cd RaceDay.Api
```

### 2. Restore dependencies

```bash
dotnet restore
```

If running from the solution directory, you can specify the solution file:

```bash
dotnet restore RaceDay.Api.slnx
```

### 3. Configure the database

Check `appsettings.json` and `appsettings.Development.json` for the database connection string.

Update the connection string to match your local SQL Server configuration. Do not commit passwords or other secrets to GitHub.

### 4. Apply database migrations

Once the database connection is configured, apply the Entity Framework Core migrations using the appropriate EF Core tooling.

### 5. Run the API

```bash
dotnet run
```

Alternatively, open the solution in Visual Studio, set the API project as the startup project, and press **F5**.

### 6. Test the API

Use the configured local URL to access the API. If Swagger is enabled in the development environment, open the Swagger UI to explore and test the available endpoints.

## Continuous Integration

GitHub Actions is configured to restore dependencies and build the solution when changes are pushed to the configured branches or a pull request is opened.

Workflow file: `.github/workflows/dotnet.yml`

## Project Structure

- `Controllers/` — API endpoints
- `Models/` — data models
- `Migrations/` — Entity Framework Core database migrations
- `AppDbContext.cs` — database context
- `Program.cs` — application entry point
- `Security/` — security-related code

## Author

Sinoxolo Mkosi
