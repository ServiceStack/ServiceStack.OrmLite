using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using ServiceStack.OrmLite.MySql.Converters;

namespace ServiceStack.OrmLite.MySql
{
    public class MySqlDialectProvider : MySqlDialectProviderBase<MySqlDialectProvider>
    {
        public static MySqlDialectProvider Instance = new MySqlDialectProvider();

        private const string TextColumnDefinition = "TEXT";

        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            return new MySqlConnection(connectionString);
        }

        public MySqlDialectProvider() : base()
        {
            base.RegisterConverter<DateTime>(new MySqlDateTimeConverter());
        }

        public override IDbDataParameter CreateParam()
        {
            return new MySqlParameter();
        }

    }
}
