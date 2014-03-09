using System.Data;
using Mono.Data.Sqlite;

namespace ServiceStack.OrmLite.Sqlite
{
    public class SqliteOrmLiteDialectProvider : SqliteOrmLiteDialectProviderBase
	{
		public static SqliteOrmLiteDialectProvider Instance = new SqliteOrmLiteDialectProvider();

		protected override IDbConnection CreateConnection(string connectionString)
		{
			return new SqliteConnection(connectionString);
		}
	}
}