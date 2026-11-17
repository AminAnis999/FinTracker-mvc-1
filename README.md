# FinTracker - ASP.NET Core MVC + Identity + ADO.NET

## Architecture
- ASP.NET Core MVC for controllers and views.
- ASP.NET Core Identity for registration, login, password hashing and cookies.
- EF Core is used only for Identity's built-in tables (`AspNetUsers`, etc.).
- ADO.NET (`Microsoft.Data.SqlClient`) is used for finance transactions.
- `TransactionRepository` is instantiated directly in the controller. It is not registered in dependency injection.
- Bootstrap 5 is loaded from CDN. The project has one shared `_Layout.cshtml` navigation layout.

> Identity's `UserManager`, `SignInManager`, and EF Core context are framework services and must be registered with ASP.NET Core's service container. Removing those registrations would break Identity. No repository service registration is used.

## Requirements
- .NET 10 SDK
- SQL Server LocalDB (included with Visual Studio) or SQL Server Express/Developer
- Visual Studio 2026 or VS Code

## Run in Visual Studio
1. Extract the ZIP.
2. Open `FinTracker.csproj` (or the project folder).
3. Update `appsettings.json` connection string if your SQL Server instance is not LocalDB.
4. Open **Tools > NuGet Package Manager > Package Manager Console**.
5. Run:
   ```powershell
   Add-Migration CreateIdentityTables
   Update-Database
   ```
6. In SQL Server Management Studio, connect to `(localdb)\\MSSQLLocalDB`. Open `Database/FinanceTables.sql` and execute it against `FinTrackerDb`.
7. Press F5 or Ctrl+F5.

## Run in VS Code / PowerShell
Open a terminal in the folder containing `FinTracker.csproj`:

```powershell
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef migrations add CreateIdentityTables
dotnet ef database update
```

Then open `Database/FinanceTables.sql` in SQL Server Management Studio and execute it against `FinTrackerDb`. Finally:

```powershell
dotnet run
```

Open the HTTPS/HTTP URL printed in the terminal.

If `dotnet-ef` is already installed, skip the tool-install command. If you use SQL Server Express, change the connection string, for example:
`Server=.\\SQLEXPRESS;Database=FinTrackerDb;Trusted_Connection=True;TrustServerCertificate=True;`

## First use
1. Choose **Create account** and register with an email and password (minimum 8 characters, at least one uppercase and one digit).
2. You are signed in automatically after successful registration.
3. Add an income or expense from **Add transaction**.
4. Dashboard totals and transaction list are calculated from your saved records.
5. Sign out, then sign back in to test Identity.

## Important notes
- The repository always filters by the authenticated user's Identity ID. One user's transactions are not shown to another user.
- This starter project includes income/expense CRUD and a dashboard. It does not yet include budgets, recurring transactions, charts, export, or password reset/email confirmation.
- If you create the database manually before running EF migrations, make sure the connection string points to that same database.
- Do not deploy with a development connection string or expose database credentials.
