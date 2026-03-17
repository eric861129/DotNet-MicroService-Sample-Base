# Quick Start Step By Step

## 1. Prerequisites

- .NET SDK 9.0.x
- Docker Desktop

執行：

```powershell
./scripts/check-prereqs.ps1
```

## 2. Start the stack

```powershell
./scripts/dev-up.ps1
```

啟動後可用端點：

- Gateway: `http://localhost:8080`
- Auth Service: `http://localhost:8085`
- Catalog Service: `http://localhost:7201`
- Ordering Service: `http://localhost:7203`

## 3. Request a token

```powershell
$token = (Invoke-RestMethod -Method Post `
  -Uri http://localhost:8085/connect/token `
  -ContentType 'application/x-www-form-urlencoded' `
  -Body 'grant_type=client_credentials&client_id=gateway-client&client_secret=gateway-secret&scope=catalog.read ordering.write').access_token
```

## 4. Create a product through Gateway

```powershell
$product = Invoke-RestMethod -Method Post `
  -Uri http://localhost:8080/catalog/products `
  -Headers @{ Authorization = "Bearer $token" } `
  -ContentType 'application/json' `
  -Body '{"sku":"SKU-001","name":"Base Lite Product","price":1280}'
```

## 5. Create an order through Gateway

```powershell
Invoke-RestMethod -Method Post `
  -Uri http://localhost:8080/ordering/orders `
  -Headers @{ Authorization = "Bearer $token" } `
  -ContentType 'application/json' `
  -Body "{""customerEmail"":""student@example.com"",""items"":[{""productId"":""$($product.productId)"",""quantity"":1}]}"
```

## 6. Verify the stack

```powershell
./scripts/run-smoke-tests.ps1 -AgainstRunningStack
```
