window.DOCS_MANIFEST = {
  siteTitle: "Enterprise Microservices Boilerplate 文件站",
  defaultDoc: "docs/Quick-Start-Step-By-Step.md",
  docs: [
    { id: "quick-start", title: "快速開始", path: "docs/Quick-Start-Step-By-Step.md", description: "最快把整個專案跑起來的入口。", category: "快速開始", audiences: ["初學者", "後端開發者", "DevOps"], tags: ["start", "setup", "first-step"] },
    { id: "docs-index", title: "文件索引", path: "docs/Docs-Index.md", description: "整份文件的總地圖，找不到東西時先來這裡。", category: "快速開始", audiences: ["全部"], tags: ["map", "index"] },
    { id: "feature-map", title: "功能地圖", path: "docs/Feature-Map.md", description: "每個功能對應的檔案位置與責任。", category: "快速開始", audiences: ["初學者", "後端開發者"], tags: ["map", "feature"] },
    { id: "local-dev", title: "本機開發指南", path: "docs/Local-Development-Guide.md", description: "在本機準備執行環境與啟動服務的做法。", category: "快速開始", audiences: ["初學者", "後端開發者"], tags: ["local", "development"] },
    { id: "devcontainer", title: "Dev Container 指南", path: "docs/Devcontainer-Guide.md", description: "用 VS Code Dev Container 或 Codespaces 快速進入可用環境。", category: "快速開始", audiences: ["初學者", "DevOps"], tags: ["devcontainer", "codespaces"] },
    { id: "architecture", title: "架構藍圖", path: "docs/Architecture-Blueprint.md", description: "整體微服務架構、通訊方式、部署拓樸與設計原則。", category: "架構與設計", audiences: ["架構師", "Tech Lead", "後端開發者"], tags: ["architecture", "design"] },
    { id: "communication", title: "服務通訊指南", path: "docs/Communication-Guide.md", description: "REST、gRPC、Event Bus 之間該怎麼用。", category: "架構與設計", audiences: ["後端開發者", "架構師"], tags: ["grpc", "rest", "event-bus"] },
    { id: "gateway", title: "Gateway 指南", path: "docs/Gateway-Guide.md", description: "YARP Gateway 路由、授權與外部入口的設計。", category: "架構與設計", audiences: ["後端開發者", "DevOps"], tags: ["yarp", "gateway"] },
    { id: "outbox", title: "Event Bus 與 Outbox 指南", path: "docs/EventBus-And-Outbox-Guide.md", description: "事件驅動、最終一致性、Outbox 與 Inbox 的核心說明。", category: "架構與設計", audiences: ["後端開發者", "架構師"], tags: ["outbox", "event", "consistency"] },
    { id: "adr-index", title: "ADR 索引", path: "docs/adr/ADR-Index.md", description: "架構決策記錄總表，說明為什麼這個範本要這樣設計。", category: "架構與設計", audiences: ["架構師", "Tech Lead", "後端開發者"], tags: ["adr", "decision"] },
    { id: "versioning-governance", title: "版本治理規範", path: "docs/adr/Versioning-Governance.md", description: "事件版本、gRPC proto 版本與破壞性變更規範。", category: "架構與設計", audiences: ["架構師", "後端開發者", "DevOps"], tags: ["versioning", "governance"] },
    { id: "developer-handbook", title: "開發者手冊", path: "docs/Developer-Handbook.md", description: "新增 Service、Event、設定與擴充方式的主手冊。", category: "開發指南", audiences: ["後端開發者", "Tech Lead"], tags: ["developer", "extension"] },
    { id: "scaffolding", title: "腳手架指南", path: "docs/Scaffolding-Guide.md", description: "如何用腳本快速建立新服務與新事件。", category: "開發指南", audiences: ["初學者", "後端開發者"], tags: ["scaffold", "script"] },
    { id: "automation", title: "自動化腳本指南", path: "docs/Automation-Guide.md", description: "一鍵啟動、關閉、重置與前置檢查腳本說明。", category: "開發指南", audiences: ["初學者", "後端開發者"], tags: ["scripts", "automation"] },
    { id: "walkthrough-index", title: "程式碼導讀索引", path: "docs/Code-Walkthrough-Index.md", description: "逐服務了解每個重要檔案該從哪裡看。", category: "開發指南", audiences: ["初學者", "後端開發者"], tags: ["walkthrough", "code"] },
    { id: "configuration", title: "設定管理指南", path: "docs/Configuration-Guide.md", description: "App Configuration、Key Vault 與本機設定的做法。", category: "平台能力", audiences: ["後端開發者", "DevOps"], tags: ["config", "key-vault"] },
    { id: "security", title: "安全指南", path: "docs/Security-Guide.md", description: "Entra ID、OpenIddict、JWT 與權限設定。", category: "平台能力", audiences: ["後端開發者", "DevOps", "SRE"], tags: ["security", "auth", "jwt"] },
    { id: "observability", title: "觀測性指南", path: "docs/Observability-Guide.md", description: "OpenTelemetry、Serilog、Grafana 與監控指標。", category: "平台能力", audiences: ["SRE", "DevOps", "後端開發者"], tags: ["otel", "grafana", "logging"] },
    { id: "testing", title: "測試指南", path: "docs/Testing-Guide.md", description: "單元測試、整合測試、契約測試與 E2E 驗證方式。", category: "平台能力", audiences: ["後端開發者", "DevOps"], tags: ["testing", "e2e", "contract"] },
    { id: "setup-guide", title: "Azure 建置與部署指南", path: "docs/Setup-Guide.md", description: "從零開始建立 Azure 資源並完成部署。", category: "部署與維運", audiences: ["DevOps", "架構師"], tags: ["azure", "deployment"] },
    { id: "troubleshooting", title: "疑難排解", path: "docs/Troubleshooting.md", description: "遇到問題時的常見排查方向。", category: "部署與維運", audiences: ["初學者", "SRE", "DevOps"], tags: ["troubleshooting", "support"] },
    { id: "runbooks", title: "Runbooks 索引", path: "docs/runbooks/Runbooks-Index.md", description: "服務啟動、poison message、migration rollback 等營運手冊。", category: "部署與維運", audiences: ["SRE", "DevOps"], tags: ["runbook", "operations"] },
    { id: "tutorial-first-order", title: "第一條黃金路徑教學", path: "docs/tutorials/First-Order-Tutorial.md", description: "從建立商品、補庫存到下單通知的完整教學。", category: "教學與入門", audiences: ["初學者", "後端開發者"], tags: ["tutorial", "first-order"] },
    { id: "persona-guide", title: "角色導覽", path: "docs/Persona-Guide.md", description: "我是初學者、後端、DevOps、SRE、架構師時該先看哪裡。", category: "教學與入門", audiences: ["全部"], tags: ["persona", "guide"] },
    { id: "pages-guide", title: "GitHub Pages 文件站部署指南", path: "docs/GitHub-Pages-Docs-Sites-Guide.md", description: "怎麼把這些靜態網站直接部署到 GitHub Pages。", category: "教學與入門", audiences: ["DevOps", "初學者"], tags: ["github-pages", "static-site"] }
  ],
  personas: [
    { id: "beginner", title: "我是初學者", description: "先把專案跑起來，再從最容易理解的文件開始。", docIds: ["quick-start", "docs-index", "tutorial-first-order", "feature-map", "walkthrough-index"] },
    { id: "backend", title: "我是後端開發者", description: "快速掌握服務邊界、事件設計、擴充方式與測試方法。", docIds: ["developer-handbook", "scaffolding", "communication", "outbox", "testing"] },
    { id: "devops", title: "我是 DevOps", description: "先建立環境，再看部署、觀測、營運與自動化。", docIds: ["setup-guide", "devcontainer", "observability", "runbooks", "automation"] },
    { id: "architect", title: "我是架構師 / Tech Lead", description: "重點放在架構藍圖、ADR、版本規範與擴充邊界。", docIds: ["architecture", "adr-index", "versioning-governance", "developer-handbook", "security"] }
  ]
};
