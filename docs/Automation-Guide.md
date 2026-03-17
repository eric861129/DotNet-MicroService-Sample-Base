# 自動化腳本指南

這份文件專門介紹 `scripts/` 目錄裡的快速操作腳本。
如果你是第一次接觸這個範本，建議先看這份，再看更深入的架構文件。

## 1. 你會用到哪些腳本

### Windows / PowerShell
- `scripts/check-prereqs.ps1`
- `scripts/dev-up.ps1`
- `scripts/dev-down.ps1`
- `scripts/reset-local.ps1`
- `scripts/run-smoke-tests.ps1`
- `scripts/new-service.ps1`
- `scripts/new-event.ps1`

### macOS / Linux / bash
- `scripts/check-prereqs.sh`
- `scripts/dev-up.sh`
- `scripts/dev-down.sh`
- `scripts/reset-local.sh`
- `scripts/run-smoke-tests.sh`
- `scripts/new-service.sh`
- `scripts/new-event.sh`

## 2. 第一次使用的最短路徑

### PowerShell
```powershell
.\scripts\check-prereqs.ps1
.\scripts\dev-up.ps1
.\scripts\run-smoke-tests.ps1 -AgainstRunningStack
```

### bash
```bash
bash scripts/check-prereqs.sh
bash scripts/dev-up.sh
bash scripts/run-smoke-tests.sh --against-running-stack
```

## 3. 每支腳本在做什麼

### `check-prereqs`
- 檢查 `.NET SDK`
- 檢查 `Docker` 與 `docker compose`
- 視需要檢查 `Azure CLI` / `Bicep`
- 自動建立 `.env`
- 檢查常用埠是否已被其他程式占用

### `dev-up`
- 啟動本機開發環境
- 預設會 `docker compose up --build -d`
- 可以用參數改成只啟動基礎設施

### `dev-down`
- 停止目前的 compose 環境
- 可選擇一起刪除 volumes

### `reset-local`
- 停止 compose 容器
- 清掉 `bin / obj / TestResults / .vs`
- 可選擇連 `.env` 一起刪掉，讓環境回到最乾淨狀態

### `run-smoke-tests`
- 執行 `restore / build / test`
- 可選擇啟用 `Testcontainers`
- 可選擇檢查目前執行中的服務是否有健康回應

### `new-service`
- 產生一個新的微服務骨架
- 包含 `Domain / Application / Infrastructure / Api / Contracts`
- 預設會幫你加入 solution

### `new-event`
- 產生新的 Integration Event
- 也可以順便產生 Consumer 檔案

## 4. 常用範例

### 只啟動基礎設施
```powershell
.\scripts\dev-up.ps1 -InfrastructureOnly
```

```bash
bash scripts/dev-up.sh --infrastructure-only
```

### 停掉環境並清掉 volumes
```powershell
.\scripts\dev-down.ps1 -RemoveVolumes -RemoveOrphans
```

```bash
bash scripts/dev-down.sh --remove-volumes --remove-orphans
```

### 執行含容器的 smoke tests
```powershell
.\scripts\run-smoke-tests.ps1 -WithContainerTests
```

```bash
bash scripts/run-smoke-tests.sh --with-container-tests
```

## 5. `.env` 是什麼

- `.env.example` 是範例
- `.env` 是你本機實際使用的設定
- 如果 `.env` 不存在，多數腳本會自動從 `.env.example` 複製一份

你最常需要改的通常只有：
- 對外 port
- `SQL_SA_PASSWORD`
- `RABBITMQ_*`

## 6. 建議閱讀順序

1. [Quick-Start-Step-By-Step.md](Quick-Start-Step-By-Step.md)
2. [Automation-Guide.md](Automation-Guide.md)
3. [tutorials/First-Order-Tutorial.md](tutorials/First-Order-Tutorial.md)
4. [Developer-Handbook.md](Developer-Handbook.md)
