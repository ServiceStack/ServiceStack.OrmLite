namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServerTableHint
    {
        public static JoinFormatDelegate ReadUncommitted = (dialect, tableDef, expr) => "{0} WITH (READUNCOMMITTED) {1}".Fmt(dialect.GetQuotedTableName(tableDef), expr);
        public static JoinFormatDelegate ReadCommitted = (dialect, tableDef, expr) => "{0} WITH (READCOMMITTED) {1}".Fmt(dialect.GetQuotedTableName(tableDef), expr);
        public static JoinFormatDelegate ReadPast = (dialect, tableDef, expr) => "{0} WITH (READPAST) {1}".Fmt(dialect.GetQuotedTableName(tableDef), expr);
        public static JoinFormatDelegate Serializable = (dialect, tableDef, expr) => "{0} WITH (SERIALIZABLE) {1}".Fmt(dialect.GetQuotedTableName(tableDef), expr);
        public static JoinFormatDelegate RepeatableRead = (dialect, tableDef, expr) => "{0} WITH (REPEATABLEREAD) {1}".Fmt(dialect.GetQuotedTableName(tableDef), expr);
    }
}
