using System;
using System.Diagnostics;


namespace JA.Mathematics.Spatial
{
    using num = Double;
    using vec3 = Vector3;
    using mat3 = Matrix3;
    using qua3 = Quaternion;


    [DebuggerDisplay("Pos={Position} Axis={RotationAxis} Angle={RotationAngle}")]
    public class Transform : ICloneable
    {
        qua3 orientation;
        mat3 rotation;
        mat3 inverse;

        #region Factory
        public Transform(vec3 position)
        {
            this.Position = position;
            this.orientation = qua3.Identity;
            this.rotation = mat3.Identity;
            this.inverse = mat3.Identity;
        }
        public Transform(qua3 orientation) : this(vec3.Zero, orientation) { }
        public Transform(AxisName axis, num angle) : this(vec3.Zero, qua3.FromAxisAngle(axis, angle)) { }
        public Transform(vec3 axis, num angle) : this(vec3.Zero, qua3.FromAxisAngle(axis, angle)) { }
        public Transform(mat3 rotation) : this(vec3.Zero, qua3.FromRotationMatrix(rotation)) { }
        public Transform(vec3 position, AxisName axis, num angle) : this(position, qua3.FromAxisAngle(axis, angle)) { }
        public Transform(vec3 position, vec3 axis, num angle) : this(position, qua3.FromAxisAngle(axis, angle)) { }
        public Transform(vec3 position, mat3 rotation) : this(position, qua3.FromRotationMatrix(rotation)) { }
        public Transform(vec3 position, qua3 orientation)
        {
            this.Position = position;
            this.orientation = orientation;
            this.rotation = orientation.ToRotationMatrix();
            this.inverse = orientation.Conjugate().ToRotationMatrix();
        }
        public Transform(Transform rhs)
        {
            this.Position = rhs.Position;
            this.orientation = rhs.orientation;
            this.rotation = rhs.rotation;
            this.inverse = rhs.inverse;
        }
        public static readonly Transform I = new Transform(vec3.Zero, qua3.Identity);

        #endregion

        #region Properties
        public vec3 Position { get; set; }
        public qua3 Orientation
        {
            get { return orientation; }
            set
            {
                orientation = value;
                rotation = value.ToRotationMatrix();
                inverse = orientation.Conjugate().ToRotationMatrix();
            }
        }
        public mat3 Rotation
        {
            get { return rotation; }
            set
            {
                orientation = qua3.FromRotationMatrix(value);
                rotation = value;
                inverse = value.Transpose();
                //Maybe ori.Conjugate().ToRotation() is faster ??
            }
        }
        public vec3 RotationAxis { get { return orientation.Axis; } }
        public num RotationAngle { get { return orientation.Angle; } }
        public qua3 InverseOrientation { get { return orientation.Conjugate(); } }
        public mat3 InverseRotation { get { return inverse; } }
        #endregion

        #region Functions

        public Transform Invert()
        {
            return new Transform(- orientation.InvRotate(Position), orientation.Inverse());
        }

        public Transform Clone()
        {
            return new Transform(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        
        #endregion
        
        #region Transforms Base to Local
        public vec3 Base2Local(vec3 point)
        {
            return inverse * (point - Position);
        }
        public mat3 Base2Local(mat3 rotation)
        {
            return inverse * rotation;
        }
        public qua3 Base2Local(qua3 rotation)
        {
            return orientation.Conjugate() * rotation;
        }
        public vec3 Base2LocalDirection(vec3 direction)
        {
            return inverse * direction;
        }
        #endregion

        #region Transforms Local to Base
        public vec3 Local2Base(vec3 point)
        {
            return Position + rotation * point;
        }
        public vec3 Local2BaseDirection(vec3 direction)
        {
            return rotation * direction;
        }

        public mat3 Local2Base(mat3 rotation)
        {
            return this.rotation * rotation;
        }

        public qua3 Local2Base(qua3 rotation)
        {
            return orientation * rotation;
        }
        #endregion

    }

}
