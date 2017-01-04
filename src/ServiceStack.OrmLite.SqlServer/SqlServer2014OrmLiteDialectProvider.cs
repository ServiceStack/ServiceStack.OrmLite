﻿using System;
using ServiceStack.Text;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServer2014OrmLiteDialectProvider : SqlServer2012OrmLiteDialectProvider
    {
        public new static SqlServer2014OrmLiteDialectProvider Instance = new SqlServer2014OrmLiteDialectProvider();

        public override string GetColumnDefinition(FieldDefinition fieldDef)
        {
            // https://msdn.microsoft.com/en-us/library/ms182776.aspx
            if (fieldDef.IsRowVersion)
                return $"{fieldDef.FieldName} rowversion NOT NULL";

            var fieldDefinition = fieldDef.CustomFieldDefinition ??
                GetColumnTypeDefinition(fieldDef.ColumnType, fieldDef.FieldLength, fieldDef.Scale);            

            var sql = StringBuilderCache.Allocate();
            sql.Append($"{GetQuotedColumnName(fieldDef.FieldName)} {fieldDefinition}");

            if (fieldDef.IsPrimaryKey)
            {
                var isMemoryTable = fieldDef.PropertyInfo.DeclaringType.FirstAttribute<SqlServerMemoryOptimizedAttribute>() != null;

                if (isMemoryTable)
                {
                    sql.Append(fieldDef.IsNullable ? " NULL" : " NOT NULL");                    
                    sql.Append(" PRIMARY KEY NONCLUSTERED");

                    var bucketCount = fieldDef.PropertyInfo.FirstAttribute<SqlServerBucketCountAttribute>()?.Count;
                    if (bucketCount.HasValue)
                    {
                        sql.Append($" HASH WITH (BUCKET_COUNT = {bucketCount.Value})");
                    }
                }
                else
                {
                    sql.Append(" PRIMARY KEY");
                }

                if (fieldDef.AutoIncrement)
                {
                    sql.Append(" ").Append(AutoIncrementDefinition);
                }
            }
            else
            {
                sql.Append(fieldDef.IsNullable ? " NULL" : " NOT NULL");
            }

            // https://msdn.microsoft.com/en-us/library/ms184391.aspx
            var collation = fieldDef.PropertyInfo.FirstAttribute<SqlServerCollateAttribute>()?.Collation;
            if (!string.IsNullOrEmpty(collation))
            {
                sql.Append($" COLLATE {collation}");
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
            var memoryTableAttrib = tableType.FirstAttribute<SqlServerMemoryOptimizedAttribute>();

            var modelDef = GetModel(tableType);

            if (fileTableAttrib == null)
            {
                foreach (var fieldDef in modelDef.FieldDefinitions)
                {
                    if (fieldDef.CustomSelect != null)
                        continue;

                    var columnDefinition = GetColumnDefinition(fieldDef);
                    if (columnDefinition == null)
                        continue;

                    if (sbColumns.Length != 0)
                        sbColumns.Append(", \n  ");

                    sbColumns.Append(columnDefinition);

                    if (fieldDef.ForeignKey == null || OrmLiteConfig.SkipForeignKeys)
                        continue;

                    var refModelDef = OrmLiteUtils.GetModelDefinition(fieldDef.ForeignKey.ReferenceType);
                    sbConstraints.Append(
                        $", \n\n  CONSTRAINT {GetQuotedName(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy, fieldDef))} " +
                        $"FOREIGN KEY ({GetQuotedColumnName(fieldDef.FieldName)}) " +
                        $"REFERENCES {GetQuotedTableName(refModelDef)} ({GetQuotedColumnName(refModelDef.PrimaryKey.FieldName)})");

                    sbConstraints.Append(GetForeignKeyOnDeleteClause(fieldDef.ForeignKey));
                    sbConstraints.Append(GetForeignKeyOnUpdateClause(fieldDef.ForeignKey));
                }

                if (memoryTableAttrib != null)
                {
                    var attrib = tableType.FirstAttribute<SqlServerMemoryOptimizedAttribute>();
                    sbTableOptions.Append(" WITH (MEMORY_OPTIMIZED = ON");
                    if (attrib.Durability == SqlServerDurability.SchemaOnly)
                        sbTableOptions.Append(", DURABILITY = SCHEMA_ONLY");
                    else if (attrib.Durability == SqlServerDurability.SchemaAndData)
                        sbTableOptions.Append(", DURABILITY = SCHEMA_AND_DATA");
                    sbTableOptions.Append(")");
                }
            }
            else
            {
                var hasFileTableDir = !string.IsNullOrEmpty(fileTableAttrib.FileTableDirectory);
                var hasFileTableCollateFileName = !string.IsNullOrEmpty(fileTableAttrib.FileTableCollateFileName);

                if (hasFileTableDir || hasFileTableCollateFileName)
                {
                    sbTableOptions.Append(" WITH (");

                    if (hasFileTableDir)
                    {
                        sbTableOptions.Append($" FILETABLE_DIRECTORY = N'{fileTableAttrib.FileTableDirectory}'\n");
                    }

                    if (hasFileTableCollateFileName)
                    {
                        if (hasFileTableDir) sbTableOptions.Append(" ,");
                        sbTableOptions.Append($" FILETABLE_COLLATE_FILENAME = {fileTableAttrib.FileTableCollateFileName ?? "database_default" }\n");
                    }
                    sbTableOptions.Append(")");
                }
            }

            var sql = $"CREATE TABLE {GetQuotedTableName(modelDef)} ";
            sql += (fileTableAttrib != null)
                ? $"\n AS FILETABLE{StringBuilderCache.ReturnAndFree(sbTableOptions)};"
                : $"\n(\n  {StringBuilderCache.ReturnAndFree(sbColumns)}{StringBuilderCacheAlt.ReturnAndFree(sbConstraints)} \n){StringBuilderCache.ReturnAndFree(sbTableOptions)}; \n";

            return sql;
        }
    }
}
