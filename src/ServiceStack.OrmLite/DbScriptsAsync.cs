using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.Script;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    [Obsolete("Use DbScriptsAsync")]
    public class TemplateDbFiltersAsync : DbScriptsAsync {}
    
    public class DbScriptsAsync : ScriptMethods
    {
        private const string DbInfo = "__dbinfo"; // Keywords.DbInfo
        private const string DbConnection = "__dbconnection"; // useDbConnection global

        private IDbConnectionFactory dbFactory;
        public IDbConnectionFactory DbFactory
        {
            get => dbFactory ?? (dbFactory = Context.Container.Resolve<IDbConnectionFactory>());
            set => dbFactory = value;
        }

        public async Task<IDbConnection> OpenDbConnectionAsync(ScriptScopeContext scope, Dictionary<string, object> options)
        {
            var dbConn = await OpenDbConnectionFromOptionsAsync(options);
            if (dbConn != null)
                return dbConn;

            if (scope.PageResult != null)
            {
                if (scope.PageResult.Args.TryGetValue(DbInfo, out var oDbInfo) && oDbInfo is ConnectionInfo dbInfo)
                    return await DbFactory.OpenDbConnectionAsync(dbInfo);

                if (scope.PageResult.Args.TryGetValue(DbConnection, out var oDbConn) && oDbConn is Dictionary<string, object> globalDbConn)
                    return await OpenDbConnectionFromOptionsAsync(globalDbConn);
            }

            return await DbFactory.OpenAsync();
        }

        public IgnoreResult useDb(ScriptScopeContext scope, Dictionary<string, object> dbConnOptions)
        {
            if (dbConnOptions == null)
            {
                scope.PageResult.Args.Remove(DbConnection);
            }
            else
            {
                if (!dbConnOptions.ContainsKey("connectionString") && !dbConnOptions.ContainsKey("namedConnection"))
                    throw new NotSupportedException(nameof(useDb) + " requires either 'connectionString' or 'namedConnection' property");

                scope.PageResult.Args[DbConnection] = dbConnOptions;
            }
            return IgnoreResult.Value;
        }

        private async Task<IDbConnection> OpenDbConnectionFromOptionsAsync(Dictionary<string, object> options)
        {
            if (options != null)
            {
                if (options.TryGetValue("connectionString", out var connectionString))
                {
                    return options.TryGetValue("providerName", out var providerName)
                        ? await DbFactory.OpenDbConnectionStringAsync((string) connectionString, (string) providerName)
                        : await DbFactory.OpenDbConnectionStringAsync((string) connectionString);
                }

                if (options.TryGetValue("namedConnection", out var namedConnection))
                {
                    return await DbFactory.OpenDbConnectionStringAsync((string) namedConnection);
                }
            }

            return null;
        }

        async Task<object> exec<T>(Func<IDbConnection, Task<T>> fn, ScriptScopeContext scope, object options)
        {
            try
            {
                using (var db = await OpenDbConnectionAsync(scope, options as Dictionary<string, object>))
                {
                    var result = await fn(db);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new StopFilterExecutionException(scope, options, ex);
            }
        }

        public Task<object> dbSelect(ScriptScopeContext scope, string sql) => 
            exec(db => db.SqlListAsync<Dictionary<string, object>>(sql), scope, null);

        public Task<object> dbSelect(ScriptScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.SqlListAsync<Dictionary<string, object>>(sql, args), scope, null);

        public Task<object> dbSelect(ScriptScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.SqlListAsync<Dictionary<string, object>>(sql, args), scope, options);


        public Task<object> dbSingle(ScriptScopeContext scope, string sql) => 
            exec(db => db.SingleAsync<Dictionary<string, object>>(sql), scope, null);

        public Task<object> dbSingle(ScriptScopeContext scope, string sql, Dictionary<string, object> args) =>
            exec(db => db.SingleAsync<Dictionary<string, object>>(sql, args), scope, null);

        public Task<object> dbSingle(ScriptScopeContext scope, string sql, Dictionary<string, object> args, object options) =>
            exec(db => db.SingleAsync<Dictionary<string, object>>(sql, args), scope, options);


        public Task<object> dbScalar(ScriptScopeContext scope, string sql) => 
            exec(db => db.ScalarAsync<object>(sql), scope, null);

        public Task<object> dbScalar(ScriptScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.ScalarAsync<object>(sql, args), scope, null);

        public Task<object> dbScalar(ScriptScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.ScalarAsync<object>(sql, args), scope, options);


        public Task<object> dbExec(ScriptScopeContext scope, string sql) => 
            exec(db => db.ExecuteSqlAsync(sql), scope, null);

        public Task<object> dbExec(ScriptScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.ExecuteSqlAsync(sql, args), scope, null);

        public Task<object> dbExec(ScriptScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.ExecuteSqlAsync(sql, args), scope, options);

        public Task<object> dbTableNames(ScriptScopeContext scope) => dbTableNames(scope, null, null);
        public Task<object> dbTableNames(ScriptScopeContext scope, Dictionary<string, object> args) => dbTableNames(scope, args, null);
        public Task<object> dbTableNames(ScriptScopeContext scope, Dictionary<string, object> args, object options) => 
            exec(db => db.GetTableNamesAsync(args != null && args.TryGetValue("schema", out var oSchema) ? oSchema as string : null), scope, options);

        public Task<object> dbTableNamesWithRowCounts(ScriptScopeContext scope) => 
            dbTableNamesWithRowCounts(scope, null, null);
        public Task<object> dbTableNamesWithRowCounts(ScriptScopeContext scope, Dictionary<string, object> args) => 
            dbTableNamesWithRowCounts(scope, args, null);
        public Task<object> dbTableNamesWithRowCounts(ScriptScopeContext scope, Dictionary<string, object> args, object options) => 
            exec(db => args == null 
                    ? db.GetTableNamesWithRowCountsAsync() 
                    : db.GetTableNamesWithRowCountsAsync(
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