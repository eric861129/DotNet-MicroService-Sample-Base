# Queue Poison Message 怎麼處理

## 適用情境

- 某個事件一直重試
- Queue 深度持續上升
- Consumer failure alert 持續觸發

## 先看哪裡

1. Grafana 的 `Consumer Retry / Failure / Duplicate`
2. RabbitMQ 管理介面
3. 該 consumer 的應用程式 log

## 排查步驟

### 1. 找出是哪個 queue
- 先看 Grafana 的 Queue Depth panel
- 再到 RabbitMQ 管理介面確認是哪個 queue 堆積

### 2. 找出是哪個事件
- 到服務 log 搜尋 `Failed to dispatch Outbox message`
- 或搜尋 consumer 例外 stack trace

### 3. 判斷原因
- Payload schema 與 consumer 版本不相容
- 目標資料不存在
- DB constraint 或唯一鍵衝突
- 外部依賴失敗

## 處理策略

### 可重試錯誤
- 先修復外部依賴
- 讓 consumer 自動 retry 或重新送一次

### 不可重試錯誤
- 先停止持續重試
- 把問題訊息移到隔離 queue 或人工記錄
- 修正資料後再決定是否重送

## 本範本建議做法

1. 保留原始 payload
2. 用 Inbox 確保重送不會重複執行
3. 修正資料或修正程式後再重送

## RabbitMQ 常用操作

### 查看 queue
```bash
curl -u guest:guest http://localhost:15672/api/queues
```

### 重新部署 consumer
- 本機：重啟對應 service container
- Azure：重啟或 rollout 新 revision

## 結束條件

- queue depth 開始下降
- consumer failure alert 清除
- 同一筆事件不再反覆失敗
