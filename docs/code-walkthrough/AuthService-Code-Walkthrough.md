# AuthService 程式碼導讀

## 1. 這個服務在做什麼

AuthService 專門負責內部服務 token。

也就是說：

- Gateway
- Ordering
- Inventory
- Notification
- Catalog

都可以向它拿 token。

## 2. 建議閱讀順序

1. `src/Identity/AuthService.Api/Program.cs`
2. `src/Identity/AuthService.Api/AuthDbContext.cs`
3. `src/Identity/AuthService.Api/OpenIddictSeeder.cs`
4. `src/BuildingBlocks/Enterprise.Security/ClientCredentialsServiceTokenProvider.cs`

## 3. 重要檔案說明

### `Program.cs`
設定 OpenIddict server。

你會看到：

- token endpoint
- client credentials flow
- scopes
- signing / encryption key

### `AuthDbContext.cs`
OpenIddict 的 EF Core 資料庫上下文。

### `OpenIddictSeeder.cs`
啟動時建立示範用 client。

### `ClientCredentialsServiceTokenProvider.cs`
示範內部服務怎麼向 AuthService 換 token。

## 4. 初學者最該記住的事

這個服務是「機器對機器」授權，不是前端登入頁。

它的重點是：

- 服務要先表明自己是誰
- AuthService 才發 token
