using AutoMapper;
using Orders.Application.DTOs;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Application.Mapping.Resolvers;

public class PriceFormatterResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        var effective = source.Category == OrderCategory.Children ? source.Price * 0.9m : source.Price;
        return effective.ToString("C2");
    }
}