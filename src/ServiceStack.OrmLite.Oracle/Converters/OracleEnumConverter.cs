using System;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Oracle.Converters
{
    public class OracleEnumConverter : EnumConverter
    {
        public override string ToQuotedString(Type fieldType, object value)
        {
            if (value is int && !fieldType.IsEnumFlags())
            {
                value = fieldType.GetEnumName(value);
            }

            if (fieldType.IsEnum)
            {
                var enumValue = DialectProvider.StringSerializer.SerializeToString(value);
                // Oracle stores empty strings in varchar columns as null so match that behavior here
                if (enumValue == null)
                    return null;
                enumValue = DialectProvider.GetQuotedValue(enumValue.Trim('"'));
                return enumValue == "''"
                    ? "null"
                    : enumValue;
            }
            return base.ToQuotedString(fieldType, value);
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            if (value is int && !fieldType.IsEnumFlags())
            {
                value = fieldType.GetEnumName(value);
            }

            var enumValue = DialectProvider.StringSerializer.SerializeToString(value);
            // Oracle stores empty strings in varchar columns as null so match that behavior here
            if (enumValue == null)
                return null;
            enumValue = enumValue.Trim('"');
            return enumValue == ""
                ? null
                : enumValue;
        }
    }
}