# 角色導覽

這份文件不是講技術細節，而是幫不同角色的人快速找到「你現在最應該看哪幾份文件」。

如果你打開 repo 後覺得文件太多，先從這裡開始。

## 我是初學者

### 你的目標

- 先把專案跑起來
- 先知道每個服務在做什麼
- 不需要一開始就讀懂所有架構細節

### 建議閱讀順序

1. [Quick-Start-Step-By-Step.md](Quick-Start-Step-By-Step.md)
2. [Automation-Guide.md](Automation-Guide.md)
3. [tutorials/First-Order-Tutorial.md](tutorials/First-Order-Tutorial.md)
4. [Feature-Map.md](Feature-Map.md)
5. [Code-Walkthrough-Index.md](Code-Walkthrough-Index.md)

### 你可以先跳過

- Bicep 細節
- CI/CD workflow 細節
- ADR 與版本治理

## 我是後端開發者

### 你的目標

- 知道要改哪一層
- 能快速新增 Service / Event / API / gRPC
- 理解 Outbox、Inbox、Consumer 的規則

### 建議閱讀順序

1. [Developer-Handbook.md](Developer-Handbook.md)
2. [Scaffolding-Guide.md](Scaffolding-Guide.md)
3. [EventBus-And-Outbox-Guide.md](EventBus-And-Outbox-Guide.md)
4. [Communication-Guide.md](Communication-Guide.md)
5. [docs/adr/ADR-Index.md](adr/ADR-Index.md)
6. [docs/adr/Versioning-Governance.md](adr/Versioning-Governance.md)

## 我是 DevOps / Platform Engineer

### 你的目標

- 快速建立環境
- 看懂 pipeline、部署與 migration 策略
- 掌握 rollback、rotation、觀測性

### 建議閱讀順序

1. [Setup-Guide.md](Setup-Guide.md)
2. [Devcontainer-Guide.md](Devcontainer-Guide.md)
3. [Observability-Guide.md](Observability-Guide.md)
4. [Testing-Guide.md](Testing-Guide.md)
5. [docs/runbooks/Runbooks-Index.md](runbooks/Runbooks-Index.md)

## 我是 SRE / 值班工程師

### 你的目標

- 知道哪裡看健康狀態
- 知道事件卡住時怎麼查
- 知道 deployment 出錯時怎麼 rollback

### 建議閱讀順序

1. [Observability-Guide.md](Observability-Guide.md)
2. [docs/runbooks/Runbooks-Index.md](runbooks/Runbooks-Index.md)
3. [Troubleshooting.md](Troubleshooting.md)
4. [docs/adr/Versioning-Governance.md](adr/Versioning-Governance.md)

## 我是架構師 / Tech Lead

### 你的目標

- 評估技術選型是否合理
- 確認服務邊界、版本治理與演進策略
- 建立團隊共識

### 建議閱讀順序

1. [Architecture-Blueprint.md](Architecture-Blueprint.md)
2. [docs/adr/ADR-Index.md](adr/ADR-Index.md)
3. [docs/adr/Versioning-Governance.md](adr/Versioning-Governance.md)
4. [Setup-Guide.md](Setup-Guide.md)
5. [Developer-Handbook.md](Developer-Handbook.md)

## 如果你只有 30 分鐘

### 最短閱讀路徑

1. [Quick-Start-Step-By-Step.md](Quick-Start-Step-By-Step.md)
2. [tutorials/First-Order-Tutorial.md](tutorials/First-Order-Tutorial.md)
3. [Feature-Map.md](Feature-Map.md)

## 如果你要帶新人

建議直接給新人這個順序：

1. [Persona-Guide.md](Persona-Guide.md)
2. [Automation-Guide.md](Automation-Guide.md)
3. [tutorials/First-Order-Tutorial.md](tutorials/First-Order-Tutorial.md)
4. [Code-Walkthrough-Index.md](Code-Walkthrough-Index.md)
