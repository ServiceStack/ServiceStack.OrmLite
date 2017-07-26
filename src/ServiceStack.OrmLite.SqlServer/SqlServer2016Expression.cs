using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer.Converters;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServer2016Expression<T> : SqlServerExpression<T>
    {
        public SqlServer2016Expression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}

        protected override object VisitSqlMethodCall(MethodCallExpression m)
        {
            List<object> args = VisitInSqlExpressionList(m.Arguments);
            object quotedColName = args[0];
            args.RemoveAt(0);

            string statement;

            switch (m.Method.Name)
            {
                case nameof(Sql.In):
                    statement = ConvertInExpressionToSql(m, quotedColName);
                    break;
                case nameof(Sql.Desc):
                    statement = $"{quotedColName} DESC";
                    break;
                case nameof(Sql.As):
                    statement = $"{quotedColName} AS {DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(args[0].ToString()))}";
                    break;
                case nameof(Sql.Sum):
                case nameof(Sql.Count):
                case nameof(Sql.Min):
                case nameof(Sql.Max):
                case nameof(Sql.Avg):
                    statement = $"{m.Method.Name}({quotedColName}{(args.Count == 1 ? $",{args[0]}" : "")})";
                    break;
                case nameof(Sql.CountDistinct):
                    statement = $"COUNT(DISTINCT {quotedColName})";
                    break;
                case nameof(Sql.AllFields):
                    var argDef = m.Arguments[0].Type.GetModelMetadata();
                    statement = DialectProvider.GetQuotedTableName(argDef) + ".*";
                    break;
                case nameof(Sql.JoinAlias):
                    statement = args[0] + "." + quotedColName.ToString().LastRightPart('.');
                    break;
                case nameof(Sql.Custom):
                    statement = quotedColName.ToString();
                    break;
                case nameof(Sql2016.IsJson):
                    statement = $"ISJSON({quotedColName})";
                    break;
                case nameof(Sql2016.JsonValue):
                    statement = $"JSON_VALUE({quotedColName}, '{args[0]}')";
                    break;
                case nameof(Sql2016.JsonQuery):
                    statement = $"JSON_QUERY({quotedColName}";
                    if (DialectProvider is SqlServer2017OrmLiteDialectProvider && args.Count > 0)
                    {
                        statement += $", '{args[0]}'";
                    }
                    statement += ")";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }
    }
}

namespace ServiceStack.OrmLite
{
    public static class Sql2016
    {
        /// <summary>Tests whether a string contains valid JSON.</summary>
        /// <param name="expression">The string to test.</param>
        /// <returns>Returns True if the string contains valid JSON; otherwise, returns False. Returns null if expression is null.</returns>
        /// <remarks>ISJSON does not check the uniqueness of keys at the same level.</remarks>
        /// <see cref="https://docs.microsoft.com/en-us/sql/t-sql/functions/isjson-transact-sql"/>
        public static bool? IsJson(string expression) => null;

        /// <summary>Extracts a scalar value from a JSON string.</summary>
        /// <param name="expression">
        /// An expression. Typically the name of a variable or a column that contains JSON text.<br/><br/>
        /// If <b>JSON_VALUE</b> finds JSON that is not valid in expression before it finds the value identified by <i>path</i>, the function returns an error. If <b>JSON_VALUE</b> doesn't find the value identified by <i>path</i>, it scans the entire text and returns an error if it finds JSON that is not valid anywhere in <i>expression</i>.
        /// </param>
        /// <param name="path">
        /// A JSON path that specifies the property to extract. For more info, see <see cref="https://docs.microsoft.com/en-us/sql/relational-databases/json/json-path-expressions-sql-server">JSON Path Expressions (SQL Server)</see>.<br/><br/>
        /// In SQL Server 2017 and in Azure SQL Database, you can provide a variable as the value of <i>path</i>.<br/><br/> 
        /// If the format of path isn't valid, <b>JSON_VALUE</b> returns an error.<br/><br/>
        /// </param>
        /// <returns>
        /// Returns a single text value of type nvarchar(4000). The collation of the returned value is the same as the collation of the input expression.
        /// If the value is greater than 4000 characters: <br/><br/>
        /// <ul>
        /// <li>In lax mode, <b>JSON_VALUE</b> returns null.</li>
        /// <li>In strict mode, <b>JSON_VALUE</b> returns an error.</li>
        /// </ul>
        /// <br/>
        /// If you have to return scalar values greater than 4000 characters, use <b>OPENJSON</b> instead of <b>JSON_VALUE</b>. For more info, see <see cref="https://docs.microsoft.com/en-us/sql/t-sql/functions/openjson-transact-sql">OPENJSON (Transact-SQL)</see>.
        /// </returns>
        /// <see cref="https://docs.microsoft.com/en-us/sql/t-sql/functions/json-value-transact-sql"/>
        public static T JsonValue<T>(string expression, string path) => default(T);

