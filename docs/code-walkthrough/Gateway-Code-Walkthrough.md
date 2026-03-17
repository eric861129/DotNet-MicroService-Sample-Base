# Gateway 程式碼導讀

## 1. 這個服務在做什麼

Gateway 是整個系統的大門。

外部請求先進 Gateway，  
Gateway 再把請求送到正確的微服務。

## 2. 建議先看哪個檔案

### 第一步
- `src/Gateway/Gateway.Api/Program.cs`

這個檔案就是 Gateway 的總開關。

## 3. 你會在 `Program.cs` 看到什麼

### `AddEnterpriseConfiguration()`
先把設定來源接上。

### `AddEnterpriseObservability("Gateway")`
讓 Gateway 自己也有 log、trace、metrics。

### `AddEnterpriseJwtAuthentication(...)`
把 JWT 驗證接上。

### `AddRateLimiter(...)`
限制單位時間的請求量。

### `AddReverseProxy().LoadFromConfig(...)`
把 YARP 路由規則從設定檔讀進來。

### `MapReverseProxy()`
真正開始做轉發。

## 4. 再看哪個檔案

### `src/Gateway/Gateway.Api/appsettings.json`

這裡會看到：

- 路由規則
- cluster
- destination

### 你可以把它想成
- Route = 入口規則
- Cluster = 要送去哪一組服務
- Destination = 最終實際 URL

## 5. 初學者最容易搞混的事

### Gateway 不處理商業邏輯
它只負責入口層工作。

### Gateway 不應該直接算訂單總金額
那是 Ordering 的責任。

## 6. 你如果要改 Gateway，通常會改哪裡

- 新增一條外部路由：改 `appsettings.json`
- 改限流：改 `Program.cs`
- 改授權：改 `Program.cs` 與安全設定

## 7. 看完這份後建議接著看

- [Catalog-Service-Code-Walkthrough.md](Catalog-Service-Code-Walkthrough.md)
- [Security-Guide.md](../Security-Guide.md)
