//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2010 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
 
    public abstract class OrmLiteDialectProviderBase
        : IOrmLiteDialectProvider
    {
        #region ADO.NET supported types
        /* ADO.NET UNDERSTOOD DATA TYPES:
			COUNTER	DbType.Int64
			AUTOINCREMENT	DbType.Int64
			IDENTITY	DbType.Int64
			LONG	DbType.Int64
			TINYINT	DbType.Byte
			INTEGER	DbType.Int64
			INT	DbType.Int32
			VARCHAR	DbType.String
			NVARCHAR	DbType.String
			CHAR	DbType.String
			NCHAR	DbType.String
			TEXT	DbType.String
			NTEXT	DbType.String
			STRING	DbType.String
			DOUBLE	DbType.Double
			FLOAT	DbType.Double
			REAL	DbType.Single
			BIT	DbType.Boolean
			YESNO	DbType.Boolean
			LOGICAL	DbType.Boolean
			BOOL	DbType.Boolean
			NUMERIC	DbType.Decimal
			DECIMAL	DbType.Decimal
			MONEY	DbType.Decimal
			CURRENCY	DbType.Decimal
			TIME	DbType.DateTime
			DATE	DbType.DateTime
			TIMESTAMP	DbType.DateTime
			DATETIME	DbType.DateTime
			BLOB	DbType.Binary
			BINARY	DbType.Binary
			VARBINARY	DbType.Binary
			IMAGE	DbType.Binary
			GENERAL	DbType.Binary
			OLEOBJECT	DbType.Binary
			GUID	DbType.Guid
			UNIQUEIDENTIFIER	DbType.Guid
			MEMO	DbType.String
			NOTE	DbType.String
			LONGTEXT	DbType.String
			LONGCHAR	DbType.String
			SMALLINT	DbType.Int16
			BIGINT	DbType.Int64
			LONGVARCHAR	DbType.String
			SMALLDATE	DbType.DateTime
			SMALLDATETIME	DbType.DateTime
		 */
        #endregion

        private static ILog log = LogManager.GetLogger(typeof(OrmLiteDialectProviderBase));

        public string StringLengthNonUnicodeColumnDefinitionFormat = "VARCHAR({0})";
        public string StringLengthUnicodeColumnDefinitionFormat = "NVARCHAR({0})";

        //Set by Constructor and UpdateStringColumnDefinitions()
        public string StringColumnDefinition;
        public string StringLengthColumnDefinitionFormat;

        public string AutoIncrementDefinition = "AUTOINCREMENT"; //SqlServer express limit
        public string IntColumnDefinition = "INTEGER";
        public string LongColumnDefinition = "BIGINT";
        public string GuidColumnDefinition = "GUID";
        public string BoolColumnDefinition = "BOOL";
        public string RealColumnDefinition = "DOUBLE";
        public string DecimalColumnDefinition = "DECIMAL";
        public string BlobColumnDefinition = "BLOB";
        public string DateTimeColumnDefinition = "DATETIME";
        public string TimeColumnDefinition = "DATETIME";

        protected OrmLiteDialectProviderBase()
        {
            InitColumnTypeMap();
            UpdateStringColumnDefinitions();
        }

        private int defaultStringLength = 8000; //SqlServer express limit
        public int DefaultStringLength
        {
            get
            {
                return defaultStringLength;
            }
            set
            {
                defaultStringLength = value;
                UpdateStringColumnDefinitions();
            }
        }

        private bool useUnicode;
        public virtual bool UseUnicode
        {
            get
            {
                return useUnicode;
            }
            set
            {
                useUnicode = value;
                UpdateStringColumnDefinitions();
            }
        }

        private void UpdateStringColumnDefinitions()
        {
            this.StringLengthColumnDefinitionFormat = useUnicode
                ? StringLengthUnicodeColumnDefinitionFormat
                : StringLengthNonUnicodeColumnDefinitionFormat;

            this.StringColumnDefinition = string.Format(
                this.StringLengthColumnDefinitionFormat, DefaultStringLength);
        }

        protected void InitColumnTypeMap()
        {
            DbTypes<string>.Set(DbType.String, StringColumnDefinition);
			DbTypes<char>.Set(DbType.StringFixedLength, StringColumnDefinition);
			DbTypes<char?>.Set(DbType.StringFixedLength, StringColumnDefinition);
            DbTypes<char[]>.Set(DbType.String, StringColumnDefinition);
			DbTypes<bool>.Set(DbType.Boolean, BoolColumnDefinition);
			DbTypes<bool?>.Set(DbType.Boolean, BoolColumnDefinition);
			DbTypes<Guid>.Set(DbType.Guid, GuidColumnDefinition);
			DbTypes<Guid?>.Set(DbType.Guid, GuidColumnDefinition);
			DbTypes<DateTime>.Set(DbType.DateTime, DateTimeColumnDefinition);
			DbTypes<DateTime?>.Set(DbType.DateTime, DateTimeColumnDefinition);
			DbTypes<TimeSpan>.Set(DbType.Time, TimeColumnDefinition);
			DbTypes<TimeSpan?>.Set(DbType.Time, TimeColumnDefinition);
			DbTypes<DateTimeOffset>.Set(DbType.Time, TimeColumnDefinition);
			DbTypes<DateTimeOffset?>.Set(DbType.Time, TimeColumnDefinition);

			DbTypes<byte>.Set(DbType.Byte, IntColumnDefinition);
			DbTypes<byte?>.Set(DbType.Byte, IntColumnDefinition);
			DbTypes<sbyte>.Set(DbType.SByte, IntColumnDefinition);
			DbTypes<sbyte?>.Set(DbType.SByte, IntColumnDefinition);
			DbTypes<short>.Set(DbType.Int16, IntColumnDefinition);
			DbTypes<short?>.Set(DbType.Int16, IntColumnDefinition);
			DbTypes<ushort>.Set(DbType.UInt16, IntColumnDefinition);
			DbTypes<ushort?>.Set(DbType.UInt16, IntColumnDefinition);
			DbTypes<int>.Set(DbType.Int32, IntColumnDefinition);
			DbTypes<int?>.Set(DbType.Int32, IntColumnDefinition);
			DbTypes<uint>.Set(DbType.UInt32, IntColumnDefinition);
			DbTypes<uint?>.Set(DbType.UInt32, IntColumnDefinition);

			DbTypes<long>.Set(DbType.Int64, LongColumnDefinition);
			DbTypes<long?>.Set(DbType.Int64, LongColumnDefinition);
			DbTypes<ulong>.Set(DbType.UInt64, LongColumnDefinition);
			DbTypes<ulong?>.Set(DbType.UInt64, LongColumnDefinition);

			DbTypes<float>.Set(DbType.Single, RealColumnDefinition);
			DbTypes<float?>.Set(DbType.Single, RealColumnDefinition);
			DbTypes<double>.Set(DbType.Double, RealColumnDefinition);
			DbTypes<double?>.Set(DbType.Double, RealColumnDefinition);

			DbTypes<decimal>.Set(DbType.Decimal, DecimalColumnDefinition);
			DbTypes<decimal?>.Set(DbType.Decimal, DecimalColumnDefinition);

            DbTypes<byte[]>.Set(DbType.Binary, BlobColumnDefinition);
        }

        public string DefaultValueFormat = " DEFAULT ({0})";

        public virtual bool ShouldQuoteValue(Type fieldType)
        {
            string fieldDefinition;
            if (!DbTypes.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition))
            {
                fieldDefinition = this.GetUndefinedColumnDefintion(fieldType);
            }

            return fieldDefinition != IntColumnDefinition
                   && fieldDefinition != LongColumnDefinition
                   && fieldDefinition != RealColumnDefinition
                   && fieldDefinition != DecimalColumnDefinition
                   && fieldDefinition != BoolColumnDefinition;
        }

        public virtual object ConvertDbValue(object value, Type type)
        {
            if (value == null || value.GetType() == typeof(DBNull)) return null;

            if (value.GetType() == type)
            {
                return value;
            }

            if (type.IsValueType)
            {
                if (type == typeof(float))
                    return value is double ? (float)((double)value) : (float)value;

                if (type == typeof(double))
                    return value is float ? (double)((float)value) : (double)value;

                if (type == typeof(decimal))
                    return (decimal)value;
            }

            if (type == typeof(string))
                return value;

            try
            {
                var convertedValue = TypeSerializer.DeserializeFromString(value.ToString(), type);
                return convertedValue;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error ConvertDbValue trying to convert {0} into {1}",
                    value, type.Name);
                throw;
            }
        }

        public virtual string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (!fieldType.UnderlyingSystemType.IsValueType && fieldType != typeof(string))
            {
                if (TypeSerializer.CanCreateFromString(fieldType))
                {
                    return "'" + EscapeParam(TypeSerializer.SerializeToString(value)) + "'";
                }

                throw new NotSupportedException(
                    string.Format("Property of type: {0} is not supported", fieldType.FullName));
            }

            if (fieldType == typeof(float))
                return ((float)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(double))
                return ((double)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(decimal))
                return ((decimal)value).ToString(CultureInfo.InvariantCulture);

            return ShouldQuoteValue(fieldType)
                    ? "'" + EscapeParam(value) + "'"
                    : value.ToString();
        }

        public abstract IDbConnection CreateConnection(string filePath, Dictionary<string, string> options);

        public virtual string EscapeParam(object paramValue)
        {
            return paramValue.ToString().Replace("'", "''");
        }

        protected virtual string GetUndefinedColumnDefintion(Type fieldType)
        {
            if (TypeSerializer.CanCreateFromString(fieldType))
            {
                return this.StringColumnDefinition;
            }

            throw new NotSupportedException(
                string.Format("Property of type: {0} is not supported", fieldType.FullName));
        }

        public virtual string GetColumnDefinition(string fieldName, Type fieldType, bool isPrimaryKey, bool autoIncrement, bool isNullable, int? fieldLength, string defaultValue)
        {
            string fieldDefinition;

            if (fieldType == typeof(string))
            {
                fieldDefinition = string.Format(StringLengthColumnDefinitionFormat, fieldLength.GetValueOrDefault(DefaultStringLength));
            }
            else
            {
                if (!DbTypes.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition))
                {
                    fieldDefinition = this.GetUndefinedColumnDefintion(fieldType);
                }
            }

            var sql = new StringBuilder();
            sql.AppendFormat("\"{0}\" {1}", fieldName, fieldDefinition);

            if (isPrimaryKey)
            {
                sql.Append(" PRIMARY KEY");
                if (autoIncrement)
                {
                    sql.Append(" ").Append(AutoIncrementDefinition);
                }
            }
            else
            {
                if (isNullable)
                {
                    sql.Append(" NULL");
                }
                else
                {
                    sql.Append(" NOT NULL");
                }
            }

            if (!string.IsNullOrEmpty(defaultValue))
            {
                sql.AppendFormat(DefaultValueFormat, defaultValue);
            }

            return sql.ToString();
        }

        public abstract long GetLastInsertId(IDbCommand command);
    }
}