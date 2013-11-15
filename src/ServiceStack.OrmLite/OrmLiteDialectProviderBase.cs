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
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;
using ServiceStack.Text;
using System.Diagnostics;
using ServiceStack.Common;
using System.IO;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{

    public abstract class OrmLiteDialectProviderBase<TDialect>
        : IOrmLiteDialectProvider
        where TDialect : IOrmLiteDialectProvider
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(IOrmLiteDialectProvider));

        [Conditional("DEBUG")]
        private static void LogDebug(string fmt, params object[] args)
        {
            if (args.Length > 0)
                Log.DebugFormat(fmt, args);
            else
                Log.Debug(fmt);
        }

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

        private static ILog log = LogManager.GetLogger(typeof(OrmLiteDialectProviderBase<>));

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
            UpdateStringColumnDefinitions();
        }

        private int defaultDecimalPrecision = 18;
        private int defaultDecimalScale = 12;

        public int DefaultDecimalPrecision
        {
            get { return defaultDecimalPrecision; }
            set { defaultDecimalPrecision = value; }
        }

        public int DefaultDecimalScale
        {
            get { return defaultDecimalScale; }
            set { defaultDecimalScale = value; }
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

        private string paramString = "@";
        public string ParamString
        {
            get { return paramString; }
            set { paramString = value; }
        }

        protected bool useUnicode;
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

        private INamingStrategy namingStrategy = new OrmLiteNamingStrategyBase();
        public INamingStrategy NamingStrategy
        {
            get
            {
                return namingStrategy;
            }
            set
            {
                namingStrategy = value;
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

        protected DbTypes<TDialect> DbTypeMap = new DbTypes<TDialect>();
        protected void InitColumnTypeMap()
        {
            DbTypeMap.Set<string>(DbType.String, StringColumnDefinition);
            DbTypeMap.Set<char>(DbType.StringFixedLength, StringColumnDefinition);
            DbTypeMap.Set<char?>(DbType.StringFixedLength, StringColumnDefinition);
            DbTypeMap.Set<char[]>(DbType.String, StringColumnDefinition);
            DbTypeMap.Set<bool>(DbType.Boolean, BoolColumnDefinition);
            DbTypeMap.Set<bool?>(DbType.Boolean, BoolColumnDefinition);
            DbTypeMap.Set<Guid>(DbType.Guid, GuidColumnDefinition);
            DbTypeMap.Set<Guid?>(DbType.Guid, GuidColumnDefinition);
            DbTypeMap.Set<DateTime>(DbType.DateTime, DateTimeColumnDefinition);
            DbTypeMap.Set<DateTime?>(DbType.DateTime, DateTimeColumnDefinition);
            DbTypeMap.Set<TimeSpan>(DbType.Time, TimeColumnDefinition);
            DbTypeMap.Set<TimeSpan?>(DbType.Time, TimeColumnDefinition);
            DbTypeMap.Set<DateTimeOffset>(DbType.Time, TimeColumnDefinition);
            DbTypeMap.Set<DateTimeOffset?>(DbType.Time, TimeColumnDefinition);

            DbTypeMap.Set<byte>(DbType.Byte, IntColumnDefinition);
            DbTypeMap.Set<byte?>(DbType.Byte, IntColumnDefinition);
            DbTypeMap.Set<sbyte>(DbType.SByte, IntColumnDefinition);
            DbTypeMap.Set<sbyte?>(DbType.SByte, IntColumnDefinition);
            DbTypeMap.Set<short>(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set<short?>(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set<ushort>(DbType.UInt16, IntColumnDefinition);
            DbTypeMap.Set<ushort?>(DbType.UInt16, IntColumnDefinition);
            DbTypeMap.Set<int>(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set<int?>(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set<uint>(DbType.UInt32, IntColumnDefinition);
            DbTypeMap.Set<uint?>(DbType.UInt32, IntColumnDefinition);

            DbTypeMap.Set<long>(DbType.Int64, LongColumnDefinition);
            DbTypeMap.Set<long?>(DbType.Int64, LongColumnDefinition);
            DbTypeMap.Set<ulong>(DbType.UInt64, LongColumnDefinition);
            DbTypeMap.Set<ulong?>(DbType.UInt64, LongColumnDefinition);

            DbTypeMap.Set<float>(DbType.Single, RealColumnDefinition);
            DbTypeMap.Set<float?>(DbType.Single, RealColumnDefinition);
            DbTypeMap.Set<double>(DbType.Double, RealColumnDefinition);
            DbTypeMap.Set<double?>(DbType.Double, RealColumnDefinition);

            DbTypeMap.Set<decimal>(DbType.Decimal, DecimalColumnDefinition);
            DbTypeMap.Set<decimal?>(DbType.Decimal, DecimalColumnDefinition);

            DbTypeMap.Set<byte[]>(DbType.Binary, BlobColumnDefinition);

            DbTypeMap.Set<object>(DbType.Object, StringColumnDefinition);
        }

        public string DefaultValueFormat = " DEFAULT ({0})";

        public virtual bool ShouldQuoteValue(Type fieldType)
        {
            string fieldDefinition;
            if (!DbTypeMap.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition))
            {
                fieldDefinition = this.GetUndefinedColumnDefinition(fieldType, null);
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
                if (type == typeof(byte[]))
                    return TypeSerializer.DeserializeFromStream<byte[]>(new MemoryStream((byte[])value));

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
            catch (Exception)
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
                    return OrmLiteConfig.DialectProvider.GetQuotedParam(TypeSerializer.SerializeToString(value));
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
                    ? OrmLiteConfig.DialectProvider.GetQuotedParam(value.ToString())
                    : value.ToString();
        }

        public abstract IDbConnection CreateConnection(string filePath, Dictionary<string, string> options);

        public virtual string GetQuotedParam(string paramValue)
        {
            return "'" + paramValue.Replace("'", "''") + "'";
        }

        public virtual string GetQuotedTableName(ModelDefinition modelDef)
        {
            return GetQuotedTableName(modelDef.ModelName);
        }

        public virtual string GetQuotedTableName(string tableName)
        {
            return string.Format("\"{0}\"", namingStrategy.GetTableName(tableName));
        }

        public virtual string GetQuotedColumnName(string columnName)
        {
            return string.Format("\"{0}\"", namingStrategy.GetColumnName(columnName));
        }

        public virtual string GetQuotedName(string name)
        {
            return string.Format("\"{0}\"", name);
        }

        protected virtual string GetUndefinedColumnDefinition(Type fieldType, int? fieldLength)
        {
            if (TypeSerializer.CanCreateFromString(fieldType))
            {
                return string.Format(StringLengthColumnDefinitionFormat, fieldLength.GetValueOrDefault(DefaultStringLength));
            }

            throw new NotSupportedException(
                string.Format("Property of type: {0} is not supported", fieldType.FullName));
        }

        public virtual string GetColumnDefinition(string fieldName, Type fieldType,
            bool isPrimaryKey, bool autoIncrement, bool isNullable,
            int? fieldLength, int? scale, string defaultValue)
        {
            string fieldDefinition;

            if (fieldType == typeof(string))
            {
                fieldDefinition = string.Format(StringLengthColumnDefinitionFormat, fieldLength.GetValueOrDefault(DefaultStringLength));
            }
            else
            {
                if (!DbTypeMap.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition))
                {
                    fieldDefinition = this.GetUndefinedColumnDefinition(fieldType, fieldLength);
                }
            }

            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldName), fieldDefinition);

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

        public virtual string SelectIdentitySql { get; set; }

        public virtual long GetLastInsertId(IDbCommand dbCmd)
        {
            if (SelectIdentitySql == null)
                throw new NotImplementedException("Returning last inserted identity is not implemented on this DB Provider.");

            dbCmd.CommandText = SelectIdentitySql;
            return dbCmd.GetLongScalar();
        }

        public virtual long InsertAndGetLastInsertId<T>(IDbCommand dbCmd)
        {
            if (SelectIdentitySql == null)
                throw new NotImplementedException("Returning last inserted identity is not implemented on this DB Provider.");
            
            dbCmd.CommandText += "; " + SelectIdentitySql;
            return dbCmd.GetLongScalar();
        }
        
        public virtual string ToCountStatement(Type fromTableType, string sqlFilter, params object[] filterParams)
        {
            var sql = new StringBuilder();
            const string SelectStatement = "SELECT ";
            var modelDef = fromTableType.GetModelDefinition();
            var isFullSelectStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.TrimStart().StartsWith(SelectStatement, StringComparison.OrdinalIgnoreCase);

            if (isFullSelectStatement) return (filterParams != null ? sqlFilter.SqlFormat(filterParams) : sqlFilter);

            sql.AppendFormat("SELECT {0} FROM {1}", "COUNT(*)",
                             GetQuotedTableName(modelDef));
            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = filterParams != null ? sqlFilter.SqlFormat(filterParams) : sqlFilter;
                if ((!sqlFilter.StartsWith("ORDER ", StringComparison.InvariantCultureIgnoreCase)
                    && !sqlFilter.StartsWith("LIMIT ", StringComparison.InvariantCultureIgnoreCase))
                    && (!sqlFilter.StartsWith("WHERE ", StringComparison.InvariantCultureIgnoreCase)))
                {
                    sql.Append(" WHERE ");
                }
                sql.Append(" " + sqlFilter);
            }
            return sql.ToString();
        }

        public virtual string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            const string SelectStatement = "SELECT";
            var isFullSelectStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.TrimStart().StartsWith(SelectStatement, StringComparison.InvariantCultureIgnoreCase);

            if (isFullSelectStatement) 
                return sqlFilter.SqlFormat(filterParams);

            var modelDef = tableType.GetModelDefinition();
            var sql = new StringBuilder("SELECT " + tableType.GetColumnNames() + " FROM " + GetQuotedTableName(modelDef));

            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFormat(filterParams);
                if (!sqlFilter.StartsWith("ORDER ", StringComparison.InvariantCultureIgnoreCase)
                    && !sqlFilter.StartsWith("LIMIT ", StringComparison.InvariantCultureIgnoreCase))
                {
                    sql.Append(" WHERE ");
                }

                sql.Append(sqlFilter);
            }

            return sql.ToString();
        }

        public virtual string ToInsertRowStatement(IDbCommand command, object objWithProperties, ICollection<string> insertFields = null)
        {
            if (insertFields == null) 
                insertFields = new List<string>();

            var sbColumnNames = new StringBuilder();
            var sbColumnValues = new StringBuilder();
            var modelDef = objWithProperties.GetType().GetModelDefinition();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsComputed) continue;
                if (fieldDef.AutoIncrement) continue;
                //insertFields contains Property "Name" of fields to insert ( that's how expressions work )
                if (insertFields.Count > 0 && !insertFields.Contains(fieldDef.Name)) continue;

                if (sbColumnNames.Length > 0) sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));
                    sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties));
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToInsertRowStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                    GetQuotedTableName(modelDef), sbColumnNames, sbColumnValues);

            return sql;
        }

        public virtual IDbCommand CreateParameterizedInsertStatement(IDbConnection connection, object objWithProperties, ICollection<string> insertFields = null)
        {
            if (insertFields == null) 
                insertFields = new List<string>();

            var sbColumnNames = new StringBuilder();
            var sbColumnValues = new StringBuilder();
            var modelDef = objWithProperties.GetType().GetModelDefinition();

            var cmd = connection.CreateCommand();
            cmd.CommandTimeout = OrmLiteConfig.CommandTimeout;

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsComputed) continue;
                if (fieldDef.AutoIncrement)
                        continue;
                    
                //insertFields contains Property "Name" of fields to insert ( that's how expressions work )
                if (insertFields.Count > 0 && !insertFields.Contains(fieldDef.Name)) continue;

                if (sbColumnNames.Length > 0) sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));
                    sbColumnValues.Append(ParamString)
                                  .Append(fieldDef.FieldName);

                    AddParameterForFieldToCommand(cmd, fieldDef, objWithProperties);
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in CreateParameterizedInsertStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            cmd.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                            GetQuotedTableName(modelDef), sbColumnNames, sbColumnValues);

            return cmd;
        }

        public void ReParameterizeInsertStatement(IDbCommand command, object objWithProperties, ICollection<string> insertFields = null)
        {
            if (insertFields == null) 
                insertFields = new List<string>();

            var modelDef = objWithProperties.GetType().GetModelDefinition();
            
            command.Parameters.Clear();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsComputed) continue;
                if (fieldDef.AutoIncrement) continue;
                //insertFields contains Property "Name" of fields to insert ( that's how expressions work )
                if (insertFields.Count > 0 && !insertFields.Contains(fieldDef.Name)) continue;

                try
                {
                    AddParameterForFieldToCommand(command, fieldDef, objWithProperties);
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ReParameterizeInsertStatement(): " + ex.Message, ex);
                    throw;
                }
            }
        }

        protected virtual void AddParameterForFieldToCommand(IDbCommand command, FieldDefinition fieldDef, object objWithProperties)
        {
            var p = command.CreateParameter();
            p.ParameterName = string.Format("{0}{1}", ParamString, fieldDef.FieldName);

            if (DbTypeMap.ColumnDbTypeMap.ContainsKey(fieldDef.FieldType))
            {
                p.DbType = DbTypeMap.ColumnDbTypeMap[fieldDef.FieldType];
                p.Value = GetValueOrDbNull(fieldDef, objWithProperties);
            }
            else
            {
                p.DbType = DbType.String;
                p.Value = GetQuotedValueOrDbNull(fieldDef, objWithProperties);
            }

            command.Parameters.Add(p);
        }

        protected object GetValueOrDbNull(FieldDefinition fieldDef, object objWithProperties)
        {
            return fieldDef.GetValue(objWithProperties) ?? DBNull.Value;
        }

        protected object GetQuotedValueOrDbNull(FieldDefinition fieldDef, object objWithProperties)
        {
            var value = fieldDef.GetValue(objWithProperties);

            if (value == null)
                return DBNull.Value;

            var unquotedVal = OrmLiteConfig.DialectProvider.GetQuotedValue(value, fieldDef.FieldType)
                .TrimStart('\'').TrimEnd('\''); ;

            if (string.IsNullOrEmpty(unquotedVal))
                return DBNull.Value;

            return unquotedVal;
        }

        public virtual string ToUpdateRowStatement(object objWithProperties, ICollection<string> updateFields = null)
        {
            if (updateFields == null) 
                updateFields = new List<string>();

            var sqlFilter = new StringBuilder();
            var sql = new StringBuilder();
            var modelDef = objWithProperties.GetType().GetModelDefinition();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsComputed) continue;

                try
                {
                    if (fieldDef.IsPrimaryKey && updateFields.Count == 0)
                    {
                        if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");

                        sqlFilter.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), fieldDef.GetQuotedValue(objWithProperties));

                        continue;
                    }

                    if (updateFields.Count > 0 && !updateFields.Contains(fieldDef.Name) || fieldDef.AutoIncrement) continue;
                    if (sql.Length > 0) sql.Append(",");
                    sql.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), fieldDef.GetQuotedValue(objWithProperties));
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToUpdateRowStatement(): " + ex.Message, ex);
                }
            }

            var updateSql = string.Format("UPDATE {0} SET {1}{2}",
                GetQuotedTableName(modelDef), sql, (sqlFilter.Length > 0 ? " WHERE " + sqlFilter : ""));

            if (sql.Length == 0)
                throw new Exception("No valid update properties provided (e.g. p => p.FirstName): " + updateSql);
            
            return updateSql;
        }

        public virtual IDbCommand CreateParameterizedUpdateStatement(IDbConnection connection, object objWithProperties, ICollection<string> updateFields = null)
        {
            if (updateFields == null) 
                updateFields = new List<string>();

            var sqlFilter = new StringBuilder();
            var sql = new StringBuilder();
            var modelDef = objWithProperties.GetType().GetModelDefinition();

            var command = connection.CreateCommand();
            command.CommandTimeout = OrmLiteConfig.CommandTimeout;
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsComputed) continue;
                try
                {
                    if (fieldDef.IsPrimaryKey && updateFields.Count == 0)
                    {
                        if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");

                        sqlFilter.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), String.Concat(ParamString, fieldDef.FieldName));
                        AddParameterForFieldToCommand(command, fieldDef, objWithProperties);

                        continue;
                    }

                    if (updateFields.Count > 0 && !updateFields.Contains(fieldDef.Name)) continue;
                    if (sql.Length > 0) sql.Append(",");
                    sql.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), String.Concat(ParamString, fieldDef.FieldName));

                    AddParameterForFieldToCommand(command, fieldDef, objWithProperties);
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in CreateParameterizedUpdateStatement(): " + ex.Message, ex);
                }
            }

            command.CommandText = string.Format("UPDATE {0} SET {1} {2}", GetQuotedTableName(modelDef), sql, (sqlFilter.Length > 0 ? "WHERE " + sqlFilter : ""));
            return command;
        }

        public virtual string ToDeleteRowStatement(object objWithProperties)
        {
            var sqlFilter = new StringBuilder();
            var modelDef = objWithProperties.GetType().GetModelDefinition();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                try
                {
                    if (fieldDef.IsPrimaryKey)
                    {
                        if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");

                        sqlFilter.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), fieldDef.GetQuotedValue(objWithProperties));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToDeleteRowStatement(): " + ex.Message, ex);
                }
            }

            var deleteSql = string.Format("DELETE FROM {0} WHERE {1}",
                GetQuotedTableName(modelDef), sqlFilter);

            return deleteSql;
        }

        public virtual string ToDeleteStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            var sql = new StringBuilder();
            const string deleteStatement = "DELETE ";

            var isFullDeleteStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.Length > deleteStatement.Length
                && sqlFilter.Substring(0, deleteStatement.Length).ToUpper().Equals(deleteStatement);

            if (isFullDeleteStatement) return sqlFilter.SqlFormat(filterParams);

            var modelDef = tableType.GetModelDefinition();
            sql.AppendFormat("DELETE FROM {0}", GetQuotedTableName(modelDef));
            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFormat(filterParams);
                sql.Append(" WHERE ");
                sql.Append(sqlFilter);
            }

            return sql.ToString();
        }

        public virtual string ToCreateTableStatement(Type tableType)
        {
            var sbColumns = new StringBuilder();
            var sbConstraints = new StringBuilder();

            var modelDef = tableType.GetModelDefinition();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumns.Length != 0) sbColumns.Append(", \n  ");

                var columnDefinition = GetColumnDefinition(
                    fieldDef.FieldName,
                    fieldDef.FieldType,
                    fieldDef.IsPrimaryKey,
                    fieldDef.AutoIncrement,
                    fieldDef.IsNullable,
                    fieldDef.FieldLength,
                    null,
                    fieldDef.DefaultValue);

                sbColumns.Append(columnDefinition);

                if (fieldDef.ForeignKey == null) continue;

                var refModelDef = fieldDef.ForeignKey.ReferenceType.GetModelDefinition();
                sbConstraints.AppendFormat(
                    ", \n\n  CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3})",
                    GetQuotedName(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy, fieldDef)),
                    GetQuotedColumnName(fieldDef.FieldName),
                    GetQuotedTableName(refModelDef),
                    GetQuotedColumnName(refModelDef.PrimaryKey.FieldName));

                sbConstraints.Append(GetForeignKeyOnDeleteClause(fieldDef.ForeignKey));
                sbConstraints.Append(GetForeignKeyOnUpdateClause(fieldDef.ForeignKey));
            }
            var sql = new StringBuilder(string.Format(
                "CREATE TABLE {0} \n(\n  {1}{2} \n); \n", GetQuotedTableName(modelDef), sbColumns, sbConstraints));

            return sql.ToString();
        }

        public virtual string GetForeignKeyOnDeleteClause(ForeignKeyConstraint foreignKey)
        {
            return !string.IsNullOrEmpty(foreignKey.OnDelete) ? " ON DELETE " + foreignKey.OnDelete : "";
        }

        public virtual string GetForeignKeyOnUpdateClause(ForeignKeyConstraint foreignKey)
        {
            return !string.IsNullOrEmpty(foreignKey.OnUpdate) ? " ON UPDATE " + foreignKey.OnUpdate : "";
        }

        public virtual List<string> ToCreateIndexStatements(Type tableType)
        {
            var sqlIndexes = new List<string>();

            var modelDef = tableType.GetModelDefinition();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!fieldDef.IsIndexed) continue;

                var indexName = GetIndexName(fieldDef.IsUnique, modelDef.ModelName.SafeVarName(), fieldDef.FieldName);

                sqlIndexes.Add(
                    ToCreateIndexStatement(fieldDef.IsUnique, indexName, modelDef, fieldDef.FieldName));
            }

            foreach (var compositeIndex in modelDef.CompositeIndexes)
            {
                var indexName = GetCompositeIndexName(compositeIndex, modelDef);
                var indexNames = string.Join(" ASC, ",
                                             compositeIndex.FieldNames.ConvertAll(
                                                 n => GetQuotedName(n)).ToArray());

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef, indexNames, true));
            }

            return sqlIndexes;
        }

        public virtual DbType GetColumnDbType(Type valueType)
        {
            if (valueType.IsEnum)
                return DbTypeMap.ColumnDbTypeMap[typeof(string)];

            return DbTypeMap.ColumnDbTypeMap[valueType];
        }

        public virtual string GetColumnTypeDefinition(Type fieldType)
        {
            string fieldDefinition;
            DbTypeMap.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition);
            return fieldDefinition ?? GetUndefinedColumnDefinition(fieldType, null);
        }

        public virtual bool DoesTableExist(IDbConnection db, string tableName)
        {
            return db.Exec(dbCmd => DoesTableExist(dbCmd, tableName));
        }

        public virtual bool DoesTableExist(IDbCommand dbCmd, string tableName)
        {
            return false;
        }

        public virtual bool DoesSequenceExist(IDbCommand dbCmd, string sequenceName)
        {
            return true;
        }

        protected virtual string GetIndexName(bool isUnique, string modelName, string fieldName)
        {
            return string.Format("{0}idx_{1}_{2}", isUnique ? "u" : "", modelName, fieldName).ToLower();
        }

        protected virtual string GetCompositeIndexName(CompositeIndexAttribute compositeIndex, ModelDefinition modelDef)
        {
            return compositeIndex.Name ?? GetIndexName(compositeIndex.Unique, modelDef.ModelName.SafeVarName(),
                                                       string.Join("_", compositeIndex.FieldNames.ToArray()));
        }

        protected virtual string GetCompositeIndexNameWithSchema(CompositeIndexAttribute compositeIndex, ModelDefinition modelDef)
        {
            return compositeIndex.Name ?? GetIndexName(compositeIndex.Unique,
                    (modelDef.IsInSchema ?
                        modelDef.Schema + "_" + GetQuotedTableName(modelDef) :
                        GetQuotedTableName(modelDef)).SafeVarName(),
                    string.Join("_", compositeIndex.FieldNames.ToArray()));
        }

        protected virtual string ToCreateIndexStatement(bool isUnique, string indexName, ModelDefinition modelDef, string fieldName, bool isCombined = false)
        {
            return string.Format("CREATE {0} INDEX {1} ON {2} ({3} ASC); \n",
                                 isUnique ? "UNIQUE" : "", indexName,
                                 GetQuotedTableName(modelDef),
                                 (isCombined) ? fieldName : GetQuotedColumnName(fieldName));
        }

        public virtual string GetColumnNames(ModelDefinition modelDef)
        {
            return modelDef.GetColumnNames();
        }

        public virtual List<string> ToCreateSequenceStatements(Type tableType)
        {
            return new List<string>();
        }

        public virtual string ToCreateSequenceStatement(Type tableType, string sequenceName)
        {
            return "";
        }        

        public virtual List<string> SequenceList(Type tableType)
        {
            return new List<string>();
        }

        // TODO : make abstract  ??
        public virtual string ToExistStatement(Type fromTableType,
            object objWithProperties,
            string sqlFilter,
            params object[] filterParams)
        {
            throw new NotImplementedException();
        }

        // TODO : make abstract  ??
        public virtual string ToSelectFromProcedureStatement(
            object fromObjWithProperties,
            Type outputModelType,
            string sqlFilter,
            params object[] filterParams)
        {
            throw new NotImplementedException();
        }

        // TODO : make abstract  ??
        public virtual string ToExecuteProcedureStatement(object objWithProperties)
        {
            throw new NotImplementedException();
        }

        protected static ModelDefinition GetModel(Type modelType)
        {
            return modelType.GetModelDefinition();
        }

        public virtual SqlExpressionVisitor<T> ExpressionVisitor<T>()
        {
            throw new NotImplementedException();
        }

        public IDbCommand CreateParameterizedDeleteStatement(IDbConnection connection, object objWithProperties)
        {
            throw new NotImplementedException();
        }

        public virtual string GetDropForeignKeyConstraints(ModelDefinition modelDef)
        {
            return null;
        }

        public static ModelDefinition GetModelDefinition(Type modelType)
        {
            return modelType.GetModelDefinition();
        }

		#region DDL
		public virtual string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef)
		{

			var column = GetColumnDefinition(fieldDef.FieldName,
			                                 fieldDef.FieldType,
			                                 fieldDef.IsPrimaryKey,
			                                 fieldDef.AutoIncrement,
			                                 fieldDef.IsNullable,
			                                 fieldDef.FieldLength,
			                                 fieldDef.Scale,
			                                 fieldDef.DefaultValue);
			return string.Format("ALTER TABLE {0} ADD COLUMN {1};",
			                     GetQuotedTableName(modelType.GetModelDefinition().ModelName),
			                     column);
		}


		public virtual string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef)
		{		
			var column = GetColumnDefinition(fieldDef.FieldName,
			                                 fieldDef.FieldType,
			                                 fieldDef.IsPrimaryKey,
			                                 fieldDef.AutoIncrement,
			                                 fieldDef.IsNullable,
			                                 fieldDef.FieldLength,
			                                 fieldDef.Scale,
			                                 fieldDef.DefaultValue);
			return string.Format("ALTER TABLE {0} MODIFY COLUMN {1};",
			                     GetQuotedTableName(modelType.GetModelDefinition().ModelName),
			                     column);
		}
		
		public virtual string ToChangeColumnNameStatement(Type modelType,
		                                                  FieldDefinition fieldDef,
		                                                  string oldColumnName)
		{
			var column = GetColumnDefinition(fieldDef.FieldName,
			                                 fieldDef.FieldType,
			                                 fieldDef.IsPrimaryKey,
			                                 fieldDef.AutoIncrement,
			                                 fieldDef.IsNullable,
			                                 fieldDef.FieldLength,
			                                 fieldDef.Scale,
			                                 fieldDef.DefaultValue);
			return string.Format("ALTER TABLE {0} CHANGE COLUMN {1} {2};",
			                     GetQuotedTableName(modelType.GetModelDefinition().ModelName),
			                     GetQuotedColumnName(oldColumnName),
			                     column);
		}

		public virtual string  ToAddForeignKeyStatement<T,TForeign>(Expression<Func<T,object>> field,
		                                                            Expression<Func<TForeign,object>> foreignField,
		                                                            OnFkOption onUpdate,
		                                                            OnFkOption onDelete,
		                                                            string foreignKeyName=null){
			var sourceMD = ModelDefinition<T>.Definition;
			var fieldName = sourceMD.GetFieldDefinition (field).FieldName; 
						
			var referenceMD=ModelDefinition<TForeign>.Definition;
			var referenceFieldName= referenceMD.GetFieldDefinition(foreignField).FieldName;
			
			string name = GetQuotedName(foreignKeyName.IsNullOrEmpty()?
			                            "fk_"+sourceMD.ModelName+"_"+ fieldName+"_"+referenceFieldName:
			                            foreignKeyName);
			
			return string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6};",
			                     GetQuotedTableName(sourceMD.ModelName),
			                     name,
			                     GetQuotedColumnName(fieldName),
			                     GetQuotedTableName(referenceMD.ModelName),
			                     GetQuotedColumnName(referenceFieldName),
			                     GetForeignKeyOnDeleteClause(new ForeignKeyConstraint(typeof(T), onDelete: FkOptionToString( onDelete))),
			                     GetForeignKeyOnUpdateClause(new ForeignKeyConstraint(typeof(T), onUpdate: FkOptionToString(onUpdate))));	
		}

		public virtual string ToCreateIndexStatement<T>(Expression<Func<T,object>> field,
		                                                string indexName=null, bool unique=false)
		{
			
			var sourceMD = ModelDefinition<T>.Definition;
			var fieldName = sourceMD.GetFieldDefinition (field).FieldName;
			
			string name =GetQuotedName(indexName.IsNullOrEmpty()?
			                           (unique?"uidx":"idx") +"_"+sourceMD.ModelName+"_"+fieldName:
			                           indexName);
			
			string command = string.Format("CREATE{0}INDEX {1} ON {2}({3});",
			                               unique?" UNIQUE ": " ",
			                               name,
			                               GetQuotedTableName(sourceMD.ModelName),
			                               GetQuotedColumnName(fieldName)
			                               );
			return command;
		}


		protected virtual string FkOptionToString(OnFkOption option){
			switch(option){
			case OnFkOption.Cascade: return "CASCADE";
			case OnFkOption.NoAction: return "NO ACTION"; 
			case OnFkOption.SetNull: return "SET NULL"; 
			case OnFkOption.SetDefault: return "SET DEFAULT";
			case OnFkOption.Restrict:
			default: return "RESTRICT";
			}
		}

		#endregion DDL

    }
}