using System;
using System.Data;
using SQLitePCL;

namespace ServiceStack.OrmLite.Sqlite
{
    //TODO: Find ADO.NET Wrapper for SQLitePCL 
    public class SqliteOrmLiteDialectProvider : SqliteOrmLiteDialectProviderBase
    {
        public static SqliteOrmLiteDialectProvider Instance = new();

        protected override IDbConnection CreateConnection(string connectionString)
        {
            // return new SqliteConnection(connectionString);
            throw new NotImplementedException();
        }

        public override IDbDataParameter CreateParam()
        {
            // return new SqliteParameter();
            throw new NotImplementedException();
        }
    }
}