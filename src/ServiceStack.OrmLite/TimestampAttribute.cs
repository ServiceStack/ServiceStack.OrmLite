using System;

namespace ServiceStack.DataAnnotations
{
    /// <summary>
    /// TimestampAttribute
    /// Use to indicate that a property is is used as Row-Version
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TimestampAttribute : Attribute
    {

    }
}