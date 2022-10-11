using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JA.Mathematics
{
    using static Math;

    public static class LinearAlgebra
    {
        public const double ZeroTolerance = 1d / 17592186044416;
        public const double TrigTolerance = 1d / 134217728;

        public static Random RandomNumberGenerator { get; } = new Random();

        #region Factory
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Vector<T>(int size) => new T[size];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Vector<T>(int size, T value)
        {
            var c = new T[size];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = value;
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Vector<T>(int size, Func<int, T> initializer)
        {
            var c = new T[size];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = initializer(i);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Vector<T>(int size, T seed, Func<T, T> sequence)
        {
            var c = new T[size];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = seed;
                seed = sequence(seed);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Matrix<T>(int rows, int columns)
        {
            return Vector(rows, (i) => new T[columns]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Matrix<T>(int rows, int columns, T value)
        {
            return Vector(rows, (i) => Vector(columns, value));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Matrix<T>(int rows, int columns, Func<int, int, T> initializer)
        {
            return Vector(rows, (i) => Vector(columns, (j) => initializer(i, j)));
        }
        public static T[][] ToJagged<T>(this T[,] matrix)
        {
            int n = matrix.GetLength(0), m = matrix.GetLength(1);
            var c = new T[n][];
            int sz = Buffer.ByteLength(matrix) / n;
            for (int i = 0; i < n; i++)
            {
                c[i] = new T[m];
                Buffer.BlockCopy(matrix, i * sz, c[i], 0, sz);
            }
            return c;
        }
        #endregion

        #region Generic Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T a, ref T b)
        {
            (b, a) = (a, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[] a, T[] b) 
        {
            a.AsSpan().CopyTo(b.AsSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Copy<T>(this T[] a)
        {
            var c = new T[a.Length];
            CopyTo(a, c);
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[,] Copy<T>(this T[,] a)
        {
            return (T[,])a.Clone();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this T[][] a, T[][] b) 
        {
            int na = a.Length, nb = b.Length;
            if (na != nb)
            {
                throw new ArgumentException("Row Count Mismatch.", nameof(b));
            }
            for (int i = 0; i < na; i++)
            {
                CopyTo(a[i], b[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Copy<T>(this T[][] a)
        {
            int n = a.Length;
            var c = new T[n][];
            for (int i = 0; i < n; i++)
            {
                c[i] = Copy(a[i]);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Slice<T>(this T[] a, int offset, int count=-1)
        {
            if (count < 0)
            {
                count = a.Length - offset;
            }
            return a.AsSpan(offset, count).ToArray();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Inject<T>(this T[] a, int offset, T[] values)
        {
            values.CopyTo(a.AsSpan(offset, values.Length));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Slice<T>(this T[][] a, int row, int column, int rowCount=-1, int colCount=-1)
        {
            if (a.Length == 0)
            {
                return new T[0][];
            }
            if (rowCount < 0)
            {
                rowCount = a.Length - row;
            }
            if (colCount < 0)
            {
                colCount = a[0].Length - column;
            }
            var c = new T[rowCount][];
            for (int i = 0; i < rowCount; i++)
            {
                c[i] = Slice(a[i + row], column, colCount);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Inject<T>(this T[][] a, int row, int column, T[][] values)
        {
            if (values.Length == 0)
            {
                return;
            }
            for (int i = 0; i < values.Length; i++)
            {
                Inject(a[i+row], column, values[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[,] Transpose<T>(this T[,] a)
        {
            int n = a.GetLength(0);
            int m = a.GetLength(1);
            T[,] c = new T[m, n];
            if (n == m)
            {
                c = Copy(a);
                for (int i = 1; i < n; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        Swap(ref c[i, j], ref c[j, i]);
                    }
                }
            }
            else
            {
                for (int j = 0; j < m; j++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        c[j, i] = a[i, j];
                    }
                }
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Transpose<T>(this T[][] a)
        {
            int n = a.Length;
            int m = n > 0 ? a[0].Length : 0;
            if (n == m)
            {
                var c = Copy(a);
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        Swap(ref c[i][j], ref c[j][i]);
                    }
                }
                return c;
            }
            else
            {
                var c = new T[m][];
                for (int j = 0; j < m; j++)
                {
                    var col = new T[n];
                    for (int i = 0; i < n; i++)
                    {
                        col[i] = a[i][j];
                    }
                    c[j] = col;
                }
                return c;
            }
        }

        #endregion

        #region Vector Algebra

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Apply(double[] a, Func<double, double> f)
        {
            int n = a.Length;
            var c = new double[n];
            for (int i = 0; i < n; i++)
            {
                c[i] = f(a[i]);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Apply(double x, double[] b, Func<double, double, double> f)
        {
            int nb = b.Length;
            var c = new double[nb];
            for (int i = 0; i < nb; i++)
            {
                c[i] = f(x, b[i]);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Apply(double[] a, double x, Func<double, double, double> f)
        {
            int nb = a.Length;
            var c = new double[nb];
            for (int i = 0; i < nb; i++)
            {
                c[i] = f(a[i], x);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Apply(double[] a, double[] b, Func<double, double, double> f)
        {
            int na = a.Length, nb = b.Length;
            if (na != nb)
            {
                throw new ArgumentException("Incompatible Vector Sizes.", nameof(b));
            }
            var c = new double[na];
            for (int i = 0; i < na; i++)
            {
                c[i] = f(a[i], b[i]);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Abs(double[] a) => Apply(a, Math.Abs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Add(double[] a, double x) => Add(x, a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Add(double x, double[] b)
        {
            int nb = b.Length;
            var c = new double[nb];
            for (int i = 0; i < nb; i++)
            {
                c[i] = x + b[i];
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Add(double[] a, double[] b)
        {
            int na = a.Length, nb = b.Length;
            if (na != nb)
            {
                throw new ArgumentException("Incompatible Vector Sizes.", nameof(b));
            }
            var c = new double[na];
            for (int i = 0; i < na; i++)
            {
                c[i] = a[i] + b[i];
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Subtract(double x, double[] b)
        {
            int nb = b.Length;
            var c = new double[nb];
            for (int i = 0; i < nb; i++)
            {
                c[i] = x - b[i];
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Subtract(double[] a, double x) => Add(a, -x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Subtract(double[] a, double[] b)
        {
            int na = a.Length, nb = b.Length;
            if (na != nb)
            {
                throw new ArgumentException("Incompatible Vector Sizes.", nameof(b));
            }
            var c = new double[na];
            for (int i = 0; i < na; i++)
            {
                c[i] = a[i] - b[i];
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Multiply(double[] a, double x) => Multiply(x, a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Multiply(double x, double[] b)
        {
            int n = b.Length;
            var c = new double[n];
            for (int i = 0; i < n; i++)
            {
                c[i] = x * b[i];
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Multiply(double[] a, double[] b)
        {
            int na = a.Length, nb = b.Length;
            if (na != nb)
            {
                throw new ArgumentException("Incompatible Vector Sizes.", nameof(b));
            }
            var c = new double[na];
            for (int i = 0; i < na; i++)
            {
                c[i] = a[i] * b[i];
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Divide(double[] a, double x) => Multiply(a, 1 / x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Divide(double x, double[] b)
        {
            int n = b.Length;
            var c = new double[n];
            for (int i = 0; i < n; i++)
            {
                c[i] = x / b[i];
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Divide(double[] a, double[] b)
        {
            int na = a.Length, nb = b.Length;
            if (na != nb)
            {
                throw new ArgumentException("Incompatible Vector Sizes.", nameof(b));
            }
            var c = new double[na];
            for (int i = 0; i < na; i++)
            {
                c[i] = a[i] / b[i];
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sum(double[] a)
        {
            int n = a.Length;
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum += a[i];
            }
            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Max(double[] a)
        {
            int n = a.Length;
            if (n == 0)
            {
                throw new ArgumentException("No elements.", nameof(a));
            }
            double res = a[0];
            for (int i = 1; i < n; i++)
            {
                res = Math.Max(res, a[i]);
            }
            return res;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Min(double[] a)
        {
            int n = a.Length;
            if (n == 0)
            {
                throw new ArgumentException("No elements.", nameof(a));
            }
            double res = a[0];
            for (int i = 1; i < n; i++)
            {
                res = Math.Min(res, a[i]);
            }
            return res;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double MaxAbs(double[] a)
        {
            int n = a.Length;
            if (n == 0)
            {
                throw new ArgumentException("No elements.", nameof(a));
            }
            double res = Math.Abs(a[0]);
            for (int i = 1; i < n; i++)
            {
                res = Math.Max(res, Math.Abs(a[i]));
            }
            return res;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double MinAbs(double[] a)
        {
            int n = a.Length;
            if (n == 0)
            {
                throw new ArgumentException("No elements.", nameof(a));
            }
            double res = Math.Abs(a[0]);
            for (int i = 1; i < n; i++)
            {
                res = Math.Min(res, Math.Abs(a[i]));
            }
            return res;
        }
        #endregion

        #region Matrix Algebra

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Apply(double[][] a, Func<double, double> f)
        {
            int n = a.Length;
            var c = new double[n][];
            for (int i = 0; i < n; i++)
            {
                c[i] = Apply(a[i], f);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Apply(double[][] a, double x, Func<double, double, double> f)
        {
            int n = a.Length;
            var c = new double[n][];
            for (int i = 0; i < n; i++)
            {
                c[i] = Apply(a[i], x, f);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Apply(double x, double[][] b, Func<double, double, double> f)
        {
            int n = b.Length;
            var c = new double[n][];
            for (int i = 0; i < n; i++)
            {
                c[i] = Apply(x, b[i], f);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Apply(double[][] a, double[][] b, Func<double, double, double> f)
        {
            int na = a.Length, nb = b.Length;
            if (na != nb)
            {
                throw new ArgumentException("Incompatible Matrix Sizes.", nameof(b));
            }
            var c = new double[na][];
            for (int i = 0; i < nb; i++)
            {
                c[i] = Apply(a[i], b[i], f);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Add(double[][] a, double x) => Add(x, a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Add(double x, double[][] b)
        {
            int nb = b.Length;
            var c = new double[nb][];
            for (int i = 0; i < nb; i++)
            {
                c[i] = Add(x, b[i]);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Add(double[][] a, double[][] b)
        {
            int na = a.Length, nb = b.Length;
            if (na != nb)
            {
                throw new ArgumentException("Incompatible Matrix Sizes.", nameof(b));
            }
            var c = new double[na][];
            for (int i = 0; i < na; i++)
            {
                c[i] = Add(a[i], b[i]);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Subtract(double[][] a, double x) => Add(a, -x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Subtract(double x, double[][] b)
        {
            int nb = b.Length;
            var c = new double[nb][];
            for (int i = 0; i < nb; i++)
            {
                c[i] = Subtract(x, b[i]);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Subtract(double[][] a, double[][] b)
        {
            int na = a.Length, nb = b.Length;
            if (na != nb)
            {
                throw new ArgumentException("Incompatible Matrix Sizes.", nameof(b));
            }
            var c = new double[na][];
            for (int i = 0; i < na; i++)
            {
                c[i] = Subtract(a[i], b[i]);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Multiply(double[][] a, double x) => Multiply(x, a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Multiply(double x, double[][] b)
        {
            int n = b.Length;
            var c = new double[n][];
            for (int i = 0; i < n; i++)
            {
                c[i] = Multiply(x, b[i]);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Multiply(double[][] a, double[][] b)
        {
            int na = a.Length, nb = b.Length;
            if (na != nb)
            {
                throw new ArgumentException("Incompatible Matrix Sizes.", nameof(b));
            }
            var c = new double[na][];
            for (int i = 0; i < na; i++)
            {
                c[i] = Multiply(a[i], b[i]);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Divide(double x, double[][] b)
        {
            int n = b.Length;
            var c = new double[n][];
            for (int i = 0; i < n; i++)
            {
                c[i] = Divide(x, b[i]);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Divide(double[][] a, double x) => Multiply(a, 1 / x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Divide(double[][] a, double[][] b)
        {
            int na = a.Length, nb = b.Length;
            if (na != nb)
            {
                throw new ArgumentException("Incompatible Matrix Sizes.", nameof(b));
            }
            var c = new double[na][];
            for (int i = 0; i < na; i++)
            {
                c[i] = Divide(a[i], b[i]);
            }
            return c;
        }
        #endregion

        #region Products
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double InnerProduct(double[] a, double[] b)
            => Sum(Multiply(a, b));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] OuterProduct(double[] a, double[] b)
        {
            int na = a.Length;
            var c = new double[na][];
            for (int i = 0; i < na; i++)
            {
                c[i] = Multiply(a[i], b);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Product(double[][] a, double[] b)
        {
            int n = a.Length;
            var c = new double[n];
            for (int i = 0; i < n; i++)
            {
                c[i] = InnerProduct(a[i], b);
            }
            return c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] Product(double[] a, double[][] b)
        {
            return Product(Transpose(b), a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[][] Product(double[][] a, double[][] b)
        {
            int n = a.Length;
            var c = new double[n][];
            b = Transpose(b);
            for (int i = 0; i < n; i++)
            {
                c[i] = Product(a, b[i]);
            }
            return c;
        }
        #endregion

        #region Matrix Decomposition
        internal static bool LuDecompose(double[][] matrix,
            out double[][] lu, out int[] perm, out int toggle)
        {
            Contract.Requires(matrix != null);
            Contract.Requires(matrix.Length > 0);
            Contract.Ensures(Contract.ValueAtReturn(out lu).Length == matrix.Length);
            Contract.Ensures(Contract.ForAll(lu, (row) => row.Length == matrix[0].Length));
            Contract.Ensures(Contract.ValueAtReturn(out perm).Length == matrix.Length);

            // Doolittle LUP decomposition.
            // assumes matrix is square.
            int n = matrix.Length; // convenience
            lu = Copy(matrix);
            perm = Enumerable.Range(0, n).ToArray();
            toggle = 1;
            for (int j = 0; j < n - 1; ++j) // each column
            {
                double colMax = Math.Abs(lu[j][j]); // largest val col j
                int pRow = j;
                for (int i = j + 1; i < n; ++i)
                {
                    var row_i = lu[i];
                    if (row_i[j] > colMax)
                    {
                        colMax = row_i[j];
                        pRow = i;
                    }
                }
                if (pRow != j) // swap rows
                {
                    Swap(ref lu[pRow], ref lu[j]);
                    Swap(ref perm[pRow], ref perm[j]);
                    toggle = -toggle; // row-swap toggle
                }
                var row_j = lu[j];
                var lu_jj = row_j[j];
                if (Math.Abs(lu[j][j]) <= ZeroTolerance)
                {
                    return false; // consider a throw
                }
                for (int i = j + 1; i < n; ++i)
                {
                    var row_i = lu[i];
                    var lu_ij = row_i[j];
                    lu_ij /= lu_jj;
                    row_i[j] = lu_ij;
                    for (int k = j + 1; k < n; ++k)
                    {
                        row_i[k] -= lu_ij * row_j[k];
                    }
                    lu[i] = row_i;
                }
            } // maj column loop
            return true;
        }
        static bool HelperSolve(double[][] lu, double[] b, double[] x)
        {
            // solve luMatrix * x = b
            Contract.Requires(lu != null);
            Contract.Requires(b != null);
            Contract.Requires(lu.Length > 0);
            Contract.Requires(lu.Length == b.Length);
            Contract.Ensures(Contract.ValueAtReturn<double[]>(out _).Length == lu[0].Length);
            int n = lu.Length;
            CopyTo(b, x);
            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                var row = lu[i];
                for (int j = 0; j < i; ++j)
                {
                    sum -= row[j] * x[j];
                }
                x[i] = sum;
            }
            var d = lu[n - 1][n - 1];
            if (Math.Abs(d) == 0)
            {
                return false;
            }

            x[n - 1] /= d;
            for (int i = n - 2; i >= 0; --i)
            {
                var row = lu[i];
                d = row[i];
                if (Math.Abs(d) == 0)
                {
                    return false;
                }

                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                {
                    sum -= row[j] * x[j];
                }
                x[i] = sum / d;
            }
            return true;
        }
        static bool HelperSolve(double[][] luMatrix, double[][] B, double[][] X)
        {
            // solve luMatrix * X = B
            Contract.Requires(luMatrix != null);
            Contract.Requires(B != null);
            Contract.Requires(B.Length > 0);
            Contract.Requires(luMatrix.Length == B.Length);
            Contract.Ensures(Contract.ValueAtReturn<double[][]>(out _).Length == luMatrix[0].Length);
            int n = luMatrix.Length;
            int m = B[0].Length;
            CopyTo(B, X);
            for (int k = 0; k < m; k++)
            {
                for (int i = 1; i < n; ++i)
                {
                    double sum = X[i][k];
                    for (int j = 0; j < i; ++j)
                    {
                        sum -= luMatrix[i][j] * X[j][k];
                    }
                    X[i][k] = sum;
                }
                var d = luMatrix[n - 1][n - 1];
                if (Math.Abs(d) == 0)
                {
                    return false;
                }

                X[n - 1][k] /= d;
                for (int i = n - 2; i >= 0; --i)
                {
                    d = luMatrix[i][i];
                    if (Math.Abs(d) == 0)
                    {
                        return false;
                    }

                    double sum = X[i][k];
                    for (int j = i + 1; j < n; ++j)
                    {
                        sum -= luMatrix[i][j] * X[j][k];
                    }
                    X[i][k] = sum / d;
                }
            }
            return true;
        }
        public static double[][] MatrixInverse(double[][] matrix)
        {
            Contract.Requires(matrix != null);
            Contract.Requires(matrix.Length > 0);
            Contract.Ensures(Contract.Result<double[][]>().Length == matrix.Length);
            Contract.Ensures(Contract.Result<double[][]>()[0].Length == matrix[0].Length);
            if (MatrixInverse(matrix, out double[][] inverse))
            {
                return inverse;
            }
            throw new ArgumentException("Singular Matrix", nameof(matrix));
        }
        public static bool MatrixInverse(double[][] matrix, out double[][] inverse)
        {
            Contract.Requires(matrix != null);
            Contract.Requires(matrix.Length > 0);
            Contract.Requires(matrix[0].Length == matrix.Length);
            if (!LuDecompose(matrix, out double[][] lu, out int[] perm, out _))
            {
                inverse = Copy(matrix);
                return false;
            }
            return LuInverse(lu, perm, out inverse);
        }
        internal static bool LuInverse(double[][] lu, int[] perm, out double[][] inverse)
        {
            int n = lu.Length;
            inverse = new double[n][];
            for (int j = 0; j < n; j++)
            {
                inverse[j] = new double[n];
            }
            double[] b = new double[n];
            double[] x = new double[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (i == perm[j])
                    {
                        b[j] = 1;
                    }
                    else
                    {
                        b[j] = 0;
                    }
                }
                if (HelperSolve(lu, b, x))
                {
                    for (int j = 0; j < n; ++j)
                    {
                        inverse[j][i] = x[j];
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        public static double MatrixDeterminant(double[][] matrix)
        {
            Contract.Requires(matrix != null);
            Contract.Requires(matrix.Length > 0);
            Contract.Requires(matrix[0].Length == matrix.Length);
            if (!LuDecompose(matrix, out double[][] lum, out _, out int toggle))
            {
                throw new ArgumentException("Unable to compute MatrixDeterminant", nameof(matrix));
            }
            return LuDeterminant(lum, toggle);
        }
        internal static double LuDeterminant(double[][] lu, int toggle)
        {
            double result = toggle;
            for (int i = 0; i < lu.Length; ++i)
            {
                result *= lu[i][i];
            }
            return result;
        }
        public static double[] SystemSolve(double[][] matrix, double[] b)
        {
            Contract.Requires(matrix != null);
            Contract.Requires(b != null);
            Contract.Requires(matrix.Length > 0);
            Contract.Requires(matrix.Length == b.Length);
            Contract.Ensures(Contract.Result<double[]>().Length == matrix[0].Length);
            double[] x = new double[matrix.Length];
            if (SystemSolve(matrix, b, x))
            {
                return x;
            }
            throw new ArgumentException("Singular Matrix", nameof(matrix));
        }
        public static bool SystemSolve(double[][] matrix, double[] b, double[] x)
        {
            Contract.Requires(matrix != null);
            Contract.Requires(b != null);
            Contract.Requires(matrix.Length > 0);
            Contract.Requires(matrix[0].Length == matrix.Length);
            Contract.Requires(b.Length == matrix.Length);
            Contract.Ensures(Contract.ValueAtReturn<double[]>(out _).Length == matrix[0].Length);
            // Solve Ax = b
            //int n = matrix.Length;
            if (!LuDecompose(matrix, out double[][] lum, out int[] perm, out _))
            {
                return false;
            }
            return LuSystemSolve(lum, perm, b, x);
        }
        internal static bool LuSystemSolve(double[][] lu, int[] perm, double[] b, double[] x)
        {
            double[] bp = new double[b.Length];
            for (int i = 0; i < lu.Length; ++i)
            {
                bp[i] = b[perm[i]];
            }
            if (!HelperSolve(lu, bp, x))
            {
                return false;
            }
            return true;
        }
        public static double[][] SystemSolve(double[][] matrix, double[][] B)
        {
            double[][] X = Matrix<double>(matrix.Length, B[0].Length);
            if (SystemSolve(matrix, B, X))
            {
                return X;
            }
            throw new ArgumentException("Singular Matrix", nameof(matrix));
        }
        public static bool SystemSolve(double[][] matrix, double[][] B, double[][] X)
        {
            Contract.Requires(matrix != null);
            Contract.Requires(matrix.Length > 0);
            Contract.Requires(matrix[0].Length == matrix.Length);
            Contract.Requires(B != null);
            Contract.Requires(B.Length == matrix.Length);
            if (!LuDecompose(matrix, out double[][] lu, out int[] perm, out _))
            {
                return false;
            }
            return LuSystemSolve(lu, perm, B, X);
        }
        internal static bool LuSystemSolve(double[][] lu, int[] perm, double[][] B, double[][] X)
        {
            double[][] Bp = new double[B.Length][];
            for (int i = 0; i < lu.Length; ++i)
            {
                Bp[i] = B[perm[i]];
            }
            if (!HelperSolve(lu, Bp, X))
            {
                return false;
            }
            return true;
        }
        internal static double[][] PermArrayToMatrix(int[] perm)
        {
            Contract.Requires(perm != null);
            Contract.Assume(perm.Length > 0);
            // Doolittle perm array to corresponding matrix
            int n = perm.Length;
            double[][] result = new double[n][];
            for (int i = 0; i < n; ++i)
            {
                result[i] = new double[n];
                result[i][perm[i]] = 1;
            }
            return result;
        }
        internal static double[][] UnPermute(double[][] lu, int[] perm)
        {
            Contract.Requires(lu != null);
            Contract.Requires(perm != null);
            Contract.Requires(lu.Length == perm.Length);

            double[][] result = Copy(lu);
            int[] unperm = new int[perm.Length];
            for (int i = 0; i < perm.Length; ++i)
            {
                unperm[perm[i]] = i; // create un-perm array
            }
            for (int r = 0; r < lu.Length; ++r) // each row
            {
                result[r] = lu[unperm[r]];
            }
            return result;
        }

        public static bool SystemSolve(double[][] matrix, double[] b, double[] x, double[] y, int knownXcount = 0)
        {
            // Solves y = A*x + b
            // but with some known rows of x
            if (knownXcount == 0)
            {
                // Solve A*x = y-b
                return SystemSolve(matrix, Subtract(y, b), x);
            }
            else if (knownXcount == x.Length)
            {
                double[] yt = Add(Product(matrix, x), b);
                CopyTo(yt, y);
                return true;
            }
            else
            {
                int n = matrix.Length;
                double[] x_1 = Slice(x, 0, knownXcount);
                double[] y_2 = Slice(y, knownXcount);
                double[] b_1 = Slice(b, 0, knownXcount);
                double[] b_2 = Slice(b, knownXcount);
                double[][] A_11 = Slice(matrix, 0, 0, knownXcount, knownXcount);
                double[][] A_12 = Slice(matrix, 0, knownXcount, knownXcount, -1);
                double[][] A_21 = Slice(matrix, knownXcount, 0, -1, knownXcount);
                double[][] A_22 = Slice(matrix, knownXcount, knownXcount,-1,-1);
                double[] rhs = Subtract(y_2, Add(Product(A_21, x_1), b_2));
                double[] x_2 = new double[n - knownXcount];
                if (SystemSolve(A_22, rhs, x_2))
                {
                    double[] y_1 = Add(b_1, Add(Product(A_11, x_1), Product(A_12, x_2)));
                    Inject(x, knownXcount, x_2);
                    Inject(y, 0, y_1);
                    return true;
                }
                return false;
            }
        }

        #endregion

    }
}
