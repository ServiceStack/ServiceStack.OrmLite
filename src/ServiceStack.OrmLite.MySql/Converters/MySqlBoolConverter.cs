using System;
using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.MySql.Converters
{
    public class MySqlBoolConverter : BoolAsIntConverter
    {
        public override string ColumnDefinition
        {
            get { return "tinyint(1)"; }
        }
    }
}