using System;
namespace ServiceStack.OrmLite
{
	public interface INamingStrategy
	{
		string GetTableName(string name);
		string GetColumnName(string name);
	}
}
