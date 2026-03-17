# DotNet MicroService Sample Base Lite

這個 repo 是 [`DotNet-MicroService-Sample-Full`](https://github.com/eric861129/DotNet-MicroService-Sample-Full-) 的精簡導入版。

它刻意只保留專案初期最常需要的骨架：

- `Gateway.Api`：單一入口，使用 YARP 代理到業務服務
- `AuthService.Api`：輕量 `client_credentials` JWT issuer
- `CatalogService.Api`：商品主資料 API
- `OrderingService.Api`：同步呼叫 Catalog 後建立訂單
- 2 個 SQL Server：Catalog DB、Ordering DB

這個版本不包含：

- Event Bus / Outbox / Inbox
- gRPC / proto contracts
- OpenTelemetry / Grafana / Loki / Tempo
- OpenIddict / Auth DB
- Azure ACA / Bicep / GitHub Pages docs site
- Inventory / Notification 示範服務

## 快速開始

1. 檢查前置工具

```powershell
./scripts/check-prereqs.ps1
```

2. 啟動本機環境

```powershell
./scripts/dev-up.ps1
```

3. 取得 token

```powershell
$token = (Invoke-RestMethod -Method Post -Uri http://localhost:8085/connect/token -ContentType 'application/x-www-form-urlencoded' -Body 'grant_type=client_credentials&client_id=gateway-client&client_secret=gateway-secret&scope=catalog.read ordering.write').access_token
```

4. 經 Gateway 建立商品

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:8080/catalog/products -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json' -Body '{"sku":"SKU-001","name":"Base Lite Product","price":1280}'
```

5. 經 Gateway 建立訂單

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:8080/ordering/orders -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json' -Body '{"customerEmail":"student@example.com","items":[{"productId":"<PRODUCT_ID>","quantity":1}]}'
```

## Repo 結構

```text
src/
  BuildingBlocks/
    Enterprise.Security/
    Enterprise.ServiceDefaults/
  Gateway/
  Identity/
  Services/
    Catalog/
    Ordering/
tests/
docs/
scripts/
```

## 文件

- [docs/Docs-Index.md](docs/Docs-Index.md)
- [docs/Quick-Start-Step-By-Step.md](docs/Quick-Start-Step-By-Step.md)
- [docs/Architecture-Blueprint.md](docs/Architecture-Blueprint.md)
- [docs/Scaffolding-Guide.md](docs/Scaffolding-Guide.md)
- [docs/Testing-Guide.md](docs/Testing-Guide.md)

## 進階版

如果你需要更完整的微服務設計，例如事件匯流、Outbox、gRPC、觀測性與 ACA/Bicep 部署，請改看完整版 repo：

`https://github.com/eric861129/DotNet-MicroService-Sample-Full-`