        /// <summary>Extracts a scalar value from a JSON string.</summary>
        /// <param name="expression">
        /// An expression. Typically the name of a variable or a column that contains JSON text.<br/><br/>
        /// If <b>JSON_VALUE</b> finds JSON that is not valid in expression before it finds the value identified by <i>path</i>, the function returns an error. If <b>JSON_VALUE</b> doesn't find the value identified by <i>path</i>, it scans the entire text and returns an error if it finds JSON that is not valid anywhere in <i>expression</i>.
        /// </param>
        /// <param name="path">
        /// A JSON path that specifies the property to extract. For more info, see <see cref="https://docs.microsoft.com/en-us/sql/relational-databases/json/json-path-expressions-sql-server">JSON Path Expressions (SQL Server)</see>.<br/><br/>
        /// In SQL Server 2017 and in Azure SQL Database, you can provide a variable as the value of <i>path</i>.<br/><br/> 
        /// If the format of path isn't valid, <b>JSON_VALUE</b> returns an error.<br/><br/>
        /// </param>
        /// <returns>
        /// Returns a single text value of type nvarchar(4000). The collation of the returned value is the same as the collation of the input expression.
        /// If the value is greater than 4000 characters: <br/><br/>
        /// <ul>
        /// <li>In lax mode, <b>JSON_VALUE</b> returns null.</li>
        /// <li>In strict mode, <b>JSON_VALUE</b> returns an error.</li>
        /// </ul>
        /// <br/>
        /// If you have to return scalar values greater than 4000 characters, use <b>OPENJSON</b> instead of <b>JSON_VALUE</b>. For more info, see <see cref="https://docs.microsoft.com/en-us/sql/t-sql/functions/openjson-transact-sql">OPENJSON (Transact-SQL)</see>.
        /// </returns>
        /// <see cref="https://docs.microsoft.com/en-us/sql/t-sql/functions/json-value-transact-sql"/>
        public static string JsonValue(string expression, string path) => 
            $"JSON_VALUE({expression}, '{path}')";

