using AutoMapper;
using Orders.Application.DTOs;
using Orders.Domain.Entities;

namespace Orders.Application.Mapping.Resolvers;

public class PublishedAgeResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        var span = DateTime.UtcNow - source.PublishedDate;
        var days = (int)span.TotalDays;
        if (days < 30) return "New Release";
        if (days < 365)
        {
            var months = days / 30;
            if (months < 1) months = 1;
            return months + " months old";
        }
        if (days == 1825) return "Classic";
        if (days < 1825)
        {
            var years = days / 365;
            if (years < 1) years = 1;
            return years + " years old";
        }
        var y = days / 365;
        if (y < 1) y = 1;
        return y + " years old";
    }
}