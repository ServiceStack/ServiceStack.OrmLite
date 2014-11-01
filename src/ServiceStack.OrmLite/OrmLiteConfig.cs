//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2013 Service Stack LLC. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteConfig
    {
        public const string IdField = "Id";

        private const int defaultCommandTimeout = 30;
        private static int? commandTimeout;

        public static int CommandTimeout
        {
            get
            {
                if (commandTimeout != null)
                    return commandTimeout.Value;
                return defaultCommandTimeout;
            }
            set
            {
                commandTimeout = value;
            }
        }

        private static IOrmLiteDialectProvider dialectProvider;
        public static IOrmLiteDialectProvider DialectProvider
        {
            get
            {
                if (dialectProvider == null)
                {
                    throw new ArgumentNullException("DialectProvider",
                        "You must set the singleton 'OrmLiteConfig.DialectProvider' to use the OrmLiteWriteExtensions");
                }
                return dialectProvider;
            }
            set
            {
                dialectProvider = value;
            }
        }

        public static IOrmLiteDialectProvider GetDialectProvider(this IDbCommand dbCmd)
        {
            var ormLiteCmd = dbCmd as OrmLiteCommand;
            return ormLiteCmd != null 
                ? ormLiteCmd.DialectProvider
                : DialectProvider;
        }

        public static IOrmLiteDialectProvider GetDialectProvider(this IDbConnection db)
        {
            var ormLiteConn = db as OrmLiteConnection;
            return ormLiteConn != null
                ? ormLiteConn.DialectProvider
                : DialectProvider;
        }

        public static void SetLastCommandText(this IDbConnection db, string sql)
        {
            var ormLiteConn = db as OrmLiteConnection;
            if (ormLiteConn != null)
            {
                ormLiteConn.LastCommandText = sql;
            }
        }

        private const string RequiresOrmLiteConnection = "{0} can only be set on a OrmLiteConnectionFactory connection, not a plain IDbConnection";

        public static void SetCommandTimeout(this IDbConnection db, int? commandTimeout)
        {
            var ormLiteConn = db as OrmLiteConnection;
            if (ormLiteConn == null)
                throw new NotImplementedException(RequiresOrmLiteConnection.Fmt("CommandTimeout"));

            ormLiteConn.CommandTimeout = commandTimeout;
        }

        public static IDbConnection ToDbConnection(this string dbConnectionStringOrFilePath)
        {
            return dbConnectionStringOrFilePath.ToDbConnection(DialectProvider);
        }

        public static IDbConnection OpenDbConnection(this string dbConnectionStringOrFilePath)
        {
            var sqlConn = dbConnectionStringOrFilePath.ToDbConnection(DialectProvider);
            sqlConn.Open();
            return sqlConn;
        }

        public static IDbConnection OpenReadOnlyDbConnection(this string dbConnectionStringOrFilePath)
        {
            var options = new Dictionary<string, string> { { "Read Only", "True" } };

            var dbConn = DialectProvider.CreateConnection(dbConnectionStringOrFilePath, options);
            dbConn.Open();
            return dbConn;
        }

        public static void ClearCache()
        {
            OrmLiteConfigExtensions.ClearCache();
        }

        public static ModelDefinition GetModelMetadata(this Type modelType)
        {
            return modelType.GetModelDefinition();
        }

        public static IDbConnection ToDbConnection(this string dbConnectionStringOrFilePath, IOrmLiteDialectProvider dialectProvider)
        {
            var dbConn = dialectProvider.CreateConnection(dbConnectionStringOrFilePath, options: null);
            return dbConn;
        }

        public static bool DisableColumnGuessFallback { get; set; }
        public static bool StripUpperInLike { get; set; }

        public static IOrmLiteResultsFilter ResultsFilter
        {
            get
            {
                var state = OrmLiteContext.OrmLiteState;
                return state != null 
                    ? state.ResultsFilter
                    : null;
            }
            set { OrmLiteContext.GetOrCreateState().ResultsFilter = value; }
        }

        private static IOrmLiteExecFilter execFilter;
        public static IOrmLiteExecFilter ExecFilter
        {
            get 
            {
                if (execFilter == null)
                    execFilter = new OrmLiteExecFilter();

                return dialectProvider != null 
                    ? dialectProvider.ExecFilter ?? execFilter 
                    : execFilter; 
            }
            set { execFilter = value; }
        }

        public static Action<IDbCommand, object> InsertFilter { get; set; }
        public static Action<IDbCommand, object> UpdateFilter { get; set; }

        public static Func<string, string> StringFilter { get; set; }
    }
}