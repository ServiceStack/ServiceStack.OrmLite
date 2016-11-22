using System;
using System.Data;
using System.Text;
using ServiceStack.Text;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServer2014OrmLiteDialectProvider : SqlServer2012OrmLiteDialectProvider
    {
        public static new SqlServer2014OrmLiteDialectProvider Instance = new SqlServer2014OrmLiteDialectProvider();

        public override string ToCreateTableStatement(Type tableType)
        {
            var sbColumns = StringBuilderCache.Allocate();
            var sbConstraints = StringBuilderCacheAlt.Allocate();
            var sbMemOptimized = StringBuilderCacheAlt.Allocate();

            var isMemoryTable = tableType.HasAttribute<SqlServerMemoryOptimizedAttribute>();

            var modelDef = OrmLiteUtils.GetModelDefinition(tableType);
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.CustomSelect != null)
                    continue;

                var columnDefinition = GetColumnDefinition(
                    fieldDef.FieldName,
                    fieldDef.ColumnType,
                    fieldDef.IsPrimaryKey,
                    fieldDef.AutoIncrement,
                    fieldDef.IsNullable,
                    fieldDef.IsRowVersion,
                    fieldDef.FieldLength,
                    fieldDef.Scale,
                    GetDefaultValue(fieldDef),
                    fieldDef.CustomFieldDefinition);

                if (columnDefinition == null)
                    continue;

                var collationAttribs = fieldDef.PropertyInfo.GetAttributes<SqlServerCollateAttribute>();
                if (collationAttribs.Count > 0)
                {
                    columnDefinition += $" COLLATE {collationAttribs[0].collation}";
                }

                if (isMemoryTable && fieldDef.IsPrimaryKey)
                {
                    columnDefinition = columnDefinition.Replace("PRIMARY KEY", "PRIMARY KEY NONCLUSTERED");

                    var bucketCountAtribs = fieldDef.PropertyInfo.GetAttributes<SqlServerBucketCountAttribute>();
                    if (bucketCountAtribs.Count > 0)
                    {
                        columnDefinition += $" HASH WITH (BUCKET_COUNT={bucketCountAtribs[0].count})";
                    }
                }

                if (sbColumns.Length != 0)
                    sbColumns.Append(", \n  ");

                sbColumns.Append(columnDefinition);

                if (fieldDef.ForeignKey == null) continue;

                var refModelDef = OrmLiteUtils.GetModelDefinition(fieldDef.ForeignKey.ReferenceType);
                sbConstraints.Append(
                    $", \n\n  CONSTRAINT {GetQuotedName(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy, fieldDef))} " +
                    $"FOREIGN KEY ({GetQuotedColumnName(fieldDef.FieldName)}) " +
                    $"REFERENCES {GetQuotedTableName(refModelDef)} ({GetQuotedColumnName(refModelDef.PrimaryKey.FieldName)})");

                sbConstraints.Append(GetForeignKeyOnDeleteClause(fieldDef.ForeignKey));
                sbConstraints.Append(GetForeignKeyOnUpdateClause(fieldDef.ForeignKey));
            }

            if (isMemoryTable)
            {
                var attrib = tableType.FirstAttribute<SqlServerMemoryOptimizedAttribute>();
                sbMemOptimized.Append(" WITH (MEMORY_OPTIMIZED=ON");
                if (attrib.Durability == SqlServerDurability.SchemaOnly)
                    sbMemOptimized.Append(", DURABILITY=SCHEMA_ONLY");
                else if (attrib.Durability == SqlServerDurability.SchemaAndData)
                    sbMemOptimized.Append(", DURABILITY=SCHEMA_AND_DATA");
                sbMemOptimized.Append(")");
            }

            var sql = $"CREATE TABLE {GetQuotedTableName(modelDef)} " +
                      $"\n(\n  {StringBuilderCache.ReturnAndFree(sbColumns)}{StringBuilderCacheAlt.ReturnAndFree(sbConstraints)} \n){StringBuilderCache.ReturnAndFree(sbMemOptimized)}; \n";

            return sql;
        }
    }
}
