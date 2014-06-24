using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;

namespace ServiceStack.OrmLite.VistaDB
{
    public class OrmLiteVistaDbParameterCollection : IDataParameterCollection
    {
        private readonly OrderedDictionary parameters;

        public IDataParameterCollection VistaDbParameterCollection { get; private set; }

        public OrmLiteVistaDbParameterCollection(IDataParameterCollection vistaDbParameterCollection)
        {
            if (vistaDbParameterCollection == null)
                throw new ArgumentNullException("vistaDbParameterCollection");

            this.VistaDbParameterCollection = vistaDbParameterCollection;

            parameters = new OrderedDictionary(StringComparer.InvariantCultureIgnoreCase);
        }

        public bool Contains(string parameterName)
        {
            return this.VistaDbParameterCollection.Contains(parameterName);
        }

        public int IndexOf(string parameterName)
        {
            return this.VistaDbParameterCollection.IndexOf(parameterName);
        }

        public void RemoveAt(string parameterName)
        {
            this.VistaDbParameterCollection.RemoveAt(parameterName);
            parameters.Remove(parameterName);
        }

        public object this[string parameterName]
        {
            get
            {
                return parameters[parameterName];
            }
            set
            {
                var parameter = (OrmLiteVistaDbParameter)value;

                this.VistaDbParameterCollection[parameterName] = parameter.VistaDbParameter;
                parameters[parameterName] = parameter;
            }
        }

        public int Add(object value)
        {
            var parameter = (OrmLiteVistaDbParameter)value;

            parameters[parameter.ParameterName] = parameter;

            return this.VistaDbParameterCollection.Add(parameter.VistaDbParameter);
        }

        public void Clear()
        {
            parameters.Clear();

            this.VistaDbParameterCollection.Clear();
        }

        public bool Contains(object value)
        {
            var parameter = (OrmLiteVistaDbParameter)value;

            return this.VistaDbParameterCollection.Contains(parameter.VistaDbParameter);
        }

        public int IndexOf(object value)
        {
            var parameter = (OrmLiteVistaDbParameter)value;

            return this.VistaDbParameterCollection.IndexOf(parameter.VistaDbParameter);
        }

        public void Insert(int index, object value)
        {
            var parameter = (OrmLiteVistaDbParameter)value;

            parameters.Insert(index, parameter.ParameterName, parameter);
            this.VistaDbParameterCollection.Insert(index, parameter.VistaDbParameter);
        }

        public bool IsFixedSize { get { return this.VistaDbParameterCollection.IsFixedSize; } }

        public bool IsReadOnly { get { return this.VistaDbParameterCollection.IsReadOnly; } }

        public void Remove(object value)
        {
            var parameter = (OrmLiteVistaDbParameter)value;

            parameters.Remove(parameter);
            this.VistaDbParameterCollection.Remove(parameter.VistaDbParameter);
        }

        public void RemoveAt(int index)
        {
            parameters.RemoveAt(index);

            this.VistaDbParameterCollection.RemoveAt(index);
        }

        public object this[int index]
        {
            get
            {
                return parameters[index];
            }
            set
            {
                var parameter = (OrmLiteVistaDbParameter)value;

                parameters[index] = parameter;
                this.VistaDbParameterCollection[index] = parameter.VistaDbParameter;
            }
        }

        public void CopyTo(Array array, int index)
        {
            this.VistaDbParameterCollection.CopyTo(array, index);
        }

        public int Count { get { return this.VistaDbParameterCollection.Count; } }

        public bool IsSynchronized { get { return this.VistaDbParameterCollection.IsSynchronized; } }

        public object SyncRoot { get { return this.VistaDbParameterCollection.SyncRoot; } }

        public IEnumerator GetEnumerator()
        {
            return parameters.Values.GetEnumerator();
        }
    }
}
