using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite.VistaDB
{
    public class OrmLiteVistaDbParameterCollection : IDataParameterCollection
    {
        private OrderedDictionary _parameters;

        public IDataParameterCollection VistaDbParameterCollection { get; private set; }

        public OrmLiteVistaDbParameterCollection(IDataParameterCollection vistaDbParameterCollection)
        {
            if (vistaDbParameterCollection == null)
                throw new ArgumentNullException("vistaDbParameterCollection");

            this.VistaDbParameterCollection = vistaDbParameterCollection;

            _parameters = new OrderedDictionary(StringComparer.InvariantCultureIgnoreCase);
        }

        #region IDataParameterCollection Members

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
            _parameters.Remove(parameterName);
        }

        public object this[string parameterName]
        {
            get
            {
                return _parameters[parameterName];
            }
            set
            {
                var parameter = (OrmLiteVistaDbParameter)value;

                this.VistaDbParameterCollection[parameterName] = parameter.VistaDbParameter;
                _parameters[parameterName] = parameter;
            }
        }

        #endregion

        #region IList Members

        public int Add(object value)
        {
            var parameter = (OrmLiteVistaDbParameter)value;

            _parameters[parameter.ParameterName] = parameter;

            return this.VistaDbParameterCollection.Add(parameter.VistaDbParameter);
        }

        public void Clear()
        {
            _parameters.Clear();

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

            _parameters.Insert(index, parameter.ParameterName, parameter);
            this.VistaDbParameterCollection.Insert(index, parameter.VistaDbParameter);
        }

        public bool IsFixedSize { get { return this.VistaDbParameterCollection.IsFixedSize; } }

        public bool IsReadOnly { get { return this.VistaDbParameterCollection.IsReadOnly; } }

        public void Remove(object value)
        {
            var parameter = (OrmLiteVistaDbParameter)value;

            _parameters.Remove(parameter);
            this.VistaDbParameterCollection.Remove(parameter.VistaDbParameter);
        }

        public void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);

            this.VistaDbParameterCollection.RemoveAt(index);
        }

        public object this[int index]
        {
            get
            {
                return _parameters[index];
            }
            set
            {
                var parameter = (OrmLiteVistaDbParameter)value;

                _parameters[index] = parameter;
                this.VistaDbParameterCollection[index] = parameter.VistaDbParameter;
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            this.VistaDbParameterCollection.CopyTo(array, index);
        }

        public int Count { get { return this.VistaDbParameterCollection.Count; } }

        public bool IsSynchronized { get { return this.VistaDbParameterCollection.IsSynchronized; } }

        public object SyncRoot { get { return this.VistaDbParameterCollection.SyncRoot; } }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return _parameters.Values.GetEnumerator();
        }

        #endregion
    }
}
