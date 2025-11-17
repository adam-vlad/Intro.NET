using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Orders.Application.Abstractions;
using Orders.Application.Context;
using Orders.Application.DTOs;
using Orders.Application.Logging;
using Orders.Application.Requests;
using Orders.Domain.Entities;
using System.Diagnostics;

namespace Orders.Application.Handlers;

public class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly IMemoryCache _cache;
    private readonly IValidator<CreateOrderProfileRequest> _validator;
    private readonly ApplicationContext _context;

    public CreateOrderHandler(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<CreateOrderHandler> logger,
        IMemoryCache cache,
        IValidator<CreateOrderProfileRequest> validator,
        ApplicationContext context)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _validator = validator;
        _context = context;
    }

    public async Task<OrderProfileDto> HandleAsync(CreateOrderProfileRequest request)
    {
        var operationId = Guid.NewGuid().ToString()[..8];
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["OperationId"] = operationId });

        _logger.LogInformation(LogEvents.OrderCreationStarted, "order creation started: title={Title}, author={Author}, category={Category}, isbn={ISBN}", request.Title, request.Author, request.Category, request.ISBN);

        var totalStopwatch = Stopwatch.StartNew();

        var validationStopwatch = Stopwatch.StartNew();

        _logger.LogInformation(LogEvents.ISBNValidationPerformed, "performing isbn validation for isbn: {ISBN}", request.ISBN);
        _logger.LogInformation(LogEvents.StockValidationPerformed, "performing stock validation for quantity: {StockQuantity}", request.StockQuantity);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning(LogEvents.OrderValidationFailed, "order validation failed: {Errors}", errors);

            var metrics = new OrderCreationMetrics
            {
                OperationId = operationId,
                OrderTitle = request.Title,
                ISBN = request.ISBN,
                Category = request.Category.ToString(),
                ValidationDuration = validationStopwatch.ElapsedMilliseconds,
                DatabaseSaveDuration = 0,
                TotalDuration = totalStopwatch.ElapsedMilliseconds,
                Success = false,
                ErrorReason = errors
            };

            LoggingExtensions.LogOrderCreationMetrics(_logger, metrics);

            throw new ValidationException(validationResult.Errors);
        }

        validationStopwatch.Stop();

        var order = _mapper.Map<Order>(request);

        var dbStopwatch = Stopwatch.StartNew();
        _logger.LogInformation(LogEvents.DatabaseOperationStarted, "starting database save for order: {OrderId}", order.Id);

        await _repository.AddAsync(order);

        dbStopwatch.Stop();
        _logger.LogInformation(LogEvents.DatabaseOperationCompleted, "database save completed for order: {OrderId}", order.Id);

        _cache.Remove("all_orders");
        _logger.LogInformation(LogEvents.CacheOperationPerformed, "cache invalidated: key=all_orders");

        _context.IncrementTodayOrderCount();

        var dto = _mapper.Map<OrderProfileDto>(order);

        totalStopwatch.Stop();

        var successMetrics = new OrderCreationMetrics
        {
            OperationId = operationId,
            OrderTitle = request.Title,
            ISBN = request.ISBN,
            Category = request.Category.ToString(),
            ValidationDuration = validationStopwatch.ElapsedMilliseconds,
            DatabaseSaveDuration = dbStopwatch.ElapsedMilliseconds,
            TotalDuration = totalStopwatch.ElapsedMilliseconds,
            Success = true
        };

        LoggingExtensions.LogOrderCreationMetrics(_logger, successMetrics);
        _logger.LogInformation(LogEvents.OrderCreationCompleted, "order creation completed: orderid={OrderId}, title={Title}", order.Id, order.Title);

        return dto;
    }
}