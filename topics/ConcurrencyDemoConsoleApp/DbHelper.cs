using Npgsql;
using System.Collections.Generic;

public class DbHelper
{
    private readonly string _connectionString;

    public DbHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public (int value, int version) GetItem(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT value, version FROM items WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return (reader.GetInt32(0), reader.GetInt32(1));
        }

        throw new Exception("Item not found");
    }

    public int UpdateItem(int id, int value, int version)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(
            "UPDATE items SET value = @value, version = @version WHERE id = @id", conn);

        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("value", value);
        cmd.Parameters.AddWithValue("version", version);

        return cmd.ExecuteNonQuery();
    }

    public void AddItem(int id, int initialValue = 0)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(
            "INSERT INTO items (id, value, version) VALUES (@id, @value, 0) " +
            "ON CONFLICT (id) DO NOTHING", conn);

        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("value", initialValue);

        int rows = cmd.ExecuteNonQuery();
        if (rows > 0)
        {
            Console.WriteLine($"Item {id} added successfully.");
        }
        else
        {
            Console.WriteLine($"Item {id} already exists.");
        }
    }
}