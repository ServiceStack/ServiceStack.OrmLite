using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Converters;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public class SqlServerJsonStringConverter : SqlServerStringConverter
	{
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, HashSet<RuntimeTypeHandle>> atrCache =
            new ConcurrentDictionary<RuntimeTypeHandle, HashSet<RuntimeTypeHandle>>();

        private static bool HasAttribute<T>(Type type) where T : Attribute
        {
            HashSet<RuntimeTypeHandle> atrHash = null;
            if (!atrCache.TryGetValue(type.TypeHandle, out atrHash))
            {
                atrHash = type.GetCustomAttributes(true).Select(a => a.GetType().TypeHandle).ToHashSet();
                atrCache.AddOrUpdate(type.TypeHandle, atrHash, (k, v) => atrHash);
            }
            return atrHash.Contains(typeof(T).TypeHandle);
        }

        // json string to object
        public override object FromDbValue(Type fieldType, object value)
		{
			if (value is string raw && HasAttribute<SqlJsonAttribute>(fieldType))
				return JsonSerializer.DeserializeFromString(raw, fieldType);

			return base.FromDbValue(fieldType, value);
		}

		// object to json string
		public override object ToDbValue(Type fieldType, object value)
		{
			if (HasAttribute<SqlJsonAttribute>(value.GetType()))
				return JsonSerializer.SerializeToString(value, value.GetType());

			return base.ToDbValue(fieldType, value);
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class SqlJsonAttribute : Attribute
	{ }
}