using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;

namespace ServiceStack.OrmLite.VistaDB
{
    public class VistaDB5DialectProvider : ReflectionBasedDialectProvider<VistaDB5DialectProvider>
    {
        const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        public static VistaDB5DialectProvider Instance = new VistaDB5DialectProvider();

        private static DateTime _timeSpanOffset = new DateTime(1900, 01, 01);

        public VistaDB5DialectProvider()
            : base("VistaDB.5.NET40", "VistaDB.Provider.VistaDBConnection")
        {
            base.AutoIncrementDefinition = "IDENTITY(1,1)";
            base.SelectIdentitySql = "SELECT @@IDENTITY";
            this.StringColumnDefinition = UseUnicode ? "NVARCHAR(4000)" : "VARCHAR(8000)";
            base.GuidColumnDefinition = "UniqueIdentifier";
            base.RealColumnDefinition = "FLOAT";
            base.BoolColumnDefinition = "BIT";
            base.BlobColumnDefinition = "VARBINARY";
            base.IntColumnDefinition = "INT";
            base.DefaultValueFormat = " DEFAULT {0}";

            base.InitColumnTypeMap();           
        }

        public override void OnAfterInitColumnTypeMap()
        {
            DbTypeMap.ColumnTypeMap.Remove(typeof(object));
            DbTypeMap.ColumnDbTypeMap.Remove(typeof(object));
        }

        public override string GetQuotedValue(string paramValue)
        {
            return (this.UseUnicode ? "N'" : "'") + paramValue.Replace("'", "''") + "'";
        }

        public override void SetParameterValues<T>(IDbCommand dbCmd, object obj)
        {
            base.SetParameterValues<T>(dbCmd, obj);

            foreach (IDbDataParameter p in dbCmd.Parameters)
            {
                var newName = p.ParameterName.Replace(" ", "___");
                dbCmd.CommandText = dbCmd.CommandText.Replace(p.ParameterName, newName);

                p.ParameterName = newName;
            }
        }

        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            var isFullConnectionString = connectionString.Contains(";");

            if (!isFullConnectionString)
            {
                var filePath = connectionString;
                var filePathWithExt = filePath.ToLower().EndsWith(".vdb4")
                    ? filePath
                    : filePath + ".vdb4";

                var fileName = Path.GetFileName(filePathWithExt);

                connectionString = string.Format(@"Data Source={0};Open Mode=NonExclusiveReadWrite;", filePathWithExt);
            }

            if (options != null)
            {
                foreach (var option in options)
                    connectionString += option.Key + "=" + option.Value + ";";
            }

            return new OrmLiteVistaDbConnection(
                base.CreateConnection(connectionString, options));
        }

        public override string ToCreateTableStatement(Type tableType)
        {
            var modelDefinition = GetModelDefinition(tableType);
            var quotedTableName = this.GetQuotedTableName(modelDefinition);

            var columns = new StringBuilder();
            var constraints = new StringBuilder();

            foreach (var fd in modelDefinition.FieldDefinitions)
            {
                if (columns.Length != 0)
                    columns.Append(", \n  ");
                
                var columnDefinition = this.GetColumnDefinition(
                    fd.FieldName, fd.FieldType, false, fd.AutoIncrement, fd.IsNullable, fd.FieldLength, null, fd.DefaultValue, fd.CustomFieldDefinition);
                
                columns.Append(columnDefinition);

                if (fd.IsPrimaryKey)
                {
                    constraints.AppendFormat("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2});\n",
                        quotedTableName,
                        this.GetQuotedName("PK_" + modelDefinition.ModelName),
                        this.GetQuotedColumnName(fd.FieldName));
                }
                else if (fd.ForeignKey != null)
                {
                    var foreignModelDefinition = GetModelDefinition(fd.ForeignKey.ReferenceType);
                    constraints.AppendFormat("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6};\n",
                        quotedTableName,
				        this.GetQuotedName(fd.ForeignKey.GetForeignKeyName(modelDefinition, foreignModelDefinition, this.NamingStrategy, fd)),
				        this.GetQuotedColumnName(fd.FieldName),
				        this.GetQuotedTableName(foreignModelDefinition),
				        this.GetQuotedColumnName(foreignModelDefinition.PrimaryKey.FieldName),
                        this.GetForeignKeyOnDeleteClause(fd.ForeignKey),
                        this.GetForeignKeyOnUpdateClause(fd.ForeignKey));
                }
            }

            return String.Format("CREATE TABLE {0} \n(\n  {1} \n); \n {2}\n",
                quotedTableName, 
                columns, 
                constraints);
        }
        
