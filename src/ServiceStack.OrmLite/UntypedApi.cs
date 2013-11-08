using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
    public static class UntypedApiExtensions
    {
        static readonly ConcurrentDictionary<Type,IUntypedApi> untypedApiMap = 
            new ConcurrentDictionary<Type, IUntypedApi>();

        public static IUntypedApi GetUntypedApi(this Type forType)
        {
            return untypedApiMap.GetOrAdd(forType, key => {
                    var genericType = typeof(UntypedApi<>).MakeGenericType(key);
                    return (IUntypedApi)genericType.CreateInstance();
                });
        }
    }

    public interface IUntypedApi
    {
        int SaveAll(IDbCommand dbCmd, IEnumerable objs);
        bool Save(IDbCommand dbCmd, object obj);
    }

    public class UntypedApi<T> : IUntypedApi
    {
        public int SaveAll(IDbCommand dbCmd, IEnumerable objs)
        {
            return dbCmd.SaveAll((IEnumerable<T>)objs);
        }

        public bool Save(IDbCommand dbCmd, object obj)
        {
            return dbCmd.Save((T)obj);
        }
    }
}