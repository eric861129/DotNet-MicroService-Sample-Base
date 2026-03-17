# 觀測性指南

這份文件要幫你理解：

- Log 看哪裡
- Trace 看哪裡
- Metrics 看哪裡
- 出事時怎麼追

## 1. 什麼是觀測性

你可以把觀測性想成「幫系統裝監視器」。

當系統出問題時，你至少要能回答：

- 哪裡壞了
- 什麼時候壞的
- 哪個請求引發的
- 它影響了哪些服務

## 2. 這個專案用了哪些工具

### Log
- Serilog

### Trace / Metrics
- OpenTelemetry

### 本機顯示工具
- OTEL Collector
- Tempo
- Loki
- Prometheus
- Grafana

## 3. 每個工具做什麼

### Serilog
把系統日誌寫成結構化格式。

### OpenTelemetry
收集：

- Trace
- Metrics
- Logs

### Tempo
看請求跨服務的旅程。

### Loki
看 Log。

### Prometheus
收集與查詢 Metrics。

### Grafana
把上面幾種資料畫成圖表。

## 4. 共用設定在哪裡

- `src/BuildingBlocks/Enterprise.Observability/ObservabilityExtensions.cs`

## 5. Trace ID 為什麼重要

當一個請求從 Gateway 進來後，它可能會經過：

- Gateway
- Ordering
- Catalog
- Inventory
- Notification

如果每個服務都各寫各的 log，你就會很難追。

Trace ID 的作用就是：

- 把這一整趟旅程串起來

## 6. 本機怎麼看

### Grafana
看板入口：`http://localhost:3000`

### Tempo
從 Grafana 內查 Trace。

### Loki
從 Grafana 內查 Log。

### Prometheus
看 Metrics 原始資料：`http://localhost:9090`

## 7. 雲端怎麼接

正式環境建議：

- OTLP -> Azure Monitor / Application Insights

## 8. 新增自訂追蹤要怎麼做

### 步驟 1：建立 Activity

```csharp
using var activity = activitySource.StartActivity("ReserveInventory");
```

### 步驟 2：加 Tag

```csharp
activity?.SetTag("product.id", productId);
```

## 9. 初學者最需要記住的事

沒有觀測性，就很難維運微服務。  
服務越多，越不能只靠 `Console.WriteLine`。
## 新增的 Dashboard 與 Alert

本機觀測性目前已補上：

- `infra/local/grafana/dashboards/enterprise-overview.json`
- `infra/local/prometheus-alerts.yml`

你可以直接在 Grafana 看到：

- API Request Rate
- API Latency P95
- API Failure Rate
- Outbox Backlog
- Consumer Retry / Failure / Duplicate
- RabbitMQ Queue Depth

Alert 規則目前會針對：

- API 5xx 比例過高
- Outbox backlog 持續堆積
- Consumer failure
- RabbitMQ queue depth 偏高
