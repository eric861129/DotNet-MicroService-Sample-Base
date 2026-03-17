# InventoryService 程式碼導讀

## 1. 這個服務在做什麼

InventoryService 管理庫存。

它有兩條重要路線：

- 同步：提供庫存查詢給 Ordering
- 非同步：接收訂單事件來保留庫存

## 2. 建議閱讀順序

1. `src/Services/Inventory/InventoryService.Api/Program.cs`
2. `src/Services/Inventory/InventoryService.Application/UpsertInventoryItemCommand.cs`
3. `src/Services/Inventory/InventoryService.Application/GetInventoryAvailabilityQuery.cs`
4. `src/Services/Inventory/InventoryService.Infrastructure/OrderPlacedConsumer.cs`
5. `src/Services/Inventory/InventoryService.Infrastructure/InventoryGrpcService.cs`
6. `src/Services/Inventory/InventoryService.Infrastructure/InventoryRepository.cs`
7. `src/Services/Inventory/InventoryService.Infrastructure/InventoryDbContext.cs`
8. `src/Services/Inventory/InventoryService.Domain/InventoryItem.cs`

## 3. 重要檔案說明

### `Program.cs`
你會看到：

- MassTransit consumer 註冊
- REST endpoint
- gRPC endpoint

### `UpsertInventoryItemCommand.cs`
更新或建立庫存資料。

### `GetInventoryAvailabilityQuery.cs`
回覆「這個商品現在剩多少」。

### `OrderPlacedConsumer.cs`
這是很重要的檔案。

它說明：

- 事件如何被接收
- Inbox 怎麼避免重複處理
- 收到訂單事件後如何保留庫存

### `InventoryGrpcService.cs`
給 Ordering 同步查庫存。

### `InventoryItem.cs`
真正保護庫存規則的地方，例如：

- 庫存不能小於 0
- 保留數量不能小於等於 0
- 庫存不足時不能保留

## 4. 初學者最該看哪個檔

如果你只想理解「事件收到後會發生什麼」，請直接看：

- `OrderPlacedConsumer.cs`

如果你只想理解「庫存規則怎麼寫」，請看：

- `InventoryItem.cs`

## 5. 如果要修改庫存策略，要改哪裡

### 改商業規則
改 `InventoryItem.cs`

### 改查詢格式
改 `GetInventoryAvailabilityQuery.cs`

### 改事件處理方式
改 `OrderPlacedConsumer.cs`
