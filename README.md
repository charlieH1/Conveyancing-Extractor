# Conveyancing Extractor

## Requirements

- MS SQL Server 2022 or later with an empty database
- .NET 10 SDK
- Visual Studio 2026

## Getting Started

### 1. Configure the connection string

Add the connection string to `appsettings.json` or a user secrets file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

### 2. Apply migrations

Run the following command from the project directory (where the `.csproj` file is located):

```bash
dotnet ef database update
```

### 3. Run the application

```bash
dotnet run
```

## Known Limitations

- **Solicitors.com** displays different results on each page load, sometimes removing a solicitor and replacing it with another. There is no unique identifier per solicitor, so matching relies on name, address, location, and source. As a result, accurate updates are not always possible and deletions are not performed, since it cannot be determined whether a solicitor has truly been removed.

## Future Work

- **Additional sources** — other sources that provide a full and complete list of solicitors could be added by following the same pattern as the `SolicitorscomScraper`.
- **Scheduled runs** — a nightly background service could be used to automate data collection.
- **Email notifications** — with more accurate data, a messaging microservice could be introduced using DAPR and a message bus to trigger email alerts.
- **CQRS pattern** — for a larger solution with multiple users and significantly more locations, a CQRS approach would be worth considering. A domain service would handle overnight data updates to an SQL instance and publish events consumed by a read service, which could cache commonly queried locations. If paired with the email notifications future work item, the messaging service would also consume these events.
- **Frontend framework** — this is a proof-of-concept built with C# and a single Razor Page. For a larger project, a dedicated frontend framework such as Angular or React would be worth evaluating.
