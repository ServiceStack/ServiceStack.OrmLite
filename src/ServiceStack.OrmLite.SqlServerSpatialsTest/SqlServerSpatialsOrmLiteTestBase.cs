using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Microsoft.SqlServer.Types;
using ServiceStack.Logging;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.OrmLite.SqlServer.Converters;

namespace ServiceStack.OrmLite.SqlServerTests.Spatials
{
    public class SqlServerSpatialsOrmLiteTestBase
    {
        protected virtual string ConnectionString { get; set; }

        public IDbConnection Db { get; set; }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            try
            {
                LogManager.LogFactory = new ConsoleLogFactory();

                // Get the appropriate Windows System Path
                //  32-bit: C:\Windows\System32
                //  64-bit: C:\Windows\SysWOW64
                var systemPathEnum = (!Environment.Is64BitProcess)
                        ? Environment.SpecialFolder.SystemX86
                        : Environment.SpecialFolder.System;
                
                var systemPath = Environment.GetFolderPath(systemPathEnum);

                // Add directory separator character if needed
                if (!systemPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                {
                    systemPath += System.IO.Path.DirectorySeparatorChar;
                }

                // The versions of the files must match the version associated with Sql Server
                // These files can been installed from the Microsoft SQL Server Feature Pack 
                // 
                // SQL Server 2008: https://www.microsoft.com/en-us/download/details.aspx?id=44277
                // SQL Server 2008 R2: https://www.microsoft.com/en-us/download/details.aspx?id=44272
                // SQL Server 2012 SP2: http://www.microsoft.com/en-us/download/details.aspx?id=43339
                // SQL Server 2014 SP1: https://www.microsoft.com/en-us/download/details.aspx?id=46696
                LoadUnmanagedAssembly(systemPath, "msvcr100.dll");
                LoadUnmanagedAssembly(systemPath, "SqlServerSpatial110.dll");

                // Appending the Sql Server Type System Version to use SqlServerSpatial110.dll (2012) assembly
                // Sql Server defaults to SqlServerSpatial100.dll (2008 R2) even for versions greater
                // https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlconnection.connectionstring.aspx
                ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString + ";Type System Version=SQL Server 2012;";

                var dialectProvider = SqlServer2012Dialect.Provider;
                dialectProvider.RegisterConverter<SqlGeography>(new SqlServerGeographyTypeConverter());
                dialectProvider.RegisterConverter<SqlGeometry>(new SqlServerGeometryTypeConverter());

                Db = new OrmLiteConnection(new OrmLiteConnectionFactory(ConnectionString, dialectProvider));                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Log(string text)
        {
            Console.WriteLine(text);
        }

        public virtual IDbConnection OpenDbConnection(string connString = null)
        {
            connString = connString ?? ConnectionString;
            return connString.OpenDbConnection();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        private static void LoadUnmanagedAssembly(string path, string assemblyName)
        {
            path += assemblyName;
            var ptr = LoadLibrary(path);
            if (ptr == IntPtr.Zero)
            {
                throw new Exception(string.Format(
                    "Error loading {0} (ErrorCode: {1})",
                    assemblyName,
                    Marshal.GetLastWin32Error()));
            }
        }
    }
}
