using System;
using System.Globalization;
using System.Reflection;

namespace ServiceStack.OrmLite.Oracle
{
    public class OracleValue : IConvertible
    {
        private readonly object _oracleValue;
        private readonly Type _oracleValueType;
        public OracleValue(object oracleValue)
        {
            _oracleValue = oracleValue;
            _oracleValueType = oracleValue.GetType();
        }

        public TypeCode GetTypeCode()
        {
            return typeof(OracleValue).GetTypeCode();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(Value);
        }

        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(Value);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(Value);
        }

        public byte ToByte(IFormatProvider provider)
        {
            var method = _oracleValueType.GetMethod("ToByte", BindingFlags.Public | BindingFlags.Instance);
            return (byte)method.Invoke(_oracleValue, null);
        }

        public short ToInt16(IFormatProvider provider)
        {
            var method = _oracleValueType.GetMethod("ToInt16", BindingFlags.Public | BindingFlags.Instance);
            return (short)method.Invoke(_oracleValue, null);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(Value);
        }

        public int ToInt32(IFormatProvider provider)
        {
            var method = _oracleValueType.GetMethod("ToInt32", BindingFlags.Public | BindingFlags.Instance);
            return (int)method.Invoke(_oracleValue, null);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(Value);
        }

        public long ToInt64(IFormatProvider provider)
        {
            var method = _oracleValueType.GetMethod("ToInt64", BindingFlags.Public | BindingFlags.Instance);
            return (long)method.Invoke(_oracleValue, null);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(Value);
        }

        public float ToSingle(IFormatProvider provider)
        {
            var method = _oracleValueType.GetMethod("ToSingle", BindingFlags.Public | BindingFlags.Instance);
            return (float)method.Invoke(_oracleValue, null);
        }

        public double ToDouble(IFormatProvider provider)
        {
            var method = _oracleValueType.GetMethod("ToDouble", BindingFlags.Public | BindingFlags.Instance);
            return (double)method.Invoke(_oracleValue, null);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return (decimal) Value;
        }

        private object Value
        {
            get
            {
                var method = _oracleValueType.GetMethod("get_Value", BindingFlags.Public | BindingFlags.Instance);
                return method != null ? method.Invoke(_oracleValue, null) : _oracleValue;
            }
        }

        public bool IsNull()
        {
            if (_oracleValue is DBNull)
                return true;

            var method = _oracleValueType.GetMethod("get_IsNull", BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
                return _oracleValue == null;

            return (bool)method.Invoke(_oracleValue, null);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(Value);
        }

        public string ToString(IFormatProvider provider)
        {
            return _oracleValue == null ? string.Empty : _oracleValue.ToString();
        }

        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return Convert.ChangeType(Value, conversionType, provider);
        }
    }
}