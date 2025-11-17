namespace Orders.Application.Context;

public class ApplicationContext
{
    private readonly Dictionary<DateTime, int> _dailyOrderCount = new();
    private readonly object _lock = new();

    public int GetTodayOrderCount()
    {
        lock (_lock)
        {
            var today = DateTime.UtcNow.Date;
            return _dailyOrderCount.TryGetValue(today, out var count) ? count : 0;
        }
    }

    public void IncrementTodayOrderCount()
    {
        lock (_lock)
        {
            var today = DateTime.UtcNow.Date;
            if (_dailyOrderCount.ContainsKey(today))
            {
                _dailyOrderCount[today]++;
            }
            else
            {
                _dailyOrderCount[today] = 1;
            }
        }
    }
}