using System;
using System.Data;

namespace ServiceStack.OrmLite.Converters
{
    public class StringConverter : OrmLiteConverter
    {
        public StringConverter() : this(8000) {}

        public StringConverter(int stringLength)
        {
            StringLength = stringLength;
        }

        public bool UseUnicode { get; set; }

        public int StringLength { get; set; }

        public virtual string MaxColumnDefinition
        {
            get { return ColumnDefinition; }
        }

        public override string ColumnDefinition
        {
            get { return GetColumnDefinition(StringLength); }
        }

        public virtual string GetColumnDefinition(int? stringLength)
        {
            return UseUnicode
                ? "NVARCHAR({0})".Fmt(stringLength.GetValueOrDefault(StringLength))
                : "VARCHAR({0})".Fmt(stringLength.GetValueOrDefault(StringLength));
        }

        public override object FromDbValue(FieldDefinition fieldDef, object value)
        {
            var strValue = value as string;
            if (strValue != null)
            {
                if (OrmLiteConfig.StringFilter != null)
                    return OrmLiteConfig.StringFilter(strValue);
            }

            return value.ToString();
        }
    }

    public class CharConverter : StringConverter
    {
        public override DbType DbType
        {
            get { return DbType.StringFixedLength; }
        }

        public override object FromDbValue(FieldDefinition fieldDef, object value)
        {
            if (value is char)
                return value;

            var strValue = value as string;
            if (strValue != null)
                return strValue[0];

            if (value.GetType().IsIntegerType())
                return (char) (int) this.ConvertNumber(typeof(int), value);

            return (char)value;
        }
    }

    public class CharArrayConverter : StringConverter
    {
        public CharArrayConverter() {}
        public CharArrayConverter(int stringLength) : base(stringLength) {}

        public override object ToDbValue(Type fieldType, object value)
        {
            var chars = (char[]) value;
            return new string(chars);
        }

        public override object FromDbValue(FieldDefinition fieldDef, object value)
        {
            if (value is char[])
                return value;

            var strValue = value as string;
            if (strValue != null)
                return strValue.ToCharArray();

            return (char[])value;
        }
    }
}