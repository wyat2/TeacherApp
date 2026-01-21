# Quick Start Guide: Deploy to Google Cloud Run

## Prerequisites
- Google Cloud account
- gcloud CLI installed ([Download](https://cloud.google.com/sdk/docs/install))
- Docker installed
- Project billing enabled

## Quick Deployment Steps

### 1. Set up Cloud SQL (One-time setup)

```bash
# Login and set project
gcloud auth login
gcloud config set project YOUR_PROJECT_ID

# Create PostgreSQL instance (takes 5-10 minutes)
gcloud sql instances create teacherapp-db \
  --database-version=POSTGRES_15 \
  --tier=db-f1-micro \
  --region=us-central1 \
  --root-password=YOUR_SECURE_ROOT_PASSWORD

# Create database and user
gcloud sql databases create teacherapp --instance=teacherapp-db
gcloud sql users create teacherapp-user \
  --instance=teacherapp-db \
  --password=YOUR_SECURE_USER_PASSWORD
```

### 2. Deploy Application

**Option A: Use PowerShell Script (Windows)**
1. Edit `deploy-cloud-run.ps1` and update:
   - PROJECT_ID
   - DB password when prompted
2. Run: `.\deploy-cloud-run.ps1`

**Option B: Manual Deployment**

```bash
# 1. Build and push Docker image
gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/teacherapp

# 2. Get Cloud SQL connection name
CONNECTION_NAME=$(gcloud sql instances describe teacherapp-db --format='value(connectionName)')

# 3. Deploy to Cloud Run
gcloud run deploy teacherapp \
  --image gcr.io/YOUR_PROJECT_ID/teacherapp \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --add-cloudsql-instances $CONNECTION_NAME \
  --set-env-vars "ConnectionStrings__PostgreSqlConnection=Host=/cloudsql/$CONNECTION_NAME;Database=teacherapp;Username=teacherapp-user;Password=YOUR_PASSWORD"
```

### 3. Verify Deployment

Your app URL will be shown after deployment (e.g., https://teacherapp-xxxxx-uc.a.run.app)

## Key Features

✅ **Automatic Database Migrations** - Runs on startup  
✅ **Auto-scaling** - Scales to 0 when not in use  
✅ **Secure** - Uses Unix socket connection to Cloud SQL  
✅ **HTTPS** - Automatic SSL certificate  

## Connection String Format for Cloud Run

```
Host=/cloudsql/PROJECT:REGION:INSTANCE;Database=teacherapp;Username=teacherapp-user;Password=YOUR_PASSWORD
```

## Costs (Approximate)

- **Cloud SQL (db-f1-micro)**: $7-15/month
- **Cloud Run**: $0 when idle, ~$0.00002400/request
- **Storage (10GB)**: ~$1.70/month

## Troubleshooting

### View Logs
```bash
gcloud logging read "resource.type=cloud_run_revision AND resource.labels.service_name=teacherapp" --limit 50
```

### Test Database Connection
```bash
gcloud sql connect teacherapp-db --user=teacherapp-user --database=teacherapp
```

### Redeploy After Changes
```bash
gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/teacherapp
gcloud run deploy teacherapp --image gcr.io/YOUR_PROJECT_ID/teacherapp --region us-central1
```

## Security Best Practices

1. **Use Secret Manager** for production passwords
2. **Enable Cloud SQL Private IP** for production
3. **Set up Cloud Armor** for DDoS protection
4. **Enable Cloud SQL automatic backups**

## Using Secret Manager (Recommended for Production)

```bash
# Store password in Secret Manager
echo -n "YOUR_PASSWORD" | gcloud secrets create teacherapp-db-password --data-file=-

# Grant Cloud Run access
gcloud secrets add-iam-policy-binding teacherapp-db-password \
  --member=serviceAccount:YOUR_PROJECT_NUMBER-compute@developer.gserviceaccount.com \
  --role=roles/secretmanager.secretAccessor

# Deploy with secret
gcloud run deploy teacherapp \
  --image gcr.io/YOUR_PROJECT_ID/teacherapp \
  --update-secrets=DB_PASSWORD=teacherapp-db-password:latest
```

Then update Program.cs to read from environment variable DB_PASSWORD.

## Need Help?

See detailed documentation in `cloudsql-setup.md`
