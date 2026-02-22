# TeacherApp Cloud Run Deployment Script (PowerShell)
# This script deploys the TeacherApp to Google Cloud Run using Cloud Build
# Email credentials are managed via Google Secret Manager

# Configuration
$PROJECT_ID = "fab-penguin"
$REGION = "us-central1"
$SERVICE_NAME = "teacherapp"

Write-Host "Starting TeacherApp deployment to Cloud Run" -ForegroundColor Green

# Check if gcloud is installed
if (!(Get-Command gcloud -ErrorAction SilentlyContinue)) {
    Write-Host "Error: gcloud CLI is not installed" -ForegroundColor Red
    exit 1
}

# Set the project
Write-Host "Setting project to $PROJECT_ID" -ForegroundColor Yellow
gcloud config set project $PROJECT_ID

# Get current commit SHA
Write-Host "Getting current commit SHA" -ForegroundColor Yellow
$COMMIT_SHA = (git rev-parse --short HEAD)
Write-Host "Commit SHA: $COMMIT_SHA" -ForegroundColor Green

# Deploy using Cloud Build
Write-Host "Deploying using Cloud Build (this takes 3-5 minutes)..." -ForegroundColor Yellow
gcloud builds submit --config cloudbuild.yaml --substitutions=COMMIT_SHA=$COMMIT_SHA .

# Get the service URL
$SERVICE_URL = (gcloud run services describe $SERVICE_NAME --region=$REGION --format='value(status.url)') | Out-String
$SERVICE_URL = $SERVICE_URL.Trim()

Write-Host "Deployment complete!" -ForegroundColor Green
Write-Host "Service URL: $SERVICE_URL" -ForegroundColor Green
Write-Host "Note: Email credentials are securely stored in Secret Manager" -ForegroundColor Yellow
