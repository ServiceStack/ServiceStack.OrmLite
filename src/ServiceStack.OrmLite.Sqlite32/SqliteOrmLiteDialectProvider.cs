using System.Data;
using System.Data.SQLite;

namespace ServiceStack.OrmLite.Sqlite
{
    public class SqliteOrmLiteDialectProvider : SqliteOrmLiteDialectProviderBase
    {
        // what's the purpose of this Instance field? (It's like a pseudo-wanna-be singleton?)
        public static SqliteOrmLiteDialectProvider Instance = new SqliteOrmLiteDialectProvider();

        protected override IDbConnection CreateConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString, parseViaFramework: ParseViaFramework);
        }

        public SqliteOrmLiteDialectProvider WithPassword(string password)
        {
            Password = password;
            return Instance;
        }

        public SqliteOrmLiteDialectProvider WithUTFEncoding()
        {
            UTF8Encoded = true;
            return Instance;
        }
    }
}