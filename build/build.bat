REM SET BUILD=Debug
SET BUILD=Release

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.* ..\..\ServiceStack\release\latest\ServiceStack.OrmLite

REM COPY ..\src\ServiceStack.OrmLite.Sqlite32\bin\%BUILD%\ServiceStack.OrmLite.SqliteNET.* ..\..\ServiceStack\release\latest\ServiceStack.OrmLite\x32
REM COPY ..\src\ServiceStack.OrmLite.Sqlite32\bin\%BUILD%\System.Data.SQLite.dll ..\..\ServiceStack\release\latest\ServiceStack.OrmLite\x32

COPY ..\src\ServiceStack.OrmLite.Sqlite32\bin\%BUILD%\*.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite32\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite64\bin\%BUILD%\*.*  ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.Sqlite64\lib
COPY ..\src\ServiceStack.OrmLite.SqlServer\bin\%BUILD%\*.* ..\..\ServiceStack.OrmLite\NuGet\ServiceStack.OrmLite.SqlServer\lib

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib\tests
COPY ..\src\ServiceStack.OrmLite.SqlServer\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib\tests

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.Examples\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.Contrib\lib
