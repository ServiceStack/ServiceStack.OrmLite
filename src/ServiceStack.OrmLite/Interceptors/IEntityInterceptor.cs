using System.Data;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite.Interceptors
{
    /// <summary>
    ///  Base interceptor
    /// </summary>

    public interface IEntityInterceptor
    {

        /// <summary>
        ///  Interceptor name, should be unique.
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    ///     Represent a plugin to perform operations for a given entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEntityInterceptor<in T> : IEntityInterceptor
    {
        /// <summary>
        ///     Execute code before create a new entity.
        /// </summary>
        /// <param name="dbCmd"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task OnInsertAsync(IDbCommand dbCmd, T entity);

        /// <summary>
        ///     Execute code before update an entity.
        /// </summary>
        /// <param name="dbCmd"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task OnUpdateAsync(IDbCommand dbCmd, T entity);
    }
}
