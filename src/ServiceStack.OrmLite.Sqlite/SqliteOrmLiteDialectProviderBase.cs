using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using ServiceStack.Text.Common;

namespace ServiceStack.OrmLite.Sqlite
{
    public abstract class SqliteOrmLiteDialectProviderBase : OrmLiteDialectProviderBase<SqliteOrmLiteDialectProviderBase>
    {
        protected SqliteOrmLiteDialectProviderBase()
        {
            base.MaxStringColumnDefinition = "VARCHAR(1000000)"; //Default Max is really 1B
            base.DateTimeColumnDefinition = base.StringColumnDefinition;
            base.BoolColumnDefinition = base.IntColumnDefinition;
            base.GuidColumnDefinition = "CHAR(36)";
            base.SelectIdentitySql = "SELECT last_insert_rowid()";

            base.InitColumnTypeMap();
        }

        public override void OnAfterInitColumnTypeMap()
        {
            DbTypeMap.Set<Guid>(DbType.String, GuidColumnDefinition);
            DbTypeMap.Set<Guid?>(DbType.String, GuidColumnDefinition);
            DbTypeMap.Set<DateTimeOffset>(DbType.DateTimeOffset, StringColumnDefinition);
            DbTypeMap.Set<DateTimeOffset?>(DbType.DateTimeOffset, StringColumnDefinition);
        }

        public static string Password { get; set; }
        public static bool UTF8Encoded { get; set; }
        public static bool ParseViaFramework { get; set; }

        public static string CreateFullTextCreateTableStatement(object objectWithProperties)
        {
            var sbColumns = new StringBuilder();
            foreach (var propertyInfo in objectWithProperties.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var columnDefinition = (sbColumns.Length == 0)
                    ? string.Format("{0} TEXT PRIMARY KEY", propertyInfo.Name)
                    : string.Format(", {0} TEXT", propertyInfo.Name);

                sbColumns.AppendLine(columnDefinition);
            }

            var tableName = objectWithProperties.GetType().Name;
            var sql = string.Format("CREATE VIRTUAL TABLE \"{0}\" USING FTS3 ({1});", tableName, sbColumns);

            return sql;
        }

        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            var isFullConnectionString = connectionString.Contains(";");
            var connString = new StringBuilder();
            if (!isFullConnectionString)
            {
                if (connectionString != ":memory:")
                {
                    var existingDir = Path.GetDirectoryName(connectionString);
                    if (!string.IsNullOrEmpty(existingDir) && !Directory.Exists(existingDir))
                    {
                        Directory.CreateDirectory(existingDir);
                    }
                }
                connString.AppendFormat(@"Data Source={0};Version=3;New=True;Compress=True;", connectionString.Trim());

            }
            else
            {
                connString.Append(connectionString);
            }
            if (!string.IsNullOrEmpty(Password))
            {
                connString.AppendFormat("Password={0};", Password);
            }
            if (UTF8Encoded)
            {
                connString.Append("UseUTF16Encoding=True;");
            }

            if (options != null)
            {
                foreach (var option in options)
                {
                    connString.AppendFormat("{0}={1};", option.Key, option.Value);
                }
            }

            return CreateConnection(connString.ToString());
        }


        protected abstract IDbConnection CreateConnection(string connectionString);

        public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            if (!modelDef.IsInSchema)
                return base.GetQuotedTableName(modelDef);

