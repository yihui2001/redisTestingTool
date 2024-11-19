# redisTestingTool

### 功能


Redis 官方提供了一款效能測試工具[redis-benchmark](https://redis.io/docs/latest/operate/oss_and_stack/management/optimization/benchmarks/)，但該工具僅能針對單一操作（如 SET/GET）進行測試。
然而，在實際使用場景中，通常需要模擬讀寫混合操作。
針對此需求，redisTestingTool 提供了一個解決方案，可模擬多個客戶端與 Redis 資料庫進行讀寫操作。
此工具根據使用者提供的參數，啟動指定數量的讀取和寫入客戶端，並執行 Redis 資料庫的資料操作，從而更貼近實際應用情境的效能測試需求。

###  安裝與執行
1. 恢復 .NET 專案依賴項：
`dotnet restore`


2. 配置

在運行專案之前，請確保在 Program 類別中正確配置 Redis 連接字串和其他參數：
```
// 設定連線資訊
private static readonly string RedisConnectionString = "your-redis-connect-ip";

// 設定運行次數，全部跑完之後，程式會計算所有運行結果的平均、中位數等統計結果
// 數字越大，結果越精準，相對也會跑更久。
// 預設為100次。
private static readonly int cycle = 100;


// 存放先前從 Redis 抓取的資料的路徑，用來模擬讀取時的 key
// 抓取redis 資料存成Json的程式碼放在RedisToJson
private static readonly string KeyFilePath = "your-path";
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
