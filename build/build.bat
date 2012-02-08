REM SET BUILD=Debug
SET BUILD=Release

COPY ..\src\ServiceStack.OrmLite.Sqlite32\bin\%BUILD%\ServiceStack.OrmLite.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite32\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite64\bin\%BUILD%\ServiceStack.OrmLite.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite64\lib
COPY ..\src\ServiceStack.OrmLite.SqlServer\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.SqlServer\lib
COPY ..\src\ServiceStack.OrmLite.MySql\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.MySql\lib
COPY ..\src\ServiceStack.OrmLite.PostgreSQL\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.PostgreSQL\lib
COPY ..\src\ServiceStack.OrmLite.Firebird\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Firebird\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite32\bin\%BUILD%\System.Data.SQLite.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite32\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite64\bin\%BUILD%\System.Data.SQLite.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite64\lib
COPY ..\src\ServiceStack.OrmLite.Firebird\bin\%BUILD%\FirebirdSql.Data.FirebirdClient.dll ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Firebird\lib

COPY ..\src\ServiceStack.OrmLite.SqlServer\bin\%BUILD%\*.* ..\..\ServiceStack\release\latest\ServiceStack.OrmLite
COPY ..\src\ServiceStack.OrmLite.Sqlite32\bin\%BUILD%\ServiceStack.OrmLite.SqliteNET.*  ..\..\ServiceStack\release\latest\ServiceStack.OrmLite\x32
COPY ..\src\ServiceStack.OrmLite.Sqlite64\bin\%BUILD%\ServiceStack.OrmLite.SqliteNET.*  ..\..\ServiceStack\release\latest\ServiceStack.OrmLite\x64
COPY ..\src\ServiceStack.OrmLite.Sqlite32\bin\%BUILD%\System.Data.SQLite.*  ..\..\ServiceStack\release\latest\ServiceStack.OrmLite\x32
COPY ..\src\ServiceStack.OrmLite.Sqlite64\bin\%BUILD%\System.Data.SQLite.*  ..\..\ServiceStack\release\latest\ServiceStack.OrmLite\x64

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib\tests
COPY ..\src\ServiceStack.OrmLite.SqlServer\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib\tests

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.Examples\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.Contrib\lib

