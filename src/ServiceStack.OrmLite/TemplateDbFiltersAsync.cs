using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.Templates;

namespace ServiceStack.OrmLite
{
    public class TemplateDbFiltersAsync : TemplateFilter
    {
        private IDbConnectionFactory dbFactory;
        public IDbConnectionFactory DbFactory
        {
            get => dbFactory ?? (dbFactory = Context.Container.Resolve<IDbConnectionFactory>());
            set => dbFactory = value;
        }

        async Task<object> exec<T>(Func<IDbConnection, Task<T>> fn, TemplateScopeContext scope, object options)
        {
            try
            {
                using (var db = DbFactory.Open())
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

        public Task<object> dbSelect(TemplateScopeContext scope, string sql) => 
            exec(db => db.SqlListAsync<Dictionary<string, object>>(sql), scope, null);

        public Task<object> dbSelect(TemplateScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.SqlListAsync<Dictionary<string, object>>(sql, args), scope, null);

        public Task<object> dbSelect(TemplateScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.SqlListAsync<Dictionary<string, object>>(sql, args), scope, options);


        public Task<object> dbSingle(TemplateScopeContext scope, string sql) => 
            exec(db => db.SingleAsync<Dictionary<string, object>>(sql), scope, null);

        public Task<object> dbSingle(TemplateScopeContext scope, string sql, Dictionary<string, object> args) =>
            exec(db => db.SingleAsync<Dictionary<string, object>>(sql, args), scope, null);

        public Task<object> dbSingle(TemplateScopeContext scope, string sql, Dictionary<string, object> args, object options) =>
            exec(db => db.SingleAsync<Dictionary<string, object>>(sql, args), scope, options);


        public Task<object> dbScalar(TemplateScopeContext scope, string sql) => 
            exec(db => db.ScalarAsync<object>(sql), scope, null);

        public Task<object> dbScalar(TemplateScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.ScalarAsync<object>(sql, args), scope, null);

        public Task<object> dbScalar(TemplateScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.ScalarAsync<object>(sql, args), scope, options);


        public Task<object> dbExec(TemplateScopeContext scope, string sql) => 
            exec(db => db.ExecuteSqlAsync(sql), scope, null);

        public Task<object> dbExec(TemplateScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.ExecuteSqlAsync(sql, args), scope, null);

        public Task<object> dbExec(TemplateScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.ExecuteSqlAsync(sql, args), scope, options);


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
        private string padCondition(string text) => string.IsNullOrEmpty(text) ? "" : " " + text;
    }
}