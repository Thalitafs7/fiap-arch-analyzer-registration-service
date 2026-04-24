namespace CrossService.IntegrationTests.Common;

public static class AsyncHelper
{
    public static async Task<bool> WaitForConditionAsync(
        Func<Task<bool>> condition,
        TimeSpan? timeout = null,
        TimeSpan? interval = null)
    {
        var timeoutValue = timeout ?? TimeSpan.FromSeconds(30);
        var intervalValue = interval ?? TimeSpan.FromMilliseconds(500);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeoutValue)
        {
            if (await condition())
                return true;

            await Task.Delay(intervalValue);
        }

        return false;
    }

    public static async Task WaitAsync(TimeSpan duration)
    {
        await Task.Delay(duration);
    }

    public static async Task<T?> WaitForValueAsync<T>(
        Func<Task<T?>> getValue,
        Func<T?, bool> predicate,
        TimeSpan? timeout = null,
        TimeSpan? interval = null)
    {
        var timeoutValue = timeout ?? TimeSpan.FromSeconds(30);
        var intervalValue = interval ?? TimeSpan.FromMilliseconds(500);
        var startTime = DateTime.UtcNow;
        T? value = default;

        while (DateTime.UtcNow - startTime < timeoutValue)
        {
            value = await getValue();
            if (predicate(value))
                return value;

            await Task.Delay(intervalValue);
        }

        return value;
    }
}
