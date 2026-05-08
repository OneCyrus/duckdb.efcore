using Microsoft.EntityFrameworkCore;

public sealed class DemoQueryService
{
    private readonly DemoDbContext _context;

    public DemoQueryService(DemoDbContext context)
    {
        _context = context;
    }

    public IQueryable<Customer> GetCustomers()
        => _context.Customers;

    public IQueryable<Order> GetOrders()
        => _context.Orders;

    public IQueryable<OrderLine> GetOrderLines()
        => _context.OrderLines;
}
