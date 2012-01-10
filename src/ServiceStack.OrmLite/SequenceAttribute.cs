using System;

namespace ServiceStack.DataAnnotations
{
	[AttributeUsage( AttributeTargets.Property)]
	/// <summary>
	/// Sequence attribute.
	/// Use in FirebirdSql. indicates name of generator for columns of type AutoIncrement
	// </summary>
	public class SequenceAttribute : Attribute
	{
		public string Name { get; set; }

		public SequenceAttribute(string name)
		{
			this.Name = name;
		}
	}
}