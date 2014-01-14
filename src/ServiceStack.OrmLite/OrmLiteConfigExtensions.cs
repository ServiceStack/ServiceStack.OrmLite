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
using System.Reflection;
using System.Threading;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite
{
    internal static class OrmLiteConfigExtensions
    {
        private static Dictionary<Type, ModelDefinition> typeModelDefinitionMap = new Dictionary<Type, ModelDefinition>();

        private static bool IsNullableType(Type theType)
        {
            return (theType.IsGenericType
                && theType.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        internal static bool CheckForIdField(IEnumerable<PropertyInfo> objProperties)
        {
            // Not using Linq.Where() and manually iterating through objProperties just to avoid dependencies on System.Xml??
            foreach (var objProperty in objProperties)
            {
                if (objProperty.Name != OrmLiteConfig.IdField) continue;
                return true;
            }
            return false;
        }

        internal static void ClearCache()
        {
            typeModelDefinitionMap = new Dictionary<Type, ModelDefinition>();
        }

        public static ModelDefinition Init(this Type modelType)
        {
            return modelType.GetModelDefinition();
        }

        internal static ModelDefinition GetModelDefinition(this Type modelType)
        {
            ModelDefinition modelDef;

            if (typeModelDefinitionMap.TryGetValue(modelType, out modelDef))
                return modelDef;

            if (modelType.IsValueType() || modelType == typeof(string))
                return null;

            var modelAliasAttr = modelType.FirstAttribute<AliasAttribute>();
            var schemaAttr = modelType.FirstAttribute<SchemaAttribute>();
            modelDef = new ModelDefinition {
                ModelType = modelType,
                Name = modelType.Name,
                Alias = modelAliasAttr != null ? modelAliasAttr.Name : null,
                Schema = schemaAttr != null ? schemaAttr.Name : null
            };

            modelDef.CompositeIndexes.AddRange(
                modelType.GetCustomAttributes(typeof(CompositeIndexAttribute), true).ToList()
                .ConvertAll(x => (CompositeIndexAttribute)x));

            var objProperties = modelType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance).ToList();

            var hasIdField = CheckForIdField(objProperties);

            var i = 0;
            foreach (var propertyInfo in objProperties)
            {
                var sequenceAttr = propertyInfo.FirstAttribute<SequenceAttribute>();
                var computeAttr= propertyInfo.FirstAttribute<ComputeAttribute>();
                var decimalAttribute = propertyInfo.FirstAttribute<DecimalLengthAttribute>();
                var belongToAttribute = propertyInfo.FirstAttribute<BelongToAttribute>();
                var isFirst = i++ == 0;

                var isPrimaryKey = propertyInfo.Name == OrmLiteConfig.IdField || (!hasIdField && isFirst)
                    || propertyInfo.HasAttributeNamed(typeof(PrimaryKeyAttribute).Name);

                var isNullableType = IsNullableType(propertyInfo.PropertyType);

                var isNullable = (!propertyInfo.PropertyType.IsValueType
                                   && !propertyInfo.HasAttributeNamed(typeof(RequiredAttribute).Name))
                                   || isNullableType;

                var propertyType = isNullableType
                    ? Nullable.GetUnderlyingType(propertyInfo.PropertyType)
                    : propertyInfo.PropertyType;

                Type treatAsType = null;
                if (propertyType.IsEnum && propertyType.HasAttribute<FlagsAttribute>())
                {
                    treatAsType = Enum.GetUnderlyingType(propertyType);
                }

                var aliasAttr = propertyInfo.FirstAttribute<AliasAttribute>();

                var indexAttr = propertyInfo.FirstAttribute<IndexAttribute>();
                var isIndex = indexAttr != null;
                var isUnique = isIndex && indexAttr.Unique;

                var stringLengthAttr = propertyInfo.CalculateStringLength(decimalAttribute);

                var defaultValueAttr = propertyInfo.FirstAttribute<DefaultAttribute>();

                var referencesAttr = propertyInfo.FirstAttribute<ReferencesAttribute>();
                var referenceAttr  = propertyInfo.FirstAttribute<ReferenceAttribute>();
                var foreignKeyAttr = propertyInfo.FirstAttribute<ForeignKeyAttribute>();

                var fieldDefinition = new FieldDefinition {
                    Name = propertyInfo.Name,
                    Alias = aliasAttr != null ? aliasAttr.Name : null,
                    FieldType = propertyType,
                    TreatAsType = treatAsType,
                    PropertyInfo = propertyInfo,
                    IsNullable = isNullable,
                    IsPrimaryKey = isPrimaryKey,
                    AutoIncrement =
                        isPrimaryKey &&
                        propertyInfo.HasAttributeNamed(typeof(AutoIncrementAttribute).Name),
                    IsIndexed = isIndex,
                    IsUnique = isUnique,
                    FieldLength = stringLengthAttr != null
                        ? stringLengthAttr.MaximumLength
                        : (int?)null,
                    DefaultValue = defaultValueAttr != null ? defaultValueAttr.DefaultValue : null,
                    ForeignKey = foreignKeyAttr == null
                        ? referencesAttr != null ? new ForeignKeyConstraint(referencesAttr.Type) : null
                        : new ForeignKeyConstraint(foreignKeyAttr.Type,
                                                    foreignKeyAttr.OnDelete,
                                                    foreignKeyAttr.OnUpdate,
                                                    foreignKeyAttr.ForeignKeyName),
                    IsReference = referenceAttr != null && propertyType.IsClass,
                    GetValueFn = propertyInfo.GetPropertyGetterFn(),
                    SetValueFn = propertyInfo.GetPropertySetterFn(),
                    Sequence = sequenceAttr != null ? sequenceAttr.Name : string.Empty,
                    IsComputed = computeAttr != null,
                    ComputeExpression = computeAttr != null ? computeAttr.Expression : string.Empty,
                    Scale = decimalAttribute != null ? decimalAttribute.Scale : (int?)null,
                    BelongToModelName = belongToAttribute != null ? belongToAttribute.BelongToTableType.GetModelDefinition().ModelName : null, 
                };

                var isIgnored = propertyInfo.HasAttributeNamed(typeof(IgnoreAttribute).Name)
                    || fieldDefinition.IsReference;
                if (isIgnored)
                  modelDef.IgnoredFieldDefinitions.Add(fieldDefinition);
                else
                  modelDef.FieldDefinitions.Add(fieldDefinition);                
            }

            modelDef.SqlSelectAllFromTable = "SELECT {0} FROM {1} "
                .Fmt(OrmLiteConfig.DialectProvider.GetColumnNames(modelDef),
                     OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef));

            Dictionary<Type, ModelDefinition> snapshot, newCache;
            do
            {
                snapshot = typeModelDefinitionMap;
                newCache = new Dictionary<Type, ModelDefinition>(typeModelDefinitionMap);
                newCache[modelType] = modelDef;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref typeModelDefinitionMap, newCache, snapshot), snapshot));
            
            LicenseUtils.AssertValidUsage(LicenseFeature.OrmLite, QuotaType.Tables, typeModelDefinitionMap.Count);

            return modelDef;
        }

        public static StringLengthAttribute CalculateStringLength(this PropertyInfo propertyInfo, DecimalLengthAttribute decimalAttribute)
        {
            var attr = propertyInfo.FirstAttribute<StringLengthAttribute>();
            if (attr != null) return attr;

            var componentAttr = propertyInfo.FirstAttribute<System.ComponentModel.DataAnnotations.StringLengthAttribute>();
            if (componentAttr != null) 
                return new StringLengthAttribute(componentAttr.MaximumLength);

            return decimalAttribute != null ? new StringLengthAttribute(decimalAttribute.Precision) : null;
        }

    }
}