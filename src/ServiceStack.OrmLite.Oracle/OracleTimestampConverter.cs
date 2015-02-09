using System;
using System.Reflection;

namespace ServiceStack.OrmLite.Oracle
{
    public class OracleTimestampConverter
    {
        private readonly Lazy<ConstructorInfo> _oracleTimeStampTzConstructor;

        private readonly Type _factoryType;

        public OracleTimestampConverter(Type factoryType)
        {
            _factoryType = factoryType;
            _oracleTimeStampTzConstructor = new Lazy<ConstructorInfo>(() => InitConstructor(_factoryType));
        }

        private static ConstructorInfo InitConstructor(Type factoryType)
        {
            var oracleAssembly = factoryType.Assembly;

            var globalizationType = oracleAssembly.GetType("Oracle.DataAccess.Client.OracleGlobalization");
            if (globalizationType == null) return null;

            var oracleTimeStampTzType = oracleAssembly.GetType("Oracle.DataAccess.Types.OracleTimeStampTZ");
            if (oracleTimeStampTzType == null) return null;

            return oracleTimeStampTzType.GetConstructor(new[] { typeof(DateTime), typeof(string) });
        }

        private ConstructorInfo OracleTimeStampTzConstructor
        {
            get
            {
                return _oracleTimeStampTzConstructor != null 
                    ? _oracleTimeStampTzConstructor.Value 
                    : null;
            }
        }

        public object ConvertToOracleTimeStampTz(DateTimeOffset timestamp)
        {
            if (OracleTimeStampTzConstructor != null)
            {
                return OracleTimeStampTzConstructor
                    .Invoke(new object[] { timestamp.DateTime, timestamp.Offset.ToString()});
            }

            return timestamp;
        }
    }
}
