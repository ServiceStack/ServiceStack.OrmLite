//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2010 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite
{
	public class ModelDefinition
	{
		public ModelDefinition()
		{
			this.FieldDefinitions = new List<FieldDefinition>();
			this.CompositeIndexes = new List<CompositeIndexAttribute>();
		}

		public string Name { get; set; }

		public string Alias { get; set; }

        public string Schema { get; set; }

        public bool IsInSchema { get { return this.Schema != null; } }

		public string ModelName
		{
			get { return this.Alias ?? this.Name; }
		}

		public Type ModelType { get; set; }

        public string SqlSelectAllFromTable { get; set; }

		public FieldDefinition PrimaryKey
		{
			get
			{
				return this.FieldDefinitions.First(x => x.IsPrimaryKey);
			}
		}

		public List<FieldDefinition> FieldDefinitions { get; set; }

		private FieldDefinition[] fieldDefinitionsArray;
		public FieldDefinition[] FieldDefinitionsArray
		{
			get
			{
				if (fieldDefinitionsArray == null)
				{
					fieldDefinitionsArray = FieldDefinitions.ToArray();
				}
				return fieldDefinitionsArray;
			}
		}

		public List<CompositeIndexAttribute> CompositeIndexes { get; set; }
	}


    public static class ModelDefinition<T>
    {
        private static ModelDefinition definition;
		public static ModelDefinition Definition
		{
			get { return definition ?? (definition = typeof(T).GetModelDefinition()); }
		}

		private static string primaryKeyName;
		public static string PrimaryKeyName
		{
			get { return primaryKeyName ?? (primaryKeyName = Definition.PrimaryKey.FieldName); }
		}

	}
}