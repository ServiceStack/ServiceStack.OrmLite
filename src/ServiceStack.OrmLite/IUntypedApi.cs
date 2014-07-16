using System.Collections;
using System.Data;

namespace ServiceStack.OrmLite
{
    public interface IUntypedApi
    {
        IDbConnection Db { get; set; }
        IDbCommand DbCmd { get; set; }

        int SaveAll(IEnumerable objs);
        bool Save(object obj);

        void InsertAll(IEnumerable objs);
        long Insert(object obj, bool selectIdentity = false);

        int UpdateAll(IEnumerable objs);
        int Update(object obj);

        int DeleteAll();
        int Delete(object obj, object anonType);
        int DeleteNonDefaults(object obj, object filter);
        int DeleteById(object id);
        int DeleteByIds(IEnumerable idValues);
        IEnumerable Cast(IEnumerable results);
    }
}