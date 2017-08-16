using System;
using System.Collections.Generic;
using System.Data;
using ServiceStack.Data;
using ServiceStack.Templates;

namespace ServiceStack.OrmLite
{
    public class TemplateDbFilters : TemplateFilter
    {
        public IDbConnectionFactory DbFactory { get; set; }
        T exec<T>(Func<IDbConnection, T> fn)
        {
            using (var db = DbFactory.Open())
            {
                return fn(db);
            }
        }

        public object dbSelect(string sql) => exec(db => db.Select<Dictionary<string, object>>(sql));
        public object dbSelect(string sql, Dictionary<string, object> args) => exec(db => db.Select<Dictionary<string, object>>(sql, args));
        public object dbSingle(string sql) => exec(db => db.Single<Dictionary<string, object>>(sql));
        public object dbSingle(string sql, Dictionary<string, object> args) => exec(db => db.Single<Dictionary<string, object>>(sql, args));
        public object dbScalar(string sql) => exec(db => db.Scalar<object>(sql));
        public object dbScalar(string sql, Dictionary<string, object> args) => exec(db => db.Scalar<object>(sql, args));
        public int dbExec(string sql, Dictionary<string, object> args) => exec(db => db.ExecuteSql(sql, args));

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