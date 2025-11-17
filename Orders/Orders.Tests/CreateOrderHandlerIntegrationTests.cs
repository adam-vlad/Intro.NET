using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Application.Abstractions;
using Orders.Application.Context;
using Orders.Application.Handlers;
using Orders.Application.Logging;
using Orders.Application.Mapping;
using Orders.Application.Repositories;
using Orders.Application.Requests;
using Orders.Application.Validation;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Xunit;
using FluentAssertions;

namespace Orders.Tests;

public class CreateOrderHandlerIntegrationTests : IDisposable
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
    private readonly Mock<ILogger<CreateOrderProfileValidator>> _validatorLoggerMock;
    private readonly ApplicationContext _context;
    private readonly IValidator<CreateOrderProfileRequest> _validator;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerIntegrationTests()
    {
        _repository = new InMemoryOrderRepository();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AdvancedOrderMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _cache = new MemoryCache(new MemoryCacheOptions());

        _loggerMock = new Mock<ILogger<CreateOrderHandler>>();
        _validatorLoggerMock = new Mock<ILogger<CreateOrderProfileValidator>>();

        _context = new ApplicationContext();

        _validator = new CreateOrderProfileValidator(_context, _repository, _validatorLoggerMock.Object);

        _handler = new CreateOrderHandler(_repository, _mapper, _loggerMock.Object, _cache, _validator, _context);
    }

    [Fact]
    public async Task Handle_ValidTechnicalOrderRequest_CreatesOrderWithCorrectMappings()
    {
        var request = new CreateOrderProfileRequest
        {
            Title = "advanced software engineering algorithms",
            Author = "john smith",
            ISBN = "978-0-123456-47-2",
            Category = OrderCategory.Technical,
            Price = 45.99m,
            PublishedDate = DateTime.UtcNow.AddMonths(-6),
            CoverImageUrl = "https://example.com/cover.jpg",
            StockQuantity = 8
        };

        var result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.CategoryDisplayName.Should().Be("technical & professional");
        result.AuthorInitials.Should().Be("JS");
        result.PublishedAge.Should().Contain("months old");
        result.FormattedPrice.Should().StartWith("$");
        result.AvailabilityStatus.Should().Be("in stock");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                LogEvents.OrderCreationStarted,
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateISBN_ThrowsValidationExceptionWithLogging()
    {
        var existingOrder = new Order
        {
            Id = Guid.NewGuid(),
            Title = "existing book",
            Author = "jane doe",
            ISBN = "978-0-111111-11-1",
            Category = OrderCategory.Fiction,
            Price = 19.99m,
            PublishedDate = DateTime.UtcNow.AddYears(-1),
            IsAvailable = true,
            StockQuantity = 5,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(existingOrder);

        var request = new CreateOrderProfileRequest
        {
            Title = "new book",
            Author = "bob williams",
            ISBN = "978-0-111111-11-1",
            Category = OrderCategory.Fiction,
            Price = 25.99m,
            PublishedDate = DateTime.UtcNow.AddMonths(-3),
            StockQuantity = 3
        };

        Func<Task> act = async () => await _handler.HandleAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*already exists*");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                LogEvents.OrderValidationFailed,
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_ChildrensOrderRequest_AppliesDiscountAndConditionalMapping()
    {
        var request = new CreateOrderProfileRequest
        {
            Title = "fun adventure story",
            Author = "mary johnson",
            ISBN = "978-0-222222-22-2",
            Category = OrderCategory.Children,
            Price = 20.00m,
            PublishedDate = DateTime.UtcNow.AddMonths(-2),
            CoverImageUrl = "https://example.com/kids-cover.jpg",
            StockQuantity = 10
        };

        var result = await _handler.HandleAsync(request);

        result.Should().NotBeNull();
        result.CategoryDisplayName.Should().Be("children's orders");
        result.Price.Should().Be(18.00m);
        result.CoverImageUrl.Should().BeNull();
    }

    public void Dispose()
    {
        _cache?.Dispose();
    }
}