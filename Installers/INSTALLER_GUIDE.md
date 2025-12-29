# TeacherApp Installer Guide

## Overview

TeacherApp includes installers for Windows, macOS, and Linux. Choose the appropriate method for your target platform.

---

## Windows Installation

### For End Users (Using MSI Installer)

1. Download `TeacherApp-1.0.0.msi`
2. Double-click to run the installer
3. Follow the setup wizard
4. Choose installation directory (default: `C:\Program Files\TeacherApp`)
5. Click "Install"
6. Shortcut will be created on Start Menu and Desktop
7. Launch from Start Menu or desktop shortcut

### For Developers (Building MSI)

**Prerequisites:**
- Visual Studio 2019+ or Build Tools
- WiX Toolset 3.11+ (https://wixtoolset.org/)

**Build Steps:**
```bash
cd TeacherApp
./Installers/build-windows-msi.bat
```

**Output:** `build-windows/TeacherApp-1.0.0.msi`

**Distribution:**
- Sign the MSI (recommended):
  ```bash
  signtool sign /f certificate.pfx /p password /t http://timestamp.server /d "TeacherApp" TeacherApp-1.0.0.msi
  ```
- Upload to your distribution server

---

## Linux Installation

### For End Users (Using Installer Script)

**Supported Distributions:**
- Ubuntu 20.04 LTS+
- Debian 11+
- CentOS 8+
- RHEL 8+
- Fedora 35+

**Installation Steps:**

1. Download `install-linux.sh`
2. Make it executable:
   ```bash
   chmod +x install-linux.sh
   ```
3. Run with sudo:
   ```bash
   sudo bash install-linux.sh
   ```
4. The installer will:
   - Check for .NET 10 runtime (install if missing)
   - Create `teacherapp` service user
   - Install app to `/opt/TeacherApp`
   - Create systemd service
   - Start the service automatically

5. Access the app:
   - URL: `http://localhost:5000`
   - Logs: `journalctl -u teacherapp -f`

**Service Management:**
```bash
# Start
sudo systemctl start teacherapp

# Stop
sudo systemctl stop teacherapp

# Restart
sudo systemctl restart teacherapp

# View logs
sudo journalctl -u teacherapp -f

# Enable on boot
sudo systemctl enable teacherapp
```

### For Developers (Building Linux Package)

**Alternative: Create .deb package (Debian/Ubuntu)**

```bash
# Install fpm (package maker)
gem install fpm

# Publish the application
dotnet publish -c Release

# Create .deb package
fpm -s dir -t deb -n teacherapp -v 1.0.0 \
  --after-install Installers/post-install.sh \
  -C bin/Release/net10.0/publish/ \
  -p TeacherApp_1.0.0_amd64.deb

# Install
sudo dpkg -i TeacherApp_1.0.0_amd64.deb
```

---

## macOS Installation

### For End Users (Using DMG)

1. Download `TeacherApp-1.0.0.dmg`
2. Double-click to mount the DMG
3. Drag `TeacherApp.app` to `/Applications`
4. Eject the DMG
5. Open Applications folder, double-click `TeacherApp.app` to run

**System Requirements:**
- macOS 10.15+
- .NET 10 runtime (app will prompt if missing)

### For Developers (Building DMG)

**Build Steps:**
```bash
cd TeacherApp
chmod +x Installers/build-macos-dmg.sh
./Installers/build-macos-dmg.sh
```

**Output:** `TeacherApp-1.0.0.dmg`

**Code Signing (Recommended):**
```bash
# Sign the app
codesign --deep --force --verify --verbose --sign - build-macos/TeacherApp.app

# Notarize (required for Big Sur+)
xcrun altool --notarize-app -f TeacherApp-1.0.0.dmg \
  -t osx -u your-apple-id@example.com -p your-app-password
```

---

## Docker (Optional - Cross-Platform)

For cloud/server deployment, use Docker:

**Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY bin/Release/net10.0/publish/ .

EXPOSE 5000
CMD ["dotnet", "TeacherApp.dll"]
```

**Build & Run:**
```bash
docker build -t teacherapp:1.0.0 .
docker run -p 5000:5000 teacherapp:1.0.0
```

---

## Post-Installation Configuration

### Initial Setup Wizard
On first launch, TeacherApp will prompt you through a setup wizard to:
1. Create the initial Admin account (username, password, name, email)
2. Configure the application for first use

**Note:** The setup wizard only runs once. Complete it carefully!

### User Roles
TeacherApp includes four user roles with different permissions:
- **Admin**: Full system access including user management, permissions, and all data
- **Director**: Access to school/director/grant information, student/tutor management
- **Tutor**: Limited access to students and tutors pages only
- **Student**: (Reserved for future features)

### Database
- **Type**: SQLite (default) - `teacherapp.db` in application directory
- **Location**: Same directory as the application executable
- **Backup**: Regularly backup the `teacherapp.db` file
- **Migrations**: Database schema updates are applied automatically on startup
- For production with multiple users: Consider migrating to SQL Server/PostgreSQL in `appsettings.json`

### Key Features
- **Student Management**: Create, edit, delete students with grade/class filtering and CSV export
- **User Management**: Separate pages for Admins, Directors, and Tutors
- **Role-Based Permissions**: Granular control over who can view/edit/delete each user type
- **Directors Page**: Track school info, director info, and grant information (type, PR/Award Number, budget)
- **Search & Filter**: Search by name/email on Students and Tutors pages; filter students by grade and class
- **Export Data**: Export filtered student lists to CSV

### Development/Testing: Seed Sample Data
To populate the database with sample users and students for testing:

**Linux:**
```bash
export SEED_USERS=true
export SEED_STUDENTS=true
sudo systemctl restart teacherapp
```

**Windows (PowerShell):**
```powershell
$env:SEED_USERS="true"
$env:SEED_STUDENTS="true"
.\TeacherApp.exe
```

**macOS:**
```bash
export SEED_USERS=true
export SEED_STUDENTS=true
./TeacherApp.app/Contents/MacOS/TeacherApp
```

This will create:
- 40 Tutors with randomized names
- 40 Students with randomized data (names, grades, classes, ethnicity)
- 40 Admin users
- Sample data only generates if the database is empty

**⚠️ Remove seed data in production!** Unset environment variables after testing.

### HTTPS
- Self-signed cert used by default
- For production: Add valid SSL certificate

### Port Configuration
- Edit `appsettings.json`:
  ```json
  {
    "Kestrel": {
      "Endpoints": {
        "Http": {
          "Url": "http://0.0.0.0:5000"
        }
      }
    }
  }
  ```

---

## Troubleshooting

### Windows
- **MSI won't install**: Ensure you have admin rights
- **"Cannot find .NET"**: Install .NET 10 runtime from https://dotnet.microsoft.com/download
- **Database file locked**: Close any other instances of TeacherApp before reinstalling

### Linux
- **"Command not found: dotnet"**: Add to PATH: `export PATH="$HOME/.dotnet:$PATH"`
- **Permission denied**: Run with `sudo`
- **Service won't start**: Check logs: `journalctl -u teacherapp -n 50`
- **Port 5000 already in use**: Stop conflicting service or change port in `appsettings.json`
- **Database permission issues**: Ensure `teacherapp` user has write access to app directory

### macOS
- **"Cannot open because it's from an unidentified developer"**: Right-click, select "Open", confirm
- **"Cannot find .NET"**: Install from https://dotnet.microsoft.com/download
- **Permission denied on database**: Ensure user has write permission to app directory

### Common Issues (All Platforms)
- **Setup wizard doesn't appear**: Check if `teacherapp.db` already exists; delete it to re-run setup
- **Can't log in**: Ensure you completed the setup wizard; check username/password (case-sensitive)
- **Missing navigation links**: Some links (Admins, Directors, Permissions) are only visible to Admin/Director roles
- **Export CSV is empty**: Ensure you have students in the database and filters aren't too restrictive
- **Database errors after update**: Migrations run automatically; if issues persist, backup and delete `teacherapp.db` to start fresh

---

## Uninstallation

### Windows
- Control Panel > Programs > Programs and Features > TeacherApp > Uninstall

### Linux
```bash
sudo systemctl stop teacherapp
sudo systemctl disable teacherapp
sudo rm -rf /opt/TeacherApp
sudo userdel teacherapp
sudo rm /etc/systemd/system/teacherapp.service
sudo systemctl daemon-reload
```

### macOS
- Drag `TeacherApp.app` to Trash from Applications folder

---

## Support

For issues or questions, contact: support@teacherapp.example.com
