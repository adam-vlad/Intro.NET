using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;
using Orders.Application.Logging;
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

        var operationId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var scopeData = new Dictionary<string, object>
        {
            ["OperationId"] = operationId
        };

        using var scope = _logger.BeginScope(scopeData);

        var start = DateTime.UtcNow;
        TimeSpan validationDuration = TimeSpan.Zero;
        TimeSpan dbDuration = TimeSpan.Zero;

        _logger.LogInformation(
            new EventId(LogEvents.OrderCreationStarted, "order_start"),
            "order creation started title={Title} author={Author} category={Category} isbn={ISBN} op={OperationId}",
            request.Title,
            request.Author,
            request.Category,
            request.ISBN,
            operationId
        );

        try
        {
            var vStart = DateTime.UtcNow;

            _logger.LogInformation(
                new EventId(LogEvents.ISBNValidationPerformed, "isbn_validation"),
                "isbn validation performed isbn={ISBN} op={OperationId}",
                request.ISBN,
                operationId
            );

            var exists = _repository.ExistsByIsbn(request.ISBN);

            _logger.LogInformation(
                new EventId(LogEvents.StockValidationPerformed, "stock_validation"),
                "stock validation performed stock={Stock} op={OperationId}",
                request.StockQuantity,
                operationId
            );

            var vEnd = DateTime.UtcNow;
            validationDuration = vEnd - vStart;

            if (exists)
            {
                var totalFail = DateTime.UtcNow - start;

                _logger.LogWarning(
                    new EventId(LogEvents.OrderValidationFailed, "validation_failed"),
                    "validation failed title={Title} isbn={ISBN} category={Category} reason={Reason} op={OperationId}",
                    request.Title,
                    request.ISBN,
                    request.Category,
                    "isbn already exists",
                    operationId
                );

                var failMetrics = new OrderCreationMetrics
                {
                    OperationId = operationId,
                    OrderTitle = request.Title,
                    ISBN = request.ISBN,
                    Category = request.Category,
                    ValidationDuration = validationDuration,
                    DatabaseSaveDuration = dbDuration,
                    TotalDuration = totalFail,
                    Success = false,
                    ErrorReason = "isbn already exists"
                };
                _logger.LogOrderCreationMetrics(failMetrics);

                throw new InvalidOperationException("isbn already exists");
            }

            var order = _mapper.Map<Order>(request);

            var dbStart = DateTime.UtcNow;

            _logger.LogInformation(
                new EventId(LogEvents.DatabaseOperationStarted, "db_start"),
                "database operation started orderId={OrderId} op={OperationId}",
                order.Id,
                operationId
            );

            _repository.Add(order);

            var dbEnd = DateTime.UtcNow;
            dbDuration = dbEnd - dbStart;

            _logger.LogInformation(
                new EventId(LogEvents.DatabaseOperationCompleted, "db_completed"),
                "database operation completed orderId={OrderId} op={OperationId}",
                order.Id,
                operationId
            );

            _cache.Remove("all_orders");

            _logger.LogInformation(
                new EventId(LogEvents.CacheOperationPerformed, "cache_op"),
                "cache operation performed key={Key} op={OperationId}",
                "all_orders",
                operationId
            );

            var dto = _mapper.Map<OrderProfileDto>(order);

            var total = DateTime.UtcNow - start;

            var successMetrics = new OrderCreationMetrics
            {
                OperationId = operationId,
                OrderTitle = request.Title,
                ISBN = request.ISBN,
                Category = request.Category,
                ValidationDuration = validationDuration,
                DatabaseSaveDuration = dbDuration,
                TotalDuration = total,
                Success = true,
                ErrorReason = null
            };
            _logger.LogOrderCreationMetrics(successMetrics);

            _logger.LogInformation(
                new EventId(LogEvents.OrderCreationCompleted, "order_completed"),
                "order creation completed orderId={OrderId} title={Title} isbn={ISBN} category={Category} op={OperationId}",
                order.Id,
                request.Title,
                request.ISBN,
                request.Category,
                operationId
            );

            return dto;
        }
        catch (Exception ex)
        {
            var totalError = DateTime.UtcNow - start;

            var errorMetrics = new OrderCreationMetrics
            {
                OperationId = operationId,
                OrderTitle = request.Title,
                ISBN = request.ISBN,
                Category = request.Category,
                ValidationDuration = validationDuration,
                DatabaseSaveDuration = dbDuration,
                TotalDuration = totalError,
                Success = false,
                ErrorReason = ex.Message
            };
            _logger.LogOrderCreationMetrics(errorMetrics);

            throw;
        }
    }
}