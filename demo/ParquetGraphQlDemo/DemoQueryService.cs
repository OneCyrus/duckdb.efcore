using Microsoft.EntityFrameworkCore;

public sealed class DemoQueryService(IDbContextFactory<DemoDbContext> contextFactory)
{
    public async Task<IReadOnlyList<Customer>> GetCustomersAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var customers = await context.Customers.AsNoTracking().ToListAsync();
        var orders = await context.Orders.AsNoTracking().ToListAsync();
        var lines = await context.OrderLines.AsNoTracking().ToListAsync();

        var linesByOrder = lines
            .GroupBy(line => line.OrderId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<OrderLine>)group.ToList());

        var ordersByCustomer = orders
            .Select(order => order with { Lines = linesByOrder.GetValueOrDefault(order.Id, []) })
            .GroupBy(order => order.CustomerId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<Order>)group.ToList());

        return customers
            .Select(customer => customer with { Orders = ordersByCustomer.GetValueOrDefault(customer.Id, []) })
            .ToList();
    }

    public async Task<IReadOnlyList<Order>> GetOrdersAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var orders = await context.Orders.AsNoTracking().ToListAsync();
        var lines = await context.OrderLines.AsNoTracking().ToListAsync();

        var linesByOrder = lines
            .GroupBy(line => line.OrderId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<OrderLine>)group.ToList());

        return orders
            .Select(order => order with { Lines = linesByOrder.GetValueOrDefault(order.Id, []) })
            .ToList();
    }

    public async Task<IReadOnlyList<OrderLine>> GetOrderLinesAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.OrderLines.AsNoTracking().ToListAsync();
    }
}
