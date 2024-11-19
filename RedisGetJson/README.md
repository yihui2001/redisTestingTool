# Redis fetch data to json

### 目的
此程式碼的目的是從 Redis 資料庫中提取資料，並將其序列化為 JSON 格式，然後寫入到本地檔案 data.json 中。
data.json會放置在與Program.cs相同的資料夾中。


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

// 設定要取幾筆資料，預設為10000比
private static readonly int keySample = 10000;
```

4. 編譯與執行

```
dotnet build
dotnet run
```
