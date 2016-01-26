#if NET45
// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
            var indexCache = reader.GetIndexFieldsCache(ModelDefinition<T>.Definition, dialectProvider);
            var values = new object[reader.FieldCount];
            return dialectProvider.ReaderRead(reader, () =>
            {
                var row = CreateInstance<T>();
                row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
                return row;
            }, token).Then(t => { 
                reader.Dispose();
                return t;
            });
        }

        public static Task<List<T>> ConvertToListAsync<T>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, CancellationToken token)
        {
            var indexCache = reader.GetIndexFieldsCache(ModelDefinition<T>.Definition, dialectProvider);
            var values = new object[reader.FieldCount];
            var isObjectList = typeof(T) == typeof(List<object>);
            var isObjectDict = typeof(T) == typeof(Dictionary<string,object>);

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
                else if (isObjectDict)
                {
                    var row = new Dictionary<string,object>();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i).Trim()] = reader.GetValue(i);
                    }
                    return (T)(object)row;
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
            }, token).Then(t =>
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
                var to = (IList)typeof(List<>).MakeGenericType(type).CreateInstance();
                x.Each(o => to.Add(o));
                return to;
            });
        }
    }
}
#endif