using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
    public static class UntypedApiExtensions
    {
        static readonly ConcurrentDictionary<Type, Type> untypedApiMap = 
            new ConcurrentDictionary<Type, Type>();

        public static IUntypedApi CreateTypedApi(this IDbConnection db, Type forType)
        {
            var genericType = untypedApiMap.GetOrAdd(forType, key => typeof(UntypedApi<>).MakeGenericType(key));
            var unTypedApi = genericType.CreateInstance<IUntypedApi>();
            unTypedApi.Db = db;
            return unTypedApi;
        }

        public static IUntypedApi CreateTypedApi(this IDbCommand dbCmd, Type forType)
        {
            var genericType = untypedApiMap.GetOrAdd(forType, key => typeof(UntypedApi<>).MakeGenericType(key));
            var unTypedApi = genericType.CreateInstance<IUntypedApi>();
            unTypedApi.DbCmd = dbCmd;
            return unTypedApi;
        }
    }

    public class UntypedApi<T> : IUntypedApi
    {
        public IDbConnection Db { get; set; }
        public IDbCommand DbCmd { get; set; }

        public T Exec<T>(Func<IDbCommand, T> filter)
        {
            return DbCmd != null ? filter(DbCmd) : Db.Exec(filter);
        }

        public void Exec(Action<IDbCommand> filter)
        {
            if (DbCmd != null)
                filter(DbCmd);
            else
                Db.Exec(filter);
        }

        public int SaveAll(IEnumerable objs)
        {
            return Exec(dbCmd => dbCmd.SaveAll((IEnumerable<T>)objs));
        }

        public bool Save(object obj)
        {
            return Exec(dbCmd => dbCmd.Save((T)obj));
        }

        public void InsertAll(IEnumerable objs)
        {
            Exec(dbCmd => dbCmd.InsertAll((IEnumerable<T>)objs));
        }

        public long Insert(object obj, bool selectIdentity = false)
        {
            return Exec(dbCmd => dbCmd.Insert((T)obj, selectIdentity: selectIdentity));
        }

        public int UpdateAll(IEnumerable objs)
        {
            return Exec(dbCmd => dbCmd.UpdateAll((IEnumerable<T>)objs));
        }

        public int Update(object obj)
        {
            return Exec(dbCmd => dbCmd.Update((T)obj));
        }

        public int DeleteAll()
        {
            return Exec(dbCmd => dbCmd.DeleteAll<T>());
        }

        public int Delete(object obj, object anonType)
        {
            return Exec(dbCmd => dbCmd.Delete<T>(anonType));
        }

        public int DeleteNonDefaults(object obj, object filter)
        {
            return Exec(dbCmd => dbCmd.DeleteNonDefaults((T)filter));
        }

        public int DeleteById(object id)
        {
            return Exec(dbCmd => dbCmd.DeleteById<T>(id));
        }

        public int DeleteByIds(IEnumerable idValues)
        {
            return Exec(dbCmd => dbCmd.DeleteByIds<T>(idValues));
        }
    }
}