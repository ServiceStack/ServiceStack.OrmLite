using System.Data;

namespace ServiceStack.OrmLite
{
    public interface IHasDbCommand
    {
        IDbCommand DbCommand { get; }
    }

    public class OrmLiteCommand : IDbCommand, IHasDbCommand
    {
        private OrmLiteConnection dbConn;
        private IDbCommand dbCmd;
        public IOrmLiteDialectProvider DialectProvider;

        public OrmLiteCommand(OrmLiteConnection dbConn, IDbCommand dbCmd)
        {
            this.dbConn = dbConn;
            this.dbCmd = dbCmd;
            this.DialectProvider = dbConn.GetDialectProvider();
        }

        public void Dispose()
        {
            dbCmd.Dispose();
        }

        public void Prepare()
        {
            dbCmd.Prepare();
        }

        public void Cancel()
        {
            dbCmd.Cancel();
        }

        public IDbDataParameter CreateParameter()
        {
            return dbCmd.CreateParameter();
        }

        public int ExecuteNonQuery()
        {
            return dbCmd.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader()
        {
            return dbCmd.ExecuteReader();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return dbCmd.ExecuteReader(behavior);
        }

        public object ExecuteScalar()
        {
            return dbCmd.ExecuteScalar();
        }

        public IDbConnection Connection
        {
            get { return dbCmd.Connection; }
            set { dbCmd.Connection = value; }
        }
        public IDbTransaction Transaction
        {
            get { return dbCmd.Transaction; }
            set { dbCmd.Transaction = value; }
        }
        public string CommandText
        {
            get { return dbCmd.CommandText; }
            set { dbCmd.CommandText = value; }
        }
        public int CommandTimeout
        {
            get { return dbCmd.CommandTimeout; }
            set { dbCmd.CommandTimeout = value; }
        }
        public CommandType CommandType
        {
            get { return dbCmd.CommandType; }
            set { dbCmd.CommandType = value; }
        }
        public IDataParameterCollection Parameters
        {
            get { return dbCmd.Parameters; }
        }
        public UpdateRowSource UpdatedRowSource
        {
            get { return dbCmd.UpdatedRowSource; }
            set { dbCmd.UpdatedRowSource = value; }
        }

        public IDbCommand DbCommand
        {
            get { return dbCmd; }
        }
    }
}