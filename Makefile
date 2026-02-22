.PHONY: help build-local run-local test docker-build docker-run docker-stop docker-logs deploy logs status clean check-deploy-env

# Variables
PROJECT_ID = fab-penguin
REGION = us-central1
SERVICE_NAME = teacherapp
IMAGE_NAME = teacherapp
REGISTRY = us-central1-docker.pkg.dev
REPOSITORY = teacherapp-repo
IMAGE_TAG = latest
FULL_IMAGE_NAME = $(REGISTRY)/$(PROJECT_ID)/$(REPOSITORY)/$(IMAGE_NAME):$(IMAGE_TAG)

# Production deployment settings
DB_CONNECTION_STRING ?=
COMMIT_SHA ?= $(shell git rev-parse --short HEAD)

help:
	@echo "TeacherApp Deployment Commands"
	@echo "==============================="
	@echo ""
	@echo "Local Development:"
	@echo "  make build-local    - Build the .NET application locally"
	@echo "  make run-local      - Run the application locally with dotnet"
	@echo "  make clean          - Clean build artifacts"
	@echo ""
	@echo "Docker Local Testing:"
	@echo "  make docker-build   - Build Docker image locally"
	@echo "  make docker-run     - Run application in Docker (docker-compose)"
	@echo "  make docker-stop    - Stop Docker containers"
	@echo "  make docker-logs    - View Docker container logs"
	@echo ""
	@echo "Cloud Run Deployment:"
	@echo "  make deploy         - Build and deploy to Cloud Run using Cloud Build"
	@echo "  make logs           - View Cloud Run logs"
	@echo "  make status         - Check Cloud Run service status"
	@echo "  make url            - Get the service URL"
	@echo ""
	@echo "Current Configuration:"
	@echo "  Project ID: $(PROJECT_ID)"
	@echo "  Region: $(REGION)"
	@echo "  Service: $(SERVICE_NAME)"
	@echo ""
	@echo "Deploy example:"
	@echo "  make deploy"

# Local development targets
build-local:
	@echo "Building application locally..."
	dotnet build -c Release

run-local:
	@echo "Running application locally on http://localhost:5000..."
	dotnet run

clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf bin obj

# Docker targets
docker-build:
	@echo "Building Docker image..."
	docker build -t $(IMAGE_NAME):$(IMAGE_TAG) .
	@echo "Docker image built: $(IMAGE_NAME):$(IMAGE_TAG)"

docker-run:
	@echo "Starting application with docker-compose..."
	docker-compose up -d
	@echo "Application running at http://localhost:8080"
	@echo "Use 'make docker-logs' to view logs"
	@echo "Use 'make docker-stop' to stop"

docker-stop:
	@echo "Stopping Docker containers..."
	docker-compose down

docker-logs:
	@echo "Showing Docker logs (Ctrl+C to exit)..."
	docker-compose logs -f

# Google Cloud setup
setup-gcloud:
	@echo "Setting up Google Cloud project..."
	@echo "Setting project to $(PROJECT_ID)..."
	gcloud config set project $(PROJECT_ID)
	@echo ""
	@echo "Enabling required APIs..."
	gcloud services enable run.googleapis.com
	gcloud services enable cloudbuild.googleapis.com
	gcloud services enable artifactregistry.googleapis.com
	@echo ""
	@echo "Creating Artifact Registry repository..."
	-gcloud artifacts repositories create $(REPOSITORY) \
		--repository-format=docker \
		--location=$(REGION) \
		--description="TeacherApp container images" 2>/dev/null || echo "Repository already exists"
	@echo ""
	@echo "Configuring Docker authentication..."
	gcloud auth configure-docker $(REGISTRY)
	@echo ""
	@echo "Setup complete!"

# Cloud Run deployment using Cloud Build
deploy:
	@echo "Building and deploying to Cloud Run using Cloud Build..."
	@echo "Commit SHA: $(COMMIT_SHA)"
	gcloud builds submit --config cloudbuild.yaml --substitutions=COMMIT_SHA=$(COMMIT_SHA) .
	@echo ""
	@echo "Deployment complete!"
	@make url

logs:
	@echo "Fetching Cloud Run logs..."
	gcloud run services logs read $(SERVICE_NAME) \
		--region=$(REGION) \
		--limit=50

status:
	@echo "Cloud Run service status:"
	gcloud run services describe $(SERVICE_NAME) \
		--region=$(REGION) \
		--format="table(status.url,status.conditions[0].type,status.conditions[0].status,status.traffic[0].percent)"

url:
	@echo "Service URL:"
	@gcloud run services describe $(SERVICE_NAME) \
		--region=$(REGION) \
		--format="value(status.url)"
