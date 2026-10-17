# RaceDay API

RESTful API for the RaceDay system, built with ASP.NET Core Web API (.NET 10), Entity Framework Core (code-first) and SQL Server.

## Green Build

![Green build](docs/green-build.png)

## Features

- Register and log in as an **Organiser** or a **Participant** (passwords hashed with BCrypt)
- Session-based authentication, with role checks on every protected endpoint
- Profile: view and update your own details
- Events: Organisers create, update and delete; everyone can view
- Categories: Organisers define categories per event; everyone can view
- Enrolments: Participants enrol with a category; Organisers view enrolments for their events
- Results: Organisers capture finish times and positions; Participants view their own
- Swagger UI to view and test all endpoints

## Project structure

- `RaceDay.Api/` - the API (Controllers, Models, Dtos, Security, Migrations)
- `RaceDayTests/` - unit tests (xUnit, in-memory database)
- `.github/workflows/ci.yml` - GitHub Actions: builds and runs all tests on every push

## Running locally

1. Clone the repo and open `RaceDay.Api/RaceDay.Api.slnx` in Visual Studio.
2. The connection string in `appsettings.json` uses SQL Server LocalDB. Change it if needed.
3. Create the database: Tools > NuGet Package Manager > Package Manager Console, then run `Update-Database`.
4. Press the green play button and open `/swagger` on the URL shown in your launch settings.

## Running the tests

Run `dotnet test RaceDay.Api/RaceDay.Api.slnx` from the repo root, or use Test Explorer in Visual Studio.

## Author

Sinoxolo Mkosi