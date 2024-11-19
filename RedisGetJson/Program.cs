using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

class Program
{
    private static readonly string RedisConnectionString = "10.43.5.237";
	private static readonly int keySample = 10000;
    static async Task Main(string[] args)
    {
        Console.WriteLine("Fetching data from Redis...");

        var connection = await ConnectionMultiplexer.ConnectAsync(RedisConnectionString);
        var db = connection.GetDatabase();

        // Retrieve 10000 keys from Redis
        var server = connection.GetServer(RedisConnectionString, 6379);
        var keys = server.Keys(pattern: "*", pageSize: keySample).Take(keySample);

		Console.WriteLine("Finish Fetching");
		var keyList = new List<RedisKey>(keys);
        var data = new Dictionary<string, string>();



		foreach (var key in keys)
		{
			var value = await db.StringGetAsync(key);
            data[key] = value;
			Console.WriteLine(key);
		}




        // Serialize data to JSON
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

        // Write JSON to file
        await File.WriteAllTextAsync("data.json", json);

        Console.WriteLine("Data has been written to redis_data.json");
    }
}

