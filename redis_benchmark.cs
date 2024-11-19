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
	// fill in your connect info 
    private static readonly string RedisConnectionString = "";
	
    private static  int AllClientCount;
    private static  int WriteClientCount;
    private static  int ReadClientCount;
    private static  int DataPointsPerSecond;
    private static  int DataPointSize; // char length 1ch = 2 byte
    private static readonly object counterLock = new object();
    private static readonly string KeyFilePath = "your-path";
    private static readonly int cycle = 100;
    private static List<string> keys;
    private static IDatabase db;
    private static ConnectionMultiplexer redisConnection;	
    private static double totalTime = 0;
	
	
    static async Task Main(string[] args)
    {
		var data = LoadKeysFromJson(KeyFilePath);
		keys = new List<string>(data.Keys);

		string hostname = null;
		string port = null;
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
					
					
				case "-host":
				 if (i + 1 < arguments.Length )
                    {
                        hostname =  arguments[i + 1];
                        i++; // Skip the next element since it's the value for the current option
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for -h option");
                    }
                    break;
				
				case "-port":
				 if (i + 1 < arguments.Length )
                    {
                        port = arguments[i + 1];
                        i++; // Skip the next element since it's the value for the current option
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for -port option");
                    }
                    break; 
					
				case "-interval":
                    if (i + 1 < arguments.Length && int.TryParse(arguments[i + 1], out int inteval ))
                    {
                        intervalTime = inteval;
                        i++; // Skip the next element since it's the value for the current option
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for -t option");
                    }
                    break;
					
				case "-cycle":
                    if (i + 1 < arguments.Length && int.TryParse(arguments[i + 1], out int c ))
                    {
                        cycle = c;
                        i++; // Skip the next element since it's the value for the current option
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for -cycle option");
                    }
                    break;
					
				case "-file":
					if (i + 1 < arguments.Length )
                    {
                        KeyFilePath = arguments[i + 1];
                        i++; // Skip the next element since it's the value for the current option
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for -file option");
                    }
                    break; 

                default:
                    Console.WriteLine($"Unknown option: {arguments[i]}");
                    break;
					
					
            }
        }
		 

		
		if(port==null){
			RedisConnectionString = $"{hostname}";
		}
		else{
			
			RedisConnectionString = $"{hostname}:{port}";
		}
		

		
        await StartTesting();


   
		
    }

    private static async Task StartTesting()
    {
		
		try
		{
			 redisConnection = await ConnectionMultiplexer.ConnectAsync(RedisConnectionString);
		}
       catch (Exception e)
		{
			Console.WriteLine("Connect error");
			throw;
		}
		
		//Console.WriteLine("Start test");
		
        var tasks = new List<Task>();
        AllClientCount = WriteClientCount + ReadClientCount;
		int runtime = cycle;
		List<double> times = new List<double>(); 
		while(runtime>0){
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
			
			
			double currentTime = ts.TotalMilliseconds;
			int delayTime = intervalTime - (int)currentTime;
			if(delayTime<0){
				delayTime = 0;
			}
			
			await Task.Delay(delayTime);
			
			
			times.Add(currentTime); 
			totalTime += currentTime;

			//Console.WriteLine(runtime);
			
			runtime--;
		}
		
		double result = totalTime/cycle;
 
		double avg = totalTime / times.Count;


		times.Sort();
		double p50 = times.Count % 2 == 0 ? 
					 (times[times.Count / 2 - 1] + times[times.Count / 2]) / 2.0 :
					 times[times.Count / 2];

		double min = times.Min();
		double max = times.Max();

	
		double p95 = times[(int)(times.Count * 0.95) - 1]; 
		double p99 = times[(int)(times.Count * 0.99) - 1]; 

		Console.WriteLine($"Average: {avg} ms");
		Console.WriteLine($"Min: {min} ms");
		Console.WriteLine($"P50 (Median): {p50} ms");
		Console.WriteLine($"P95: {p95} ms");
		Console.WriteLine($"P99: {p99} ms");
		Console.WriteLine($"Max: {max} ms");		
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
            var Writetasks = new List<Task>();
			

            foreach (var dataPoint in dataPoints)
            {	
				string key = String.Format("{0:MM-dd_HH-mm-ss-ffffff}",DateTime.Now);  
                Writetasks.Add(db.StringSetAsync(key,dataPoint,expiry)); // set expiry
				//Console.WriteLine(key);
			
            }
		
            await Task.WhenAll(Writetasks);
			        
    }
	private static async Task SimulateReadClient(int clientId)
    {
		
		var db = redisConnection.GetDatabase();
        var random = new Random();
		var times = DataPointsPerSecond;
		var Readtasks = new List<Task>();
        while (times>0)
        {
			
			times--;
            var key = keys[random.Next(keys.Count)];
            // var value = await db.StringGetAsync(key);


			Readtasks.Add(Task.Run(async () =>
			{
				var value = await db.StringGetAsync(key);
				// Console.WriteLine($"Retrieved value for key {key}: {value}");
			}));
			await Task.WhenAll(Readtasks);
			
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