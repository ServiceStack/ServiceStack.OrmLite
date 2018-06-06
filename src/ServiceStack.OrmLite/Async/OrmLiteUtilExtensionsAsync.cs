#if ASYNC
// Copyright (c) ServiceStack, Inc. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    internal static class OrmLiteUtilExtensionsAsync
    {
        public static T CreateInstance<T>()
        {
            return (T)ReflectionExtensions.CreateInstance<T>();
        }

        public static Task<T> ConvertToAsync<T>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, CancellationToken token)
        {
            return dialectProvider.ReaderRead(reader, () =>
            {
                if (typeof(T) == typeof(List<object>))
                    return (T)(object)reader.ConvertToListObjects();

                if (typeof(T) == typeof(Dictionary<string, object>))
                    return (T)(object)reader.ConvertToDictionaryObjects();

                var values = new object[reader.FieldCount];

                if (typeof(T).IsValueTuple())
                    return reader.ConvertToValueTuple<T>(values, dialectProvider);

                var row = CreateInstance<T>();
                var indexCache = reader.GetIndexFieldsCache(ModelDefinition<T>.Definition, dialectProvider);
                row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
                return row;
            }, token).Then(t => { 
                reader.Dispose();
                return t;
            });
        }

        public static Task<List<T>> ConvertToListAsync<T>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, HashSet<string> onlyFields, CancellationToken token)
        {
            var values = new object[reader.FieldCount];
            var isObjectList = typeof(T) == typeof(List<object>);
            var isObjectDict = typeof(T) == typeof(Dictionary<string,object>);
            var isDynamic = typeof(T) == typeof(object);
            var isValueTuple = typeof(T).IsValueTuple();
            var isTuple = typeof(T).IsTuple();
            var indexCache = isObjectDict || isObjectDict || isValueTuple || isTuple
                ? null
                : reader.GetIndexFieldsCache(ModelDefinition<T>.Definition, dialectProvider, onlyFields);

            var genericArgs = isTuple ? typeof(T).GetGenericArguments() : null;
            var modelIndexCaches = isTuple ? reader.GetMultiIndexCaches(dialectProvider, onlyFields, genericArgs) : null;
            var genericTupleMi = isTuple ? typeof(T).GetGenericTypeDefinition().GetCachedGenericType(genericArgs) : null;
#if NETSTANDARD2_0
            var activator = isTuple ? System.Reflection.TypeExtensions.GetConstructor(genericTupleMi, genericArgs).GetActivator() : null;
#else
            var activator = isTuple ? genericTupleMi.GetConstructor(genericArgs).GetActivator() : null;
#endif
            return dialectProvider.ReaderEach(reader, () =>
            {
                if (isObjectList)
                {
                    var row = new List<object>();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.GetValue(i));
                    }
                    return (T)(object)row;
                }
                if (isObjectDict)
                {
                    var row = new Dictionary<string,object>();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i).Trim()] = reader.GetValue(i);
                    }
                    return (T)(object)row;
                }
                if (isDynamic)
                {
                    var row = (IDictionary<string, object>) new ExpandoObject();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i).Trim()] = reader.GetValue(i);
                    }
                    return (T)(object)row;
                }
                if (isValueTuple)
                {
                    var row = reader.ConvertToValueTuple<T>(values, dialectProvider);
                    return (T)row;
                }
                if (isTuple)
                {
                    var tupleArgs = reader.ToMultiTuple(dialectProvider, modelIndexCaches, genericArgs, values);
                    var tuple = activator(tupleArgs.ToArray());
                    return (T)tuple;
                }
                else
                {
                    var row = CreateInstance<T>();
                    row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
                    return row;
                }
            }, token).Then(t => {
                reader.Dispose();
                return t;
            });
        }

        public static Task<object> ConvertToAsync(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, Type type, CancellationToken token)
        {
            var modelDef = type.GetModelDefinition();
            var indexCache = reader.GetIndexFieldsCache(modelDef, dialectProvider);
            var values = new object[reader.FieldCount];
            return dialectProvider.ReaderRead(reader, () =>
            {
                var row = type.CreateInstance();
                row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
                return row;
            }, token).Then<object,object>(t =>
            {
                reader.Dispose();
                return t;
            });
        }

        public static Task<IList> ConvertToListAsync(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, Type type, CancellationToken token)
        {
            var modelDef = type.GetModelDefinition();
            var indexCache = reader.GetIndexFieldsCache(modelDef, dialectProvider);
            var values = new object[reader.FieldCount];
            return dialectProvider.ReaderEach(reader, () =>
            {
                var row = type.CreateInstance();
                row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
                return row;
            }, token)
            .Then(x =>
            {
                reader.Dispose();
                var to = (IList)typeof(List<>).GetCachedGenericType(type).CreateInstance();
                x.Each(o => to.Add(o));
                return to;
            });
        }
    }
}
#endif