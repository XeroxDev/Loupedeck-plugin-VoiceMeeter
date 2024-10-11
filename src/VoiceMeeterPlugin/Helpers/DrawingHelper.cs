namespace Loupedeck.VoiceMeeterPlugin.Helpers
{
    using System.Globalization;

    using SkiaSharp;

    public static class DrawingHelper
    {
        private static readonly String RESOURCE_PATH = "Loupedeck.VoiceMeeterPlugin.Resources";

        public static BitmapImage ReadImage(String imageName, String ext = "png", String addPath = "Images")
            => EmbeddedResources.ReadImage($"{RESOURCE_PATH}.{addPath}.{imageName}.{ext}");

        public static BitmapBuilder LoadBitmapBuilder(String imageName = "clear", String text = null, BitmapColor? textColor = null, String ext = "png", String addPath = "Images")
            => LoadBitmapBuilder(ReadImage(imageName, ext, addPath), text, textColor);

        public static BitmapBuilder LoadBitmapBuilder(BitmapImage image, String text = null, BitmapColor? textColor = null)
        {
            var builder = new BitmapBuilder(80, 80);
            builder.Clear(BitmapColor.Black);
            builder.DrawImage(image);

            return text is null ? builder : builder.AddTextOutlined(text, textColor: textColor);
        }

        public static BitmapImage LoadBitmapImage(String imageName = "clear", String text = null, BitmapColor? textColor = null, String ext = "png", String addPath = "Images")
            => LoadBitmapBuilder(imageName, text, textColor, ext, addPath).ToImage();

        public static BitmapImage LoadBitmapImage(BitmapImage image, String text = null, BitmapColor? textColor = null)
            => LoadBitmapBuilder(image, text, textColor).ToImage();

        public static BitmapBuilder AddTextOutlined(this BitmapBuilder builder, String text, BitmapColor? outlineColor = null, BitmapColor? textColor = null, Int32 fontSize = 12)
        {
            builder.DrawText(text, 0, -30, 80, 80, textColor, fontSize, 0, 0);
            return builder;
        }

        public static BitmapImage DrawDefaultImage(String innerText, String outerText, SKColor brushColor, Int32 width = 80, Int32 height = 80)
        {
            SKTypeface font = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            var info = new SKImageInfo(width, height);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;
            using var paint = new SKPaint();
            paint.Color = brushColor;
            paint.IsAntialias = true;
            paint.Typeface = font;

            var rect = new SKRect(5, 20, width - 5, height - 20);

            paint.TextSize = GetOptimalFontSize(innerText, paint, rect);

            var cornerRadius = Math.Min(width, height) / 2;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 2;
            canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, paint);
            paint.Style = SKPaintStyle.Fill;

            paint.TextAlign = SKTextAlign.Center;
            canvas.DrawText(innerText, rect.MidX, rect.MidY - (paint.FontMetrics.Descent + paint.FontMetrics.Ascent) / 2, paint);

            var image = surface.Snapshot();
            var data = image.Encode(SKEncodedImageFormat.Png, 100);

            return LoadBitmapImage(BitmapImage.FromArray(data.ToArray()), outerText);
        }

        public static BitmapImage DrawVolumeBar(PluginImageSize imageSize, BitmapColor backgroundColor, BitmapColor foregroundColor, Single currentValue, Int32 minValue, Int32 maxValue, Int32 scaleFactor, String cmd, String name = "", Boolean drawValue = true)
        {
            var dim = imageSize.GetDimension();
            var percentage = (currentValue - minValue) / (maxValue - minValue);
            var height = (Int32)(dim * 0.9);
            var width = (Int32)(dim * 0.6);
            var calculatedHeight = (Int32)(height * percentage);
            var xCenter = dim / 2 - width / 2;
            var yCenter = dim / 2 + height / 2;
            using var builder = new BitmapBuilder(dim, dim);

            builder.Clear(BitmapColor.Black);
            builder.DrawRectangle(xCenter, yCenter, width, -height, backgroundColor);
            builder.FillRectangle(xCenter, yCenter, width, -calculatedHeight, backgroundColor);

            if (drawValue)
            {
                builder.DrawText((currentValue / scaleFactor).ToString(CultureInfo.CurrentCulture), foregroundColor);
            }

            var cmdSize = GetOptimalFontSize(cmd, dim: dim);
            builder.DrawText(cmd, 0, dim / 2 - cmdSize / 2, dim, dim, foregroundColor, cmdSize, 0, 0);

            if (String.IsNullOrWhiteSpace(name))
            {
                return builder.ToImage();
            }

            var nameSize = GetOptimalFontSize(name, dim: dim);
            builder.DrawText(name, 0, dim / 2 * -1 + nameSize / 2, dim, dim, foregroundColor, nameSize, 0, 0);

            return builder.ToImage();
        }

        private static Int32 GetOptimalFontSize(String text, SKPaint paint = null, SKRect? rect = null, Int32? dim = null)
        {
            var minFontSize = 1;
            var maxFontSize = 16;
            SKRect textBounds = new SKRect();

            if (paint is null)
            {
                paint = new SKPaint { IsAntialias = true };
            }

            while (minFontSize <= maxFontSize)
            {
                var midFontSize = (minFontSize + maxFontSize) / 2;
                paint.TextSize = midFontSize;
                paint.MeasureText(text, ref textBounds);

                var fits = false;

                if (rect.HasValue)
                {
                    fits = textBounds.Width <= rect.Value.Width && textBounds.Height <= rect.Value.Height;
                }
                else if (dim.HasValue)
                {
                    fits = textBounds.Width <= dim.Value && textBounds.Height <= dim.Value;
                }

                if (fits)
                {
                    minFontSize = midFontSize + 1;
                }
                else
                {
                    maxFontSize = midFontSize - 1;
                }
            }

            return maxFontSize;
        }



        private static Int32 GetDimension(this PluginImageSize size) =>
            size switch
            {
                PluginImageSize.Width60 => 50,
                PluginImageSize.Width90 => 80,
                PluginImageSize.Width116 => 116,
                _ => 80,
            };
    }
}