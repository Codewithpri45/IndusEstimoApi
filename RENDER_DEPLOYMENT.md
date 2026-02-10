# Render.com Deployment Guide 🚀

## ✅ Render के Fayde:
- ✅ **No Credit Card Required** - Free tier के लिए
- ✅ **GitHub से Direct Deploy** - एक click में
- ✅ **750 Hours/Month Free** - हमेशा के लिए
- ✅ **Auto HTTPS/SSL** - Free में
- ✅ **Easy Setup** - 5 minutes में deploy

---

## 📋 Prerequisites

1. **GitHub Account** - Repository public या private हो सकती है
2. **Render Account** - Free में बनाएं: https://render.com

---

## 🚀 Step-by-Step Deployment

### Step 1: Code को GitHub पर Push करें

```bash
# अगर अभी तक push नहीं किया है:
git add .
git commit -m "Add Render deployment configuration"
git push origin master
```

### Step 2: Render Account बनाएं

1. **Visit**: https://render.com
2. **"Get Started"** click करें
3. **"Sign up with GitHub"** select करें
4. GitHub authorization approve करें

**बस! No credit card, no phone verification needed!** ✨

### Step 3: New Web Service बनाएं

1. Render Dashboard में **"New +"** button click करें
2. **"Web Service"** select करें
3. **"Connect a repository"** - आपकी GitHub repository select करें
   - Repository name: `MonarchBackendGit`
   - अगर दिख नहीं रही तो **"Configure account"** से access दें

### Step 4: Configuration करें

Render automatically detect कर लेगा कि Docker project है:

```
Name: indas-estimo-api
Region: Singapore (भारत के लिए सबसे नजदीक)
Branch: master
Runtime: Docker
Docker Context: .
Dockerfile Path: ./Dockerfile

Instance Type: Free
```

**"Advanced"** button click करके:
- **Auto-Deploy**: Yes (recommended)

### Step 5: Environment Variables Add करें

**बहुत Important!** ये variables add करें:

```bash
# Database Connection
ConnectionStrings__MasterDatabase = Data Source=13.200.122.70,1433;Initial Catalog=Indus;Persist Security Info=True;User ID=Indus;Password=Param@99811;Connection Timeout=3600

# JWT Settings
JwtSettings__SecretKey = YourSuperSecretKeyWith32CharsMin!
JwtSettings__Issuer = IndusMonarch.Api
JwtSettings__Audience = IndusMonarch.Clients
JwtSettings__AccessTokenExpirationMinutes = 480
JwtSettings__RefreshTokenExpirationDays = 7

# Encryption Settings
EncryptionSettings__AesKey = Your32CharacterAES256EncryptKeyq
EncryptionSettings__KeyDerivationIterations = 10000

# Caching
Caching__TenantConnectionCacheDurationMinutes = 5

# ASP.NET Core Settings
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://+:8080
```

**Add करने का तरीका:**
1. **"Environment"** section में
2. **"Add Environment Variable"** click करें
3. हर variable के लिए Key और Value डालें

### Step 6: Deploy करें!

1. **"Create Web Service"** button click करें
2. Render automatically build और deploy करेगा
3. **5-10 minutes** लगेंगे पहली बार

**Progress देखें:**
- Logs automatically stream होंगे
- Build status दिखेगा

---

## 🎉 Deployment Success!

Deploy होने के बाद आपको मिलेगा:

```
Your app is live at: https://indas-estimo-api.onrender.com
```

**Test करें:**
```bash
# Health check
curl https://indas-estimo-api.onrender.com/api/health

# Swagger (if enabled)
https://indas-estimo-api.onrender.com/swagger
```

---

## 🔧 Important: Database Firewall Configuration

**बहुत जरूरी!** आपके SQL Server firewall में Render के IP addresses allow करने होंगे:

### Render के IP Ranges (Singapore Region):

Render dynamic IPs use करता है, so:

**Option 1: Specific IP Range Allow करें**
```
Render Singapore Region IPs:
- Check करें: https://render.com/docs/static-outbound-ip-addresses
```

**Option 2: SQL Server Firewall में Add करें**

आपके SQL Server admin से कहें:
```sql
-- Render IPs allow करने के लिए
-- या temporarily test के लिए:
-- 0.0.0.0 - 255.255.255.255 (Not recommended for production)
```

**Better Approach**: Use Azure SQL or managed database जो dynamic IPs allow करे

---

## ⚙️ render.yaml File (Already Created!)

File already है project में: [render.yaml](render.yaml)

ये file automatically configuration करती है। अगर manually deploy कर रहे हैं तो ye file optional है।

---

## 🔄 Auto-Deploy Setup

**GitHub पर code push करते ही automatically deploy होगा!**

```bash
# Changes करें
git add .
git commit -m "Update API"
git push origin master

# Render automatically deploy करेगा!
```

**Disable करने के लिए:**
- Render Dashboard → Settings → Auto-Deploy: Off

---

## 📊 Monitoring & Logs

### Live Logs देखें:

