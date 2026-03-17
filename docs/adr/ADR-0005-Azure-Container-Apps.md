# ADR-0005 以 Azure Container Apps 作為主要部署平台

## 狀態

Accepted

## 決策

正式環境主線使用 `Azure Container Apps`，不是 AKS。

## 背景

這份範本要兼顧：

- 雲端原生部署
- 成本與操作複雜度
- 團隊上手速度

## 為什麼選 ACA

- 對多數中小型到中大型微服務場景已足夠
- 比 AKS 更快上手
- 與 revision、ingress、secret、Dapr、生態整合友善
- 適合把焦點放在應用程式，而不是 K8s 維運

## 沒選什麼

### AKS
- 功能更完整
- 但維運、網路、升級與平台知識門檻更高

## 代價

- 某些細緻平台控制能力不如 AKS
- 進階網路與平台治理彈性較有限

## 結論

對這份 `教學優先 + 企業可落地` 的 boilerplate 來說，ACA 是更合理的主線。
