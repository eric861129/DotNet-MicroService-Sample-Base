# ADR-0004 使用 OpenIddict 作為內部服務授權核心

## 狀態

Accepted

## 決策

內部 service-to-service token 採用 `OpenIddict`。

## 背景

範本需要同時支援：

- 外部使用者授權
- 內部服務彼此呼叫
- 本機可啟動
- Azure 可部署

## 為什麼選 OpenIddict

- .NET 原生整合度高
- 可以直接放進現有 ASP.NET Core 與 EF Core 生態
- 對教學型 boilerplate 來說，比引入外部大型 IAM 產品更容易理解

## 沒選什麼

### Duende IdentityServer
- 很強，但商業授權與學習成本較高

### Keycloak
- 功能完整，但對這份 .NET-first 範本來說額外維運成本較高

## 代價

- 需要自行管理簽章金鑰與 client seed
- 正式環境要搭配 Key Vault 與憑證治理

## 結論

OpenIddict 適合作為這份範本的內部授權主線，外部身分則可由 Entra ID 接手。
