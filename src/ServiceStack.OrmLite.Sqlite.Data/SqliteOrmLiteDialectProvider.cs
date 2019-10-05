using System.Data;
using Microsoft.Data.Sqlite;

namespace ServiceStack.OrmLite.Sqlite
{
    public class SqliteOrmLiteDialectProvider : SqliteOrmLiteDialectProviderBase
    {
        public static SqliteOrmLiteDialectProvider Instance = new SqliteOrmLiteDialectProvider();

        protected override IDbConnection CreateConnection(string connectionString)
        {
            return new SqliteConnection(connectionString);
        }

        public override IDbDataParameter CreateParam()
        {
            return new SqliteParameter();
        }
    }
}