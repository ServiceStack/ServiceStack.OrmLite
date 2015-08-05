using System.Data;

namespace ServiceStack.OrmLite
{
    public interface IOrmLiteConverter
    {
        IOrmLiteDialectProvider DialectProvider { get; set; }

        string ToQuotedString(object value);

        object ToDbValue(FieldDefinition fieldDef, object value);

        object FromDbValue(FieldDefinition fieldDef, IDataReader reader, int columnIndex);
    }
}