# 安全性指南

這份文件要說明這個 boilerplate 的安全模型。

## 1. 先講最簡單的版本

這個系統有兩種身分：

- 外部使用者
- 內部服務

兩者不應該使用同一套授權方式。

## 2. 對外流量怎麼驗證

正式設計方向是：

- 使用 Microsoft Entra ID
- Gateway 驗證 token

目前程式碼已經預留：

- `Security:Jwt:Authority`
- `Security:Jwt:Audience`

## 3. 對內流量怎麼驗證

內部服務之間，使用：

- `OpenIddict`
- `client_credentials`

也就是說：

- 每個服務像一個機器帳號
- 先向 Auth Service 拿 token
- 再帶 token 呼叫其他服務

## 4. Auth Service 在哪裡

- `src/Identity/AuthService.Api`

主要檔案：

- `Program.cs`
- `AuthDbContext.cs`
- `OpenIddictSeeder.cs`

## 5. 為什麼還要提 Managed Identity

因為內部服務除了互相呼叫，  
還可能存取 Azure 資源，例如：

- Key Vault
- App Configuration
- Service Bus

這時候最好的方式通常不是再塞一組帳密，  
而是使用 Managed Identity。

## 6. 目前示範與正式環境的差別

### 示範版
- 使用簡單的 appsettings 與 seed client
- 使用 ephemeral signing key

### 正式版
- 應改成 Key Vault 管理 signing / encryption key
- 應使用 Entra ID 整合對外流量
- 應使用 Managed Identity 存取 Azure 資源

## 7. 你應該保護哪些東西

- API 入口
- 內部服務 token
- 資料庫連線字串
- ACR 帳密
- OpenIddict signing key
- App Configuration / Key Vault 權限

## 8. 開發者最容易忽略的事

### 只保護外部 API，沒保護 service-to-service
這是不夠的。

### 把秘密放進 repo
這是高風險。

### 把 Gateway 當成唯一安全邊界
內部服務本身也應該有授權驗證能力。
