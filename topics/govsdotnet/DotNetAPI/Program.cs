using DotNetAPI;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Host=postgres-db;Port=5432;Username=simha;Password=Postgres2019!;Database=weather";
builder.Services.AddSingleton(new ProductRepository(connectionString));

builder.Services.AddHttpClient("ProductApi", client =>
{
    client.BaseAddress = new Uri("http://product-api:3000"); // docker network
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.MapGet("/process/{id}", async (
    int id,
    ProductRepository db,
    IHttpClientFactory httpClientFactory
) =>
{
    // 1 - Get local product
    ProductDto localProduct;
    try
    {
        localProduct = db.GetProduct(id);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.NotFound($"Local product with id {id} not found.");
    }

    if (false)
    {
        // 2 - Call remote Product API
        var client = httpClientFactory.CreateClient("ProductApi");
        var response = await client.GetAsync($"/product/{id}");
        if (!response.IsSuccessStatusCode)
            return Results.Problem("Remote Product API request failed");

        var remoteProduct = await response.Content.ReadFromJsonAsync<ProductDto>();
        if (remoteProduct == null)
            return Results.Problem("Failed to parse remote Product API response");

        // 3 - Increment product number 10k times
        for (int i = 0; i < 10_000; i++)
            remoteProduct.Number++;
    }

    // 4 - Return combined result
    return Results.Ok(new
    {
        LocalProduct = new ProductDto
        {
            Id = localProduct.Id,
            Name = localProduct.Name,
            Number = localProduct.Number
        },
        //RemoteProduct = remoteProduct
    });
});

app.UseCors("AllowAll");

app.Run();