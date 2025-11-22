class Program
{
    static async Task Main()
    {
        var connectionString = "Host=localhost;Port=8081;Username=simha;Password=Postgres2019!;Database=weather";
        var db = new DbHelper(connectionString);

        int itemId = 1;
        db.AddItem(itemId, 0);

        Console.WriteLine("Starting concurrent update attempts...");

        Task t1 = UpdateItemSimAsync(db, itemId, "T1");
        Task t2 = UpdateItemSimAsync(db, itemId, "T2");
        Task t3 = UpdateItemSimAsync(db, itemId, "T3");
        Task t4 = UpdateItemSimAsync(db, itemId, "T4");

        await Task.WhenAll(t1, t2, t3, t4);

        Console.WriteLine("Done.");
    }

    static async Task UpdateItemSimAsync(DbHelper db, int itemId, string taskName)
    {
        var (value, version) = db.GetItem(itemId);
        Console.WriteLine($"{taskName} read value={value}, version={version}");

        var rand = new Random();
        await Task.Delay(rand.Next(100, 500));

        try
        {
            db.UpdateItem(itemId, rand.Next(1, 10), version + 1);
            Console.WriteLine($"{taskName} successfully updated item. Original value/version: {value}/{version}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{taskName} failed to update item: {ex.Message}");
        }
    }
}
