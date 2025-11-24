using Npgsql;

namespace DotNetAPI;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public ProductDto GetProduct(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT id, name, number FROM products WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new ProductDto
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Number = reader.GetInt32(2)
            };
        }

        throw new Exception("Product not found");
    }


    public int AddProduct(int id, string name, int number)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(@"
            INSERT INTO products (id, name, number)
            VALUES (@id, @name, @number)
            ON CONFLICT (id) DO NOTHING", conn);

        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("name", name);
        cmd.Parameters.AddWithValue("number", number);

        return cmd.ExecuteNonQuery();
    }

    public int UpdateProductNumber(int id, int addValue)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(
            "UPDATE products SET number = number + @addValue WHERE id = @id", conn);

        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("addValue", addValue);

        return cmd.ExecuteNonQuery();
    }

    public int DeleteProduct(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("DELETE FROM products WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);

        return cmd.ExecuteNonQuery();
    }

    public void CleanUp()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("DELETE FROM products", conn);
        cmd.ExecuteNonQuery();
    }
}

