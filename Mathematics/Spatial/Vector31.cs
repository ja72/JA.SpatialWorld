using System;
using System.Collections.Generic;
using System.Linq;

using static System.Math;

namespace JA.Mathematics.Spatial
{
    using num = Double;
    using vec3 = Vector3;
    /// <summary>
    /// A 4x1 block vector of [r,d] with r a Vector3 and d a scalar.
    /// </summary>
    public struct Vector31 : IEnumerable<num>
    {

        #region Constructors

        public Vector31(vec3 vector, num scalar)
        {
            this.Vector = vector;
            this.Scalar = scalar;
        }
        public Vector31(num x, num y, num z, num w)
        {
            this.Vector = new Vector3(x, y, z);
            this.Scalar = w;
        }

        #endregion

        #region Properties
        public double Scalar { get; }
        public vec3 Vector { get; }

        public num X { get { return Vector.X; } }
        public num Y { get { return Vector.Y; } }
        public num Z { get { return Vector.Z; } }
        public num W { get { return Scalar; } }

        public num this[int i]
        {
            get
            {
                if (i >= 0 && i < 3)
                {
                    return Vector[i];
                }
                else if (i == 3)
                {
                    return Scalar;
                }
                else
                    throw new IndexOutOfRangeException("i");
            }
        }

        public bool IsTransposed { get { return Vector.IsTransposed; } }
        
        #endregion

        #region Actions/Functions
        public Vector31 Normalized()
        {
            num mag = Sqrt(Vector.SumSquares  + Scalar*Scalar);
            if (mag>0)
            {
                if (mag!=1)
                {
                    return new Vector31(Vector / mag, Scalar / mag);
                }
                else
                {
                    return this;
                }
            }
            else
            {
                return new Vector31(vec3.Zero, 0f);
            }
        }

        public Vector31 Transpose()
        {
            return new Vector31(Vector.Transpose(), Scalar);
        }
        public double MaxAbs() => ToArray().Aggregate((max, x) => Max(Abs(x), max));
        #endregion

        #region static methods
        public static Vector31 Add(Vector31 lhs, Vector31 rhs)
        {
            return new Vector31(lhs.Vector + rhs.Vector, lhs.Scalar + rhs.Scalar);
        }
        public static Vector31 Subtract(Vector31 lhs, Vector31 rhs)
        {
            return new Vector31(lhs.Vector - rhs.Vector, lhs.Scalar - rhs.Scalar);
        }
        public static Vector31 Multiply(num lhs, Vector31 rhs)
        {
            return new Vector31(lhs * rhs.Vector, lhs * rhs.Scalar);
        }
        public static Vector31 Multiply(Matrix31 lhs, Vector31 rhs)
        {
            return new Vector31(lhs.A11 * rhs.Vector + lhs.Vector1 * rhs.Scalar, Vector3.Dot(lhs.Vector2,rhs.Vector) + lhs.Scalar * rhs.Scalar);
        }
        public static Vector31 Divide(Vector31 lhs, num rhs)
        {
            return new Vector31(lhs.Vector / rhs, lhs.Scalar / rhs);
        }
        public static num InnerProduct(Vector31 lhs, Vector31 rhs)
        {
            return vec3.Dot(lhs.Vector, rhs.Vector) + lhs.Scalar * rhs.Scalar;
        }
        #endregion

        #region Operators
        public static Vector31 operator +(Vector31 lhs, Vector31 rhs) { return Add(lhs, rhs); }
        public static Vector31 operator -(Vector31 lhs, Vector31 rhs) { return Subtract(lhs, rhs); }
        public static Vector31 operator *(num lhs, Vector31 rhs) { return Multiply(lhs, rhs); }
        public static Vector31 operator *(Vector31 lhs, num rhs) { return Multiply(rhs, lhs); }
        public static Vector31 operator *(Matrix31 lhs, Vector31 rhs) { return Multiply(lhs, rhs); }
        public static Vector31 operator /(Vector31 lhs, num rhs) { return Divide(lhs, rhs); }
        public static num operator *(Vector31 lhs, Vector31 rhs)
        {
            if( lhs.IsTransposed ^ rhs.IsTransposed ) //either one or the other need to be transposed
            {
                return vec3.Dot(lhs.Vector, rhs.Vector) + lhs.Scalar * rhs.Scalar; ;
            }
            throw new ArgumentException();
        }
        #endregion

        #region IVx<num> Members


        public int Size => 4;


        public num[] ToArray()
        {
            return new num[] { Vector.X, Vector.Y, Vector.Z, Scalar };
        }

        #endregion

        #region IEnumerable<num> Members

        public IEnumerator<num> GetEnumerator()
        {
            return ((IEnumerable<num>)ToArray()).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IFormattable Members

        public override string ToString()
        {
            return ToString("G3", System.Globalization.CultureInfo.CurrentCulture.NumberFormat);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ArrayFormatting.ToFixedColumnString(ToArray(), 11, format, formatProvider);
        }

        public string ToDerive()
        {
            return string.Format("[{0},{1},{2},{3}]", Vector.X, Vector.Y, Vector.Z, Scalar);
        }

        #endregion
    }
}