1. Render Dashboard → आपकी service
2. **"Logs"** tab
3. Real-time logs stream होंगे

### Metrics देखें:

1. **"Metrics"** tab में:
   - CPU usage
   - Memory usage
   - Response times
   - Request count

---

## 💰 Free Tier Limitations

**Render Free Tier:**
- ✅ 750 hours/month (पूरा महीना चल सकता है)
- ✅ 512 MB RAM
- ✅ Auto HTTPS/SSL
- ✅ Custom domains allowed
- ⚠️ **Sleeps after 15 minutes of inactivity**
- ⚠️ Cold start: 30-60 seconds (जब sleep से wake up हो)
- ⚠️ 100 GB bandwidth/month

**Sleep Problem का Solution:**
- Paid plan ($7/month) - No sleep
- या free external monitoring service use करें जो हर 10 mins में ping करे

---

## 🌐 Custom Domain Setup (Optional)

**Free tier में भी custom domain use कर सकते हैं!**

1. Render Dashboard → Settings → Custom Domains
2. **Add Custom Domain** click करें
3. Domain name डालें (e.g., `api.yourdomain.com`)
4. DNS records update करें:
   ```
   Type: CNAME
   Name: api
   Value: indas-estimo-api.onrender.com
   ```
5. SSL automatically enable होगा

---

## 🐛 Common Issues & Solutions

### Issue 1: Build Failed
**Solution:**
```bash
# Logs check करें
# Usually .NET 10.0 runtime issue होता है
# Dockerfile में .NET version verify करें
```

### Issue 2: Database Connection Failed
**Solution:**
- SQL Server firewall में Render IPs allow करें
- Connection string verify करें environment variables में
- Test करें local से उसी connection string से

### Issue 3: Application Error
**Solution:**
```bash
# Logs देखें Render dashboard में
# Environment variables check करें
# appsettings.json vs Environment variables priority check करें
```

### Issue 4: Slow Response (Cold Start)
**Solution:**
- Free tier में normal है
- 15 mins inactivity के बाद sleep होता है
- Paid plan upgrade करें ($7/month) - No sleep
- या UptimeRobot जैसी service से ping करते रहें

---

## 🚀 Post-Deployment Checklist

- [ ] App successfully deployed
- [ ] Database connection working (SQL Server firewall configured)
- [ ] Environment variables properly set
- [ ] API endpoints accessible
- [ ] JWT authentication working
- [ ] Swagger accessible (if enabled)
- [ ] Logs monitoring setup
- [ ] Custom domain configured (optional)
- [ ] Auto-deploy enabled from GitHub

---

## 🔒 Security Best Practices

### 1. Environment Variables
✅ Database credentials Render environment में store करें
✅ Git में commit न करें

### 2. SQL Server Security
⚠️ Current setup में database password hardcoded है
✅ Production में Azure Key Vault या Secret Manager use करें

### 3. HTTPS
✅ Render automatically HTTPS provide करता है
✅ Force HTTPS enabled रखें

---

## 📈 Upgrade Options

**अगर free tier काफी नहीं है:**

**Starter Plan: $7/month**
- No sleep
- 512 MB RAM
- Always on
- Better for production

**Standard Plan: $25/month**
- 2 GB RAM
- High availability
- Priority support

---

## 🎯 Quick Commands

### Deploy करने के लिए:
```bash
git add .
git commit -m "Deploy to Render"
git push origin master
```

### Logs देखने के लिए:
```bash
# Render CLI install करें (optional):
npm install -g render-cli

# Login:
render login

# Logs:
render logs -s indas-estimo-api
```

---

## 🆚 Render vs Azure Comparison

| Feature | Render Free | Azure Free |
|---------|-------------|------------|
| Signup | No card needed | Credit card required |
| Setup Time | 5 minutes | 15-20 minutes |
| Deployment | GitHub one-click | Multiple steps |
| Sleep | After 15 mins | After 20 mins |
| Monthly Hours | 750 hrs | 60 mins/day |
| Database | External only | Azure SQL free tier |
| Best For | Quick deploy | Enterprise apps |

**Recommendation**: Render बेहतर है quick deployment और testing के लिए!

---

## 📞 Support & Resources

- **Render Docs**: https://render.com/docs
- **Community**: https://community.render.com
- **Status**: https://status.render.com

---

## 🎊 Congratulations!

आपका .NET API Render पर live है! 🚀

**Test करें:**
```bash
curl https://indas-estimo-api.onrender.com/swagger/index.html
```

**अगर कोई problem हो तो मुझे बताएं!** 💪

---

## 🔥 Pro Tips

1. **Environment Groups**: Multiple services के लिए common env variables
2. **Health Checks**: Render automatically `/health` endpoint ping करता है
3. **Blueprints**: Infrastructure as Code के लिए `render.yaml` use करें
4. **Preview Environments**: PR के लिए automatic preview deployments
5. **Background Workers**: Cron jobs और background tasks के लिए

**Happy Deploying! 🎉**
