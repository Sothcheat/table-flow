namespace TableFlow.Web.Services;

public class UnauthorizedNotifier
{
    public event Func<Task>? OnUnauthorized;

    public async Task NotifyAsync()
    {
        if (OnUnauthorized is not null)
            await OnUnauthorized.Invoke();
    }
}
