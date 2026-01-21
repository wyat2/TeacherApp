# Custom Domain Setup for TeacherApp
## demo.fabpenguin.com → Cloud Run

### DNS Record to Add

**RECOMMENDED: CNAME Record (Simplest)**

```
Type:  CNAME
Name:  demo
Value: ghs.googlehosted.com
TTL:   3600 (or default)
```

### Alternative: A Records (if CNAME not supported)

```
Type:  A
Name:  demo
Value: 216.239.32.21
TTL:   3600

Type:  A
Name:  demo
Value: 216.239.34.21
TTL:   3600
```

### Optional: AAAA Record (IPv6)

```
Type:  AAAA
Name:  demo
Value: 2001:4860:4802:32::15
TTL:   3600
```

---

## Setup Steps

### 1. Add DNS Record
Go to your DNS provider (where you manage fabpenguin.com) and add the CNAME record above.

### 2. Wait for DNS Propagation
Typically 15 minutes, can take up to 48 hours.

**Check propagation:**
```bash
nslookup demo.fabpenguin.com
```

Or use:
- https://dnschecker.org
- https://www.whatsmydns.net

### 3. Map Domain in Cloud Run

**Option A: Using gcloud CLI**
```bash
gcloud beta run domain-mappings create \
  --service=teacherapp \
  --domain=demo.fabpenguin.com \
  --region=us-central1 \
  --project=fab-penguin
```

**Option B: Using Google Cloud Console**
1. Visit: https://console.cloud.google.com/run/domains/add?project=fab-penguin
2. Select service: `teacherapp`
3. Enter domain: `demo.fabpenguin.com`
4. Click "Continue" and follow verification steps

### 4. Wait for SSL Certificate
Google will automatically provision a free SSL certificate (takes 15-30 minutes).

---

## After Setup Complete

Your app will be accessible at:
- ✅ **https://demo.fabpenguin.com** (custom domain)
- ✅ **https://teacherapp-864267732950.us-central1.run.app** (Cloud Run URL)

Both URLs will work, pointing to the same application!

### Features
- ✅ Free automatic SSL certificate
- ✅ HTTPS enforced automatically
- ✅ No redirect needed
- ✅ Both URLs work simultaneously

---

## Troubleshooting

### DNS not resolving
- Wait longer (DNS propagation can take time)
- Check record syntax in DNS provider
- Verify you added the record to the correct domain

### Certificate not provisioning
- Ensure DNS is properly configured
- Wait up to 30 minutes for certificate
- Check Cloud Run domain mapping status:
  ```bash
  gcloud beta run domain-mappings describe demo.fabpenguin.com --region=us-central1
  ```

### Cannot access site
- Verify domain mapping is complete
- Check SSL certificate status in Cloud Console
- Clear browser cache
- Try in incognito mode

---

## Verification Commands

**Check DNS:**
```bash
nslookup demo.fabpenguin.com
dig demo.fabpenguin.com
```

**Check domain mapping:**
```bash
gcloud beta run domain-mappings list --region=us-central1 --project=fab-penguin
```

**Test HTTPS:**
```bash
curl -I https://demo.fabpenguin.com
```

---

## Summary

| Setting | Value |
|---------|-------|
| Domain | demo.fabpenguin.com |
| DNS Record Type | CNAME |
| DNS Record Value | ghs.googlehosted.com |
| Service | teacherapp |
| Region | us-central1 |
| SSL | Automatic (free) |
| Setup Time | 15-30 minutes |

---

**Date Created:** 2026-01-20  
**Project:** fab-penguin  
**Service:** teacherapp
