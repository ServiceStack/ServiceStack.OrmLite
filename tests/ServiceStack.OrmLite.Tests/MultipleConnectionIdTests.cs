using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class MultipleConnectionIdTests : OrmLiteTestBase
    {
        private int _waitingThreadCount;
        private int _waitingThreadsReleasedCounter;

        [SetUp]
        public void SetUp()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<MultipleConnection>();
            }
        }

        [Test]
        public void TwoSimultaneousInsertsGetDifferentIds()
        {
            if (Dialect == Dialect.Sqlite) return; // Sqlite doesn't support concurrent writers

            var dataArray = new[]
            {
                new MultipleConnection {Data = "one"},
                new MultipleConnection {Data = "two"}
            };

            var originalExecFilter = OrmLiteConfig.ExecFilter;
            try
            {
                OrmLiteConfig.ExecFilter = new PostExecuteActionExecFilter(originalExecFilter, cmd => PauseForOtherThreadsAfterInserts(cmd, 2));

                Parallel.ForEach(dataArray, data =>
                {
                    using (var db = OpenDbConnection())
                    {
                        data.Id = db.Insert(new MultipleConnection {Data = data.Data}, selectIdentity: true);

                        Assert.That(data.Id, Is.Not.EqualTo(0));
                    }
                });
            }
            finally
            {
                OrmLiteConfig.ExecFilter = originalExecFilter;
            }

            Assert.That(dataArray[1].Id, Is.Not.EqualTo(dataArray[0].Id));
        }

        private void PauseForOtherThreadsAfterInserts(IDbCommand cmd, int numberOfThreads)
        {
            if (!cmd.CommandText.StartsWith("INSERT ", StringComparison.InvariantCultureIgnoreCase))
                return;

            var initialReleasedCounter = _waitingThreadsReleasedCounter;
            Interlocked.Increment(ref _waitingThreadCount);
            try
            {
                var waitUntil = DateTime.UtcNow.AddSeconds(2);
                while ((_waitingThreadCount < numberOfThreads) && (initialReleasedCounter == _waitingThreadsReleasedCounter))
                {
                    if (DateTime.UtcNow >= waitUntil)
                        throw new Exception("There were not enough waiting threads after timeout");
                    Thread.Sleep(1);
                }

                Interlocked.Increment(ref _waitingThreadsReleasedCounter);
            }
            finally
            {
                Interlocked.Decrement(ref _waitingThreadCount);
            }
        }

        [Test]
        public void TwoSimultaneousSavesGetDifferentIds()
        {
            if (Dialect == Dialect.Sqlite) return; // Sqlite doesn't support concurrent writers

            var dataArray = new[]
            {
                new MultipleConnection {Data = "one"},
                new MultipleConnection {Data = "two"}
            };

            var originalExecFilter = OrmLiteConfig.ExecFilter;
            try
            {
                OrmLiteConfig.ExecFilter = new PostExecuteActionExecFilter(originalExecFilter, cmd => PauseForOtherThreadsAfterInserts(cmd, 2));

                Parallel.ForEach(dataArray, data =>
                {
                    using (var db = OpenDbConnection())
                    {
                        db.Save(data);

                        Assert.That(data.Id, Is.Not.EqualTo(0));
                    }
                });
            }
            finally
            {
                OrmLiteConfig.ExecFilter = originalExecFilter;
            }

            Assert.That(dataArray[1].Id, Is.Not.EqualTo(dataArray[0].Id));
        }

        private class PostExecuteActionExecFilter : IOrmLiteExecFilter
        {
            private readonly IOrmLiteExecFilter _inner;
            private readonly Action<IDbCommand> _postExecuteAction;

            public PostExecuteActionExecFilter(IOrmLiteExecFilter inner, Action<IDbCommand> postExecuteAction)
            {
                _inner = inner;
                _postExecuteAction = postExecuteAction;
            }

            public SqlExpression<T> SqlExpression<T>(IDbConnection dbConn)
            {
                return _inner.SqlExpression<T>(dbConn);
            }

            public IDbCommand CreateCommand(IDbConnection dbConn)
            {
                var innerCommand = _inner.CreateCommand(dbConn);
                return new PostExcuteActionCommand(innerCommand, _postExecuteAction);
            }

            public void DisposeCommand(IDbCommand dbCmd, IDbConnection dbConn)
            {
                _inner.DisposeCommand(dbCmd, dbConn);
            }

            public T Exec<T>(IDbConnection dbConn, Func<IDbCommand, T> filter)
            {
                var cmd = CreateCommand(dbConn);
                try
                {
                    return filter(cmd);
                }
                finally
                {
                    DisposeCommand(cmd, dbConn);
                }
            }

            public async Task<T> Exec<T>(IDbConnection dbConn, Func<IDbCommand, Task<T>> filter)
            {
                var cmd = CreateCommand(dbConn);
                try
                {
                    return await filter(cmd);
                }
                finally
                {
                    DisposeCommand(cmd, dbConn);
                }
            }

            public void Exec(IDbConnection dbConn, Action<IDbCommand> filter)
            {
                var cmd = CreateCommand(dbConn);
                try
                {
                    filter(cmd);
                }
                finally
                {
                    DisposeCommand(cmd, dbConn);
                }
            }

            public async Task Exec(IDbConnection dbConn, Func<IDbCommand, Task> filter)
            {
                var cmd = CreateCommand(dbConn);
                try
                {
                    await filter(cmd);
                }
                finally
                {
                    DisposeCommand(cmd, dbConn);
                }
            }

            public IEnumerable<T> ExecLazy<T>(IDbConnection dbConn, Func<IDbCommand, IEnumerable<T>> filter)
            {
                var cmd = CreateCommand(dbConn);
                try
                {
                    var results = filter(cmd);

                    foreach (var item in results)
                    {
                        yield return item;
                    }
                }
                finally
                {
                    DisposeCommand(cmd, dbConn);
                }
            }
        }

        private class PostExcuteActionCommand : IDbCommand
        {
            private readonly IDbCommand _inner;
            private readonly Action<IDbCommand> _postExecuteAction;

            public PostExcuteActionCommand(IDbCommand inner, Action<IDbCommand> postExecuteAction)
            {
                _inner = inner;
                _postExecuteAction = postExecuteAction;
            }

            public void Dispose()
            {
                _inner.Dispose();
            }

            public void Prepare()
            {
                _inner.Prepare();
            }

            public void Cancel()
            {
                _inner.Cancel();
            }

            public IDbDataParameter CreateParameter()
            {
                return _inner.CreateParameter();
            }

            public int ExecuteNonQuery()
            {
                var result = _inner.ExecuteNonQuery();
                _postExecuteAction(this);
                return result;
            }

            public IDataReader ExecuteReader()
            {
                var result = _inner.ExecuteReader();
                _postExecuteAction(this);
                return result;
            }

            public IDataReader ExecuteReader(CommandBehavior behavior)
            {
                var result = _inner.ExecuteReader(behavior);
                _postExecuteAction(this);
                return result;
            }

            public object ExecuteScalar()
            {
                var result = _inner.ExecuteScalar();
                _postExecuteAction(this);
                return result;
            }

            public IDbConnection Connection
            {
                get { return _inner.Connection; }
                set { _inner.Connection = value; }
            }

            public IDbTransaction Transaction
            {
                get { return _inner.Transaction; }
                set { _inner.Transaction = value; }
            }

            public string CommandText
            {
                get { return _inner.CommandText; }
                set { _inner.CommandText = value; }
            }

            public int CommandTimeout
            {
                get { return _inner.CommandTimeout; }
                set { _inner.CommandTimeout = value; }
            }

            public CommandType CommandType
            {
                get { return _inner.CommandType; }
                set { _inner.CommandType = value; }
            }

            public IDataParameterCollection Parameters
            {
                get { return _inner.Parameters; }
            }

            public UpdateRowSource UpdatedRowSource
            {
                get { return _inner.UpdatedRowSource; }
                set { _inner.UpdatedRowSource = value; }
            }
        }
    }

    public class MultipleConnection
    {
        [AutoIncrement]
        public long Id { get; set; }

        public string Data { get; set; }
    }
}
