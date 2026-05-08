using System.Xml;
using DuckDB.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;

public sealed class DemoDbContext(DbContextOptions<DemoDbContext> options) : DbContext(options)
{
    private string path = "data/order_lines.parquet";
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Customer>()
            .FromParquet("data/customers.parquet")
            .HasMany(customer => customer.Orders)
            .WithOne(order => order.Customer)
            .HasForeignKey(order => order.CustomerId);

        modelBuilder
            .Entity<Order>()
            .HasMany(order => order.Lines)
            .WithOne(line => line.Order)
            .HasForeignKey(line => line.OrderId);

        modelBuilder.Entity<OrderLine>().FromParquet((sp) =>  {var t = sp.GetService<DemoDbContext>(); return path; });
    }
}
