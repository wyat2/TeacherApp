#!/bin/bash
# Post-install script for Linux .deb packages
# Runs after package installation

set -e

# Create service user if not exists
if ! id "teacherapp" &>/dev/null; then
    useradd -r -s /bin/false teacherapp
fi

# Set permissions
chown -R teacherapp:teacherapp /opt/TeacherApp
chmod +x /opt/TeacherApp/TeacherApp

# Enable systemd service
systemctl daemon-reload
systemctl enable teacherapp.service || true

echo ""
echo "========================================"
echo "TeacherApp installed successfully!"
echo "========================================"
echo ""
echo "Next steps:"
echo "1. Start the service: systemctl start teacherapp"
echo "2. Open browser to http://localhost:5000"
echo "3. Complete the setup wizard (first-time only)"
echo "4. Create your Admin account and start using TeacherApp"
echo ""
echo "View logs: journalctl -u teacherapp -f"
echo "Database location: /opt/TeacherApp/teacherapp.db"
echo "Backup the database file regularly!"
echo ""
