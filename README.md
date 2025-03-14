# redisTestingTool

### Features

Redis officially provides a performance testing tool, [redis-benchmark](https://redis.io/docs/latest/operate/oss_and_stack/management/optimization/benchmarks/), but this tool can only test a single operation (such as SET/GET).
However, in real-world usage scenarios, it is often necessary to simulate mixed read and write operations.

To address this need, redisTestingTool provides a solution that simulates multiple clients interacting with a Redis database for both read and write operations.
Based on user-provided parameters, the tool launches a specified number of read and write clients and performs Redis database operations, making the performance testing process more representative of actual application scenarios.

### Execution
1. Restore .NET project dependencies:
`dotnet restore`


2. Configuration

Before running the project, ensure that the **Program** class is correctly configured with the Redis connection string and other parameters:
```

// Set connection information
private static readonly string RedisConnectionString = "your-redis-connect-ip";

// Set the number of test cycles. Once all cycles are completed,
// the program calculates statistical results such as average and median.
// A higher number provides more accurate results but increases execution time.
// Default: 100 cycles.
private static readonly int cycle = 100;

// Path for storing previously fetched data from Redis, used for simulating read keys.
// The script to fetch Redis data and store it as JSON is available in RedisToJson.
private static readonly string KeyFilePath = "your-path";

```

4. Build & Run

```
dotnet build
dotnet run -set <number_of_write_clients> -get <number_of_read_clients> -d <data_size_per_entry> -P <parallel_data_per_second>

```
### Use Case
1. Read-heavy workload
  - Scenario: Read-heavy workload
  - Command:
     `dotnet run -set 50 -get 1000 -d 1000 -P 5`
    
2. Heavy write workload with minimal reads
  - Scenario: Simulate a high-write workload with 1000 write clients, each writing 50 data entries per second, with each entry 1 KB in size, while only 10 clients perform read operations.
  - Command:
     `dotnet run -set 1000 -get 10 -d 1000 -P 50`

3.  Balanced read/write scenario
  - Scenario: Push the system to its limits by simulating 5000 write clients, each writing 100 data entries per second, with each entry 2 KB in size, while 5000 clients read concurrently.
  - Command:
     `dotnet run -set 5000 -get 5000 -d 2000 -P 100`
    
  The output will include Average, Min, Max, P50, P95, P99  


#### Parameter Description
`-set`: Specifies the number of write clients. Enter an integer value.

`-get`: Specifies the number of read clients. Enter an integer value.

`-d`: Sets the data size per entry. Enter an integer value.

`-P`: Sets the number of concurrent data operations per second. Enter an integer value.


#### Note
This is still a basic project, and the args handling has not been fully tested.
If you have any suggestions or improvements, feel free to submit a pull request.😃😃
