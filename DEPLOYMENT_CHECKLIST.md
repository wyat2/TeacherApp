# ✅ Google Cloud Deployment Checklist

Use this checklist to track your deployment progress.

## Phase 1: Prerequisites ☁️

- [ ] Google Cloud account created
- [ ] Billing enabled on your project
- [ ] gcloud CLI installed ([Download](https://cloud.google.com/sdk/docs/install))
- [ ] Docker Desktop installed and running
- [ ] Git installed
- [ ] Authenticated with Google Cloud (`gcloud auth login`)

## Phase 2: Project Setup 🏗️

- [ ] Created or selected Google Cloud project
- [ ] Set project ID: `gcloud config set project YOUR_PROJECT_ID`
- [ ] Enabled required APIs:
  ```powershell
  gcloud services enable sqladmin.googleapis.com
  gcloud services enable run.googleapis.com
  gcloud services enable cloudbuild.googleapis.com
  ```
- [ ] Noted your Project ID: ___________________

## Phase 3: Database Creation 🗄️

- [ ] Created Cloud SQL instance:
  ```powershell
  gcloud sql instances create teacherapp-db `
    --database-version=POSTGRES_15 `
    --tier=db-f1-micro `
    --region=us-central1 `
    --root-password=YOUR_ROOT_PASSWORD
  ```
- [ ] Waited for instance creation (5-10 minutes)
- [ ] Created database:
  ```powershell
  gcloud sql databases create teacherapp --instance=teacherapp-db
  ```
- [ ] Created database user:
  ```powershell
  gcloud sql users create teacherapp-user `
    --instance=teacherapp-db `
    --password=YOUR_USER_PASSWORD
  ```
- [ ] Saved connection name:
  ```powershell
  gcloud sql instances describe teacherapp-db --format='value(connectionName)'
  ```
  Connection Name: ___________________

## Phase 4: Application Deployment 🚀

### Option A: Using Deployment Script (Recommended)

- [ ] Opened `deploy-cloud-run.ps1` in editor
- [ ] Updated PROJECT_ID in the script
- [ ] Ran deployment script: `.\deploy-cloud-run.ps1`
- [ ] Entered database password when prompted
- [ ] Waited for build and deployment (5-10 minutes)
- [ ] Noted the service URL

### Option B: Manual Deployment

- [ ] Built Docker image:
  ```powershell
  docker build -t gcr.io/YOUR_PROJECT_ID/teacherapp:latest .
  ```
- [ ] Configured Docker authentication:
  ```powershell
  gcloud auth configure-docker
  ```
- [ ] Pushed image to Container Registry:
  ```powershell
  docker push gcr.io/YOUR_PROJECT_ID/teacherapp:latest
  ```
- [ ] Deployed to Cloud Run:
  ```powershell
  gcloud run deploy teacherapp `
    --image gcr.io/YOUR_PROJECT_ID/teacherapp:latest `
    --platform managed `
    --region us-central1 `
    --allow-unauthenticated `
    --add-cloudsql-instances YOUR_CONNECTION_NAME `
    --set-env-vars "ConnectionStrings__PostgreSqlConnection=Host=/cloudsql/YOUR_CONNECTION_NAME;Database=teacherapp;Username=teacherapp-user;Password=YOUR_PASSWORD"
  ```
- [ ] Noted the service URL

## Phase 5: Verification ✅

- [ ] Service URL: ___________________
- [ ] Opened service URL in browser
- [ ] Application loaded successfully
- [ ] Setup wizard appeared
- [ ] Created admin account through setup wizard
- [ ] Logged in successfully
- [ ] Database is persisting data (add a student, refresh page, verify it's still there)
- [ ] Checked logs for errors:
  ```powershell
  gcloud logging read "resource.type=cloud_run_revision AND resource.labels.service_name=teacherapp" --limit 50
  ```

## Phase 6: Security Hardening 🔒

- [ ] Changed default passwords to strong passwords
- [ ] Verified passwords meet requirements (12+ chars, mixed case, numbers, special chars)
- [ ] Removed any test/demo accounts
- [ ] Reviewed audit log settings
- [ ] Enabled Cloud SQL automatic backups:
  ```powershell
  gcloud sql instances patch teacherapp-db --backup-start-time=03:00
  ```
- [ ] Configured backup retention:
  ```powershell
  gcloud sql instances patch teacherapp-db --backup-retention-count=7
  ```
- [ ] (Optional) Moved passwords to Secret Manager (see CLOUD_DEPLOYMENT.md)

## Phase 7: Monitoring Setup 📊

- [ ] Set up Cloud Monitoring alerts
- [ ] Configured error rate alerts
- [ ] Configured latency alerts
- [ ] Set up budget alerts for cost control
- [ ] Bookmarked Cloud Run dashboard
- [ ] Bookmarked Cloud SQL dashboard

## Phase 8: Documentation 📝

- [ ] Documented connection string format
- [ ] Saved deployment commands for future reference
- [ ] Noted service URL in team documentation
- [ ] Updated team wiki/documentation with access info
- [ ] Shared admin account details securely with authorized users

## Phase 9: Optional Enhancements ⚡

- [ ] Set up custom domain:
  ```powershell
  gcloud beta run domain-mappings create --service=teacherapp --domain=YOUR_DOMAIN
  ```
- [ ] Configured Cloud CDN for static assets
- [ ] Set up Cloud Armor for DDoS protection
- [ ] Configured VPC connector for private networking
- [ ] Set up Cloud Scheduler for periodic tasks
- [ ] Implemented Secret Manager for sensitive data
- [ ] Set up CI/CD with Cloud Build triggers

## Phase 10: Cost Optimization 💰

- [ ] Reviewed instance sizing (db-f1-micro for low traffic)
- [ ] Verified min-instances=0 (scales to zero)
- [ ] Set max-instances appropriately
- [ ] Reviewed backup retention settings
- [ ] Set up budget alerts
- [ ] Scheduled database shutdown for non-production (optional):
  ```powershell
  gcloud sql instances patch teacherapp-db --activation-policy=NEVER
  ```

## Ongoing Maintenance 🔧

### Weekly
- [ ] Review error logs
- [ ] Check application performance metrics
- [ ] Verify backups are running

### Monthly
- [ ] Review access logs
- [ ] Update dependencies if needed
- [ ] Review and optimize costs
- [ ] Test disaster recovery

### Quarterly
- [ ] Review security settings
- [ ] Update database version if available
- [ ] Review and update documentation
- [ ] Conduct security audit

## Troubleshooting Checklist 🛠️

If something goes wrong:

- [ ] Check Cloud Run logs:
  ```powershell
  gcloud logging read "resource.type=cloud_run_revision" --limit 100
  ```
- [ ] Verify database is running:
  ```powershell
  gcloud sql instances describe teacherapp-db
  ```
- [ ] Test database connection:
  ```powershell
  gcloud sql connect teacherapp-db --user=teacherapp-user
  ```
- [ ] Verify environment variables:
  ```powershell
  gcloud run services describe teacherapp --region=us-central1
  ```
- [ ] Check Cloud SQL instances are added to Cloud Run service
- [ ] Verify connection string format
- [ ] Review migrations status in logs

## Rollback Plan 🔄

If deployment fails:

- [ ] Previous version tag: ___________________
- [ ] Rollback command:
  ```powershell
  gcloud run deploy teacherapp --image gcr.io/YOUR_PROJECT_ID/teacherapp:PREVIOUS_TAG
  ```
- [ ] Backup restoration command:
  ```powershell
  gcloud sql backups restore BACKUP_ID --backup-instance=teacherapp-db
  ```

## Support Resources 📞

- **Documentation**: See `CLOUD_DEPLOYMENT.md`, `QUICK_REFERENCE.md`
- **Google Cloud Support**: https://cloud.google.com/support
- **Cloud Run Docs**: https://cloud.google.com/run/docs
- **Cloud SQL Docs**: https://cloud.google.com/sql/docs
- **Community**: Stack Overflow tag `google-cloud-run`, `google-cloud-sql`

---

**Date Completed**: ___________________
**Deployed By**: ___________________
**Production URL**: ___________________
**Database Instance**: ___________________

🎉 **Congratulations! Your TeacherApp is now running on Google Cloud with persistent storage!**
