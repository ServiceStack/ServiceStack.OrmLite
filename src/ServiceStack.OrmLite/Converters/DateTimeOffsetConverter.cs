namespace ServiceStack.OrmLite.Converters
{
    public abstract class DateTimeOffsetConverter : OrmLiteConverter
    {
        //From OrmLiteDialectProviderBase:
        //public override object FromDbValue(FieldDefinition fieldDef, IDataReader reader, int columnIndex)
        //{
        //    var value = reader.GetValue(columnIndex);
        //    var strValue = value as string;
        //    if (strValue != null)
        //    {
        //        var moment = DateTimeOffset.Parse(strValue, null, DateTimeStyles.RoundtripKind);
        //        return moment;
        //    }
        //    if (value is DateTime)
        //    {
        //        return new DateTimeOffset((DateTime)value);
        //    }
        //    return value;
        //}
    }
}