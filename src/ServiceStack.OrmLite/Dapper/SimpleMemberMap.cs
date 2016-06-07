using System;
using System.Reflection;

//Apache 2.0 License: https://github.com/StackExchange/dapper-dot-net/blob/master/License.txt
namespace ServiceStack.OrmLite.Dapper
{
    /// <summary>
    /// Represents simple member map for one of target parameter or property or field to source DataReader column
    /// </summary>
    sealed class SimpleMemberMap : SqlMapper.IMemberMap
    {
        /// <summary>
        /// Creates instance for simple property mapping
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <param name="property">Target property</param>
        public SimpleMemberMap(string columnName, PropertyInfo property)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            if (property == null)
                throw new ArgumentNullException("property");

            ColumnName = columnName;
            Property = property;
        }

        /// <summary>
        /// Creates instance for simple field mapping
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <param name="field">Target property</param>
        public SimpleMemberMap(string columnName, FieldInfo field)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            if (field == null)
                throw new ArgumentNullException("field");

            ColumnName = columnName;
            Field = field;
        }

        /// <summary>
        /// Creates instance for simple constructor parameter mapping
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <param name="parameter">Target constructor parameter</param>
        public SimpleMemberMap(string columnName, ParameterInfo parameter)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            if (parameter == null)
                throw new ArgumentNullException("parameter");

            ColumnName = columnName;
            Parameter = parameter;
        }

        /// <summary>
        /// DataReader column name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// Target member type
        /// </summary>
        public Type MemberType
        {
            get { return Field != null ? Field.FieldType : (Property != null ? Property.PropertyType : (Parameter != null ? Parameter.ParameterType : null)); }
        }

        /// <summary>
        /// Target property
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// Target field
        /// </summary>
        public FieldInfo Field { get; set; }

        /// <summary>
        /// Target constructor parameter
        /// </summary>
        public ParameterInfo Parameter { get; set; }
    }
}
