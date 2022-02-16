using System.Drawing;
using System.Drawing.Drawing2D;

using static System.Math;

namespace JA.UI
{
    public static class Gdi2
    {
        public readonly static Color[] Pallette = new Color[]
        {
            Color.Blue,
            Color.Red,
            Color.Green,
            Color.Cyan,
            Color.Orange,
            Color.Yellow,
            Color.Purple,
            Color.Olive,
            Color.Lime
        };

        public static Color Darken(this Color color, float amount)
        {
            var scale = 1-amount;

            return Color.FromArgb(color.A,
                (int)(scale*255),
                (int)(scale*255),
                (int)(scale*255));
        }
        public static Color Lighten(this Color color, float amount)
        {
            var max = Max(color.R, Max(color.G, color.B));

            if (max>0)
            {
                return Color.FromArgb(color.A,
                    (int)(color.R + amount*(255-color.R)),
                    (int)(color.G + amount*(255-color.G)),
                    (int)(color.B + amount*(255-color.B)));
            }
            else
            {
                return Color.FromArgb(color.A,
                    (int)(amount*255),
                    (int)(amount*255),
                    (int)(amount*255));
            }
        }

        public static void DrawArrow(this Graphics g, PointF origin, PointF target, Color color)
        {
            using (var pen = new Pen(color,2))
            {
                pen.EndCap = LineCap.ArrowAnchor;                
                g.DrawLine(pen, origin, target);
            }
        }
        public static void DrawArrow(this Graphics g, PointF origin, float dx, float dy, Color color)
        {
            using (var pen = new Pen(color, 0))
            {
                pen.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(pen, origin.X, origin.Y, origin.X+dx, origin.Y+dy);
            }
        }

        public static GraphicsPath Point(PointF center, float diameter = 4f)
        {
            var gp = new GraphicsPath();
            AddPoint(gp, center, diameter);
            return gp;
        }

        public static GraphicsPath Polygon(params PointF[] points)
        {
            var gp = new GraphicsPath();
            gp.AddPolygon(points);
            return gp;
        }

        public static void AddPoint(this GraphicsPath path, PointF center, float diameter = 4f)
        {
            path.AddEllipse(center.X-diameter/2, center.Y-diameter/2, diameter, diameter);
        }
        public static void AddPolygon(this GraphicsPath path, params PointF[] points)
        {
            path.AddPolygon(points);
        }

        public static void Draw(this Graphics g, GraphicsPath path, Color color, float width = 0f, DashStyle dash = DashStyle.Solid)
        {
            using (var pen = new Pen(color, width))
            {
                pen.DashStyle = dash;
                g.DrawPath(pen, path);
            }
        }

        public static void Fill(this Graphics g, GraphicsPath path, Color color)
        {
            using (var brush = new SolidBrush(color))
            {
                g.FillPath(brush, path);
            }
        }
    }
}
