# 版本治理規範

這份文件定義這個 boilerplate 在 `Event`、`gRPC proto` 與公開契約上的版本治理原則。

## 1. 核心原則

1. 盡量向後相容
2. 破壞性變更必須明確標示
3. 契約版本不能只靠口頭約定
4. 每次破壞性變更都要補文件與測試

## 2. Integration Event 版本規則

### 事件基本規範

每個事件都應該有：

- `EventId`
- `OccurredOnUtc`
- `Version`

### 什麼叫非破壞性變更

- 新增 optional 欄位
- 新增 consumer 可忽略的 metadata

### 什麼叫破壞性變更

- 刪除欄位
- 改欄位型別
- 改欄位語意
- 改事件名稱

### 建議做法

- 非破壞性變更：維持事件名稱，更新 `Version`
- 破壞性變更：建立新事件型別，例如 `OrderPlacedV2IntegrationEvent`

## 3. gRPC proto 版本規則

### 基本規則

- 不重用 field number
- 已刪除欄位要保留 `reserved`
- 新欄位要用新的 field number

### 建議策略

- 小幅相容調整：同一份 proto 演進
- 破壞性改版：建立新 proto package 或新 service，例如 `catalog.v2`

## 4. REST / Gateway 版本規則

### 建議策略

- 對外 REST 優先使用 URL versioning 或 route namespace
- 例如：
  - `/api/v1/orders`
  - `/api/v2/orders`

### Gateway 規則

- Gateway route 要明確對應版本
- 不要讓不同版本共用模糊 route

## 5. Breaking Change 管理

發生 breaking change 時，至少要做這幾件事：

1. 更新對應 ADR 或版本治理文件
2. 更新 consumer / client
3. 補契約測試
4. 補 migration 或資料轉換策略
5. 明確標記淘汰時間

## 6. Deprecation 規則

### 建議流程

1. 宣告舊版本 deprecated
2. 保留一段觀察期
3. 監控是否還有流量或消費者使用
4. 再正式移除

## 7. 文件要求

只要有以下任一情況，就要同步更新文件：

- Event payload 改變
- proto 改變
- REST route 改變
- Gateway 路由改變
- 授權 scope 改變

## 8. 測試要求

版本變更至少要補一種：

- Unit test
- Contract test
- Integration test

若是跨服務契約，建議至少補：

- Contract test
- 一條 E2E happy path
