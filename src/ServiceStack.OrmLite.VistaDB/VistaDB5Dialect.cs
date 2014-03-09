using ServiceStack.OrmLite.VistaDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite
{
    public static class VistaDB5Dialect
    {
        public static IOrmLiteDialectProvider Provider { get { return VistaDB5DialectProvider.Instance; } }
    }
}
