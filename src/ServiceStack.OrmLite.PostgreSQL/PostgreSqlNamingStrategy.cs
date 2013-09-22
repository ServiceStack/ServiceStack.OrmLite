using ServiceStack.Text;

namespace ServiceStack.OrmLite.PostgreSQL
{
    public class PostgreSqlNamingStrategy : INamingStrategy
    {
        public string GetTableName(string name)
        {
            return name.ToLowercaseUnderscore();
        }

        public string GetColumnName(string name)
        {
            return name.ToLowercaseUnderscore();
        }
    }
}