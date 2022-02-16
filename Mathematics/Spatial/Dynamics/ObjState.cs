using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace JA.Mathematics.Spatial.Dynamics
{
    public struct ObjState
    {
        public ObjState(Vector3 position, Quaternion orientation)
            : this(position, orientation, Vector3.Zero, Vector3.Zero) { }
        public ObjState(Vector3 position, Quaternion orientation, Vector3 velocity, Vector3 omega)
        {
            Position=position;
            Orientation=orientation;
            Velocity=velocity;
            Omega=omega;
        }
        public static implicit operator ObjState(Vector3 position) => new ObjState(position, Quaternion.Identity);
        public static implicit operator ObjState(Quaternion orientation) => new ObjState(Vector3.Zero, orientation);

        public Vector3 Position { get; }
        public Quaternion Orientation { get; }
        public Vector3 Velocity { get; }
        public Vector3 Omega { get; }
        public Matrix3 Rotation => Orientation.ToRotationMatrix();
        public Matrix3 InverseRotation => Orientation.ToRotationMatrix(true);

        public ObjState Step(double timeStep, Vector3 acceleration, Vector3 alpha)
        {
            var pos = Position + timeStep*Velocity;
            var ori = Quaternion.FromRotVelocityAndTime(Omega, timeStep)*Orientation;
            var vee = Velocity + timeStep*acceleration;
            var omg = Omega + timeStep*alpha;

            return new ObjState(pos, ori, vee, omg);
        }
        public ObjState Step(double timeStep, ObjRate rate) 
        {
            var pos = Position + timeStep*rate.Velocity;
            var ori = Quaternion.FromRotVelocityAndTime(rate.Omega, timeStep)*Orientation;
            var vee = Velocity + timeStep*rate.Acceleration;
            var omg = Omega + timeStep*rate.Alpha;

            return new ObjState(pos, ori, vee, omg);
        }
        public IEnumerable<Vector3> Transform(IEnumerable<Vector3> nodes)
        {
            var p = Position;
            var R = Orientation.ToRotationMatrix();
            return nodes.Select((node) => p + R*node);
        }
        public Vector3[] Transform(params Vector3[] nodes)
        {
            return Transform(nodes.AsEnumerable()).ToArray();
        }
        public Vector3[][] Transform(Vector3[][] mesh)
        {
            var p = Position;
            var R = Orientation.ToRotationMatrix();
            return mesh.Select((face) => face.Select((node)=>p+R*node).ToArray()).ToArray();
        }
    }

    public struct ObjRate
    {
        public ObjRate(ObjState state, Vector3 acceleration, Vector3 alpha)
            : this(state.Velocity, state.Omega, acceleration, alpha) { }

        public ObjRate(Vector3 velocity, Vector3 omega, Vector3 acceleration, Vector3 alpha)
        {
            Velocity=velocity;
            Omega=omega;
            Acceleration=acceleration;
            Alpha=alpha;
        }

        public Vector3 Velocity { get; }
        public Vector3 Omega { get; }
        public Vector3 Acceleration { get; }
        public Vector3 Alpha { get; }


        #region Algebra
        public ObjRate Scale(double factor)
            => new ObjRate(
                factor*Velocity,
                factor*Omega,
                factor*Acceleration,
                factor*Alpha);
        public ObjRate Add(ObjRate other)
            => new ObjRate(
                Velocity+other.Velocity,
                Omega+other.Omega,
                Acceleration+other.Acceleration,
                Alpha+other.Alpha);

        public static ObjRate operator +(ObjRate a, ObjRate b) => a.Add(b);
        public static ObjRate operator -(ObjRate a) => a.Scale(-1);
        public static ObjRate operator -(ObjRate a, ObjRate b) => a.Add(-b);
        public static ObjRate operator *(double factor, ObjRate a) => a.Scale(factor);
        public static ObjRate operator *(ObjRate a, double factor) => a.Scale(factor);
        public static ObjRate operator /(ObjRate a, double dividor) => a.Scale(1/dividor);
        #endregion

    }
}
