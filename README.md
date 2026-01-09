# TeacherApp

ASP.NET Core MVC application with Identity for comprehensive TRIO program student and user management.

Prerequisites
- .NET 10 SDK
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

Initial Setup
- On first run, complete the Setup Wizard to create your admin account
- The setup wizard creates the database and initial configuration
- All passwords must meet security requirements (uppercase, lowercase, number, special character)

Key Features
- **Role-Based Access Control**: Admin, Director, Tutor, and Student roles with granular permissions
- **Audit Logging**: Comprehensive tracking of all user actions (create, edit, delete operations)
- **Student Management**: Track students with grade, class, ethnicity, and contact information
- **User Management**: Separate interfaces for Admins, Directors, and Tutors
- **Directors Dashboard**: School info, director details, and grant tracking (PR/Award Number)
- **Search & Filtering**: Find students by name/email and filter by grade or class
- **Data Export**: Export filtered student data to CSV
- **Security Compliance**: FERPA, password policies, session management, and audit trails
- **Data Update Scripts**: Built-in scripts to update dropdown values and standardize names

Security & Compliance
See [SECURITY_COMPLIANCE.md](SECURITY_COMPLIANCE.md) for detailed information on:
- FERPA compliance guidelines
- Password policies and user authentication
- Role-based access control
- Audit logging and data privacy
- Session management and security best practices

Data Maintenance
- **UpdateDropdownValues.cs**: Migrate old values to new standardized dropdown options
- **UpdateNamesToUppercase.cs**: Convert all student/user names to uppercase for consistency
