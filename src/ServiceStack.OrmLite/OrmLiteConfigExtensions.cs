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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using ServiceStack.Common.Extensions;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

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

        /// <summary>
        /// Facilitates specifying Class attributes at runtime when creating a table.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="classType"></param>
        /// <param name="runtimeAnnotations">A bunch of data annotation Attributes</param>
        /// <returns></returns>
        private static TAttribute FindAttribute<TAttribute>(Type classtype, DataAnnotationsCollection runtimeAnnotations)
        {
            var attribute = classtype.FirstAttribute<TAttribute>();

            //let's check if runtimeAnnotations have been specified first, to
            //minimize execution for existing users who are not using runtime annotations 
            if (runtimeAnnotations != null && attribute == null)
            {
                attribute = runtimeAnnotations.FindAnnotation<TAttribute>(classtype.Name);
            }

            return attribute;
        }

        /// <summary>
        /// Facilitates specifying Property attributes at runtime when creating a table.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <param name="runtimeAnnotations">A bunch of data annotation Attributes</param>
        /// <returns></returns>
        private static TAttribute FindAttribute<TAttribute>(PropertyInfo propertyInfo, DataAnnotationsCollection runtimeAnnotations)
        {
            var attribute = propertyInfo.FirstAttribute<TAttribute>();

            //let's check if runtimeAnnotations have been specified first, to
            //minimize execution for existing users who are not using runtime annotations 
            if (runtimeAnnotations != null && attribute == null)
            {
                attribute= runtimeAnnotations.FindAnnotation<TAttribute>(propertyInfo.Name);
            }

            return attribute;
        }
        
        internal static ModelDefinition GetModelDefinition(this Type modelType, DataAnnotationsCollection runtimeAnnotations = null)
        {
            ModelDefinition modelDef;

            if (typeModelDefinitionMap.TryGetValue(modelType, out modelDef))
                return modelDef;

            var modelAliasAttr = FindAttribute<AliasAttribute>(modelType, runtimeAnnotations);
            var schemaAttr = FindAttribute<SchemaAttribute>(modelType, runtimeAnnotations);
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
                if (FindAttribute<IgnoreAttribute>(propertyInfo, runtimeAnnotations) != null) continue;
                var sequenceAttr = FindAttribute<SequenceAttribute>(propertyInfo, runtimeAnnotations);
                var computeAttr = FindAttribute<ComputeAttribute>(propertyInfo, runtimeAnnotations);
                var pkAttribute = FindAttribute<PrimaryKeyAttribute>(propertyInfo, runtimeAnnotations);
                var decimalAttribute = FindAttribute<DecimalLengthAttribute>(propertyInfo, runtimeAnnotations);
                var belongToAttribute = FindAttribute<BelongToAttribute>(propertyInfo, runtimeAnnotations);
                var isFirst = i++ == 0;

                var isPrimaryKey = propertyInfo.Name == OrmLiteConfig.IdField || (!hasIdField && isFirst)
                    || pkAttribute != null;

                var isNullableType = IsNullableType(propertyInfo.PropertyType);

                var isNullable = (!propertyInfo.PropertyType.IsValueType
                                   && FindAttribute<RequiredAttribute>(propertyInfo, runtimeAnnotations) == null)
                                 || isNullableType;

                var propertyType = isNullableType
                    ? Nullable.GetUnderlyingType(propertyInfo.PropertyType)
                    : propertyInfo.PropertyType;

                var aliasAttr = FindAttribute<AliasAttribute>(propertyInfo, runtimeAnnotations);

                var indexAttr = FindAttribute<IndexAttribute>(propertyInfo, runtimeAnnotations);
                var isIndex = indexAttr != null;
                var isUnique = isIndex && indexAttr.Unique;

                var stringLengthAttr = FindAttribute<StringLengthAttribute>(propertyInfo, runtimeAnnotations);

                var defaultValueAttr = FindAttribute<DefaultAttribute>(propertyInfo, runtimeAnnotations);

                var referencesAttr = FindAttribute<ReferencesAttribute>(propertyInfo, runtimeAnnotations);
                var foreignKeyAttr = FindAttribute<ForeignKeyAttribute>(propertyInfo, runtimeAnnotations);

                if (decimalAttribute != null && stringLengthAttr == null)
                    stringLengthAttr = new StringLengthAttribute(decimalAttribute.Precision);

                var fieldDefinition = new FieldDefinition {
                    Name = propertyInfo.Name,
                    Alias = aliasAttr != null ? aliasAttr.Name : null,
                    FieldType = propertyType,
                    PropertyInfo = propertyInfo,
                    IsNullable = isNullable,
                    IsPrimaryKey = isPrimaryKey,
                    AutoIncrement =
                        isPrimaryKey &&
                        FindAttribute<AutoIncrementAttribute>(propertyInfo, runtimeAnnotations) != null,
                    IsIndexed = isIndex,
                    IsUnique = isUnique,
                    FieldLength =
                        stringLengthAttr != null
                            ? stringLengthAttr.MaximumLength
                            : (int?)null,
                    DefaultValue =
                        defaultValueAttr != null ? defaultValueAttr.DefaultValue : null,
                    ForeignKey =
                        foreignKeyAttr == null
                            ? referencesAttr == null
                                  ? null
                                  : new ForeignKeyConstraint(referencesAttr.Type)
                            : new ForeignKeyConstraint(foreignKeyAttr.Type,
                                                       foreignKeyAttr.OnDelete,
                                                       foreignKeyAttr.OnUpdate),
                    GetValueFn = propertyInfo.GetPropertyGetterFn(),
                    SetValueFn = propertyInfo.GetPropertySetterFn(),
                    Sequence = sequenceAttr != null ? sequenceAttr.Name : string.Empty,
                    IsComputed = computeAttr != null,
                    ComputeExpression =
                        computeAttr != null ? computeAttr.Expression : string.Empty,
                    Scale = decimalAttribute != null ? decimalAttribute.Scale : (int?)null,
                    BelongToModelName = belongToAttribute != null ? belongToAttribute.BelongToTableType.GetModelDefinition().ModelName : null,
                };

                modelDef.FieldDefinitions.Add(fieldDefinition);
            }

            modelDef.SqlSelectAllFromTable = "SELECT {0} FROM {1} ".Fmt(OrmLiteConfig.DialectProvider.GetColumnNames(modelDef),
                                                                        OrmLiteConfig.DialectProvider.GetQuotedTableName(
                                                                            modelDef));
            Dictionary<Type, ModelDefinition> snapshot, newCache;
            do
            {
                snapshot = typeModelDefinitionMap;
                newCache = new Dictionary<Type, ModelDefinition>(typeModelDefinitionMap);
                newCache[modelType] = modelDef;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref typeModelDefinitionMap, newCache, snapshot), snapshot));

            return modelDef;
        }

    }
}