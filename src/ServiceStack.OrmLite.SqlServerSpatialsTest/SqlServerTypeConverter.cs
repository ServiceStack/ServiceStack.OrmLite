using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using ServiceStack.OrmLite;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public abstract class SqlServerTypeConverter : OrmLiteConverter
    {
        public SqlServerTypeConverter(string libraryPath = null, string msvcrFileName = "msvcr100.dll", string sqlSpatialFileName = "SqlServerSpatial110.dll")
        {
            // default libraryPath to Windows System
            if (String.IsNullOrEmpty(libraryPath))
            {
                // Get the appropriate Windows System Path
                //  32-bit: C:\Windows\System32
                //  64-bit: C:\Windows\SysWOW64
                var systemPathEnum = (!Environment.Is64BitProcess)
                        ? Environment.SpecialFolder.SystemX86
                        : Environment.SpecialFolder.System;

                libraryPath = Environment.GetFolderPath(systemPathEnum);
            }

            // The versions of the files must match the version associated with Sql Server
            // These files can been installed from the Microsoft SQL Server Feature Pack 
            // 
            // SQL Server 2008: https://www.microsoft.com/en-us/download/details.aspx?id=44277
            // SQL Server 2008 R2: https://www.microsoft.com/en-us/download/details.aspx?id=44272
            // SQL Server 2012 SP2: http://www.microsoft.com/en-us/download/details.aspx?id=43339
            // SQL Server 2014 SP1: https://www.microsoft.com/en-us/download/details.aspx?id=46696
            LoadUnmanagedAssembly(libraryPath, msvcrFileName);
            LoadUnmanagedAssembly(libraryPath, sqlSpatialFileName);
        }

        public override DbType DbType
        {
            get { return DbType.Object; }
        }

        public override void InitDbParam(IDbDataParameter p, Type fieldType)
        {
            var sqlParam = (SqlParameter)p;
            sqlParam.SqlDbType = SqlDbType.Udt;
            sqlParam.UdtTypeName = ColumnDefinition;
        }



        protected static void LoadUnmanagedAssembly(string libraryPath, string fileName)
        {
            var path = Path.Combine(libraryPath, fileName);
            var ptr = LoadLibrary(path);
            if (ptr == IntPtr.Zero)
            {
                throw new Exception(string.Format(
                    "Error loading {0} (ErrorCode: {1})",
                    fileName,
                    Marshal.GetLastWin32Error()));
            }
        }

        protected static void UnloadUnmanagedAssembly(string assemblyName)
        {
            var hMod = IntPtr.Zero;
            if (GetModuleHandleExA(0, assemblyName, hMod))
            {
                while (FreeLibrary(hMod))
                {
                }
            }
        }


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetModuleHandleExA(int dwFlags, string moduleName, IntPtr phModule);


    }
}
