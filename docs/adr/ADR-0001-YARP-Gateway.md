# ADR-0001 使用 YARP 作為 API Gateway

## 狀態

Accepted

## 決策

我們選擇 `YARP` 作為 API Gateway。

## 背景

這個範本需要一個能夠：

- 代理多個微服務
- 與 ASP.NET Core 深度整合
- 容易用程式碼與設定擴充
- 容易接上 JWT、Rate Limit、Health、OpenTelemetry

的 Gateway。

## 為什麼選 YARP

- 它是 Microsoft 維護
- 它直接建構在 ASP.NET Core 上
- 它和現有 .NET pipeline 非常一致
- 對這個 boilerplate 來說，心智模型較簡單

## 沒選什麼

### Ocelot
- 文件與社群可用，但整體整合感不如 YARP 自然

### Envoy / NGINX
- 很強，但會讓這份 .NET 教學型範本多出額外學習成本

## 代價

- 如果未來需要更複雜的 service mesh 能力，YARP 不是最終解
- 某些高階流量治理能力可能仍要交給平台層

## 結論

對於 `.NET 微服務範本` 這個目標，YARP 在可維護性與上手速度之間取得最好平衡。
