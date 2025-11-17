using AutoMapper;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Application.Mapping.Resolvers;

public class CategoryDisplayResolver : IValueResolver<Order, object, string>
{
    public string Resolve(Order source, object destination, string destMember, ResolutionContext context)
    {
        return source.Category switch
        {
            OrderCategory.Fiction => "fiction & literature",
            OrderCategory.NonFiction => "non-fiction",
            OrderCategory.Technical => "technical & professional",
            OrderCategory.Children => "children's orders",
            _ => "uncategorized"
        };
    }
}