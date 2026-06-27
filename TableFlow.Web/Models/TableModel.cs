namespace TableFlow.Web.Models
{
    public record TableModel(
        int Id,
        int TableNumber,
        string Status
    );

    // Static QR for a table — base64 PNG encoding /menu?t={token}.
    public record TableQrModel(int TableNumber, string QrCodeBase64);
}
