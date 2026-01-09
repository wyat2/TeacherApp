# TeacherApp Database Schema

**Database Type:** SQLite  
**Database File:** `teacherapp.db`  
**Version:** 1.1.0  
**Last Updated:** January 8, 2026

---

## Table of Contents

1. [Authentication & Identity Tables](#authentication--identity-tables)
2. [Application-Specific Tables](#application-specific-tables)
3. [Support Tables](#support-tables)
4. [Relationships](#relationships)
5. [Indices](#indices)

---

## Authentication & Identity Tables

### AspNetUsers

User accounts and authentication data. Extended with custom fields for TRIO application.

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | TEXT | PRIMARY KEY | Unique user identifier |
| UserName | TEXT | UNIQUE INDEX | Login username (email) |
| NormalizedUserName | TEXT | UNIQUE INDEX | Case-insensitive lookup |
| Email | TEXT | | User email address |
| NormalizedEmail | TEXT | INDEX | Case-insensitive lookup |
| EmailConfirmed | INTEGER | NOT NULL | 0=false, 1=true |
| PasswordHash | TEXT | | Hashed password |
| SecurityStamp | TEXT | | For password changes |
| ConcurrencyStamp | TEXT | | For concurrent updates |
| PhoneNumber | TEXT | | User phone number |
| PhoneNumberConfirmed | INTEGER | NOT NULL | 0=false, 1=true |
| TwoFactorEnabled | INTEGER | NOT NULL | 0=false, 1=true |
| LockoutEnd | TEXT | | Lockout expiration timestamp |
| LockoutEnabled | INTEGER | NOT NULL | 0=false, 1=true |
| AccessFailedCount | INTEGER | NOT NULL | Failed login attempts |
| **FirstName** | TEXT | | **Custom field** - User first name |
| **LastName** | TEXT | | **Custom field** - User last name |
| **Primary** | INTEGER | NOT NULL DEFAULT 0 | **Custom field** - Director primary flag (0=false, 1=true) |

**Purpose:** Stores all user accounts for Admin, Director, Tutor, and Student roles.

---

### AspNetRoles

Role definitions for role-based access control.

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | TEXT | PRIMARY KEY | Unique role identifier |
| Name | TEXT | UNIQUE INDEX | Role name: Admin, Director, Tutor, Student |
| NormalizedName | TEXT | | Case-insensitive lookup |
| ConcurrencyStamp | TEXT | | For concurrent updates |

**Roles:**
- `Admin` - Full system access
- `Director` - School/Grant management, user management
- `Tutor` - Student and tutor management (limited)
- `Student` - (Reserved for future use)

---

### AspNetUserRoles

Many-to-many junction table linking users to roles.

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| UserId | TEXT | PRIMARY KEY (composite), FK | References AspNetUsers.Id |
| RoleId | TEXT | PRIMARY KEY (composite), FK | References AspNetRoles.Id |

**Purpose:** Associates each user with one or more roles.

**Example:**
- director@gmail.com → Director role
- admin@example.com → Admin role
- tutor1@example.com → Tutor role

---

### AspNetRoleClaims

Role-based claims for advanced authorization (currently unused).

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | INTEGER | PRIMARY KEY AUTO_INCREMENT | Claim identifier |
| RoleId | TEXT | FK | References AspNetRoles.Id |
| ClaimType | TEXT | | Type of claim |
| ClaimValue | TEXT | | Claim value |

---

### AspNetUserClaims

User-specific claims for advanced authorization (currently unused).

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | INTEGER | PRIMARY KEY AUTO_INCREMENT | Claim identifier |
| UserId | TEXT | FK | References AspNetUsers.Id |
| ClaimType | TEXT | | Type of claim |
| ClaimValue | TEXT | | Claim value |

---

### AspNetUserLogins

External login providers (e.g., Google, Facebook) - currently unused.

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| LoginProvider | TEXT | PRIMARY KEY (composite) | Provider name |
| ProviderKey | TEXT | PRIMARY KEY (composite) | Provider user ID |
| ProviderDisplayName | TEXT | | Display name from provider |
| UserId | TEXT | FK | References AspNetUsers.Id |

---

### AspNetUserTokens

Two-factor authentication and password reset tokens (currently unused).

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| UserId | TEXT | PRIMARY KEY (composite), FK | References AspNetUsers.Id |
| LoginProvider | TEXT | PRIMARY KEY (composite) | Token provider |
| Name | TEXT | PRIMARY KEY (composite) | Token name |
| Value | TEXT | | Token value |

---

## Application-Specific Tables

### Students

Student records with grade, class, ethnicity, and contact information.

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | INTEGER | PRIMARY KEY AUTO_INCREMENT | Unique student identifier |
| FirstName | TEXT | NOT NULL | Student first name |
| LastName | TEXT | NOT NULL | Student last name |
| ClassName | TEXT | NOT NULL | Class name (e.g., "3A", "5B") |
| Grade | TEXT | NOT NULL | Grade level (e.g., "3rd", "5th") |
| Ethnicity | TEXT | NULLABLE | Student ethnicity |
| Email | TEXT | NULLABLE | Student email (added in migration) |
| CreatedByUserId | TEXT | FK NULLABLE | References AspNetUsers.Id (tutor who created) |

**Indices:**
- `IX_Students_CreatedByUserId` - For filtering by creator

**Purpose:** Core student data for TRIO program tracking.

---

### RolePermissions

Granular permission control for each role (delete and edit permissions).

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | INTEGER | PRIMARY KEY AUTO_INCREMENT | Permission record identifier |
| RoleName | TEXT | NOT NULL | Role name (Admin, Director, Tutor) |
| CanDeleteStudents | INTEGER | NOT NULL DEFAULT 0 | Can delete students (0/1) |
| CanDeleteAdmins | INTEGER | NOT NULL DEFAULT 0 | Can delete admin users (0/1) |
| CanDeleteDirectors | INTEGER | NOT NULL DEFAULT 0 | Can delete director users (0/1) |
| CanDeleteTutors | INTEGER | NOT NULL DEFAULT 0 | Can delete tutor users (0/1) |
| CanEditAdmins | INTEGER | NOT NULL DEFAULT 0 | Can edit admin users (0/1) |
| CanEditDirectors | INTEGER | NOT NULL DEFAULT 0 | Can edit director users (0/1) |
| CanEditTutors | INTEGER | NOT NULL DEFAULT 0 | Can edit tutor users (0/1) |
| CanManageAdmins | INTEGER | NOT NULL DEFAULT 0 | Can manage admins (0/1) |

**Seeded Values (typical):**
- `Admin`: All permissions enabled (1)
- `Director`: Can edit/delete Directors and Tutors; can manage Admins
- `Tutor`: No delete/edit permissions

**Purpose:** Defines what actions each role can perform on different user types.

---

### DirectorsPageInfos

Centralized storage for school, director, and grant information shared across all directors.

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | INTEGER | PRIMARY KEY AUTO_INCREMENT | Record identifier (typically only 1 row) |
| SchoolName | TEXT | NULLABLE | School name |
| Address | TEXT | NULLABLE | School address |
| SchoolPhone | TEXT | NULLABLE | School telephone number |
| DirectorName | TEXT | NULLABLE | Director name |
| DirectorEmail | TEXT | NULLABLE | Director email |
| DirectorPhone | TEXT | NULLABLE | Director phone number |
| GrantType | TEXT | NULLABLE | Type of grant |
| PRAwardNumber | TEXT | NULLABLE | PR/Award Number (renamed from GrantId) |
| GrantBudget | TEXT | NULLABLE | Grant budget amount (decimal stored as TEXT) |

**Purpose:** Single record containing school, director, and grant information for the Directors page tabs.

**Design Note:** This table typically contains only 1 row, shared by all directors. Individual director users are stored in AspNetUsers with Director role.

---

### AuditLogs

Comprehensive audit trail of all user actions in the system.

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| Id | INTEGER | PRIMARY KEY AUTO_INCREMENT | Unique audit log identifier |
| Timestamp | TEXT | NOT NULL | ISO 8601 timestamp of the action |
| UserId | TEXT | NOT NULL | References AspNetUsers.Id (user who performed action) |
| UserName | TEXT | NOT NULL | Username for quick reference |
| Action | TEXT | NOT NULL | Action performed (Create, Edit, Delete) |
| EntityType | TEXT | NOT NULL | Type of entity (Student, Admin, Director, Tutor, etc.) |
| EntityId | TEXT | NOT NULL | ID of the affected entity |
| Details | TEXT | NULLABLE | Additional details about the action |

**Indices:**
- `IX_AuditLogs_UserId` - For filtering by user
- `IX_AuditLogs_Timestamp` - For chronological queries
- `IX_AuditLogs_EntityType` - For filtering by entity type

**Purpose:** Maintains complete audit trail for compliance (FERPA) and security monitoring. Accessible via /Audit page (Admin only).

---

## Support Tables

### __EFMigrationsHistory

Entity Framework Core migration tracking table. Automatically managed.

| Column | Type | Constraints | Notes |
|--------|------|-------------|-------|
| MigrationId | TEXT | PRIMARY KEY | Migration identifier (timestamp + name) |
| ProductVersion | TEXT | NOT NULL | EF Core version that applied migration |

**Purpose:** Tracks which migrations have been applied to maintain schema version.

**Migrations Applied:**
- 20251211035339_Initial
- 20251211040527_AddStudentCreatedByUserForeignKey
- 20251218040136_AddStudentEthnicity
- 20251218042131_AddUserNames
- 20251218043313_AddRolePermissions
- 20251220022040_AddDirectorsPageInfo
- 20251220023825_AddDirectorPrimaryToUsers
- 20251220215149_RenameGrantIdToPRAwardNumber
- 20260104043201_AddAuditLogging
- 20260106074622_RemoveClassNameFromStudent

---

## Relationships

```
┌─────────────────────────────────────────────────────────┐
│                    AspNetUsers                          │
│ (PK: Id, FK: CreatedByUserId → Students)               │
└────────┬───────────┬──────────────────┬──────────────────┘
         │           │                  │
         │   ┌───────▼────────┐  ┌──────▼──────────────┐
         │   │ AspNetUserRoles│  │     Students        │
         │   │(FK: UserId,    │  │ (CreatedByUserId FK)│
         │   │    RoleId)     │  └─────────────────────┘
         │   │                │
         │   │ AspNetRoles    │
         │   │ (PK: Id)       │
         │   └────────────────┘
         │
         └───────────────────────────────────────┐
                                                 │
                                          ┌──────▼──────┐
                                          │  AuditLogs  │
                                          │ (UserId FK) │
                                          └─────────────┘

DirectorsPageInfos (separate, single shared record for all directors)
RolePermissions (role-based permissions, separate table)
```

### Key Relationships

1. **User → Role (Many-to-Many)**
   - AspNetUsers ← AspNetUserRoles → AspNetRoles
   - One user can have multiple roles (though typically one)

2. **Student → Creator (One-to-Many)**
   - AspNetUsers (Tutor) ← Students (CreatedByUserId)
   - One tutor can create multiple students

3. **User → Audit Logs (One-to-Many)**
   - AspNetUsers ← AuditLogs (UserId)
   - One user can have many audit log entries

4. **Roles → Permissions (One-to-Many)**
   - AspNetRoles ← RolePermissions (by RoleName)
   - Each role has one permission record

5. **Directors → School Info (One-to-One logical)**
   - AspNetUsers (Director role) → DirectorsPageInfos
   - All directors reference same DirectorsPageInfos record

---

## Indices

**Performance Optimization Indices:**

| Index Name | Table | Column(s) | Purpose |
|------------|-------|-----------|---------|
| RoleNameIndex | AspNetRoles | NormalizedName | Fast role lookup |
| UserNameIndex | AspNetUsers | NormalizedUserName | Fast user login lookup |
| EmailIndex | AspNetUsers | NormalizedEmail | Fast email-based lookup |
| IX_AspNetRoleClaims_RoleId | AspNetRoleClaims | RoleId | Role claim lookups |
| IX_AspNetUserClaims_UserId | AspNetUserClaims | UserId | User claim lookups |
| IX_AspNetUserLogins_UserId | AspNetUserLogins | UserId | External login lookups |
| IX_AspNetUserRoles_RoleId | AspNetUserRoles | RoleId | Role membership lookups |
| IX_Students_CreatedByUserId | Students | CreatedByUserId | Filter students by creator |
| IX_AuditLogs_UserId | AuditLogs | UserId | Filter audit logs by user |
| IX_AuditLogs_Timestamp | AuditLogs | Timestamp | Chronological audit queries |
| IX_AuditLogs_EntityType | AuditLogs | EntityType | Filter by entity type |

---

## Sample Queries

### List all Directors
```sql
SELECT u.Id, u.Email, u.FirstName, u.LastName, u.PhoneNumber, u.Primary
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Director'
ORDER BY u.LastName;
```

### List all Students in Grade 5
```sql
SELECT s.Id, s.FirstName, s.LastName, s.ClassName, s.Grade, s.Email
FROM Students s
WHERE s.Grade = '5th'
ORDER BY s.LastName, s.FirstName;
```

### Get User Role Permissions
```sql
SELECT rp.RoleName, rp.CanDeleteStudents, rp.CanDeleteAdmins, 
       rp.CanEditDirectors, rp.CanEditTutors
FROM RolePermissions rp
WHERE rp.RoleName = 'Admin';
```

### School Information
```sql
SELECT SchoolName, Address, SchoolPhone, GrantType, PRAwardNumber, GrantBudget
FROM DirectorsPageInfos
LIMIT 1;
```

### Recent Audit Logs
```sql
SELECT al.Timestamp, al.UserName, al.Action, al.EntityType, al.EntityId, al.Details
FROM AuditLogs al
ORDER BY al.Timestamp DESC
LIMIT 50;
```

### Audit Logs for Specific User
```sql
SELECT al.Timestamp, al.Action, al.EntityType, al.EntityId, al.Details
FROM AuditLogs al
WHERE al.UserName = 'admin@example.com'
ORDER BY al.Timestamp DESC;
```

---

## Notes

- **SQLite Limitations**: No true decimals; GrantBudget stored as TEXT
- **Nullable Columns**: Most application fields nullable for flexibility
- **Custom Fields**: FirstName, LastName, Primary added to AspNetUsers (standard Identity table)
- **Single Record**: DirectorsPageInfos typically contains one row shared by all directors
- **Security**: Passwords stored using ASP.NET Core Identity hashing (PBKDF2 by default)
- **Audit Logging**: All create, edit, and delete operations are logged to AuditLogs table for compliance
- **FERPA Compliance**: See SECURITY_COMPLIANCE.md for detailed security and privacy guidelines
- **Data Standardization**: Use UpdateDropdownValues.cs and UpdateNamesToUppercase.cs scripts for data cleanup

---

**Database File Location:** `/Users/awyatt/Documents/Projects/TRIO program/TeacherApp/teacherapp.db`
