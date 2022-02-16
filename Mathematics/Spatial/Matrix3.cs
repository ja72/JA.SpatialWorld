using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using static System.Math;

namespace JA.Mathematics.Spatial
{
    using vec3 = Vector3;
    using mat3 = Matrix3;

    public struct Matrix3 : IEnumerable<double>, IFormattable, IEquatable<mat3>
    {

        public static readonly mat3 Zero = new mat3(0, 0, 0, 0, 0, 0, 0, 0, 0);
        public static readonly mat3 Identity = new mat3(1, 0, 0, 0, 1, 0, 0, 0, 1);

        #region Factory

        public Matrix3(
            double a11, double a12, double a13,
            double a21, double a22, double a23,
            double a31, double a32, double a33)
        {
            this.A11 = a11; this.A21 = a21; this.A31 = a31;
            this.A12 = a12; this.A22 = a22; this.A32 = a32;
            this.A13 = a13; this.A23 = a23; this.A33 = a33;
        }

        public Matrix3(params double[] elements)
        : this(elements[0], elements[1], elements[2],
                elements[3], elements[4], elements[5],
                elements[6], elements[7], elements[8])
        { }


        public Matrix3(double[][] rhs)
        : this(rhs[0][0], rhs[0][1], rhs[0][2],
                rhs[1][0], rhs[1][1], rhs[1][2],
                rhs[2][0], rhs[2][1], rhs[2][2])
        { }


        public static mat3 Diagonal(double value)
        {
            return value*Identity;
        }


        public static mat3 Diagonal(int offset, params double[] values)
        {
            if (offset == 0 && values.Length == 3)
            {
                return Diagonal(values[0], values[1], values[2]);
            }
            if (offset == 1 && values.Length == 2)
            {
                return new mat3(0, values[0], 0, 0, 0, values[1], 0, 0, 0);
            }
            if (offset == -1 && values.Length == 2)
            {
                return new mat3(0, 0, 0, values[0], 0, 0, 0, values[1], 0);
            }
            if (offset == 2 && values.Length == 1)
            {
                return new mat3(0, 0, values[0], 0, 0, 0, 0, 0, 0);
            }
            if (offset == -2 && values.Length == 1)
            {
                return new mat3(0, 0, 0, 0, 0, 0, values[0], 0, 0);
            }
            throw new ArgumentException(nameof(offset));
        }

        public static mat3 Diagonal(double a11, double a22, double a33)
        {
            return new mat3(a11, 0, 0, 0, a22, 0, 0, 0, a33);
        }
        public static mat3 Symmetric(double a11, double a22, double a33, double a12, double a13, double a23)
        {
            return new mat3(a11, a12, a13, a12, a22, a23, a13, a23, a33);
        }
        public static mat3 SkewSymmetric(double a12, double a13, double a23)
        {
            return new mat3(0, a12, a13, -a12, 0, a23, -a13, -a23, 0);
        }

        public static mat3 Combine(vec3 unit_x, vec3 unit_y, vec3 unit_z)
        {
            return new mat3(
                unit_x.X, unit_y.X, unit_z.Z,
                unit_x.Y, unit_y.Y, unit_z.Y,
                unit_x.Z, unit_y.Z, unit_z.Z);
        }

        #endregion

        #region Properties
        public int Rows => 3;
        public int Columns => 3;

        public double A11 { get; }
        public double A21 { get; }
        public double A31 { get; }
        public double A12 { get; }
        public double A22 { get; }
        public double A32 { get; }
        public double A13 { get; }
        public double A23 { get; }
        public double A33 { get; }

        public double this[int row, int col]
        {
            get
            {
                if (col==0)
                {
                    if (row==0) { return A11; }
                    if (row==1) { return A21; }
                    if (row==2) { return A31; }
                }
                if (col==1)
                {
                    if (row==0) { return A12; }
                    if (row==1) { return A22; }
                    if (row==2) { return A32; }
                }
                if (col==2)
                {
                    if (row==0) { return A13; }
                    if (row==1) { return A23; }
                    if (row==2) { return A33; }
                }
                throw new ArgumentOutOfRangeException(nameof(col));
            }
        }

