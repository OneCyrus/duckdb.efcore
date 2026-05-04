using DuckDB.NET.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(string parquetRoot)
    {
        var customersFile = System.IO.Path.Combine(parquetRoot, "customers.parquet");
        var ordersFile = System.IO.Path.Combine(parquetRoot, "orders.parquet");
        var orderLinesFile = System.IO.Path.Combine(parquetRoot, "order_lines.parquet");

        if (File.Exists(customersFile) && File.Exists(ordersFile) && File.Exists(orderLinesFile))
        {
            return;
        }

        await using var connection = new DuckDBConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            CREATE TABLE customers AS
            SELECT * FROM (
                VALUES
                    (1, 'Ada Lovelace', 'ada@example.com'),
                    (2, 'Grace Hopper', 'grace@example.com'),
                    (3, 'Donald Knuth', 'donald@example.com')
            ) AS t(Id, Name, Email);

            CREATE TABLE orders AS
            SELECT * FROM (
                VALUES
                    (101, 1, '2026-01-14'::DATE, 120.00),
                    (102, 1, '2026-02-19'::DATE, 62.50),
                    (103, 2, '2026-03-01'::DATE, 42.75)
            ) AS t(Id, CustomerId, OrderedOn, TotalAmount);

            CREATE TABLE order_lines AS
            SELECT * FROM (
                VALUES
                    (1001, 101, 'DuckDB mug', 2, 20.00),
                    (1002, 101, 'GraphQL book', 1, 80.00),
                    (1003, 102, 'Mechanical keyboard', 1, 62.50),
                    (1004, 103, 'Debug stickers', 3, 14.25)
            ) AS t(Id, OrderId, ProductName, Quantity, LineTotal);

            COPY customers TO '{customersFile.Replace("\\", "\\\\")}' (FORMAT PARQUET);
            COPY orders TO '{ordersFile.Replace("\\", "\\\\")}' (FORMAT PARQUET);
            COPY order_lines TO '{orderLinesFile.Replace("\\", "\\\\")}' (FORMAT PARQUET);
            """;

        await command.ExecuteNonQueryAsync();
    }
}
