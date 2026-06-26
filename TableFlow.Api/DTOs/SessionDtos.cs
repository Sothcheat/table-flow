namespace TableFlow.Api.DTOs
{
    public record SessionResponse(
        int Id,
        int TableId,
        int TableNumber,
        string SessionStatus,
        string? PaymentMethod,
        decimal? TotalAmount,
        decimal? AmountReceived,
        DateTime OpenedAt,
        DateTime? ClosedAt,
        string CreatedById,
        string CreatedByName
    );

    public record CreateSessionRequest(
        int TableId
    );

    // Returned when a customer scans a static table QR (/menu?t={token}).
    // SessionId is null when the table has no open session yet (waiting screen).
    public record TableTokenResolveResponse(
        int TableNumber,
        bool IsOpen,
        int? SessionId
    );

    public record CloseSessionRequest(
        string PaymentMethod,
        decimal AmountReceived
    );

    public record SessionStatsResponse(
        decimal TodayRevenue,
        int TodayClosedSessions,
        int OpenSessions,
        List<TopItemResponse> TopItems
    );

    public record TopItemResponse(
        string ItemName,
        int TotalQuantity
    );

    public record SessionListResponse(
        List<SessionResponse> Items,
        int TotalCount,
        decimal TotalRevenue,
        int OpenCount,
        int CashCount,
        int KhqrCount
    );
}
