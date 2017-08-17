using System;
using System.Data;
using System.Data.SqlClient;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Converters;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public class SqlServerJsonToObjectConverter : SqlServerStringConverter
	{
		public override object FromDbValue(Type fieldType, object value)
		{
			var deflt = fieldType.GetDefaultValue();
			if (value == null || value == deflt)
				return deflt;

			var json = value.ToString();
			return JsonSerializer.DeserializeFromString(json, fieldType);
		}

		public override object ToDbValue(Type fieldType, object value)
		{
			if (value != null && value.GetType().HasInterface(typeof(ISqlJson)))
			{
				return value.ToJson();
			}

			return base.ToDbValue(fieldType, value);
		}
	}

	public interface ISqlJson
	{ }
}