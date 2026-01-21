# TeacherApp - Google Cloud SQL & Cloud Run Setup Complete

## What Has Been Configured

Your TeacherApp is now ready to deploy to Google Cloud Run with persistent PostgreSQL database storage using Cloud SQL.

### ✅ Files Created/Updated

1. **cloudsql-setup.md** - Complete step-by-step guide for Cloud SQL setup
2. **CLOUD_DEPLOYMENT.md** - Quick start deployment guide
3. **deploy-cloud-run.ps1** - PowerShell deployment script (Windows)
4. **deploy-cloud-run.sh** - Bash deployment script (Linux/Mac)
5. **cloudbuild.yaml** - Automated CI/CD configuration
6. **.gcloudignore** - Excludes unnecessary files from deployment
7. **appsettings.Production.json** - Production configuration
8. **README.md** - Updated with cloud deployment information

### 📊 Application Database Architecture

**Current Setup:**
- Database Type: PostgreSQL (via Npgsql)
- ORM: Entity Framework Core 8.0
- Context: ApplicationDbContext
- Migrations: Automatic on startup (Program.cs line 153)

**Tables:**
- Students, StudentClasses, TutoringNotes
- Homework, Tests, Contacts
- AspNetUsers, AspNetRoles (Identity)
- AuditLogs, RolePermissions, DirectorsPageInfos
- DropdownValues

### 🚀 Deployment Steps

#### Option 1: Quick Deploy (Recommended)

1. **Install gcloud CLI** (if not installed):
   - Windows: https://cloud.google.com/sdk/docs/install
   - Run: `gcloud init`

2. **Create Cloud SQL Database** (one-time):
   ```powershell
   gcloud sql instances create teacherapp-db `
     --database-version=POSTGRES_15 `
     --tier=db-f1-micro `
     --region=us-central1
   
   gcloud sql databases create teacherapp --instance=teacherapp-db
   
   gcloud sql users create teacherapp-user `
     --instance=teacherapp-db `
     --password=YOUR_SECURE_PASSWORD
   ```

3. **Deploy Application**:
   ```powershell
   # Edit deploy-cloud-run.ps1 and update PROJECT_ID
   .\deploy-cloud-run.ps1
   ```

#### Option 2: Manual Deploy

See `CLOUD_DEPLOYMENT.md` for detailed manual deployment steps.

### 🔗 Connection String Format

**For Cloud Run (Unix Socket):**
```
Host=/cloudsql/PROJECT_ID:REGION:INSTANCE_NAME;Database=teacherapp;Username=teacherapp-user;Password=YOUR_PASSWORD
```

**For Local Development (TCP):**
```
Host=localhost;Port=5432;Database=teacherapp;Username=teacherapp;Password=YOUR_PASSWORD
```

### 🔒 Security Features

- ✅ Unix socket connection (more secure than TCP)
- ✅ Private connection between Cloud Run and Cloud SQL
- ✅ No public IP exposure required
- ✅ FERPA-compliant password policies
- ✅ Audit logging enabled
- ✅ Automatic HTTPS

### 💰 Cost Estimate

Monthly costs (approximate):
- **Cloud SQL (db-f1-micro)**: $7-15
- **10GB SSD Storage**: $1.70
- **Cloud Run**: $0 when idle, ~$0.000024 per request
- **Network Egress**: Variable

**Total**: ~$10-20/month for light usage

### 📈 Auto-Scaling Configuration

- **Min instances**: 0 (scales to zero when idle)
- **Max instances**: 10
- **Memory**: 512Mi per instance
- **CPU**: 1 vCPU
- **Timeout**: 300 seconds

### 🔍 Monitoring & Logging

**View Application Logs:**
```powershell
gcloud logging read "resource.type=cloud_run_revision AND resource.labels.service_name=teacherapp" --limit 50
```

**Check Database Status:**
```powershell
gcloud sql operations list --instance=teacherapp-db
```

**View Service Details:**
```powershell
gcloud run services describe teacherapp --region=us-central1
```

### 🛠️ Post-Deployment Tasks

1. **Verify Database Connection**:
   - Visit your Cloud Run URL
   - Complete the Setup Wizard
   - Create admin account

2. **Optional: Populate Test Data**:
   ```powershell
   # Set environment variable and redeploy
   gcloud run services update teacherapp --set-env-vars "SEED_USERS=true,SEED_STUDENTS=true"
   ```

3. **Enable Backups** (Recommended):
   ```powershell
   gcloud sql instances patch teacherapp-db --backup-start-time=03:00
   ```

4. **Set up Monitoring**:
   - Go to Google Cloud Console
   - Navigate to Cloud Run > teacherapp
   - Set up alerts for errors/latency

### 🔄 Updating the Application

After making code changes:

```powershell
# Build and deploy
docker build -t gcr.io/YOUR_PROJECT_ID/teacherapp:latest .
docker push gcr.io/YOUR_PROJECT_ID/teacherapp:latest

# Update Cloud Run
gcloud run deploy teacherapp --image gcr.io/YOUR_PROJECT_ID/teacherapp:latest --region us-central1
```

Or use the automated script:
```powershell
.\deploy-cloud-run.ps1
```

### 📚 Additional Resources

- **Cloud SQL Documentation**: https://cloud.google.com/sql/docs
- **Cloud Run Documentation**: https://cloud.google.com/run/docs
- **Entity Framework Core**: https://learn.microsoft.com/en-us/ef/core/

### ⚠️ Important Notes

1. **Never commit passwords** to source control
2. For production, use **Secret Manager** for passwords (see CLOUD_DEPLOYMENT.md)
3. Database migrations run **automatically** on startup
4. The app creates required roles on first run
5. Setup wizard runs on first access

### 🆘 Troubleshooting

**Connection Issues:**
- Verify Cloud SQL instance is running
- Check connection string format
- Ensure Cloud Run has Cloud SQL instance added

**Migration Failures:**
- Check logs: `gcloud logging read`
- Verify database user has necessary permissions
- Run migrations manually if needed

**Performance Issues:**
- Increase memory/CPU allocation
- Check database query performance
- Review audit logs for bottlenecks

### ✅ Next Steps

1. Run `.\deploy-cloud-run.ps1` to deploy
2. Visit the Cloud Run URL provided after deployment
3. Complete the Setup Wizard
4. Start managing your TRIO program students!

---

**Your application is now ready for persistent, scalable cloud deployment! 🎉**
