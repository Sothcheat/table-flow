using QRCoder;

namespace TableFlow.Api.Services
{
    // Shared QR generation so both session and table endpoints encode URLs the same way.
    public static class QrCodeHelper
    {
        public static string GenerateBase64(string url)
        {
            using var qrCodeData = QRCodeGenerator.GenerateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(10);
            return Convert.ToBase64String(qrCodeBytes);
        }
    }
}
