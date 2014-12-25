using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
    public class OrmLiteSPStatement : IDisposable
    {
        private readonly IDbConnection db;
        private readonly IDbCommand dbCmd;
        private readonly IOrmLiteDialectProvider dialectProvider;

        public OrmLiteSPStatement(IDbCommand dbCmd)
            : this(null, dbCmd) {}

        public OrmLiteSPStatement(IDbConnection db, IDbCommand dbCmd)
        {
            this.db = db;
            this.dbCmd = dbCmd;
            dialectProvider = dbCmd.GetDialectProvider();
        }

        public List<T> ConvertToList<T>()
        {
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                throw new Exception("Type " + typeof(T).Name + " is a primitive type. Use ConvertScalarToList function.");

            IDataReader reader = null;
            try
            {
                reader = dbCmd.ExecuteReader();
                return reader.ConvertToList<T>(dialectProvider);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public List<T> ConvertToScalarList<T>()
        {
            if (!((typeof(T).IsPrimitive) || (typeof(T) == typeof(string)) || (typeof(T) == typeof(String))))
                throw new Exception("Type " + typeof(T).Name + " is a non primitive type. Use ConvertToList function.");

            IDataReader reader = null;
            try
            {
                reader = dbCmd.ExecuteReader();
                return reader.Column<T>(dialectProvider);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public T ConvertTo<T>()
        {
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                throw new Exception("Type " + typeof(T).Name + " is a primitive type. Use ConvertScalarTo function.");

            IDataReader reader = null;
            try
            {
                reader = dbCmd.ExecuteReader();
                return reader.ConvertTo<T>(dialectProvider);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public T ConvertToScalar<T>()
        {
            if (!((typeof(T).IsPrimitive) || (typeof(T) == typeof(string)) || (typeof(T) == typeof(String))))
                throw new Exception("Type " + typeof(T).Name + " is a non primitive type. Use ConvertTo function.");

            IDataReader reader = null;
            try
            {
                reader = dbCmd.ExecuteReader();
                return reader.Scalar<T>(dialectProvider);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public List<T> ConvertFirstColumnToList<T>()
        {
            if (!((typeof(T).IsPrimitive) || (typeof(T) == typeof(string)) || (typeof(T) == typeof(String))))
                throw new Exception("Type " + typeof(T).Name + " is a non primitive type. Only primitive type can be used.");

            IDataReader reader = null;
            try
            {
                reader = dbCmd.ExecuteReader();
                return reader.Column<T>(dialectProvider);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public HashSet<T> ConvertFirstColumnToListDistinct<T>()
        {
            if (!((typeof(T).IsPrimitive) || (typeof(T) == typeof(string)) || (typeof(T) == typeof(String))))
                throw new Exception("Type " + typeof(T).Name + " is a non primitive type. Only primitive type can be used.");

            IDataReader reader = null;
            try
            {
                reader = dbCmd.ExecuteReader();
                return reader.ColumnDistinct<T>(dialectProvider);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public int ExecuteNonQuery()
        {
            return dbCmd.ExecuteNonQuery();
        }

        public bool HasResult()
        {
            IDataReader reader = null;
            try
            {
                reader = dbCmd.ExecuteReader();
                if (reader.Read())
                    return true;
                else
                    return false;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public void Dispose()
        {
            dialectProvider.GetExecFilter().DisposeCommand(this.dbCmd, this.db);
        }
    }
}
