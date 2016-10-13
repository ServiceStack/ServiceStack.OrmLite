using System;
using ServiceStack.DataAnnotations;
#if NETSTANDARD1_3
using System.Globalization;
#endif

namespace ServiceStack.OrmLite.Converters
{
    public class EnumConverter : StringConverter
    {
        public EnumConverter() : base(255) {}

        public override string ToQuotedString(Type fieldType, object value)
        {
            var isEnumAsInt = fieldType.HasAttribute<EnumAsIntAttribute>();
            if (isEnumAsInt)
                return this.ConvertNumber(Enum.GetUnderlyingType(fieldType), value).ToString();

            var isEnumFlags = fieldType.IsEnumFlags() ||
                (!fieldType.IsEnum() && fieldType.IsNumericType()); //i.e. is real int && not Enum

            long enumValue;
            if (!isEnumFlags && long.TryParse(value.ToString(), out enumValue))
                value = Enum.ToObject(fieldType, enumValue);

            var enumString = DialectProvider.StringSerializer.SerializeToString(value);
            if (enumString == null || enumString == "null")
                enumString = value.ToString();

            return !isEnumFlags 
                ? DialectProvider.GetQuotedValue(enumString.Trim('"')) 
                : enumString;
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            var isIntEnum = fieldType.IsEnumFlags() || 
                fieldType.HasAttribute<EnumAsIntAttribute>() ||
                (!fieldType.IsEnum() && fieldType.IsNumericType()); //i.e. is real int && not Enum

            if (isIntEnum && value.GetType().IsEnum())
                return Convert.ChangeType(value, Enum.GetUnderlyingType(fieldType));

            long enumValue;
            if (long.TryParse(value.ToString(), out enumValue))
            {
                if (isIntEnum)
                    return enumValue;

                value = Enum.ToObject(fieldType, enumValue);
            }

            var enumString = DialectProvider.StringSerializer.SerializeToString(value);
            return enumString != null && enumString != "null"
                ? enumString.Trim('"') 
                : value.ToString();
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            var strVal = value as string;
            if (strVal != null)
                return Enum.Parse(fieldType, strVal, ignoreCase:true);

            return Enum.ToObject(fieldType, value);
        }
    }

    public class RowVersionConverter : OrmLiteConverter
    {
        public override string ColumnDefinition => "BIGINT";

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
        public override string ColumnDefinition => DialectProvider.GetStringConverter().MaxColumnDefinition;

        public override string MaxColumnDefinition => DialectProvider.GetStringConverter().MaxColumnDefinition;

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
            return fieldType == typeof(byte[]) 
                ? value 
                : DialectProvider.StringSerializer.SerializeToString(value);
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            var convertedValue = DialectProvider.StringSerializer.DeserializeFromString(value.ToString(), fieldType);
            return convertedValue;
        }
    }

    public class ValueTypeConverter : StringConverter
    {
        public override string ColumnDefinition => DialectProvider.GetStringConverter().MaxColumnDefinition;
        public override string MaxColumnDefinition => DialectProvider.GetStringConverter().MaxColumnDefinition;

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
            var convertedValue = DialectProvider.StringSerializer.DeserializeFromString(value.ToString(), fieldType);
            return convertedValue;
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            if (fieldType.InstanceOfType(value))
                return value;

            var convertedValue = DialectProvider.StringSerializer.DeserializeFromString(value.ToString(), fieldType);
            return convertedValue;
        }
    }
}