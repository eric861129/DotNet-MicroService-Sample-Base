# Key Vault Secret Rotation

## 適用情境

- 憑證或密碼即將到期
- 懷疑秘密外洩
- 要例行輪替連線字串、client secret、API key

## 原則

1. 先新增新版 secret，不要直接覆蓋舊值
2. 讓 App Configuration 或應用程式指到新版 secret
3. 驗證新版本正常後，再停用舊版

## 建議步驟

### 1. 建立新 secret version
```bash
az keyvault secret set \
  --vault-name <kv-name> \
  --name <secret-name> \
  --value <new-secret-value>
```

### 2. 如果有 App Configuration reference，確認 reference 指向同一個 secret 名稱
- 多數情況不需要改名稱
- 只需要讓應用程式 refresh 設定

### 3. 觸發設定刷新
- 等 sentinel key 刷新
- 或重新部署服務

### 4. 驗證
- 看服務健康狀態
- 看存取外部資源是否成功
- 看 log 是否出現授權失敗

### 5. 停用舊版
- 確認所有環境都已切換成功後，再停用舊 secret version

## 特別注意

- 如果是 `InternalAuth` 的 client secret，Auth Service 與 client 端要同步切換
- 如果是 DB 連線字串，要先確認資料庫端帳密已更新

## 完成條件

- 新 secret 已被所有目標服務使用
- 舊 secret 不再有存取紀錄
