namespace ServiceStack.OrmLite.Firebird
{
    public class FirebirdNamingStrategy : OrmLiteNamingStrategyBase
    {
        public override string GetTableName(string name)
        {
            if (name.Length > 31)
                name = name.Substring(0, 31);

            return name.ToUpper();
        }

        public override string GetColumnName(string name)
        {
            if (name.Length > 31)
                name = name.Substring(0, 31);

            return name.ToUpper();
        }
    }
}