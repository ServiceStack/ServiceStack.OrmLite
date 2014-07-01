using System;
using System.Data;

namespace ServiceStack.OrmLite.VistaDB
{
    public class OrmLiteVistaDbParameter : IDbDataParameter
    {
        private object _backgroundValue;

        public IDbDataParameter VistaDbParameter { get; private set; }

        internal OrmLiteVistaDbParameter(IDbDataParameter vistaDbParameter)
        {
            if (vistaDbParameter == null)
                throw new ArgumentNullException("vistaDbParameter");

            this.VistaDbParameter = vistaDbParameter;

            _backgroundValue = vistaDbParameter.Value;
        }

        public byte Precision
        {
            get { return this.VistaDbParameter.Precision; }
            set { this.VistaDbParameter.Precision = value; }
        }

        public byte Scale
        {
            get { return this.VistaDbParameter.Scale; }
            set { this.VistaDbParameter.Scale = value; }
        }

        public int Size
        {
            get { return this.VistaDbParameter.Size; }
            set { this.VistaDbParameter.Size = value; }
        }

        public DbType DbType
        {
            get { return this.VistaDbParameter.DbType; }
            set { this.VistaDbParameter.DbType = value; }
        }

        public ParameterDirection Direction
        {
            get { return this.VistaDbParameter.Direction; }
            set { this.VistaDbParameter.Direction = value; }
        }

        public bool IsNullable { get { return this.VistaDbParameter.IsNullable; } }

        public string ParameterName
        {
            get { return this.VistaDbParameter.ParameterName; }
            set { this.VistaDbParameter.ParameterName = value; }
        }

        public string SourceColumn
        {
            get { return this.VistaDbParameter.SourceColumn; }
            set { this.VistaDbParameter.SourceColumn = value; }
        }

        public DataRowVersion SourceVersion
        {
            get { return this.VistaDbParameter.SourceVersion; }
            set { this.VistaDbParameter.SourceVersion = value; }
        }

        public object Value
        {
            get { return _backgroundValue; }
            set 
            {
                _backgroundValue = value;

                var vistaDbValue = value; ;
                if (vistaDbValue != null && vistaDbValue.GetType().IsEnum)
                    vistaDbValue = vistaDbValue.ToString();
 
                this.VistaDbParameter.Value = vistaDbValue; 
            }
        }
    }
}
