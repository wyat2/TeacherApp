# Google Cloud SQL PostgreSQL Setup for TeacherApp

## Step 1: Create Cloud SQL PostgreSQL Instance

Run these commands in Google Cloud Shell or with gcloud CLI installed:

```bash
# Set your project ID
gcloud config set project YOUR_PROJECT_ID

# Create Cloud SQL PostgreSQL instance
gcloud sql instances create teacherapp-db \
  --database-version=POSTGRES_15 \
  --tier=db-f1-micro \
  --region=us-central1 \
  --root-password=YOUR_SECURE_PASSWORD \
  --storage-type=SSD \
  --storage-size=10GB \
  --backup \
  --maintenance-window-day=SUN \
  --maintenance-window-hour=3

# Wait for instance to be created (this may take a few minutes)
gcloud sql instances describe teacherapp-db

# Create the database
gcloud sql databases create teacherapp --instance=teacherapp-db

# Create a database user
gcloud sql users create teacherapp-user \
  --instance=teacherapp-db \
  --password=YOUR_DATABASE_PASSWORD
```

## Step 2: Get Connection Details

```bash
# Get the connection name (format: PROJECT_ID:REGION:INSTANCE_NAME)
gcloud sql instances describe teacherapp-db --format='value(connectionName)'
```

**Save this connection name** - you'll need it in the next steps.

## Step 3: Build and Push Docker Image to Google Container Registry

```bash
# Configure Docker to use gcloud as a credential helper
gcloud auth configure-docker

# Build your Docker image
docker build -t gcr.io/YOUR_PROJECT_ID/teacherapp:latest .

# Push the image to Google Container Registry
docker push gcr.io/YOUR_PROJECT_ID/teacherapp:latest
```

## Step 4: Deploy to Cloud Run with Cloud SQL Connection

```bash
# Deploy to Cloud Run with Cloud SQL connection
gcloud run deploy teacherapp \
  --image gcr.io/YOUR_PROJECT_ID/teacherapp:latest \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --add-cloudsql-instances YOUR_CONNECTION_NAME \
  --set-env-vars "ConnectionStrings__PostgreSqlConnection=Host=/cloudsql/YOUR_CONNECTION_NAME;Database=teacherapp;Username=teacherapp-user;Password=YOUR_DATABASE_PASSWORD" \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production" \
  --memory 512Mi \
  --cpu 1 \
  --max-instances 10 \
  --min-instances 0
```

## Alternative: Using Cloud SQL Proxy (for local development)

If you want to test the Cloud SQL connection locally:

```bash
# Download Cloud SQL Proxy
curl -o cloud-sql-proxy https://storage.googleapis.com/cloud-sql-connectors/cloud-sql-proxy/v2.8.0/cloud-sql-proxy.linux.amd64
chmod +x cloud-sql-proxy

# Run the proxy (replace with your connection name)
./cloud-sql-proxy YOUR_CONNECTION_NAME
```

Then update your local appsettings.json to use:
```
Host=127.0.0.1;Port=5432;Database=teacherapp;Username=teacherapp-user;Password=YOUR_DATABASE_PASSWORD
```

## Step 5: Run Database Migrations

After the Cloud Run service is deployed, run migrations:

```bash
# Option 1: SSH into a Cloud Run instance and run migrations
gcloud run services proxy teacherapp --project=YOUR_PROJECT_ID

# Option 2: Use Cloud SQL Proxy locally and run:
dotnet ef database update

# Option 3: The application will automatically run migrations on startup (as configured in Program.cs)
```

## Step 6: Verify Deployment

```bash
# Get the service URL
gcloud run services describe teacherapp --region=us-central1 --format='value(status.url)'

# Test the endpoint
curl YOUR_SERVICE_URL
```

## Important Security Notes

1. **Never commit passwords to source control**
2. Use **Secret Manager** for production passwords:
   ```bash
   echo -n "YOUR_DATABASE_PASSWORD" | gcloud secrets create db-password --data-file=-
   ```
3. Update Cloud Run to use secrets:
   ```bash
   gcloud run services update teacherapp \
     --update-secrets=DB_PASSWORD=db-password:latest
   ```

## Costs Estimation

- **db-f1-micro**: ~$7-15/month
- **10GB SSD**: ~$1.70/month
- **Cloud Run**: Pay per use, ~$0.00002400/request
- **Network egress**: Varies by usage

## Monitoring

View logs:
```bash
gcloud logging read "resource.type=cloud_run_revision AND resource.labels.service_name=teacherapp" --limit 50
```

Monitor database:
```bash
gcloud sql operations list --instance=teacherapp-db
```
