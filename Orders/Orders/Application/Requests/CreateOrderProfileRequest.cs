using Orders.Application.Validation.CustomAttributes;
using Orders.Domain.Enums;

namespace Orders.Application.Requests;

public class CreateOrderProfileRequest
{
    public string Title { get; set; } = string.Empty;
    
    public string Author { get; set; } = string.Empty;
    
    [ValidISBN]
    public string ISBN { get; set; } = string.Empty;
    
    [OrderCategory(OrderCategory.Fiction, OrderCategory.NonFiction, OrderCategory.Technical, OrderCategory.Children)]
    public OrderCategory Category { get; set; }
    
    [PriceRange(0.01, 9999.99)]
    public decimal Price { get; set; }
    
    public DateTime PublishedDate { get; set; }
    
    public string? CoverImageUrl { get; set; }
    
    public int StockQuantity { get; set; } = 1;
}