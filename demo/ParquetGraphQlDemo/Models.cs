using DuckDB.EFCore.Metadata;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

[Parquet("data/orders.parquet")]
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }
    public DateOnly OrderedOn { get; set; }
    public decimal TotalAmount { get; set; }
    public virtual ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public virtual Order? Order { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
