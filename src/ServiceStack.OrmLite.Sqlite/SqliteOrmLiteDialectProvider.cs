using System.Data;
using Mono.Data.Sqlite;

namespace ServiceStack.OrmLite.Sqlite
{
    public class SqliteOrmLiteDialectProvider : SqliteOrmLiteDialectProviderBase
	{
		// what's the purpose of this Instance field? (It's like a pseudo-wanna-be singleton?)
		public static SqliteOrmLiteDialectProvider Instance = new SqliteOrmLiteDialectProvider();

		protected override IDbConnection CreateConnection(string connectionString)
		{
			return new SqliteConnection(connectionString);
		}
	}
}