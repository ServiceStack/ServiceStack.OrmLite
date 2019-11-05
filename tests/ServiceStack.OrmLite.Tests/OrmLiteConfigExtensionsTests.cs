using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class OrmLiteConfigExtensionsTests
    {
        public OrmLiteConfigExtensionsTests()
        {
            OrmLiteConfig.ClearCache();
        }

        private readonly List<Type> _attributes = new List<Type>{
            typeof(PreCreateTableAttribute),
            typeof(PostCreateTableAttribute),
            typeof(PreDropTableAttribute),
            typeof(PostDropTableAttribute),
            typeof(CompositeIndexAttribute),
            typeof(UniqueConstraintAttribute),
            typeof(PrimaryKeyAttribute),
            typeof(SequenceAttribute),
            typeof(ComputeAttribute),
            typeof(ComputedAttribute),
            typeof(CustomSelectAttribute),
            typeof(DecimalLengthAttribute),
            typeof(BelongToAttribute),
            typeof(ReferenceAttribute),
            typeof(RequiredAttribute),
            typeof(DataAnnotations.IgnoreAttribute),
            typeof(AutoIdAttribute),
            typeof(AliasAttribute),
            typeof(IndexAttribute),
            typeof(DefaultAttribute),
            typeof(ReferencesAttribute),
            typeof(ForeignKeyAttribute),
            typeof(CustomFieldAttribute),
            typeof(CheckConstraintAttribute),
            typeof(AutoIncrementAttribute),
            typeof(IgnoreOnInsertAttribute),
            typeof(IgnoreOnUpdateAttribute),
            typeof(ReturnOnInsertAttribute),
            typeof(UniqueAttribute),
            typeof(DataMemberAttribute),
            typeof(IgnoreDataMemberAttribute)
        };

        private readonly List<Type> _modelTypes = new List<Type>
        {
            typeof(Movie),
            typeof(Shipper),
            typeof(ModelWithAliasedRowVersion),
            typeof(ModelWithComplexType),
            typeof(ModelWithComplexTypes),
            typeof(ModelWithCompositeIndexFields),
            typeof(ModelWithCompositeIndexFieldsDesc),
            typeof(ModelWithCompositeIndexOnFieldSpacesDesc),
            typeof(ModelWithDifferentNumTypes),
            typeof(ModelWithEnum),
            typeof(ModelWithFieldsOfDifferentAndNullableTypes),
            typeof(ModelWithFieldsOfDifferentAndNullableTypesFactory),
            typeof(ModelWithFieldsOfDifferentTypes),
            typeof(ModelWithFieldsOfDifferentTypesAsNullables),
            typeof(ModelWithFieldsOfNullableTypes),
            typeof(ModelWithIdAndName),
            typeof(ModelWithIdOnly),
            typeof(ModelWithIndexFields),
            typeof(ModelWithLongIdAndStringFields),
            typeof(ModelWithNamedCompositeIndex),
            typeof(ModelWithOnlyStringFields),
            typeof(ModelWithOptimisticChildren),
            typeof(ModelWithPostDropSql),
            typeof(ModelWithPreCreateSql),
            typeof(ModelWithPreDropSql),
            typeof(ModelWithRowVersion),
            typeof(ModelWithRowVersionAlias),
            typeof(ModelWithRowVersionAndParent),
            typeof(ModelWithRowVersionBase),
            typeof(ModelWithSeedDataSql),
            typeof(WaybillIn),
            typeof(WaybillBase),
            typeof(SeparateWaybillIn),
            typeof(Poco),
            typeof(PocoTable),
            typeof(PocoWithTime)
        };

        /// <summary>
        /// It takes 2600-2800 ms: it's 5 times longer that usual <see cref="PlatformExtensions.HasAttribute"/>.
        /// The performance problem was gone after update ServiceStack.Text to v.5.7.1.
        /// </summary>
        [Test]
        public void HasAttributeCached()
        {
            foreach (var modelType in _modelTypes)
                foreach (var type in _attributes)
                    GoGenericHasAttributeCachedMethod.MakeGenericMethod(type).Invoke(this, new object[] { modelType });
        }

        /// <summary>
        /// It takes 40-60 ms only!
        /// </summary>
        [Test]
        public void HasAttribute()
        {
            foreach (var modelType in _modelTypes)
                foreach (var type in _attributes)
                    GoGenericHasAttributeMethod.MakeGenericMethod(type).Invoke(this, new object[] { modelType });

        }

        private MethodInfo GoGenericHasAttributeCachedMethod = typeof(OrmLiteConfigExtensionsTests).GetMethod(nameof(GoGenericHasAttributeCached), BindingFlags.NonPublic | BindingFlags.Instance);

        private void GoGenericHasAttributeCached<T>(Type modelType)
        {
            var objProperties = modelType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance).ToList();

            var hasPkAttr = objProperties.Any(p => p.HasAttributeCached<T>());
        }

        private MethodInfo GoGenericHasAttributeMethod = typeof(OrmLiteConfigExtensionsTests).GetMethod(nameof(GoGenericHasAttribute), BindingFlags.NonPublic | BindingFlags.Instance);

        private void GoGenericHasAttribute<T>(Type modelType)
        {
            var objProperties = modelType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance).ToList();

            var hasPkAttr = objProperties.Any(p => p.HasAttribute<T>());
        }
    }
}