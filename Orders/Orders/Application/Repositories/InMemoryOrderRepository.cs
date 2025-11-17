using Orders.Application.Abstractions;
using Orders.Domain.Entities;

namespace Orders.Application.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();

    public Task AddAsync(Order order)
    {
        _orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Order>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Order>>(_orders);
    }
}