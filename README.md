# Enterprise Microservices Boilerplate

這個專案是一套「可以拿來當企業級微服務起點」的範本。

它做的事情不是只放幾個空專案給你，而是把一套實際會在公司裡用到的基礎能力先準備好，包含：

- 微服務分層架構
- API Gateway
- gRPC 與 REST
- Event Bus
- Outbox Pattern
- OpenTelemetry 與 Serilog
- OpenIddict 內部授權
- docker-compose 本機環境
- Azure Container Apps 與 Bicep
- GitHub Actions CI/CD

目前程式碼使用 `net9.0`，原因是這台開發環境只有 `.NET SDK 9.0.310`。  
整體結構已經按照 `.NET 10` 升級路徑整理好，之後升版時不需要重做整個架構。

## 先看哪裡

如果你是第一次接觸這個專案，建議照這個順序閱讀：

1. [文件導覽地圖](docs/Docs-Index.md)
2. [一步一步快速開始](docs/Quick-Start-Step-By-Step.md)
3. [系統架構藍圖](docs/Architecture-Blueprint.md)
4. [開發者手冊](docs/Developer-Handbook.md)
5. [Skills 選用索引](docs/skills-index.md)

如果你只想找某一個主題，可以直接看這些文件：

- [功能地圖](docs/Feature-Map.md)
- [程式碼導讀總覽](docs/Code-Walkthrough-Index.md)
- [本機開發指南](docs/Local-Development-Guide.md)
- [設定管理指南](docs/Configuration-Guide.md)
- [Gateway 指南](docs/Gateway-Guide.md)
- [事件匯流與 Outbox 指南](docs/EventBus-And-Outbox-Guide.md)
- [安全性指南](docs/Security-Guide.md)
- [觀測性指南](docs/Observability-Guide.md)
- [測試指南](docs/Testing-Guide.md)
- [Azure 建置與部署指南](docs/Setup-Guide.md)
- [常見問題與除錯手冊](docs/Troubleshooting.md)

## 專案目錄

```text
src/
  BuildingBlocks/
  Gateway/
  Identity/
  Services/
tests/
infra/
docs/
```

## 主角有哪些

- `Gateway.Api`
  - 專案入口的總大門
  - 負責路由、限流、統一入口
- `AuthService.Api`
  - 負責發內部服務使用的 token
- `CatalogService`
  - 管理商品資料
- `InventoryService`
  - 管理庫存資料
- `OrderingService`
  - 負責下單
  - 也是 Outbox Pattern 的主要示範服務
- `NotificationService`
  - 負責接收事件並記錄通知
- `src/BuildingBlocks`
  - 放跨服務可共用的基礎能力

## 本機執行入口

- Gateway: `http://localhost:8080`
- Auth Service: `http://localhost:8085`
- Catalog API / gRPC: `http://localhost:7201`
- Inventory API / gRPC: `http://localhost:7202`
- Ordering API / gRPC: `http://localhost:7203`
- Notification API / gRPC: `http://localhost:7204`
- RabbitMQ 管理畫面: `http://localhost:15672`
- Grafana: `http://localhost:3000`
- Prometheus: `http://localhost:9090`

## 最短啟動流程

1. 啟動相依服務

```powershell
docker compose up -d rabbitmq catalog-db ordering-db inventory-db notification-db auth-db otel-collector prometheus loki tempo grafana
```

2. 建置與測試

```powershell
dotnet restore EnterpriseMicroservicesBoilerplate.sln
dotnet build EnterpriseMicroservicesBoilerplate.sln
dotnet test EnterpriseMicroservicesBoilerplate.sln
```

3. 啟動整套服務

```powershell
docker compose up --build
```

## 這份文件的設計方式

這套文件刻意分成兩條閱讀路線：

- 初學者路線
  - 先學怎麼跑起來
  - 再學怎麼新增服務
  - 最後才學架構原理
- 進階工程師路線
  - 直接查特定功能文件
  - 快速定位到安全、事件、觀測性或部署主題

如果你的目標是讓團隊新人也看得懂，請從 [文件導覽地圖](docs/Docs-Index.md) 開始。

## 新增的快速入口
- [自動化腳本指南](docs/Automation-Guide.md)
- [腳手架指南](docs/Scaffolding-Guide.md)
- [第一條黃金路徑教學](docs/tutorials/First-Order-Tutorial.md)
- [Runbooks 索引](docs/runbooks/Runbooks-Index.md)
- [Dev Container / Codespaces 指南](docs/Devcontainer-Guide.md)
- [角色導覽](docs/Persona-Guide.md)
- [ADR 與架構決策索引](docs/adr/ADR-Index.md)
- [GitHub Pages 文件站部署指南](docs/GitHub-Pages-Docs-Sites-Guide.md)
