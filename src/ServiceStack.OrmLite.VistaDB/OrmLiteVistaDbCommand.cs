using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite.VistaDB
{
    public class OrmLiteVistaDbCommand : IDbCommand
    {
        public IDbCommand VistaDbCommand { get; private set; }

        private OrmLiteVistaDbConnection _connectionWrapper;
        private OrmLiteVistaDbTransaction _transactionWrapper;

        public OrmLiteVistaDbCommand(OrmLiteVistaDbConnection connectionWrapper, IDbCommand vistaDbCommand)
        {
            if (connectionWrapper == null)
                throw new ArgumentNullException("connectionWrapper");

            if (vistaDbCommand == null)
                throw new ArgumentNullException("vistaDbCommand");

            _connectionWrapper = connectionWrapper;

            this.VistaDbCommand = vistaDbCommand;
        }

        #region IDbCommand Members

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
            get { return _connectionWrapper; }
            set
            {
                _connectionWrapper = (OrmLiteVistaDbConnection)value;
                this.VistaDbCommand.Connection = _connectionWrapper.VistaDbConnection;
            }
        }

        public IDbDataParameter CreateParameter()
        {
            return this.VistaDbCommand.CreateParameter();
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

        public IDataParameterCollection Parameters { get { return this.VistaDbCommand.Parameters; } }

        public void Prepare()
        {
            this.VistaDbCommand.Prepare();
        }

        public IDbTransaction Transaction
        {
            get
            {
                return _transactionWrapper;
            }
            set
            {
                _transactionWrapper = (OrmLiteVistaDbTransaction)value;
                
                if (_transactionWrapper != null)
                    this.VistaDbCommand.Transaction = _transactionWrapper.VistaDbTransaction;
            }
        }

        public UpdateRowSource UpdatedRowSource
        {
            get { return this.VistaDbCommand.UpdatedRowSource; }
            set { this.VistaDbCommand.UpdatedRowSource = value; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.VistaDbCommand.Dispose();
        }

        #endregion
    }
}
