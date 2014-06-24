using System;
using System.Data;

namespace ServiceStack.OrmLite.VistaDB
{
    public class OrmLiteVistaDbCommand : IDbCommand
    {
        public IDbCommand VistaDbCommand { get; private set; }

        private OrmLiteVistaDbConnection connectionWrapper;
        private OrmLiteVistaDbTransaction transactionWrapper;

        public OrmLiteVistaDbCommand(OrmLiteVistaDbConnection connectionWrapper, IDbCommand vistaDbCommand)
        {
            if (connectionWrapper == null)
                throw new ArgumentNullException("connectionWrapper");

            if (vistaDbCommand == null)
                throw new ArgumentNullException("vistaDbCommand");

            this.connectionWrapper = connectionWrapper;

            this.VistaDbCommand = vistaDbCommand;
            this.Parameters = new OrmLiteVistaDbParameterCollection(vistaDbCommand.Parameters);
        }

        public void Cancel()
        {
            this.VistaDbCommand.Cancel();
        }

        public string CommandText
        {
            get { return this.VistaDbCommand.CommandText; }
            set { this.VistaDbCommand.CommandText = value; }
        }

        public int CommandTimeout
        {
            get { return this.VistaDbCommand.CommandTimeout; }
            set { this.VistaDbCommand.CommandTimeout = value; }
        }

        public CommandType CommandType
        {
            get { return this.VistaDbCommand.CommandType; }
            set { this.VistaDbCommand.CommandType = value; }
        }

        public IDbConnection Connection
        {
            get { return connectionWrapper; }
            set
            {
                connectionWrapper = (OrmLiteVistaDbConnection)value;
                this.VistaDbCommand.Connection = connectionWrapper.VistaDbConnection;
            }
        }

        public IDbDataParameter CreateParameter()
        {
            var vistaDbParameter = this.VistaDbCommand.CreateParameter();

            return new OrmLiteVistaDbParameter(vistaDbParameter);
        }

        public int ExecuteNonQuery()
        {
            return this.VistaDbCommand.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return this.VistaDbCommand.ExecuteReader(behavior);
        }

        public IDataReader ExecuteReader()
        {
            return this.VistaDbCommand.ExecuteReader();
        }

        public object ExecuteScalar()
        {
            return this.VistaDbCommand.ExecuteScalar();
        }

        public IDataParameterCollection Parameters { get; private set; }

        public void Prepare()
        {
            this.VistaDbCommand.Prepare();
        }

        public IDbTransaction Transaction
        {
            get
            {
                return transactionWrapper;
            }
            set
            {
                transactionWrapper = (OrmLiteVistaDbTransaction)value;
                
                if (transactionWrapper != null)
                    this.VistaDbCommand.Transaction = transactionWrapper.VistaDbTransaction;
            }
        }

        public UpdateRowSource UpdatedRowSource
        {
            get { return this.VistaDbCommand.UpdatedRowSource; }
            set { this.VistaDbCommand.UpdatedRowSource = value; }
        }

        public void Dispose()
        {
            this.VistaDbCommand.Dispose();
        }
    }
}
