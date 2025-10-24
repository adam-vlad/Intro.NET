using AutoMapper;
using Orders.Application.DTOs;
using Orders.Application.Mapping.Resolvers;
using Orders.Application.Requests;
using Orders.Domain.Entities;
using Orders.Domain.Enums;

namespace Orders.Application.Mapping;

public class AdvancedOrderMappingProfile : Profile
{
    public AdvancedOrderMappingProfile()
    {
        CreateMap<CreateOrderProfileRequest, Order>()
            .ForMember(d => d.Id, o => o.MapFrom(_ => Guid.NewGuid()))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.IsAvailable, o => o.MapFrom(s => s.StockQuantity > 0))
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        CreateMap<Order, OrderProfileDto>()
            .ForMember(d => d.CategoryDisplayName, o => o.MapFrom<CategoryDisplayResolver>())
            .ForMember(d => d.FormattedPrice, o => o.MapFrom<PriceFormatterResolver>())
            .ForMember(d => d.PublishedAge, o => o.MapFrom<PublishedAgeResolver>())
            .ForMember(d => d.AuthorInitials, o => o.MapFrom<AuthorInitialsResolver>())
            .ForMember(d => d.AvailabilityStatus, o => o.MapFrom<AvailabilityStatusResolver>())
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Category == OrderCategory.Children ? s.Price * 0.9m : s.Price))
            .ForMember(d => d.CoverImageUrl, o => o.MapFrom(s => s.Category == OrderCategory.Children ? null : s.CoverImageUrl));
    }
}