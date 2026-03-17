# Azure 建置與部署指南

這份文件會用比較完整的方式，說明如何把這個專案搬到 Azure。

## 1. 先理解這份文件的目標

本機可以跑起來，不代表就能安全上線。  
上線到 Azure 時，你還要處理：

- 容器映像檔
- Container Registry
- Container Apps
- SQL Database
- Service Bus
- App Configuration
- Key Vault
- CI/CD

## 2. 建議的 Azure 資源清單

- Resource Group
- Azure Container Registry
- Azure Container Apps Environment
- Azure SQL Server
- Azure SQL Databases
- Azure Service Bus Namespace
- Azure App Configuration
- Azure Key Vault
- Log Analytics Workspace

## 3. 這個專案的 IaC 在哪裡

- `infra/bicep/main.bicep`
- `infra/bicep/modules/container-app.bicep`
- `infra/bicep/modules/sql-database.bicep`

## 4. 建立 Resource Group

```powershell
az group create --name rg-enterprise-ms-dev --location eastasia
```

## 5. 建立 ACR

```powershell
az acr create `
  --resource-group rg-enterprise-ms-dev `
  --name myenterpriseacr `
  --sku Standard
```

## 6. 編輯 Bicep 參數

打開：

- `infra/bicep/main.parameters.json`

確認：

- `environmentName`
- `location`
- `containerRegistryServer`
- `containerRegistryUsername`
- `containerRegistryPassword`

## 7. 執行 Bicep 部署

```powershell
az deployment group create `
  --resource-group rg-enterprise-ms-dev `
  --template-file infra/bicep/main.bicep `
  --parameters infra/bicep/main.parameters.json `
  --parameters containerRegistryServer=myenterpriseacr.azurecr.io `
  --parameters containerRegistryUsername=<acr-username> `
  --parameters containerRegistryPassword=<acr-password>
```

## 8. 建議的正式環境調整

目前模板是示範版，正式上線前建議做這些事：

- SQL 密碼改放 Key Vault
- OpenIddict 金鑰改放 Key Vault 憑證
- Service Bus 與其他 Azure 資源改用 Managed Identity
- Gateway 前面加 Front Door / WAF

## 9. EF Core Migration 自動化策略

### 建議做法
每個服務一個 migration bundle。

例如：

```powershell
dotnet ef migrations bundle `
  --project src/Services/Ordering/OrderingService.Infrastructure `
  --startup-project src/Services/Ordering/OrderingService.Api `
  --output artifacts/ordering-migrate.exe
```

### 建議部署順序
1. 先跑 migration job
2. 確認 schema 成功
3. 再佈署新版本服務

## 10. GitHub Actions 在哪裡

- `.github/workflows/pr-validation.yml`
- `.github/workflows/cd-aca.yml`

## 11. 上線前檢查清單

- 所有示範密碼都已替換
- ACR、SQL、Service Bus 權限已收斂
- Key Vault 已接上
- App Configuration 已分環境
- Health Check 與告警規則已建立
- Trace / Log 可以在雲端看得到
