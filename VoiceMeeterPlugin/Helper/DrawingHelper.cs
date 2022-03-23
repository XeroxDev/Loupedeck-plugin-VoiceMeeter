namespace Loupedeck.VoiceMeeterPlugin.Helper
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;

    using Extensions;

    public static class DrawingHelper
    {
        private static String RESOURCE_PATH = "Loupedeck.VoiceMeeterPlugin.Resources";

        public static GraphicsPath RoundedRect(Rectangle bounds, Int32 radius)
        {
            var diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc
            path.AddArc(arc, 180, 90);

            // top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
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
            var builder = new BitmapBuilder(90, 90);

            builder.Clear(BitmapColor.Black);
            builder.DrawImage(image);
            // builder.FillRectangle(0, 0, 90, 90, new BitmapColor(0, 0, 0, 100));

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
            builder.DrawText(text, 0, -25, 90, 90, textColor, fontSize, 0, 0);
            return builder;
        }

        public static BitmapImage DrawDefaultImage(String innerText, String outerText, Color brushColor)
        {
            var imageDimension = 90;
            var dimension = 70;
            using var bitmap = new Bitmap(imageDimension, imageDimension);
            using var g = Graphics.FromImage(bitmap);
            var font = new Font("Arial", 20, FontStyle.Bold);
            var brush = new SolidBrush(brushColor);
            var rect = new Rectangle(0, 0, dimension, dimension / 2);
            rect.X = bitmap.Width / 2 - rect.Width / 2;
            rect.Y = bitmap.Height / 2 - rect.Height / 2;
            var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            g.DrawPath(new Pen(brush.Color, 2), DrawingHelper.RoundedRect(rect, 15));
            g.DrawAutoAdjustedFont(innerText, font, brush, rect, sf, 20);

            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);

            return DrawingHelper.LoadBitmapImage(BitmapImage.FromArray(ms.ToArray()), outerText);
            
        }
    }
}