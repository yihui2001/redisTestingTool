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
	
    private static readonly int AllClientCount = 600;
	private static readonly int WriteClientCount = 500;
	private static readonly int ReadClientCount = 100;
    private static readonly int DataPointsPerSecond = 20;
    private static readonly int DataPointSize = 100; // 1ch = 2 byte
	private static readonly object counterLock = new object();
	private static readonly string KeyFilePath = "/home/asck8s02/Documents/dotnet/redisGetJson/data.json";
	private static List<string> keys;
	
	private static IDatabase db;
	
	private static ConnectionMultiplexer redisConnection;
    static async Task Main(string[] args)
    {
		var data = LoadKeysFromJson(KeyFilePath);
		keys = new List<string>(data.Keys);
        var tasks = new List<Task>();
		
		redisConnection = await ConnectionMultiplexer.ConnectAsync(RedisConnectionString);

		
		
		
		
		
		
		
		
		
		
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


		Console.WriteLine($"{WriteClientCount * DataPointsPerSecond} set + {ReadClientCount * DataPointsPerSecond} get requests completed in {ts.TotalMilliseconds} ms");
		Console.WriteLine($"{AllClientCount}  parallel clients include: {WriteClientCount} set clients, {ReadClientCount} get clients");
		// Console.WriteLine($"{WriteClientCount} set clients");
		// Console.WriteLine($"{ReadClientCount} get clients");
		Console.WriteLine($"{DataPointSize*2} bytes payload" );
		// Console.WriteLine("Total write request: "+ WriteClientCount*DataPointsPerSecond);
		// Console.WriteLine("Total read request: "+ ReadClientCount*DataPointsPerSecond);

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
