//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2013 ServiceStack, Inc. All Rights Reserved.
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

        internal static ModelDefinition GetModelDefinition(this Type modelType)
        {
            if (typeModelDefinitionMap.TryGetValue(modelType, out var modelDef))
                return modelDef;

            if (modelType.IsValueType || modelType == typeof(string))
                return null;

            var modelAliasAttr = modelType.FirstAttribute<AliasAttribute>();
            var schemaAttr = modelType.FirstAttribute<SchemaAttribute>();

            var preCreate = modelType.FirstAttribute<PreCreateTableAttribute>();
            var postCreate = modelType.FirstAttribute<PostCreateTableAttribute>();
            var preDrop = modelType.FirstAttribute<PreDropTableAttribute>();
            var postDrop = modelType.FirstAttribute<PostDropTableAttribute>();

            modelDef = new ModelDefinition
            {
                ModelType = modelType,
                Name = modelType.Name,
                Alias = modelAliasAttr?.Name,
                Schema = schemaAttr?.Name,
                PreCreateTableSql = preCreate?.Sql,
                PostCreateTableSql = postCreate?.Sql,
                PreDropTableSql = preDrop?.Sql,
                PostDropTableSql = postDrop?.Sql,
            };

            modelDef.CompositeIndexes.AddRange(
                modelType.AllAttributes<CompositeIndexAttribute>().ToList());

            modelDef.UniqueConstraints.AddRange(
                modelType.AllAttributes<UniqueConstraintAttribute>().ToList());

            var objProperties = modelType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance).ToList();

            var hasPkAttr = objProperties.Any(p => p.HasAttribute<PrimaryKeyAttribute>());

            var hasIdField = CheckForIdField(objProperties);

            var i = 0;
            foreach (var propertyInfo in objProperties)
            {
                if (propertyInfo.GetIndexParameters().Length > 0)
                    continue; //Is Indexer

                var sequenceAttr = propertyInfo.FirstAttribute<SequenceAttribute>();
                var computeAttr = propertyInfo.FirstAttribute<ComputeAttribute>();
                var computedAttr = propertyInfo.FirstAttribute<ComputedAttribute>();
                var customSelectAttr = propertyInfo.FirstAttribute<CustomSelectAttribute>();
                var decimalAttribute = propertyInfo.FirstAttribute<DecimalLengthAttribute>();
                var belongToAttribute = propertyInfo.FirstAttribute<BelongToAttribute>();
                var referenceAttr = propertyInfo.FirstAttribute<ReferenceAttribute>();
                
                var isRowVersion = propertyInfo.Name == ModelDefinition.RowVersionName
                    && (propertyInfo.PropertyType == typeof(ulong) || propertyInfo.PropertyType == typeof(byte[]));

                var isNullableType = propertyInfo.PropertyType.IsNullableType();

                var isNullable = (!propertyInfo.PropertyType.IsValueType
                                   && !propertyInfo.HasAttributeNamed(typeof(RequiredAttribute).Name))
                                   || isNullableType;

                var propertyType = isNullableType
                    ? Nullable.GetUnderlyingType(propertyInfo.PropertyType)
                    : propertyInfo.PropertyType;

                Type treatAsType = null;
                if (propertyType.IsEnumFlags() || propertyType.HasAttribute<EnumAsIntAttribute>())
                    treatAsType = Enum.GetUnderlyingType(propertyType);

                var isReference = referenceAttr != null && propertyType.IsClass;
                var isIgnored = propertyInfo.HasAttribute<IgnoreAttribute>() || isReference;

                var isFirst = !isIgnored && i++ == 0;

                var isAutoId = propertyInfo.HasAttribute<AutoIdAttribute>();

                var isPrimaryKey = (!hasPkAttr && (propertyInfo.Name == OrmLiteConfig.IdField || (!hasIdField && isFirst)))
                   || propertyInfo.HasAttributeNamed(typeof(PrimaryKeyAttribute).Name)
                   || isAutoId;

                
                var aliasAttr = propertyInfo.FirstAttribute<AliasAttribute>();

                var indexAttr = propertyInfo.FirstAttribute<IndexAttribute>();
                var isIndex = indexAttr != null;
                var isUnique = isIndex && indexAttr.Unique;

                var stringLengthAttr = propertyInfo.CalculateStringLength(decimalAttribute);

                var defaultValueAttr = propertyInfo.FirstAttribute<DefaultAttribute>();

                var referencesAttr = propertyInfo.FirstAttribute<ReferencesAttribute>();
                var fkAttr = propertyInfo.FirstAttribute<ForeignKeyAttribute>();
                var customFieldAttr = propertyInfo.FirstAttribute<CustomFieldAttribute>();
                var chkConstraintAttr = propertyInfo.FirstAttribute<CheckConstraintAttribute>();

                var fieldDefinition = new FieldDefinition
                {
                    Name = propertyInfo.Name,
                    Alias = aliasAttr?.Name,
                    FieldType = propertyType,
                    FieldTypeDefaultValue = propertyType.GetDefaultValue(),
                    TreatAsType = treatAsType,
                    PropertyInfo = propertyInfo,
                    IsNullable = isNullable,
                    IsPrimaryKey = isPrimaryKey,
                    AutoIncrement =
                        isPrimaryKey &&
                        propertyInfo.HasAttribute<AutoIncrementAttribute>(),
                    AutoId = isAutoId,
                    IsIndexed = !isPrimaryKey && isIndex,
                    IsUniqueIndex = isUnique,
                    IsClustered = indexAttr?.Clustered == true,
                    IsNonClustered = indexAttr?.NonClustered == true,
                    IndexName = indexAttr?.Name, 
                    IsRowVersion = isRowVersion,
                    IgnoreOnInsert = propertyInfo.HasAttribute<IgnoreOnInsertAttribute>(),
                    IgnoreOnUpdate = propertyInfo.HasAttribute<IgnoreOnUpdateAttribute>(),
                    ReturnOnInsert = propertyInfo.HasAttribute<ReturnOnInsertAttribute>(),
                    FieldLength = stringLengthAttr?.MaximumLength,
                    DefaultValue = defaultValueAttr?.DefaultValue,
                    CheckConstraint = chkConstraintAttr?.Constraint,
                    IsUniqueConstraint = propertyInfo.HasAttribute<UniqueAttribute>(),
                    ForeignKey = fkAttr == null
                        ? referencesAttr != null ? new ForeignKeyConstraint(referencesAttr.Type) : null
                        : new ForeignKeyConstraint(fkAttr.Type, fkAttr.OnDelete, fkAttr.OnUpdate, fkAttr.ForeignKeyName),
                    IsReference = isReference,
                    GetValueFn = propertyInfo.CreateGetter(),
                    SetValueFn = propertyInfo.CreateSetter(),
                    Sequence = sequenceAttr?.Name,
                    IsComputed = computeAttr != null || computedAttr != null || customSelectAttr != null,
                    ComputeExpression = computeAttr != null ? computeAttr.Expression : string.Empty,
                    CustomSelect = customSelectAttr?.Sql,
                    Scale = decimalAttribute?.Scale,
                    BelongToModelName = belongToAttribute?.BelongToTableType.GetModelDefinition().ModelName,
                    CustomFieldDefinition = customFieldAttr?.Sql,
                    IsRefType = propertyType.IsRefType(),
                };

                if (isIgnored)
                    modelDef.IgnoredFieldDefinitions.Add(fieldDefinition);
                else
                    modelDef.FieldDefinitions.Add(fieldDefinition);

                if (isRowVersion)
                    modelDef.RowVersion = fieldDefinition;
            }

            modelDef.AfterInit();

            Dictionary<Type, ModelDefinition> snapshot, newCache;
            do
            {
                snapshot = typeModelDefinitionMap;
                newCache = new Dictionary<Type, ModelDefinition>(typeModelDefinitionMap) { [modelType] = modelDef };

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