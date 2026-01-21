# TeacherApp Cloud Run Deployment Script (PowerShell)
# This script deploys the TeacherApp to Google Cloud Run with Cloud SQL connection

# Configuration - UPDATE THESE VALUES
$PROJECT_ID = "fab-penguin"
$REGION = "us-central1"
$SERVICE_NAME = "teacherapp"
$DB_INSTANCE_NAME = "teacherapp-db"
$DB_NAME = "teacherapp"
$DB_USER = "teacherapp-user"

Write-Host "Starting TeacherApp deployment to Cloud Run" -ForegroundColor Green

# Check if gcloud is installed
if (!(Get-Command gcloud -ErrorAction SilentlyContinue)) {
    Write-Host "Error: gcloud CLI is not installed" -ForegroundColor Red
    exit 1
}

# Set the project
Write-Host "Setting project to $PROJECT_ID" -ForegroundColor Yellow
gcloud config set project $PROJECT_ID

# Get the Cloud SQL connection name
Write-Host "Getting Cloud SQL connection name" -ForegroundColor Yellow
$CONNECTION_NAME = (gcloud sql instances describe $DB_INSTANCE_NAME --format='value(connectionName)') | Out-String
$CONNECTION_NAME = $CONNECTION_NAME.Trim()
Write-Host "Connection name: $CONNECTION_NAME" -ForegroundColor Green

# Prompt for database password
$DB_PASSWORD = Read-Host "Enter the database password for user $DB_USER" -AsSecureString
$DB_PASSWORD_PLAIN = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($DB_PASSWORD))

# Build the Docker image
Write-Host "Building Docker image" -ForegroundColor Yellow
$IMAGE_NAME = "gcr.io/$PROJECT_ID/$SERVICE_NAME`:latest"
docker build -t $IMAGE_NAME .

# Configure Docker to use gcloud
Write-Host "Configuring Docker authentication" -ForegroundColor Yellow
gcloud auth configure-docker --quiet

# Push the image
Write-Host "Pushing image to Google Container Registry" -ForegroundColor Yellow
docker push $IMAGE_NAME

# Build the connection string for Cloud SQL Unix socket
$CONNECTION_STRING = "Host=/cloudsql/$CONNECTION_NAME;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD_PLAIN"

# Deploy to Cloud Run
Write-Host "Deploying to Cloud Run" -ForegroundColor Yellow
gcloud run deploy $SERVICE_NAME `
  --image $IMAGE_NAME `
  --platform managed `
  --region $REGION `
  --allow-unauthenticated `
  --add-cloudsql-instances $CONNECTION_NAME `
  --set-env-vars "ConnectionStrings__PostgreSqlConnection=$CONNECTION_STRING" `
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production" `
  --memory 512Mi `
  --cpu 1 `
  --max-instances 10 `
  --min-instances 0 `
  --timeout 300

# Get the service URL
$SERVICE_URL = (gcloud run services describe $SERVICE_NAME --region=$REGION --format='value(status.url)') | Out-String
$SERVICE_URL = $SERVICE_URL.Trim()

Write-Host "Deployment complete!" -ForegroundColor Green
Write-Host "Service URL: $SERVICE_URL" -ForegroundColor Green
Write-Host "Note: The application will run migrations automatically on startup" -ForegroundColor Yellow
