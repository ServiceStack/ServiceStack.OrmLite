using System.Data;
#if NETSTANDARD1_3
using Microsoft.Data.Sqlite;
#else
using Mono.Data.Sqlite;
#endif

namespace ServiceStack.OrmLite.Sqlite
{
    public class SqliteOrmLiteDialectProvider : SqliteOrmLiteDialectProviderBase
    {
        public static SqliteOrmLiteDialectProvider Instance = new SqliteOrmLiteDialectProvider();

        protected override IDbConnection CreateConnection(string connectionString)
        {
#if NETSTANDARD1_3
            return new NetCoreSqliteConnection(connectionString);
#else
            return new SqliteConnection(connectionString);
#endif
        }

        public override IDbDataParameter CreateParam()
        {
            return new SqliteParameter();
        }
    }
}