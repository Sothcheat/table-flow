namespace TableFlow.Api.DTOs
{
    public record TableResponse(
        int Id,
        int TableNumber,
        string Status
    );

    public record CreateTableRequest(
        int TableNumber
    );

    public record UpdateTableRequest(
        int TableNumber
    );

    public record UpdateTableStatusRequest(string Status);
}