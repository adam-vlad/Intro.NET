using System;
using Orders.Domain.Enums;

namespace Orders.Application.Logging;

public class OrderCreationMetrics
{
    public string OperationId { get; set; } = string.Empty;
    public string OrderTitle { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public OrderCategory Category { get; set; }
    public TimeSpan ValidationDuration { get; set; }
    public TimeSpan DatabaseSaveDuration { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public bool Success { get; set; }
    public string? ErrorReason { get; set; }
}