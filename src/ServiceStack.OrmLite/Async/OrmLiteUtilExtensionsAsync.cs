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

        public static Task<T> ConvertToAsync<T>(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider, CancellationToken token)
        {
            var fieldDefs = ModelDefinition<T>.Definition.AllFieldDefinitionsArray;
            using (dataReader)
            {
                return dialectProvider.ReaderRead(dataReader, () =>
                {
                    var row = CreateInstance<T>();
                    var indexCache = dataReader.GetIndexFieldsCache(ModelDefinition<T>.Definition);
                    row.PopulateWithSqlReader(dialectProvider, dataReader, fieldDefs, indexCache);
                    return row;
                }, token);
            }
        }

        public static Task<List<T>> ConvertToListAsync<T>(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider, CancellationToken token)
        {
            var fieldDefs = ModelDefinition<T>.Definition.AllFieldDefinitionsArray;
            using (dataReader)
            {
                var indexCache = dataReader.GetIndexFieldsCache(ModelDefinition<T>.Definition);
                return dialectProvider.ReaderEach(dataReader, () =>
                {
                    var row = CreateInstance<T>();
                    row.PopulateWithSqlReader(dialectProvider, dataReader, fieldDefs, indexCache);
                    return row;
                }, token);
            }
        }

        public static Task<object> ConvertToAsync(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider, Type type, CancellationToken token)
        {
            var modelDef = type.GetModelDefinition();
            var fieldDefs = modelDef.AllFieldDefinitionsArray;

            using (dataReader)
            {
                return dialectProvider.ReaderRead(dataReader, () =>
                {
                    var row = type.CreateInstance();
                    var indexCache = dataReader.GetIndexFieldsCache(modelDef);
                    row.PopulateWithSqlReader(dialectProvider, dataReader, fieldDefs, indexCache);
                    return row;
                }, token);
            }
        }

        public static Task<IList> ConvertToListAsync(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider, Type type, CancellationToken token)
        {
            var modelDef = type.GetModelDefinition();
            var fieldDefs = modelDef.AllFieldDefinitionsArray;

            using (dataReader)
            {
                var indexCache = dataReader.GetIndexFieldsCache(modelDef);
                return dialectProvider.ReaderEach(dataReader, () =>
                {
                    var row = type.CreateInstance();
                    row.PopulateWithSqlReader(dialectProvider, dataReader, fieldDefs, indexCache);
                    return row;
                }, token)
                .Then(x => {
                    var to = (IList)typeof(List<>).MakeGenericType(type).CreateInstance();
                    x.Each(o => to.Add(o));
                    return to;
                });
            }
        }
    }
}
#endif