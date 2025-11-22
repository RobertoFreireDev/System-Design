using System.Diagnostics;

class Program
{
    public static int ConcurrencyType = 0;
    // 0 -> No Concurrency Control
    // 1 -> Optimistic Concurrency Control
    // 2 -> Pessimistic Concurrency Control

    static async Task Main()
    {
        Console.WriteLine("Select concurrency type:");
        Console.WriteLine("0 -> No Concurrency Control");
        Console.WriteLine("1 -> Optimistic Concurrency Control");
        Console.WriteLine("2 -> Pessimistic Concurrency Control");
        Console.Write("Enter choice (0/1/2): ");

        string? input = Console.ReadLine();

        if (!int.TryParse(input, out ConcurrencyType) || ConcurrencyType < 0 || ConcurrencyType > 2)
        {
            Console.WriteLine("Invalid choice. Defaulting to 0 (No Concurrency Control).");
            ConcurrencyType = 0;
        }
        else
        {
            Console.WriteLine($"Selected concurrency type: {ConcurrencyType}");
        }

        var connectionString = "Host=localhost;Port=8081;Username=simha;Password=Postgres2019!;Database=weather";
        var db = new DbHelper(connectionString);

        db.CleanUp();
        int itemId = 1;
        db.AddItem(itemId, 0);

        Console.WriteLine("Starting concurrent update attempts...");

        int taskCount = 100;
        var sw = Stopwatch.StartNew();

        var tasks = new List<Task>();

        var (value, version) = db.GetItem(itemId);

        for (int i = 1; i <= taskCount; i++)
        {
            string taskName = $"T{i}";
            tasks.Add(UpdateItemSimAsync(db, itemId, value, version, taskName));
        }

        await Task.WhenAll(tasks);

        sw.Stop();
        Console.WriteLine($"All {taskCount} tasks completed in {sw.ElapsedMilliseconds} ms");
    }

    static async Task UpdateItemSimAsync(DbHelper db, int itemId, int value, int version, string taskName)
    {
        var rand = new Random();
        await Task.Delay(rand.Next(100, 500));
        var rows = 0;
        switch (ConcurrencyType)
        {
            case 0:
                rows = db.UpdateItem(itemId, value, 1, version);
                break;
            case 1:
                rows = db.OptimisticConcurrencyUpdateItem(itemId, value, 1, version);
                break;
            case 2:
                rows = db.PessimisticConcurrencyUpdateItem(itemId, 1);
                break;
        }
    }
}