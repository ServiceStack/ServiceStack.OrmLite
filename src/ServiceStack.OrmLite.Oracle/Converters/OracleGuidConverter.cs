using System;
using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Oracle.Converters
{
    public class OracleGuidConverter : GuidConverter
    {
        public override string ColumnDefinition
        {
            get { return "VARCHAR2(37)"; }
        }

        public override DbType DbType
        {
            get { return DbType.String; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            var guid = (Guid)value;
            return string.Format("CAST('{0}' AS {1})", guid, ColumnDefinition);
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            return ((Guid)value).ToString();
        }

        public virtual object ToDbValue(FieldDefinition fieldDef, object value)
        {
            var guid = (Guid)value;
            return guid.ToString();
        }

        public override object FromDbValue(FieldDefinition fieldDef, object value)
        {
            return new Guid(value.ToString());
        }
    }

    public class OracleCompactGuidConverter : GuidConverter
    {
        public override string ColumnDefinition
        {
            get { return "RAW(16)"; }
        }

        public override DbType DbType
        {
            get { return DbType.Binary; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            var guid = (Guid)value;
            return string.Format("CAST('{0}' AS {1})", BitConverter.ToString(guid.ToByteArray()).Replace("-", ""), ColumnDefinition);
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            return ((Guid)value).ToByteArray();
        }

        public virtual object ToDbValue(FieldDefinition fieldDef, object value)
        {
            var guid = (Guid)value;
            return guid.ToByteArray();
        }

        public override object FromDbValue(FieldDefinition fieldDef, object value)
        {
            var raw = (byte[])value;
            return new Guid(raw);
        }
    }
}