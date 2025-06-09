using QRCoder;
using System.Drawing.Imaging;

namespace WebApplication1.ProjectSERVICES
{
    public class QrService
    {
        public void GenerateQrCode(string url)
        {
            string baseDir = Directory.GetCurrentDirectory();
            string resourceDir = Path.Combine(baseDir, "Resources");

            // Upewnij się, że folder Resources istniej

            // Pełna ścieżka do pliku logo
            string logoPath = Path.Combine(resourceDir, "LOGO.png");

            if (!File.Exists(logoPath))
            {
                throw new FileNotFoundException("Nie znaleziono pliku logo.PNG", logoPath);
            }

            // Ścieżka do wygenerowanego pliku QR
            string outputPath = Path.Combine(resourceDir, "QR.PNG");

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

            Bitmap icon = (Bitmap)Image.FromFile(logoPath);

            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(
                pixelsPerModule: 5,
                darkColor: Color.FromArgb(0, 0, 0),
                lightColor: Color.FromArgb(255, 255, 255),
                icon: icon,
                iconSizePercent: 10,
                iconBorderWidth: 20,
                drawQuietZones: true
            );

            qrCodeImage.Save(outputPath, ImageFormat.Png);
        }
    }
}
