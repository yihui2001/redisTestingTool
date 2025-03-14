# Redis fetch data to json

### Purpose
The purpose of this script is to extract data from a Redis database, serialize it into JSON format, and save it to a local file named data.json.
The data.json file will be placed in the same directory as Program.cs.


###  Installation & Execution
1. Clone the repository to your local machine:

```
git clone <repository-url>
cd <repository-directory>
```


2. Restore .NET project dependencies:
`dotnet restore`


3. Configuration

Before running the project, ensure that the Program class is correctly configured with the Redis connection string and other parameters:
```
// Set connection information
private static readonly string RedisConnectionString = "10.43.5.237";

// Set the number of records to fetch (default: 10,000)
private static readonly int keySample = 10000;

```

4. Build & Run

```
dotnet build
dotnet run
```
