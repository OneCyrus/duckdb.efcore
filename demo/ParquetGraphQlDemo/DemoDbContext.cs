using Microsoft.EntityFrameworkCore;

public sealed class DemoDbContext(DbContextOptions<DemoDbContext> options, DemoOptions demoOptions) : DbContext(options)
{
    private readonly string _parquetRoot = demoOptions.ParquetRoot;

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var customersPath = Path.Combine(_parquetRoot, "customers.parquet").Replace("\\", "\\\\");
        var ordersPath = Path.Combine(_parquetRoot, "orders.parquet").Replace("\\", "\\\\");
        var orderLinesPath = Path.Combine(_parquetRoot, "order_lines.parquet").Replace("\\", "\\\\");

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasNoKey();
            entity.ToSqlQuery($"SELECT id, name, email FROM read_parquet('{customersPath}')");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasNoKey();
            entity.ToSqlQuery($"SELECT id, customer_id AS CustomerId, ordered_on AS OrderedOn, total_amount AS TotalAmount FROM read_parquet('{ordersPath}')");
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasNoKey();
            entity.ToSqlQuery($"SELECT id, order_id AS OrderId, product_name AS ProductName, quantity, line_total AS LineTotal FROM read_parquet('{orderLinesPath}')");
        });
    }
}
