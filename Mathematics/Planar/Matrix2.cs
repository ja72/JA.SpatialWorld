using System;

using static System.Math;

namespace JA.Mathematics.Planar
{

    using num = Double;
    using vec2 = Vector2;
    using mat2 = Matrix2;

    /// <summary>
    /// 2x2 Matrix used in planar rotations and projections
    /// </summary>
    public struct Matrix2 : ICloneable
    {
        const int dof = 2;

        #region Factory
        public Matrix2(double a11, double a12, double a21, double a22)
        {
            this.A11 = a11;
            this.A12 = a12;
            this.A21 = a21;
            this.A22 = a22;
        }

        public static readonly mat2 O = new mat2(0f, 0f, 0f, 0f);
        public static readonly mat2 I = new mat2(1f, 0f, 0f, 1f);

        public static mat2 Combine(vec2 ux, vec2 uy)
        {
            return new mat2(ux.X, uy.X, ux.Y, uy.Y);
        }

        public static mat2 Clone(mat2 rhs)
        {
            return rhs;
        }
        public static mat2 Symmetric(num a12)
        {
            return new mat2(0, a12, a12, 0);
        }
        public static mat2 Symmetric(num a11, num a12, num a22)
        {
            return new mat2(a11, a12, a12, a22);
        }
        public static mat2 SkewSymmetric(num a12)
        {
            return new mat2(0f, a12, -a12, 0f);
        }
        public static mat2 SkewSymmetric(num a11, num a12, num a22)
        {
            return new mat2(a11, a12, -a12, a22);
        }
        public static mat2 Rotation(num angle)
        {
            num c = Cos(angle); ;
            num s = Sin(angle);
            return new mat2(c, -s, s, c);
        }
        #endregion

        #region Properties
        public num this[int i, int j]
        {
            get
            {
                if (i == 0 && j == 0) { return A11; }
                if (i == 1 && j == 0) { return A21; }
                if (i == 0 && j == 1) { return A12; }
                if (i == 1 && j == 1) { return A22; }
                throw new ArgumentOutOfRangeException(string.Empty, "Index i or index j are out of range.");
            }
            set
            {
                if (i == 0 && j == 0) { A11 = value; return; }
                if (i == 1 && j == 0) { A21 = value; return; }
                if (i == 0 && j == 1) { A12 = value; return; }
                if (i == 1 && j == 1) { A22 = value; return; }
                throw new ArgumentOutOfRangeException(string.Empty, "Index i or index j are out of range.");
            }
        }

        public num Determinant { get { return A11 * A22 - A12 * A21; } }

        public vec2 UX { get { return vec2.Catersian(A11, A21); } set { A11 = value.X; A21 = value.Y; } }
        public vec2 UY { get { return vec2.Catersian(A12, A22); } set { A12 = value.X; A22 = value.Y; } }
        public vec2 Diagonal { get { return vec2.Catersian(A11, A22); } }

        public double A11 { get; set; }
        public double A12 { get; set; }
        public double A21 { get; set; }
        public double A22 { get; set; }

        #endregion

        #region Functions/Actions

        public num[] ToArray()
        {
            return new num[] { A11, A21, A12, A22};
        }
        public num[][] ToArray2()
        {
            return new num[][] {
                new num[] { A11, A12},
                new num[] { A21, A22}
            };
        }

        public mat2 Transpose()
        {
            return new mat2(A11, A21, A12, A22);
        }

        public static void Transpose(mat2 lhs, ref mat2 res)
        {
            res = new mat2(lhs.A11, lhs.A21, lhs.A12, lhs.A22);
        }

        public mat2 Solve(mat2 rhs)
        {
            mat2 res = mat2.O;
            Solve(this, rhs, ref res);
            return res;
        }
        public vec2 Solve(vec2 rhs)
        {
            vec2 res = vec2.O;
            Solve(this, rhs, ref res);
            return res;
        }
        /// <summary>
        /// Solves the system of equations lhs*v=rhs
        /// </summary>
        public static void Solve(mat2 lhs, vec2 rhs, ref vec2 v)
        {
            num D = lhs.Determinant;
            v.X = ((lhs.A22*rhs.X - lhs.A12*rhs.Y) / D);
            v.Y = -((lhs.A11*rhs.Y-lhs.A21*rhs.X) / D);
        }
        /// <summary>
        /// Solves the system of equations lhs*res = rhs
        /// </summary>
        public static void Solve(mat2 lhs, mat2 rhs, ref mat2 res)
        {
            Inverse(lhs, ref res);
            Multiply(res, rhs, ref res);
        }

        /// <summary>
        /// Returns the inverse
        /// </summary>
        public mat2 Inverse()
        {
            mat2 res = O;
            Inverse(this, ref res);
            return res;
        }

