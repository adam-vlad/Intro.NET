using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;
using Orders.Application.Handlers;
using Orders.Application.Requests;

namespace Orders.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly CreateOrderHandler _handler;
    private readonly IOrderRepository _repository;
    private readonly IMemoryCache _cache;

    public OrdersController(CreateOrderHandler handler, IOrderRepository repository, IMemoryCache cache)
    {
        _handler = handler;
        _repository = repository;
        _cache = cache;
    }

    [HttpPost]
    public async Task<ActionResult<OrderProfileDto>> CreateOrder([FromBody] CreateOrderProfileRequest request)
    {
        var result = await _handler.HandleAsync(request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderProfileDto>>> GetOrders()
    {
        if (_cache.TryGetValue("all_orders", out IEnumerable<OrderProfileDto>? cachedOrders) && cachedOrders != null)
        {
            return Ok(cachedOrders);
        }

        var orders = await _repository.GetAllAsync();
        return Ok(orders);
    }
}