using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;

namespace JA.Mathematics.Spatial.Dynamics
{
    using JA.UI;

    public class Object
    {
        public Object(ObjState initialState, Color color, params Mesh3[] meshes)
        {
            this.InitialCondition = initialState;
            this.Color = color;
            this.Meshes=new ObservableCollection<Mesh3>(meshes);
            this.RefreshVolume();
            this.Meshes.CollectionChanged += (s, ev) => RefreshVolume();
        }
        void RefreshVolume() 
        {
            var vol = new Solid3();
            foreach (var mesh in Meshes)
            {
                vol += new Solid3(mesh);
            }
            this.Solid = vol;
        }
        public ObjState InitialCondition { get; set; }
        public ObservableCollection<Mesh3> Meshes { get; }
        public Solid3 Solid { get; private set; }
        public Color Color { get; set; }
        public Vector3[][][] GetShape(ObjState frame) 
            => Meshes.Select((mesh) => frame.Transform(mesh.GetShape())).ToArray();

        public void Render(Graphics g, Camera camera, ObjState frame)
        {
            foreach (var mesh in Meshes)
            {
                var shape = mesh.GetShape();
                foreach (var face in shape)
                {
                    var vertex = frame.Transform(face);
                    var points = camera.Project(vertex);
                    var path = Gdi2.Polygon(points);
                    g.Fill(path, Color.FromArgb(64, Color));
                    g.Draw(path, Color.Black);
                }
            }
        }
    }

    public class RigidBody : Object
    {
        public RigidBody(ObjState initState, double mass, Color color, params Mesh3[] meshes)
            : base(initState, color, meshes)
        {
            this.Mass= mass;
        }
        public double Mass { get; }
        public Matrix3 Mmoi { get => Mass*Solid.VmoiAtCentroid; }
    }
}
