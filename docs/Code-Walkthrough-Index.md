# 程式碼導讀總覽

這份文件是「看原始碼時的地圖」。

如果你打開一堆 `.cs` 檔案後不知道先看哪裡，  
就先回到這裡。

## 建議閱讀順序

### 第一次看這個專案
1. [Gateway-Code-Walkthrough.md](code-walkthrough/Gateway-Code-Walkthrough.md)
2. [Catalog-Service-Code-Walkthrough.md](code-walkthrough/Catalog-Service-Code-Walkthrough.md)
3. [Inventory-Service-Code-Walkthrough.md](code-walkthrough/Inventory-Service-Code-Walkthrough.md)
4. [Ordering-Service-Code-Walkthrough.md](code-walkthrough/Ordering-Service-Code-Walkthrough.md)
5. [Notification-Service-Code-Walkthrough.md](code-walkthrough/Notification-Service-Code-Walkthrough.md)
6. [AuthService-Code-Walkthrough.md](code-walkthrough/AuthService-Code-Walkthrough.md)

### 如果你只想理解下單流程
1. [Ordering-Service-Code-Walkthrough.md](code-walkthrough/Ordering-Service-Code-Walkthrough.md)
2. [Inventory-Service-Code-Walkthrough.md](code-walkthrough/Inventory-Service-Code-Walkthrough.md)
3. [Notification-Service-Code-Walkthrough.md](code-walkthrough/Notification-Service-Code-Walkthrough.md)

### 如果你只想理解平台基礎設施
1. [Gateway-Code-Walkthrough.md](code-walkthrough/Gateway-Code-Walkthrough.md)
2. [AuthService-Code-Walkthrough.md](code-walkthrough/AuthService-Code-Walkthrough.md)
3. 再搭配 [Architecture-Blueprint.md](Architecture-Blueprint.md)

## 每份導讀文件裡有什麼

每份文件都會固定說明：

- 這個服務在整體系統的任務
- 建議先看哪個檔案
- 每個重要檔案在幹嘛
- 初學者應該先看哪一段
- 修改功能時常碰的地方

## 最重要的一句話

看這個專案時，不要從隨便一個 `.cs` 檔開始亂點。  
先從服務入口 `Program.cs` 看，再往 `Application`、`Infrastructure`、`Domain` 走，會最容易懂。
