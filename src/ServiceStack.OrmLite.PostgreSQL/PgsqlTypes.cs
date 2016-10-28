using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.PostgreSQL
{
    public class PgSqlJsonAttribute : CustomFieldAttribute
    {
        public PgSqlJsonAttribute() : base("json") { }
    }

    public class PgSqlJsonBAttribute : CustomFieldAttribute
    {
        public PgSqlJsonBAttribute() : base("jsonb") { }
    }

    public class PgSqlHStoreAttribute : CustomFieldAttribute
    {
        public PgSqlHStoreAttribute() : base("hstore") { }
    }

    public class PgSqlTextArrayAttribute : CustomFieldAttribute
    {
        public PgSqlTextArrayAttribute() : base("text[]") { }
    }

    public class PgSqlIntArrayAttribute : CustomFieldAttribute
    {
        public PgSqlIntArrayAttribute() : base("integer[]") { }
    }

    public class PgSqlBigIntArrayAttribute : CustomFieldAttribute
    {
        public PgSqlBigIntArrayAttribute() : base("bigint[]") { }
    }
}