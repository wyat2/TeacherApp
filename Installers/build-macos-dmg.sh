#!/bin/bash

# TeacherApp macOS Installer Script
# Creates a DMG installer and app bundle
# Run this on macOS after publishing the app

set -e

echo "========================================"
echo "TeacherApp macOS Installer Builder"
echo "========================================"

# Configuration
APP_NAME="TeacherApp"
VERSION="1.0.0"
PUBLISH_DIR="./bin/Release/net10.0/publish"
BUILD_DIR="./build-macos"
DMG_NAME="TeacherApp-${VERSION}.dmg"

# Check if dotnet publish exists
if [ ! -d "$PUBLISH_DIR" ]; then
    echo "Publishing application..."
    dotnet publish -c Release -o "$PUBLISH_DIR"
fi

# Create build directory
rm -rf "$BUILD_DIR"
mkdir -p "$BUILD_DIR"

# Create macOS app bundle structure
APP_BUNDLE="$BUILD_DIR/$APP_NAME.app/Contents"
mkdir -p "$APP_BUNDLE/MacOS"
mkdir -p "$APP_BUNDLE/Resources"

# Copy application files
cp -r "$PUBLISH_DIR"/* "$APP_BUNDLE/MacOS/"

# Create Info.plist
cat > "$APP_BUNDLE/Info.plist" <<'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDisplayName</key>
    <string>TeacherApp</string>
    <key>CFBundleExecutable</key>
    <string>TeacherApp</string>
    <key>CFBundleIdentifier</key>
    <string>com.teacherapp.app</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>TeacherApp</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    <key>CFBundleVersion</key>
    <string>1</string>
    <key>NSPrincipalClass</key>
    <string>NSApplication</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF

# Make executable
chmod +x "$APP_BUNDLE/MacOS/TeacherApp"

# Create DMG
echo "Creating DMG installer..."
hdiutil create -volname "$APP_NAME" -srcfolder "$BUILD_DIR" -ov -format UDZO "$DMG_NAME"

echo "========================================"
echo "macOS DMG created: $DMG_NAME"
echo "========================================"
echo ""
echo "Distribution instructions:"
echo "1. Sign the DMG (optional but recommended):"
echo "   codesign --deep --force --verify --verbose --sign - $BUILD_DIR/$APP_NAME.app"
echo ""
echo "2. Notarize the DMG for Big Sur+ (required for distribution):"
echo "   xcrun altool --notarize-app -f $DMG_NAME -t osx -u your-apple-id@example.com -p your-app-password"
echo ""
echo "3. Users can mount and drag TeacherApp.app to /Applications"
echo ""
echo "First-time setup for end users:"
echo "- Launch TeacherApp.app from Applications"
echo "- Complete the setup wizard to create Admin account"
echo "- Database will be stored in the app directory"
echo "- Access at http://localhost:5000"
