using Orders.Application.Abstractions;
using Orders.Domain.Entities;

namespace Orders.Application.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new List<Order>();

    public bool ExistsByIsbn(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn)) return false;
        for (int i = 0; i < _orders.Count; i++)
        {
            var x = _orders[i];
            if (string.Equals(x.ISBN, isbn, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    public void Add(Order order)
    {
        _orders.Add(order);
    }

    public IEnumerable<Order> GetAll()
    {
        return _orders;
    }
}