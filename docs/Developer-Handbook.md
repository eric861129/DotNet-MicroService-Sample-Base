# 開發者手冊

這份手冊是給要修改、擴充、維護這個專案的工程師使用的。

如果你想：

- 新增一個服務
- 新增一個事件
- 新增一個設定
- 新增一個 gRPC 契約
- 新增一個資料表

請從這份文件開始。

## 1. 先記住這個專案的規矩

### 規矩 1：每個服務都要有清楚分層
- Domain
- Application
- Infrastructure
- Api

### 規矩 2：不要跨服務共用資料庫

### 規矩 3：跨服務狀態推進優先用事件

### 規矩 4：共通能力優先放 BuildingBlocks

## 2. 如何新增一個 Service

下面用 `BillingService` 當例子。

### 步驟 1：建立資料夾
在 `src/Services` 下建立：

- `Billing/BillingService.Domain`
- `Billing/BillingService.Application`
- `Billing/BillingService.Infrastructure`
- `Billing/BillingService.Api`
- `Billing/BillingService.Contracts`

### 步驟 2：設定專案參考

#### Domain
只參考：

- `Enterprise.SharedKernel`

#### Application
參考：

- `Enterprise.Application`
- `BillingService.Domain`

#### Infrastructure
參考：

- `Enterprise.Persistence`
- `Enterprise.Messaging`
- `BillingService.Application`
- `BillingService.Domain`
- `BillingService.Contracts`

#### Api
參考：

- `Enterprise.Configuration`
- `Enterprise.Observability`
- `Enterprise.Security`
- `Enterprise.ServiceDefaults`
- `BillingService.Application`
- `BillingService.Infrastructure`
- `BillingService.Contracts`

### 步驟 3：在 Domain 建立 Aggregate

原則：

- 商業規則留在 Domain
- 不要把 EF 或 HTTP 細節放進來

### 步驟 4：在 Application 建立 Command / Query

原則：

- 寫用例流程
- 使用 MediatR
- 搭配 FluentValidation

### 步驟 5：在 Infrastructure 實作 Repository、DbContext、外部呼叫

### 步驟 6：在 Api 的 `Program.cs` 註冊

至少要有：

```csharp
builder.Services.AddEnterpriseServiceDefaults(builder.Configuration);
builder.Services.AddEnterpriseJwtAuthentication(builder.Configuration);
builder.Services.AddEnterpriseApplication(typeof(SomeCommand).Assembly);
builder.Services.AddBillingInfrastructure(builder.Configuration);
builder.Services.AddGrpc();
builder.Services.AddOpenApi();
```

## 3. 如何新增一個 Event

### 步驟 1：決定事件名稱

事件名稱要描述「已經發生」的事。  
例如：

- `PaymentSucceeded`
- `OrderCancelled`

不要用：

- `DoPayment`
- `CreateOrder`

因為那是命令，不是事件。

### 步驟 2：在 Contracts 建立事件型別

繼承：

- `IntegrationEvent`

### 步驟 3：在發送端寫入 Outbox

不要直接 Publish，應該：

```csharp
await outboxStore.AddAsync(new SomeIntegrationEvent { ... });
```

### 步驟 4：在接收端建立 Consumer

Consumer 要做兩件事：

1. 檢查 Inbox
2. 執行真正處理邏輯

### 步驟 5：補測試

- 事件契約測試
- 消費邏輯測試
- 整合測試

## 4. 如何新增 gRPC 契約

### 步驟 1：在 `Contracts/Protos` 新增 `.proto`

### 步驟 2：更新 `csproj`

例如：

```xml
<Protobuf Include="Protos\billing.proto" GrpcServices="Both" />
```

### 步驟 3：在 Infrastructure 建立 gRPC Server 或 Client Adapter

### 步驟 4：在 Application 只依賴抽象介面

這樣可以避免 Application 直接綁死 gRPC 細節。

## 5. 如何新增 Config

### 步驟 1：建立 Options 類別

### 步驟 2：在 DI 註冊 `Configure<TOptions>`

### 步驟 3：決定是放 App Configuration 還是 Key Vault

### 判斷原則
- 不敏感：App Configuration
- 敏感：Key Vault

## 6. 如何新增資料表與 Migration

### 步驟 1：在 `DbContext` 新增 `DbSet<T>`

### 步驟 2：在 `ConfigureDomain` 補模型設定

### 步驟 3：建立 migration

```powershell
dotnet ef migrations add AddBillingTables `
  --project src/Services/Billing/BillingService.Infrastructure `
  --startup-project src/Services/Billing/BillingService.Api
```

### 步驟 4：更新部署流程

如果這個服務要上雲端，記得同步補：

- migration bundle
- ACA job
- CI/CD

## 7. 如何擴充觀測性

### 要記錄業務 log
使用結構化 log。

### 要新增 Trace
建立 Activity。

### 要新增 Metrics
使用 `Meter`、`Counter` 或 `Histogram`。

## 8. 如何擴充安全性

### 外部 API
通常由 Gateway 驗證 token。

### 內部服務
使用 Auth Service 發 token。

### Azure 資源
優先改用 Managed Identity。

## 9. 開發者自我檢查表

送出 PR 前，請至少檢查：

- 有沒有破壞分層
- 有沒有新增對應測試
- 有沒有把秘密放進 repo
- 有沒有補文件
- 有沒有考慮事件重複處理

## 10. 最後記住一句話

這個 boilerplate 最重要的不是「把功能塞進去」，  
而是「用一致的方式把功能放進去」。