            return string.Format("\"{0}_{1}\"", modelDef.Schema, modelDef.ModelName);
        }

        public override object ConvertDbValue(object value, Type type)
        {
            if (value == null || value is DBNull) return null;

            if (type == typeof(bool) && !(value is bool))
            {
                var intVal = int.Parse(value.ToString());
                return intVal != 0; 
            }

            if (type == typeof(DateTimeOffset))
            {
                var moment = DateTimeOffset.Parse((string)value, null, DateTimeStyles.RoundtripKind);
                return moment;
            }

            try
            {
                return base.ConvertDbValue(value, type);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override void SetParameter(FieldDefinition fieldDef, IDbDataParameter p)
        {
            base.SetParameter(fieldDef, p);
        }

        public override void SetDbValue(FieldDefinition fieldDef, IDataReader dataReader, int colIndex, object instance)
        {
            if (fieldDef == null || fieldDef.SetValueFn == null || colIndex == NotFound) return;
            if (dataReader.IsDBNull(colIndex))
            {
                if (fieldDef.IsNullable)
                {
                    fieldDef.SetValueFn(instance, null);
                }
                else
                {
                    fieldDef.SetValueFn(instance, fieldDef.FieldType.GetDefaultValue());
                }
                return;
            }

            var fieldType = Nullable.GetUnderlyingType(fieldDef.FieldType) ?? fieldDef.FieldType;
            if (fieldType == typeof(Guid))
            {
                var guidStr = dataReader.GetString(colIndex);
                var guidValue = new Guid(guidStr);

                fieldDef.SetValueFn(instance, guidValue);
            }
            else if (fieldType == typeof(DateTime))
            {
                try
                {
                    var dbValue = dataReader.GetDateTime(colIndex);

                    fieldDef.SetValueFn(instance, dbValue);
                }
                catch (Exception)
                {
                    var dateStr = dataReader.GetString(colIndex);
                    var dateValue = DateTimeSerializer.ParseShortestXsdDateTime(dateStr);
                    fieldDef.SetValueFn(instance, dateValue);
                }
            }
            else
            {
                base.SetDbValue(fieldDef, dataReader, colIndex, instance);
            }
        }

        public override string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (fieldType == typeof(DateTime))
            {
                var dateValue = (DateTime)value;
                
                //Not forcing co-ercsion into UTC for Sqlite
                var dateStr = DateTimeSerializer.ToLocalXsdDateTimeString(dateValue);
                const int tzPos = 6; //"-00:00".Length;
                var timeZoneMod = dateStr.Substring(dateStr.Length - tzPos, 1);
                if (timeZoneMod == "+" || timeZoneMod == "-")
                {
                    dateStr = dateStr.Substring(0, dateStr.Length - tzPos);
                }
                return base.GetQuotedValue(dateStr, typeof(string));
            }
            if (fieldType == typeof(bool))
            {
                var boolValue = (bool)value;
                return base.GetQuotedValue(boolValue ? 1 : 0, typeof(int));
            }

            // output datetimeoffset as a string formatted for roundtripping.
            if (fieldType == typeof (DateTimeOffset))
            {
                var dateTimeOffsetValue = (DateTimeOffset) value;
                return base.GetQuotedValue(dateTimeOffsetValue.ToString("o"), typeof (string));
            }

            return base.GetQuotedValue(value, fieldType);
        }

        protected override object GetValueOrDbNull<T>(FieldDefinition fieldDef, object obj)
        {
            var value = GetValue<T>(fieldDef, obj);
            if (fieldDef.FieldType == typeof(DateTimeOffset))
            {
                var dateTimeOffsetValue = (DateTimeOffset)value;
                return dateTimeOffsetValue.ToString("o");
            }

            return value ?? DBNull.Value;
        }

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new SqliteExpression<T>();
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
        {
            var sql = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = {0}"
                .SqlFmt(tableName);

            dbCmd.CommandText = sql;
            var result = dbCmd.LongScalar();

            return result > 0;
        }

        public override string GetColumnDefinition(string fieldName, Type fieldType, bool isPrimaryKey, bool autoIncrement, 
            bool isNullable, int? fieldLength, int? scale, string defaultValue, string customFieldDefinition)
        {
            // http://www.sqlite.org/lang_createtable.html#rowid
            var ret = base.GetColumnDefinition(fieldName, fieldType, isPrimaryKey, autoIncrement, isNullable, fieldLength, scale, defaultValue, customFieldDefinition);
            if (isPrimaryKey)
                return ret.Replace(" BIGINT ", " INTEGER ");
            return ret;
        }
    }

    public static class SqliteExtensions
    {
        public static IOrmLiteDialectProvider Configure(this IOrmLiteDialectProvider provider,
            string password = null, bool parseViaFramework = false, bool utf8Encoding = false)
        {
            if (password != null)
                SqliteOrmLiteDialectProviderBase.Password = password;
            if (parseViaFramework)
                SqliteOrmLiteDialectProviderBase.ParseViaFramework = true;
            if (utf8Encoding)
                SqliteOrmLiteDialectProviderBase.UTF8Encoded = true;

            return provider;
        }
    }
}
