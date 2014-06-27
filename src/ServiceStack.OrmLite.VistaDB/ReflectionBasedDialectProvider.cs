using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace ServiceStack.OrmLite.VistaDB
{
    public abstract class ReflectionBasedDialectProvider<T> : OrmLiteDialectProviderBase<T>
        where T: IOrmLiteDialectProvider
    {
        private Lazy<Type> _connectionType;

        private AssemblyName _assemblyGacName;
        private AssemblyName _assemblyLocalName;
        private string _providerTypeName;

        protected ReflectionBasedDialectProvider()
        {
            _connectionType = new Lazy<Type>(LoadAssemblyAndGetType);
        }

        protected abstract AssemblyName DefaultAssemblyGacName { get; }

        protected abstract AssemblyName DefaultAssemblyLocalName { get; }

        protected abstract string DefaultProviderTypeName { get; }

        protected Type ConnectionType { get { return _connectionType.Value; } }

        public bool UseLibraryFromGac { get; set; }

        public AssemblyName AssemblyGacName 
        {
            get
            {
                if (_assemblyGacName == null)
                    _assemblyGacName = DefaultAssemblyGacName;

                return _assemblyGacName;
            }
            set
            {
                SetValueSafe(value, v => _assemblyGacName = v);
            }
        }

        public AssemblyName AssemblyLocalName 
        {
            get 
            {
                if (_assemblyLocalName == null)
                    _assemblyLocalName = DefaultAssemblyLocalName;

                return _assemblyLocalName;
            }
            set
            {
                SetValueSafe(value, v => _assemblyLocalName = v);
            }
        }

        public string ProviderTypeName 
        {
            get
            {
                if (_providerTypeName == null)
                    _providerTypeName = DefaultProviderTypeName;

                return _providerTypeName;
            }
            set
            {
                SetValueSafe(value, v => _providerTypeName = v);
            } 
        }

        protected Type LoadAssemblyAndGetType()
        {
            var assemblyName = this.UseLibraryFromGac
                ? this.AssemblyGacName
                : this.AssemblyLocalName;

            var assembly = Assembly.Load(assemblyName);

            return assembly.GetType(this.ProviderTypeName, true);
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

        private static void ThrowUnableToChangeProperty()
        {
            throw new InvalidOperationException("This property should only be set before the first time a connection is created");
        }

        private void SetValueSafe<TValue>(TValue value, Action<TValue> action)
            where TValue: class
        {
            if (_connectionType.IsValueCreated)
                ThrowUnableToChangeProperty();

            action.Invoke(value);
        }
    }
}
