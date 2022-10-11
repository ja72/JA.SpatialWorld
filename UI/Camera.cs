using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JA.Mathematics.Spatial;

using static System.Math;
using static JA.Mathematics.Factory;

namespace JA.UI
{
    public class Camera
    {
        public Camera(Control target)
        {
            this.Target= target;
            this.ModelSize = 10;
            this.EyeDistance = 40;

            Point mouseDn = Point.Empty, mouseMv;
            MouseButtons buttons;
            target.MouseDown += (s, ev) =>
            {
                if (ev.Button == MouseButtons.Left)
                {
                    mouseDn = ev.Location;
                }
            };
            target.MouseMove += (s, ev) =>
            {
                if (ev.Button == MouseButtons.Left)
                {
                    int dx = ev.Location.X - mouseDn.X, dy = ev.Location.Y - mouseDn.Y;
                    Azimuth += (float)(dx*DEG);
                    Altitude += (float)(dy*DEG);

                    mouseDn = ev.Location;
                }
            };
        }

        public Control Target { get; }
        public float ModelSize { get; set; }
        public float EyeDistance { get; set; }
        public float Azimuth { get; set; } 
        public float Altitude { get; set; } 
        public float Fov
        {
            get => (float)(Atan(ModelSize/EyeDistance)/DEG);
            set => EyeDistance = (float)(ModelSize*Tan(value*DEG));
        }
        public RectangleF Client { get => Target.ClientRectangle; }
        public float PixelSize => Min(Client.Width, Client.Height);
        public float Scale => PixelSize/ModelSize;

        public PointF Project(Vector3 location)
        {
            var q =Quaternion.RotateX(Altitude) * Quaternion.RotateY(Azimuth);
            location = q.Rotate(location);
            var x = (float)location.X;
            var y = (float)location.Y;
            var z = (float)location.Z;
            return new PointF(
                Scale*(EyeDistance*x/(EyeDistance-z)),
                Scale*(EyeDistance*y/(EyeDistance-z)));
        }
        public IEnumerable<PointF> Project(IEnumerable<Vector3> nodes)
        {
            var s = Scale;
            var q =Quaternion.RotateX(Altitude) * Quaternion.RotateY(Azimuth);
            foreach (var xyz in nodes)
            {
                var location = q.Rotate(xyz);
                var x = (float)location.X;
                var y = (float)location.Y;
                var z = (float)location.Z;
                yield return new PointF(
                    s*(EyeDistance*x/(EyeDistance-z)),
                    s*(EyeDistance*y/(EyeDistance-z)));
            }
        }
        public PointF[] Project(params Vector3[] locations)
            => Project(locations.AsEnumerable()).ToArray();
    }
}
