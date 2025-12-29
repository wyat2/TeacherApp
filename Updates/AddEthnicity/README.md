Update: Add Ethnicity to Student

This update adds an `Ethnicity` field to the `Student` model and makes it editable in the UI.

Files changed by this update:
- `Models/Student.cs` -> added `public string? Ethnicity { get; set; }`
- `Controllers/StudentsController.cs` -> CSV export updated to include `Ethnicity`
- `Views/Students/Create.cshtml` -> added input for `Ethnicity`
- `Views/Students/Edit.cshtml` -> added input for `Ethnicity`
- `Views/Students/Index.cshtml` -> added `Ethnicity` column
- `Views/Students/Details.cshtml` -> display `Ethnicity`

Database migration options:
1. Recommended (EF Core):
   - Run in project root:
     ```bash
     dotnet ef migrations add AddStudentEthnicity -o Data/Migrations
     dotnet ef database update
     ```
   - This will generate a migration and apply it to your SQLite DB.

2. Quick SQL (SQLite):
   - Run this SQL to alter the table directly (note: SQLite will recreate table internally):
     ```sql
     ALTER TABLE Students ADD COLUMN Ethnicity TEXT NULL;
     ```
   - Example with `sqlite3`:
     ```bash
     sqlite3 teacherapp.db "ALTER TABLE Students ADD COLUMN Ethnicity TEXT NULL;"
     ```

Choose EF migrations if you want migration history; choose SQL if you prefer a quick patch.

If you want, I can run the EF migration commands here and update the `Data/Migrations` folder automatically. If you prefer that, tell me and I'll apply the migration now.