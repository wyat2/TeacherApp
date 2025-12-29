#!/bin/bash
# Apply direct SQLite ALTER TABLE to add Ethnicity column
DB_FILE="teacherapp.db"
if [ ! -f "$DB_FILE" ]; then
  echo "Database file $DB_FILE not found in project root. Run from project root where teacherapp.db exists."
  exit 1
fi

sqlite3 "$DB_FILE" "ALTER TABLE Students ADD COLUMN Ethnicity TEXT NULL;"
echo "Ethnicity column added (if it did not already exist)."
