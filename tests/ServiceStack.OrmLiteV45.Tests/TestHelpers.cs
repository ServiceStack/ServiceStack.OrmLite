namespace ServiceStack.OrmLite.Tests
{
    public enum Dialect
    {
        Sqlite,
        SqlServer,
        PostgreSql,
        MySql,
        SqlServerMdf,
        Oracle,
        VistaDb,
    }

    public static class TestHelpers
    {
        public static string NormalizeSql(this string sql)
        {
            return sql.ToLower()
                .Replace("\"", "")
                .Replace("`", "")
                .Replace("_", "")
                .Replace(":", "@")   //postgresql
                .Replace("\n", " ")
                .TrimEnd(); 
        }
    }
}