# CatalogService 程式碼導讀

## 1. 這個服務在做什麼

CatalogService 管理商品資料。

它是最適合當第一個學習服務的例子，因為：

- 功能單純
- 分層清楚
- 同時有 REST 與 gRPC

## 2. 建議閱讀順序

1. `src/Services/Catalog/CatalogService.Api/Program.cs`
2. `src/Services/Catalog/CatalogService.Application/CreateProductCommand.cs`
3. `src/Services/Catalog/CatalogService.Application/GetProductQuery.cs`
4. `src/Services/Catalog/CatalogService.Infrastructure/CatalogProductRepository.cs`
5. `src/Services/Catalog/CatalogService.Infrastructure/CatalogDbContext.cs`
6. `src/Services/Catalog/CatalogService.Infrastructure/CatalogGrpcService.cs`
7. `src/Services/Catalog/CatalogService.Domain/Product.cs`

## 3. 每個檔案在幹嘛

### `Program.cs`
服務入口。

你會看到：

- 共用設定註冊
- JWT
- Application
- Infrastructure
- REST endpoint
- gRPC endpoint

### `CreateProductCommand.cs`
建立商品的用例。

這裡負責：

- 驗證輸入
- 呼叫 Domain 建立商品
- 交給 Repository 保存

### `GetProductQuery.cs`
查詢商品。

這裡示範：

- Query 與 Command 分開

### `CatalogProductRepository.cs`
幫 Application 隔離 EF Core。

### `CatalogDbContext.cs`
定義 Product 在資料庫中的表結構。

### `CatalogGrpcService.cs`
給其他服務查商品資料用。

### `Product.cs`
商品的商業規則。

## 4. 初學者應該先看哪裡

如果你只想快速理解一條完整流程：

1. 看 `Program.cs`
2. 看 `CreateProductCommand.cs`
3. 看 `Product.cs`
4. 看 `CatalogProductRepository.cs`

## 5. 你如果要新增商品欄位，應該改哪裡

假設你要新增 `Description`：

1. `Product.cs`
2. `ProductDto.cs`
3. `CreateProductCommand.cs`
4. `CatalogDbContext.cs`
5. migration

## 6. 這個服務最適合學什麼

- Clean Architecture 的基本走法
- Application 與 Domain 的分工
- REST 與 gRPC 如何共存
