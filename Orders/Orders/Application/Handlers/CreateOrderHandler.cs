using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;
using Orders.Application.Requests;
using Orders.Domain.Entities;

namespace Orders.Application.Handlers;

public class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly IMemoryCache _cache;

    public CreateOrderHandler(IOrderRepository repository, IMapper mapper, ILogger<CreateOrderHandler> logger, IMemoryCache cache)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public OrderProfileDto Handle(CreateOrderProfileRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        _logger.LogInformation("creating order: title={Title} author={Author} category={Category} isbn={ISBN}", request.Title, request.Author, request.Category, request.ISBN);
        if (_repository.ExistsByIsbn(request.ISBN)) throw new InvalidOperationException("isbn already exists");
        var order = _mapper.Map<Order>(request);
        _repository.Add(order);
        _cache.Remove("all_orders");
        var dto = _mapper.Map<OrderProfileDto>(order);
        return dto;
    }
}