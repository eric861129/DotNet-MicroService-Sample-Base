# 功能地圖

這份文件的目的，是讓你快速知道：

- 這個專案有哪些功能
- 每個功能在哪裡
- 你要改哪個檔案

## 1. 功能總覽

| 功能 | 主要位置 | 功能說明 |
| --- | --- | --- |
| 共享核心 | `src/BuildingBlocks/Enterprise.SharedKernel` | 放 Entity、AggregateRoot、DomainException |
| CQRS 與 MediatR | `src/BuildingBlocks/Enterprise.Application` | 放 Command、Query、Pipeline Behaviors |
| 事件匯流 | `src/BuildingBlocks/Enterprise.Messaging` | 放 IntegrationEvent、MassTransit 註冊、Outbox 序列化 |
| 資料持久化 | `src/BuildingBlocks/Enterprise.Persistence` | 放 `ServiceDbContext`、Outbox Dispatcher、Migration Hosted Service |
| 觀測性 | `src/BuildingBlocks/Enterprise.Observability` | 放 Serilog、OpenTelemetry 設定 |
| 設定管理 | `src/BuildingBlocks/Enterprise.Configuration` | 放 Azure App Configuration / Key Vault 掛接 |
| 安全性 | `src/BuildingBlocks/Enterprise.Security` | 放 JWT、CurrentUser、Service Token Provider |
| 服務預設 | `src/BuildingBlocks/Enterprise.ServiceDefaults` | 放全域例外、CORS、Health Checks、Resilience |

## 2. 業務服務地圖

### CatalogService
- Domain: 商品 Entity
- Application: 商品建立與查詢
- Infrastructure: EF Core、Repository、gRPC Server
- Api: REST 與 gRPC 對外入口

### InventoryService
- Domain: 庫存 Entity
- Application: 更新庫存、查可用量
- Infrastructure: EF Core、Repository、OrderPlaced Consumer、gRPC Server
- Api: REST 與 gRPC 對外入口

### OrderingService
- Domain: 訂單 Aggregate
- Application: 下單命令與查詢
- Infrastructure: EF Core、gRPC Client、Outbox 發送
- Api: 下單 REST 與 gRPC

### NotificationService
- Domain: 通知紀錄
- Application: 查詢通知
- Infrastructure: EF Core、OrderPlaced Consumer、gRPC Server
- Api: 通知查詢 API

## 3. 平台服務地圖

### Gateway.Api
- 路由轉發
- 限流
- 統一入口

### AuthService.Api
- OpenIddict
- client credentials token 發放

## 4. 本機開發資產在哪裡

| 功能 | 檔案 |
| --- | --- |
| 本機所有服務編排 | `docker-compose.yml` |
| OTEL Collector | `infra/local/otel-collector-config.yaml` |
| Prometheus | `infra/local/prometheus.yml` |
| Loki | `infra/local/loki-config.yml` |
| Tempo | `infra/local/tempo.yml` |
| Grafana Datasource | `infra/local/grafana/provisioning/datasources/datasources.yml` |

## 5. Azure 與 CI/CD 資產在哪裡

| 功能 | 檔案 |
| --- | --- |
| Azure 主要部署模板 | `infra/bicep/main.bicep` |
| Container App 模組 | `infra/bicep/modules/container-app.bicep` |
| SQL Database 模組 | `infra/bicep/modules/sql-database.bicep` |
| PR 驗證 | `.github/workflows/pr-validation.yml` |
| CD 佈署 | `.github/workflows/cd-aca.yml` |

## 6. 如果你想改某一種功能，要先去哪裡

### 想改 API 路由
先看 `Gateway.Api` 與 [Gateway-Guide.md](Gateway-Guide.md)

### 想改事件流程
先看 `Enterprise.Messaging`、`Enterprise.Persistence` 與 [EventBus-And-Outbox-Guide.md](EventBus-And-Outbox-Guide.md)

### 想改授權
先看 `Enterprise.Security` 與 [Security-Guide.md](Security-Guide.md)

### 想改設定來源
先看 `Enterprise.Configuration` 與 [Configuration-Guide.md](Configuration-Guide.md)

### 想知道下單流程是怎麼走的
先看 [Communication-Guide.md](Communication-Guide.md)
