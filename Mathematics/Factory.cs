using System;
using System.Collections.Generic;

namespace JA.Mathematics
{
    public static class Factory
    {
        public const double DEG = Math.PI / 180;
        public const double TrigonometricPrecision = 1.0 / 134217728;

        #region Vectors
        public static T[] MakeVector<T>(int size, Linear<T> initializer)
        {
            T[] res = new T[size];
            for( int i = 0; i < res.Length; i++ )
            {
                res[i] = initializer(i);
            }
            return res;
        }

        public static T[] MakeVector<T>(IEnumerable<T> list)
        {
            if (list is T[])
            {
                return (T[])list;
            }
            if (list is List<T>)
            {
                return (list as List<T>).ToArray();
            }
            List<T> res = new List<T>(list);
            return res.ToArray();
        }

        public static T[] MakeVector<T>(IEnumerator<T> list)
        {
            List<T> res = new List<T>();
            while( list.MoveNext() )
            {
                res.Add(list.Current);
            }
            return res.ToArray();
        }

        public static T[] FillVector<T>(int size, T value)
        {
            T[] res = new T[size];
            for( int i = 0; i < res.Length; i++ )
            {
                res[i] = value;
            }
            return res;
        }

        public static float[] Add(float[] lhs, float[] rhs)
        {
            if (lhs == null) return rhs;
            if (rhs == null) return lhs;
            int N = Math.Min(lhs.Length, rhs.Length);
            float[] res = new float[N];
            for (int i = 0; i < N; i++)
            {
                res[i] = lhs[i] + rhs[i];
            }
            for (int i = N; i < lhs.Length; i++)
            {
                res[i] = lhs[i];
            }
            for (int i = N; i < rhs.Length; i++)
            {
                res[i] = rhs[i];
            }
            return res;
        }

        public static float[] Subtract(float[] lhs, float[] rhs)
        {
            if (lhs == null) return rhs;
            if (rhs == null) return lhs;
            int N = Math.Min(lhs.Length, rhs.Length);
            float[] res = new float[N];
            for (int i = 0; i < N; i++)
            {
                res[i] = lhs[i] - rhs[i];
            }
            for (int i = N; i < lhs.Length; i++)
            {
                res[i] = lhs[i];
            }
            for (int i = N; i < rhs.Length; i++)
            {
                res[i] = -rhs[i];
            }
            return res;
        }
        public static double[] Add(double[] lhs, double[] rhs)
        {
            if (lhs == null) return rhs;
            if (rhs == null) return lhs;
            int N = Math.Min(lhs.Length, rhs.Length);
            double[] res = new double[N];
            for (int i = 0; i < N; i++)
            {
                res[i] = lhs[i] + rhs[i];
            }
            for (int i = N; i < lhs.Length; i++)
            {
                res[i] = lhs[i];
            }
            for (int i = N; i < rhs.Length; i++)
            {
                res[i] = rhs[i];
            }
            return res;
        }

        public static double[] Subtract(double[] lhs, double[] rhs)
        {
            if (lhs == null) return rhs;
            if (rhs == null) return lhs;
            int N = Math.Min(lhs.Length, rhs.Length);
            double[] res = new double[N];
            for (int i = 0; i < N; i++)
            {
                res[i] = lhs[i] - rhs[i];
            }
            for (int i = N; i < lhs.Length; i++)
            {
                res[i] = lhs[i];
            }
            for (int i = N; i < rhs.Length; i++)
            {
                res[i] = -rhs[i];
            }
            return res;
        }

        public static T[] Section<T>(T[] input, int start, int length)
        {
            T[] res = new T[length];
            System.Array.Copy(input, start, res, 0, length);
            return res;
        }

        public static void Inject<T>(T[] values, int start, T[] result)
        {
            System.Array.Copy(values, 0, result, start, values.Length);
        }

        #endregion

