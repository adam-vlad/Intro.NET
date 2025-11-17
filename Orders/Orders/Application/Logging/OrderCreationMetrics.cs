namespace Orders.Application.Logging;

public class OrderCreationMetrics
{
    public string OperationId { get; set; } = string.Empty;
    public string OrderTitle { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long ValidationDuration { get; set; }
    public long DatabaseSaveDuration { get; set; }
    public long TotalDuration { get; set; }
    public bool Success { get; set; }
    public string? ErrorReason { get; set; }
}