using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ServiceStack.OrmLite.VistaDB
{
    public abstract class ReflectionBasedDialectProvider<T> : OrmLiteDialectProviderBase<T>
        where T: IOrmLiteDialectProvider
    {
        protected Type ConnectionType { get; private set; }

        protected ReflectionBasedDialectProvider(Type connectionType)
        {
            if (connectionType == null)
                throw new ArgumentNullException("connectionType");

            if (!typeof(IDbConnection).IsAssignableFrom(connectionType))
                throw new ArgumentException("Invalid connectionType");

            this.ConnectionType = connectionType;
        }

        protected ReflectionBasedDialectProvider(AssemblyName assemblyName, string typeName)
            : this (LoadAssemblyAndGetType(assemblyName, typeName))
        {}

        protected ReflectionBasedDialectProvider(string assemblyName, string typeName)
            : this (new AssemblyName(assemblyName), typeName)
        {}

        protected static Type LoadAssemblyAndGetType(AssemblyName assemblyName, string typeName)
        {
            var assembly = Assembly.Load(assemblyName);
            return assembly.GetType(typeName, true);
        }

        protected virtual IDbConnection ActivateDbConnection(string connectionString)
        {
            var conn = Activator.CreateInstance(this.ConnectionType) as IDbConnection;
            conn.ConnectionString = connectionString;
            
            return conn;
        }
        
        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            return this.ActivateDbConnection(connectionString);
        }
    }
}
