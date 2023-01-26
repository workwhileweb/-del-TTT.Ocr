using System.Drawing;
using System.Drawing.Imaging;
using Windows.Foundation;
using Windows.Globalization;
using TTT.Tesseract;
// ReSharper disable UnusedMember.Global

namespace TTT.Ocr;

using System.Linq;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

/// <summary>
/// https://github.com/workwhileweb/AutoNutri3/blob/master/TextSearch.cs
/// https://github.com/ShareX/ShareX/blob/develop/ShareX/Tools/OCR/OCRHelper.cs
/// https://www.nuget.org/packages/Microsoft.Windows.WinMD
/// https://learn.microsoft.com/en-us/uwp/api/windows.media.ocr?view=winrt-22621
/// https://devblogs.microsoft.com/dotnet/announcing-the-windows-compatibility-pack-for-net-core/
/// </summary>
public static class Helper
{
    private static readonly Language Language = new("en-US");
    private static readonly OcrEngine Engine = OcrEngine.TryCreateFromLanguage(Language);

    public static async Task<SoftwareBitmap> BitmapConvertAsync(this Bitmap bitmap)
    {
        using var stream = new InMemoryRandomAccessStream();
        bitmap.Save(stream.AsStream(), ImageFormat.Png);
        var decoder = await BitmapDecoder.CreateAsync(stream);
        return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
    }

    public static async Task<Page> Parse(this Bitmap bitmap)
    {
        using var softwareBitmap = await BitmapConvertAsync(bitmap);
        var ocrResult = await Engine.RecognizeAsync(softwareBitmap);
        var lines = ocrResult.Lines.Select(ln => ln.ToLine()).ToList();
        var rect = lines.Select(line => line.Rectangle).RectanglesUnion();
        var para = new Paragraph(lines, rect);
        var block = new Block(new[] { para }, rect);
        var page = new Page(new[] { block }, rect);
        return page;
    }
    
    public static Line ToLine(this OcrLine line)
    {
        return new Line(line.Words.Select(word => word.ToTextBox()).ToList(),
            line.Words.Select(word => word.BoundingRect).RectsUnion().ToRectangle());
    }

    public static TextBox ToTextBox(this OcrWord word)
    {
        return new TextBox(word.Text, word.BoundingRect.ToRectangle());
    }

    public static Rectangle ToRectangle(this Rect rect)
    {
        return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
    }

    public static Rectangle RectanglesUnion(this IEnumerable<Rectangle> rectangles)
    {
        return rectangles.Aggregate(Rectangle.Empty, Rectangle.Union);
    }

    public static Rect RectsUnion(this IEnumerable<Rect> rectangles)
    {
        var union = Rect.Empty;
        foreach (var word in rectangles) union.Union(word);
        return union;
    }
}