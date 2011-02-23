REM SET BUILD=Debug
SET BUILD=Release

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.* ..\..\ServiceStack\release\latest\ServiceStack.OrmLite

COPY ..\src\ServiceStack.OrmLite.Sqlite64\bin\%BUILD%\ServiceStack.OrmLite.Sqlite64.* ..\..\ServiceStack\release\latest\ServiceStack.OrmLite\x64
COPY ..\src\ServiceStack.OrmLite.Sqlite64\bin\%BUILD%\System.Data.SQLite.dll ..\..\ServiceStack\release\latest\ServiceStack.OrmLite\x64

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib\tests

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.Examples\lib

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.Contrib\lib
