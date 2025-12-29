@echo off
REM TeacherApp Windows Installer Build Script
REM Prerequisites: 
REM   - WiX Toolset 3.11+ installed (https://wixtoolset.org/)
REM   - Visual Studio Build Tools (optional, for better integration)
REM 
REM Usage: build-windows-msi.bat

echo ========================================
echo TeacherApp Windows MSI Installer Builder
echo ========================================

REM Check if WiX is installed
where candle.exe >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: WiX Toolset not found. Please install WiX 3.11+
    echo Download from: https://wixtoolset.org/
    exit /b 1
)

REM Publish the application
echo Publishing application...
dotnet publish -c Release -o bin\Release\net10.0\publish

REM Create build directories
if not exist "build-windows" mkdir build-windows
if not exist "build-windows\obj" mkdir build-windows\obj

REM Compile WiX source
echo Compiling WiX source...
candle.exe Installers\TeacherApp.wxs -o build-windows\obj\ -dPublishDir=bin\Release\net10.0\publish\

REM Link to create MSI
echo Linking to create MSI...
light.exe -out build-windows\TeacherApp-1.0.0.msi build-windows\obj\TeacherApp.wixobj

echo ========================================
echo MSI Installer created successfully!
echo Location: build-windows\TeacherApp-1.0.0.msi
echo ========================================
