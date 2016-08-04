//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2013 Service Stack LLC. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.DataAnnotations;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public class ModelDefinition
    {
        public ModelDefinition()
        {
            this.FieldDefinitions = new List<FieldDefinition>();
            this.IgnoredFieldDefinitions = new List<FieldDefinition>();
            this.CompositeIndexes = new List<CompositeIndexAttribute>();
        }

        public const string RowVersionName = "RowVersion";

        public string Name { get; set; }

        public string Alias { get; set; }

        public string Schema { get; set; }

        public string PreCreateTableSql { get; set; }

        public string PostCreateTableSql { get; set; }

        public string PreDropTableSql { get; set; }

        public string PostDropTableSql { get; set; }

        public bool IsInSchema { get { return this.Schema != null; } }

        public bool HasAutoIncrementId
        {
            get { return PrimaryKey != null && PrimaryKey.AutoIncrement; }
        }

        public FieldDefinition RowVersion { get; set; }

        public string ModelName
        {
            get { return this.Alias ?? this.Name; }
        }

        public Type ModelType { get; set; }

        public FieldDefinition PrimaryKey
        {
            get { return this.FieldDefinitions.First(x => x.IsPrimaryKey); }
        }

        public object GetPrimaryKey(object instance)
        {
            var pk = PrimaryKey;
            return pk != null
                ? pk.GetValue(instance)
                : instance.GetId();
        }

        public List<FieldDefinition> FieldDefinitions { get; set; }

        private FieldDefinition[] fieldDefinitionsArray;
        public FieldDefinition[] FieldDefinitionsArray
        {
            get { return fieldDefinitionsArray; }
        }

        private FieldDefinition[] fieldDefinitionsWithAliases;
        public FieldDefinition[] FieldDefinitionsWithAliases
        {
            get { return fieldDefinitionsWithAliases; }
        }

        public List<FieldDefinition> IgnoredFieldDefinitions { get; set; }

        private FieldDefinition[] ignoredFieldDefinitionsArray;
        public FieldDefinition[] IgnoredFieldDefinitionsArray
        {
            get { return ignoredFieldDefinitionsArray; }
        }

        private FieldDefinition[] allFieldDefinitionsArray;
        public FieldDefinition[] AllFieldDefinitionsArray
        {
            get { return allFieldDefinitionsArray; }
        }

        private readonly object fieldDefLock = new object();
        private Dictionary<string, FieldDefinition> fieldDefinitionMap;
        private Func<string, string> fieldNameSanitizer;

        public Dictionary<string, FieldDefinition> GetFieldDefinitionMap(Func<string, string> sanitizeFieldName)
        {
            lock (fieldDefLock)
            {
                if (fieldDefinitionMap != null && fieldNameSanitizer == sanitizeFieldName) 
                    return fieldDefinitionMap;
                
                fieldDefinitionMap = new Dictionary<string, FieldDefinition>(StringComparer.OrdinalIgnoreCase);
                fieldNameSanitizer = sanitizeFieldName;
                foreach (var fieldDef in FieldDefinitionsArray)
                {
                    fieldDefinitionMap[sanitizeFieldName(fieldDef.FieldName)] = fieldDef;
                }
                return fieldDefinitionMap;
            }
        }

        public List<CompositeIndexAttribute> CompositeIndexes { get; set; }

        public FieldDefinition GetFieldDefinition<T>(Expression<Func<T, object>> field)
        {
            return GetFieldDefinition(ExpressionUtils.GetMemberName(field));
        }

        public FieldDefinition GetFieldDefinition(string fieldName)
        {
            if (fieldName != null)
            {
                foreach (var f in FieldDefinitionsWithAliases)
                {
                    if (f.Alias == fieldName)
                        return f;
                }
                foreach (var f in FieldDefinitionsArray)
                {
                    if (f.Name == fieldName)
                        return f;
                }
                foreach (var f in FieldDefinitionsWithAliases)
                {
                    if (string.Equals(f.Alias, fieldName, StringComparison.OrdinalIgnoreCase))
                        return f;
                }
                foreach (var f in FieldDefinitionsArray)
                {
                    if (string.Equals(f.Name, fieldName, StringComparison.OrdinalIgnoreCase))
                        return f;
                }
            }
            return null;
        }

        public void AfterInit()
        {
            fieldDefinitionsArray = FieldDefinitions.ToArray();
            fieldDefinitionsWithAliases = FieldDefinitions.Where(x => x.Alias != null).ToArray();

            ignoredFieldDefinitionsArray = IgnoredFieldDefinitions.ToArray();

            var allItems = new List<FieldDefinition>(FieldDefinitions);
            allItems.AddRange(IgnoredFieldDefinitions);
            allFieldDefinitionsArray = allItems.ToArray();
        }

        public bool IsRefField(FieldDefinition fieldDef)
        {
            return (fieldDef.Alias != null && IsRefField(fieldDef.Alias))
                    || IsRefField(fieldDef.Name);
        }

        private bool IsRefField(string name)
        {
            return (Alias != null && Alias + "Id" == name)
                    || Name + "Id" == name;
        }
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