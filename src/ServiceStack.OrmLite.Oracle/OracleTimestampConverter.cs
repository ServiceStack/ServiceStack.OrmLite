using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace ServiceStack.OrmLite.Oracle
{
    public class OracleTimestampConverter
    {
        private string DateTimeOffsetOutputFormat { get; set; }
        private string DateTimeOffsetInputFormat { get; set; }
        private string TimestampTzFormat { get; set; }

        private const BindingFlags InvokeStaticPublic = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod;
        private Assembly OracleAssembly { get; set; }
        private object[] SetThreadInfoArgs { get; set; }
        private MethodInfo SetThreadInfo { get; set; }
        private MethodInfo SetOracleDbType { get; set; }
        private object[] SetOracleDbTypeArgs { get; set; }
        private MethodInfo GetOracleValue { get; set; }

        public OracleTimestampConverter(DbProviderFactory factory)
        {
            OracleAssembly = factory.GetType().Assembly;
            var globalizationType = OracleAssembly.GetType("Oracle.DataAccess.Client.OracleGlobalization");
            if (globalizationType != null)
            {
                DateTimeOffsetInputFormat = DateTimeOffsetOutputFormat = "yyyy-MM-dd HH:mm:ss.ffffff zzz";
                TimestampTzFormat = "YYYY-MM-DD HH24:MI:SS.FF6 TZH:TZM";

                SetThreadInfoArgs = new [] {globalizationType.InvokeMember("GetClientInfo", InvokeStaticPublic, null, null, null)};
                const BindingFlags setProperty = BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance;
                globalizationType.InvokeMember("TimeStampTZFormat", setProperty, null, SetThreadInfoArgs[0], new object[] { TimestampTzFormat });
                SetThreadInfo = globalizationType.GetMethod("SetThreadInfo", BindingFlags.Public | BindingFlags.Static);

                var parameterType = OracleAssembly.GetType("Oracle.DataAccess.Client.OracleParameter");
                var oracleDbTypeProperty = parameterType.GetProperty("OracleDbType", BindingFlags.Public | BindingFlags.Instance);
                SetOracleDbType = oracleDbTypeProperty.GetSetMethod();

                var oracleDbType = OracleAssembly.GetType("Oracle.DataAccess.Client.OracleDbType");
                SetOracleDbTypeArgs = new [] {Enum.Parse(oracleDbType, "TimeStampTZ")};

                var readerType = OracleAssembly.GetType("Oracle.DataAccess.Client.OracleDataReader");
                GetOracleValue = readerType.GetMethod("GetOracleValue", BindingFlags.Public | BindingFlags.Instance);
            }
            else
            {
                //TODO This is Microsoft provider support and it does not handle the offsets correctly,
                // but I don't know how to make it work.

                DateTimeOffsetOutputFormat = "dd-MMM-yy hh:mm:ss.fff tt";
                DateTimeOffsetInputFormat = "dd-MMM-yy hh:mm:ss tt";
                TimestampTzFormat = "DD-MON-RR HH.MI.SSXFF AM";

//                var parameterType = OracleAssembly.GetType("System.Data.OracleClient.OracleParameter");
//                var oracleTypeProperty = parameterType.GetProperty("OracleType", BindingFlags.Public | BindingFlags.Instance);
//                SetOracleDbType = oracleTypeProperty.GetSetMethod();

                var oracleDbType = OracleAssembly.GetType("System.Data.OracleClient.OracleType");
                SetOracleDbTypeArgs = new [] {Enum.Parse(oracleDbType, "TimestampWithTZ")};

//                var readerType = OracleAssembly.GetType("System.Data.OracleClient.OracleDataReader");
//                GetOracleValue = readerType.GetMethod("GetOracleValue", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        public void SetOracleTimestampTzFormat()
        {
            if (SetThreadInfoArgs != null)
                SetThreadInfo.Invoke(null, SetThreadInfoArgs);
        }

        public void SetOracleParameterTypeTimestampTz(IDataParameter p)
        {
            if (SetOracleDbType != null)
                SetOracleDbType.Invoke(p, SetOracleDbTypeArgs);
        }

        public DateTimeOffset ConvertTimestampTzToDateTimeOffset(IDataReader dataReader, int colIndex)
        {
            if (GetOracleValue != null)
            {
                var value = GetOracleValue.Invoke(dataReader, new object[] { colIndex }).ToString();
                return DateTimeOffset.ParseExact(value, DateTimeOffsetInputFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                var value = dataReader.GetValue(colIndex);
                return new DateTimeOffset((DateTime)value);
            }
        }

        public string ConvertDateTimeOffsetToString(DateTimeOffset timestamp)
        {
            return timestamp.ToString(DateTimeOffsetOutputFormat, CultureInfo.InvariantCulture);
        }
    }
}
