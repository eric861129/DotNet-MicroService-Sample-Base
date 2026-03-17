# Scaffolding Guide

Base Lite 的新服務腳手架只有單一 `*.Api` 專案。

## 建立新服務

```powershell
./scripts/new-service.ps1 -Name Billing
```

會產生：

- `src/Services/Billing/BillingService.Api`
- Minimal API
- SQL Server `DbContext`
- `appsettings.json`
- `Dockerfile`
- `.http` 範例檔

## 手動接入項目

1. 把新專案加入 solution
2. 在 `docker-compose.yml` 新增 API 與 DB 容器
3. 視需要在 `Gateway.Api/appsettings*.json` 新增 route / cluster
4. 補對應的 integration tests

## 原則

- 先用 REST
- 先單專案
- 先同步流程
- 等需求成形後，再考慮升級到完整版設計
