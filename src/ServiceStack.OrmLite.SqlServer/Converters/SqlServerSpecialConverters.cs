using System;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public class SqlServerRowVersionConverter : RowVersionConverter
    {
        public override string ColumnDefinition
        {
            get { return "rowversion"; }
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            var bytes = value as byte[];
            if (bytes != null)
            {
	            if (fieldType == typeof(byte[])) return bytes;
				if (fieldType == typeof(ulong)) return OrmLiteUtils.ConvertToULong(bytes);

                // an SQL row version has to be declared as either byte[] OR ulong... 
				throw new Exception("SQL Rowversion property must be declared as either byte[] or ulong");
            }
            return null;
        }

  //      public override object FromDbRowVersion(Type fieldType, object value)
  //      {
  //          var bytes = value as byte[];
	 //       if (bytes != null)
	 //       {
		//        if (fieldType == typeof(byte[])) return bytes;
		//        if (fieldType == typeof(ulong)) return OrmLiteUtils.ConvertToULong(bytes);

		//        // an SQL row version has to be declared as either byte[] OR ulong... 
		//        throw new Exception("SQL Rowversion property must be declared as either byte[] or ulong");
	 //       }
		//	//var ulongValue = OrmLiteUtils.ConvertToULong(bytes);
		//	//return ulongValue;
		//	throw new Exception("Rowversion could not be parsed as byte array");
		//}
    }
}