        public vec3 UX => vec3.Cartesian(A11, A21, A31);
        public vec3 UY => vec3.Cartesian(A12, A22, A32);
        public vec3 UZ => vec3.Cartesian(A13, A23, A33);

        #endregion

        #region Functions

        public vec3 Column(int index)
        {
            switch (index)
            {
                case 0:
                    return vec3.Cartesian(A11, A21, A31);
                case 1:
                    return vec3.Cartesian(A12, A22, A32);
                case 2:
                    return vec3.Cartesian(A13, A23, A33);
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
        public vec3 Row(int index)
        {
            switch (index)
            {
                case 0:
                    return new vec3(A11, A12, A13, true);
                case 1:
                    return new vec3(A21, A22, A23, true);
                case 2:
                    return new vec3(A31, A32, A33, true);
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public double[] ToArray()
        {
            return new double[] { A11, A21, A31, A12, A22, A32, A13, A23, A33 };
        }
        public double[][] ToArray2()
        {
            return new double[][] {
                new double[] { A11, A12,  A13},
                new double[] { A21, A22, A23},
                new double[] { A31, A32, A33}
            };
        }

        public static mat3 Add(mat3 lhs, mat3 rhs)
        {
            return new mat3(
                (lhs.A11 + rhs.A11), (lhs.A12 + rhs.A12), (lhs.A13 + rhs.A13),
                (lhs.A21 + rhs.A21), (lhs.A22 + rhs.A22), (lhs.A23 + rhs.A23),
                (lhs.A31 + rhs.A31), (lhs.A32 + rhs.A32), (lhs.A33 + rhs.A33)
                );
        }

        public static mat3 Subtract(mat3 lhs, mat3 rhs)
        {
            return new mat3(
                (lhs.A11 - rhs.A11), (lhs.A12 - rhs.A12), (lhs.A13 - rhs.A13),
                (lhs.A21 - rhs.A21), (lhs.A22 - rhs.A22), (lhs.A23 - rhs.A23),
                (lhs.A31 - rhs.A31), (lhs.A32 - rhs.A32), (lhs.A33 - rhs.A33)
                );
        }

        public static mat3 Negate(mat3 rhs)
        {
            return new mat3(-rhs.A11, -rhs.A12, -rhs.A13, -rhs.A21, -rhs.A22, -rhs.A23, -rhs.A31, -rhs.A32, -rhs.A33);
        }

        public static mat3 Multiply(double lhs, mat3 rhs)
        {
            return new mat3(
                lhs * rhs.A11, lhs * rhs.A12, lhs * rhs.A13,
                lhs * rhs.A21, lhs * rhs.A22, lhs * rhs.A23,
                lhs * rhs.A31, lhs * rhs.A32, lhs * rhs.A33
                );
        }


        public static mat3 Divide(mat3 lhs, double rhs)
        {
            return new mat3(
                lhs.A11 / rhs, lhs.A12 / rhs, lhs.A13 / rhs,
                lhs.A21 / rhs, lhs.A22 / rhs, lhs.A23 / rhs,
                lhs.A31 / rhs, lhs.A32 / rhs, lhs.A33 / rhs
                );
        }

        public static vec3 Multiply(mat3 lhs, vec3 rhs)
        {
            if (!rhs.IsTransposed)
            {
                return new vec3(
                    (lhs.A11 * rhs.X + lhs.A12 * rhs.Y + lhs.A13 * rhs.Z),
                    (lhs.A21 * rhs.X + lhs.A22 * rhs.Y + lhs.A23 * rhs.Z),
                    (lhs.A31 * rhs.X + lhs.A32 * rhs.Y + lhs.A33 * rhs.Z),
                    false);
            }
            else
                throw new ArgumentException("rhs should not be transposed.");
        }
        public static vec3 Multiply(vec3 lhs, mat3 rhs)
        {
            if (lhs.IsTransposed)
            {
                return new vec3(
                    (rhs.A11 * lhs.X + rhs.A21 * lhs.Y + rhs.A31 * lhs.Z),
                    (rhs.A12 * lhs.X + rhs.A22 * lhs.Y + rhs.A32 * lhs.Z),
                    (rhs.A13 * lhs.X + rhs.A23 * lhs.Y + rhs.A33 * lhs.Z),
                    true);
            }
            else
                throw new ArgumentException("lhs should be transposed.");
        }

        public static mat3 Multiply(mat3 lhs, mat3 rhs)
        {
            return new mat3(
                (lhs.A11 * rhs.A11 + lhs.A12 * rhs.A21 + lhs.A13 * rhs.A31),
                (lhs.A11 * rhs.A12 + lhs.A12 * rhs.A22 + lhs.A13 * rhs.A32),
                (lhs.A11 * rhs.A13 + lhs.A12 * rhs.A23 + lhs.A13 * rhs.A33),

                (lhs.A21 * rhs.A11 + lhs.A22 * rhs.A21 + lhs.A23 * rhs.A31),
                (lhs.A21 * rhs.A12 + lhs.A22 * rhs.A22 + lhs.A23 * rhs.A32),
                (lhs.A21 * rhs.A13 + lhs.A22 * rhs.A23 + lhs.A23 * rhs.A33),

                (lhs.A31 * rhs.A11 + lhs.A32 * rhs.A21 + lhs.A33 * rhs.A31),
                (lhs.A31 * rhs.A12 + lhs.A32 * rhs.A22 + lhs.A33 * rhs.A32),
                (lhs.A31 * rhs.A13 + lhs.A32 * rhs.A23 + lhs.A33 * rhs.A33)
                );
        }

        public double MaxAbs() => ToArray().Aggregate((max, x) => Max(Abs(x), max));

        public mat3 Transpose()
        {
            return new mat3(A11, A21, A31, A12, A22, A32, A13, A23, A33);
        }

        public double Determinant => A11 * (A22 * A33 - A23 * A32) + A12 * (A23 * A31 - A21 * A33) + A13 * (A21 * A32 - A22 * A31);

        public vec3 DiagonalVector => vec3.Cartesian(A11, A22, A33);

        public mat3 DiagonalMatrix => mat3.Diagonal(A11, A22, A33);

        public double Trace => A11 + A22 + A33;

        /// <summary>
        /// Calculates the inverse 4x4 matrix
        /// </summary>
        /// <returns></returns>
        public mat3 Inverse()
        {
            double D = Determinant;
            if (D > 0)
            {
                return new mat3(
                    ((A22 * A33 - A23 * A32) / D),
                    ((A13 * A32 - A12 * A33) / D),
                    ((A12 * A23 - A13 * A22) / D),

                    ((A23 * A31 - A21 * A33) / D),
                    ((A11 * A33 - A13 * A31) / D),
                    ((A13 * A21 - A11 * A23) / D),

                    ((A21 * A32 - A22 * A31) / D),
                    ((A12 * A31 - A11 * A32) / D),
                    ((A11 * A22 - A12 * A21) / D)
                    );
            }
            else
            {
                return mat3.Zero;
            }
        }

        /// <summary>
        /// Solves the system of equations lhs*v=rhs
        /// </summary>
        public vec3 Solve(vec3 rhs)
        {
            double D = Determinant;
            return vec3.Cartesian(
                 (((A22 * A33 - A23 * A32) * rhs.X + (A13 * A32 - A12 * A33) * rhs.Y + (A12 * A23 - A13 * A22) * rhs.Z) / D),
                -(((A21 * A33 - A23 * A31) * rhs.X + (A13 * A31 - A11 * A33) * rhs.Y + (A11 * A23 - A13 * A21) * rhs.Z) / D),
                 (((A21 * A32 - A22 * A31) * rhs.X + (A12 * A31 - A11 * A32) * rhs.Y + (A11 * A22 - A12 * A21) * rhs.Z) / D)
            );
        }
        /// <summary>
        /// Solves the system of equations lhs*res = rhs
        /// </summary>
        public mat3 Solve(mat3 rhs)
        {
            double D = Determinant;
            return new mat3(
                 (((A22 * A33 - A23 * A32) * rhs.A11 + (A13 * A32 - A12 * A33) * rhs.A21 + (A12 * A23 - A13 * A22) * rhs.A31) / D),
                 (((A22 * A33 - A23 * A32) * rhs.A12 + (A13 * A32 - A12 * A33) * rhs.A22 + (A12 * A23 - A13 * A22) * rhs.A32) / D),
                 (((A22 * A33 - A23 * A32) * rhs.A13 + (A13 * A32 - A12 * A33) * rhs.A23 + (A12 * A23 - A13 * A22) * rhs.A33) / D),

                -(((A21 * A33 - A23 * A31) * rhs.A11 + (A13 * A31 - A11 * A33) * rhs.A21 + (A11 * A23 - A13 * A21) * rhs.A31) / D),
                -(((A21 * A33 - A23 * A31) * rhs.A12 + (A13 * A31 - A11 * A33) * rhs.A22 + (A11 * A23 - A13 * A21) * rhs.A32) / D),
                -(((A21 * A33 - A23 * A31) * rhs.A13 + (A13 * A31 - A11 * A33) * rhs.A23 + (A11 * A23 - A13 * A21) * rhs.A33) / D),

                (((A21 * A32 - A22 * A31) * rhs.A11 + (A12 * A31 - A11 * A32) * rhs.A21 + (A11 * A22 - A12 * A21) * rhs.A31) / D),
                (((A21 * A32 - A22 * A31) * rhs.A12 + (A12 * A31 - A11 * A32) * rhs.A22 + (A11 * A22 - A12 * A21) * rhs.A32) / D),
                (((A21 * A32 - A22 * A31) * rhs.A13 + (A12 * A31 - A11 * A32) * rhs.A23 + (A11 * A22 - A12 * A21) * rhs.A33) / D)
            );

        }


        #endregion

        #region Operators
        public static mat3 operator +(double lhs, mat3 rhs) => Add(Matrix3.Diagonal(lhs), rhs);
        public static mat3 operator +(mat3 lhs, mat3 rhs) => Add(lhs, rhs);
        public static mat3 operator -(mat3 lhs, mat3 rhs) => Subtract(lhs, rhs);
        public static mat3 operator -(mat3 rhs) => Negate(rhs);
        public static mat3 operator *(double lhs, mat3 rhs) => Multiply(lhs, rhs);
        public static vec3 operator *(mat3 lhs, vec3 rhs) => Multiply(lhs, rhs);
        public static mat3 operator *(mat3 lhs, mat3 rhs) => Multiply(lhs, rhs);
        public static vec3 operator *(vec3 lhs, mat3 rhs) => Multiply(lhs, rhs);
        public static mat3 operator /(mat3 lhs, double rhs) => Divide(lhs, rhs);
        public static mat3 operator ~(mat3 rhs) => rhs.Transpose();
        public static mat3 operator !(mat3 rhs) => rhs.Inverse();
        #endregion

        #region IEnumerable<double> Members

        public IEnumerator<double> GetEnumerator()
        {
            yield return A11;
            yield return A21;
            yield return A31;
            yield return A12;
            yield return A22;
            yield return A23;
            yield return A13;
            yield return A23;
            yield return A33;
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #endregion

        #region IFormattable Members

        public override string ToString()
        {
            return string.Format("[{0},{1},{2}|{3},{4},{5}|{6},{7},{8}]", A11, A12, A13, A21, A22, A23, A31, A32, A33);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ArrayFormatting.ToFixedColumnString(ToArray2(), 11, 3);
        }

        public string ToDerive()
        {
            return string.Format("[[{0},{1},{2}],[{3},{4},{5}],[{6},{7},{8}]]", A11, A12, A13, A21, A22, A23, A31, A32, A33);
        }
        #endregion


        #region IEquatable<M> Members

        public override bool Equals(object obj)
        {
            if (obj is mat3)
            {
                return Equals((mat3)obj);
            }
            return false;
        }

        public bool Equals(mat3 other)
        {
            return A11.Equals(other.A11) && A12.Equals(other.A12) && A13.Equals(other.A13)
                && A21.Equals(other.A21) && A22.Equals(other.A22) && A23.Equals(other.A23)
                && A31.Equals(other.A31) && A32.Equals(other.A32) && A33.Equals(other.A33);
        }

        public override int GetHashCode()
        {
            int res = 0;
            int k = 0;
            Array.ForEach(ToArray(), delegate (double value)
            {
                res ^= value.GetHashCode() << k++;
            });
            return res;
        }

        #endregion
    }
}
