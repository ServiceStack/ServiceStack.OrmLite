using ServiceStack.OrmLite.VistaDB;

namespace ServiceStack.OrmLite
{
    public static class VistaDbDialect
    {
        public static IOrmLiteDialectProvider Provider { get { return VistaDbDialectProvider.Instance; } }
    }
}
