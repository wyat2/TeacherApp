#!/bin/bash

# TeacherApp Cloud Run Deployment Script
# This script deploys the TeacherApp to Google Cloud Run with Cloud SQL connection

set -e

# Configuration - UPDATE THESE VALUES
PROJECT_ID="your-project-id"
REGION="us-central1"
SERVICE_NAME="teacherapp"
DB_INSTANCE_NAME="teacherapp-db"
DB_NAME="teacherapp"
DB_USER="teacherapp-user"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting TeacherApp deployment to Cloud Run${NC}"

# Check if gcloud is installed
if ! command -v gcloud &> /dev/null; then
    echo -e "${RED}Error: gcloud CLI is not installed${NC}"
    exit 1
fi

# Set the project
echo -e "${YELLOW}Setting project to $PROJECT_ID${NC}"
gcloud config set project $PROJECT_ID

# Get the Cloud SQL connection name
echo -e "${YELLOW}Getting Cloud SQL connection name${NC}"
CONNECTION_NAME=$(gcloud sql instances describe $DB_INSTANCE_NAME --format='value(connectionName)')
echo -e "${GREEN}Connection name: $CONNECTION_NAME${NC}"

# Prompt for database password
echo -e "${YELLOW}Enter the database password for user $DB_USER:${NC}"
read -s DB_PASSWORD

# Build the Docker image
echo -e "${YELLOW}Building Docker image${NC}"
IMAGE_NAME="gcr.io/$PROJECT_ID/$SERVICE_NAME:latest"
docker build -t $IMAGE_NAME .

# Configure Docker to use gcloud
echo -e "${YELLOW}Configuring Docker authentication${NC}"
gcloud auth configure-docker --quiet

# Push the image
echo -e "${YELLOW}Pushing image to Google Container Registry${NC}"
docker push $IMAGE_NAME

# Build the connection string for Cloud SQL Unix socket
CONNECTION_STRING="Host=/cloudsql/$CONNECTION_NAME;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD"

# Deploy to Cloud Run
echo -e "${YELLOW}Deploying to Cloud Run${NC}"
gcloud run deploy $SERVICE_NAME \
  --image $IMAGE_NAME \
  --platform managed \
  --region $REGION \
  --allow-unauthenticated \
  --add-cloudsql-instances $CONNECTION_NAME \
  --set-env-vars "ConnectionStrings__PostgreSqlConnection=$CONNECTION_STRING" \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production" \
  --memory 512Mi \
  --cpu 1 \
  --max-instances 10 \
  --min-instances 0 \
  --timeout 300

# Get the service URL
SERVICE_URL=$(gcloud run services describe $SERVICE_NAME --region=$REGION --format='value(status.url)')

echo -e "${GREEN}Deployment complete!${NC}"
echo -e "${GREEN}Service URL: $SERVICE_URL${NC}"
echo -e "${YELLOW}Note: The application will run migrations automatically on startup${NC}"
