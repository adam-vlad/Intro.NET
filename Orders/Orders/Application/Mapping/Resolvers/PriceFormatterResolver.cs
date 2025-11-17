using AutoMapper;
using Orders.Domain.Entities;
using System.Globalization;

namespace Orders.Application.Mapping.Resolvers;

public class PriceFormatterResolver : IValueResolver<Order, object, string>
{
    public string Resolve(Order source, object destination, string destMember, ResolutionContext context)
    {
        var effectivePrice = source.Category == Domain.Enums.OrderCategory.Children
            ? source.Price * 0.9m
            : source.Price;

        return effectivePrice.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
    }
}