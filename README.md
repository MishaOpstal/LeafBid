# LeafBid

## Prerequisites
- [Git](https://git-scm.com/downloads)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Visual Studio](https://visualstudio.microsoft.com) or [JetBrains Rider](https://www.jetbrains.com/rider/)

## Getting Started

### Clone the Repository
```bash
git clone https://github.com/MishaOpstal/LeafBid.git
cd LeafBid
```

---

### API INSTRUCTIONS

#### 1. Go into the API Project (Assuming we're currently in the repository directory)
```bash
cd LeafBidAPI
```

#### 2. Open the Project
Open the solution in Visual Studio or JetBrains Rider and wait for dependencies to restore.

#### 3. Start Docker Services
Run the following command to start the required services (SQL Server, etc.):
```bash
docker compose up --build -d
```

**Note:** If Docker requests storage permissions, accept them. If the initial run fails after granting permissions, simply run the command again.

#### 3.5. Install dotnet-ef
Run the following command to install dotnet-ef for use in the next steps:
```bash
dotnet tool install --global dotnet-ef --version 8.0.22
```

#### 4. Apply Database Migrations
Run the Entity Framework migrations to set up the database schema:
```bash
dotnet ef database update
```

#### 5. Restart Docker Services
Restart the services to apply all changes:
```bash
docker compose restart
```

#### 6. Access the Application
The API should now be running at: [http://localhost:5001/swagger](http://localhost:5001/swagger/index.html)

## Development Configuration

### Environment Settings
During development, ensure `ASPNETCORE_ENVIRONMENT` is set to `Development`. This is configured in `docker-compose.yml`:
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
```

### Connection Strings
- **Development:** Uses connection string from `appsettings.Development.json`
- **Production:** Connection strings are injected via Docker environment variables

> ⚠️ **TODO:** Remove the `DefaultConnection` from `appsettings.json` before production deployment. Production connection strings should only be passed through Docker environment variables.

## Docker Services
The `docker-compose.yml` file includes:
- SQL Server (exposed on port 1430)
- LeafBid API (exposed on port 5001)

Services communicate internally via Docker networking. The API references SQL Server by service name, not `localhost`.

## Troubleshooting

### Port Already in Use
If ports 5001 or 1430 are already in use, modify the port mappings in `docker-compose.yml`.

### Database Connection Issues
Ensure Docker services are running and the connection string in `appsettings.Development.json` uses the SQL Server service name from docker-compose (e.g., `Server=sqlserver,1433`).

### Migrations Not Applied
If you see database errors, verify migrations were applied successfully:
```bash
dotnet ef migrations list
dotnet ef database update
```

### Changes I make don't reflect in the API container
This is normal as we do not have live-reloading yet.
In order to apply changes made to the codebase, (re)run `docker compose up --build -d`

## Useful Commands
```bash
# Create a new migration when you've changed a model
 - dotnet ef migrations add <MigrationName>
 - dotnet ef database update
```

## Contributing
1. Create a feature branch from `main`
2. Make your changes
3. Test locally using the development environment
4. Submit a pull request

## Licenses
[LeafBid is MIT Licensed](https://github.com/MishaOpstal/LeafBid/blob/main/LICENSE)

We use the [dotnet-ef-seeder](https://github.com/djoufson/dotnet-ef-seeder/) from [djoufson](https://github.com/djoufson/) for seeding the database.
- See it's License here:
[dotnet-ef-seeder MIT](https://github.com/djoufson/dotnet-ef-seeder/blob/main/LICENSE.txt)
