namespace TableFlow.Web.Models;

public class CashierStatsModel
{
    public decimal TodayRevenue { get; set; }
    public int TodayClosedSessions { get; set; }
}

public class SessionPagedResult
{
    public List<SessionModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OpenCount { get; set; }
    public int CashCount { get; set; }
    public int KhqrCount { get; set; }
}

public class SessionModel
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public int TableNumber { get; set; }
    public string SessionStatus { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public decimal? TotalAmount { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
}

// Result of resolving a static table QR token (/menu?t={token}).
// SessionId is null when the table has no open session yet.
public record TableResolveModel(int TableNumber, bool IsOpen, int? SessionId);

// Lifecycle of the customer menu after scanning a static table QR.
public enum MenuSessionState
{
    Loading,       // resolving the token
    InvalidToken,  // no/invalid token — show "scan the QR" message
    Waiting,       // valid table but no open session yet — poll until cashier opens
    Active,        // open session — show the menu and allow ordering
    Ended          // session was closed while browsing
}
