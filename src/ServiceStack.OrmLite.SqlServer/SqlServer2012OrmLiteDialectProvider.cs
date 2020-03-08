using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServer2012OrmLiteDialectProvider : SqlServerOrmLiteDialectProvider
    {
        public new static SqlServer2012OrmLiteDialectProvider Instance = new SqlServer2012OrmLiteDialectProvider();

        public override bool DoesSequenceExist(IDbCommand dbCmd, string sequenceName)
        {
            var sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.sequences WHERE object_id=object_id({0})) THEN 1 ELSE 0 END"
                .SqlFmt(this, sequenceName);

            dbCmd.CommandText = sql;
            var result = dbCmd.ExecLongScalar();

            return result == 1;
        }

        protected override string GetAutoIncrementDefinition(FieldDefinition fieldDef)
        {
            if (!string.IsNullOrEmpty(fieldDef.Sequence))
                return $"DEFAULT NEXT VALUE FOR {Sequence(NamingStrategy.GetSchemaName(GetModel(fieldDef.PropertyInfo?.ReflectedType)), fieldDef.Sequence)}";
            else
                return AutoIncrementDefinition;
        }

        protected override bool ShouldSkipInsert(FieldDefinition fieldDef) => 
            fieldDef.ShouldSkipInsert() && string.IsNullOrEmpty(fieldDef.Sequence);

        protected override bool SupportsSequences(FieldDefinition fieldDef) => 
            !string.IsNullOrEmpty(fieldDef.Sequence);
        
        public override List<string> ToCreateSequenceStatements(Type tableType)
        {
            var modelDef = GetModel(tableType);
            return SequenceList(tableType).Select(seq => $"CREATE SEQUENCE {Sequence(NamingStrategy.GetSchemaName(modelDef), seq)} AS BIGINT START WITH 1 INCREMENT BY 1 NO CACHE;").ToList();
        }

        public override string ToCreateSequenceStatement(Type tableType, string sequenceName)
        {
            var modelDef = GetModel(tableType);
            return $"CREATE SEQUENCE {Sequence(NamingStrategy.GetSchemaName(modelDef), sequenceName)} AS BIGINT START WITH 1 INCREMENT BY 1 NO CACHE;";
        }

        public override List<string> SequenceList(Type tableType)
        {
            var gens = new List<string>();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!string.IsNullOrEmpty(fieldDef.Sequence))
                {
                    gens.AddIfNotExists(fieldDef.Sequence);
                }
            }
            return gens;
        }

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

            if (offset != null || rows != null)
            {
                if (orderByExpression.IsEmpty())
                {
                    var orderBy = offset == null && rows == 1 //Avoid for Single requests
                        ? "1"
                        : this.GetQuotedColumnName(modelDef, modelDef.PrimaryKey);

                    sb.Append(" ORDER BY " + orderBy);
                }

                sb.Append(" ").Append(SqlLimit(offset, rows));
            }

            return StringBuilderCache.ReturnAndFree(sb);
        }

        public override string GetColumnDefinition(FieldDefinition fieldDef)
        {
            // https://msdn.microsoft.com/en-us/library/ms182776.aspx
            if (fieldDef.IsRowVersion)
                return $"{fieldDef.FieldName} rowversion NOT NULL";

            var fieldDefinition = ResolveFragment(fieldDef.CustomFieldDefinition) ??
                GetColumnTypeDefinition(fieldDef.ColumnType, fieldDef.FieldLength, fieldDef.Scale);

            var sql = StringBuilderCache.Allocate();
            sql.Append($"{GetQuotedColumnName(fieldDef.FieldName)} {fieldDefinition}");

            if (fieldDef.FieldType == typeof(string))
            {
                // https://msdn.microsoft.com/en-us/library/ms184391.aspx
                var collation = fieldDef.PropertyInfo?.FirstAttribute<SqlServerCollateAttribute>()?.Collation;
                if (!string.IsNullOrEmpty(collation))
                {
                    sql.Append($" COLLATE {collation}");
                }
            }

            if (fieldDef.IsPrimaryKey)
            {
                sql.Append(" PRIMARY KEY");

                if (fieldDef.IsNonClustered)
                    sql.Append(" NONCLUSTERED");
 
                if (fieldDef.AutoIncrement)
                {
                    sql.Append(" ").Append(AutoIncrementDefinition);
                }
            }
            else
            {
                sql.Append(fieldDef.IsNullable ? " NULL" : " NOT NULL");
            }

            if (fieldDef.IsUniqueConstraint)
            {
                sql.Append(" UNIQUE");
            }

            var defaultValue = GetDefaultValue(fieldDef);
            if (!string.IsNullOrEmpty(defaultValue))
            {
                sql.AppendFormat(DefaultValueFormat, defaultValue);
            }

            return StringBuilderCache.ReturnAndFree(sql);
        }

        public override string ToCreateTableStatement(Type tableType)
        {
            var sbColumns = StringBuilderCache.Allocate();
            var sbConstraints = StringBuilderCacheAlt.Allocate();
            var sbTableOptions = StringBuilderCacheAlt.Allocate();

            var fileTableAttrib = tableType.FirstAttribute<SqlServerFileTableAttribute>();

            var modelDef = GetModel(tableType);

            if (fileTableAttrib == null)
            {
                foreach (var fieldDef in modelDef.FieldDefinitions)
                {
                    if (fieldDef.CustomSelect != null || (fieldDef.IsComputed && !fieldDef.IsPersisted))
                        continue;

                    var columnDefinition = GetColumnDefinition(fieldDef);

                    if (columnDefinition == null)
                        continue;

                    if (sbColumns.Length != 0)
                        sbColumns.Append(", \n  ");

                    sbColumns.Append(columnDefinition);

                    var sqlConstraint = GetCheckConstraint(modelDef, fieldDef);
                    if (sqlConstraint != null)
                    {
                        sbConstraints.Append(",\n" + sqlConstraint);
                    }

                    if (fieldDef.ForeignKey == null || OrmLiteConfig.SkipForeignKeys)
                        continue;

                    var refModelDef = GetModel(fieldDef.ForeignKey.ReferenceType);
                    sbConstraints.Append(
                        $", \n\n  CONSTRAINT {GetQuotedName(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy, fieldDef))} " +
                        $"FOREIGN KEY ({GetQuotedColumnName(fieldDef.FieldName)}) " +
                        $"REFERENCES {GetQuotedTableName(refModelDef)} ({GetQuotedColumnName(refModelDef.PrimaryKey.FieldName)})");

                    sbConstraints.Append(GetForeignKeyOnDeleteClause(fieldDef.ForeignKey));
                    sbConstraints.Append(GetForeignKeyOnUpdateClause(fieldDef.ForeignKey));
                }
            }
            else
            {
                if (fileTableAttrib.FileTableDirectory != null || fileTableAttrib.FileTableCollateFileName != null)
                {
                    sbTableOptions.Append(" WITH (");

                    if (fileTableAttrib.FileTableDirectory != null)
                    {
                        sbTableOptions.Append($" FILETABLE_DIRECTORY = N'{fileTableAttrib.FileTableDirectory}'\n");
                    }

                    if (fileTableAttrib.FileTableCollateFileName != null)
                    {
                        if (fileTableAttrib.FileTableDirectory != null)
                            sbTableOptions.Append(" ,");

                        sbTableOptions.Append($" FILETABLE_COLLATE_FILENAME = {fileTableAttrib.FileTableCollateFileName ?? "database_default" }\n");
                    }
                    sbTableOptions.Append(")");
                }
            }

            var uniqueConstraints = GetUniqueConstraints(modelDef);
            if (uniqueConstraints != null)
            {
                sbConstraints.Append(",\n" + uniqueConstraints);
            }

            var sql = $"CREATE TABLE {GetQuotedTableName(modelDef)} ";
            sql += (fileTableAttrib != null)
                ? $"\n AS FILETABLE{StringBuilderCache.ReturnAndFree(sbTableOptions)};"
                : $"\n(\n  {StringBuilderCache.ReturnAndFree(sbColumns)}{StringBuilderCacheAlt.ReturnAndFree(sbConstraints)} \n){StringBuilderCache.ReturnAndFree(sbTableOptions)}; \n";

            return sql;
        }

        public override void AppendFieldCondition(StringBuilder sqlFilter, FieldDefinition fieldDef, IDbCommand cmd)
        {
            if (isSpatialField(fieldDef))
            {
                // Append condition statement to determine if SqlGeometry or SqlGeography type is Equal
                // using the type's STEquals method
                //
                // SqlGeometry: https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.types.sqlgeometry.stequals.aspx
                // SqlGeography: https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.types.sqlgeography.stequals.aspx
                sqlFilter
                    .Append(GetQuotedColumnName(fieldDef.FieldName))
                    .Append(".STEquals(")
                    .Append(this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName)))
                    .Append(") = 1");
 
                AddParameter(cmd, fieldDef);
            }
            else 
            {
                base.AppendFieldCondition(sqlFilter, fieldDef, cmd);
            }
        }

        public override void AppendNullFieldCondition(StringBuilder sqlFilter, FieldDefinition fieldDef)
        {
            if (hasIsNullProperty(fieldDef))
            {
                // Append condition statement to determine if SqlHierarchyId, SqlGeometry, or SqlGeography type is NULL
                // using the type's IsNull property
                //
                // SqlHierarchyId: https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.types.sqlhierarchyid.isnull.aspx
                // SqlGeometry: https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.types.sqlgeometry.isnull.aspx
                // SqlGeography: https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.types.sqlgeography.isnull.aspx
                sqlFilter
                    .Append(GetQuotedColumnName(fieldDef.FieldName))
                    .Append(".IsNull = 1");
            }
            else 
            {
                base.AppendNullFieldCondition(sqlFilter, fieldDef);
            }
        }

        internal bool isSpatialField(FieldDefinition fieldDef) => 
            fieldDef.FieldType.Name == "SqlGeography" || fieldDef.FieldType.Name == "SqlGeometry";

        internal bool hasIsNullProperty(FieldDefinition fieldDef) =>
            isSpatialField(fieldDef) || fieldDef.FieldType.Name == "SqlHierarchyId";
    }
}
