using System;
using System.ComponentModel;

namespace ServiceStack.OrmLite.Tests
{
    /// <summary>
    /// Primary config for all tests 
    /// </summary>
    public class TestConfig
    {
        /// <summary>
        /// This value controls which providers are tested for all <see cref="TestFixtureOrmLiteAttribute"/> tests where dialects are not explicitly set
        /// </summary>
        public static Dialect DefaultDialects = EnvironmentVariable("DefaultDialects", Dialect.Sqlite);
        
        public static string SqliteMemoryDb = EnvironmentVariable("SQLITE_CONNECTION", ":memory:");
        public static string SqlServerBuildDb = EnvironmentVariable("MSSQL_CONNECTION", "Data Source=tcp:localhost,48501\\SQLExpress;Initial Catalog=master;User Id=sa;Password=Test!tesT;Connect Timeout=120;MultipleActiveResultSets=True;");
        public static string OracleDb = EnvironmentVariable("ORACLE_CONNECTION", "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=48401))(CONNECT_DATA=(SID=XE)));User Id=system;Password=test;");
        
        // TODO should dates be handled by reader instead of having to set config options?
        public static string MySqlDb_5_5 = EnvironmentVariable("MYSQL_CONNECTION", "Server=localhost;Port=48201;Database=test;UID=root;Password=test;SslMode=none;Convert Zero Datetime=True;");
        public static string MySqlDb_10_1 = EnvironmentVariable("MYSQL101_CONNECTION", "Server=localhost;Port=48202;Database=test;UID=root;Password=test;SslMode=none");
        public static string MySqlDb_10_2 = EnvironmentVariable("MYSQL102_CONNECTION", "Server=localhost;Port=48203;Database=test;UID=root;Password=test;SslMode=none");
        public static string MySqlDb_10_3 = EnvironmentVariable("MYSQL103_CONNECTION", "Server=localhost;Port=48204;Database=test;UID=root;Password=test;SslMode=none");
        public static string MySqlDb_10_4 = EnvironmentVariable("MYSQL104_CONNECTION", "Server=localhost;Port=48205;Database=test;UID=root;Password=test;SslMode=none");
        
        public static string PostgresDb_9 = EnvironmentVariable("PGSQL_CONNECTION", "Server=localhost;Port=48301;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200");
        public static string PostgresDb_10 = EnvironmentVariable("PGSQL10_CONNECTION", "Server=localhost;Port=48302;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200");
        public static string PostgresDb_11 = EnvironmentVariable("PGSQL10_CONNECTION", "Server=localhost;Port=48303;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200");
        public static string FirebirdDb_3 = EnvironmentVariable("FIREBIRD_CONNECTION", @"User=SYSDBA;Password=masterkey;Database=/firebird/data/test.gdb;DataSource=localhost;Port=48101;Dialect=3;charset=ISO8859_1;MinPoolSize=0;MaxPoolSize=100;");
        public static string VistaDb = EnvironmentVariable("VISTADB_CONNECTION", @"Data Source='|DataDirectory|\Database.vdb5'");
        public static string SqliteFileDb = "~/App_Data/db.sqlite".MapAbsolutePath();
        
        // The default provider and connection the DbFactory is initialised with
        public static IOrmLiteDialectProvider DefaultProvider = SqliteDialect.Provider;
        public static string DefaultConnection = SqliteMemoryDb;
        
        private static T EnvironmentVariable<T>(string variable, T defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(variable);
            return string.IsNullOrEmpty(value) ? defaultValue : Convert<T>(value);
        }
        
        private static T Convert<T>(string value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromInvariantString(value);
        }
    }
}
