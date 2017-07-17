﻿using System;
using System.Data;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Converters
{
    public class StringConverter : OrmLiteConverter, IHasColumnDefinitionLength
    {
        public StringConverter() : this(8000) {}

        public StringConverter(int stringLength)
        {
            StringLength = stringLength;
        }

        public bool UseUnicode { get; set; }

        public int StringLength { get; set; }

        protected string maxColumnDefinition;
        public virtual string MaxColumnDefinition
        {
            get => maxColumnDefinition ?? ColumnDefinition;
            set => maxColumnDefinition = value;
        }

        public override string ColumnDefinition => GetColumnDefinition(StringLength);

        public virtual string GetColumnDefinition(int? stringLength)
        {
            if (stringLength.GetValueOrDefault() == StringLengthAttribute.MaxText)
                return MaxColumnDefinition;

            return UseUnicode
                ? $"NVARCHAR({stringLength.GetValueOrDefault(StringLength)})"
                : $"VARCHAR({stringLength.GetValueOrDefault(StringLength)})";
        }

        public override object FromDbValue(Type fieldType, object value)
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
        public override string ColumnDefinition => "CHAR(1)";

        public override DbType DbType => DbType.StringFixedLength;

        public override string GetColumnDefinition(int? stringLength)
        {
            return ColumnDefinition;
        }

        public override object FromDbValue(Type fieldType, object value)
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

        public override object FromDbValue(Type fieldType, object value)
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