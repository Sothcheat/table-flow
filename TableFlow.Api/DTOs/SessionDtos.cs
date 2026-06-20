namespace TableFlow.Api.DTOs
{
    public record SessionResponse(
        int Id,
        int TableId,
        int TableNumber,
        string SessionStatus,
        string? PaymentMethod,
        decimal? TotalAmount,
        DateTime OpenedAt,
        DateTime? ClosedAt,
        string CreatedById,
        string CreatedByName,
        string? QrCodeBase64
    );

    public record CreateSessionRequest(
        int TableId
    );

    public record CloseSessionRequest(
        string PaymentMethod
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
}
