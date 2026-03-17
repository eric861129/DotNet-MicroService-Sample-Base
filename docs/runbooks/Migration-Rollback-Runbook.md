# 資料庫 Migration 失敗怎麼 Rollback

## 適用情境

- EF Core migration job 失敗
- 服務啟動時自動 migration 失敗
- 新 schema 已套用一半，系統進入不一致狀態

## 原則

1. 先停止繼續 rollout
2. 先確認哪一個 migration 失敗
3. 再決定要修正重跑還是回滾

## 本機排查

### 查看 migration
```powershell
dotnet ef migrations list `
  --project src/Services/Ordering/OrderingService.Infrastructure `
  --startup-project src/Services/Ordering/OrderingService.Api
```

### 回退到上一版
```powershell
dotnet ef database update PreviousMigrationName `
  --project src/Services/Ordering/OrderingService.Infrastructure `
  --startup-project src/Services/Ordering/OrderingService.Api
```

## Azure 建議流程

1. 暫停新 revision 對外流量
2. 找到失敗的 migration job log
3. 如果 schema 可安全回滾，執行上一版 migration bundle
4. 如果 schema 已經有破壞性修改，優先用 forward fix

## 不建議直接做的事

- 不要手動刪 `__EFMigrationsHistory` 記錄
- 不要在不明白資料影響的情況下直接改 production schema
- 不要用 `EnsureDeleted` 或破壞性 SQL 硬清資料庫

## 判斷完成條件

- schema 與應用程式版本重新對齊
- migration job 成功
- 服務健康檢查恢復正常
