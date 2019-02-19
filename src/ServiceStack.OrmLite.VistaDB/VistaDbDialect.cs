using ServiceStack.OrmLite.VistaDB;

namespace ServiceStack.OrmLite
{
    public static class VistaDbDialect
    {
        public static IOrmLiteDialectProvider Provider => VistaDbDialectProvider.Instance;
        public static VistaDbDialectProvider Instance => VistaDbDialectProvider.Instance;
    }
}
