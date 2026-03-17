# Testing Guide

## 測試組成

- `tests/Enterprise.UnitTests`
  - `DemoTokenIssuer` claim 與有效期
- `tests/Enterprise.IntegrationTests`
  - Auth token issuance
  - Gateway authorization / proxy
  - Catalog create / query
  - Ordering create / query / error handling

## 執行方式

```powershell
dotnet test EnterpriseMicroservicesBoilerplate.sln
```

## Smoke Test

如果本機 docker stack 已經啟動：

```powershell
./scripts/run-smoke-tests.ps1 -AgainstRunningStack
```

這會：

1. 建置與測試 solution
2. 透過 Auth 拿 token
3. 經 Gateway 建商品
4. 經 Gateway 建訂單
