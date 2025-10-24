using AutoMapper;
using Orders.Application.DTOs;
using Orders.Domain.Entities;

namespace Orders.Application.Mapping.Resolvers;

public class AuthorInitialsResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        var name = (source.Author ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length >= 2)
        {
            var first = char.ToUpperInvariant(parts[0][0]);
            var last = char.ToUpperInvariant(parts[^1][0]);
            return new string(new[] { first, last });
        }
        var single = char.ToUpperInvariant(parts[0][0]);
        return single.ToString();
    }
}