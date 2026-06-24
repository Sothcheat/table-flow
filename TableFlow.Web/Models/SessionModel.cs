namespace TableFlow.Web.Models;

public class CashierStatsModel
{
    public decimal TodayRevenue { get; set; }
    public int TodayClosedSessions { get; set; }
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
    public string? QrCodeBase64 { get; set; }  // ← add this
}
