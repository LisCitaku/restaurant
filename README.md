# Restaurant POS Enterprise

Professional restaurant POS solution built with ASP.NET Core and a lightweight operational frontend.

## Included

- Clean layered backend with `Domain`, `Application`, `Infrastructure`, and `Api`
- JWT authentication with role-based access for admin, waiter, and cashier
- SQL Server integration with EF Core
- Order creation, payment, and receipt generation
- Simulated receipt printing to text files
- Swagger/OpenAPI documentation
- Browser frontend for login, floor overview, menu, ordering, and receipt workflow

## Default Demo Users

- `admin / admin123`
- `waiter / waiter123`
- `cashier / cashier123`

## Project Layout

- `C:\Users\Admin\Desktop\restaurant\src\Backend\RestaurantPosEnterprise.Domain`
- `C:\Users\Admin\Desktop\restaurant\src\Backend\RestaurantPosEnterprise.Application`
- `C:\Users\Admin\Desktop\restaurant\src\Backend\RestaurantPosEnterprise.Infrastructure`
- `C:\Users\Admin\Desktop\restaurant\src\Backend\RestaurantPosEnterprise.Api`
- `C:\Users\Admin\Desktop\restaurant\src\Frontend`
- `C:\Users\Admin\Desktop\restaurant\docs`

## Run

1. Restore dependencies:
   `dotnet restore C:\Users\Admin\Desktop\restaurant\RestaurantPosEnterprise.sln`
2. Confirm SQL Server on machine `FAVELLA` is running and the database `FAVELLA` exists, or update the connection string in `appsettings.json`.
3. Start the API:
   `dotnet run --project C:\Users\Admin\Desktop\restaurant\src\Backend\RestaurantPosEnterprise.Api\RestaurantPosEnterprise.Api.csproj`
4. Open Swagger at `https://localhost:7116/swagger`
5. Open the frontend at `https://localhost:7116/`

## Receipt Output

Generated receipts are written to:

- `C:\Users\Admin\Desktop\restaurant\src\Backend\RestaurantPosEnterprise.Api\PrintedReceipts` when running from the API directory
- or the process working directory if changed externally

## SQL Server Notes

- Default connection is now configured for SSMS on machine `FAVELLA` with database `FAVELLA`
- Current string: `Server=FAVELLA;Database=FAVELLA;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False`
- If you use SQL login instead of Windows login, replace it with `User Id=...;Password=...`
