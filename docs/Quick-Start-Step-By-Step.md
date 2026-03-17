# 一步一步快速開始

這份文件是給第一次接觸這個專案的人用的。

目標只有一個：  
讓你不要想太多，照著做，就能把整個專案跑起來。

## 1. 你現在要做的事

你可以把這個專案想成一個城市：

- Gateway 是大門
- Catalog 是商品商店
- Inventory 是倉庫
- Ordering 是收銀台
- Notification 是通知員
- Auth Service 是守門員
- RabbitMQ 是送信員
- SQL Server 是資料倉庫

我們現在要做的事情，就是先把這座城市的基本設施全部打開。

## 2. 你需要先安裝什麼

### 必要工具
- `.NET SDK 9.0.310` 或更新版本
- `Docker Desktop`
- `Git`

### 如果你還要部署到 Azure
- `Azure CLI`
- `Bicep`

## 3. 先打開專案資料夾

```powershell
cd c:\Users\EricHuang黃祈豫\source\repos\eric861129\DotNet-MicroService-Sample
```

## 4. 啟動資料庫、訊息佇列與觀測工具

先不要急著啟動所有 API。  
先把「基礎設施」開起來。

```powershell
docker compose up -d rabbitmq catalog-db ordering-db inventory-db notification-db auth-db otel-collector prometheus loki tempo grafana
```

### 這一步做了什麼
- 開了 5 個 SQL Server 容器
- 開了 1 個 RabbitMQ
- 開了 OTEL Collector
- 開了 Prometheus、Tempo、Loki、Grafana

## 5. 建置專案

```powershell
dotnet restore EnterpriseMicroservicesBoilerplate.sln
dotnet build EnterpriseMicroservicesBoilerplate.sln
```

### 如果建置成功
你會看到 `Build succeeded`。

### 如果建置失敗
請先看 [Troubleshooting.md](Troubleshooting.md)。

## 6. 執行測試

```powershell
dotnet test EnterpriseMicroservicesBoilerplate.sln
```

### 你會看到三類測試
- Unit Tests
- Contract Tests
- Integration Tests

## 7. 啟動所有服務

如果你想用容器一次啟動整套系統：

```powershell
docker compose up --build
```

如果你想在 Visual Studio 或命令列逐個啟動也可以。

## 8. 啟動後可以去哪裡看

### API 入口
- Gateway: `http://localhost:8080`
- Catalog: `http://localhost:7201`
- Inventory: `http://localhost:7202`
- Ordering: `http://localhost:7203`
- Notification: `http://localhost:7204`
- Auth Service: `http://localhost:8085`

### 平台工具
- RabbitMQ 管理介面: `http://localhost:15672`
- Grafana: `http://localhost:3000`
- Prometheus: `http://localhost:9090`

## 9. 第一個你可以做的操作

### 步驟 1：建立商品
呼叫 Catalog API 建立一筆商品。

### 步驟 2：建立庫存
呼叫 Inventory API 幫這筆商品放入庫存。

### 步驟 3：送出訂單
呼叫 Ordering API 建立訂單。

### 步驟 4：檢查結果
- Inventory 是否扣到保留量
- Notification 是否出現通知紀錄

## 10. 你剛剛到底做了什麼

如果你成功建立訂單，系統實際上做了這些事：

1. Ordering 先問 Catalog「這個商品存在嗎」
2. Ordering 再問 Inventory「庫存夠不夠」
3. Ordering 把訂單存進資料庫
4. Ordering 把事件寫到 Outbox
5. 背景服務把事件送到 RabbitMQ
6. Inventory 收到事件後保留庫存
7. Notification 收到事件後建立通知

這就是這個 boilerplate 想示範的重點流程。

## 11. 下一步建議

跑起來之後，建議你接著看：

1. [Feature-Map.md](Feature-Map.md)
2. [Communication-Guide.md](Communication-Guide.md)
3. [Developer-Handbook.md](Developer-Handbook.md)
