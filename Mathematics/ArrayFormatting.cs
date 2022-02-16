using System;
using System.Globalization;
using System.Text;

namespace JA.Mathematics
{

    #region Delegates & Enums
    public delegate T Linear<out T>(int index);
    public delegate T Factory<out T>(int row, int col);
    #endregion

    public static class ArrayFormatting
    {
        public static readonly int DecimalPlaces = 3;

        #region Formatters
        public static string ToFixedColumnString<T>(T[] data, int column_width, params int[] decimal_places)
        {
            int[] decimals = BuildDecimalPlaces(data.Length, decimal_places);
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                res.Append(FormatItem(data[i], column_width, "g" + decimals[i], CultureInfo.CurrentCulture.NumberFormat));
            }
            return res.ToString();
        }
        public static string ToFixedColumnString<T>(T[] data, int column_width, string format, IFormatProvider provider)
        {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                res.Append(FormatItem(data[i], column_width, format, provider));
            }
            return res.ToString();
        }

        public static string ToFixedColumnString<T>(T[][] data, int column_width, params int[] decimal_places)
        {
            int rows = data.Length;
            int columns = 0;
            if (rows > 0)
            {
                columns = data[0].Length;
            }
            int[] decimals = BuildDecimalPlaces(columns, decimal_places);
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    res.Append(FormatItem(data[i][j], column_width, "g" + decimals[j], CultureInfo.CurrentCulture.NumberFormat));
                }
                res.AppendLine();
            }
            return res.ToString();
        }

        public static string ToFixedColumnString<T>(this T[][] data, int column_width, string format, IFormatProvider provider)
        {
            int rows = data.Length;
            int columns = 0;
            if (rows > 0)
            {
                columns = data[0].Length;
            }
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    res.Append(FormatItem(data[i][j], column_width, format, provider));
                }
                res.AppendLine();
            }
            return res.ToString();
        }

        public static string Visualizer<T>(T[][] data, string[] heads, int min_col_width, string[] formats, int[] cols)
        {
            string[] m_heads = heads;
            string[] m_formats = formats;
            int[] m_col_width = new int[m_heads.Length];
            for (int i = 0; i < m_heads.Length; i++)
            {
                if (i < cols.Length)
                {
                    m_col_width[i] = Math.Max(min_col_width, cols[i]);
                }
                else
                {
                    m_col_width[i] = Math.Max(min_col_width, m_heads[i].Length + 1);
                }
            }
            StringBuilder hdf = new StringBuilder();
            for (int i = 0; i < m_heads.Length; i++)
            {
                hdf.AppendFormat("{{{0},{1}}}", i, m_col_width[i]);
            }
            StringBuilder hd = new StringBuilder();
            hd.AppendFormat(hdf.ToString().Trim(), m_heads);
            string m_header = hd.ToString();

            int N = data.Length;
            int M = data[0].Length;
            StringBuilder res = new StringBuilder();

            string last_fmt = null;

            res.Append(m_header);
            res.AppendLine();
            for (int i = 0; i < N; i++)
            {
                object[] items = new object[M];
                StringBuilder line_fmt = new StringBuilder();
                for (int j = 0; j < M; j++)
                {
                    if (j < m_formats.Length)
                    {
                        last_fmt = m_formats[j];
                    }
                    if (last_fmt != null && last_fmt.Length > 0)
                    {
                        line_fmt.AppendFormat("{{{0},{1}:" + last_fmt + "}}", j, m_col_width[j]);
                    }
                    else
                    {
                        line_fmt.AppendFormat("{{{0},{1}:F3}}", j, m_col_width[j]);
                    }
                    items[j] = data[i][j];
                }
                res.AppendFormat(line_fmt.ToString().Trim(), items);
                res.AppendLine();
            }
            return res.ToString();
        }
        public static string ToDerive<T>(T[] data)
        {
            string[] fmt_items = new string[data.Length];
            for (int i = 0; i < fmt_items.Length; i++)
            {
                fmt_items[i] = string.Format("{{{0}}}", i);
            }
            string fmt = string.Format("[{0}]", string.Join(",", fmt_items));
            return string.Format(fmt, data);
        }

        public static string ToDerive<T>(T[][] data)
        {
            string[] rows = Array.ConvertAll<T[], string>(data, delegate (T[] row)
            {
                return ToDerive(row);
            });
            return ToDerive(rows);
        }

        #region Private Helper Formatting Functions

        public static string FormatItem<T>(T item, string format, IFormatProvider provider)
        {
            if (item is IFormattable f)
            {
                return f.ToString(format, provider);
            }
            else
            {
                return item.ToString();
            }
        }

        public static string FormatItem<T>(T item, int column_width, string format, IFormatProvider provider)
        {
            string result;
            if (item is IFormattable f)
            {
                format = format ??"G";
                result = string.Format(provider, "{0:" + format + "}", f);
                char[] fmt_chars = { 'E', 'e', 'F', 'f', 'G', 'g', 'N', 'n', 'P', 'p' };
                char[] exp_chars = { 'E', 'e' };
                int res_len = 0;
                while (result.Length > column_width-1)
                {
                    res_len = result.Length;
                    int pivot_index = format.IndexOf('.');
                    if (pivot_index >= 0)
                    {
                        if (pivot_index < format.Length - 1)
                        {
                            format = format.Substring(0, format.Length - 1);
                            result = string.Format(provider, "{0:" + format + "}", f);
                        }
                    }
                    if ((pivot_index = format.IndexOfAny(fmt_chars)) >= 0 && result.Length==res_len)
                    {
                        if (int.TryParse(format.Substring(pivot_index + 1), out int digits))
                        {
                            string fmt = format.Substring(0, pivot_index+1);
                            if (digits > 0)
                            {
                                format = fmt + (digits - 1).ToString();
                                result = string.Format(provider, "{0:" + format + "}", f);
                            }
                        }
                    }
                    if ((pivot_index = result.IndexOfAny(exp_chars)) >= 0 && result.Length == res_len)
                    {
                        string mantisa = result.Substring(0, pivot_index - 1);
                        string exponent = result.Substring(pivot_index + 1);
                        result = mantisa.Substring(0, mantisa.Length - 1) + result.Substring(pivot_index, 1) + exponent;
                    }
                    if (result.Length==res_len) // nothing changed
                    {
                        result = result.Substring(0, column_width - 2) + "…";
                    }
                }
            }
            else
            {
                result = item.ToString();
                if (result.Length > column_width - 1)
                {
                    result = result.Substring(0, column_width - 2) + "…";
                }
            }
            return string.Format("{0," + column_width + "}", result);
        }

        private static int[] BuildDecimalPlaces(int columns, params int[] decimal_places)
        {
            int[] decimals = new int[columns];
            if (decimal_places.Length > 0)
            {
                for (int i = 0; i < Math.Min(columns, decimal_places.Length); i++)
                {
                    decimals[i] = decimal_places[i];
                }
                for (int i = decimal_places.Length; i < columns; i++)
                {
                    decimals[i] = decimal_places[decimal_places.Length - 1];
                }
            }
            else
            {
                for (int i = 0; i < columns; i++)
                {
                    decimals[i] = DecimalPlaces;
                }
            }
            return decimals;
        }
        #endregion

        #endregion
    }

}
