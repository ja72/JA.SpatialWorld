using System;
using System.Collections.Generic;
using System.Diagnostics;

using static System.Math;

namespace JA.Mathematics.Planar
{

    //using num = Double;

    [DebuggerDisplay("({X},{Y})")]
    public struct Vector2 : ICloneable, IEnumerable<double>
    {
        const int dof = 2;

        public Vector2(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        #region Factory
        public static Vector2 Catersian(double x, double y)
        {
            return new Vector2(x, y);
        }
        public static Vector2 Polar(double radius, double angle)
        {
            return new Vector2(radius*Cos(angle), radius*Sin(angle));
        }
        #endregion

        #region Functions/Properties
        public Vector2 Rotate(double angle)
        {
            return new Vector2(
                X * Math.Cos(angle) - Y * Math.Sin(angle),
                X * Math.Sin(angle) + Y * Math.Cos(angle));
        }

        public double SumSquares { get { return X * X + Y * Y; } }
        public double Magnitude { get { return Sqrt(SumSquares); } }
        public void Normalize()
        {
            double m = Magnitude;
            if (m != 0d)
            {
                X /= m;
                Y /= m;
            }
        }
        public Vector2 Normalized()
        {
            double m = Magnitude;
            if (m==0d)
            {
                return Vector2.O;
            }
            return Catersian(X / m, Y / m);
        }

        public Vector2 Cross()
        {
            return Catersian(-Y, X);
        }
        public static double Inner(Vector2 v, Vector2 w)
        {
            return v.X * w.X + v.Y * w.Y;
        }
        public static double Cross(Vector2 v, Vector2 w)
        {
            return v.X * w.Y - v.Y * w.X;
        }
        public double DirectionAngle
        {
            get { return Atan2(Y, X); }
        }

        public bool IsZero() { return SumSquares==0; }
        public System.Drawing.PointF ToPointF() { return new System.Drawing.PointF((float)X, (float)Y); }
        public System.Drawing.SizeF ToSizeF() { return new System.Drawing.SizeF((float)X, (float)Y); }
        #endregion

        public static readonly Vector2 O = Catersian(0d, 0d);
        public static readonly Vector2 I = Catersian(1d, 0d);
        public static readonly Vector2 J = Catersian(0d, 1d);

        #region Operators
        public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
        { return Vector2.Catersian(lhs.X + rhs.X, lhs.Y + rhs.Y); }
        public static Vector2 operator -(Vector2 rhs)
        { return Vector2.Catersian(-rhs.X, -rhs.Y); }
        public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
        { return Vector2.Catersian(lhs.X - rhs.X, lhs.Y - rhs.Y); }
        public static double operator *(Vector2 lhs, Vector2 rhs)
        { return lhs.X * rhs.X + lhs.Y * rhs.Y; }
        public static Vector2 operator *(double lhs, Vector2 rhs)
        { return Vector2.Catersian(lhs * rhs.X, lhs *  rhs.Y); }
        public static Vector2 operator *(Vector2 lhs, double rhs)
        { return Vector2.Catersian(lhs.X * rhs, lhs.Y * rhs); }
        public static Vector2 operator /(Vector2 lhs, double rhs)
        { return Vector2.Catersian(lhs.X / rhs, lhs.Y / rhs); }

        public double Inner(Vector2 other)
        {
            return X*other.X+Y*other.Y;
        }
        public Matrix2 Outer(Vector2 other)
        {
            return new Matrix2(
                X*other.X, X*other.Y,
                Y*other.X, Y*other.Y);
        }
        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Vector2 Clone() { return Catersian(X, Y); }

        #endregion

        #region IEquatable Members

        /// <summary>
        /// Equality overrides from <see cref="System.Object"/>
        /// </summary>
        /// <param name="obj">The object to compare this with</param>
        /// <returns>False if object is a different type, otherwise it calls <code>Equals(Vector2)</code></returns>
        public override bool Equals(object obj)
        {
            if (obj is Vector2)
            {
                return Equals((Vector2)obj);
            }
            return false;
        }

        /// <summary>
        /// Checks for equality among <see cref="Vector2"/> classes
        /// </summary>
        /// <param name="other">The other <see cref="Vector2"/> to compare it to</param>
        /// <returns>True if equal</returns>
        public bool Equals(Vector2 other)
        {
            return X.Equals(other.X)&&Y.Equals(other.Y);
        }

        /// <summary>
        /// Calculates the hash code for the <see cref="Vector2"/>
        /// </summary>
        /// <returns>The int hash value</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (17*23+X.GetHashCode())*23+Y.GetHashCode();
            }
        }

        #endregion

        #region IVx<double> Members

        public void SwapElements(int index1, int index2)
        {
            if (index1!=index2)
            {
                var e1 = this[index1];
                var e2 = this[index2];
                this[index1]=e2;
                this[index2]=e1;
            }
        }

        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y=value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public int Size
        {
            get { return 2; }
        }

        public double X { get; set; }
        public double Y { get; set; }

        public double[] ToArray()
        {
            return new double[] { X, Y };
        }

        #endregion

        #region IEnumerable<double> Members

        public IEnumerator<double> GetEnumerator()
        {
            yield return X;
            yield return Y;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
