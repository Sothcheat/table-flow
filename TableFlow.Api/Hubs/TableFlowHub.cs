using Microsoft.AspNetCore.SignalR;

namespace TableFlow.Api.Hubs
{
    public class TableFlowHub : Hub
    {
        // Server pushes events to clients — no client-callable methods needed.
        // Events broadcast:
        //   "SessionOpened"   (string tablePublicToken, int sessionId, int tableNumber)
        //   "SessionClosed"   (int sessionId)
        //   "OrdersUpdated"   (int sessionId)
    }
}
