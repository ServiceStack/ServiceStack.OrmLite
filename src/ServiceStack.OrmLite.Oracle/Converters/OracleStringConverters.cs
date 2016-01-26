using System;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Oracle.Converters
{
    public class OracleStringConverter : StringConverter
    {
        public OracleStringConverter() : base(128) {}
        public OracleStringConverter(int stringLength) : base(stringLength) {}

        public override string MaxColumnDefinition
        {
            get { return GetColumnDefinition(4000); }
        }

        public override string GetColumnDefinition(int? stringLength)
        {
            var safeLength = Math.Min(stringLength.GetValueOrDefault(StringLength), 4000);
            return UseUnicode
                ? "NVARCHAR2({0})".Fmt(safeLength)
                : "VARCHAR2({0})".Fmt(safeLength);
        }
    }
}