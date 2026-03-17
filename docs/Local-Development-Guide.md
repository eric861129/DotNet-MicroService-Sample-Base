# 本機開發指南

這份文件會告訴你在本機開發時應該怎麼操作。

## 1. 本機開發的基本原則

- 基礎設施盡量用 Docker
- API 可以用 IDE 或命令列啟動
- 先確認資料庫與 RabbitMQ 活著
- 每次改大功能前先跑測試

## 2. 建議的工作順序

1. 啟動 docker-compose 基礎服務
2. `dotnet build`
3. `dotnet test`
4. 啟動你要修改的服務
5. 用 Postman / curl / Swagger 驗證

## 3. 本機常用指令

### 啟動基礎設施
```powershell
docker compose up -d rabbitmq catalog-db ordering-db inventory-db notification-db auth-db otel-collector prometheus loki tempo grafana
```

### 重建全部容器
```powershell
docker compose up --build
```

### 關閉全部
```powershell
docker compose down
```

### 看 log
```powershell
docker compose logs -f ordering-service-api
```

## 4. 本機除錯建議

### 想看 API 流程
先看服務 console log。

### 想看跨服務流程
去 Grafana 看 trace。

### 想看事件有沒有送出去
去 RabbitMQ 管理畫面看 queue。

## 5. 建議的學習順序

如果你是新人，建議先只看一條主線：

1. Catalog 建商品
2. Inventory 建庫存
3. Ordering 下單
4. Notification 看結果

先把這條主線跑通，再去理解更複雜的設定。
