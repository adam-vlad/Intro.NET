using Orders.Domain.Entities;

namespace Orders.Application.Abstractions;

public interface IOrderRepository
{
    bool ExistsByIsbn(string isbn);
    void Add(Order order);
    IEnumerable<Order> GetAll();
}