# 微服務通訊指南

這份文件專門說明：

- 誰跟誰講話
- 用什麼方式講
- 為什麼要這樣選

## 1. 先用一句話理解

這個專案有兩種對話方式：

- 馬上要答案：用同步通訊
- 不用馬上回：用非同步通訊

## 2. 同步通訊是什麼

同步通訊就是：

「我現在問你，你現在回我。」

在這個專案裡：

- 外部世界呼叫系統：REST
- 系統內部服務互相呼叫：gRPC

## 3. 非同步通訊是什麼

非同步通訊就是：

「我先把訊息丟出去，你晚點處理。」

在這個專案裡：

- 透過 MassTransit 發送事件
- 本機使用 RabbitMQ
- 雲端使用 Azure Service Bus

## 4. 哪些服務會同步呼叫

目前最重要的是：

### Ordering -> Catalog
用途：取得商品資訊

### Ordering -> Inventory
用途：確認庫存是否足夠

## 5. 哪些服務會非同步接收事件

### Inventory
接收 `OrderPlacedIntegrationEvent`

用途：

- 根據訂單內容保留庫存

### Notification
接收 `OrderPlacedIntegrationEvent`

用途：

- 建立通知紀錄

## 6. 為什麼 Ordering 不能直接改 Inventory 的資料庫

因為這會破壞微服務邊界。

正確做法是：

- Ordering 只能透過 API / gRPC / Event 跟 Inventory 溝通
- 不可以直接連進 Inventory 的 DB 改資料

這樣做的好處是：

- 邊界清楚
- 不會讓服務互相綁死
- 日後可以單獨部署與演進

## 7. gRPC 契約放哪裡

每個服務的 gRPC 契約都放在自己的 `Contracts` 專案：

- `CatalogService.Contracts`
- `InventoryService.Contracts`
- `OrderingService.Contracts`
- `NotificationService.Contracts`

`.proto` 檔案放在：

- `Protos/catalog.proto`
- `Protos/inventory.proto`
- `Protos/ordering.proto`
- `Protos/notification.proto`

## 8. 事件契約放哪裡

事件契約也放在 `Contracts` 專案。

目前主要事件：

- `OrderPlacedIntegrationEvent`

這樣做的原因是：

- 發送者與接收者都可以共用同一份契約
- 不需要每個服務各自再定義一次

## 9. 新增一條跨服務呼叫時要注意什麼

### 問自己三個問題
1. 我真的需要馬上拿到答案嗎？
2. 這個動作會不會造成跨服務寫入？
3. 如果對方暫時壞掉，我要怎麼處理？

### 決策原則
- 要立即回覆：優先 gRPC
- 不需要立即回覆：優先 Event
- 涉及跨服務狀態推進：優先 Event，不要同步串太長

## 10. 這份文件最重要的重點

如果你之後要新增功能，請優先記住：

- 查詢用同步
- 狀態推進用非同步
- 不共享資料庫
- 契約獨立放在 Contracts 專案
