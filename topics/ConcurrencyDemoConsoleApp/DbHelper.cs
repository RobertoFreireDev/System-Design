using Npgsql;

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

    public void CleanUp()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(@"
            DELETE FROM item_updates_log;
            DELETE FROM items;", conn);

        int affectedRows = cmd.ExecuteNonQuery();
    }

    public int OptimisticConcurrencyUpdateItem(int id, int value, int addValue, int version)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(
            "UPDATE items SET value = @value, version = @version + 1 WHERE id = @id AND version = @version", conn);

        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("value", value + addValue);
        cmd.Parameters.AddWithValue("version", version);

        return cmd.ExecuteNonQuery();
    }

    public int PessimisticConcurrencyUpdateItem(int id, int addValue)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        // Start a transaction to hold the lock
        using var transaction = conn.BeginTransaction();

        // Step 1: Lock the row
        using var selectCmd = new NpgsqlCommand(
            "SELECT value, version FROM items WHERE id = @id FOR UPDATE", conn, transaction);
        selectCmd.Parameters.AddWithValue("id", id);

        using var reader = selectCmd.ExecuteReader();
        if (!reader.Read())
            return -1;

        int currentValue = reader.GetInt32(0);
        int currentVersion = reader.GetInt32(1);
        reader.Close(); // Important: close reader before executing another command

        // Step 2: Update the row safely
        using var updateCmd = new NpgsqlCommand(
            "UPDATE items SET value = @value, version = version + 1 WHERE id = @id", conn, transaction);
        updateCmd.Parameters.AddWithValue("id", id);
        updateCmd.Parameters.AddWithValue("value", currentValue + addValue);

        int rowsAffected = updateCmd.ExecuteNonQuery();

        // Step 3: Commit the transaction (releases the lock)
        transaction.Commit();

        return rowsAffected;
    }

    public int UpdateItem(int id, int value, int addValue, int version)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(
            "UPDATE items SET value = @value, version = @version + 1 WHERE id = @id", conn);

        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("value", value + addValue);
        cmd.Parameters.AddWithValue("version", version);

        return cmd.ExecuteNonQuery();
    }

    public int AddItem(int id, int initialValue = 0)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(
            "INSERT INTO items (id, value, version) VALUES (@id, @value, 0) " +
            "ON CONFLICT (id) DO NOTHING", conn);

        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("value", initialValue);

        return cmd.ExecuteNonQuery();
    }
}