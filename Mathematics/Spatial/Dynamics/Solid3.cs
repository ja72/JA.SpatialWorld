using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Math;

namespace JA.Mathematics.Spatial.Dynamics
{
    public struct Solid3
    {
        public Solid3(double area, double volume, Vector3 center, Matrix3 vmoiAtCenter)
        {
            this.SurfaceArea = area;
            this.TotalVolume = volume;
            this.Centroid = center;
            this.VmoiAtCentroid = vmoiAtCenter;
            this.VmoiAtOrigin = vmoiAtCenter + center.Parallel();
        }

        public Solid3(Mesh3 mesh)
        {
            double area = 0;
            double volume = 0;
            Vector3 center = Vector3.Zero;
            Matrix3 vmoi = Matrix3.Zero;
            foreach (var face in mesh.Tesselate())
            {
                foreach (var trig in face)
                {
                    var ai = trig.Area;
                    var vi = trig.TripleProduct/6;
                    var ci = (3d/4)*trig.Centoid;
                    var ui = 
                        ((trig.A+trig.B).Parallel()
                        +(trig.B+trig.C).Parallel()
                        +(trig.C+trig.A).Parallel())/20;
                    area += ai;
                    volume += vi;
                    center += vi*ci;
                    vmoi  += vi*ui;
                }
            }
            //tex: $v_i = \tfrac{1}{6} \,A\cdot(B\times C)$
            //tex: $u_i = ([A+B]^2+[B+C]^2+[C+A]^2)/20$
            //tex: $I_C/m = \tfrac{1}{V} \Sigma_i (v_i u_i) -  c^2 $ 
            this.SurfaceArea = area;
            this.TotalVolume= volume;
            this.Centroid = center/volume;
            this.VmoiAtOrigin = vmoi/volume;
            this.VmoiAtCentroid = VmoiAtOrigin - Centroid.Parallel();
        }

        public double SurfaceArea { get; }
        public double TotalVolume { get; }
        public Vector3 Centroid { get; }
        public Matrix3 VmoiAtOrigin { get; }
        public Matrix3 VmoiAtCentroid { get; }


        #region Algebra

        public Solid3 Scale(double factor)
        {
            return new Solid3(
                factor*SurfaceArea,
                factor*TotalVolume,
                Centroid,
                factor*VmoiAtCentroid);
        }
        public Solid3 Add(Solid3 other)
        {
            var area = SurfaceArea + other.SurfaceArea;
            var vol = TotalVolume + other.TotalVolume;
            var center = (TotalVolume*Centroid + other.TotalVolume*other.Centroid)/vol;
            var vmoi = VmoiAtCentroid + TotalVolume * Centroid.Parallel()
                + other.VmoiAtCentroid + other.TotalVolume * other.Centroid.Parallel();
            vmoi = vmoi - vol*center.Parallel();
            return new Solid3(area,
                vol,
                center,
                vmoi);
        }

        public static Solid3 operator +(Solid3 a, Solid3 b) => a.Add(b);
        public static Solid3 operator -(Solid3 a) => a.Scale(-1);
        public static Solid3 operator -(Solid3 a, Solid3 b) => a.Add(-b);
        public static Solid3 operator *(double f, Solid3 a) => a.Scale(f);
        public static Solid3 operator *(Solid3 a, double f) => a.Scale(f);
        public static Solid3 operator /(Solid3 a, double d) => a.Scale(1/d);
        #endregion

        public static void TestVolumeCalc()
        {
            var mesh = new Mesh3();
            mesh.AddFace(
                new Vector3(40, 0, 0),
                new Vector3(0, 25, 0),
                new Vector3(0, 0, 8));

            var obj = new Solid3(mesh);
            double m = 0.01333333333333333;
            double V = 8000.0/6;
            Vector3 cg = Vector3.Cartesian(10, 6.25, 2);
            Matrix3 Vc = Matrix3.Symmetric(
                0.3445, 0.8320, 1.1125,
                0.16666667, 0.05333333, 0.033333333)/m;

            Debug.Assert(Abs(obj.TotalVolume-V) < 1e-8);
            Debug.Assert((obj.Centroid-cg).MaxAbs() < 1e-8);
            Debug.Assert((obj.VmoiAtCentroid-Vc).MaxAbs() < 1e-4);
        }
    }
}
