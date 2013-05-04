using System;

namespace ServiceStack.OrmLite.MySql.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TextAttribute : Attribute
    {
        public TextAttribute(MySqlTextType mySqlTextType = MySqlTextType.TEXT )
        {
            MySqlTextType = mySqlTextType;
        }

        public MySqlTextType MySqlTextType { get; set; }
    }


    public enum MySqlTextType
    {
        TEXT,
        TINYTEXT,
        MEDIUMTEXT,
        LONGTEXT
    }
}