        /// <summary>
        /// Extracts an object or an array from a JSON string.<br/><br/>
        /// To extract a scalar value from a JSON string instead of an object or an array, see <see cref="https://docs.microsoft.com/en-us/sql/t-sql/functions/json-value-transact-sql">JSON_VALUE(Transact-SQL)</see>. 
        /// For info about the differences between <b>JSON_VALUE</b> and <b>JSON_QUERY</b>, see <see cref="https://docs.microsoft.com/en-us/sql/relational-databases/json/validate-query-and-change-json-data-with-built-in-functions-sql-server#JSONCompare">Compare JSON_VALUE and JSON_QUERY</see>.
        /// </summary>
        /// <typeparam name="T">Type of objects returned</typeparam>
        /// <param name="expression">
        /// An expression. Typically the name of a variable or a column that contains JSON text.<br/><br/>
        /// If <b>JSON_QUERY</b> finds JSON that is not valid in <i>expression</i> before it finds the value identified by <i>path</i>, the function returns an error. If <b>JSON_QUERY</b> doesn't find the value identified by <i>path</i>, it scans the entire text and returns an error if it finds JSON that is not valid anywhere in <i>expression</i>.
        /// </param>
        /// <param name="path">
        /// A JSON path that specifies the object or the array to extract.<br/><br/>
        /// In SQL Server 2017 and in Azure SQL Database, you can provide a variable as the value of <i>path</i>.<br/><br/>
        /// The JSON path can specify lax or strict mode for parsing.If you don't specify the parsing mode, lax mode is the default. For more info, see <see cref="https://docs.microsoft.com/en-us/sql/relational-databases/json/json-path-expressions-sql-server">JSON Path Expressions (SQL Server)</see>.<br/><br/>
        /// The default value for path is '$'. As a result, if you don't provide a value for path, <b>JSON_QUERY</b> returns the input <i>expression</i>.<br/><br/>
        /// If the format of <i>path</i> isn't valid, <b>JSON_QUERY</b> returns an error.
        /// </param>
        /// <returns>
        /// Returns a JSON fragment of type T. The collation of the returned value is the same as the collation of the input expression.<br/><br/>
        /// If the value is not an object or an array:
        /// <ul>
        /// <li>In lax mode, <b>JSON_QUERY</b> returns null.</li>
        /// <li>In strict mode, <b>JSON_QUERY</b> returns an error.</li>
        /// </ul>
        /// </returns>
        public static T JsonQuery<T>(string expression, string path = null) => default(T);

        /// <summary>
        /// Extracts an object or an array from a JSON string.<br/><br/>
        /// To extract a scalar value from a JSON string instead of an object or an array, see <see cref="https://docs.microsoft.com/en-us/sql/t-sql/functions/json-value-transact-sql">JSON_VALUE(Transact-SQL)</see>. 
        /// For info about the differences between <b>JSON_VALUE</b> and <b>JSON_QUERY</b>, see <see cref="https://docs.microsoft.com/en-us/sql/relational-databases/json/validate-query-and-change-json-data-with-built-in-functions-sql-server#JSONCompare">Compare JSON_VALUE and JSON_QUERY</see>.
        /// </summary>
        /// <typeparam name="T">Type of objects returned</typeparam>
        /// <param name="expression">
        /// An expression. Typically the name of a variable or a column that contains JSON text.<br/><br/>
        /// If <b>JSON_QUERY</b> finds JSON that is not valid in <i>expression</i> before it finds the value identified by <i>path</i>, the function returns an error. If <b>JSON_QUERY</b> doesn't find the value identified by <i>path</i>, it scans the entire text and returns an error if it finds JSON that is not valid anywhere in <i>expression</i>.
        /// </param>
        /// <param name="path">
        /// A JSON path that specifies the object or the array to extract.<br/><br/>
        /// In SQL Server 2017 and in Azure SQL Database, you can provide a variable as the value of <i>path</i>.<br/><br/>
        /// The JSON path can specify lax or strict mode for parsing.If you don't specify the parsing mode, lax mode is the default. For more info, see <see cref="https://docs.microsoft.com/en-us/sql/relational-databases/json/json-path-expressions-sql-server">JSON Path Expressions (SQL Server)</see>.<br/><br/>
        /// The default value for path is '$'. As a result, if you don't provide a value for path, <b>JSON_QUERY</b> returns the input <i>expression</i>.<br/><br/>
        /// If the format of <i>path</i> isn't valid, <b>JSON_QUERY</b> returns an error.
        /// </param>
        /// <returns>
        /// Returns a JSON fragment of type T. The collation of the returned value is the same as the collation of the input expression.<br/><br/>
        /// If the value is not an object or an array:
        /// <ul>
        /// <li>In lax mode, <b>JSON_QUERY</b> returns null.</li>
        /// <li>In strict mode, <b>JSON_QUERY</b> returns an error.</li>
        /// </ul>
        /// </returns>
        public static string JsonQuery(string expression, string path = null) =>
            (path.Contains("$")) 
                ? $"JSON_QUERY({expression}, '{path}')"
                : $"JSON_QUERY({expression})";
    }
}