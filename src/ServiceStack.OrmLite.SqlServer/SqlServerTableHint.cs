using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServerTableHint
    {
        public const string ReadUncommitted = "WITH (READUNCOMMITTED)";
        public const string ReadCommitted = "WITH (READCOMMITTED)";
        public const string ReadPast = "WITH (READPAST)";
        public const string Serializable = "WITH (SERIALIZABLE)";
        public const string RepeatableRead = "WITH (REPEATABLEREAD)";
    }
}
