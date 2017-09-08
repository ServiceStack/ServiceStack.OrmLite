using System;
using System.Collections.Generic;
using System.Data;
using ServiceStack.Data;
using ServiceStack.Templates;

namespace ServiceStack.OrmLite
{
    public class TemplateDbFilters : TemplateFilter
    {
        private IDbConnectionFactory dbFactory;
        public IDbConnectionFactory DbFactory
        {
            get => dbFactory ?? (dbFactory = Context.Container.Resolve<IDbConnectionFactory>());
            set => dbFactory = value;
        }

        T exec<T>(Func<IDbConnection, T> fn, TemplateScopeContext scope, object options)
        {
            try
            {
                using (var db = DbFactory.Open())
                {
                    return fn(db);
                }
            }
            catch (Exception ex)
            {
                throw new StopFilterExecutionException(scope, options, ex);
            }
        }

        public object dbSelect(TemplateScopeContext scope, string sql) => 
            exec(db => db.SqlList<Dictionary<string, object>>(sql), scope, null);

        public object dbSelect(TemplateScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.SqlList<Dictionary<string, object>>(sql, args), scope, null);

        public object dbSelect(TemplateScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.SqlList<Dictionary<string, object>>(sql, args), scope, options);


        public object dbSingle(TemplateScopeContext scope, string sql) => 
            exec(db => db.Single<Dictionary<string, object>>(sql), scope, null);

        public object dbSingle(TemplateScopeContext scope, string sql, Dictionary<string, object> args) =>
            exec(db => db.Single<Dictionary<string, object>>(sql, args), scope, null);

        public object dbSingle(TemplateScopeContext scope, string sql, Dictionary<string, object> args, object options) =>
            exec(db => db.Single<Dictionary<string, object>>(sql, args), scope, options);


        public object dbScalar(TemplateScopeContext scope, string sql) => 
            exec(db => db.Scalar<object>(sql), scope, null);

        public object dbScalar(TemplateScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.Scalar<object>(sql, args), scope, null);

        public object dbScalar(TemplateScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.Scalar<object>(sql, args), scope, options);


        public int dbExec(TemplateScopeContext scope, string sql) => 
            exec(db => db.ExecuteSql(sql), scope, null);

        public int dbExec(TemplateScopeContext scope, string sql, Dictionary<string, object> args) => 
            exec(db => db.ExecuteSql(sql, args), scope, null);

        public int dbExec(TemplateScopeContext scope, string sql, Dictionary<string, object> args, object options) => 
            exec(db => db.ExecuteSql(sql, args), scope, options);


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