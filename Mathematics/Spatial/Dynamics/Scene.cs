using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JA.Mathematics.Spatial.Dynamics
{
    using JA.UI;

    public delegate ObjRate[] RateFunction(Frame3 frame);

    public class Scene
    {
        public Scene(Camera camera, params Object[] objects)
        {
            this.Camera=camera;
            this.Objects=new ObservableCollection<Object>(objects);
            this.InnerHistory = new List<Frame3>();
            this.GetRate = (f) => f.States.Select(
                (state, i) =>  new ObjRate(state, Vector3.Zero, Vector3.Zero)           
                ).ToArray(); 
            this.Objects.CollectionChanged += (s, ev) => Reset();
        }
        public RateFunction GetRate { get; set; }
        public ObservableCollection<Object> Objects { get; }
        public Camera Camera { get; }
        internal List<Frame3> InnerHistory { get; }
        public double Time { get => Current?.Time ?? 0; }
        public Frame3 Current { get => InnerHistory.LastOrDefault(); }
        public ReadOnlyCollection<Frame3> History { get => InnerHistory.AsReadOnly(); }

        public void Reset()
        {
            InnerHistory.Clear();
            InnerHistory.Add(new Frame3(0.0, Objects.Select((item) => item.InitialCondition).ToArray()));
        }
        public void Step(double timeStep)
        {
            if (InnerHistory.Count==0)
            {
                Reset();
            }
            var frame = Current;
            var next = frame.Integrate(timeStep, GetRate);
            InnerHistory.Add(next);
        }
        public void Render(Graphics g)
        {
            var origin = Camera.Project(Vector3.Zero);
            var x_axis = Camera.Project(Vector3.UnitX);
            var y_axis = Camera.Project(Vector3.UnitY);
            var z_axis = Camera.Project(Vector3.UnitZ);

            g.DrawArrow(origin, x_axis, Color.Red);
            g.DrawArrow(origin, y_axis, Color.Green);
            g.DrawArrow(origin, z_axis, Color.Blue);

            var frame = Current;
            if (frame!=null)
            {
                for (int i = 0; i < Objects.Count; i++)
                {
                    Objects[i].Render(g, Camera, frame.States[i]);
                }
            }
        }
    }
}
