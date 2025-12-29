#!/bin/bash

# TeacherApp Linux Installer Script
# Supports: Ubuntu/Debian, CentOS/RHEL, Fedora
# Usage: sudo bash install-linux.sh

set -e

echo "========================================"
echo "TeacherApp Linux Installer"
echo "========================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if running as root
if [[ $EUID -ne 0 ]]; then
   echo -e "${RED}This script must be run as root (use sudo)${NC}"
   exit 1
fi

# Detect OS
if [ -f /etc/os-release ]; then
    . /etc/os-release
    OS=$ID
else
    echo -e "${RED}Cannot detect OS. Exiting.${NC}"
    exit 1
fi

echo -e "${YELLOW}Detected OS: $OS${NC}"

# Install .NET 10 runtime if not present
if ! command -v dotnet &> /dev/null; then
    echo -e "${YELLOW}Installing .NET 10 runtime...${NC}"
    
    if [[ "$OS" == "ubuntu" || "$OS" == "debian" ]]; then
        apt-get update
        apt-get install -y wget
        wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
        chmod +x dotnet-install.sh
        ./dotnet-install.sh --channel 10.0
        rm dotnet-install.sh
        export PATH="$HOME/.dotnet:$PATH"
    elif [[ "$OS" == "centos" || "$OS" == "rhel" || "$OS" == "fedora" ]]; then
        dnf install -y dotnet-runtime-10.0
    fi
else
    echo -e "${GREEN}.NET runtime already installed$(dotnet --version)${NC}"
fi

# Installation directory
INSTALL_DIR="/opt/TeacherApp"
SERVICE_USER="teacherapp"

echo -e "${YELLOW}Installing to $INSTALL_DIR${NC}"

# Create service user
if ! id "$SERVICE_USER" &>/dev/null; then
    echo -e "${YELLOW}Creating service user: $SERVICE_USER${NC}"
    useradd -r -s /bin/false $SERVICE_USER
fi

# Create installation directory
mkdir -p $INSTALL_DIR
cd $INSTALL_DIR

# Copy application files (assumes publish folder exists)
if [ -d "bin/Release/net10.0/publish" ]; then
    cp -r bin/Release/net10.0/publish/* $INSTALL_DIR/
    echo -e "${GREEN}Application files copied${NC}"
else
    echo -e "${RED}Publish directory not found. Run: dotnet publish -c Release${NC}"
    exit 1
fi

# Set permissions
chown -R $SERVICE_USER:$SERVICE_USER $INSTALL_DIR
chmod +x $INSTALL_DIR/TeacherApp

# Create systemd service file
echo -e "${YELLOW}Creating systemd service...${NC}"
cat > /etc/systemd/system/teacherapp.service <<EOF
[Unit]
Description=TeacherApp - Student & Class Management
After=network.target

[Service]
Type=notify
User=$SERVICE_USER
WorkingDirectory=$INSTALL_DIR
ExecStart=$INSTALL_DIR/TeacherApp
Restart=on-failure
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF

# Enable and start service
systemctl daemon-reload
systemctl enable teacherapp.service
systemctl start teacherapp.service

echo -e "${GREEN}========================================"
echo "TeacherApp installed successfully!"
echo "========================================"
echo -e "Service status: ${NC}"
systemctl status teacherapp.service --no-pager

echo ""
echo -e "${YELLOW}Access the app at: http://localhost:5000${NC}"
echo -e "${YELLOW}View logs: journalctl -u teacherapp -f${NC}"
echo -e "${YELLOW}Stop service: systemctl stop teacherapp${NC}"
echo ""
echo -e "${GREEN}First-time setup:${NC}"
echo "1. Navigate to http://localhost:5000 in your browser"
echo "2. Complete the setup wizard to create your Admin account"
echo "3. Log in and start managing students and users"
echo ""
echo -e "${YELLOW}Database location: $INSTALL_DIR/teacherapp.db${NC}"
echo -e "${YELLOW}Backup this file regularly!${NC}"
