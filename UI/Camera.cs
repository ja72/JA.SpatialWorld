using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JA.Mathematics.Spatial;

using static System.Math;

namespace JA.UI
{
    public class Camera
    {
        public Camera(Control target)
        {
            this.Target= target;
            this.ModelSize = 10;
            this.EyeDistance = 40;
        }

        public Control Target { get; }
        public float ModelSize { get; set; }
        public float EyeDistance { get; set; }
        public float Fov
        {
            get => (float)((180/PI)*Atan(ModelSize/EyeDistance));
            set => EyeDistance = (float)(ModelSize*Tan(PI*value/180));
        }
        public RectangleF Client { get => Target.ClientRectangle; }
        public float PixelSize => Min(Client.Width, Client.Height);
        public float Scale => PixelSize/ModelSize;

        public PointF Project(Vector3 location)
        {
            var x = (float)location.X;
            var y = (float)location.Y;
            var z = (float)location.Z;
            return new PointF(
                Scale*(EyeDistance*x/(EyeDistance-z)),
                Scale*(EyeDistance*y/(EyeDistance-z)));
        }
        public IEnumerable<PointF> Project(IEnumerable<Vector3> location)
        {
            var s = Scale;
            foreach (var vertex in location)
            {
                var x = (float)vertex.X;
                var y = (float)vertex.Y;
                var z = (float)vertex.Z;
                yield return new PointF(
                    s*(EyeDistance*x/(EyeDistance-z)),
                    s*(EyeDistance*y/(EyeDistance-z)));
            }
        }
        public PointF[] Project(params Vector3[] locations)
            => Project(locations.AsEnumerable()).ToArray();
    }
}
