# redisTestingTool

### 目的
此程式碼的目的是模擬多個客戶端與 Redis 資料庫進行讀寫操作。程式會根據使用者提供的參數，啟動指定數量的寫入和讀取客戶端，並在 Redis 資料庫中進行資料點的操作。


###  安裝與執行
1. clone此存儲庫到您的本機：

```
git clone <repository-url>
cd <repository-directory>
```


2. 恢復 .NET 專案依賴項：
`dotnet restore`


3. 配置

在運行專案之前，請確保在 Program 類別中正確配置 Redis 連接字串和其他參數：
```
// 設定連線資訊
private static readonly string RedisConnectionString = "10.43.5.237";

// 設定運行次數，全部跑完之後，程式會計算所有運行結果的平均、中位數等統計結果
// 數字越大，結果越精準，相對也會跑更久。
// 預設為100次。
private static readonly int cycle = 100;


// 存放先前從 Redis 抓取的資料的路徑，用來模擬讀取時的 key
// 抓取redis 資料存成Json的程式碼放在RedisToJson
private static readonly string KeyFilePath = "/home/asck8s02/Documents/dotnet/redisGetJson/data.json";
```

4. 編譯與執行

```
dotnet build
dotnet run -set <寫入client數量> -get <讀取client數量> -d <每筆資料大小> -P <設定每秒並行的資料的數量>
```

#### 參數說明
-   -set：指定寫入客戶端的數量。請輸入一個整數值。
-   -get：指定讀取客戶端的數量。請輸入一個整數值。
-   -d：設定每筆資料大小。請輸入一個整數值。
-   -P：設定每秒並行的資料的數量。請輸入一個整數值。
