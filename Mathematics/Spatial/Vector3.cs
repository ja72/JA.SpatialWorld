using System;
using System.Collections.Generic;
using System.Diagnostics;

using static System.Math;

namespace JA.Mathematics.Spatial
{
    using num = Double;

    [DebuggerDisplay("X={X} Y={Y} Z={Z}")]
    public struct Vector3 : IEnumerable<num>, ICloneable, IFormattable, IEquatable<Vector3>
    {


        #region Constuctors
        public Vector3(num x, num y, num z, bool transpose = false)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.IsTransposed = transpose;
        }
        public Vector3(params num[] elements)
            : this(elements[0], elements[1], elements[2])
        {
        }

        public static Vector3 FromAxis(AxisName axis)
        {
            switch (axis)
            {
                case AxisName.X:
                    return UnitX;
                case AxisName.Y:
                    return UnitY;
                case AxisName.Z:
                    return UnitZ;
                default:
                    return Zero;
            }
        }

        public static Vector3 Cartesian(num x, num y, num z)
        {
            return new Vector3(x, y, z);
        }

        public static Vector3 Parse(string text)
        {
            // parse string "(x,y,z)" into vector

            // remove any parethesis.
            text = text.TrimStart('(').TrimEnd(')');
            var parts = text.Split(',');
            if (parts.Length>=3)
            {
                double.TryParse(parts[0].Trim(), out double x);
                double.TryParse(parts[1].Trim(), out double y);
                double.TryParse(parts[2].Trim(), out double z);

                return new Vector3(x, y, z);
            }
            return Zero;
        }

        #endregion

