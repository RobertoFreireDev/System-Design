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
        Console.Write("Enter 1 or 2:");
        string input = Console.ReadLine();

        int port;
        switch (input)
        {
            case "1":
                port = dotnetPort;
                break;
            case "2":
                port = goPort;
                break;
            default:
                Console.WriteLine("Invalid option. Using default port 5000.");
                return;
        }
        
        string baseUrl = $"http://localhost:{port}/process/";
        int numberOfRequests = 1000;

        HttpClient client = new HttpClient();
        List<Task<double>> tasks = new List<Task<double>>();

        Stopwatch totalStopwatch = Stopwatch.StartNew();

        for (int i = 1; i <= numberOfRequests; i++)
        {
            int id = i; // capture loop variable
            tasks.Add(MakeRequestAsync(client, baseUrl, id));
        }

        double[] requestTimes = await Task.WhenAll(tasks);

        totalStopwatch.Stop();

        double totalTime = totalStopwatch.Elapsed.TotalSeconds;
        double averageTime = 0;
        if (requestTimes.Length > 0)
        {
            averageTime = ComputeAverage(requestTimes);
        }

        Console.WriteLine($"Total time for {numberOfRequests} requests: {totalTime:F2} seconds");
        Console.WriteLine($"Average time per request: {averageTime:F2} seconds");
    }

    static async Task<double> MakeRequestAsync(HttpClient client, string baseUrl, int id)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            HttpResponseMessage response = await client.GetAsync($"{baseUrl}{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request {id} failed: {ex.Message}");
        }

        stopwatch.Stop();
        return stopwatch.Elapsed.TotalSeconds;
    }

    static double ComputeAverage(double[] times)
    {
        double sum = 0;
        foreach (var t in times)
        {
            sum += t;
        }
        return sum / times.Length;
    }
}