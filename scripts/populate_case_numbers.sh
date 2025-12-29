#!/bin/bash

# Script to populate case numbers for existing students
# This runs the TeacherApp with the --populate-case-numbers flag

cd "$(dirname "$0")/.."

echo "Populating case numbers for existing students..."
dotnet run --populate-case-numbers

echo "Done!"
