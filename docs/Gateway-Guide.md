# Gateway 指南

這份文件專門說明 `Gateway.Api` 的角色。

## 1. Gateway 是什麼

Gateway 就像商場的正門。

外部客人不會直接走進每一間店的後門，  
而是先從正門進來，再被帶到正確的位置。

## 2. 這個專案的 Gateway 在做什麼

目前它負責：

- 對外統一入口
- 路由到對應服務
- 基本限流
- 集中放入口相關設定

## 3. 目前有哪些路由

- `/catalog/*` -> `CatalogService`
- `/ordering/*` -> `OrderingService`
- `/inventory/*` -> `InventoryService`
- `/notification/*` -> `NotificationService`

## 4. Gateway 設定在哪裡

主要在：

- `src/Gateway/Gateway.Api/Program.cs`
- `src/Gateway/Gateway.Api/appsettings.json`
- `src/Gateway/Gateway.Api/appsettings.Development.json`

## 5. 為什麼選 YARP

因為它：

- 與 ASP.NET Core 整合度高
- 客製化彈性好
- 容易延伸中介流程
- 對 .NET 團隊很友善

## 6. 現在的限流策略

目前使用固定視窗限流：

- 每分鐘 60 次
- 佇列 10 筆

這是示範用的預設值。  
正式環境通常要依 API 類型再細分。

## 7. 之後可繼續加什麼

- 更細的路由授權 policy
- Header 清理與轉換
- API Key 驗證
- WAF / Front Door 整合
- 分流到不同版本的服務

## 8. 初學者要記住的重點

Gateway 不是拿來放商業邏輯的。  
它應該只做入口層責任。

也就是說：

- 可以放路由
- 可以放認證
- 可以放限流
- 不要放訂單計算邏輯
