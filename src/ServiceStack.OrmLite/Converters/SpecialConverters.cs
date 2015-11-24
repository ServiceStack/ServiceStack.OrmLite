using System;

namespace ServiceStack.OrmLite.Converters
{
    public class EnumConverter : StringConverter
    {
        public EnumConverter() : base(255) {}

        public override string ToQuotedString(Type fieldType, object value)
        {
            var isEnumFlags = fieldType.IsEnumFlags() ||
                (!fieldType.IsEnum && fieldType.IsNumericType()); //i.e. is real int && not Enum

            long enumValue;
            if (!isEnumFlags && long.TryParse(value.ToString(), out enumValue))
            {
                value = Enum.ToObject(fieldType, enumValue).ToString();
            }

            var enumString = DialectProvider.StringSerializer.SerializeToString(value);

            if (!isEnumFlags)
                return DialectProvider.GetQuotedValue(enumString.Trim('"'));

            return enumString;
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            var enumValue = DialectProvider.StringSerializer.SerializeToString(value);
            if (enumValue == null)
                return null;

            enumValue = enumValue.Trim('"');
            long intEnum;
            if (long.TryParse(enumValue, out intEnum))
                return intEnum;

            return enumValue;
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            var strVal = value as string;
            if (strVal != null)
                return Enum.Parse(fieldType, strVal, ignoreCase:true);

            return Convert.ChangeType(value, fieldType.GetTypeCode());
        }
    }

    public class RowVersionConverter : OrmLiteConverter
    {
        public override string ColumnDefinition
        {
            get { return "BIGINT"; }
        }

        public virtual ulong FromDbRowVersion(object value)
        {
            return (ulong)this.ConvertNumber(typeof(ulong), value);
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            return value != null
                ? this.ConvertNumber(typeof(ulong), value)
                : null;
        }
    }

    public class ReferenceTypeConverter : StringConverter
    {
        public override string ColumnDefinition
        {
            get { return DialectProvider.GetStringConverter().MaxColumnDefinition; }
        }

        public override string MaxColumnDefinition
        {
            get { return DialectProvider.GetStringConverter().MaxColumnDefinition; }
        }

        public override string GetColumnDefinition(int? stringLength)
        {
            return stringLength != null
                ? base.GetColumnDefinition(stringLength)
                : MaxColumnDefinition;
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            return DialectProvider.GetQuotedValue(DialectProvider.StringSerializer.SerializeToString(value));
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            //Let ADO.NET providers handle byte[]
            if (fieldType == typeof(byte[]))
                return value;

            return DialectProvider.StringSerializer.SerializeToString(value);
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            var convertedValue = DialectProvider.StringSerializer.DeserializeFromString(value.ToString(), fieldType);
            return convertedValue;
        }
    }

    public class ValueTypeConverter : StringConverter
    {
        public override string ColumnDefinition
        {
            get { return DialectProvider.GetStringConverter().MaxColumnDefinition; }
        }

        public override string MaxColumnDefinition
        {
            get { return DialectProvider.GetStringConverter().MaxColumnDefinition; }
        }

        public override string GetColumnDefinition(int? stringLength)
        {
            return stringLength != null
                ? base.GetColumnDefinition(stringLength)
                : MaxColumnDefinition;
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            return DialectProvider.GetQuotedValue(DialectProvider.StringSerializer.SerializeToString(value));
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            return DialectProvider.ToDbValue(value, fieldType);
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            return DialectProvider.FromDbValue(value, fieldType);
        }
    }
}