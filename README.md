# TeacherApp

Minimal ASP.NET Core MVC app with Identity for teacher/student management.

Prerequisites
- .NET 7 SDK (or update csproj to your installed SDK)
- (Optional) `dotnet-ef` tool: `dotnet tool install --global dotnet-ef` if you plan to run migrations.

How to run

1. Restore packages:

```bash
cd "/Users/awyatt/Documents/Projects/TRIO program/TeacherApp"
dotnet restore
```

2. Add migrations and update database (recommended):

```bash
# If you don't have dotnet-ef globally installed, install it first:
# dotnet tool install --global dotnet-ef

dotnet ef migrations add Initial -o Data/Migrations
dotnet ef database update
```

If you prefer not to run migrations, the app will create the SQLite DB and apply migrations on startup.

3. Run the app:

```bash
dotnet run
```

4. Open a browser at `https://localhost:5001` or `http://localhost:5000`.

Default seeded admin user
- Email: `admin@example.com`
- Password: `Password123!`

Change the seeded admin account or password in `Program.cs` for production use.

Notes
- This is a minimal scaffold to get started. You can add more fields to `Student`, more controllers, and better role management.
- The `Users` pages are protected to the `Admin` role.
