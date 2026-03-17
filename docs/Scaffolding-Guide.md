# 腳手架指南

這份文件教你怎麼用腳手架快速新增：
- 一個新的 Service
- 一個新的 Integration Event

如果你不想手動建立十幾個資料夾與專案檔，先從這裡開始。

## 1. 新增一個 Service

### PowerShell
```powershell
.\scripts\new-service.ps1 -Name Billing
```

### bash
```bash
bash scripts/new-service.sh -Name Billing
```

## 2. 這個腳本會幫你建立什麼

以 `Billing` 為例，腳本會建立：

```text
src/Services/Billing/
  BillingService.Domain/
  BillingService.Application/
  BillingService.Infrastructure/
  BillingService.Api/
  BillingService.Contracts/
```

而且每個專案都會帶上最基本的可編譯骨架：
- `Domain` 會有一個示範 Aggregate Root
- `Application` 會有 `CreateCommand`、`ListQuery`、Repository 介面
- `Infrastructure` 會有 `DbContext`、Repository、gRPC service、DI 註冊
- `Api` 會有 `Program.cs`、`Dockerfile`、`appsettings.json`、`.http`
- `Contracts` 會有 `.proto`

## 3. 建完之後你還要做什麼

腳手架不是魔法，它先幫你鋪好路，真正的業務語意還是要你來填。

建完新的服務後，通常還要補這些：

1. 在 `docker-compose.yml` 加上新的 API 容器與資料庫容器
2. 在 `Gateway` 加上 route 與 cluster
3. 把示範 `Record` 替換成真實的 Domain Model
4. 依需要加入事件、Consumer、Outbox 發送流程
5. 建立第一個 EF Core migration

## 4. 新增一個 Event

### PowerShell
```powershell
.\scripts\new-event.ps1 `
  -EventName PaymentCompleted `
  -ContractsProject src/Services/Ordering/OrderingService.Contracts/OrderingService.Contracts.csproj `
  -ConsumerProject src/Services/Notification/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj
```

### bash
```bash
bash scripts/new-event.sh \
  -EventName PaymentCompleted \
  -ContractsProject src/Services/Ordering/OrderingService.Contracts/OrderingService.Contracts.csproj \
  -ConsumerProject src/Services/Notification/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj
```

## 5. `new-event` 會幫你做什麼

- 在 Contracts 專案建立 `PaymentCompletedIntegrationEvent.cs`
- 如果你有傳 `ConsumerProject`，會再建立一個 Consumer 範例檔

## 6. 腳手架的設計原則

- 先讓專案可編譯
- 先讓開發者有閱讀路徑
- 先讓目錄與依賴規則正確

也就是說，它先幫你處理「結構」，再讓你專心處理「業務」。

## 7. 最佳搭配

建議把這幾份文件一起看：

1. [Developer-Handbook.md](Developer-Handbook.md)
2. [Code-Walkthrough-Index.md](Code-Walkthrough-Index.md)
3. [EventBus-And-Outbox-Guide.md](EventBus-And-Outbox-Guide.md)
