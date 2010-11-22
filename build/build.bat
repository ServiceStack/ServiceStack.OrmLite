SET DEPLOY_PATH=C:\src\ServiceStack\release\latest\ServiceStack.OrmLite

REM SET BUILD=Debug
SET BUILD=Release

SET PROJ_LIBS=
SET PROJ_LIBS=%PROJ_LIBS% ..\lib\ServiceStack.Client.dll
SET PROJ_LIBS=%PROJ_LIBS% ..\lib\ServiceStack.Common.dll
SET PROJ_LIBS=%PROJ_LIBS% ..\src\ServiceStack.OrmLite\bin\%BUILD%\ServiceStack.OrmLite.dll
SET PROJ_LIBS=%PROJ_LIBS% ..\src\ServiceStack.OrmLite\bin\%BUILD%\ServiceStack.OrmLite.Sqlite.dll
SET PROJ_LIBS=%PROJ_LIBS% ..\src\ServiceStack.OrmLite\bin\%BUILD%\ServiceStack.OrmLite.SqlServer.dll

ILMerge.exe /ndebug /t:library /out:ServiceStack.OrmLite.dll %PROJ_LIBS%
COPY *.dll %DEPLOY_PATH%
