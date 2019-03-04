using System;
using System.Collections.Generic;
using System.Data;
using ServiceStack.Data;
using ServiceStack.Script;

namespace ServiceStack.OrmLite
{
    [Obsolete("Use DbScripts")]
    public class TemplateDbFilters : DbScripts {}
    
    public class DbScripts : ScriptMethods
    {
        private IDbConnectionFactory dbFactory;
        public IDbConnectionFactory DbFactory
        {
            get => dbFactory ?? (dbFactory = Context.Container.Resolve<IDbConnectionFactory>());
            set => dbFactory = value;
        }

        public IDbConnection OpenDbConnection(ScriptScopeContext scope, Dictionary<string, object> options)
        {
            if (options != null)
            {
                if (options.TryGetValue("connectionString", out var connectionString))
                    return options.TryGetValue("providerName", out var providerName)
                       ? DbFactory.OpenDbConnectionString((string)connectionString, (string)providerName) 
                       : DbFactory.OpenDbConnectionString((string)connectionString);
                
                if (options.TryGetValue("namedConnection", out var namedConnection))
                    return DbFactory.OpenDbConnection((string)namedConnection);
            }
            
            if (scope.PageResult != null && scope.PageResult.Args.TryGetValue("__dbinfo", out var oDbInfo) && oDbInfo is ConnectionInfo dbInfo) // Keywords.DbInfo
                return DbFactory.OpenDbConnection(dbInfo);

            return DbFactory.OpenDbConnection();
        }

        T exec<T>(Func<IDbConnection, T> fn, ScriptScopeContext scope, object options)
        {
            try
            {
                using (var db = OpenDbConnection(scope, options as Dictionary<string, object>))
                {
                    return fn(db);
                }
            }
            catch (Exception ex)
            {
                throw new StopFilterExecutionException(scope, options, ex);
            }
        }

        public object dbSelect(ScriptScopeContext scope, string sql) => 
            exec(db => db.SqlList<Dictionary<string, object>>(sql), scope, null);

        public object dbSelect(ScriptScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.SqlList<Dictionary<string, object>>(sql, args), scope, null);

        public object dbSelect(ScriptScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.SqlList<Dictionary<string, object>>(sql, args), scope, options);


        public object dbSingle(ScriptScopeContext scope, string sql) => 
            exec(db => db.Single<Dictionary<string, object>>(sql), scope, null);

        public object dbSingle(ScriptScopeContext scope, string sql, Dictionary<string, object> args) =>
            exec(db => db.Single<Dictionary<string, object>>(sql, args), scope, null);

        public object dbSingle(ScriptScopeContext scope, string sql, Dictionary<string, object> args, object options) =>
            exec(db => db.Single<Dictionary<string, object>>(sql, args), scope, options);


        public object dbScalar(ScriptScopeContext scope, string sql) => 
            exec(db => db.Scalar<object>(sql), scope, null);

        public object dbScalar(ScriptScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.Scalar<object>(sql, args), scope, null);

        public object dbScalar(ScriptScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.Scalar<object>(sql, args), scope, options);


        public int dbExec(ScriptScopeContext scope, string sql) => 
            exec(db => db.ExecuteSql(sql), scope, null);

        public int dbExec(ScriptScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.ExecuteSql(sql, args), scope, null);

        public int dbExec(ScriptScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.ExecuteSql(sql, args), scope, options);

        public List<string> dbTableNames(ScriptScopeContext scope) => dbTableNames(scope, null, null);
        public List<string> dbTableNames(ScriptScopeContext scope, Dictionary<string, object> args) => dbTableNames(scope, args, null);
        public List<string> dbTableNames(ScriptScopeContext scope, Dictionary<string, object> args, object options) => 
            exec(db => db.GetTableNames(args != null && args.TryGetValue("schema", out var oSchema) ? oSchema as string : null), scope, options);

        public List<KeyValuePair<string, long>> dbTableNamesWithRowCounts(ScriptScopeContext scope) => 
            dbTableNamesWithRowCounts(scope, null, null);
        public List<KeyValuePair<string, long>> dbTableNamesWithRowCounts(ScriptScopeContext scope, Dictionary<string, object> args) => 
            dbTableNamesWithRowCounts(scope, args, null);
        public List<KeyValuePair<string, long>> dbTableNamesWithRowCounts(ScriptScopeContext scope, Dictionary<string, object> args, object options) => 
            exec(db => args == null 
                    ? db.GetTableNamesWithRowCounts() 
                    : db.GetTableNamesWithRowCounts(
                        live: args.TryGetValue("live", out var oLive) && oLive is bool b && b,
                        schema: args.TryGetValue("schema", out var oSchema) ? oSchema as string : null), 
                scope, options);

        public string sqlQuote(string name) => OrmLiteConfig.DialectProvider.GetQuotedName(name);
        public string sqlConcat(IEnumerable<object> values) => OrmLiteConfig.DialectProvider.SqlConcat(values);
        public string sqlCurrency(string fieldOrValue) => OrmLiteConfig.DialectProvider.SqlCurrency(fieldOrValue);
        public string sqlCurrency(string fieldOrValue, string symbol) => OrmLiteConfig.DialectProvider.SqlCurrency(fieldOrValue, symbol);

        public string sqlBool(bool value) => OrmLiteConfig.DialectProvider.SqlBool(value);
        public string sqlTrue() => OrmLiteConfig.DialectProvider.SqlBool(true);
        public string sqlFalse() => OrmLiteConfig.DialectProvider.SqlBool(false);
        public string sqlLimit(int? offset, int? limit) => padCondition(OrmLiteConfig.DialectProvider.SqlLimit(offset, limit));
        public string sqlLimit(int? limit) => padCondition(OrmLiteConfig.DialectProvider.SqlLimit(null, limit));
        public string sqlSkip(int? offset) => padCondition(OrmLiteConfig.DialectProvider.SqlLimit(offset, null));
        public string sqlTake(int? limit) => padCondition(OrmLiteConfig.DialectProvider.SqlLimit(null, limit));
        public string ormliteVar(string name) => OrmLiteConfig.DialectProvider.Variables.TryGetValue(name, out var value) ? value : null;

        public bool isUnsafeSql(string sql) => OrmLiteUtils.isUnsafeSql(sql, OrmLiteUtils.VerifySqlRegEx);
        public bool isUnsafeSqlFragment(string sql) => OrmLiteUtils.isUnsafeSql(sql, OrmLiteUtils.VerifyFragmentRegEx);

        private string padCondition(string text) => string.IsNullOrEmpty(text) ? "" : " " + text;
    }
}