using System.Data;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    public class PostgreSqlCharConverter : OrmLiteConverter
    {
        public override string ColumnDefinition
        {
            get { return "integer"; }
        }

        public override DbType DbType
        {
            get { return DbType.Int32; }
        }

        public override string ToQuotedString(object value)
        {
            var intVal = (int)(char)value;
            return intVal.ToString();
        }

        public override object ToDbValue(FieldDefinition fieldDef, object value)
        {
            return (int)(char)value;
        }

        public override object FromDbValue(FieldDefinition fieldDef, IDataReader reader, int columnIndex)
        {
            var value = reader.GetValue(columnIndex);
            return (char)(int)value;
        }
    }
}