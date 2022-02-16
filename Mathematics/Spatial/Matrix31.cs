using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

using static System.Math;

namespace JA.Mathematics.Spatial
{
    /// <summary>
    /// A 4x4 block matrix decomposed into [ A11, A12 ; ~A21, A22 ]  where A11:{3x3}, A12,A21:{3x1} and A22:scalar
    /// </summary>
    public struct Matrix31 : IEnumerable<double>
    {

        #region Factory
        public Matrix31(Matrix3 matrix, Vector3 vector1, Vector3 vector2, double scalar)
        {
            this.Matrix = matrix;
            this.Vector1 = vector1;
            this.Vector2 = vector2;
            this.Scalar = scalar;
        }

        public Matrix31(double[][] values)
        {
            this.Matrix = new Matrix3(values[0][0], values[0][1], values[0][2],
                                values[1][0], values[1][1], values[1][2],
                                values[2][0], values[2][1], values[2][2]);
            this.Vector1 = new Vector3(values[0][3], values[1][3], values[2][3]);
            this.Vector2 = new Vector3(values[3][0], values[3][1], values[3][2]);
            this.Scalar = values[3][3];
        }

        public Matrix31(double A11, double A12, double A13, double A14,
            double A21, double A22, double A23, double A24,
            double A31, double A32, double A33, double A34,
            double A41, double A42, double A43, double A44)
        {

            this.Matrix = new Matrix3(A11, A12, A13, A21, A22, A23, A31, A32, A33);
            this.Vector1 = new Vector3(A14, A24, A34, false);
            this.Vector2 = new Vector3(A41, A42, A43, true);
            this.Scalar = A44;
        }

        public Matrix31(double[] elements)
        {

            this.Matrix = new Matrix3(elements[0], elements[1], elements[2],
                            elements[4], elements[5], elements[6],
                            elements[8], elements[9], elements[10]);
            this.Vector1 = new Vector3(elements[3], elements[7], elements[11]);
            this.Vector2 = new Vector3(elements[12], elements[13], elements[14]);
            this.Scalar = elements[15];
        }

        public static readonly Matrix31 Zero = new Matrix31(Matrix3.Zero, Vector3.Zero, ~Vector3.Zero, 0f);
        public static readonly Matrix31 Identity = new Matrix31(Matrix3.Identity, Vector3.Zero, ~Vector3.Zero, 1f);
        #endregion

        #region Properties
        public Matrix3 A11 { get => Matrix; }
        public Vector3 Vector2 { get; }
        public Vector3 Vector1 { get; }
        public double Scalar { get; }

        public int Rows => 4;
        public int Columns => 4;

        public Matrix3 Matrix { get; }

        public double this[int i, int j] 
        {
            get
            {
                if (i >= 0 && i < 3)
                {
                    if (j >= 0 && j < 3)
                    {
                        return A11[i, j];
                    }
                    else if (j == 3)
                    {
                        return Vector1[i];
                    }
                    else
                        throw new ArgumentOutOfRangeException(nameof(j));
                }
                else if (i == 3)
                {
                    if (j >= 0 && j < 3)
                    {
                        return Vector2[j];
                    }
                    else if (j == 3)
                    {
                        return Scalar;
                    }
                    else
                        throw new ArgumentOutOfRangeException(nameof(j));
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(i));
            }
        }

        #endregion

        #region Functions/Actions

        public double[] ToArray()
        {
            return new double[] { 
                A11.A11, A11.A12, A11.A13, Vector1.X, 
                A11.A21, A11.A22, A11.A23, Vector1.Y, 
                A11.A31, A11.A32, A11.A33, Vector1.Z, 
                Vector2.X, Vector2.Y, Vector2.Z, Scalar };
        }

        public double[][] ToArray2()
        {
            return new double[][] { 
                new double[] {A11.A11, A11.A12, A11.A13, Vector1.X},
                new double[] {A11.A21, A11.A22, A11.A23, Vector1.Y}, 
                new double[] {A11.A31, A11.A32, A11.A33, Vector1.Z}, 
                new double[] {Vector2.X, Vector2.Y, Vector2.Z, Scalar} };
        }

        public Matrix31 Transpose()
        {
            return new Matrix31(~A11, ~Vector2, ~Vector1, Scalar);
        }
        
        public Matrix31 Inverse()
        {
            Vector3 C12 = Vector1 / Scalar;
            Vector3 C21 = Vector2 * A11.Inverse();

            Matrix3 D11 = A11 - (Vector1 & Vector2) / Scalar;
            double D22 = Scalar - Vector2 * A11.Solve(Vector1);

            Matrix3 F11 = D11.Inverse();
            Vector3 F12 = -D11.Solve(C12);
            Vector3 F21 = -C21 / D22;
            double F22 = 1f / D22;

            return new Matrix31( F11, F12, F21, F22 );

        }

