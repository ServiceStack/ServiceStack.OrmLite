using System;
using System.Data;

namespace ServiceStack.OrmLite
{
    public interface IOrmLiteConverter
    {
        IOrmLiteDialectProvider DialectProvider { get; set; }
        
        DbType DbType { get; }

        string ColumnDefinition { get; }

        string ToQuotedString(object value);

        object ToDbParamValue(FieldDefinition fieldDef, object value);

        object ToDbValue(FieldDefinition fieldDef, object value);

        object FromDbValue(FieldDefinition fieldDef, IDataReader reader, int columnIndex);
    }

    public abstract class OrmLiteConverter : IOrmLiteConverter
    {
        /// <summary>
        /// RDBMS Dialect this Converter is for. Injected at registration.
        /// </summary>
        public IOrmLiteDialectProvider DialectProvider { get; set; }

        /// <summary>
        /// SQL Column Definiton used in CREATE Table. 
        /// return null to use default String Column definition
        /// </summary>
        public virtual string ColumnDefinition
        {
            get { return null; }
        }

        /// <summary>
        /// Used in DB Params. Defaults to DbType.String
        /// </summary>
        public virtual DbType DbType
        {
            get { return DbType.String; }
        }

        /// <summary>
        /// Quoted Value in SQL Statement
        /// </summary>
        public virtual string ToQuotedString(object value)
        {
            return DialectProvider.GetQuotedValue(value.ToString());
        }

        /// <summary>
        /// Used in Parameterized Value. Optional, Defaults to ToDbValue()
        /// </summary>
        public virtual object ToDbParamValue(FieldDefinition fieldDef, object value)
        {
            return ToDbValue(fieldDef, value);
        }

        /// <summary>
        /// Value to Save in DB
        /// </summary>
        public virtual object ToDbValue(FieldDefinition fieldDef, object value)
        {
            return value;
        }

        /// <summary>
        /// Value from DB to Populate on POCO Data Model
        /// </summary>
        public virtual object FromDbValue(FieldDefinition fieldDef, IDataReader reader, int columnIndex)
        {
            return reader.GetValue(columnIndex);
        }
    }
}