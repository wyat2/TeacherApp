# 🚀 Quick Reference: Deploy TeacherApp to Google Cloud

## Prerequisites Checklist
- [ ] Google Cloud account with billing enabled
- [ ] gcloud CLI installed
- [ ] Docker installed
- [ ] Project ID ready

## 🎯 One-Time Setup (15 minutes)

### 1. Login & Set Project
```powershell
gcloud auth login
gcloud config set project YOUR_PROJECT_ID
```

### 2. Create Database
```powershell
gcloud sql instances create teacherapp-db --database-version=POSTGRES_15 --tier=db-f1-micro --region=us-central1 --root-password=YOUR_ROOT_PASSWORD
gcloud sql databases create teacherapp --instance=teacherapp-db
gcloud sql users create teacherapp-user --instance=teacherapp-db --password=YOUR_USER_PASSWORD
```

### 3. Deploy App
```powershell
# Option A: Use script
.\deploy-cloud-run.ps1

# Option B: Manual
gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/teacherapp
gcloud run deploy teacherapp --image gcr.io/YOUR_PROJECT_ID/teacherapp --region us-central1 --add-cloudsql-instances YOUR_PROJECT_ID:us-central1:teacherapp-db --set-env-vars "ConnectionStrings__PostgreSqlConnection=Host=/cloudsql/YOUR_PROJECT_ID:us-central1:teacherapp-db;Database=teacherapp;Username=teacherapp-user;Password=YOUR_PASSWORD"
```

## 🔄 Update After Changes
```powershell
gcloud builds submit --tag gcr.io/YOUR_PROJECT_ID/teacherapp
gcloud run deploy teacherapp --image gcr.io/YOUR_PROJECT_ID/teacherapp --region us-central1
```

## 📊 Useful Commands

| Action | Command |
|--------|---------|
| View logs | `gcloud logging read "resource.type=cloud_run_revision" --limit 50` |
| Get service URL | `gcloud run services describe teacherapp --region=us-central1 --format='value(status.url)'` |
| Check DB status | `gcloud sql instances describe teacherapp-db` |
| Connect to DB | `gcloud sql connect teacherapp-db --user=teacherapp-user --database=teacherapp` |
| Delete service | `gcloud run services delete teacherapp --region=us-central1` |
| Delete database | `gcloud sql instances delete teacherapp-db` |

## 💰 Cost Management
- Scale to zero when idle (automatic)
- Delete when not needed: `gcloud run services delete teacherapp`
- Stop database: `gcloud sql instances patch teacherapp-db --activation-policy=NEVER`
- Start database: `gcloud sql instances patch teacherapp-db --activation-policy=ALWAYS`

## 🛑 Troubleshooting

| Issue | Solution |
|-------|----------|
| Can't connect to DB | Check connection string format and Cloud SQL instance added |
| Migration errors | Check logs with `gcloud logging read` |
| App won't start | Verify environment variables are set correctly |
| High costs | Check instance is scaling to zero, review Cloud SQL tier |

## 📁 Important Files
- `deploy-cloud-run.ps1` - Deployment script
- `CLOUD_DEPLOYMENT.md` - Full guide
- `cloudsql-setup.md` - Detailed setup
- `DEPLOYMENT_SUMMARY.md` - Complete overview

## 🔐 Security Checklist
- [ ] Use strong passwords (12+ chars, mixed case, numbers, symbols)
- [ ] Never commit passwords to git
- [ ] Use Secret Manager for production
- [ ] Enable Cloud SQL backups
- [ ] Review audit logs regularly

## ✅ Success Indicators
1. Cloud Run service shows "Healthy" status
2. Can access service URL
3. Setup wizard loads
4. Can create admin account
5. Database persists after container restart
