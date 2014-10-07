using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace ServiceStack.OrmLite.Oracle
{
    public class OracleDoubleConverter
    {
        public Assembly OracleAssembly { get; set; }
        private MethodInfo GetOracleValue { get; set; }

        public OracleDoubleConverter(DbProviderFactory factory)
        {
            OracleAssembly = factory.GetType().Assembly;
            var readerType = OracleAssembly.GetType("Oracle.DataAccess.Client.OracleDataReader");
            GetOracleValue = readerType.GetMethod("GetOracleValue", BindingFlags.Public | BindingFlags.Instance);
        }

        public double ConvertToDouble(IDataReader dataReader, int colIndex)
        {
            object value;
            if (GetOracleValue == null)
                value = dataReader.GetValue(colIndex);
            else
                value = GetOracleValue.Invoke(dataReader, new object[] {colIndex});
            return double.Parse(value.ToString(), CultureInfo.InvariantCulture);
        }
    }
}
