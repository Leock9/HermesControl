using HermesControl.Consumer.Domain;
using HermesControl.Consumer.Domain.Gateways;
using Npgsql;
using System.Text.Json;

namespace HermesControl.Consumer.Infrastructure.OrderGateway.PostgreDb;

public class OrderGateway : IOrderGateway
{
    private readonly Context _context;

    public OrderGateway(Context context)
    {
        _context = context;
        EnsureTableExistsAsync().Wait();
    }

    public void Create(Order order)
    {
        var conn = _context.GetConnection();
        conn.Open();

        var jsonItemMenu = JsonSerializer.Serialize(order);
        using var cmd = new NpgsqlCommand("INSERT INTO orders (id, order_data) VALUES (@id, @data::jsonb)", conn);


        cmd.Parameters.AddWithValue("@id", order.Id);
        cmd.Parameters.AddWithValue("@data", jsonItemMenu);

        cmd.ExecuteNonQuery();
        conn.Close();
    }

    public async Task<IEnumerable<Order>> GetAll()
    {
        var orders = new List<Order>();

        await using var conn = _context.GetConnection();
        await conn.OpenAsync();

        const string commandText = "SELECT order_data FROM orders";
        await using var cmd = new NpgsqlCommand(commandText, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var orderJson = reader.GetString(0);
            var order = JsonSerializer.Deserialize<Order>(orderJson);
            if (order != null)
                orders.Add(order);
        }

        await conn.CloseAsync();
        return orders;
    }

    public async Task<Order> GetById(Guid id)
    {
        await using var conn = _context.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT order_data FROM orders WHERE order_data->>'Id' = @Id::text", conn);

        cmd.Parameters.Add(new NpgsqlParameter("Id", id));

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        var jsonData = reader.GetString(0);

        await conn.CloseAsync();
        return JsonSerializer.Deserialize<Order>(jsonData);
    }

    public async Task UpdateAsync(Order order)
    {
        await using var conn = _context.GetConnection();
        await conn.OpenAsync();

        var orderJson = JsonSerializer.Serialize(order);

        var commandText = @"
        UPDATE orders
        SET order_data = @OrderData::jsonb
        WHERE id = @Id;
        ";

        await using var cmd = new NpgsqlCommand(commandText, conn);

        cmd.Parameters.AddWithValue("@Id", order.Id);
        cmd.Parameters.AddWithValue("@OrderData", orderJson);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        await conn.CloseAsync();
        if (rowsAffected == 0)
            throw new InvalidOperationException($"Pedido com ID {order.Id} não encontrado para atualização.");
    }

    private async Task EnsureTableExistsAsync()
    {
        var conn = _context.GetConnection();
        conn.Open();

        using var cmd = new NpgsqlCommand
                (
                   "CREATE TABLE IF NOT EXISTS orders (id uuid PRIMARY KEY, order_data jsonb)",
                   conn
                );

        await cmd.ExecuteNonQueryAsync();
        conn.Close();
    }
}
