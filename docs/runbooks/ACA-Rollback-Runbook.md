# Azure Container Apps Rollback

## 適用情境

- 新 revision 部署後健康檢查失敗
- smoke test 失敗
- 錯誤率或延遲突然升高

## 原則

1. 先恢復服務，再分析根因
2. 優先切回上一個健康 revision
3. 不要在故障狀態下繼續推新版本

## 快速回滾步驟

### 1. 找出 revision
```bash
az containerapp revision list \
  --name <app-name> \
  --resource-group <rg> \
  --output table
```

### 2. 把流量切回健康 revision
```bash
az containerapp ingress traffic set \
  --name <app-name> \
  --resource-group <rg> \
  --revision-weight <healthy-revision>=100
```

### 3. 驗證
```bash
curl -f https://<gateway-url>/
curl -f https://<gateway-url>/ordering/health
```

## 如果是整批服務一起出問題

1. 先回滾 Gateway
2. 再回滾核心寫入路徑服務，例如 Ordering
3. 最後回滾周邊服務，例如 Notification

## 回滾後要做的事

- 保留失敗 revision 的 log
- 比對這次變更的 image tag、設定、migration
- 補上對應 runbook 或測試，避免同類問題再發生
