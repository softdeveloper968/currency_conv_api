1. From solution src folder run: dotnet restore
2. To add migrations: install dotnet-ef tool and run migrations targeting Backend.Infrastructure with Backend.Api as startup
dotnet tool install --global dotnet-ef
cd src/Backend.Api
dotnet ef migrations add InitialCreate -p ../Backend.Infrastructure -s .
dotnet ef database update -p ../Backend.Infrastructure -s .
3. Set OPENEXCHANGE_API_KEY in environment variables or appsettings.Development.json
4. Run API: dotnet run --project Backend.Api