        /// <summary>
        /// Solve a system of A*x=b for x based on block matrix. Method taken from <see cref="http://matrixcookbook.com/"/>
        /// </summary>
        /// <param name="rhs">The right hand vector b</param>
        /// <returns>The solution vector x</returns>
        public Vector31 Solve(Vector31 rhs)
        {
            Vector3 c1 = rhs.Vector - Vector1 * (rhs.Scalar / Scalar);
            double c2 = rhs.Scalar - Vector2 * A11.Solve(rhs.Vector);

            Matrix3 D11 = A11 - (Vector1 & Vector2) / Scalar;
            double D22 = Scalar - Vector2 * A11.Solve(Vector1);

            Vector3 x1 = D11.Solve(c1);
            double x2 = c2 / D22;

            return new Vector31(x1, x2);

        }
        public Matrix31 Solve(Matrix31 rhs)
        {

            Vector3 C12 = Vector1 / Scalar;
            Vector3 C21 = Vector2 * A11.Inverse();

            Matrix3 D11 = A11 - (Vector1 & Vector2) / Scalar;
            double D22 = Scalar - Vector2 * A11.Solve(Vector1);

            Matrix3 D11_inv = D11.Inverse();

            Matrix3 E11 = rhs.A11 - (rhs.Vector1 & C12);
            Vector3 E12 = rhs.Vector1 - rhs.Scalar * C12;
            Vector3 E21 = rhs.Vector2 - rhs.A11 * C21;
            double E22 = rhs.Scalar - rhs.Vector1 * C21;

            Matrix3 F11 = E11 * D11_inv;
            Vector3 F12 = E12 * D11_inv;
            Vector3 F21 = E21 / D22;
            double F22 = E22 / D22;

            return new Matrix31(F11, F12, F21, F22);
        }

        public double MaxAbs() => ToArray().Aggregate((max, x) => Max(Abs(x), max));
        #endregion

        #region Static Methods
        public static Matrix31 Add(Matrix31 lhs, Matrix31 rhs) { return new Matrix31(lhs.A11 + rhs.A11, lhs.Vector1 + rhs.Vector1, lhs.Vector2 + rhs.Vector2, lhs.Scalar + rhs.Scalar); }
        public static Matrix31 Subtract(Matrix31 lhs, Matrix31 rhs) { return new Matrix31(lhs.A11 - rhs.A11, lhs.Vector1 - rhs.Vector1, lhs.Vector2 - rhs.Vector2, lhs.Scalar - rhs.Scalar); }
        public static Matrix31 Multiply(double lhs, Matrix31 rhs) { return new Matrix31(lhs * rhs.A11, lhs * rhs.Vector1, lhs * rhs.Vector2, lhs * rhs.Scalar); }
        public static Matrix31 Divide(Matrix31 lhs, double rhs) { return new Matrix31(lhs.A11 / rhs, lhs.Vector1 / rhs, lhs.Vector2 / rhs, lhs.Scalar / rhs); }
        public static Matrix31 Multiply(Matrix31 lhs, Matrix31 rhs)
        {
            Matrix3 A = lhs.A11 * rhs.A11 + (lhs.Vector1 & rhs.Vector2);
            Vector3 b = lhs.A11 * rhs.Vector1 + lhs.Vector1 * rhs.Scalar;
            Vector3 c = lhs.Vector2 * rhs.A11 + lhs.Scalar * rhs.Vector2;
            double d = (lhs.Vector2 * rhs.Vector1) + lhs.Scalar * rhs.Scalar;
            return new Matrix31(A, b, c, d);
        }
        #endregion

        #region Operators
        public static Matrix31 operator +(Matrix31 lhs, Matrix31 rhs) { return Add(lhs, rhs); }
        public static Matrix31 operator -(Matrix31 lhs, Matrix31 rhs) { return Subtract(lhs, rhs); }
        public static Matrix31 operator *(double lhs, Matrix31 rhs) { return Multiply(lhs, rhs); }
        public static Matrix31 operator /(Matrix31 lhs, double rhs) { return Divide(lhs, rhs); } 
        public static Matrix31 operator *(Matrix31 lhs, Matrix31 rhs) { return Multiply(lhs, rhs); }
        #endregion

        #region IEnumerable<float> Members

        public IEnumerator<double> GetEnumerator()
        {
            return ( (IEnumerable<double>)ToArray() ).GetEnumerator();
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
            return ToString("g3");
        }
        public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture.NumberFormat);
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringWriter sw = new StringWriter();
            sw.WriteLine();
            sw.Write(ArrayFormatting.ToFixedColumnString(ToArray2(), 11, format, formatProvider));
            return sw.ToString();
        }

        public string ToDerive()
        {
            return string.Format("[[{0},{1},{2},{3}],[{4},{5},{6},{7}],[{8},{9},{10},{11}],[{12},{13},{14},{15}]]",
                A11.A11, A11.A12, A11.A13, Vector1.X,
                A11.A21, A11.A22, A11.A23, Vector1.Y,
                A11.A31, A11.A32, A11.A33, Vector1.Z,
                Vector2.X, Vector2.Y, Vector2.Z, Scalar);
        }
        


        #endregion

    }
}
