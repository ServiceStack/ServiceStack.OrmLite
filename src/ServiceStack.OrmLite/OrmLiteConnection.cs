using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ServiceStack.DataAccess;

namespace ServiceStack.OrmLite
{
	/// <summary>
	/// Wrapper IDbConnection class to allow for connection sharing, mocking, etc.
	/// </summary>
	public class OrmLiteConnection
		: IDbConnection, IHasDbConnection 
	{
	    public readonly OrmLiteConnectionFactory Factory;
		private IDbConnection dbConnection;
		private bool isOpen;

        public OrmLiteConnection(OrmLiteConnectionFactory factory)
        {
            this.Factory = factory;
        }

        public IDbConnection DbConnection
		{
			get
			{
				if (dbConnection == null)
				{
					dbConnection = Factory.ConnectionString.ToDbConnection(Factory.DialectProvider);
				}
				return dbConnection;
			}
		}

		public void Dispose()
		{
            if (Factory.OnDispose != null) Factory.OnDispose(this);
            if (!Factory.AutoDisposeConnection) return;

			DbConnection.Dispose();
			dbConnection = null;
			isOpen = false;
        }

		public IDbTransaction BeginTransaction()
		{
			if (Factory.AlwaysReturnTransaction != null)
				return Factory.AlwaysReturnTransaction;

			return DbConnection.BeginTransaction();
		}

		public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
		{
			if (Factory.AlwaysReturnTransaction != null)
				return Factory.AlwaysReturnTransaction;

			return DbConnection.BeginTransaction(isolationLevel);
		}

		public void Close()
		{
            DbConnection.Close();
        }

		public void ChangeDatabase(string databaseName)
		{
			DbConnection.ChangeDatabase(databaseName);
		}

		public IDbCommand CreateCommand()
		{
			if (Factory.AlwaysReturnCommand != null)
				return Factory.AlwaysReturnCommand;

			return DbConnection.CreateCommand();
		}

		public void Open()
		{
			if (isOpen) return;
			
			DbConnection.Open();
			isOpen = true;
		}

		public string ConnectionString
		{
			get { return Factory.ConnectionString; }
			set { Factory.ConnectionString = value; }
		}

		public int ConnectionTimeout
		{
			get { return DbConnection.ConnectionTimeout; }
		}

		public string Database
		{
			get { return DbConnection.Database; }
		}

		public ConnectionState State
		{
			get { return DbConnection.State; }
		}

		public static explicit operator SqlConnection(OrmLiteConnection dbConn)
		{
			return (SqlConnection)dbConn.DbConnection;
		}

		public static explicit operator DbConnection(OrmLiteConnection dbConn)
		{
			return (DbConnection)dbConn.DbConnection;
		}
	}
}