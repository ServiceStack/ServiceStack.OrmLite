using System;
using System.Data;

namespace ServiceStack.OrmLite.Sqlite.Converters
{
    public class SqliteGuidConverter : OrmLiteConverter
    {
        public override string ColumnDefinition
        {
            get { return "CHAR(36)"; }
        }

        public override object FromDbValue(FieldDefinition fieldDef, IDataReader reader, int columnIndex)
        {
            var guidStr = reader.GetString(columnIndex);
            return new Guid(guidStr);
        }
    }
}