        public mat2 Rotate(num angle)
        {
            return Combine(UX.Rotate(angle), UY.Rotate(angle));
        }

        /// <summary>
        /// Calculates the inverse of a 2x2 matrix
        /// </summary>
        /// <param name="lhs">Input Matrix</param>
        /// <param name="res">Resuting inverse</param>
        public static void Inverse(mat2 lhs, ref mat2 res)
        {
            num D = lhs.Determinant;
            res.A11 = (lhs.A22 / D);
            res.A12 = (-lhs.A12 / D);
            res.A21 = (-lhs.A21 / D);
            res.A22 = (lhs.A11 / D);
        }

        #endregion

        #region Arithmetic
        public static void Add(mat2 lhs, mat2 rhs, ref mat2 res)
        {
            res.A11 = (lhs.A11 + rhs.A11);
            res.A21 = (lhs.A21 + rhs.A21);
            res.A12 = (lhs.A12 + rhs.A12);
            res.A22 = (lhs.A22 + rhs.A22);
        }
        public static void Subtract(mat2 lhs, mat2 rhs, ref mat2 res)
        {
            res.A11 = (lhs.A11 - rhs.A11);
            res.A21 = (lhs.A21 - rhs.A21);
            res.A12 = (lhs.A12 - rhs.A12);
            res.A22 = (lhs.A22 - rhs.A22);
        }
        public static void Negate(mat2 rhs, ref mat2 res)
        {
            res.A11 = (- rhs.A11);
            res.A21 = (- rhs.A21);
            res.A12 = (- rhs.A12);
            res.A22 = (- rhs.A22);
        }
        public static void Multiply(num lhs, mat2 rhs, ref mat2 res)
        {
            res.A11 = (lhs * rhs.A11);
            res.A21 = (lhs * rhs.A21);
            res.A12 = (lhs * rhs.A12);
            res.A22 = (lhs * rhs.A22);
        }
        public static void Multiply(mat2 lhs, mat2 rhs, ref mat2 res)
        {
            res.A11 = (lhs.A11 * rhs.A11 + lhs.A12 * rhs.A21);
            res.A21 = (lhs.A21 * rhs.A11 + lhs.A22 * rhs.A21);
            res.A12 = (lhs.A11 * rhs.A12 + lhs.A12 * rhs.A22);
            res.A22 = (lhs.A21 * rhs.A12 + lhs.A22 * rhs.A22);
        }
        public static void Multiply(mat2 lhs, vec2 rhs, ref vec2 res)
        {
            res.X = (lhs.A11 * rhs.X + lhs.A12 * rhs.Y);
            res.Y = (lhs.A21 * rhs.X + lhs.A22 * rhs.Y);
        }
        public static void Multiply(vec2 lhs, mat2 rhs, ref vec2 res)
        {
            res.X = (lhs.X * rhs.A11 + lhs.Y * rhs.A21);
            res.Y = (lhs.X * rhs.A12 + lhs.Y * rhs.A22);
        }
        public static mat2 operator +(mat2 lhs, mat2 rhs)
        {
            mat2 res = O;
            Add(lhs, rhs, ref res);
            return res;
        }
        public static mat2 operator -(mat2 lhs, mat2 rhs)
        {
            mat2 res = O ;
            Subtract(lhs, rhs, ref res);
            return res;
        }
        public static mat2 operator -(mat2 rhs)
        {
            mat2 res = O;
            Negate(rhs, ref res);
            return res;
        }
        public static mat2 operator *(mat2 lhs, mat2 rhs)
        {
            mat2 res = O;
            Multiply(lhs, rhs, ref res);
            return res;
        }
        public static mat2 operator *(num lhs, mat2 rhs)
        {
            mat2 res = O;
            Multiply(lhs, rhs, ref res);
            return res;
        }
        public static mat2 operator *(mat2 lhs, num rhs)
        {
            mat2 res = O;
            Multiply(rhs, lhs, ref res);
            return res;
        }
        public static mat2 operator /(mat2 lhs, num rhs)
        {
            mat2 res = O;
            Multiply(1f/rhs, lhs, ref res);
            return res;
        }
        public static vec2 operator *(mat2 lhs, vec2 rhs)
        {
            vec2 res = vec2.O;
            Multiply(lhs, rhs, ref res);
            return res;
        }
        public static vec2 operator *(vec2 lhs, mat2 rhs)
        {
            vec2 res = vec2.O;
            Multiply(lhs, rhs, ref res);
            return res;
        }
        public static mat2 operator ~(mat2 rhs) { return rhs.Transpose(); }
        public static explicit operator mat2(num value)
        {
            return new mat2(value, 0f, 0f, value);
        }
        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone(this);
        }

        #endregion
    }
}
