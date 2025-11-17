using FluentValidation;
using Orders.Application.Abstractions;
using Orders.Application.Context;
using Orders.Application.Handlers;
using Orders.Application.Mapping;
using Orders.Application.Repositories;
using Orders.Application.Requests;
using Orders.Application.Validation;
using Orders.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Orders API",
        Version = "v1",
        Description = "API for managing orders with advanced validation and mapping"
    });
});

builder.Services.AddAutoMapper(typeof(AdvancedOrderMappingProfile).Assembly);
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<ApplicationContext>();
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<IValidator<CreateOrderProfileRequest>, CreateOrderProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderProfileValidator>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationMiddleware>();

app.MapControllers();

app.Run();