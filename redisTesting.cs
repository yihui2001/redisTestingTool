using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Text;

class Program
{
    private static readonly string RedisConnectionString = "10.43.5.237";
	
    private static  int AllClientCount;
	private static  int WriteClientCount;
	private static  int ReadClientCount;
    private static  int DataPointsPerSecond;
    private static  int DataPointSize; // char length 1ch = 2 byte
	private static readonly object counterLock = new object();
	private static readonly string KeyFilePath = "/home/asck8s02/Documents/dotnet/redisGetJson/data.json";
	private static List<string> keys;
	
	private static IDatabase db;
	
	private static ConnectionMultiplexer redisConnection;
    static async Task Main(string[] args)
    {
		var data = LoadKeysFromJson(KeyFilePath);
		keys = new List<string>(data.Keys);

		
		

		// 獲取命令列參數
        string[] arguments = Environment.GetCommandLineArgs();

        for (int i = 1; i < arguments.Length; i++) // Skip the first element which is the executable path
        {
            switch (arguments[i])
            {
                case "-set":
                    if (i + 1 < arguments.Length && int.TryParse(arguments[i + 1], out int setClients))
                    {
                        WriteClientCount = setClients;
                        i++; // Skip the next element since it's the value for the current option
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for -s option");
                    }
                    break;

                case "-get":
                    if (i + 1 < arguments.Length && int.TryParse(arguments[i + 1], out int getClients))
                    {
                        ReadClientCount = getClients;
                        i++; // Skip the next element since it's the value for the current option
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for -g option");
                    }
                    break;

                case "-d":
                    if (i + 1 < arguments.Length && int.TryParse(arguments[i + 1], out int dataSize))
                    {
                        DataPointSize = dataSize/2;
                        i++; // Skip the next element since it's the value for the current option
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for -d option");
                    }
                    break;

                case "-P":
                    if (i + 1 < arguments.Length && int.TryParse(arguments[i + 1], out int Pipeline ))
                    {
                        DataPointsPerSecond = Pipeline;
                        i++; // Skip the next element since it's the value for the current option
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for -d option");
                    }
                    break;

                default:
                    Console.WriteLine($"Unknown option: {arguments[i]}");
                    break;
            }
        }
		 
         StartTesting().Wait();


        Console.WriteLine("Analysis completed");
		
    }

    private static async Task StartTesting()
    {
        redisConnection = await ConnectionMultiplexer.ConnectAsync(RedisConnectionString);
        var tasks = new List<Task>();
        AllClientCount = WriteClientCount + ReadClientCount;
		Stopwatch stopwatch = Stopwatch.StartNew();	
        for (int i = 0; i < WriteClientCount; i++)
        {
            tasks.Add(Task.Run(() => SimulateWriteClient(i)));
        }
		for (int i = 0; i < ReadClientCount; i++){
			
			tasks.Add(Task.Run(() => SimulateReadClient(i)));
		}
        await Task.WhenAll(tasks);
		
		stopwatch.Stop();
		TimeSpan ts = stopwatch.Elapsed;

        Console.WriteLine($"{AllClientCount * DataPointsPerSecond} requests completed in {ts.TotalMilliseconds} ms");
        
		Console.WriteLine($"{WriteClientCount * DataPointsPerSecond} set requests , {ReadClientCount * DataPointsPerSecond} get requests");

		Console.WriteLine($"{AllClientCount}  parallel clients include: {WriteClientCount} set clients, {ReadClientCount} get clients");

		Console.WriteLine($"{DataPointSize*2} bytes payload" );


    }



    private static async Task SimulateWriteClient(int clientId)
    {
        var db = redisConnection.GetDatabase();
		var expiry = TimeSpan.FromDays(1); // Set expiration day to 1 day
		// save data to dic in order to save in json file
		
            var dataPoints = GenerateDataPoints(DataPointsPerSecond, DataPointSize);
            var tasks = new List<Task>();
			

            foreach (var dataPoint in dataPoints)
            {	
				string key = String.Format("{0:MM/dd-HH:mm:ss:ffffff}",DateTime.Now);  
                tasks.Add(db.StringSetAsync(key,dataPoint,expiry)); // set expiry
				//Console.WriteLine(key);
			
            }
		
            await Task.WhenAll(tasks);
			        
    }
	private static async Task SimulateReadClient(int clientId)
    {
		
		var db = redisConnection.GetDatabase();
        var random = new Random();
		var times = DataPointsPerSecond;
        while (times>0)
        {
			
			times--;
            var key = keys[random.Next(keys.Count)];
            var value = await db.StringGetAsync(key);

			// TODO: if value is null, go mongo db 
					
            // Console.WriteLine($"Key: {key}, Value: {value}");

        }
    }

    private static List<string> GenerateDataPoints(int count, int size)
    {
        var dataPoints = new List<string>();

        for (int i = 0; i < count; i++)
        {
            dataPoints.Add(GenerateRandomString(size));
        }

        return dataPoints;
    }

    private static string GenerateRandomString(int size)
    {
        var random = new Random();
        var builder = new StringBuilder(size);

        for (int i = 0; i < size; i++)
        {
            char ch = (char)random.Next('A', 'Z' + 1);
            builder.Append(ch);
        }
		
        return builder.ToString();
    }
	
	
	private static Dictionary<string, string> LoadKeysFromJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        if (keyValuePairs == null)
        {
            throw new InvalidOperationException("Failed to deserialize JSON file.");
        }

        return keyValuePairs;
    }

    
}
