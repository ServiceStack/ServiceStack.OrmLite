using System;

namespace ServiceStack.OrmLite
{
    /// <summary>
    /// Primary key name attribute.
    /// Use to indicate the template of the primary key name. It allows to use 2 format items: {0} - table name {1} - column name
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyNameAttribute : Attribute
    {
        public string Template { get; set; }

        public PrimaryKeyNameAttribute(string template)
        {
            this.Template = template;
        }
    }
}