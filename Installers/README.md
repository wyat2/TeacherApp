# Build Installers for All Platforms

This directory contains installer scripts and configurations for TeacherApp across Windows, macOS, and Linux.

## About TeacherApp

TeacherApp is a comprehensive student and class management system designed for TRIO programs, featuring:
- **Role-Based Access Control**: Admin, Director, Tutor, and Student roles with granular permissions
- **Audit Logging**: Complete tracking of all user actions with timestamp, user, and operation details
- **Student Management**: Track students with grade, class, ethnicity, and contact information
- **User Management**: Separate interfaces for managing Admins, Directors, and Tutors
- **Directors Dashboard**: Manage school information, director details, and grant tracking (PR/Award Number)
- **Search & Filtering**: Find students by name/email and filter by grade or class
- **Data Export**: Export filtered student data to CSV
- **Security Compliance**: FERPA-compliant with comprehensive password policies and session management
- **Data Update Scripts**: Built-in tools for data standardization and migration
- **Setup Wizard**: First-run configuration for easy deployment

## Quick Start

### Windows (MSI)
```bash
./Installers/build-windows-msi.bat
```
Output: `build-windows/TeacherApp-1.0.0.msi`

### macOS (DMG)
```bash
chmod +x Installers/build-macos-dmg.sh
./Installers/build-macos-dmg.sh
```
Output: `TeacherApp-1.0.0.dmg`

### Linux (Bash Installer)
No build needed — distribute `Installers/install-linux.sh` to users.

Users run:
```bash
sudo bash install-linux.sh
```

## Files

| File | Purpose |
|------|---------|
| `TeacherApp.wxs` | WiX source for Windows MSI |
| `build-windows-msi.bat` | Windows build script (requires WiX) |
| `build-macos-dmg.sh` | macOS DMG builder |
| `install-linux.sh` | Linux installer script |
| `post-install.sh` | Post-install hook for .deb packages |
| `INSTALLER_GUIDE.md` | Complete installer documentation |

## Prerequisites

### Windows
- .NET 10 SDK
- WiX Toolset 3.11+ ([Download](https://wixtoolset.org/))

### macOS
- .NET 10 SDK
- Xcode Command Line Tools (for signing)

### Linux
- .NET 10 SDK
- (Optional) fpm for .deb package creation

## Distribution

1. **Windows:** Distribute `.msi` file — users run installer directly
2. **macOS:** Distribute `.dmg` file — users drag & drop to Applications
3. **Linux:** Distribute `.sh` script — users run with `sudo bash install-linux.sh`

For more details, see `INSTALLER_GUIDE.md`.
