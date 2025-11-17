using AutoMapper;
using Orders.Domain.Entities;

namespace Orders.Application.Mapping.Resolvers;

public class AvailabilityStatusResolver : IValueResolver<Order, object, string>
{
    public string Resolve(Order source, object destination, string destMember, ResolutionContext context)
    {
        if (!source.IsAvailable)
            return "out of stock";

        if (source.StockQuantity == 0)
            return "unavailable";

        if (source.StockQuantity == 1)
            return "last copy";

        if (source.StockQuantity <= 5)
            return "limited stock";

        return "in stock";
    }
}