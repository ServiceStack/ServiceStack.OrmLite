using System;
using System.Data;

namespace ServiceStack.OrmLite.Oracle
{
    public class OracleExecFilter : OrmLiteExecFilter
    {
        public override IDbCommand CreateCommand(IDbConnection dbConn)
        {
            var command = base.CreateCommand(dbConn);

            // Doing comparison and set this way to avoid having a reference to the Oracle client
            // so that customers can use a different version than we used to compile without
            // requiring a version redirect in a config file somewhere.
            var commandType = command.GetType();
            if (commandType.FullName.StartsWith("Oracle.DataAccess.Client", StringComparison.InvariantCulture))
            {
                var pi = commandType.GetProperty("BindByName");
                pi.SetValue(command, true, null);
            }
            return command;
        }
    }
}
