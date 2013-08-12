
using System;

namespace ServiceStack.DataAnnotations
{
	
	/// <summary>
	/// IgnoreAttribute
	/// Use to indicate that a property is not a field  in the table
	/// properties with this attribute are ignored when building sql sentences
	/// </summary>
	
	
	[AttributeUsage( AttributeTargets.Property)]
	public class IgnoreAttribute : Attribute
	{
        public bool AlwaysIgnore { get; set; }


        /// <summary>
        /// Ignores field from SQL Table creation
        /// </summary>
        public IgnoreAttribute()
        {

        }


        /// <summary>
        /// Ignores field from SQL Table creation
        /// </summary>
        /// <param name="alwaysIgnore">Dont try to fetch from table</param>
        public IgnoreAttribute(bool alwaysIgnore)
        {
            AlwaysIgnore = alwaysIgnore;
        }
	}
}