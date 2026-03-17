# OrderingService 程式碼導讀

## 1. 這個服務在做什麼

OrderingService 是這個 boilerplate 的核心。

它示範了最完整的一條業務流程：

- 同步查詢其他服務
- 建立 Aggregate
- 寫入資料庫
- 寫入 Outbox
- 由背景工作發事件

## 2. 建議閱讀順序

1. `src/Services/Ordering/OrderingService.Api/Program.cs`
2. `src/Services/Ordering/OrderingService.Application/PlaceOrderCommand.cs`
3. `src/Services/Ordering/OrderingService.Infrastructure/CatalogGrpcClient.cs`
4. `src/Services/Ordering/OrderingService.Infrastructure/InventoryGrpcClient.cs`
5. `src/Services/Ordering/OrderingService.Domain/Order.cs`
6. `src/Services/Ordering/OrderingService.Domain/OrderItem.cs`
7. `src/Services/Ordering/OrderingService.Infrastructure/OrderRepository.cs`
8. `src/Services/Ordering/OrderingService.Infrastructure/OrderingDbContext.cs`
9. `src/BuildingBlocks/Enterprise.Persistence/OutboxDispatcherBackgroundService.cs`

## 3. 這條流程怎麼看

### `Program.cs`
先看服務入口怎麼把所有積木組起來。

尤其注意：

- `AddOrderingInfrastructure(...)`
- `AddEnterpriseEventTypeRegistry(...)`
- `AddEnterpriseMassTransit(...)`
- `AddHostedService<OutboxDispatcherBackgroundService<...>>()`

### `PlaceOrderCommand.cs`
這是下單的主舞台。

它做了四件事：

1. 問 Catalog 商品資料
2. 問 Inventory 庫存狀態
3. 建立 Order Aggregate
4. 把 `OrderPlacedIntegrationEvent` 寫進 Outbox

### `CatalogGrpcClient.cs` / `InventoryGrpcClient.cs`
這兩個檔案是橋樑。

它們把外部 gRPC 契約轉成 Application 層想要的資料格式。

### `Order.cs` / `OrderItem.cs`
這裡是訂單本身的商業規則。

### `OrderRepository.cs`
負責保存與讀取訂單。

### `OrderingDbContext.cs`
決定訂單在 SQL Server 裡怎麼存。

### `OutboxDispatcherBackgroundService.cs`
決定事件怎麼從資料庫被送到 Event Bus。

## 4. 初學者最該注意的觀念

### 觀念 1：同步查詢與非同步事件要分開
查商品、查庫存是同步。  
推進後續服務是非同步。

### 觀念 2：不要直接 Publish
先寫 Outbox，再由背景工作發送。

### 觀念 3：Order 是 Aggregate Root
OrderItem 要跟著 Order 一起被管理。

## 5. 如果你要擴充 Ordering，常改哪裡

### 新增訂單欄位
- `Order.cs`
- `OrderDto.cs`
- `OrderingDbContext.cs`

### 新增下單規則
- `PlaceOrderCommand.cs`
- `Order.cs`

### 新增新事件
- `OrderingService.Contracts`
- `PlaceOrderCommand.cs`
- 對應 consumer
