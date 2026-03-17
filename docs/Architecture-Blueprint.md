# 系統架構藍圖

這份文件會從大方向到細節，說明整個專案的設計。

如果你是第一次學微服務，請先把這份文件當成「地圖」。  
你不需要一次全部記住，但你需要知道每一塊在做什麼。

## 1. 這個系統想解決什麼問題

企業級微服務最常遇到的問題，不是「怎麼切專案」而已。  
真正麻煩的是下面這些事情：

- 服務之間怎麼講話
- 失敗時怎麼重試
- 多個服務都要更新資料時怎麼避免亂掉
- 怎麼知道是哪個服務出錯
- 怎麼把設定集中管理
- 怎麼安全地發 token

這個 boilerplate 的目的，就是先把這些共同問題整理好。

## 2. 架構分層

每個微服務都固定切成四層：

### Domain
負責商業規則。

這一層只關心：

- 這件事情合不合理
- 這個狀態能不能改
- 這個規則有沒有被破壞

### Application
負責用例流程。

這一層只關心：

- 收到一個命令後，應該做哪些步驟
- 需要查哪些資料
- 要呼叫哪些外部服務
- 要不要發事件

### Infrastructure
負責接外面的世界。

例如：

- EF Core
- SQL Server
- MassTransit
- gRPC Client / Server
- Repository

### Api
負責最外層入口。

例如：

- REST API
- OpenAPI
- Middleware
- DI 註冊
- Health Check

## 3. 現在有哪些服務

### CatalogService
負責商品主檔。

### InventoryService
負責庫存資料與可用量。

### OrderingService
負責下單流程，是範例裡最核心的服務。

### NotificationService
負責通知紀錄。

### Gateway.Api
所有外部流量的統一入口。

### AuthService.Api
專門發內部服務用 token。

## 4. 服務之間怎麼通訊

這個專案把通訊分成兩種：

### 同步通訊
意思是：我現在問你，你要現在回我。

這裡使用：

- 對外：REST
- 對內：gRPC

### 非同步通訊
意思是：我先把訊息送出去，你之後再處理。

這裡使用：

- MassTransit
- RabbitMQ（本機）
- Azure Service Bus（雲端）

## 5. 為什麼對外用 REST、對內用 gRPC

### 對外用 REST
因為：

- 容易被前端、Postman、Swagger 使用
- 團隊普遍熟悉
- 比較適合公開 API

### 對內用 gRPC
因為：

- 速度快
- 型別明確
- 契約清楚
- 很適合 service-to-service 呼叫

## 6. 下單流程怎麼走

下單是這套範本最重要的示範。

流程如下：

1. 使用者呼叫 `OrderingService`
2. `OrderingService` 同步呼叫 `CatalogService`
3. `OrderingService` 同步呼叫 `InventoryService`
4. 驗證通過後建立訂單
5. 把 `OrderPlacedIntegrationEvent` 寫進 Outbox
6. 背景服務把 Outbox 事件送到 Event Bus
7. `InventoryService` 消費事件並保留庫存
8. `NotificationService` 消費事件並建立通知

## 7. 為什麼要用 Outbox Pattern

先想像一個問題：

如果 `OrderingService` 已經把訂單存進資料庫了，  
但剛好 RabbitMQ 壞掉，事件送不出去，會怎樣？

結果就是：

- 訂單有了
- 但是庫存沒保留
- 通知也沒發

這就會亂掉。

Outbox Pattern 的做法是：

1. 先把訂單和事件都一起存進資料庫
2. 之後由背景工作慢慢把事件送出去

這樣就算訊息代理暫時壞掉，也不會讓交易直接不一致。

## 8. 為什麼消費端還要做 Inbox

訊息系統常見狀況是「至少一次投遞」。

意思是同一則訊息可能會送兩次。

所以消費端要記錄：

- 這個事件我處理過了嗎

如果處理過，就跳過。

這就是 `InboxMessages` 的用途。

## 9. API Gateway 做什麼

Gateway 不只是轉發而已，它也是整個系統的大門。

目前它負責：

- 統一路由
- 流量入口
- Rate Limiting
- 集中化管理外部請求

之後如果團隊需要，也可以繼續放：

- 統一授權 policy
- Request/Response 轉換
- API 版本策略

## 10. Service Discovery 怎麼處理

### 本機
用 docker-compose 的 service name。

例如：

- `catalog-service-api`
- `inventory-service-api`

### Azure Container Apps
使用 Container Apps 內部網路與服務名稱。

## 11. 韌性策略怎麼做

這個專案在共用層接了 `Microsoft.Extensions.Http.Resilience`。

實際上你可以把它理解成：

- 失敗時先重試
- 重試太多次就暫時熔斷
- 等一段時間後再恢復

建議原則：

- 查詢類呼叫可以比較積極重試
- 命令類呼叫要小心避免重覆副作用

## 12. 設定管理怎麼做

### 本機
- `appsettings.json`
- `appsettings.Development.json`
- 環境變數

### 雲端
- Azure App Configuration
- Azure Key Vault

原則是：

- 非敏感值放 App Configuration
- 敏感值放 Key Vault

## 13. 安全模型怎麼做

### 對外
可以接 Microsoft Entra ID。

### 對內
透過 OpenIddict 發 `client_credentials` token。

### 對 Azure 資源
正式環境應優先使用 Managed Identity。

## 14. 可觀測性怎麼做

### Log
用 Serilog

### Trace / Metrics
用 OpenTelemetry

### 本機視覺化
- Tempo 看 Trace
- Loki 看 Log
- Prometheus 看 Metrics
- Grafana 看儀表板

## 15. 這份藍圖最重要的三件事

如果你只記得三件事，請記這三個：

1. 同步查詢用 gRPC，跨服務狀態推進用事件
2. 交易一致性靠 Outbox + Inbox
3. 所有共通能力盡量收斂到 BuildingBlocks
