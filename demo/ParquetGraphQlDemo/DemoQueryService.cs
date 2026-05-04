using Microsoft.EntityFrameworkCore;

public sealed class DemoQueryService(DemoDbContext context)
{
    public IQueryable<Customer> GetCustomers()
        => context.Customers.AsNoTracking();

    public IQueryable<Order> GetOrders()
        => context.Orders.AsNoTracking();

    public IQueryable<OrderLine> GetOrderLines()
        => context.OrderLines.AsNoTracking();
}
