namespace TableFlow.Web.Services;

// Singleton event bus for cross-circuit real-time updates in Blazor Server.
// All circuits share the same process, so events fired by one circuit (e.g. cashier opening
// a session) are received by all subscribed components in other circuits (e.g. the customer
// waiting screen). No extra packages or infrastructure needed.
public sealed class AppUpdateService
{
    // sessionId = -1 signals all subscribers to reload (used when sessionId is unknown)
    public event Action<int>? OrdersUpdated;

    // Fired when a cashier opens a session; menu pages waiting on this table activate.
    public event Action<int, int>? SessionOpened; // (tableNumber, sessionId)

    // Fired when a cashier closes a session; active menu pages switch to "ended" state.
    public event Action<int>? SessionClosed; // sessionId

    public void BroadcastOrdersUpdated(int sessionId = -1)
    {
        try { OrdersUpdated?.Invoke(sessionId); }
        catch { /* one bad subscriber must not kill others */ }
    }

    public void BroadcastSessionOpened(int tableNumber, int sessionId)
    {
        try { SessionOpened?.Invoke(tableNumber, sessionId); }
        catch { }
    }

    public void BroadcastSessionClosed(int sessionId)
    {
        try { SessionClosed?.Invoke(sessionId); }
        catch { }
    }
}
