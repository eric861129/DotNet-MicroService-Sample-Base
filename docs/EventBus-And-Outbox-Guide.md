# 事件匯流與 Outbox 指南

這份文件是整個專案的核心文件之一。

如果你想知道這套微服務範本最像企業實戰的地方在哪裡，答案通常就是這份文件講的內容。

## 1. 先用最簡單的方式理解

當 `OrderingService` 建立訂單後，  
它還希望另外兩件事情發生：

- `InventoryService` 保留庫存
- `NotificationService` 建立通知

問題是：

這兩件事情不應該用「同一個資料庫交易」強行綁在一起。  
所以我們改用事件。

## 2. Event Bus 是什麼

你可以把 Event Bus 想成一個郵局。

- 發送者只負責投信
- 收件者各自收信
- 發送者不需要知道每個收件者怎麼做事

在本專案中：

- 發送工具：MassTransit
- 本機郵局：RabbitMQ
- Azure 郵局：Azure Service Bus

## 3. Outbox 是什麼

Outbox 可以想成「先寫好的待寄信匣」。

流程是：

1. 訂單成功建立
2. 事件先寫進資料庫中的 `OutboxMessages`
3. 背景服務看到有待送事件
4. 背景服務再把它送到 RabbitMQ / Service Bus

## 4. 為什麼不能直接 Publish

如果你直接 Publish，可能會遇到這種情況：

- 訂單資料已經存入 DB
- 但 Event Bus 剛好連不上

結果會變成：

- 訂單存在
- 但後續服務完全不知道發生了什麼事

這就是一致性破壞。

## 5. 目前專案是怎麼實作的

### 寫入事件
`OrderingService.Application/PlaceOrderCommand.cs`

在建立訂單後，會呼叫：

```csharp
await outboxStore.AddAsync(new OrderPlacedIntegrationEvent { ... });
```

### 背景發送
`Enterprise.Persistence/OutboxDispatcherBackgroundService.cs`

它會：

1. 找出還沒處理的 Outbox 記錄
2. 還原成真正的事件物件
3. 發送到 MassTransit
4. 成功後標記已處理

## 6. Inbox 是什麼

Inbox 是收件者的「已收信清單」。

因為訊息系統常常是至少一次投遞，  
同一個事件可能被送兩次。

所以消費者要先檢查：

- 這封信我有沒有收過？

如果收過，就不要再處理第二次。

## 7. 目前哪些服務有用 Inbox

### InventoryService
收到 `OrderPlacedIntegrationEvent` 後：

- 先查 `InboxMessages`
- 沒處理過才真的保留庫存

### NotificationService
收到 `OrderPlacedIntegrationEvent` 後：

- 先查 `InboxMessages`
- 沒處理過才建立通知

## 8. 事件版本怎麼控制

所有事件都繼承 `IntegrationEvent`，裡面有：

- `EventId`
- `OccurredOnUtc`
- `Version`

目前預設版本是 `v1`。

## 9. 新增事件的標準步驟

1. 在 `*.Contracts` 建立新事件 record
2. 繼承 `IntegrationEvent`
3. 在發送端用 `IOutboxStore.AddAsync(...)`
4. 在接收端建立 Consumer
5. 加入 Inbox 防重複處理
6. 補上測試

## 10. 哪些事情不要做

### 不要直接在命令裡呼叫 `IPublishEndpoint`
因為這樣很容易破壞交易一致性。

### 不要假設事件只會來一次
你一定要考慮重複投遞。

### 不要把整個資料表都塞進事件
事件 payload 應該只放最小必要資料。

## 11. 你可以把這套設計記成一句話

先把事情安全寫下來，再慢慢把消息送出去。
