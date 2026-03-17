# 設定管理指南

這份文件會說明：

- 設定值從哪裡來
- 本機怎麼改
- 雲端怎麼管

## 1. 設定值有三種類型

### 類型 1：普通設定
例如：

- 服務 URL
- 重試次數
- 功能開關

### 類型 2：敏感設定
例如：

- 密碼
- secret
- token key

### 類型 3：環境差異設定
例如：

- Development 與 Production 的 DB 連線字串不同

## 2. 本機設定從哪裡來

優先會來自：

1. `appsettings.json`
2. `appsettings.Development.json`
3. 環境變數

## 3. 雲端設定從哪裡來

### Azure App Configuration
放非敏感值。

### Azure Key Vault
放敏感值。

## 4. 目前專案是怎麼掛進來的

共用程式在：

- `src/BuildingBlocks/Enterprise.Configuration/EnterpriseConfigurationExtensions.cs`

這個 extension 會：

- 讀 App Configuration Endpoint
- 使用 `DefaultAzureCredential`
- 註冊 refresh sentinel
- 接上 Key Vault

## 5. Sentinel Key 是什麼

它可以想成一個「總開關」。

當這個 key 改變時，  
應用程式就知道要重新整理設定。

目前示範使用：

- `shared:sentinel`

## 6. 如何新增一個設定

### 步驟 1：建立 Options 類別

例如：

```csharp
public sealed class BillingOptions
{
    public const string SectionName = "Billing";
    public int RetryCount { get; init; } = 3;
}
```

### 步驟 2：在 DI 中綁定

```csharp
services.Configure<BillingOptions>(configuration.GetSection(BillingOptions.SectionName));
```

### 步驟 3：在需要的地方注入 `IOptions<T>`

## 7. 什麼要放進 Key Vault

以下內容不要放進 git：

- SQL 密碼
- ACR 密碼
- JWT signing key
- OpenIddict 金鑰
- 第三方 API secret

## 8. 初學者最容易犯的錯

### 把秘密直接寫進 `appsettings.json`
這在正式環境不安全。

### 不分環境共用同一個設定
這會讓測試與正式環境互相影響。

### 改了雲端設定卻不知道 app 為什麼沒更新
要確認 refresh 機制與 sentinel key 是否正確。
