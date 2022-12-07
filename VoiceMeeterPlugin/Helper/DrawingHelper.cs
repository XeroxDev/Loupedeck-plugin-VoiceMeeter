namespace Loupedeck.VoiceMeeterPlugin.Helper
{
    using System;
    using System.Globalization;

    using SkiaSharp;

    public static class DrawingHelper
    {
        private static String RESOURCE_PATH = "Loupedeck.VoiceMeeterPlugin.Resources";

        public static SKPath RoundedRect(SKRect bounds, Int32 radius)
        {
            var diameter = radius * 2;
            SKRect arc = new SKRect(bounds.Left, bounds.Top, bounds.Left + diameter, bounds.Top + diameter);
            SKPath path = new SKPath();

            if (radius == 0)
            {
                path.AddRect(bounds);
                return path;
            }

            // top left arc
            path.ArcTo(arc, 180, 90, false);

            // top right arc
            arc = new SKRect(bounds.Right - diameter, bounds.Top, bounds.Right, bounds.Top + diameter);
            path.ArcTo(arc, 270, 90, false);

            // bottom right arc
            arc = new SKRect(bounds.Right - diameter, bounds.Bottom - diameter, bounds.Right, bounds.Bottom);
            path.ArcTo(arc, 0, 90, false);

            // bottom left arc
            arc = new SKRect(bounds.Left, bounds.Bottom - diameter, bounds.Left + diameter, bounds.Bottom);
            path.ArcTo(arc, 90, 90, false);

            path.Close();
            return path;
        }

        public static BitmapImage ReadImage(String imageName, String ext = "png", String addPath = "Images")
            => EmbeddedResources.ReadImage($"{RESOURCE_PATH}.{addPath}.{imageName}.{ext}");

        public static BitmapBuilder LoadBitmapBuilder
        (String imageName = "clear", String text = null, BitmapColor? textColor = null, String ext = "png",
            String addPath = "Images")
            => LoadBitmapBuilder(ReadImage(imageName, ext, addPath), text, textColor);

        public static BitmapBuilder LoadBitmapBuilder
            (BitmapImage image, String text = null, BitmapColor? textColor = null)
        {
            var builder = new BitmapBuilder(80, 80);

            builder.Clear(BitmapColor.Black);
            builder.DrawImage(image);

            return text is null ? builder : builder.AddTextOutlined(text, textColor: textColor);
        }

        public static BitmapImage LoadBitmapImage
        (String imageName = "clear", String text = null, BitmapColor? textColor = null, String ext = "png",
            String addPath = "Images")
            => LoadBitmapBuilder(imageName, text, textColor, ext, addPath).ToImage();

        public static BitmapImage LoadBitmapImage(BitmapImage image, String text = null, BitmapColor? textColor = null)
            => LoadBitmapBuilder(image, text, textColor).ToImage();

        public static BitmapBuilder AddTextOutlined(this BitmapBuilder builder, String text,
            BitmapColor? outlineColor = null,
            BitmapColor? textColor = null, Int32 fontSize = 12)
        {
            // TODO: Make it outline
            builder.DrawText(text, 0, -30, 80, 80, textColor, fontSize, 0, 0);
            return builder;
        }

        public static BitmapImage DrawDefaultImage(String innerText, String outerText, SKColor brushColor)
        {
            // Set the dimensions and font
            int width = 80;
            int height = 80;
            SKTypeface font = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            int fontSize = 20;

            // Create the canvas and paint
            var info = new SKImageInfo(width, height);
            var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;
            var paint = new SKPaint { Color = brushColor, IsAntialias = true, Typeface = font };

            // Calculate the dimensions of the rounded rectangle outline
            var rect = new SKRect(5, 20, width - 5, height - 20);

            // Adjust the font size if necessary to fit the inner text within the dimensions of the rounded rectangle outline
            while (true)
            {
                paint.TextSize = fontSize;
                SKRect tb = new SKRect();
                paint.MeasureText(innerText, ref tb);
                if (tb.Width < rect.Width - 5 && tb.Height < rect.Height)
                {
                    break;
                }

                fontSize--;
            }

            // Draw the rounded rectangle outline
            var cornerRadius = Math.Min(width, height) / 2;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 2;
            canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, paint);
            paint.Style = SKPaintStyle.Fill;

            // Draw the inner text centered within the rounded rectangle outline
            paint.TextAlign = SKTextAlign.Center;
            paint.TextSize = fontSize;
            canvas.DrawText(innerText, rect.MidX, rect.MidY - ((paint.FontMetrics.Descent + paint.FontMetrics.Ascent) / 2), paint);

            // Save the image to memory and return the memory streams
            var image = surface.Snapshot();
            var data = image.Encode(SKEncodedImageFormat.Png, 100);

            return LoadBitmapImage(BitmapImage.FromArray(data.ToArray()), outerText);
        }

        public static BitmapImage DrawVolumeBar(PluginImageSize imageSize, BitmapColor backgroundColor, BitmapColor foregroundColor, Single currentValue, Int32 minValue, Int32 maxValue,
            Int32 scaleFactor, String name = "")
        {
            var dim = imageSize.GetDimension();
            var percentage = (currentValue - minValue) / (maxValue - minValue) * 100;
            var width = (Int32)(dim * percentage / 100.0);

            var builder = new BitmapBuilder(dim, dim);
            builder.Clear(BitmapColor.Black);

            builder.Translate(dim / 4, 0);
            builder.DrawRectangle(0, 0, dim / 2, dim - 1, backgroundColor);
            builder.FillRectangle(0, dim, dim / 2, -width, backgroundColor);
            builder.ResetMatrix();
            builder.DrawText((currentValue / scaleFactor).ToString(CultureInfo.CurrentCulture), foregroundColor);

            // if name is available, draw it under the volume bar
            if (String.IsNullOrEmpty(name))
            {
                return builder.ToImage();
            }

            var fontSize = name.Length switch
            {
                > 15 => 6,
                > 10 => 8,
                _ => 12
            };

            builder.DrawText(name, 0, dim / 2, dim, dim / 2, foregroundColor, fontSize, 0, 0);

            return builder.ToImage();
        }

        public static Int32 GetDimension(this PluginImageSize size) => size == PluginImageSize.Width60 ? 50 : 80;
    }
}