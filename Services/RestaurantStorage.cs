using Microsoft.Data.Sqlite;
using NET_MAUI_App.Data;

namespace NET_MAUI_App.Services;

public sealed record SavedOrderLine(string Name, int Quantity, decimal Price);
public sealed record SavedOrder(long Id, DateTime CreatedAt, int Persons, int TableNumber, string PaymentMethod, IReadOnlyList<SavedOrderLine> Lines)
{
    public decimal Total => Lines.Sum(line => line.Price * line.Quantity);
}

public static class RestaurantStorage
{
    private static SqliteConnection Open()
    {
        var connection = new SqliteConnection(Constants.DatabasePath);
        try
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS RestaurantCart (ItemId TEXT PRIMARY KEY, Quantity INTEGER NOT NULL CHECK(Quantity > 0));
                CREATE TABLE IF NOT EXISTS RestaurantDraft (Id INTEGER PRIMARY KEY AUTOINCREMENT, CreatedAt TEXT NOT NULL, Persons INTEGER NOT NULL, TableNumber INTEGER NOT NULL, PaymentMethod TEXT NOT NULL);
                CREATE TABLE IF NOT EXISTS RestaurantDraftLine (DraftId INTEGER NOT NULL, ItemId TEXT NOT NULL, Name TEXT NOT NULL, Quantity INTEGER NOT NULL, Price TEXT NOT NULL);
                """;
            command.ExecuteNonQuery();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    public static Dictionary<string, int> LoadCart()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ItemId, Quantity FROM RestaurantCart";
        using var reader = command.ExecuteReader();
        var cart = new Dictionary<string, int>();
        while (reader.Read()) cart.Add(reader.GetString(0), reader.GetInt32(1));
        return cart;
    }

    public static void SetQuantity(string id, int quantity)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = quantity == 0
            ? "DELETE FROM RestaurantCart WHERE ItemId = @id"
            : "INSERT INTO RestaurantCart (ItemId, Quantity) VALUES (@id, @quantity) ON CONFLICT(ItemId) DO UPDATE SET Quantity = excluded.Quantity";
        command.Parameters.AddWithValue("@id", id);
        if (quantity != 0) command.Parameters.AddWithValue("@quantity", quantity);
        command.ExecuteNonQuery();
    }

    public static void SaveDraft(int persons, int table, string payment, IReadOnlyList<RestaurantCartLine> cart)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "INSERT INTO RestaurantDraft (CreatedAt, Persons, TableNumber, PaymentMethod) VALUES (@date, @persons, @table, @payment); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("@date", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@persons", persons);
        command.Parameters.AddWithValue("@table", table);
        command.Parameters.AddWithValue("@payment", payment);
        var id = (long)command.ExecuteScalar()!;
        foreach (var line in cart)
        {
            using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = "INSERT INTO RestaurantDraftLine (DraftId, ItemId, Name, Quantity, Price) VALUES (@draft, @item, @name, @quantity, @price)";
            insert.Parameters.AddWithValue("@draft", id);
            insert.Parameters.AddWithValue("@item", line.Item.Id);
            insert.Parameters.AddWithValue("@name", line.Item.Name);
            insert.Parameters.AddWithValue("@quantity", line.Quantity);
            insert.Parameters.AddWithValue("@price", line.Item.Price.ToString(System.Globalization.CultureInfo.InvariantCulture));
            insert.ExecuteNonQuery();
        }
        transaction.Commit();
    }

    public static IReadOnlyDictionary<string, int> GetDraftCounts()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT ItemId, SUM(Quantity) FROM RestaurantDraftLine GROUP BY ItemId";
        using var reader = command.ExecuteReader();
        var counts = new Dictionary<string, int>();
        while (reader.Read()) counts.Add(reader.GetString(0), reader.GetInt32(1));
        return counts;
    }

    public static IReadOnlyList<SavedOrder> GetDrafts()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT d.Id, d.CreatedAt, d.Persons, d.TableNumber, d.PaymentMethod, l.Name, l.Quantity, l.Price FROM RestaurantDraft d JOIN RestaurantDraftLine l ON l.DraftId = d.Id ORDER BY d.Id DESC, l.rowid";
        using var reader = command.ExecuteReader();
        var orders = new List<SavedOrder>();
        List<SavedOrderLine>? lines = null;
        while (reader.Read())
        {
            var id = reader.GetInt64(0);
            if (orders.Count == 0 || orders[^1].Id != id)
            {
                lines = new List<SavedOrderLine>();
                orders.Add(new SavedOrder(id, DateTime.Parse(reader.GetString(1), null, System.Globalization.DateTimeStyles.RoundtripKind).ToLocalTime(), reader.GetInt32(2), reader.GetInt32(3), reader.GetString(4), lines));
            }
            lines!.Add(new SavedOrderLine(reader.GetString(5), reader.GetInt32(6), decimal.Parse(reader.GetString(7), System.Globalization.CultureInfo.InvariantCulture)));
        }
        return orders;
    }
}
