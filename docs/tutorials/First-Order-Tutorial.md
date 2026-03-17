# 第一條黃金路徑教學

這份教學會帶你從零走完一條最重要的流程：

1. 啟動整個系統
2. 建立商品
3. 建立庫存
4. 建立訂單
5. 查看通知

如果你是第一次接觸這個範本，請先完成這條路徑。
因為只要這條路跑通，你就會一次摸到：
- Gateway
- REST API
- Ordering -> Outbox
- RabbitMQ Event Bus
- Inventory Consumer
- Notification Consumer

## 1. 啟動環境

### PowerShell
```powershell
.\scripts\check-prereqs.ps1
.\scripts\dev-up.ps1
```

### bash
```bash
bash scripts/check-prereqs.sh
bash scripts/dev-up.sh
```

等 20 到 40 秒，讓資料庫、RabbitMQ、各服務都啟動完成。

## 2. 打開測試請求檔

你可以直接使用這份檔案：

- [First-Order-Tutorial.http](/c:/Users/EricHuang黃祈豫/source/repos/eric861129/DotNet-MicroService-Sample/docs/tutorials/First-Order-Tutorial.http)

如果你使用的是 VS Code REST Client、Rider HTTP Client 或 Visual Studio `.http` 支援，都可以直接送出請求。

## 3. 建立第一個商品

送出這個 request：

```http
POST http://localhost:8080/catalog/products
Content-Type: application/json

{
  "sku": "DEMO-CHAIR-001",
  "name": "學習小椅子",
  "price": 1280
}
```

### 你應該看到什麼

回應會長得像這樣：

```json
{
  "productId": "請把這個值記下來",
  "sku": "DEMO-CHAIR-001",
  "name": "學習小椅子",
  "price": 1280
}
```

把 `productId` 複製起來，下一步會用到。

## 4. 建立庫存

把剛剛的 `productId` 貼到下面：

```http
POST http://localhost:8080/inventory/inventory
Content-Type: application/json

{
  "productId": "貼上剛剛的 productId",
  "sku": "DEMO-CHAIR-001",
  "quantityOnHand": 50
}
```

### 這一步做了什麼

- Inventory Service 建立或更新庫存資料
- Ordering 之後下單時，會先向 Inventory 查詢可用數量

## 5. 建立訂單

```http
POST http://localhost:8080/ordering/orders
Content-Type: application/json

{
  "customerEmail": "student@example.com",
  "items": [
    {
      "productId": "貼上剛剛的 productId",
      "quantity": 2
    }
  ]
}
```

### 這一步背後真正發生的事情

1. Ordering 先去 Catalog 查商品資料
2. Ordering 再去 Inventory 查可用庫存
3. Ordering 建立本地訂單
4. Ordering 把 `OrderPlacedIntegrationEvent` 寫進 Outbox
5. 背景服務把 Outbox 事件送到 RabbitMQ
6. Inventory Consumer 收到事件後保留庫存
7. Notification Consumer 收到事件後寫入通知紀錄

這就是這個範本最重要的示範流程。

## 6. 查看通知

建立訂單後等 3 到 5 秒，再查：

```http
GET http://localhost:8080/notification/notifications
```

### 你應該看到什麼

你會看到一筆新的通知紀錄，代表 Notification Service 已經成功消費事件。

## 7. 如果你想看更細

### 看服務健康狀態
```http
GET http://localhost:7201/health
GET http://localhost:7202/health
GET http://localhost:7203/health
GET http://localhost:7204/health
```

### 看 RabbitMQ
- `http://localhost:15672`

### 看 Grafana
- `http://localhost:3000`

## 8. 常見失敗點

### 商品建立成功，但下單失敗
- 多半是因為沒有先建立庫存
- 或者 `quantityOnHand` 小於你下單的數量

### 下單成功，但看不到通知
- 先等 3 到 5 秒
- 再檢查 `notification-service-api` 與 `rabbitmq` 是否正常啟動
- 也可以執行 `.\scripts\run-smoke-tests.ps1 -AgainstRunningStack`

## 9. 下一步看哪裡

跑通這份教學後，建議按順序閱讀：

1. [Automation-Guide.md](../Automation-Guide.md)
2. [EventBus-And-Outbox-Guide.md](../EventBus-And-Outbox-Guide.md)
3. [Developer-Handbook.md](../Developer-Handbook.md)
4. [Code-Walkthrough-Index.md](../Code-Walkthrough-Index.md)
