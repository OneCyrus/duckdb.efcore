using Microsoft.EntityFrameworkCore;

public sealed class DemoQueryService(IDbContextFactory<DemoDbContext> contextFactory)
{
    public async Task<IReadOnlyList<Customer>> GetCustomersAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var customers = await context.Customers.AsNoTracking().ToListAsync();
        var orders = await context.Orders.AsNoTracking().ToListAsync();
        var lines = await context.OrderLines.AsNoTracking().ToListAsync();

        var linesByOrder = lines.GroupBy(l => l.OrderId).ToDictionary(g => g.Key, g => (IReadOnlyList<OrderLine>)g.ToList());
        var ordersByCustomer = orders
            .Select(o => o with { Lines = linesByOrder.GetValueOrDefault(o.Id, []) })
            .GroupBy(o => o.CustomerId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<Order>)g.ToList());

        return customers
            .Select(c => c with { Orders = ordersByCustomer.GetValueOrDefault(c.Id, []) })
            .ToList();
    }

    public async Task<IReadOnlyList<Order>> GetOrdersAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var orders = await context.Orders.AsNoTracking().ToListAsync();
        var lines = await context.OrderLines.AsNoTracking().ToListAsync();
        var linesByOrder = lines.GroupBy(l => l.OrderId).ToDictionary(g => g.Key, g => (IReadOnlyList<OrderLine>)g.ToList());

        return orders.Select(o => o with { Lines = linesByOrder.GetValueOrDefault(o.Id, []) }).ToList();
    }

    public async Task<IReadOnlyList<OrderLine>> GetOrderLinesAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.OrderLines.AsNoTracking().ToListAsync();
    }
}
