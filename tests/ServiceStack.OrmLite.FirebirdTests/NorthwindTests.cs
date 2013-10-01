using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.OrmLite.Firebird;
using Northwind.Common.DataModel;

namespace ServiceStack.OrmLite.FirebirdTests
{
    [TestFixture]
    class NorthwindTests : OrmLiteTestBase
    {
        [Test]
        public void JoinSqlBuilderWithFieldAliasTest()
        {
            var factory = new OrmLiteConnectionFactory(ConnectionString, FirebirdDialect.Provider);

            using (var db = factory.OpenDbConnection())
            {
                var jn = new JoinSqlBuilder<Northwind.Common.DataModel.Employee, Northwind.Common.DataModel.EmployeeTerritory>();

                jn = jn.Join<Northwind.Common.DataModel.Employee, Northwind.Common.DataModel.EmployeeTerritory>(x => x.Id, x => x.EmployeeId)
                       .LeftJoin<Northwind.Common.DataModel.EmployeeTerritory, Northwind.Common.DataModel.Territory>(x => x.TerritoryId, x => x.Id)
                       .Where<Northwind.Common.DataModel.Territory>(x => x.TerritoryDescription.Trim() == "Westboro");
                
                var sql = jn.ToSql();
                // here sql should contain Employees.EmployeID instead of Employees.Id

                var result = db.Query<Northwind.Common.DataModel.Employee>(sql);
                // the generated Sql is ok if the Query doesn't fail
            }
        }
    }
}
