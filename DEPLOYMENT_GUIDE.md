# TeacherApp Deployment Guide

## Overview
TeacherApp uses Google Cloud Run for production deployment with credentials securely stored in Google Secret Manager.

## Prerequisites
- Git installed
- gcloud CLI installed and authenticated
- Google Cloud project: `fab-penguin`

## Local Development

### Running Locally
```powershell
# Run with dotnet
dotnet run

# Or using Docker Compose
docker-compose up
```

**Local Configuration:**
- Email credentials are in `appsettings.Development.json` (gitignored, not committed)
- Database runs in Docker container (PostgreSQL)
- Application runs on http://localhost:5000

## Production Deployment

### Quick Deploy (Recommended)

**Option 1: Using PowerShell Script**
```powershell
.\deploy-cloud-run.ps1
```

**Option 2: Using Makefile** (requires make/WSL)
```bash
make deploy
```

**Option 3: Manual Cloud Build**
```powershell
# Get current commit SHA
$COMMIT_SHA = (git rev-parse --short HEAD)

# Submit to Cloud Build
gcloud builds submit --config cloudbuild.yaml --substitutions=COMMIT_SHA=$COMMIT_SHA .
```

### Standard Deployment Workflow

1. **Make your code changes**

2. **Commit and push to GitHub**
   ```powershell
   git add .
   git commit -m "Your commit message"
   git push origin main
   ```

3. **Deploy to Cloud Run**
   ```powershell
   .\deploy-cloud-run.ps1
   ```

**Expected deployment time:** 3-5 minutes

## Secret Manager Configuration

Email credentials are stored securely in Google Secret Manager:

### Secrets Configured
- `email-password` - SMTP password/app password
- `email-username` - SMTP username/email
- `email-smtp-server` - SMTP server (smtp.gmail.com)

### Updating Secrets
```powershell
# Update email password
echo "new-password" | gcloud secrets versions add email-password --data-file=-

# Update email username
echo "new-email@domain.com" | gcloud secrets versions add email-username --data-file=-

# Update SMTP server
echo "smtp.newserver.com" | gcloud secrets versions add email-smtp-server --data-file=-
```

After updating secrets, redeploy the application for changes to take effect.

## How It Works

### Build Process (Cloud Build)
1. **Build** - Docker image built in Google Cloud
2. **Push** - Image pushed to Google Container Registry (gcr.io)
3. **Deploy** - New revision deployed to Cloud Run

### Runtime Configuration
- **Production Mode**: Loads email credentials from Secret Manager
- **Development Mode**: Uses credentials from appsettings.Development.json
- **Database**: Connects to Cloud SQL PostgreSQL via Unix socket

### Environment Variables (Cloud Run)
- `ASPNETCORE_ENVIRONMENT=Production`
- `GOOGLE_CLOUD_PROJECT=fab-penguin`
- `ConnectionStrings__PostgreSqlConnection` - Database connection string
- `EmailSettings__SmtpPort=587`
- `EmailSettings__SenderName=TeacherApp Support`

## Troubleshooting

### Deployment Fails
```powershell
# Check recent builds
gcloud builds list --limit=5

# View logs for specific build
gcloud builds log [BUILD_ID]
```

### Application Errors
```powershell
# View Cloud Run logs
gcloud run services logs read teacherapp --region=us-central1 --limit=100

# Or using Make
make logs
```

### Secret Access Issues
```powershell
# Verify secrets exist
gcloud secrets list

# Check IAM permissions
gcloud secrets get-iam-policy email-password
```

## Architecture

```
Developer
    ↓ (git push)
GitHub Repository
    ↓ (gcloud builds submit)
Google Cloud Build
    ↓ (docker build)
Container Registry (gcr.io)
    ↓ (deploy)
Cloud Run
    ↓ (reads secrets at runtime)
Secret Manager
```

## Security Notes

✅ **Secure:**
- Email credentials stored in Secret Manager (encrypted)
- Database password in environment variable (not in code)
- HTTPS enforced
- Container images don't contain plain text secrets

❌ **Don't:**
- Commit `appsettings.Development.json` (already in .gitignore)
- Hardcode credentials in code
- Share Secret Manager access unnecessarily

## Service URLs

- **Production**: https://teacherapp-75647xanxa-uc.a.run.app
- **Cloud Console**: https://console.cloud.google.com/run?project=fab-penguin
- **Secret Manager**: https://console.cloud.google.com/security/secret-manager?project=fab-penguin
