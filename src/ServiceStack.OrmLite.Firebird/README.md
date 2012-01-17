ServiceStack.OrmLite.Firebird : DialectProvider for FirebirdSql (http://firebirdsql.org), 
based on Demis Bellot's ServiceStack.OrmLite (http://servicestack.net)


For usage samples see :

https://github.com/aicl/ServiceStack.OrmLite/tree/master/src/FirebirdTests/TestLiteFirebird00

https://github.com/aicl/ServiceStack.OrmLite/tree/master/src/FirebirdTests/TestSimpleFirebird01

https://github.com/aicl/ServiceStack.OrmLite/tree/master/src/FirebirdTests/TestSimpleFirebird02

https://github.com/aicl/ServiceStack.OrmLite/tree/master/src/FirebirdTests/TestSimpleFirebird03

https://github.com/aicl/ServiceStack.OrmLite/tree/master/src/FirebirdTests/TestSimpleFirebirdProcedures


If you already have a firebird database you can use ClassWriter to generate CSharp code for each table.
see sample at : https://github.com/aicl/ServiceStack.OrmLite/tree/master/src/FirebirdTests/TestClassWriter

To run samples you must use employee.fdb from https://github.com/aicl/ServiceStack.OrmLite/tree/master/src/ServiceStack.OrmLite.Firebird/App_Data/employee.fdb.tar.gz

add this line to firebird aliases.conf: employee.fdb = /path/to/your/employee.fdb



Unit tests at:

https://github.com/aicl/ServiceStack.OrmLite/tree/master/tests/ServiceStack.OrmLite.FirebirdTests

for unit test you need ormlite-tests.fdb from https://github.com/aicl/ServiceStack.OrmLite/tree/master/src/ServiceStack.OrmLite.Firebird/App_Data/ormlite-tests.fdb.tar.gz

add this line to firebird aliases.conf: ormlite-tests.fdb = /path/to/your/ormlite-tests.fdb