        #region Properties
        public num X { get; }
        public num Y { get; }
        public num Z { get; }
        public bool IsTransposed { get; }
        public int Size => 3;
        public bool IsZero => X==0 && Y==0 && Z==0;
        public num SumSquares => X * X + Y * Y + Z * Z;
        public num Magnitude => Sqrt(SumSquares);
        public num this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                }
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        #endregion

        #region Functions
        public Vector3 Normalized()
        {
            num m = Magnitude;
            if (m != 1f && m > 1e-8f)
            {
                return Cartesian(X / m, Y / m, Z / m);
            }
            return this;
        }
        public num[] ToArray()
        {
            return new num[] { X, Y, Z };
        }
        public num[][] ToArray2()
        {
            if (IsTransposed)
            {
                return new num[][] { 
                    new num[] { X, Y, Z } 
                };
            }
            else
            {
                return new num[][] { 
                    new num[] { X }, 
                    new num[] { Y }, 
                    new num[] { Z } 
                };
            }
        }
        public double MaxAbs() => Max(Abs(X), Max(Abs(Y), Abs(Z)));
        public Vector3 Negative()
        {
            return new Vector3(-X, -Y, -Z, IsTransposed);
        }
        public Vector3 Transpose()
        {
            return new Vector3(X, Y, Z, !IsTransposed);
        }
        public Vector3 Cross(Vector3 rhs)
        {
            return Cross(this, rhs);
        }
        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
            if (lhs.IsTransposed == rhs.IsTransposed)
            {
                return new Vector3(
                    (lhs.Y * rhs.Z - lhs.Z * rhs.Y),
                    (lhs.Z * rhs.X - lhs.X * rhs.Z),
                    (lhs.X * rhs.Y - lhs.Y * rhs.X), lhs.IsTransposed
                );
            }
            else
            {
                throw new ArgumentException("Cannot cross mistransposed matrices.");
            }
        }
        public Matrix3 Cross() => Matrix3.SkewSymmetric(-Z, Y, -X);
        public Matrix3 Parallel(double factor = 1)
        {
            return new Matrix3(
                factor*(Y*Y+Z*Z), -factor*X*Y, -factor*X*Z,
                -factor*X*Y, factor*(X*X+Z*Z), -factor*Y*Z,
                -factor*X*Z, -factor*Y*Z, factor*(X*X+Y*Y));
        }
        public static num Dot(Vector3 lhs, Vector3 rhs)
        {
            return (lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z);
        }
        public static num Distance(Vector3 from, Vector3 to)
        {
            return (to - from).Magnitude;
        }
        public static Matrix3 Outer(Vector3 lhs, Vector3 rhs)
        {
            return new Matrix3(
                lhs.X * rhs.X, lhs.X * rhs.Y, lhs.X * rhs.Z,
                lhs.Y * rhs.X, lhs.Y * rhs.Y, lhs.Y * rhs.Z,
                lhs.Z * rhs.X, lhs.Z * rhs.Y, lhs.Z * rhs.Z);
        }

        public Vector3[] Nullspace()
        {
            if (Magnitude == 0)
            {
                return new Vector3[] { UnitX, UnitY, UnitZ };
            }
            if (X != 0f || Y != 0f)
            {
                return new Vector3[] {
                    Cartesian(X * Z, Y * Z, -X * X - Y * Y).Normalized(),
                    Cartesian(-Y, X, 0).Normalized() 
                };
            }
            else
            {
                return new Vector3[] { UnitX, UnitY };
            }
        }

        public Vector3 UnitVector()
        {
            num m = Magnitude;
            if (m>0)
            {
                return new Vector3(X / m, Y / m, Z / m);
            }
            return this;
        }
        public Vector3 RotateZ(num angle)
        {
            num c = Cos(angle);
            num s = Sin(angle);
            return Cartesian(c * X - s * Y, s * X + c * Y, Z);
        }
        public Vector3 RotateX(num angle)
        {
            num c = Cos(angle);
            num s = Sin(angle);
            return Cartesian(X, c * Y - s * Z, s * Y + c * Z);
        }
        public Vector3 RotateY(num angle)
        {
            num c = Cos(angle);
            num s = Sin(angle);
            return Cartesian(c * Z - s * X, Y, s * Z + c * X);
        }
        public Vector3 Rotate(Vector3 axis, num angle)
        {
            return Rotations.FromAxisRotation(axis, angle) * this;
        }
        public Vector3 Slerp(num parameter, Vector3 A, Vector3 B)
        {
            num dot = Dot(A, B);
            if( dot == 0 ) { return A; }
            if( dot < 0 ) { A = -A; dot *= -1; }
            num angle = Acos(dot);
            num x1 = Sin((1 - parameter) * angle) / Sin(angle);
            num x2 = Sin(parameter * angle) / Sin(angle);
            return x1 * A + x2 * B;
        }

        #endregion

        #region Constants and Conversions
        public static implicit operator Vector3(num[] rhs)
        {
            return new Vector3(rhs);
        }

        public static explicit operator num[](Vector3 rhs)
        {
            return new num[] { rhs.X, rhs.Y, rhs.Z };
        }


        public static readonly Vector3 Zero = Cartesian(0f, 0f, 0f);
        public static readonly Vector3 UnitX = Cartesian(1f, 0f, 0f);
        public static readonly Vector3 UnitY = Cartesian(0f, 1f, 0f);
        public static readonly Vector3 UnitZ = Cartesian(0f, 0f, 1f);

        public bool Contains(double value)
        {
            return X==value
                || Y==value
                || Z==value;
        }
        #endregion

        #region Operators


        public static Vector3 operator +(Vector3 lhs, Vector3 rhs)
        {
            return Cartesian(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }
        public static Vector3 operator -(Vector3 lhs, Vector3 rhs)
        {
            return Cartesian(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        }
        public static Vector3 operator -(Vector3 rhs) { return rhs.Negative(); }
        public static num operator *(Vector3 lhs, Vector3 rhs) { return Dot(lhs, rhs); }
        public static Matrix3 operator &(Vector3 lhs, Vector3 rhs) { return Outer(lhs, rhs); }
        public static Vector3 operator *(num lhs, Vector3 rhs)
        {
            return new Vector3(lhs * rhs.X, lhs * rhs.Y, lhs * rhs.Z, rhs.IsTransposed);
        }
        public static Vector3 operator *(Vector3 lhs, num rhs)
        {
            return new Vector3(rhs * lhs.X, rhs * lhs.Y, rhs * lhs.Z, lhs.IsTransposed);
        }
        public static Vector3 operator /(Vector3 lhs, num rhs)
        {
            return new Vector3(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.IsTransposed);
        }
        public static Vector3 operator ^(Vector3 lhs, Vector3 rhs) { return Cross(lhs, rhs); }
        public static Vector3 operator ~(Vector3 rhs) { return rhs.Transpose(); }

        #endregion

        #region Geometry
        /// <summary>
        /// Linear Interpolation between two Vectors
        /// </summary>
        /// <returns>returns v+bias*dv</returns>
        public static Vector3 LinerInterpolation(Vector3 from_vector, Vector3 to_vector, num bias)
        {
            Vector3 diff = to_vector - from_vector;
            return from_vector + bias * diff;
        }
        /// <summary>
        /// Linear Interpolatin between three Vectors
        /// </summary>
        /// <returns>returns diag+bias_1*dv_1+bias_2*dv_2</returns>
        public static Vector3 Barycentric(Vector3 A, Vector3 B, Vector3 C, num bias_AB, num bias_AC)
        {
            Vector3 diff_AB = B - A;
            Vector3 diff_AC = C - A;
            return A + bias_AB * diff_AB + bias_AC * diff_AC;
        }

        /// <summary>
        /// Returns the point on lower_diag line closest to the origin.
        /// </summary>
        /// <amount name="origin">The origin location</amount>
        /// <amount name="pt_on_line">A point on the line</amount>
        /// <amount name="line_dir">The diretion of the line</amount>
        /// <returns>A vector to resulting point.</returns>
        public static Vector3 ClosestToLine(Vector3 origin, Vector3 pt_on_line, Vector3 line_dir)
        {
            Vector3 p = pt_on_line - origin;
            Vector3 e = line_dir.Normalized();
            Vector3 m1 = Cartesian(e.Y * e.Y + e.Z * e.Z, -e.X * e.Y, -e.X * e.Z);
            Vector3 m2 = Cartesian(m1.Y, 1f - e.Y * e.Y, -e.Y * e.Z);
            Vector3 m3 = Cartesian(m1.Z, m2.Z, 1f - e.Z * e.Z);
            return Cartesian(m1 * p, m2 * p, m3 * p);
        }


        public static num IncludedAngle(Vector3 A, Vector3 B)
        {
            Vector3 k = Cross(A, B);
            num sin_th = k.Magnitude;
            num cos_th = Dot(A, B);

            return Atan2(sin_th, cos_th);
        }
        public static num IncludedAngle(Vector3 corner, Vector3 vertex1, Vector3 vertex2)
        {
            return IncludedAngle(vertex1 - corner, vertex2 - corner);
        }
        #endregion

        #region IEnumerable Members

        public IEnumerator<num> GetEnumerator()
        {
            yield return X;
            yield return Y;
            yield return Z;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ICloneable Members

        public Vector3 Clone()
        {
            return Cartesian(X, Y, Z);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("[{0},{1},{2}]",
                X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider),
                Z.ToString(format, formatProvider)
            );
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToDerive()
        {
            return !IsTransposed ? string.Format("[[{0}],[{1}],[{2}]]", X, Y, Z) : string.Format("[[{0},{1},{2}]]", X, Y, Z);
        }
        #endregion


        #region IEquatable<V> Members

        public override bool Equals(object obj)
        {
            if(obj is Vector3)
            {
                return Equals((Vector3)obj);
            }
            return false;
        }

        public bool Equals(Vector3 other)
        {
            return X.Equals(other.X)
                && Y.Equals(other.Y)
                && Z.Equals(other.Z);
        }
        public bool Equals(Vector3 other, double tolerance)
        {
            return Abs(X-other.X)<=tolerance
                && Abs(Y-other.Y)<=tolerance
                && Abs(Z-other.Z)<=tolerance;
        }
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() << 4;
        }

        #endregion
    }
}
