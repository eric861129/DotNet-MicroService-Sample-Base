# GitHub Pages 文件站部署指南

這份文件會教你怎麼把這個專案裡新增的靜態文件網站，直接部署到 GitHub Pages。

這次準備好的文件站有 3 套：

- `site/classic/`：正式文件站風格
- `site/cards/`：卡片式探索風格
- `site/storybook/`：角色導覽風格

這 3 套會一起部署上去，你不需要自己挑一套才能上線。
部署完成後，首頁會先讓使用者選風格。

## 1. 這套做法的特色

- 不需要 Node.js
- 不需要 npm install
- 不需要 build 前端專案
- 直接讀取 repo 裡的 `docs/*.md`
- 很適合 GitHub Pages
- 文件改了，網站內容也會跟著更新

## 2. 網站部署後的網址結構

如果你的 repo 是：

```text
https://github.com/eric861129/DotNet-MicroService-Sample
```

那 GitHub Pages 通常會長得像這樣：

```text
https://eric861129.github.io/DotNet-MicroService-Sample/
```

部署完成後，你會有這幾個入口：

- `/`：文件站首頁，展示 3 套風格
- `/classic/`：Classic Docs
- `/cards/`：Cards Explorer
- `/storybook/`：Storybook Guide

## 3. repo 裡新增了哪些檔案

### 文件網站本體

- `site/index.html`
- `site/classic/`
- `site/cards/`
- `site/storybook/`
- `site/shared/`

### GitHub Pages workflow

- `.github/workflows/docs-pages.yml`

### 這份部署教學

- `docs/GitHub-Pages-Docs-Sites-Guide.md`

## 4. GitHub Pages 是怎麼部署的

這個 workflow 會做 2 件事：

1. 把 `site/` 複製到部署輸出目錄
2. 把 `docs/` 也一起複製到部署輸出目錄

這樣網站前端就可以直接用瀏覽器讀取 `docs/*.md`。

也就是說，網站不是把 Markdown 先轉成 HTML 再部署，
而是把 Markdown 原檔一起放上去，讓瀏覽器在前端動態顯示。

## 5. 啟用 GitHub Pages 的步驟

### 第 1 步：把程式碼推上 GitHub

確認這些檔案已經在 `main`：

- `site/`
- `.github/workflows/docs-pages.yml`
- `docs/GitHub-Pages-Docs-Sites-Guide.md`

### 第 2 步：到 GitHub repo 設定頁

打開：

```text
Repository > Settings > Pages
```

### 第 3 步：設定 Source

把 Source 設成：

```text
GitHub Actions
```

### 第 4 步：等待 workflow 跑完

進到：

```text
Repository > Actions
```

找到：

```text
docs-pages
```

等它成功。

## 6. 什麼時候會自動重新部署

只要你推送下列內容到 `main`，網站就會自動重新部署：

- `site/**`
- `docs/**`
- `.github/workflows/docs-pages.yml`

## 7. 我該選哪一套風格當主推

### 如果你希望最穩、最像正式文件站

選 `Classic Docs`

### 如果你希望新手比較不害怕

選 `Cards Explorer`

### 如果你希望做出「帶路感」

選 `Storybook Guide`

## 8. 建議的實務用法

我建議不要刪掉任何一套。

最好的做法是：

- 首頁保留 3 套入口
- 預設推薦 `Classic Docs`
- onboarding 或教學場合使用 `Storybook Guide`
- 展示與 demo 時使用 `Cards Explorer`

## 9. 如果你想再客製

### 改文件清單

編輯：

```text
site/shared/docs-manifest.js
```

### 改共用讀檔邏輯

編輯：

```text
site/shared/docs-app.js
```

### 改各套網站的樣式

編輯：

```text
site/classic/theme.css
site/cards/theme.css
site/storybook/theme.css
```

### 改首頁

編輯：

```text
site/index.html
```

## 10. 為什麼不用 React / Vue / Next.js

因為這次目標是：

- 簡單
- 快速
- 能直接部署 GitHub

這種情境下，純靜態 HTML/CSS/JS 的維護成本最低。
