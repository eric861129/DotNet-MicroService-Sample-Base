# Dev Container / Codespaces 指南

這份文件教你怎麼用 `.devcontainer/` 在 VS Code 或 GitHub Codespaces 裡，快速打開這個 repo。

## 這個功能能解決什麼

如果團隊成員使用：

- Windows
- macOS
- Linux
- GitHub Codespaces

那麼本機環境常常會因為 SDK 版本、CLI 工具、shell 差異而不一致。

`.devcontainer/` 的目的，就是把這些差異盡量縮小，讓大家都能在接近一致的開發環境中工作。

## 目前提供的內容

`.devcontainer/` 目前已經內建：

- `.NET 9 SDK`
- Docker-in-Docker
- Azure CLI
- GitHub CLI
- VS Code 常用 extension
- 自動 `dotnet restore`
- 第一次啟動時自動建立 `.env`

## 在 VS Code 使用

### 1. 安裝 extension

請先安裝：

- `Dev Containers`

### 2. 開啟專案

在 VS Code 打開這個 repo。

### 3. 重新在容器中開啟

按 `F1`，輸入：

```text
Dev Containers: Reopen in Container
```

### 4. 等待初始化

第一次啟動會需要一些時間，因為要：

- 建立 dev container
- 安裝 features
- restore solution

## 在 GitHub Codespaces 使用

### 1. 建立 Codespace

在 GitHub repo 頁面點：

```text
Code > Codespaces > Create codespace on main
```

### 2. 等待 Codespace 初始化

它會直接套用 `.devcontainer/devcontainer.json`

### 3. 開始工作

進入 Codespace 後，你就可以直接：

```bash
dotnet build EnterpriseMicroservicesBoilerplate.sln
bash scripts/check-prereqs.sh
```

## 建議啟動流程

進入 dev container 後，建議照這個順序：

1. `bash scripts/check-prereqs.sh`
2. `bash scripts/dev-up.sh --infrastructure-only`
3. `dotnet build EnterpriseMicroservicesBoilerplate.sln`
4. `dotnet test EnterpriseMicroservicesBoilerplate.sln -c Debug`

## 注意事項

### Docker-in-Docker

這個 dev container 會安裝 Docker-in-Docker 功能。

它的目的不是取代正式環境，而是讓你可以在容器裡執行：

- `docker compose`
- Testcontainers
- image build

### Codespaces 資源限制

如果你在 Codespaces 啟動整套微服務與觀測性元件，會比較吃記憶體。

建議：

- 先起 `--infrastructure-only`
- 真正需要時再起所有服務

## 建議一起閱讀

1. [Automation-Guide.md](Automation-Guide.md)
2. [tutorials/First-Order-Tutorial.md](tutorials/First-Order-Tutorial.md)
3. [Setup-Guide.md](Setup-Guide.md)
