using System;
using System.Text;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    public class PostrgreSqlByteArrayConverter : ByteArrayConverter
    {
        public override string ColumnDefinition
        {
            get { return "BYTEA"; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            return "E'" + this.ToBinary(value) + "'";
        }
    }

    //public class PostgreSqlStringArrayConverter : PostgreSqlStringConverter
    //{
    //    public override string ToQuotedString(Type fieldType, object value)
    //    {
    //        var stringArray = (string[])value;
    //        return this.ToArray(stringArray);
    //    }
    //}

    public class PostgreSqlIntArrayConverter : PostgreSqlStringConverter
    {
        public override string ToQuotedString(Type fieldType, object value)
        {
            var integerArray = (int[])value;
            return this.ToArray(integerArray);
        }
    }

    public class PostgreSqlLongArrayConverter : PostgreSqlStringConverter
    {
        public override string ToQuotedString(Type fieldType, object value)
        {
            var longArray = (long[])value;
            return this.ToArray(longArray);
        }
    }


    public static class PostgreSqlConverterExtensions
    {
        /// <summary>
        /// based on Npgsql2's source: Npgsql2\src\NpgsqlTypes\NpgsqlTypeConverters.cs
        /// </summary>
        public static string ToBinary(this IOrmLiteConverter converter, object NativeData)
        {
            var byteArray = (byte[])NativeData;
            var res = new StringBuilder(byteArray.Length * 5);
            foreach (byte b in byteArray)
                if (b >= 0x20 && b < 0x7F && b != 0x27 && b != 0x5C)
                    res.Append((char)b);
                else
                    res.Append("\\\\")
                        .Append((char)('0' + (7 & (b >> 6))))
                        .Append((char)('0' + (7 & (b >> 3))))
                        .Append((char)('0' + (7 & b)));
            return res.ToString();
        }

        public static string ToArray<T>(this IOrmLiteConverter converter, T[] source)
        {
            var values = new StringBuilder();
            foreach (var value in source)
            {
                if (values.Length > 0) values.Append(",");
                values.Append(converter.DialectProvider.GetQuotedValue(value, typeof(T)));
            }
            return "ARRAY[" + values + "]";
        }
    }
}