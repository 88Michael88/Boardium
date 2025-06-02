using QRCoder;
using SkiaSharp;

namespace Boardium.Services;

public class QrCodeService
{
    public byte[] GenerateQrCodeBytes(string text)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        var moduleMatrix = qrCodeData.ModuleMatrix;

        int pixelsPerModule = 20;
        int size = moduleMatrix.Count * pixelsPerModule;

        using var bitmap = new SKBitmap(size, size);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var paint = new SKPaint { Color = SKColors.Black };
        for (int y = 0; y < moduleMatrix.Count; y++)
        {
            for (int x = 0; x < moduleMatrix.Count; x++)
            {
                if (moduleMatrix[y][x])
                {
                    var rect = new SKRect(x * pixelsPerModule, y * pixelsPerModule, (x + 1) * pixelsPerModule, (y + 1) * pixelsPerModule);
                    canvas.DrawRect(rect, paint);
                }
            }
        }

        canvas.Flush();

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }

    public string GenerateQrCodeBase64(string text)
    {
        var bytes = GenerateQrCodeBytes(text);
        return Convert.ToBase64String(bytes);
    }

}