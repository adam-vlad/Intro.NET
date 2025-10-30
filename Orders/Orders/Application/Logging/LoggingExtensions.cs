using Microsoft.Extensions.Logging;

namespace Orders.Application.Logging;

public static class LoggingExtensions
{
    public static void LogOrderCreationMetrics(this ILogger logger, OrderCreationMetrics metrics)
    {
        var validationMs = (int)metrics.ValidationDuration.TotalMilliseconds;
        var dbMs = (int)metrics.DatabaseSaveDuration.TotalMilliseconds;
        var totalMs = (int)metrics.TotalDuration.TotalMilliseconds;
        var eventId = metrics.Success ? LogEvents.OrderCreationCompleted : LogEvents.OrderValidationFailed;

        logger.LogInformation(
            new EventId(eventId, "order_metrics"),
            "order metrics op={OperationId} title={OrderTitle} isbn={ISBN} category={Category} validation_ms={ValidationMs} db_ms={DatabaseMs} total_ms={TotalMs} success={Success} error={Error}",
            metrics.OperationId,
            metrics.OrderTitle,
            metrics.ISBN,
            metrics.Category,
            validationMs,
            dbMs,
            totalMs,
            metrics.Success,
            metrics.ErrorReason ?? string.Empty
        );
    }
}