# NotificationService 程式碼導讀

## 1. 這個服務在做什麼

NotificationService 很適合用來理解：

- 事件驅動
- Inbox 防重複
- 簡單的讀模型

## 2. 建議閱讀順序

1. `src/Services/Notification/NotificationService.Api/Program.cs`
2. `src/Services/Notification/NotificationService.Infrastructure/OrderPlacedConsumer.cs`
3. `src/Services/Notification/NotificationService.Domain/NotificationLog.cs`
4. `src/Services/Notification/NotificationService.Infrastructure/NotificationRepository.cs`
5. `src/Services/Notification/NotificationService.Infrastructure/NotificationDbContext.cs`
6. `src/Services/Notification/NotificationService.Application/GetRecentNotificationsQuery.cs`
7. `src/Services/Notification/NotificationService.Infrastructure/NotificationGrpcService.cs`

## 3. 重要檔案說明

### `Program.cs`
註冊 MassTransit consumer、REST、gRPC。

### `OrderPlacedConsumer.cs`
收到訂單事件後建立通知資料。

這份檔案同時示範：

- 先查 Inbox
- 再做真正處理
- 最後記錄已處理

### `NotificationLog.cs`
通知紀錄的 Domain。

### `NotificationRepository.cs`
查詢最近通知清單。

### `NotificationDbContext.cs`
定義通知資料表結構。

### `NotificationGrpcService.cs`
提供內部服務查最近通知。

## 4. 這個服務最值得學的地方

如果你想學「最簡單的事件消費者長什麼樣」，  
這個服務就是最好的例子。
