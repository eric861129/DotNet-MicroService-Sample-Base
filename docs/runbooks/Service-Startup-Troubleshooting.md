# 服務啟不來怎麼查

## 適用情境

- API container 一直重啟
- 本機 `docker compose up` 後服務沒有成功起來
- ACA 部署後某個 revision 一直不健康

## 檢查順序

1. 先看容器或服務的標準輸出 log
2. 再看 `/health` 與 `/health/ready`
3. 再看相依服務是否已經啟動
4. 最後才去查程式碼或設定

## 本機排查

### 1. 先跑 preflight
```powershell
.\scripts\check-prereqs.ps1
```

### 2. 查看 compose 狀態
```powershell
docker compose ps
```

### 3. 查看單一服務 log
```powershell
docker compose logs ordering-service-api --tail 200
```

### 4. 檢查健康端點
```powershell
curl http://localhost:7203/health
curl http://localhost:7203/health/ready
```

## 最常見原因

### 連線字串錯誤
- 檢查 `.env`
- 檢查 `ConnectionStrings__*`
- 檢查 SQL container 是否真的起來

### Auth Authority 設錯
- 檢查 `Security__Jwt__Authority`
- 檢查 `auth-service-api` 是否正常

### RabbitMQ 沒起來
- 檢查 `rabbitmq` container
- 檢查 `Messaging__RabbitMq__Host`

### Migration 失敗
- 看服務啟動前的 migration log
- 參考 [Migration-Rollback-Runbook.md](Migration-Rollback-Runbook.md)

## Azure 排查

### 1. 看 revision 狀態
```bash
az containerapp revision list \
  --name <app-name> \
  --resource-group <rg> \
  --output table
```

### 2. 看容器 log
```bash
az containerapp logs show \
  --name <app-name> \
  --resource-group <rg> \
  --follow
```

### 3. 看 ingress 與環境變數
```bash
az containerapp show \
  --name <app-name> \
  --resource-group <rg> \
  --output yaml
```

## 判斷完成條件

- `/health` 與 `/health/ready` 都能正常回應
- log 沒有持續重啟或未處理例外
- 相依的資料庫、RabbitMQ、Auth 都可連通
