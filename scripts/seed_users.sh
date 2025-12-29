#!/usr/bin/env bash
# Seed users by logging in as admin and posting to Create endpoints
set -euo pipefail
BASE="http://localhost:5000"
ADMIN_EMAIL="admin@example.com"
ADMIN_PASS="Password123!"
COOKIEFILE="/tmp/teacherapp_cookies.txt"
TMP_LOGIN="/tmp/teacherapp_login.html"

echo "Ensure the app is running on http://localhost:5000"

# fetch login page and extract antiforgery token
curl -s -c "$COOKIEFILE" "$BASE/Account/Login" -o "$TMP_LOGIN"
TOKEN=$(perl -nle 'print $1 if /name="__RequestVerificationToken".*?value="([^"]+)"/' "$TMP_LOGIN")
if [ -z "$TOKEN" ]; then
  echo "Failed to extract antiforgery token"
  exit 1
fi

# perform login
curl -s -b "$COOKIEFILE" -c "$COOKIEFILE" -X POST -L "$BASE/Account/Login" \
  -d "__RequestVerificationToken=$TOKEN&Email=$ADMIN_EMAIL&Password=$ADMIN_PASS&RememberMe=false" -o /dev/null

# helper to post create for role
post_create() {
  role="$1" # Tutor|Student|Admin
  email="$2"
  first="$3"
  last="$4"
  pass="$5"
  phone="$6"

  # fetch create page to get antiforgery token
  curl -s -b "$COOKIEFILE" -c "$COOKIEFILE" "$BASE/$role/Create" -o "$TMP_LOGIN"
  TOKEN2=$(perl -nle 'print $1 if /name="__RequestVerificationToken".*?value="([^"]+)"/' "$TMP_LOGIN")
  if [ -z "$TOKEN2" ]; then
    echo "Failed to get antiforgery token for $role Create"
    return 1
  fi

  curl -s -b "$COOKIEFILE" -c "$COOKIEFILE" -X POST -L "$BASE/$role/Create" \
    -d "__RequestVerificationToken=$TOKEN2&email=$email&firstName=$first&lastName=$last&password=$pass&phoneNumber=$phone" -o /dev/null
}

# simple name lists
FIRSTS=(Olivia Liam Emma Noah Ava Oliver Sophia Elijah Isabella Lucas Mia Mason Amelia Logan)
LASTS=(Smith Johnson Williams Brown Jones Garcia Miller Davis Rodriguez Martinez Hernandez Lopez Gonzalez Wilson Anderson Thomas Taylor Moore Jackson Martin Lee Perez Thompson White Harris)

rand(){
  echo $((RANDOM))
}

COUNT=40
PASSWORD="Password123!"

for role in Tutor Student Admin; do
  echo "Creating $COUNT users for role $role..."
  for i in $(seq 1 $COUNT); do
    f=${FIRSTS[$((RANDOM % ${#FIRSTS[@]}))]}
    l=${LASTS[$((RANDOM % ${#LASTS[@]}))]}
    num=$(printf "%03d" $i)
    email=$(printf "%s.%s.%s.%s@example.com" "$f" "$l" "$role" "$num" | tr '[:upper:]' '[:lower:]')
    phone="555-1$(printf "%03d" $((RANDOM%900+100)))"
    post_create "$role" "$email" "$f" "$l" "$PASSWORD" "$phone" || true
    if (( i % 10 == 0 )); then echo "  $i/$COUNT for $role"; fi
  done
done

echo "Seeding done."

# show counts
sqlite3 teacherapp.db "SELECT r.Name, COUNT(*) FROM AspNetUsers u JOIN AspNetUserRoles ur ON ur.UserId=u.Id JOIN AspNetRoles r ON r.Id=ur.RoleId GROUP BY r.Name;"
