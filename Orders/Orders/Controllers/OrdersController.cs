using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Orders.Application.Abstractions;
using Orders.Application.DTOs;
using Orders.Application.Handlers;
using Orders.Application.Requests;

namespace Orders.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly CreateOrderHandler _handler;
    private readonly IMemoryCache _cache;
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public OrdersController(CreateOrderHandler handler, IMemoryCache cache, IOrderRepository repository, IMapper mapper)
    {
        _handler = handler;
        _cache = cache;
        _repository = repository;
        _mapper = mapper;
    }

    [HttpPost]
    public ActionResult<OrderProfileDto> Create([FromBody] CreateOrderProfileRequest request)
    {
        var dto = _handler.Handle(request);
        return Ok(dto);
    }

    [HttpGet]
    public ActionResult<List<OrderProfileDto>> GetAll()
    {
        if (!_cache.TryGetValue("all_orders", out List<OrderProfileDto>? cached))
        {
            var items = _repository.GetAll();
            var list = _mapper.Map<List<OrderProfileDto>>(items);
            var options = new MemoryCacheEntryOptions();
            _cache.Set("all_orders", list, options);
            cached = list;
        }
        return Ok(cached);
    }
}