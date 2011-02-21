REM SET BUILD=Debug
SET BUILD=Release

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\release\latest\ServiceStack.OrmLite
COPY ..\src\ServiceStack.OrmLite.SqlServer\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\release\latest\ServiceStack.OrmLite

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib
COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack\lib\tests

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.Examples\lib

COPY ..\src\ServiceStack.OrmLite.Sqlite\bin\%BUILD%\ServiceStack.OrmLite.* ..\..\ServiceStack.Contrib\lib
