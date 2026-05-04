using DuckDB.EFCore.Metadata;

[Parquet("data/customers.parquet")]
public sealed record Customer
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IReadOnlyList<Order> Orders { get; init; } = [];
}

[Parquet("data/orders.parquet")]
public sealed record Order
{
    public int Id { get; init; }
    public int CustomerId { get; init; }
    public DateOnly OrderedOn { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<OrderLine> Lines { get; init; } = [];
}

[Parquet("data/order_lines.parquet")]
public sealed record OrderLine
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}