        #region Vector products
        public static float InnerProduct(float[] x, float[] y)
        {
            if (x.Length == y.Length)
            {
                return InnerProduct(x, y, 0, 1);
            }
            else
                throw new System.ArgumentException(nameof(y));
        }

        public static float InnerProduct(float[] lhs, float[] rhs, int index, int step)
        {
            int N = lhs.Length;
            float res = 0f;
            for (int i = index; i < N; i += step)
            {
                float tmp = lhs[i] * rhs[i];
                res += tmp;
            }
            return res;
        }

        public static float[] OuterProduct(float[] lhs, float[] rhs, int index, int step)
        {
            int N = lhs.Length;
            int M = rhs.Length;
            float[] res = new float[N * M];
            for (int i = index; i < N; i += step)
            {
                int k = i * M;
                float l = lhs[i];
                for (int j = index; j < M; j += step)
                {
                    res[j + k] = l * rhs[j];
                }
            }
            return res;
        }

        public static double InnerProduct(double[] x, double[] y)
        {
            if (x.Length == y.Length)
            {
                return InnerProduct(x, y, 0, 1);
            }
            else
                throw new System.ArgumentException(nameof(y));
        }

        public static double InnerProduct(double[] lhs, double[] rhs, int index, int step)
        {
            int N = lhs.Length;
            double res = 0d;
            for (int i = index; i < N; i += step)
            {
                double tmp = lhs[i] * rhs[i];
                res += tmp;
            }
            return res;
        }

        public static double[] OuterProduct(double[] lhs, double[] rhs, int index, int step)
        {
            int N = lhs.Length;
            int M = rhs.Length;
            double[] res = new double[N * M];
            for (int i = index; i < N; i += step)
            {
                int k = i * M;
                double l = lhs[i];
                for (int j = index; j < M; j += step)
                {
                    res[j + k] = l * rhs[j];
                }
            }
            return res;
        }
        
        #endregion

        #region Vector Enumerators

        public static T[] Append<T>(IEnumerable<T> array, T new_term)
        {
            List<T> list = new List<T>(array)
            {
                new_term
            };
            return list.ToArray();
        }

        public static T[] Append<T>(params IEnumerable<T>[] args)
        {
            int M = args.Length;
            int N = 0;
            T[][] list = new T[M][];
            for (int i = 0; i < M; i++)
            {
                list[i] = MakeVector(args[i]);
                N += list[i].Length;
            }
            List<T> res = new List<T>(N);
            for (int i = 0; i < M; i++)
            {
                res.AddRange(list[i]);
            }
            return res.ToArray();
        }

        public static T[] Append<T>(params IEnumerator<T>[] args)
        {
            int M = args.Length;
            int N = 0;
            T[][] list = new T[M][];
            for( int i = 0; i < M; i++ )
            {
                list[i] = MakeVector(args[i]);
                N += list[i].Length;
            }
            List<T> res = new List<T>(N);
            for( int i = 0; i < M; i++ )
            {
                res.AddRange(list[i]);
            }
            return res.ToArray();
        }
        
        #endregion

        #region Matrices

        public static T[][] MakeMatrix<T>(int rows, int columns)
        {
            bool ref_types = !typeof(T).IsValueType;
            T[][] res = new T[rows][];
            for (int i = 0; i < rows; i++)
            {
                T[] row = new T[columns];
                for (int j = 0; j < columns; j++)
                {
                    if (ref_types)
                    {
                        row[j] = Activator.CreateInstance<T>();
                    }
                }
                res[i] = row;
            }
            return res;
        }
        public static T[][] MakeMatrix<T>(int rows, int columns, params T[] values)
        {
            bool ref_types = !typeof(T).IsValueType;
            int k = 0;
            T[][] res = new T[rows][];
            for (int i = 0; i < rows; i++)
            {
                T[] row = new T[columns];
                for (int j = 0; j < columns; j++)
                {
                    if (k < values.Length)
                    {
                        row[j] = values[k];
                        k++;
                    }
                    else if (ref_types)
                    {
                        row[j] = Activator.CreateInstance<T>();
                    }
                }
                res[i] = row;
            }
            return res;
        }
        public static T[][] MakeMatrix<T>(int rows, int columns, Factory<T> initializer)
        {
            T[][] res = new T[rows][];
            for (int i = 0; i < rows; i++)
            {
                T[] row = new T[columns];
                for (int j = 0; j < columns; j++)
                {
                    row[j] = initializer(i, j);
                }
                res[i] = row;
            }
            return res;
        }


