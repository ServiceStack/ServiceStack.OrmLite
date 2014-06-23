using System.Text;

namespace ServiceStack.OrmLite.SqlServer
{
	public class SqlServerExpression<T> : SqlExpression<T>
	{
        public SqlServerExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}

        public override string ToUpdateStatement(T item, bool excludeDefaults = false)
        {
            var setFields = new StringBuilder();
            var dialectProvider = OrmLiteConfig.DialectProvider;

            foreach (var fieldDef in ModelDef.FieldDefinitions)
            {
                if (UpdateFields.Count > 0 && !UpdateFields.Contains(fieldDef.Name) || fieldDef.AutoIncrement) continue; // added
                var value = fieldDef.GetValue(item);
                if (excludeDefaults && (value == null || value.Equals(value.GetType().GetDefaultValue()))) continue; //GetDefaultValue?

                fieldDef.GetQuotedValue(item);

                if (setFields.Length > 0) 
                    setFields.Append(", ");

                setFields.AppendFormat("{0}={1}",
                    dialectProvider.GetQuotedColumnName(fieldDef.FieldName),
                    dialectProvider.GetQuotedValue(value, fieldDef.FieldType));
            }

            return string.Format("UPDATE {0} SET {1} {2}",
                dialectProvider.GetQuotedTableName(ModelDef), setFields, WhereExpression);
        }
	}
}