using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp.Rendering;

namespace Boardium.Services;

public class BarcodeService
{
    public byte[] GenerateBarcode(string barcodeNumber)
    {
        var writer = new BarcodeWriter<SKBitmap>
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Height = 100,
                Width = 300,
                Margin = 10
            },
            Renderer = new SKBitmapRenderer()
        };

        var bitmap = writer.Write(barcodeNumber);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}