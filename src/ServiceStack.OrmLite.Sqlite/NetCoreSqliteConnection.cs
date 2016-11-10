#if NETSTANDARD1_3
using System.Data;
using Microsoft.Data.Sqlite;

namespace ServiceStack.OrmLite.Sqlite
{
    public class NetCoreSqliteConnection : SqliteConnection
    {
        public NetCoreSqliteConnection(string connectionString) 
            : base(connectionString) {}

        public override SqliteTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            //.NET Core Sqlite does not support IsolationLevel.ReadCommited
            if (isolationLevel == IsolationLevel.ReadCommitted)
                isolationLevel = IsolationLevel.Serializable;

            return base.BeginTransaction(isolationLevel);
        }
    }
}
#endif