REM SET BUILD=Debug
SET BUILD=Release
SET MSBUILD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe 

MD ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite32\lib\
MD ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite32\lib\net40
MD ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite64\lib\net40
MD ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.SqlServer\lib
MD ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite.Mono\lib
MD ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite.Mono\lib\net40
MD ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.MySql\lib
MD ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.PostgreSQL\lib
MD ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Oracle\lib
MD ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Firebird\lib
MD ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.T4\content

COPY ..\src\ServiceStack.OrmLite.Sqlite32\bin\%BUILD%\ServiceStack.OrmLite.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite32\lib\net35
COPY ..\src\ServiceStack.OrmLite.Sqlite32\bin\x86\ServiceStack.OrmLite.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite32\lib\net40
COPY ..\src\ServiceStack.OrmLite.Sqlite64\bin\%BUILD%\ServiceStack.OrmLite.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite64\lib\net35
COPY ..\src\ServiceStack.OrmLite.Sqlite64\bin\x64\ServiceStack.OrmLite.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite64\lib\net40
COPY ..\src\ServiceStack.OrmLite.SqlServer\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.SqlServer\lib

COPY ..\lib\Mono.Data.Sqlite.dll  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite.Mono\lib\net35
COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite.Mono\lib\net35
COPY ..\lib\Mono.Data.Sqlite.dll  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite.Mono\lib\net40
COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\x86\ServiceStack.OrmLite.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite.Mono\lib\net40

COPY ..\src\T4\*.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.T4\content


COPY ..\src\ServiceStack.OrmLite.MySql\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.MySql\lib
COPY ..\src\ServiceStack.OrmLite.PostgreSQL\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.PostgreSQL\lib
COPY ..\src\ServiceStack.OrmLite.Oracle\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Oracle\lib
COPY ..\src\ServiceStack.OrmLite.Firebird\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Firebird\lib
COPY ..\src\ServiceStack.OrmLite.Firebird\bin\%BUILD%\FirebirdSql.Data.FirebirdClient.dll ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Firebird\lib

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\x86\sqlite3.dll ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\x86\Mono.Data.Sqlite.dll ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite.Windows\bin\%BUILD%\ServiceStack.OrmLite.Sqlite.Windows* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.OrmLite.SqlServer\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.OrmLite.MySql\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.OrmLite.PostgreSQL\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.OrmLite.PostgreSQL\bin\%BUILD%\Npgsql.* ..\..\ServiceStack\lib

COPY ..\src\ServiceStack.OrmLite\bin\Signed\ServiceStack.OrmLite.* ..\..\ServiceStack\lib\signed
COPY ..\src\ServiceStack.OrmLite.SqlServer\bin\Signed\ServiceStack.OrmLite.SqlServer.* ..\..\ServiceStack\lib\signed

COPY ..\src\ServiceStack.OrmLiteV45\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib\net45
COPY ..\src\ServiceStack.OrmLite.SqlServerV45\bin\%BUILD%\ServiceStack.OrmLite.SqlServer.* ..\..\ServiceStack\lib\net45
