using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.OrmLite.Interceptors;
using ServiceStack.OrmLite.Support;

namespace ServiceStack.OrmLite
{
    public static class InterceptorOperationsExtensions
    {
        public static async Task OnInsertAsync<T>(this IDbCommand dbCmd, T obj)
        {
            OrmLiteConfig.InsertFilter?.Invoke(dbCmd, obj);

            foreach (var candidate in OrmLiteConfig.RegisteredInterceptors.Select(s => s.Value).ToList())
            {
                if (candidate is IEntityInterceptor<T> entityInterceptor)
                {
                    await entityInterceptor.OnInsertAsync(dbCmd, obj).ConfigureAwait(false);
                }
            }
        }

        public static void OnInsert<T>(this IDbCommand dbCmd, T obj)
        {
            AsyncHelper.RunSync(() => OnInsertAsync(dbCmd, obj));
        }

        public static async Task OnUpdateAsync<T>(this IDbCommand dbCmd, T obj)
        {
            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, obj);

            foreach (var candidate in OrmLiteConfig.RegisteredInterceptors.Select(s => s.Value).ToList())
            {
                if (candidate is IEntityInterceptor<T> entityInterceptor)
                {
                    await entityInterceptor.OnUpdateAsync(dbCmd, obj).ConfigureAwait(false);
                }
            }
        }

        public static void OnUpdate<T>(this IDbCommand dbCmd, T obj)
        {
            AsyncHelper.RunSync(() => OnUpdateAsync(dbCmd, obj));
        }
    }
}