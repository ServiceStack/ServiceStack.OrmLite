using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Expression
{
    public enum ActivityType
    {
        Unknown = 0,
        Unspecified,
        HavingFun,
        Working
    }

    public class Activity
    {
        [AutoIncrement]
        public int Id { get; set; }
        public ActivityType ActivityType { get; set; }
        public string Comment { get; set; }
    }

    public class ExpressionUsingCustomSerializedEnumTests : ExpressionsTestBase
    {
        [Test]
        public void Can_select_on_custom_serialized_enum()
        {
            using (var db = OpenDbConnection())
            {
                var expected = Init(db);

                var unknownActivities = db.Select<Activity>(
                    s => s.ActivityType == expected.ActivityType
                      && s.Comment == expected.Comment);

                Assert.That(unknownActivities.Count, Is.EqualTo(1));
            }
        }

        private static Activity Init(IDbConnection db)
        {
            EnumSerializer.Configure();
            
            db.DropAndCreateTable<Activity>();

            var activities = new []
                {
                    new Activity {ActivityType = ActivityType.Unknown, Comment = "know nothing about this"},
                    new Activity {ActivityType = ActivityType.Unspecified, Comment = "know we don't know about this"},
                    new Activity {ActivityType = ActivityType.HavingFun, Comment = "want to be doing this"},
                };
            db.InsertAll(activities);
            return activities[0];
        }
    }

    public class EnumSerializer
    {
        public static void Configure()
        {
            var type = typeof(ActivityType);
            InvokeStaticGenericMethod(type, "ConfigureEnumSerialization");
        }

        private static object InvokeStaticGenericMethod(Type genericType, string methodName)
        {
            return InvokeGenericMethod(genericType, methodName, null);
        }

        private static object InvokeGenericMethod(Type genericType, string methodName, object obj)
        {
            return typeof(EnumSerializer).GetMethod(methodName).MakeGenericMethod(genericType).Invoke(obj, null);
        }

        public static void ConfigureEnumSerialization<TEnum>()
        {
            DefaultEnumValues.Add(typeof(TEnum), GetDefault<TEnum>());
            JsConfig<TEnum>.SerializeFn = NonDefaultSerializer;
            JsConfig<TEnum>.DeSerializeFn = NonDefaultDeSerializer<TEnum>;
        }

        private static readonly Dictionary<Type, object> DefaultEnumValues = new Dictionary<Type, object>();

        private static string NonDefaultSerializer<TEnum>(TEnum value)
        {
            return value.Equals(DefaultEnumValues[typeof(TEnum)]) ? null : value.ToString();
        }

        private static TEnum NonDefaultDeSerializer<TEnum>(string value)
        {
            return (String.IsNullOrEmpty(value) ? (TEnum)DefaultEnumValues[typeof(TEnum)] : (TEnum)Enum.Parse(typeof(TEnum), value, true));
        }

        private static T GetDefault<T>()
        {
            return default(T);
        }
    }
}
