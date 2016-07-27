namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServerTableHint
    {
        public static JoinFormatDelegate ReadUncommitted = (table, expr) => "{0} WITH (READUNCOMMITTED) {1}".Fmt(table, expr);
        public static JoinFormatDelegate ReadCommitted = (table, expr) => "{0} WITH (READCOMMITTED) {1}".Fmt(table, expr);
        public static JoinFormatDelegate ReadPast = (table, expr) => "{0} WITH (READPAST) {1}".Fmt(table, expr);
        public static JoinFormatDelegate Serializable = (table, expr) => "{0} WITH (SERIALIZABLE) {1}".Fmt(table, expr);
        public static JoinFormatDelegate RepeatableRead = (table, expr) => "{0} WITH (REPEATABLEREAD) {1}".Fmt(table, expr);
    }
}
