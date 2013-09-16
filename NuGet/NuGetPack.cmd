SET NUGET=..\src\.nuget\nuget
%NUGET% pack ServiceStack.OrmLite.Sqlite.Mono\servicestack.ormlite.sqlite.mono.nuspec -symbols
%NUGET% pack ServiceStack.OrmLite.Sqlite32\servicestack.ormlite.sqlite32.nuspec -symbols
%NUGET% pack ServiceStack.OrmLite.Sqlite64\servicestack.ormlite.sqlite64.nuspec -symbols
%NUGET% pack ServiceStack.OrmLite.SqlServer\servicestack.ormlite.sqlserver.nuspec -symbols
%NUGET% pack ServiceStack.OrmLite.MySql\servicestack.ormlite.mysql.nuspec -symbols
%NUGET% pack ServiceStack.OrmLite.PostgreSQL\servicestack.ormlite.postgresql.nuspec -symbols
%NUGET% pack ServiceStack.OrmLite.Oracle\servicestack.ormlite.oracle.nuspec -symbols
%NUGET% pack ServiceStack.OrmLite.Firebird\servicestack.ormlite.firebird.nuspec -symbols
%NUGET% pack ServiceStack.OrmLite.T4\servicestack.ormlite.t4.nuspec -symbols

