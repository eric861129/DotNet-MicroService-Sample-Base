# 測試指南

這份文件說明這個專案的測試策略。

## 1. 為什麼要分三種測試

因為不同層級的測試，解決的是不同問題。

### 單元測試
測一小塊規則對不對。

### 契約測試
測服務之間的協議有沒有跑掉。

### 整合測試
測真實元件一起合作時有沒有問題。

## 2. 目前有哪些測試專案

- `tests/Enterprise.UnitTests`
- `tests/Enterprise.ContractTests`
- `tests/Enterprise.IntegrationTests`

## 3. 單元測試在測什麼

目前示範：

- 訂單總金額計算
- Outbox 事件序列化 / 反序列化

## 4. 契約測試在測什麼

目前示範：

- gRPC Client / Base 是否有正確生成
- Integration Event 預設版本是否正確

## 5. 整合測試在測什麼

目前示範：

- Testcontainers 可否啟動 SQL Server 與 RabbitMQ

這個測試預設不強制啟動容器，  
只有在設定 `RUN_CONTAINER_TESTS=true` 時才真的跑容器。

## 6. 怎麼執行測試

### 全部測試
```powershell
dotnet test EnterpriseMicroservicesBoilerplate.sln
```

### 啟動容器型整合測試
```powershell
$env:RUN_CONTAINER_TESTS = "true"
dotnet test tests/Enterprise.IntegrationTests/Enterprise.IntegrationTests.csproj
```

## 7. 新功能應該補什麼測試

### 如果你新增 Domain 規則
補單元測試。

### 如果你新增 gRPC / Event 契約
補契約測試。

### 如果你新增資料庫或訊息整合
補整合測試。

## 8. 新人常犯的錯

### 只測 Happy Path
也要測失敗情境。

### 只測 API，不測 Domain
這樣會漏掉商業規則。

### 有整合測試就不寫單元測試
整合測試通常比較慢，不能完全取代單元測試。
## 新增的深度整合測試

目前 `tests/Enterprise.IntegrationTests/OrderProcessingEndToEndTests.cs` 已補上核心 E2E 驗證，會檢查：

- 下單後是否真的寫入 Outbox
- Outbox dispatcher 是否會把事件送出去
- Inventory / Notification consumer 是否真的處理事件
- 重複投遞同一筆事件時，Inbox 是否能保證冪等

如果你要在本機執行包含 Testcontainers 的深度測試，請使用：

```powershell
$env:RUN_CONTAINER_TESTS="true"
dotnet test tests/Enterprise.IntegrationTests/Enterprise.IntegrationTests.csproj -c Debug
```
