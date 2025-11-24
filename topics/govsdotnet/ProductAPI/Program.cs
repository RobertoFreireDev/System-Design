using ProductAPI;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/product/{id}", async (int id) =>
{
    await Task.Delay(500);

    var product = new ProductDto
    {
        Id = id,
        Name = $"Product-{id}",
        Number = 0
    };

    return Results.Ok(product);
});

app.Run();
