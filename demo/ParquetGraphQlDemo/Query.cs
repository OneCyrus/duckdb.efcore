public sealed class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Customer> Customers(DemoDbContext context) => context.Customers;

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Order> Orders(DemoDbContext context) => context.Orders;

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<OrderLine> OrderLines(DemoDbContext context) => context.OrderLines;
}
