# 常見問題與除錯手冊

這份文件整理最常遇到的問題。

## 1. `dotnet build` 失敗

### 可能原因
- .NET SDK 版本不對
- 套件還沒 restore
- 先前的 `bin/obj` 殘留

### 解法
```powershell
dotnet --info
dotnet restore EnterpriseMicroservicesBoilerplate.sln
dotnet build EnterpriseMicroservicesBoilerplate.sln
```

如果還是不行：

```powershell
Get-ChildItem -Recurse -Directory -Filter bin | Remove-Item -Recurse -Force
Get-ChildItem -Recurse -Directory -Filter obj | Remove-Item -Recurse -Force
```

## 2. Docker 容器啟不起來

### 可能原因
- 1433 或 8080 相關 port 被占用
- Docker Desktop 沒開
- 記憶體不足

### 解法
- 先確認 Docker Desktop 已啟動
- 檢查 Port 是否衝突
- 用 `docker compose logs` 看詳細錯誤

## 3. API 可以開，但資料庫連不上

### 可能原因
- SQL 容器尚未 ready
- 連線字串 port 不對
- SA 密碼不一致

### 解法
- 稍等 10 到 20 秒再重試
- 檢查對應的 `appsettings.Development.json`
- 檢查 docker compose 中 SQL 容器 port mapping

## 4. 訂單建立成功，但 Inventory 沒更新

### 先檢查三件事
1. Ordering 的 Outbox 是否有資料
2. RabbitMQ 是否收到訊息
3. Inventory Consumer 是否有錯誤 log

### 常見原因
- RabbitMQ 沒啟動
- Event 沒有被 dispatcher 發出去
- Consumer 處理過程中拋例外

## 5. Notification 沒出現資料

### 檢查方向
- `NotificationService` 是否啟動
- `InboxMessages` 是否已記錄
- `Notifications` 表是否有資料

## 6. Grafana 沒資料

### 可能原因
- OTEL Collector 沒啟動
- App 沒有把 OTLP export 送出去
- Grafana datasource 沒掛成功

### 先看哪裡
- `docker compose ps`
- `otel-collector` log
- `grafana` log

## 7. Azure 部署失敗

### 檢查順序
1. `az login`
2. Resource Group 是否存在
3. ACR 帳密是否正確
4. Bicep parameters 是否填對
5. Container image 是否真的 push 成功

## 8. 什麼時候應該回頭看哪份文件

- 本機跑不起來：看 [Quick-Start-Step-By-Step.md](Quick-Start-Step-By-Step.md)
- 想知道服務怎麼串：看 [Communication-Guide.md](Communication-Guide.md)
- 想知道事件為什麼沒送：看 [EventBus-And-Outbox-Guide.md](EventBus-And-Outbox-Guide.md)
- 想知道設定從哪來：看 [Configuration-Guide.md](Configuration-Guide.md)
