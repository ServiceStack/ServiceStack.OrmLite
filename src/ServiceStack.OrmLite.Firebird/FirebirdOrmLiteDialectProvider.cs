using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Firebird.Converters;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Firebird
{
    public class FirebirdOrmLiteDialectProvider : OrmLiteDialectProviderBase<FirebirdOrmLiteDialectProvider>
    {
        public static List<string> RESERVED = new List<string>(new[] {
            "USER","ORDER","PASSWORD", "ACTIVE","LEFT","DOUBLE", "FLOAT", "DECIMAL","STRING", "DATE","DATETIME", "TYPE","TIMESTAMP",
            "INDEX","UNIQUE", "PRIMARY", "KEY", "ALTER", "DROP", "CREATE", "DELETE", "VALUES", "FUNCTION", "INT", "LONG", "CHAR", "VALUE", "TIME"
        });

        public static FirebirdOrmLiteDialectProvider Instance = new FirebirdOrmLiteDialectProvider();

        internal long LastInsertId { get; set; }

        public FirebirdOrmLiteDialectProvider() : this(false) { }

        public FirebirdOrmLiteDialectProvider(bool compactGuid)
        {
            base.AutoIncrementDefinition = string.Empty;
            DefaultValueFormat = " DEFAULT {0}";
            NamingStrategy = new FirebirdNamingStrategy();

            base.InitColumnTypeMap();

            if (compactGuid)
                base.RegisterConverter<Guid>(new FirebirdCompactGuidConverter());
            else
                base.RegisterConverter<Guid>(new FirebirdGuidConverter());

            base.RegisterConverter<DateTime>(new FirebirdDateTimeConverter());
            base.RegisterConverter<DateTimeOffset>(new FirebirdDateTimeOffsetConverter());

            base.RegisterConverter<bool>(new FirebirdBoolConverter());
            base.RegisterConverter<string>(new FirebirdStringConverter());
            base.RegisterConverter<char[]>(new FirebirdCharArrayConverter());
            base.RegisterConverter<byte[]>(new FirebirdByteArrayConverter());

            base.RegisterConverter<float>(new FirebirdFloatConverter());
            base.RegisterConverter<double>(new FirebirdDoubleConverter());
            base.RegisterConverter<decimal>(new FirebirdDecimalConverter());

            this.Variables = new Dictionary<string, string>
            {
                { OrmLiteVariables.SystemUtc, "CURRENT_TIMESTAMP" },
            };
        }

        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            if (options != null)
            {
                foreach (var option in options)
                {
                    connectionString += option.Key + "=" + option.Value + ";";
                }
            }

            return new FbConnection(connectionString);
        }

        public override long GetLastInsertId(IDbCommand dbCmd)
        {
            return LastInsertId;
        }

        public override long InsertAndGetLastInsertId<T>(IDbCommand dbCmd)
        {
            dbCmd.CommandText += " returning {0};".Fmt(typeof(T).GetModelMetadata().PrimaryKey.FieldName);

            return dbCmd.ExecLongScalar();
        }

        public override string ToRowCountStatement(string innerSql)
        {
            return "SELECT COUNT(*) FROM ({0})".Fmt(innerSql);
        }

        public override string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            const string SelectStatement = "SELECT";
            var modelDef = GetModel(tableType);
            sqlFilter = (sqlFilter ?? "").TrimStart();
            var isFullSelectStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.Length > SelectStatement.Length
                && sqlFilter.StartsWithIgnoreCase(SelectStatement);

            if (isFullSelectStatement)
                return sqlFilter.SqlFmt(filterParams);

            var sql = StringBuilderCache.Allocate()
                .AppendFormat("SELECT {0} FROM {1}",
                    GetColumnNames(modelDef),
                    GetQuotedTableName(modelDef));

            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFmt(filterParams);
                if (!sqlFilter.StartsWith("\nORDER ", StringComparison.OrdinalIgnoreCase)
                    && !sqlFilter.StartsWith("\nROWS ", StringComparison.OrdinalIgnoreCase)) // ROWS <m> [TO <n>])
                {
                    sql.Append("\nWHERE ");
                }
                sql.Append(sqlFilter);
            }

            return StringBuilderCache.ReturnAndFree(sql);
        }

        public override string ToInsertRowStatement(IDbCommand cmd, object objWithProperties, ICollection<string> insertFields = null)
        {
            if (insertFields == null)
                insertFields = new List<string>();

            var sbColumnNames = StringBuilderCache.Allocate();
            var sbColumnValues = StringBuilderCacheAlt.Allocate();

            var tableType = objWithProperties.GetType();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitionsArray)
            {

                if (fieldDef.IsComputed)
                    continue;
                if (insertFields.Count > 0 && !insertFields.Contains(fieldDef.Name, StringComparer.OrdinalIgnoreCase))
                    continue;

                if ((fieldDef.AutoIncrement || !string.IsNullOrEmpty(fieldDef.Sequence)
                    || fieldDef.Name == OrmLiteConfig.IdField)
                    && cmd != null)
                {
                    EnsureAutoIncrementSequence(modelDef, fieldDef);

                    var result = GetNextValue(cmd, fieldDef.Sequence, fieldDef.GetValue(objWithProperties));

                    var fieldValue = this.ConvertNumber(fieldDef.FieldType, result);
                    fieldDef.SetValueFn(objWithProperties, fieldValue);
                }

                if (sbColumnNames.Length > 0)
                    sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0)
                    sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));
                    sbColumnValues.Append(this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName)));

                    var p = AddParameter(cmd, fieldDef);
                    p.Value = fieldDef.GetValue(objWithProperties) ?? DBNull.Value;
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToInsertRowStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2});",
                GetQuotedTableName(modelDef), 
                StringBuilderCache.ReturnAndFree(sbColumnNames), 
                StringBuilderCacheAlt.ReturnAndFree(sbColumnValues));

            return sql;
        }

        private void EnsureAutoIncrementSequence(ModelDefinition modelDef, FieldDefinition fieldDef)
        {
            if (fieldDef.AutoIncrement && string.IsNullOrEmpty(fieldDef.Sequence))
            {
                fieldDef.Sequence = Sequence(modelDef.ModelName, fieldDef.FieldName, fieldDef.Sequence);
            }
        }

        public override void PrepareParameterizedInsertStatement<T>(IDbCommand cmd, ICollection<string> insertFields = null)
        {
            var sbColumnNames = StringBuilderCache.Allocate();
            var sbColumnValues = StringBuilderCacheAlt.Allocate();
            var modelDef = OrmLiteUtils.GetModelDefinition(typeof(T));

            cmd.Parameters.Clear();
            cmd.CommandTimeout = OrmLiteConfig.CommandTimeout;

            foreach (var fieldDef in modelDef.FieldDefinitionsArray)
            {
                if (fieldDef.ShouldSkipInsert() && !fieldDef.AutoIncrement)
                    continue;

                //insertFields contains Property "Name" of fields to insert ( that's how expressions work )
                if (insertFields != null && !insertFields.Contains(fieldDef.Name, StringComparer.OrdinalIgnoreCase))
                    continue;

                if (sbColumnNames.Length > 0)
                    sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0)
                    sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));

                    if (!fieldDef.AutoIncrement)
                    {
                        sbColumnValues.Append(this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName)));
                        AddParameter(cmd, fieldDef);
                    }
                    else
                    {
                        EnsureAutoIncrementSequence(modelDef, fieldDef);
                        sbColumnValues.Append("NEXT VALUE FOR " + fieldDef.Sequence);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in PrepareParameterizedInsertStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            cmd.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                GetQuotedTableName(modelDef), 
                StringBuilderCache.ReturnAndFree(sbColumnNames), 
                StringBuilderCacheAlt.ReturnAndFree(sbColumnValues));
        }

        public override void PrepareUpdateRowStatement(IDbCommand dbCmd, object objWithProperties, ICollection<string> updateFields = null)
        {
            if (updateFields == null)
                updateFields = new List<string>();

            var sql = StringBuilderCache.Allocate();
            var sqlFilter = StringBuilderCacheAlt.Allocate();
            var tableType = objWithProperties.GetType();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsComputed)
                    continue;

                if ((fieldDef.IsPrimaryKey || fieldDef.Name == OrmLiteConfig.IdField)
                    && updateFields.Count == 0)
                {
                    if (sqlFilter.Length > 0)
                        sqlFilter.Append(" AND ");

                    sqlFilter
                        .Append(GetQuotedColumnName(fieldDef.FieldName))
                        .Append("=")
                        .Append(this.AddParam(dbCmd, fieldDef.GetValue(objWithProperties), fieldDef).ParameterName);

                    continue;
                }

                if (updateFields.Count > 0 && !updateFields.Contains(fieldDef.Name, StringComparer.OrdinalIgnoreCase))
                    continue;

                if (sql.Length > 0)
                    sql.Append(",");

                sql
                    .Append(GetQuotedColumnName(fieldDef.FieldName))
                    .Append("=")
                    .Append(this.AddParam(dbCmd, fieldDef.GetValue(objWithProperties), fieldDef).ParameterName);
            }

            var strFilter = StringBuilderCacheAlt.ReturnAndFree(sqlFilter);
            dbCmd.CommandText = string.Format("UPDATE {0} \nSET {1} {2}",
                GetQuotedTableName(modelDef), 
                StringBuilderCache.ReturnAndFree(sql),
                strFilter.Length > 0 ? "\nWHERE " + strFilter : "");
        }

        public override string ToCreateTableStatement(Type tableType)
        {
            var sbColumns = StringBuilderCache.Allocate();
            var sbConstraints = StringBuilderCacheAlt.Allocate();

            var sbPk = new StringBuilder();

            var modelDef = GetModel(tableType);
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.CustomSelect != null)
                    continue;

                if (fieldDef.IsPrimaryKey)
                    sbPk.AppendFormat(sbPk.Length != 0 ? ",{0}" : "{0}", GetQuotedColumnName(fieldDef.FieldName));

                if (sbColumns.Length != 0)
                    sbColumns.Append(", \n  ");

                var columnDefinition = GetColumnDefinition(fieldDef);
                sbColumns.Append(columnDefinition);

                if (fieldDef.ForeignKey == null || OrmLiteConfig.SkipForeignKeys)
                    continue;

                var refModelDef = GetModel(fieldDef.ForeignKey.ReferenceType);

                var fkName = NamingStrategy.ApplyNameRestrictions(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy, fieldDef)).ToLower();
                sbConstraints.AppendFormat(", \n\n  CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3})",
                    GetQuotedName(fkName),
                    GetQuotedColumnName(fieldDef.FieldName),
                    GetQuotedTableName(refModelDef),
                    GetQuotedColumnName(refModelDef.PrimaryKey.FieldName));

                sbConstraints.Append(GetForeignKeyOnDeleteClause(fieldDef.ForeignKey));
                sbConstraints.Append(GetForeignKeyOnUpdateClause(fieldDef.ForeignKey));
            }

            if (sbPk.Length != 0)
                sbColumns.AppendFormat(", \n  PRIMARY KEY({0})", sbPk);

            var sql = string.Format(
                "CREATE TABLE {0} \n(\n  {1}{2} \n); \n",
                GetQuotedTableName(modelDef),
                StringBuilderCache.ReturnAndFree(sbColumns),
                StringBuilderCacheAlt.ReturnAndFree(sbConstraints));

            return sql;
        }

        public override List<string> ToCreateSequenceStatements(Type tableType)
        {
            var gens = new List<string>();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.AutoIncrement || !fieldDef.Sequence.IsNullOrEmpty())
                {
                    var sequence = Sequence(modelDef.ModelName, fieldDef.FieldName, fieldDef.Sequence);
                    gens.Add("CREATE GENERATOR {0};".Fmt(sequence));
                    gens.Add("SET GENERATOR {0} TO 0;".Fmt(sequence));
                }
            }
            return gens;
        }

        public override string GetColumnDefinition(FieldDefinition fieldDef)
        {
            var fieldDefinition = fieldDef.CustomFieldDefinition 
                ?? GetColumnTypeDefinition(fieldDef.ColumnType, fieldDef.FieldLength, fieldDef.Scale);

            var sql = StringBuilderCache.Allocate();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldDef.FieldName), fieldDefinition);

            var defaultValue = GetDefaultValue(fieldDef);
            if (fieldDef.IsRowVersion)
            {
                sql.AppendFormat(DefaultValueFormat, 1L);
            }
            else if (!string.IsNullOrEmpty(defaultValue))
            {
                sql.AppendFormat(DefaultValueFormat, defaultValue);
            }

            if (!fieldDef.IsNullable)
            {
                sql.Append(" NOT NULL");
            }

            return StringBuilderCacheAlt.ReturnAndFree(sql);
        }

        public override List<string> ToCreateIndexStatements(Type tableType)
        {
            var sqlIndexes = new List<string>();

            var modelDef = GetModel(tableType);
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!fieldDef.IsIndexed) continue;

                var indexName = GetIndexName(
                    fieldDef.IsUnique, modelDef.ModelName, fieldDef.FieldName);

                sqlIndexes.Add(
                    ToCreateIndexStatement(fieldDef.IsUnique, indexName, modelDef, fieldDef.FieldName, false));
            }

            foreach (var compositeIndex in modelDef.CompositeIndexes)
            {
                var indexName = GetCompositeIndexName(compositeIndex, modelDef);
                var indexNames = string.Join(",", compositeIndex.FieldNames.ToArray());

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef, indexNames, false));
            }

            return sqlIndexes;
        }

        protected override string GetIndexName(bool isUnique, string modelName, string fieldName)
        {
            return NamingStrategy.ApplyNameRestrictions(
                string.Format("{0}idx_{1}_{2}", isUnique ? "u" : "", modelName, fieldName).ToLower());
        }

        protected override string ToCreateIndexStatement(bool isUnique, string indexName, ModelDefinition modelDef, string fieldName,
            bool isCombined = false, FieldDefinition fieldDef = null)
        {
            var fieldNames = fieldName.Split(',')
                .Map(x => NamingStrategy.GetColumnName(x.LeftPart(' ')));

            return string.Format("CREATE {0} INDEX {1} ON {2} ({3}); \n",
                isUnique ? "UNIQUE" : "",
                indexName,
                GetQuotedTableName(modelDef),
                string.Join(",", fieldNames.ToArray()));
        }

        public static string RowVersionTriggerFormat = "{0}RowVersionUpdateTrigger";
        public override string ToPostCreateTableStatement(ModelDefinition modelDef)
        {
            if (modelDef.RowVersion != null)
            {
                var triggerName = NamingStrategy.ApplyNameRestrictions(
                    RowVersionTriggerFormat.Fmt(modelDef.ModelName));
                var triggerBody = "new.{0} = old.{0}+1;".Fmt(
                    modelDef.RowVersion.FieldName.SqlColumn(this));

                var sql = "CREATE OR ALTER TRIGGER {0} BEFORE UPDATE ON {1} AS BEGIN {2} END;".Fmt(
                    Quote(triggerName), 
                    GetTableName(modelDef.ModelName, modelDef.Schema), 
                    triggerBody);

                return sql;
            }

            return null;
        }

        public override string ToExistStatement(Type fromTableType,
            object objWithProperties,
            string sqlFilter,
            params object[] filterParams)
        {

            var fromModelDef = GetModel(fromTableType);
            var sql = StringBuilderCache.Allocate();
            sql.AppendFormat("SELECT 1 \nFROM {0}", GetQuotedTableName(fromModelDef));

            var filter = StringBuilderCacheAlt.Allocate();
            var hasFilter = false;

            if (objWithProperties != null)
            {
                var tableType = objWithProperties.GetType();

                if (fromTableType != tableType)
                {
                    int i = 0;
                    var fpk = new List<FieldDefinition>();
                    var modelDef = GetModel(tableType);

                    foreach (var def in modelDef.FieldDefinitions)
                    {
                        if (def.IsPrimaryKey) fpk.Add(def);
                    }

                    foreach (var fieldDef in fromModelDef.FieldDefinitions)
                    {
                        if (fieldDef.IsComputed)
                            continue;

                        if (fieldDef.ForeignKey != null
                            && GetModel(fieldDef.ForeignKey.ReferenceType).ModelName == modelDef.ModelName)
                        {
                            if (filter.Length > 0)
                                filter.Append(" AND ");

                            filter.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName),
                                fpk[i].GetQuotedValue(objWithProperties));
                            i++;
                        }
                    }

                }
                else
                {
                    var modelDef = GetModel(tableType);
                    foreach (var fieldDef in modelDef.FieldDefinitions)
                    {
                        if (fieldDef.IsComputed)
                            continue;

                        if (fieldDef.IsPrimaryKey)
                        {
                            if (filter.Length > 0) filter.Append(" AND ");
                            filter.AppendFormat("{0} = {1}",
                                GetQuotedColumnName(fieldDef.FieldName),
                                fieldDef.GetQuotedValue(objWithProperties));
                        }
                    }
                }

                hasFilter = filter.Length > 0;
                if (hasFilter)
                    sql.AppendFormat("\nWHERE {0} ", StringBuilderCacheAlt.ReturnAndFree(filter));
            }

            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFmt(filterParams);
                if (!sqlFilter.StartsWith("\nORDER ", StringComparison.OrdinalIgnoreCase)
                    && !sqlFilter.StartsWith("\nROWS ", StringComparison.OrdinalIgnoreCase)) // ROWS <m> [TO <n>])
                {
                    sql.Append(hasFilter ? " AND  " : "\nWHERE ");
                }
                sql.Append(sqlFilter);
            }

            var sb = StringBuilderCacheAlt.Allocate()
                .Append("select 1 from RDB$DATABASE where")
                .AppendFormat(" exists ({0})", StringBuilderCache.ReturnAndFree(sql));

            return StringBuilderCacheAlt.ReturnAndFree(sb);
        }

        public override string ToSelectFromProcedureStatement(
            object fromObjWithProperties,
            Type outputModelType,
            string sqlFilter,
            params object[] filterParams)
        {

            var sbColumnValues = StringBuilderCache.Allocate();

            Type fromTableType = fromObjWithProperties.GetType();

            var modelDef = GetModel(fromTableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumnValues.Length > 0)
                    sbColumnValues.Append(",");

                sbColumnValues.Append(fieldDef.GetQuotedValue(fromObjWithProperties));
            }

            var columnVaues = StringBuilderCache.ReturnAndFree(sbColumnValues);
            var sql = StringBuilderCache.Allocate();
            sql.AppendFormat("SELECT {0} \nFROM {1} {2}{3}{4} \n",
                GetColumnNames(GetModel(outputModelType)),
                GetQuotedTableName(modelDef),
                columnVaues.Length > 0 ? "(" : "",
                columnVaues,
                columnVaues.Length > 0 ? ")" : "");

            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFmt(filterParams);
                if (!sqlFilter.StartsWith("\nORDER ", StringComparison.OrdinalIgnoreCase)
                    && !sqlFilter.StartsWith("\nROWS ", StringComparison.OrdinalIgnoreCase)) // ROWS <m> [TO <n>]
                {
                    sql.Append("\nWHERE ");
                }
                sql.Append(sqlFilter);
            }

            return StringBuilderCache.ReturnAndFree(sql);
        }

        public override string ToExecuteProcedureStatement(object objWithProperties)
        {
            var sbColumnValues = StringBuilderCache.Allocate();

            var tableType = objWithProperties.GetType();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumnValues.Length > 0)
                    sbColumnValues.Append(",");

                sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties));
            }

            var columnValues = StringBuilderCache.ReturnAndFree(sbColumnValues);
            var sql = string.Format("EXECUTE PROCEDURE {0} {1}{2}{3};",
                GetQuotedTableName(modelDef),
                columnValues.Length > 0 ? "(" : "",
                columnValues,
                columnValues.Length > 0 ? ")" : "");

            return sql;
        }

        private object GetNextValue(IDbCommand dbCmd, string sequence, object value)
        {
            Object retObj;

            if (value.ToString() != "0")
            {
                long nv;
                if (long.TryParse(value.ToString(), out nv))
                {
                    LastInsertId = nv;
                    retObj = LastInsertId;
                }
                else
                {
                    LastInsertId = 0;
                    retObj = value;
                }
                return retObj;

            }

            dbCmd.CommandText = string.Format("select next value for {0} from RDB$DATABASE", Quote(sequence));
            long result = (long)dbCmd.ExecuteScalar();
            LastInsertId = result;
            return result;
        }

        public bool QuoteNames { get; set; }

        private string Quote(string name)
        {
            return QuoteNames
                ? string.Format("\"{0}\"", name)
                : RESERVED.Contains(name.ToUpper())
                    ? string.Format("\"{0}\"", name)
                    : name;
        }

        public override string EscapeWildcards(string value)
        {
            if (value == null)
                return null;

            return value
                .Replace("^", @"^^")
                .Replace("_", @"^_")
                .Replace("%", @"^%");
        }

        public override string GetQuotedName(string fieldName)
        {
            return Quote(fieldName);
        }

        public virtual string GetTableName(ModelDefinition modelDef)
        {
            return GetTableName(modelDef.ModelName, modelDef.Schema);
        }

        public override string GetTableName(string table, string schema = null)
        {
            return schema != null
                ? string.Format("{0}_{1}",
                    NamingStrategy.GetSchemaName(schema),
                    NamingStrategy.GetTableName(table))
                : NamingStrategy.GetTableName(table);
        }

        public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            if (!modelDef.IsInSchema)
                return Quote(NamingStrategy.GetTableName(modelDef.ModelName));

            return Quote(GetTableName(modelDef.ModelName, modelDef.Schema));
        }

        public override string GetQuotedColumnName(string fieldName)
        {
            return Quote(NamingStrategy.GetColumnName(fieldName));
        }

        private string Sequence(string modelName, string fieldName, string sequence)
        {
            return sequence.IsNullOrEmpty()
                ? Quote(NamingStrategy.GetSequenceName(modelName, fieldName))
                : Quote(sequence);
        }

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new FirebirdSqlExpression<T>(this);
        }

        public override IDbDataParameter CreateParam()
        {
            return new FbParameter();
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName, string schema = null)
        {
            tableName = GetTableName(tableName, schema);

            if (!QuoteNames & !RESERVED.Contains(tableName.ToUpper()))
                tableName = tableName.ToUpper();

            var sql = "SELECT count(*) FROM rdb$relations " +
                "WHERE rdb$system_flag = 0 AND rdb$view_blr IS NULL AND rdb$relation_name ={0}"
                .SqlFmt(tableName);

            var result = dbCmd.ExecLongScalar(sql);
            return result > 0;
        }

        public override bool DoesColumnExist(IDbConnection db, string columnName, string tableName, string schema = null)
        {
            var table = GetTableName(tableName, schema);

            if (!QuoteNames & !RESERVED.Contains(tableName.ToUpper()))
                table = table.ToUpper();

            var sql = "SELECT COUNT(*) FROM RDB$RELATION_FIELDS "
                    + " WHERE RDB$RELATION_FIELDS.RDB$RELATION_NAME = UPPER(@table)"
                    + "   AND RDB$RELATION_FIELDS.RDB$FIELD_NAME = UPPER(@columnName)";

            var result = db.SqlScalar<long>(sql, new { table, columnName });

            return result > 0;
        }

        public override string GetForeignKeyOnDeleteClause(ForeignKeyConstraint foreignKey)
        {
            return (!string.IsNullOrEmpty(foreignKey.OnDelete) && foreignKey.OnDelete.ToUpper() != "RESTRICT") ? " ON DELETE " + foreignKey.OnDelete : "";
        }

        public override string GetForeignKeyOnUpdateClause(ForeignKeyConstraint foreignKey)
        {
            return (!string.IsNullOrEmpty(foreignKey.OnUpdate) && foreignKey.OnUpdate.ToUpper() != "RESTRICT") ? " ON UPDATE " + foreignKey.OnUpdate : "";
        }

        #region DDL
        public override string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            var column = GetColumnDefinition(fieldDef);
            return $"ALTER TABLE {GetQuotedTableName(GetModel(modelType))} ADD {column} ;";
        }

        public override string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            var column = GetColumnDefinition(fieldDef);
            return $"ALTER TABLE {GetQuotedTableName(GetModel(modelType))} ALTER {column} ;";
        }

        public override string ToChangeColumnNameStatement(Type modelType, FieldDefinition fieldDef, string oldColumnName)
        {
            return string.Format("ALTER TABLE {0} ALTER {1} TO {2};",
                GetQuotedTableName(GetModel(modelType)),
                GetQuotedColumnName(oldColumnName),
                GetQuotedColumnName(fieldDef.FieldName));
        }
        #endregion DDL

        public override string ToSelectStatement(ModelDefinition modelDef,
            string selectExpression,
            string bodyExpression,
            string orderByExpression = null,
            int? offset = null,
            int? rows = null)
        {

            var sb = StringBuilderCache.Allocate()
                .Append(selectExpression)
                .Append(bodyExpression);

            if (orderByExpression != null)
                sb.Append(orderByExpression);

            if (rows != null || offset != null)
            {
                var sqlPrefix = "SELECT";
                if (rows != null)
                    sqlPrefix += " FIRST " + rows;
                if (offset != null)
                    sqlPrefix += " SKIP " + offset;

                var sql = StringBuilderCache.ReturnAndFree(sb);
                return sqlPrefix + sql.Substring("SELECT".Length);
            }

            return StringBuilderCache.ReturnAndFree(sb);
        }

        public override void DropColumn(IDbConnection db, Type modelType, string columnName)
        {
            var provider = db.GetDialectProvider();
            var command = string.Format("ALTER TABLE {0} DROP {1};",
                provider.GetQuotedTableName(modelType.GetModelMetadata().ModelName),
                provider.GetQuotedColumnName(columnName));

            db.ExecuteSql(command);
        }

        public override string SqlConcat(IEnumerable<object> args) => string.Join(" || ", args);
    }
}

/*
DEBUG: Ignoring existing generator 'CREATE GENERATOR ModelWFDT_Id_GEN;': unsuccessful metadata update
DEFINE GENERATOR failed
attempt to store duplicate value (visible to active transactions) in unique index "RDB$INDEX_11" 
*/