        /// <summary>Returns lower_diag diagonal matrix of the given values.</summary>
        public static T[][] MakeDiagonal<T>(int rows, int cols, params T[] values)
        {
            bool ref_types = !typeof(T).IsValueType;
            int k = 0;
            T[][] res = new T[rows][];
            for (int i = 0; i < rows; i++)
            {
                T[] row = new T[cols];
                for (int j = 0; j < cols; j++)
                {
                    if (i == j && k < values.Length)
                    {
                        row[j] = values[k++];
                    }
                    else if (i == j)
                    {
                        row[j] = values[values.Length - 1];
                    }
                    else if (ref_types)
                    {
                        row[j] = Activator.CreateInstance<T>();
                    }
                }
                res[i] = row;
            }

            return res;

        }

        public static T[][] MakeDiagonal<T>(int rows, int cols, Linear<T> initializer)
        {
            bool ref_types = !typeof(T).IsValueType;
            int k = 0;
            T[][] res = new T[rows][];
            for (int i = 0; i < rows; i++)
            {
                T[] row = new T[cols];
                row[i] = initializer(i);
                k++;
                if (ref_types)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (i != j)
                        {
                            row[j] = Activator.CreateInstance<T>();
                        }
                    }
                }
                res[i] = row;
            }
            return res;
        }

        public static T[][] Transpose<T>(T[][] data)
        {
            int N = data.Length;
            int M = data[0].Length;
            T[][] res = new T[M][];
            for (int i = 0; i < M; i++)
            {
                res[i] = new T[N];
                for (int j = 0; j < N; j++)
                {
                    res[i][j] = data[j][i];
                }
            }
            return res;
        }

        public static T[][] Section<T>(T[][] input, int row_start, int col_start, int row_count, int col_count)
        {
            T[][] res = new T[row_count][];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = Section(input[row_start + i], col_start, col_count);
            }
            return res;
        }

        public static void Inject<T>(T[][] values, int row_index, int col_index, T[][] result)
        {
            for (int i = 0; i < values.Length; i++)
            {
                Inject(values[i], col_index, result[row_index + i]);
            }
        }
        public static T[][] MakeBlock<T>(int rows, int columns, params T[][][] args)
        {
            if (rows * columns == args.Length)
            {
                int[] row_count = new int[rows];
                int[] col_count = new int[columns];

                int N = 0;
                int M = 0;

                for (int i = 0; i < rows; i++)
                {
                    row_count[i] = args[i * columns].Length;
                    N += row_count[i];
                }
                for (int j = 0; j < columns; j++)
                {
                    col_count[j] = args[0][j].Length;
                    M += col_count[j];
                }

                T[][] res = new T[N][];
                for (int i = 0; i < N; i++)
                {
                    res[i] = new T[M];
                }

                for (int i = 0, row_i = 0; i < rows; i++)
                {
                    for (int j = 0, col_j = 0; j < columns; j++)
                    {
                        Inject(args[j+i*columns], row_i, col_j, res);
                        col_j += col_count[j];
                    }
                    row_i += row_count[i];
                }
                return res;
            }
            else
                throw new ArgumentException(string.Format("Argumnets must be {0}*{1}={2} to create a block matrix", rows, columns, rows * columns));
        }

        #endregion

    }

}
