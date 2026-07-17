# Conveyincing Extractor

Requirements:  MSSQL Server with a empty database preferably 2022 or later, .Net 10 SDK, Visual Studio 2026

Before running please configure a connection string in either the appsettings.json or create a user secrets file with the connection string. The connection string should be in the following format:
```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

once connection string is configured please run the following command to apply migrations and create the database schema:
```
dotnet ef database update
```
this should be run from the project directory where the .csproj file is located.

then you can run the application using the following command:
```
dotnet run
```