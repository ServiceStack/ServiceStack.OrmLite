using System.Data;
using System.Data.Common;
using NUnit.Framework;
using ServiceStack.MiniProfiler;
using ServiceStack.MiniProfiler.Data;
using ServiceStack.OrmLite.Oracle;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class WrappedCommandTests : OrmLiteTestBase
    {
        [Test]
        public void WrappingWithMiniProfilerSucceeds()
        {
            var factory = new OrmLiteConnectionFactory(ConnectionString, OracleDialect.Provider)
            {
                ConnectionFilter = x => new ProfiledDbConnection(x, Profiler.Current)
            };

            using (var db = factory.OpenDbConnection())
            {
                DoInsertUpdate(db);
            }
        }

        private static void DoInsertUpdate(IDbConnection db)
        {
            db.CreateTable<ParamPassword>(true);

            var row = new ParamPassword { Id = 2, Password = 6 };
            db.Insert(row);

            row.Password = 335;
            db.Update(row);
        }

        [Test]
        public void WrappingWithSpecializedMiniProfilerSucceeds()
        {
            var factory = new OrmLiteConnectionFactory(ConnectionString, OracleDialect.Provider)
            {
                ConnectionFilter = x => new SpecializedProfiledDbConnection(x, Profiler.Current)
            };

            using (var db = factory.OpenDbConnection())
            {
                DoInsertUpdate(db);
            }
        }
    }

    public class ParamPassword
    {
        public int Id { get; set; }
        public int Password { get; set; }
    }

    public class SpecializedProfiledDbConnection : ProfiledDbConnection
    {
        public SpecializedProfiledDbConnection(DbConnection connection, IDbProfiler profiler, bool autoDisposeConnection = true)
            : base(connection, profiler, autoDisposeConnection)
        { }

        public SpecializedProfiledDbConnection(IDbConnection connection, IDbProfiler profiler, bool autoDisposeConnection = true)
            : base(connection, profiler, autoDisposeConnection)
        { }

        protected override DbCommand CreateDbCommand()
        {
            return new SpecializedProfiledDbCommand(InnerConnection.CreateCommand(), InnerConnection, Profiler);
        }
    }

    public class SpecializedProfiledDbCommand : ProfiledDbCommand
    {
        public SpecializedProfiledDbCommand(DbCommand cmd, DbConnection conn, IDbProfiler profiler)
            : base(cmd, conn, profiler)
        { }
    }
}
