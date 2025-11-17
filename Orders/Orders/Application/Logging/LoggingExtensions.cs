namespace Orders.Application.Logging;

public static class LoggingExtensions
{
    public static void LogOrderCreationMetrics(ILogger logger, OrderCreationMetrics metrics)
    {
        if (metrics.Success)
        {
            logger.LogInformation(
                LogEvents.OrderCreationCompleted,
                "order creation metrics: title={Title}, isbn={ISBN}, category={Category}, validation={ValidationMs}ms, database={DatabaseMs}ms, total={TotalMs}ms, success={Success}",
                metrics.OrderTitle,
                metrics.ISBN,
                metrics.Category,
                metrics.ValidationDuration,
                metrics.DatabaseSaveDuration,
                metrics.TotalDuration,
                metrics.Success
            );
        }
        else
        {
            logger.LogWarning(
                LogEvents.OrderValidationFailed,
                "order creation metrics: title={Title}, isbn={ISBN}, category={Category}, validation={ValidationMs}ms, database={DatabaseMs}ms, total={TotalMs}ms, success={Success}, error={Error}",
                metrics.OrderTitle,
                metrics.ISBN,
                metrics.Category,
                metrics.ValidationDuration,
                metrics.DatabaseSaveDuration,
                metrics.TotalDuration,
                metrics.Success,
                metrics.ErrorReason
            );
        }
    }
}