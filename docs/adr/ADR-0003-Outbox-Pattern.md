# ADR-0003 使用 Transactional Outbox Pattern

## 狀態

Accepted

## 決策

所有跨服務整合事件都要先寫進 Outbox，再由背景 dispatcher 發送。

## 背景

如果直接在交易內 publish broker message，會遇到最典型的問題：

- DB 成功，但 message 沒送出去
- message 送出去了，但 DB rollback

這會造成服務之間狀態不一致。

## 為什麼選 Outbox

- 能把「資料寫入」與「事件待發送」放在同一個本地交易
- 能提高分散式最終一致性的可靠性
- 是企業微服務常見基礎模式

## 搭配策略

- 發送端：Outbox
- 接收端：Inbox / Idempotent Consumer

## 代價

- 需要背景 dispatcher
- 需要監控 backlog
- 系統是最終一致，不是強一致

## 結論

這是這份 boilerplate 的核心能力之一，也是微服務範本是否真正 production-ready 的分水嶺。