        public override string GetColumnDefinition(string fieldName, Type fieldType, bool isPrimaryKey, bool autoIncrement, bool isNullable, int? fieldLength, int? scale, string defaultValue, string customFieldDefinition)
        {
            string fieldDefinition;
            if (fieldType == typeof(string))
                fieldDefinition = string.Format(this.StringLengthColumnDefinitionFormat, fieldLength.GetValueOrDefault(this.DefaultStringLength));
            else if (!this.DbTypeMap.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition))
                fieldDefinition = this.GetUndefinedColumnDefinition(fieldType, fieldLength);
            
            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", this.GetQuotedColumnName(fieldName), fieldDefinition);
            if (isPrimaryKey)
            {
                sql.Append(" PRIMARY KEY");                
            }
            else
            {
                if (isNullable && !autoIncrement)
                    sql.Append(" NULL");
                else
                    sql.Append(" NOT NULL");
            }

            if (autoIncrement)
                sql.Append(" ").Append(this.AutoIncrementDefinition);

            if (!String.IsNullOrEmpty(defaultValue))
                sql.AppendFormat(this.DefaultValueFormat, defaultValue);
            
            return sql.ToString();
        }

        public override string ToExistStatement(Type fromTableType, object objWithProperties, string sqlFilter, params object[] filterParams)
        {
            var fromModelDef = GetModel(fromTableType);
           
            var sql = new StringBuilder();
            sql.AppendFormat("SELECT 1 \nFROM {0}", this.GetQuotedTableName(fromModelDef));

            var filter = new StringBuilder();

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
                        if (def.IsPrimaryKey) 
                            fpk.Add(def);
                    }

                    foreach (var fieldDef in fromModelDef.FieldDefinitions)
                    {
                        if (fieldDef.IsComputed || fieldDef.ForeignKey == null) 
                            continue;

                        var model = GetModel(fieldDef.ForeignKey.ReferenceType);
                        if (model.ModelName != modelDef.ModelName)
                            continue;
                         
                        if (filter.Length > 0) 
                            filter.Append(" AND ");
                            
                        filter.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), fpk[i++].GetQuotedValue(objWithProperties));
                    }
                }
                else
                {
                    var modelDef = GetModel(tableType);
                    foreach (var fieldDef in modelDef.FieldDefinitions)
                    {
                        if (fieldDef.IsComputed || !fieldDef.IsPrimaryKey) 
                            continue;
                        
                        if (filter.Length > 0) 
                            filter.Append(" AND ");
                            
                        filter.AppendFormat("{0} = {1}",
                            GetQuotedColumnName(fieldDef.FieldName), fieldDef.GetQuotedValue(objWithProperties));
                    }
                }

                if (filter.Length > 0) 
                    sql.AppendFormat("\nWHERE {0} ", filter);
            }

            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFmt(filterParams);                 
                sql.Append(filter.Length > 0 ? " AND  " : "\nWHERE ");
                sql.Append(sqlFilter);
            }

            return String.Format("SELECT EXISTS({0});", sql);
        }

        public override object ConvertDbValue(object value, Type type)
        {
            if (value == null || value is DBNull)
                return null;

            if (type == typeof(bool) && !(value is bool))
            {
                var intVal = Convert.ToInt32(value.ToString());
                return intVal != 0;
            }
            
            if (type == typeof(TimeSpan) && value is DateTime)
            {
                var dateTimeValue = (DateTime)value;
                return dateTimeValue - _timeSpanOffset;
            }

            if (type == typeof(byte[]))
                return value;

            return base.ConvertDbValue(value, type);
        }

        public override string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) 
                return "NULL";

            if (fieldType == typeof(Guid))
                return string.Format("CAST('{0}' AS UNIQUEIDENTIFIER)", (Guid)value);

            if (fieldType == typeof(DateTime))
            {
                var dateValue = (DateTime)value;

                return base.GetQuotedValue(dateValue.ToString(dateTimeFormat, CultureInfo.InvariantCulture), typeof(string));
            }

            if (fieldType == typeof(bool))
                return base.GetQuotedValue((bool)value ? 1 : 0, typeof(int));

            if (fieldType == typeof(string))
                return GetQuotedValue(value.ToString());

            if (fieldType == typeof(byte[]))
                return "0x" + BitConverter.ToString((byte[])value).Replace("-", "");

            return base.GetQuotedValue(value, fieldType);
        }
                
        public override SqlExpression<T> SqlExpression<T>()
        {
            return new VistaDB5Expression<T>();
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
        {
            dbCmd.CommandText = "SELECT COUNT(*) FROM [database schema] WHERE typeid = 1 AND name = {0}"
                .SqlFmt(tableName);

            return dbCmd.LongScalar() > 0;
        }

        public override bool UseUnicode
        {
            get { return this.useUnicode; }
            set
            {
                this.useUnicode = value;
                if (this.useUnicode && this.DefaultStringLength > 4000)
                    this.DefaultStringLength = 4000;
            }
        }

        public override string GetForeignKeyOnDeleteClause(ForeignKeyConstraint foreignKey)
        {
            return String.Equals("RESTRICT", foreignKey.OnDelete, StringComparison.OrdinalIgnoreCase)
                ? String.Empty
                : base.GetForeignKeyOnDeleteClause(foreignKey);
        }

        public override string GetForeignKeyOnUpdateClause(ForeignKeyConstraint foreignKey)
        {
            return String.Equals("RESTRICT", foreignKey.OnUpdate, StringComparison.OrdinalIgnoreCase)
                ? String.Empty
                : base.GetForeignKeyOnUpdateClause(foreignKey);
        }

        public override string GetDropForeignKeyConstraints(ModelDefinition modelDef)
        {
            var sb = new StringBuilder();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.ForeignKey != null)
                {
                    var foreignKeyName = fieldDef.ForeignKey.GetForeignKeyName(
                        modelDef,
                        GetModelDefinition(fieldDef.ForeignKey.ReferenceType),
                        NamingStrategy,
                        fieldDef);

                    var tableName = GetQuotedTableName(modelDef);
                    sb.AppendFormat("IF EXISTS (SELECT name FROM [database schema] WHERE typeid = 7 AND name = '{0}')\n", foreignKeyName);
                    sb.AppendLine("BEGIN");
                    sb.AppendFormat("  ALTER TABLE {0} DROP {1};\n", tableName, foreignKeyName);
                    sb.AppendLine("END");
                }
            }

            return sb.ToString();
        }

        public override string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            var column = GetColumnDefinition(
                fieldDef.FieldName,
                fieldDef.FieldType,
                fieldDef.IsPrimaryKey,
                fieldDef.AutoIncrement,
                fieldDef.IsNullable,
                fieldDef.FieldLength,
                fieldDef.Scale,
                fieldDef.DefaultValue,
                fieldDef.CustomFieldDefinition);

            return string.Format("ALTER TABLE {0} ADD {1};",
                GetQuotedTableName(GetModel(modelType).ModelName),
                column);
        }

        public override string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            var column = GetColumnDefinition(
                fieldDef.FieldName, 
                fieldDef.FieldType, 
                fieldDef.IsPrimaryKey, 
                fieldDef.AutoIncrement, 
                fieldDef.IsNullable,
                fieldDef.FieldLength,
                fieldDef.Scale,
                fieldDef.DefaultValue,
                fieldDef.CustomFieldDefinition);

            return string.Format("ALTER TABLE {0} ALTER COLUMN {1};",
                GetQuotedTableName(GetModel(modelType).ModelName),
                column);
        }

        public override string ToChangeColumnNameStatement(Type modelType, FieldDefinition fieldDef, string oldColumnName)
        {
            var objectName = string.Format("{0}.{1}",
                NamingStrategy.GetTableName(GetModel(modelType).ModelName),
                oldColumnName);

            return string.Format("sp_rename ({0}, {1}, {2});",
                GetQuotedValue(objectName),
                GetQuotedValue(fieldDef.FieldName),
                GetQuotedValue("COLUMN"));
        }
    }
}
