# Architecture Blueprint

## Services

- `Gateway.Api`
  - 對外唯一正式入口
  - 驗證 JWT
  - 代理 `/catalog/*` 與 `/ordering/*`
- `AuthService.Api`
  - 發行對稱簽章 JWT
  - 只支援 demo `client_credentials`
- `CatalogService.Api`
  - 管理商品資料
  - 只提供 REST
- `OrderingService.Api`
  - 建立與查詢訂單
  - 透過 REST 同步呼叫 Catalog 驗證商品

## Data Boundaries

- Catalog 與 Ordering 各自一個 SQL Server database
- 不共用資料庫
- 不透過事件同步資料

## Security Model

- Gateway 驗證 `issuer` / `audience` / `signing key`
- Auth Service 提供 token
- Catalog 與 Ordering 本機 port 視為 internal/dev use，正式入口仍是 Gateway

## Deliberately Removed

- Event bus 與 eventual consistency
- gRPC
- OpenIddict
- Observability stack
- Cloud deployment assets

這份樣板的目標是「先跑起來、先落地」，不是一次帶進所有完整版設計。
