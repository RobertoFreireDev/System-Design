using System.Diagnostics;

class Program
{
    static async Task Main()
    {
        const int dotnetPort = 3001;
        const int goPort = 3002;

        Console.WriteLine("Choose an option:");
        Console.WriteLine("1 - Use dotnet");
        Console.WriteLine("2 - Use go");
        Console.Write("Enter 1 or 2: ");
        string input = Console.ReadLine();

        int port = input switch
        {
            "1" => dotnetPort,
            "2" => goPort,
            _ => -1
        };

        if (port == -1)
        {
            Console.WriteLine("Invalid option. Exiting.");
            return;
        }

        string baseUrl = $"http://localhost:{port}/process/";
        HttpClient client = new HttpClient();

        // First batch
        Console.WriteLine("\n=== Running first 10 requests ===");
        await RunBatch(client, baseUrl, 10);

        // Second batch
        Console.WriteLine("\n=== Running second 10 requests ===");
        await RunBatch(client, baseUrl, 10);

        // Thrid batch
        Console.WriteLine("\n=== Running thrid 10 requests ===");
        await RunBatch(client, baseUrl, 10);

        // Forth batch
        Console.WriteLine("\n=== Running forth 10 requests ===");
        await RunBatch(client, baseUrl, 10);
    }

    static async Task RunBatch(HttpClient client, string baseUrl, int numberOfRequests)
    {
        List<Task<double>> tasks = new List<Task<double>>();
        Stopwatch totalStopwatch = Stopwatch.StartNew();

        for (int i = 1; i <= numberOfRequests; i++)
        {
            tasks.Add(MakeRequestAsync(client, baseUrl, 1));
        }

        double[] requestTimes = await Task.WhenAll(tasks);

        totalStopwatch.Stop();
        double totalTime = totalStopwatch.Elapsed.TotalSeconds;
        double averageTime = ComputeAverage(requestTimes);

        Console.WriteLine($"Total time for {numberOfRequests} requests: {totalTime:F2} seconds");
        Console.WriteLine($"Average time per request: {averageTime:F4} seconds");
    }

    static async Task<double> MakeRequestAsync(HttpClient client, string baseUrl, int id)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        string url = $"{baseUrl}{id}";

        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request failed ({url}): {ex.Message}");
        }

        stopwatch.Stop();
        return stopwatch.Elapsed.TotalSeconds;
    }

    static double ComputeAverage(double[] times)
    {
        double sum = 0;
        foreach (var t in times)
            sum += t;

        return sum / times.Length;
    }
}
