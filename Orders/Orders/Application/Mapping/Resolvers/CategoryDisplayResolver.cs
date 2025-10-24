using AutoMapper;
using Orders.Application.DTOs;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Application.Mapping.Resolvers;

public class CategoryDisplayResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        if (source.Category == OrderCategory.Fiction) return "Fiction & Literature";
        if (source.Category == OrderCategory.NonFiction) return "Non-Fiction";
        if (source.Category == OrderCategory.Technical) return "Technical & Professional";
        if (source.Category == OrderCategory.Children) return "Children's Orders";
        return "Uncategorized";
    }
}