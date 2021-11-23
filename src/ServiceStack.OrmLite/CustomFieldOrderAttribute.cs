using System;

namespace ServiceStack.OrmLite
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CustomFieldOrderAttribute : AttributeBase
    {
        public int Order { get; set; }
    }
}
