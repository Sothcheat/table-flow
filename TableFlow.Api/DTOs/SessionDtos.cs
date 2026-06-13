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
        string CreatedByName
    );

    public record CreateSessionRequest(
        int TableId
    );

    public record CloseSessionRequest(
        string PaymentMethod
    );
}