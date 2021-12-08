namespace Loupedeck.VoiceMeeterPlugin.Extensions
{
    using System;
    using System.Drawing;

    public static class GraphicsExtensions
    {
        public static void DrawAutoAdjustedFont(this Graphics g, String s, Font font, Brush brush, Single x,
            Single y, Int32 maxFontSize = 90, Int32 minFontSize = 5, Boolean smallestOnFail = true, Int32 width = 0)
            => g.DrawAutoAdjustedFont(s, font, brush, new RectangleF(x, y, 0.0f, 0.0f), (StringFormat)null,
                maxFontSize, minFontSize, smallestOnFail);

        public static void DrawAutoAdjustedFont(this Graphics g, String s, Font font, Brush brush, PointF point,
            Int32 maxFontSize = 90, Int32 minFontSize = 5, Boolean smallestOnFail = true, Int32 width = 0)
            => g.DrawAutoAdjustedFont(s, font, brush, new RectangleF(point.X, point.Y, 0.0f, 0.0f),
                (StringFormat)null, maxFontSize, minFontSize, smallestOnFail);

        public static void DrawAutoAdjustedFont(this Graphics g, String s, Font font, Brush brush, Single x, Single y,
            StringFormat format, Int32 maxFontSize = 90, Int32 minFontSize = 5, Boolean smallestOnFail = true,
            Int32 width = 0)
            => g.DrawAutoAdjustedFont(s, font, brush, new RectangleF(x, y, 0.0f, 0.0f), format,
                maxFontSize, minFontSize, smallestOnFail);

        public static void DrawAutoAdjustedFont(this Graphics g, String s, Font font, Brush brush, PointF point,
            StringFormat format, Int32 maxFontSize = 90, Int32 minFontSize = 5, Boolean smallestOnFail = true,
            Int32 width = 0)
            => g.DrawAutoAdjustedFont(s, font, brush, new RectangleF(point.X, point.Y, 0.0f, 0.0f), format,
                maxFontSize, minFontSize, smallestOnFail);

        public static void
            DrawAutoAdjustedFont(this Graphics g, String s, Font font, Brush brush, RectangleF layoutRectangle,
                Int32 maxFontSize = 90, Int32 minFontSize = 5, Boolean smallestOnFail = true, Int32 width = 0)
            => g.DrawAutoAdjustedFont(s, font, brush, layoutRectangle, (StringFormat)null,
                maxFontSize, minFontSize, smallestOnFail);

        public static void DrawAutoAdjustedFont(this Graphics g, String s, Font font, Brush brush,
            RectangleF layoutRectangle, StringFormat format,
            Int32 maxFontSize = 90, Int32 minFontSize = 5, Boolean smallestOnFail = true, Int32 width = 0)
        {
            g.DrawString(
                s,
                g.GetAdjustedFont(s, font, width == 0 ? layoutRectangle.Width : width, maxFontSize, minFontSize,
                    smallestOnFail),
                brush,
                layoutRectangle,
                format
            );
        }

        private static Font GetAdjustedFont(this Graphics g, String graphicString, Font originalFont,
            Single containerWidth, Int32 maxFontSize = 90, Int32 minFontSize = 5, Boolean smallestOnFail = true)
        {
            Font testFont = null;
            for (var adjustedSize = maxFontSize; adjustedSize >= minFontSize; adjustedSize--)
            {
                testFont = new Font(originalFont.Name, adjustedSize, originalFont.Style);

                var adjustedSizeNew = g.MeasureString(graphicString, testFont);

                if (containerWidth > Convert.ToInt32(adjustedSizeNew.Width))
                {
                    return testFont;
                }
            }

            return smallestOnFail ? testFont : originalFont;
        }
    }
}