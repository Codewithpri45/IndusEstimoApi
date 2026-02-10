# Azure App Service Deployment Guide

## 🚀 Prerequisites

1. **Azure Account** - Free account बनाएं: https://azure.microsoft.com/free/
2. **Azure CLI** (Optional) - Install करें: https://aka.ms/installazurecli
3. **GitHub Account** - For automatic deployment

---

## 📋 Deployment Methods

### Method 1: GitHub Actions से Auto Deployment (Recommended)

#### Step 1: Azure App Service बनाएं

1. Azure Portal खोलें: https://portal.azure.com
2. **Create a resource** → **Web App** select करें
3. Details भरें:
   - **Resource Group**: नया बनाएं या existing select करें
   - **Name**: Unique app name (e.g., `indas-estimo-api`)
   - **Publish**: Code
   - **Runtime stack**: .NET 10 (Early Access)
   - **Operating System**: Linux (recommended) या Windows
   - **Region**: Central India या nearest
   - **Pricing Plan**: Free (F1) - Free tier के लिए

4. **Review + Create** → **Create** करें

#### Step 2: Publish Profile Download करें

1. Azure Portal में अपनी App Service खोलें
2. **Overview** → **Get publish profile** button पर click करें
3. File download हो जाएगी (`.publishsettings`)

#### Step 3: GitHub Secrets Setup करें

1. आपकी GitHub repository खोलें
2. **Settings** → **Secrets and variables** → **Actions**
3. **New repository secret** click करें:
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Download किए हुए publish profile file का content paste करें
4. **Add secret** करें

#### Step 4: Workflow File Update करें

`.github/workflows/azure-deploy.yml` file में:
```yaml
env:
  AZURE_WEBAPP_NAME: 'indas-estimo-api'  # अपना app name डालें
```

#### Step 5: Configuration Settings (Important!)

Azure Portal में App Service खोलें:

1. **Configuration** → **Application settings** जाएं
2. **New application setting** click करें और ये add करें:

```
ConnectionStrings__MasterDatabase = Data Source=13.200.122.70,1433;Initial Catalog=Indus;Persist Security Info=True;User ID=Indus;Password=Param@99811;Connection Timeout=3600

JwtSettings__SecretKey = YourSuperSecretKeyWith32CharsMin!
JwtSettings__Issuer = IndusMonarch.Api
JwtSettings__Audience = IndusMonarch.Clients
JwtSettings__AccessTokenExpirationMinutes = 480
JwtSettings__RefreshTokenExpirationDays = 7

EncryptionSettings__AesKey = Your32CharacterAES256EncryptKeyq
EncryptionSettings__KeyDerivationIterations = 10000

Caching__TenantConnectionCacheDurationMinutes = 5

ASPNETCORE_ENVIRONMENT = Production
```

3. **Save** करें

#### Step 6: Deploy करें

1. Code को GitHub पर push करें:
```bash
git add .
git commit -m "Add Azure deployment configuration"
git push origin master
```

2. GitHub Actions automatically deploy कर देगा
3. Progress देखें: GitHub → **Actions** tab

---

### Method 2: VS Code से Direct Deployment

#### Step 1: Azure Extension Install करें

1. VS Code में **Azure App Service** extension install करें
2. Azure account से sign in करें

#### Step 2: Deploy

1. VS Code में project open करें
2. **Azure** icon click करें (left sidebar)
3. **App Services** expand करें
4. Right-click → **Create New Web App**
5. Details fill करें
6. Deploy folder select करें: `src/IndasEstimo.Api`

---

### Method 3: Azure CLI से Deployment

```bash
# Login to Azure
az login

# Create resource group
az group create --name IndusEstimo-RG --location centralindia

# Create App Service Plan (Free tier)
az appservice plan create --name IndusEstimo-Plan --resource-group IndusEstimo-RG --sku F1 --is-linux

# Create Web App
az webapp create --resource-group IndusEstimo-RG --plan IndusEstimo-Plan --name indas-estimo-api --runtime "DOTNET|10.0"

# Configure app settings
az webapp config appsettings set --resource-group IndusEstimo-RG --name indas-estimo-api --settings @appsettings.json

# Deploy from local
cd src/IndasEstimo.Api
dotnet publish -c Release -o ./publish
cd publish
zip -r ../deploy.zip .
az webapp deployment source config-zip --resource-group IndusEstimo-RG --name indas-estimo-api --src ../deploy.zip
```

---

## 🔒 Security Best Practices

### ⚠️ IMPORTANT: Secrets को Secure करें

**appsettings.json** में hardcoded credentials हैं! Production में ये करें:

1. **Local Development के लिए**:
   - `appsettings.Development.json` में secrets रखें
   - `.gitignore` में add करें

2. **Azure Production के लिए**:
   - सभी secrets Azure App Service Configuration में store करें
   - Code में hardcoded values हटाएं या Azure Key Vault use करें

3. **Recommended**: Azure Key Vault use करें:
```bash
az keyvault create --name IndusEstimo-Vault --resource-group IndusEstimo-RG --location centralindia
az keyvault secret set --vault-name IndusEstimo-Vault --name "ConnectionString" --value "your-connection-string"
```

---

## 🌐 Custom Domain Setup (Optional)

1. Azure Portal → App Service → **Custom domains**
2. **Add custom domain** click करें
3. Domain verification करें
4. DNS records update करें

---

## 📊 Monitoring & Logs

### Logs देखने के लिए:

**Azure Portal**:
1. App Service → **Log stream**
2. या **Monitoring** → **Logs**

**VS Code**:
1. Azure Extension → App Service
2. Right-click → **Start Streaming Logs**

**Azure CLI**:
```bash
az webapp log tail --name indas-estimo-api --resource-group IndusEstimo-RG
```

---

## 🐛 Common Issues & Solutions

### Issue 1: Database Connection Failed
- Azure App Service का IP address SQL Server firewall में allow करें
- Connection string check करें Azure Configuration में

### Issue 2: App Not Starting
- Logs check करें (Log stream)
- .NET 10.0 runtime available है verify करें

### Issue 3: 500 Internal Server Error
- Application settings correctly configured हैं check करें
- Environment variables verify करें

---

## 💰 Free Tier Limitations

**Azure App Service Free (F1) Tier**:
- ✅ 1 GB RAM
- ✅ 1 GB Storage
- ✅ 60 minutes/day compute time
- ⚠️ No custom domain SSL
- ⚠️ App sleeps after 20 minutes of inactivity
- ⚠️ No auto-scaling

**Upgrade के लिए**: Basic (B1) tier recommended - ₹1,000-1,500/month

---

## 🎯 Post-Deployment Checklist

- [ ] App successfully deployed
- [ ] Database connection working
- [ ] API endpoints accessible
- [ ] JWT authentication working
- [ ] Swagger UI accessible (if enabled in production)
- [ ] Logs monitoring setup
- [ ] Secrets properly configured
- [ ] SSL certificate active (for custom domain)

---

## 📞 Support

**Azure Documentation**: https://docs.microsoft.com/azure/app-service/

**GitHub Actions Troubleshooting**: Check Actions tab में logs

---

## 🚀 Quick Deploy Command

```bash
# सबसे quick deployment के लिए:
git add .
git commit -m "Deploy to Azure"
git push origin master

# GitHub Actions automatically deploy कर देगा!
```

---

**Note**: Database already deployed है server पर, so बस API को deploy करना है। Make sure Azure App Service का IP address database server के firewall में allow हो!
