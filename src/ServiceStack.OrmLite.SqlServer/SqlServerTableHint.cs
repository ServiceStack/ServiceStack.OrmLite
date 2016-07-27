namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServerTableHint
    {
        public const string ReadUncommitted = "{0} WITH (READUNCOMMITTED) {1}";
        public const string ReadCommitted = "{0} WITH (READCOMMITTED) {1}";
        public const string ReadPast = "{0} WITH (READPAST) {1}";
        public const string Serializable = "{0} WITH (SERIALIZABLE) {1}";
        public const string RepeatableRead = "{0} WITH (REPEATABLEREAD) {1}";
    }
}
