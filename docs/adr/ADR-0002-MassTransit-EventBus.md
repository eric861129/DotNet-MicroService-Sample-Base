# ADR-0002 使用 MassTransit 抽象 Event Bus

## 狀態

Accepted

## 決策

我們使用 `MassTransit` 當作 Event Bus abstraction，並支援：

- 本機 `RabbitMQ`
- Azure 正式環境 `Azure Service Bus`

## 背景

範本需要非同步事件，但不希望應用程式直接綁死在某一個 broker SDK 上。

## 為什麼選 MassTransit

- .NET 生態成熟
- 對 consumer、retry、routing、transport 切換支援完整
- 本地與 Azure 能共用同一套應用程式碼

## 沒選什麼

### 直接使用 RabbitMQ.Client
- 太貼 transport，會讓應用程式層知道太多 broker 細節

### 直接使用 Azure Service Bus SDK
- 不利於本機與多雲場景

## 代價

- 多一層 abstraction，要理解 MassTransit 的慣例
- 觀測性與 retry 行為需要額外治理

## 結論

MassTransit 讓這份範本能同時保有：

- 教學可讀性
- 基礎設施可替換性
- 真實世界可落地性
