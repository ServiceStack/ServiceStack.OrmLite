//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2010 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
	public static class OrmLiteConfig
	{
		public const string IdField = "Id";

        [ThreadStatic] public static IOrmLiteDialectProvider TSDialectProvider;
	    [ThreadStatic] public static IDbTransaction TSTransaction;

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
                return TSDialectProvider ?? dialectProvider;
			}
			set
			{
				dialectProvider = value;
			}
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

            var sqlConn = DialectProvider.CreateConnection(dbConnectionStringOrFilePath, options);
            sqlConn.Open();
            return sqlConn;
        }

		public static void ClearCache()
		{
			OrmLiteConfigExtensions.ClearCache();
		}

        public static IDbConnection ToDbConnection(this string dbConnectionStringOrFilePath, IOrmLiteDialectProvider dialectProvider)
	    {
            return dialectProvider.CreateConnection(dbConnectionStringOrFilePath, null);
        }
	}
}