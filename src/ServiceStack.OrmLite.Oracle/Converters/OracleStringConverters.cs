using System;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Oracle.Converters
{
    public class OracleStringConverter : StringConverter
    {
        public OracleStringConverter() : base(2000) {}
        public OracleStringConverter(int stringLength) : base(stringLength) {}

        public override string MaxColumnDefinition
        {
            get { return GetColumnDefinition(2000); }
        }

        public override string GetColumnDefinition(int? stringLength)
        {
            var maxLength = UseUnicode ? 2000 : 4000;
            var safeLength = Math.Min(stringLength.GetValueOrDefault(StringLength), maxLength);
            return UseUnicode
                ? "NVARCHAR2({0})".Fmt(safeLength)
                : "VARCHAR2({0})".Fmt(safeLength);
        }
    }
}