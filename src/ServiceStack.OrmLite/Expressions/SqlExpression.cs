using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ServiceStack.OrmLite.Dapper;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public abstract partial class SqlExpression<T> : ISqlExpression, IHasUntypedSqlExpression
    {
        public const string TrueLiteral = "(1=1)";
        public const string FalseLiteral = "(1=0)";

        protected bool visitedExpressionIsTableColumn = false;
        protected bool skipParameterizationForThisExpression = false;

        private Expression<Func<T, bool>> underlyingExpression;
        private List<string> orderByProperties = new List<string>();
        private string selectExpression = string.Empty;
        private string fromExpression = null;
        private string whereExpression;
        private string groupBy = string.Empty;
        private string havingExpression;
        private string orderBy = string.Empty;
        public HashSet<string> OnlyFields { get; protected set; }

        public List<string> UpdateFields { get; set; }
        public List<string> InsertFields { get; set; }

        private string sep = string.Empty;
        protected bool useFieldName = false;
        protected bool selectDistinct = false;
        protected bool CustomSelect { get; set; }
        protected ModelDefinition modelDef;
        public bool PrefixFieldWithTableName { get; set; }
        public bool WhereStatementWithoutWhereString { get; set; }
        public IOrmLiteDialectProvider DialectProvider { get; set; }
        public List<IDbDataParameter> Params { get; set; }
        public Func<string,string> SqlFilter { get; set; }
        public static Action<SqlExpression<T>> SelectFilter { get; set; }

        protected string Sep => sep;

        protected SqlExpression(IOrmLiteDialectProvider dialectProvider)
        {
            UpdateFields = new List<string>();
            InsertFields = new List<string>();

            modelDef = typeof(T).GetModelDefinition();
            PrefixFieldWithTableName = false;
            WhereStatementWithoutWhereString = false;

            DialectProvider = dialectProvider;
            Params = new List<IDbDataParameter>();
            tableDefs.Add(modelDef);
        }

        public SqlExpression<T> Clone()
        {
            return CopyTo(DialectProvider.SqlExpression<T>());
        }

        protected virtual SqlExpression<T> CopyTo(SqlExpression<T> to)
        {
            to.underlyingExpression = underlyingExpression;
            to.orderByProperties = orderByProperties;
            to.selectExpression = selectExpression;
            to.selectDistinct = selectDistinct;
            to.CustomSelect = CustomSelect;
            to.fromExpression = fromExpression;
            to.whereExpression = whereExpression;
            to.groupBy = groupBy;
            to.havingExpression = havingExpression;
            to.orderBy = orderBy;
            to.OnlyFields = OnlyFields != null ? new HashSet<string>(OnlyFields) : null;
            to.UpdateFields = UpdateFields;
            to.InsertFields = InsertFields;
            to.modelDef = modelDef;
            to.PrefixFieldWithTableName = PrefixFieldWithTableName;
            to.WhereStatementWithoutWhereString = WhereStatementWithoutWhereString;
            to.Params = new List<IDbDataParameter>(Params);
            to.SqlFilter = SqlFilter;
            return to;
        }

        /// <summary>
        /// Clear select expression. All properties will be selected.
        /// </summary>
        public virtual SqlExpression<T> Select()
        {
            return Select(string.Empty);
        }

        /// <summary>
        /// set the specified selectExpression.
        /// </summary>
        /// <param name='selectExpression'>
        /// raw Select expression: "Select SomeField1, SomeField2 from SomeTable"
        /// </param>
        public virtual SqlExpression<T> Select(string selectExpression)
        {
            selectExpression?.SqlVerifyFragment();

            return UnsafeSelect(selectExpression);
        }

        public virtual SqlExpression<T> UnsafeSelect(string rawSelect)
        {
            if (string.IsNullOrEmpty(rawSelect))
            {
                BuildSelectExpression(string.Empty, false);
            }
            else
            {
                this.selectExpression = "SELECT " + rawSelect;
                this.CustomSelect = true;
                OnlyFields = null;
            }
            return this;
        }

        /// <summary>
        /// Set the specified selectExpression using matching fields.
        /// </summary>
        /// <param name='fields'>
        /// Matching Fields: "SomeField1, SomeField2"
        /// </param>
        public virtual SqlExpression<T> Select(string[] fields)
        {
            if (fields == null || fields.Length == 0)
                return Select(string.Empty);

            useFieldName = true;

            var allTableDefs = new List<ModelDefinition> { modelDef };
            allTableDefs.AddRange(tableDefs);

            var fieldsList = new List<string>();
            var sb = StringBuilderCache.Allocate();
            foreach (var field in fields)
            {
                if (string.IsNullOrEmpty(field))
                    continue;

                if (field.EndsWith(".*"))
                {
                    var tableName = field.Substring(0, field.Length - 2);
                    var tableDef = allTableDefs.FirstOrDefault(x => string.Equals(x.Name, tableName, StringComparison.OrdinalIgnoreCase));
                    if (tableDef != null)
                    {
                        foreach (var fieldDef in tableDef.FieldDefinitionsArray)
                        {
                            var qualifiedField = GetQuotedColumnName(tableDef, fieldDef.Name);

                            if (sb.Length > 0)
                                sb.Append(", ");

                            sb.Append(qualifiedField);
                            fieldsList.Add(fieldDef.Name);
                        }
                    }
                    continue;
                }

                fieldsList.Add(field); //Could be non-matching referenced property

                var match = FirstMatchingField(field);
                if (match == null)
                    continue;

                var qualifiedName = GetQuotedColumnName(match.Item1, match.Item2.Name);

                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(qualifiedName);
            }

            UnsafeSelect(StringBuilderCache.ReturnAndFree(sb));
            OnlyFields = new HashSet<string>(fieldsList, StringComparer.OrdinalIgnoreCase);

            return this;
        }

        private SqlExpression<T> InternalSelect(Expression fields, bool distinct=false)
        {
            sep = string.Empty;
            useFieldName = true;
            CustomSelect = true;
            BuildSelectExpression(Visit(fields).ToString(), distinct: distinct);
            return this;
        }

        /// <summary>
        /// Fields to be selected.
        /// </summary>
        /// <param name='fields'>
        /// x=> x.SomeProperty1 or x=> new{ x.SomeProperty1, x.SomeProperty2}
        /// </param>
        /// </typeparam>
        public virtual SqlExpression<T> Select(Expression<Func<T, object>> fields)
        {
            return InternalSelect(fields);
        }

        public virtual SqlExpression<T> Select<Table1>(Expression<Func<Table1, object>> fields)
        {
            return InternalSelect(fields);
        }

        public virtual SqlExpression<T> Select<Table1, Table2>(Expression<Func<Table1, Table2, object>> fields)
        {
            return InternalSelect(fields);
        }

        public virtual SqlExpression<T> Select<Table1, Table2, Table3>(Expression<Func<Table1, Table2, Table3, object>> fields)
        {
            return InternalSelect(fields);
        }

        public virtual SqlExpression<T> Select<Table1, Table2, Table3, Table4>(Expression<Func<Table1, Table2, Table3, Table4, object>> fields)
        {
            return InternalSelect(fields);
        }

        public virtual SqlExpression<T> Select<Table1, Table2, Table3, Table4, Table5>(Expression<Func<Table1, Table2, Table3, Table4, Table5, object>> fields)
        {
            return InternalSelect(fields);
        }

        public virtual SqlExpression<T> Select<Table1, Table2, Table3, Table4, Table5, Table6>(Expression<Func<Table1, Table2, Table3, Table4, Table5, Table6, object>> fields)
        {
            return InternalSelect(fields);
        }

        public virtual SqlExpression<T> Select<Table1, Table2, Table3, Table4, Table5, Table6, Table7>(Expression<Func<Table1, Table2, Table3, Table4, Table5, Table6, Table7, object>> fields)
        {
            return InternalSelect(fields);
        }

        public virtual SqlExpression<T> SelectDistinct(Expression<Func<T, object>> fields)
        {
            return InternalSelect(fields, distinct:true);
        }

        public virtual SqlExpression<T> SelectDistinct<Table1, Table2>(Expression<Func<Table1, Table2, object>> fields)
        {
            return InternalSelect(fields, distinct: true);
        }

        public virtual SqlExpression<T> SelectDistinct<Table1, Table2, Table3>(Expression<Func<Table1, Table2, Table3, object>> fields)
        {
            return InternalSelect(fields, distinct: true);
        }

        public virtual SqlExpression<T> SelectDistinct<Table1, Table2, Table3, Table4>(Expression<Func<Table1, Table2, Table3, Table4, object>> fields)
        {
            return InternalSelect(fields, distinct: true);
        }

        public virtual SqlExpression<T> SelectDistinct<Table1, Table2, Table3, Table4, Table5>(Expression<Func<Table1, Table2, Table3, Table4, Table5, object>> fields)
        {
            return InternalSelect(fields, distinct: true);
        }

        public virtual SqlExpression<T> SelectDistinct<Table1, Table2, Table3, Table4, Table5, Table6>(Expression<Func<Table1, Table2, Table3, Table4, Table5, Table6, object>> fields)
        {
            return InternalSelect(fields, distinct: true);
        }

        public virtual SqlExpression<T> SelectDistinct<Table1, Table2, Table3, Table4, Table5, Table6, Table7>(Expression<Func<Table1, Table2, Table3, Table4, Table5, Table6, Table7, object>> fields)
        {
            return InternalSelect(fields, distinct: true);
        }

        public virtual SqlExpression<T> SelectDistinct()
        {
            selectDistinct = true;
            return this;
        }

        public virtual SqlExpression<T> From(string tables)
        {
            tables?.SqlVerifyFragment();

            return UnsafeFrom(tables);
        }

        public virtual SqlExpression<T> UnsafeFrom(string rawFrom)
        {
            if (string.IsNullOrEmpty(rawFrom))
            {
                FromExpression = null;
            }
            else
            {
                var singleTable = rawFrom.ToLower().IndexOfAny("join", ",") == -1;
                FromExpression = singleTable
                    ? " \nFROM " + DialectProvider.GetQuotedTableName(rawFrom)
                    : " \nFROM " + rawFrom;
            }

            return this;
        }

        public virtual SqlExpression<T> Where()
        {
            underlyingExpression = null; //Where() clears the expression

            whereExpression = null;
            return this;
        }

        private string FormatFilter(string sqlFilter, params object[] filterParams)
        {
            if (string.IsNullOrEmpty(sqlFilter))
                return null;

            for (var i = 0; i < filterParams.Length; i++)
            {
                var pLiteral = "{" + i + "}";
                var filterParam = filterParams[i];
                var sqlParams = filterParam as SqlInValues;

                if (sqlParams != null)
                {
                    var sqlIn = CreateInParamSql(sqlParams.GetValues());
                    sqlFilter = sqlFilter.Replace(pLiteral, sqlIn);
                }
                else
                {
                    var p = AddParam(filterParam);
                    sqlFilter = sqlFilter.Replace(pLiteral, p.ParameterName);
                }
            }
            return sqlFilter;
        }

        private string CreateInParamSql(IEnumerable values)
        {
            var sbParams = StringBuilderCache.Allocate();
            foreach (var item in values)
            {
                var p = AddParam(item);

                if (sbParams.Length > 0)
                    sbParams.Append(",");

                sbParams.Append(p.ParameterName);
            }
            var sqlIn = StringBuilderCache.ReturnAndFree(sbParams);
            return sqlIn;
        }

        public virtual SqlExpression<T> UnsafeWhere(string rawSql, params object[] filterParams)
        {
            return AppendToWhere("AND", FormatFilter(rawSql, filterParams));
        }

        public virtual SqlExpression<T> Where(string sqlFilter, params object[] filterParams)
        {
            return AppendToWhere("AND", FormatFilter(sqlFilter.SqlVerifyFragment(), filterParams));
        }

        public virtual SqlExpression<T> UnsafeAnd(string rawSql, params object[] filterParams)
        {
            return AppendToWhere("AND", FormatFilter(rawSql, filterParams));
        }

        public virtual SqlExpression<T> And(string sqlFilter, params object[] filterParams)
        {
            return AppendToWhere("AND", FormatFilter(sqlFilter.SqlVerifyFragment(), filterParams));
        }

        public virtual SqlExpression<T> UnsafeOr(string rawSql, params object[] filterParams)
        {
            return AppendToWhere("OR", FormatFilter(rawSql, filterParams));
        }

        public virtual SqlExpression<T> Or(string sqlFilter, params object[] filterParams)
        {
            return AppendToWhere("OR", FormatFilter(sqlFilter.SqlVerifyFragment(), filterParams));
        }

        public virtual SqlExpression<T> AddCondition(string condition, string sqlFilter, params object[] filterParams)
        {
            return AppendToWhere(condition, FormatFilter(sqlFilter.SqlVerifyFragment(), filterParams));
        }

        public virtual SqlExpression<T> Where(Expression<Func<T, bool>> predicate)
        {
            return AppendToWhere("AND", predicate);
        }

        public virtual SqlExpression<T> And(Expression<Func<T, bool>> predicate)
        {
            return AppendToWhere("AND", predicate);
        }

        public virtual SqlExpression<T> Or(Expression<Func<T, bool>> predicate)
        {
            return AppendToWhere("OR", predicate);
        }

        protected SqlExpression<T> AppendToWhere(string condition, Expression predicate)
        {
            if (predicate == null)
                return this;

            useFieldName = true;
            sep = " ";
            var newExpr = WhereExpressionToString(Visit(predicate));
            return AppendToWhere(condition, newExpr);
        }

        private static string WhereExpressionToString(object expression)
        {
            if (expression is bool)
                return (bool)expression ? TrueLiteral : FalseLiteral;
            return expression.ToString();
        }

        protected SqlExpression<T> AppendToWhere(string condition, string sqlExpression)
        {
            whereExpression = string.IsNullOrEmpty(whereExpression)
                ? (WhereStatementWithoutWhereString ? "" : "WHERE ")
                : whereExpression + " " + condition + " ";

            whereExpression += sqlExpression;
            return this;
        }

        public virtual SqlExpression<T> GroupBy()
        {
            return GroupBy(string.Empty);
        }

        public virtual SqlExpression<T> GroupBy(string groupBy)
        {
            groupBy.SqlVerifyFragment();
            if (!string.IsNullOrEmpty(groupBy))
                this.groupBy = "GROUP BY " + groupBy;
            return this;
        }

        private SqlExpression<T> InternalGroupBy(Expression keySelector)
        {
            sep = string.Empty;
            useFieldName = true;

            var groupByKey = Visit(keySelector);
            StripAliases(groupByKey as SelectList); // No "AS ColumnAlias" in GROUP BY, just the column names/expressions

            return GroupBy(groupByKey.ToString());
        }

        public virtual SqlExpression<T> GroupBy<Table>(Expression<Func<Table, object>> keySelector)
        {
            return InternalGroupBy(keySelector);
        }

        public virtual SqlExpression<T> GroupBy<Table1, Table2>(Expression<Func<Table1, Table2, object>> keySelector)
        {
            return InternalGroupBy(keySelector);
        }

        public virtual SqlExpression<T> GroupBy<Table1, Table2, Table3>(Expression<Func<Table1, Table2, Table3, object>> keySelector)
        {
            return InternalGroupBy(keySelector);
        }

        public virtual SqlExpression<T> GroupBy<Table1, Table2, Table3, Table4>(Expression<Func<Table1, Table2, Table3, Table4, object>> keySelector)
        {
            return InternalGroupBy(keySelector);
        }

        public virtual SqlExpression<T> GroupBy(Expression<Func<T, object>> keySelector)
        {
            return InternalGroupBy(keySelector);
        }

        public virtual SqlExpression<T> Having()
        {
            return Having(string.Empty);
        }

        public virtual SqlExpression<T> Having(string sqlFilter, params object[] filterParams)
        {
            havingExpression = FormatFilter(sqlFilter.SqlVerifyFragment(), filterParams);

            if (havingExpression != null)
                havingExpression = "HAVING " + havingExpression;

            return this;
        }

        public virtual SqlExpression<T> UnsafeHaving(string sqlFilter, params object[] filterParams)
        {
            havingExpression = FormatFilter(sqlFilter, filterParams);

            if (havingExpression != null)
                havingExpression = "HAVING " + havingExpression;

            return this;
        }

        public virtual SqlExpression<T> Having(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                useFieldName = true;
                sep = " ";
                havingExpression = Visit(predicate).ToString();
                if (!string.IsNullOrEmpty(havingExpression))
                    havingExpression = "HAVING " + havingExpression;
            }
            else
                havingExpression = string.Empty;

            return this;
        }

        public virtual SqlExpression<T> OrderBy()
        {
            return OrderBy(string.Empty);
        }

        public virtual SqlExpression<T> OrderBy(string orderBy)
        {
            return UnsafeOrderBy(orderBy.SqlVerifyFragment());
        }

        public virtual SqlExpression<T> OrderBy(long columnIndex)
        {
            return UnsafeOrderBy(columnIndex.ToString());
        }

        public virtual SqlExpression<T> UnsafeOrderBy(string orderBy)
        {
            orderByProperties.Clear();
            this.orderBy = string.IsNullOrEmpty(orderBy)
                ? null
                : "ORDER BY " + orderBy;
            return this;
        }

        public virtual SqlExpression<T> OrderByRandom()
        {
            return OrderBy("RAND()");
        }

        public ModelDefinition GetModelDefinition(FieldDefinition fieldDef)
        {
            if (modelDef.FieldDefinitions.Any(x => x == fieldDef))
                return modelDef;

            return tableDefs
                .FirstOrDefault(tableDef => tableDef.FieldDefinitions.Any(x => x == fieldDef));
        }

        private SqlExpression<T> OrderByFields(string orderBySuffix, FieldDefinition[] fields)
        {
            orderByProperties.Clear();

            if (fields.Length == 0)
            {
                this.orderBy = null;
                return this;
            }

            useFieldName = true;

            var sbOrderBy = StringBuilderCache.Allocate();
            foreach (var field in fields)
            {
                var tableDef = GetModelDefinition(field);
                var qualifiedName = modelDef != null
                    ? GetQuotedColumnName(tableDef, field.Name)
                    : DialectProvider.GetQuotedColumnName(field);

                if (sbOrderBy.Length > 0)
                    sbOrderBy.Append(", ");

                sbOrderBy.Append(qualifiedName + orderBySuffix);
            }

            this.orderBy = "ORDER BY " + StringBuilderCache.ReturnAndFree(sbOrderBy);
            return this;
        }

        static class OrderBySuffix
        {
            public const string Asc = "";
            public const string Desc = " DESC";
        }

        public virtual SqlExpression<T> OrderByFields(params FieldDefinition[] fields)
        {
            return OrderByFields(OrderBySuffix.Asc, fields);
        }

        public virtual SqlExpression<T> OrderByFieldsDescending(params FieldDefinition[] fields)
        {
            return OrderByFields(OrderBySuffix.Desc, fields);
        }

        private SqlExpression<T> OrderByFields(string orderBySuffix, string[] fieldNames)
        {
            orderByProperties.Clear();

            if (fieldNames.Length == 0)
            {
                this.orderBy = null;
                return this;
            }

            useFieldName = true;

            var sbOrderBy = StringBuilderCache.Allocate();
            foreach (var fieldName in fieldNames)
            {
                var reverse = fieldName.StartsWith("-");
                var useSuffix = reverse
                    ? (orderBySuffix == OrderBySuffix.Asc ? OrderBySuffix.Desc : OrderBySuffix.Asc)
                    : orderBySuffix;
                var useName = reverse ? fieldName.Substring(1) : fieldName;

                var field = FirstMatchingField(useName);
                if (field == null)
                    throw new ArgumentException("Could not find field " + useName);
                var qualifiedName = GetQuotedColumnName(field.Item1, field.Item2.Name);

                if (sbOrderBy.Length > 0)
                    sbOrderBy.Append(", ");

                sbOrderBy.Append(qualifiedName + useSuffix);
            }

            this.orderBy = "ORDER BY " + StringBuilderCache.ReturnAndFree(sbOrderBy);
            return this;
        }

        public virtual SqlExpression<T> OrderByFields(params string[] fieldNames)
        {
            return OrderByFields("", fieldNames);
        }

        public virtual SqlExpression<T> OrderByFieldsDescending(params string[] fieldNames)
        {
            return OrderByFields(" DESC", fieldNames);
        }

        public virtual SqlExpression<T> OrderBy(Expression<Func<T, object>> keySelector)
        {
            return OrderByInternal(keySelector);
        }

        public virtual SqlExpression<T> OrderBy<Table>(Expression<Func<Table, object>> keySelector)
        {
            return OrderByInternal(keySelector);
        }

        private SqlExpression<T> OrderByInternal(Expression keySelector)
        {
            sep = string.Empty;
            useFieldName = true;
            orderByProperties.Clear();
            var orderBySql = Visit(keySelector);
            if (IsSqlClass(orderBySql))
            {
                var fields = orderBySql.ToString();
                orderByProperties.Add(fields);
                BuildOrderByClauseInternal();
            }
            return this;
        }

        private static bool IsSqlClass(object obj)
        {
            return obj != null &&
                   (obj is PartialSqlString ||
                    obj is SelectList);
        }

        public virtual SqlExpression<T> ThenBy(string orderBy)
        {
            orderBy.SqlVerifyFragment();
            orderByProperties.Add(orderBy);
            BuildOrderByClauseInternal();
            return this;
        }

        public virtual SqlExpression<T> ThenBy(Expression<Func<T, object>> keySelector)
        {
            return ThenByInternal(keySelector);
        }

        public virtual SqlExpression<T> ThenBy<Table>(Expression<Func<Table, object>> keySelector)
        {
            return ThenByInternal(keySelector);
        }

        private SqlExpression<T> ThenByInternal(Expression keySelector)
        {
            sep = string.Empty;
            useFieldName = true;
            var orderBySql = Visit(keySelector);
            if (IsSqlClass(orderBySql))
            {
                var fields = orderBySql.ToString();
                orderByProperties.Add(fields);
                BuildOrderByClauseInternal();
            }
            return this;
        }

        public virtual SqlExpression<T> OrderByDescending(Expression<Func<T, object>> keySelector)
        {
            return OrderByDescendingInternal(keySelector);
        }

        public virtual SqlExpression<T> OrderByDescending<Table>(Expression<Func<Table, object>> keySelector)
        {
            return OrderByDescendingInternal(keySelector);
        }

        private SqlExpression<T> OrderByDescendingInternal(Expression keySelector)
        {
            sep = string.Empty;
            useFieldName = true;
            orderByProperties.Clear();
            var orderBySql = Visit(keySelector);
            if (IsSqlClass(orderBySql))
            {
                var fields = orderBySql.ToString();
                fields.ParseTokens()
                    .Each(x => orderByProperties.Add(x + " DESC"));
                BuildOrderByClauseInternal();
            }
            return this;
        }

        public virtual SqlExpression<T> OrderByDescending(string orderBy)
        {
            return UnsafeOrderByDescending(orderBy.SqlVerifyFragment());
        }

        public virtual SqlExpression<T> OrderByDescending(long columnIndex)
        {
            return UnsafeOrderByDescending(columnIndex.ToString());
        }

        private SqlExpression<T> UnsafeOrderByDescending(string orderBy)
        {
            orderByProperties.Clear();
            orderByProperties.Add(orderBy + " DESC");
            BuildOrderByClauseInternal();
            return this;
        }

        public virtual SqlExpression<T> ThenByDescending(string orderBy)
        {
            orderBy.SqlVerifyFragment();
            orderByProperties.Add(orderBy + " DESC");
            BuildOrderByClauseInternal();
            return this;
        }

        public virtual SqlExpression<T> ThenByDescending(Expression<Func<T, object>> keySelector)
        {
            return ThenByDescendingInternal(keySelector);
        }

        public virtual SqlExpression<T> ThenByDescending<Table>(Expression<Func<Table, object>> keySelector)
        {
            return ThenByDescendingInternal(keySelector);
        }

        private SqlExpression<T> ThenByDescendingInternal(Expression keySelector)
        {
            sep = string.Empty;
            useFieldName = true;
            var orderBySql = Visit(keySelector);
            if (IsSqlClass(orderBySql))
            {
                var fields = orderBySql.ToString();
                fields.ParseTokens()
                    .Each(x => orderByProperties.Add(x + " DESC"));
                BuildOrderByClauseInternal();
            }
            return this;
        }

        private void BuildOrderByClauseInternal()
        {
            if (orderByProperties.Count > 0)
            {
                var sb = StringBuilderCache.Allocate();
                foreach (var prop in orderByProperties)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");

                    sb.Append(prop);
                }
                orderBy = "ORDER BY " + StringBuilderCache.ReturnAndFree(sb);
            }
            else
            {
                orderBy = null;
            }
        }

        /// <summary>
        /// Offset of the first row to return. The offset of the initial row is 0
        /// </summary>
        public virtual SqlExpression<T> Skip(int? skip = null)
        {
            Offset = skip;
            return this;
        }

        /// <summary>
        /// Number of rows returned by a SELECT statement
        /// </summary>
        public virtual SqlExpression<T> Take(int? take = null)
        {
            Rows = take;
            return this;
        }

        /// <summary>
        /// Set the specified offset and rows for SQL Limit clause.
        /// </summary>
        /// <param name='skip'>
        /// Offset of the first row to return. The offset of the initial row is 0
        /// </param>
        /// <param name='rows'>
        /// Number of rows returned by a SELECT statement
        /// </param>	
        public virtual SqlExpression<T> Limit(int skip, int rows)
        {
            Offset = skip;
            Rows = rows;
            return this;
        }

        /// <summary>
        /// Set the specified offset and rows for SQL Limit clause where they exist.
        /// </summary>
        /// <param name='skip'>
        /// Offset of the first row to return. The offset of the initial row is 0
        /// </param>
        /// <param name='rows'>
        /// Number of rows returned by a SELECT statement
        /// </param>	
        public virtual SqlExpression<T> Limit(int? skip, int? rows)
        {
            Offset = skip;
            Rows = rows;
            return this;
        }

        /// <summary>
        /// Set the specified rows for Sql Limit clause.
        /// </summary>
        /// <param name='rows'>
        /// Number of rows returned by a SELECT statement
        /// </param>
        public virtual SqlExpression<T> Limit(int rows)
        {
            Offset = null;
            Rows = rows;
            return this;
        }

        /// <summary>
        /// Clear Sql Limit clause
        /// </summary>
        public virtual SqlExpression<T> Limit()
        {
            Offset = null;
            Rows = null;
            return this;
        }

        /// <summary>
        /// Clear Offset and Limit clauses. Alias for Limit()
        /// </summary>
        /// <returns></returns>
        public virtual SqlExpression<T> ClearLimits()
        {
            return Limit();
        }

        /// <summary>
        /// Fields to be updated.
        /// </summary>
        /// <param name='updatefields'>
        /// List&lt;string&gt; containing Names of properties to be updated
        /// </param>
        public virtual SqlExpression<T> Update(List<string> updateFields)
        {
            this.UpdateFields = updateFields;
            return this;
        }

        /// <summary>
        /// Fields to be updated.
        /// </summary>
        /// <param name='updatefields'>
        /// IEnumerable&lt;string&gt; containing Names of properties to be updated
        /// </param>
        public virtual SqlExpression<T> Update(IEnumerable<string> updateFields)
        {
            this.UpdateFields = new List<string>(updateFields);
            return this;
        }

        /// <summary>
        /// Fields to be updated.
        /// </summary>
        /// <param name='fields'>
        /// x=> x.SomeProperty1 or x=> new { x.SomeProperty1, x.SomeProperty2 }
        /// </param>
        public virtual SqlExpression<T> Update(Expression<Func<T, object>> fields)
        {
            sep = string.Empty;
            useFieldName = false;
            this.UpdateFields = fields.GetFieldNames().ToList();
            return this;
        }

        /// <summary>
        /// Clear UpdateFields list ( all fields will be updated)
        /// </summary>
        public virtual SqlExpression<T> Update()
        {
            this.UpdateFields = new List<string>();
            return this;
        }

        /// <summary>
        /// Fields to be inserted.
        /// </summary>
        /// <param name='fields'>
        /// x=> x.SomeProperty1 or x=> new{ x.SomeProperty1, x.SomeProperty2}
        /// </param>
        /// <typeparam name='TKey'>
        /// objectWithProperties
        /// </typeparam>
        public virtual SqlExpression<T> Insert<TKey>(Expression<Func<T, TKey>> fields)
        {
            sep = string.Empty;
            useFieldName = false;
            var fieldList = Visit(fields);
            InsertFields = fieldList.ToString().Split(',').Select(f => f.Trim()).ToList();
            return this;
        }

        /// <summary>
        /// fields to be inserted.
        /// </summary>
        /// <param name='insertFields'>
        /// IList&lt;string&gt; containing Names of properties to be inserted
        /// </param>
        public virtual SqlExpression<T> Insert(List<string> insertFields)
        {
            this.InsertFields = insertFields;
            return this;
        }

        /// <summary>
        /// Clear InsertFields list ( all fields will be inserted)
        /// </summary>
        public virtual SqlExpression<T> Insert()
        {
            this.InsertFields = new List<string>();
            return this;
        }

        public virtual SqlExpression<T> WithSqlFilter(Func<string,string> sqlFilter)
        {
            this.SqlFilter = sqlFilter;
            return this;
        }

        public string SqlTable(ModelDefinition modelDef)
        {
            return DialectProvider.GetQuotedTableName(modelDef);
        }

        public string SqlColumn(string columnName)
        {
            return DialectProvider.GetQuotedColumnName(columnName);
        }

        public virtual IDbDataParameter AddParam(object value)
        {
            var paramName = Params.Count.ToString();
            var paramValue = value;

            var parameter = CreateParam(paramName, paramValue);
            Params.Add(parameter);
            return parameter;
        }

        public string ConvertToParam(object value)
        {
            var p = AddParam(value);
            return p.ParameterName;
        }

        public virtual void CopyParamsTo(IDbCommand dbCmd)
        {
            try
            {
                foreach (var sqlParam in Params)
                {
                    dbCmd.Parameters.Add(sqlParam);
                }
            }
            catch (Exception)
            {
                //SQL Server + PostgreSql doesn't allow re-using db params in multiple queries
                foreach (var sqlParam in Params)
                {
                    var p = dbCmd.CreateParameter();
                    p.PopulateWith(sqlParam);
                    dbCmd.Parameters.Add(p);
                }
            }
        }

        public virtual string ToDeleteRowStatement()
        {
            string sql;
            var hasTableJoin = tableDefs.Count > 1;
            if (hasTableJoin)
            {
                var clone = this.Clone();
                var pk = DialectProvider.GetQuotedColumnName(modelDef, modelDef.PrimaryKey);
                clone.Select(pk);
                var subSql = clone.ToSelectStatement();
                sql = $"DELETE FROM {DialectProvider.GetQuotedTableName(modelDef)} WHERE {pk} IN ({subSql})";
            }
            else
            {
                sql = $"DELETE FROM {DialectProvider.GetQuotedTableName(modelDef)} {WhereExpression}";
            }

            return SqlFilter != null
                ? SqlFilter(sql)
                : sql;
        }

        public virtual void PrepareUpdateStatement(IDbCommand dbCmd, T item, bool excludeDefaults = false)
        {
            CopyParamsTo(dbCmd);

            var setFields = StringBuilderCache.Allocate();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.ShouldSkipUpdate()) continue;
                if (fieldDef.IsRowVersion) continue;
                if (UpdateFields.Count > 0
                    && !UpdateFields.Contains(fieldDef.Name)) continue; // added

                var value = fieldDef.GetValue(item);
                if (excludeDefaults
                    && (value == null || (!fieldDef.IsNullable && value.Equals(value.GetType().GetDefaultValue()))))
                    continue;

                if (setFields.Length > 0)
                    setFields.Append(", ");

                setFields
                    .Append(DialectProvider.GetQuotedColumnName(fieldDef.FieldName))
                    .Append("=")
                    .Append(DialectProvider.AddParam(dbCmd, value, fieldDef).ParameterName);
            }

            if (setFields.Length == 0)
                throw new ArgumentException("No non-null or non-default values were provided for type: " + typeof(T).Name);

            var sql = $"UPDATE {DialectProvider.GetQuotedTableName(modelDef)} " +
                      $"SET {StringBuilderCache.ReturnAndFree(setFields)} {WhereExpression}";

            dbCmd.CommandText = SqlFilter != null
                ? SqlFilter(sql)
                : sql;
        }

        public virtual string ToSelectStatement()
        {
            SelectFilter?.Invoke(this);
            OrmLiteConfig.SqlExpressionSelectFilter?.Invoke(GetUntyped());

            var sql = DialectProvider
                .ToSelectStatement(modelDef, SelectExpression, BodyExpression, OrderByExpression, Offset, Rows);

            return SqlFilter != null
                ? SqlFilter(sql)
                : sql;
        }

        public virtual string ToCountStatement()
        {
            SelectFilter?.Invoke(this);
            OrmLiteConfig.SqlExpressionSelectFilter?.Invoke(GetUntyped());

            var sql = "SELECT COUNT(*)" + BodyExpression;

            return SqlFilter != null
                ? SqlFilter(sql)
                : sql;
        }

        public string SelectExpression
        {
            get
            {
                if (string.IsNullOrEmpty(selectExpression))
                    BuildSelectExpression(string.Empty, false);
                return selectExpression;
            }
            set
            {
                selectExpression = value;
            }
        }

        public string FromExpression
        {
            get
            {
                return string.IsNullOrEmpty(fromExpression)
                    ? " \nFROM " + DialectProvider.GetQuotedTableName(modelDef)
                    : fromExpression;
            }
            set { fromExpression = value; }
        }

        public string BodyExpression => FromExpression
            + (string.IsNullOrEmpty(WhereExpression) ? "" : "\n" + WhereExpression)
            + (string.IsNullOrEmpty(GroupByExpression) ? "" : "\n" + GroupByExpression)
            + (string.IsNullOrEmpty(HavingExpression) ? "" : "\n" + HavingExpression);

        public string WhereExpression
        {
            get
            {
                return whereExpression;
            }
            set
            {
                whereExpression = value;
            }
        }

        public string GroupByExpression
        {
            get
            {
                return groupBy;
            }
            set
            {
                groupBy = value;
            }
        }

        public string HavingExpression
        {
            get
            {
                return havingExpression;
            }
            set
            {
                havingExpression = value;
            }
        }


        public string OrderByExpression
        {
            get
            {
                return string.IsNullOrEmpty(orderBy) ? "" : "\n" + orderBy;
            }
            set
            {
                orderBy = value;
            }
        }

        public int? Rows { get; set; }
        public int? Offset { get; set; }

        public ModelDefinition ModelDef
        {
            get
            {
                return modelDef;
            }
            protected set
            {
                modelDef = value;
            }
        }

        protected internal bool UseFieldName
        {
            get
            {
                return useFieldName;
            }
            set
            {
                useFieldName = value;
            }
        }

        public virtual object Visit(Expression exp)
        {
            visitedExpressionIsTableColumn = false;

            if (exp == null)
                return string.Empty;

            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return VisitLambda(exp as LambdaExpression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess(exp as MemberExpression);
                case ExpressionType.Constant:
                    return VisitConstant(exp as ConstantExpression);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    //return "(" + VisitBinary(exp as BinaryExpression) + ")";
                    return VisitBinary(exp as BinaryExpression);
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary(exp as UnaryExpression);
                case ExpressionType.Parameter:
                    return VisitParameter(exp as ParameterExpression);
                case ExpressionType.Call:
                    return VisitMethodCall(exp as MethodCallExpression);
                case ExpressionType.New:
                    return VisitNew(exp as NewExpression);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray(exp as NewArrayExpression);
                case ExpressionType.MemberInit:
                    return VisitMemberInit(exp as MemberInitExpression);
                case ExpressionType.Index:
                    return VisitIndexExpression(exp as IndexExpression);
                case ExpressionType.Conditional:
                    return VisitConditional(exp as ConditionalExpression);
                default:
                    return exp.ToString();
            }
        }

        protected internal virtual object VisitJoin(Expression exp)
        {
            skipParameterizationForThisExpression = true;
            var visitedExpression = Visit(exp);
            skipParameterizationForThisExpression = false;
            return visitedExpression;
        }

        protected virtual object VisitLambda(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.MemberAccess && sep == " ")
            {
                MemberExpression m = lambda.Body as MemberExpression;

                if (m.Expression != null)
                {
                    var r = VisitMemberAccess(m);
                    if (!(r is PartialSqlString))
                        return r;

                    if (m.Expression.Type.IsNullableType())
                        return r.ToString();

                    return $"{r}={GetQuotedTrueValue()}";
                }

            }
            return Visit(lambda.Body);
        }

        public virtual object GetValue(object value, Type type)
        {
            if (skipParameterizationForThisExpression)
                return DialectProvider.GetQuotedValue(value, type);

            var paramValue = DialectProvider.GetParamValue(value, type);
            return paramValue ?? "null";
        }

        protected virtual object VisitBinary(BinaryExpression b)
        {
            object originalLeft = null, originalRight = null, left, right;
            var operand = BindOperant(b.NodeType);   //sep= " " ??
            if (operand == "AND" || operand == "OR")
            {
                if (IsBooleanComparison(b.Left))
                {
                    left = VisitMemberAccess((MemberExpression) b.Left);
                    if (left is PartialSqlString)
                        left = new PartialSqlString($"{left}={GetQuotedTrueValue()}");
                }
                else left = Visit(b.Left);

                if (IsBooleanComparison(b.Right))
                {
                    right = VisitMemberAccess((MemberExpression)b.Right);
                    if (right is PartialSqlString)
                        right = new PartialSqlString($"{right}={GetQuotedTrueValue()}");
                }
                else right = Visit(b.Right);

                if (!(left is PartialSqlString) && !(right is PartialSqlString))
                {
                    var result = CachedExpressionCompiler.Evaluate(PreEvaluateBinary(b, left, right));
                    return result;
                }

                if (!(left is PartialSqlString))
                    left = ((bool)left) ? GetTrueExpression() : GetFalseExpression();
                if (!(right is PartialSqlString))
                    right = ((bool)right) ? GetTrueExpression() : GetFalseExpression();
            }
            else if ((operand == "=" || operand == "<>") && b.Left is MethodCallExpression && ((MethodCallExpression)b.Left).Method.Name == "CompareString")
            {
                //Handle VB.NET converting (x => x.Name == "Foo") into (x => CompareString(x.Name, "Foo", False)
                var methodExpr = (MethodCallExpression)b.Left;
                var args = this.VisitExpressionList(methodExpr.Arguments);
                right = GetValue(args[1], typeof(string));
                ConvertToPlaceholderAndParameter(ref right);
                return new PartialSqlString($"({args[0]} {operand} {right})");
            }
            else
            {
                originalLeft = left = Visit(b.Left);
                originalRight = right = Visit(b.Right);

                // Handle "expr = true/false", including with the constant on the left

                if (operand == "=" || operand == "<>")
                {
                    if (left is bool)
                    {
                        Swap(ref left, ref right); // Should be safe to swap for equality/inequality checks
                    }

                    if (right is bool && !IsFieldName(left)) // Don't change anything when "expr" is a column name - then we really want "ColName = 1"
                    {
                        if (operand == "=")
                            return (bool)right ? left : GetNotValue(left); // "expr == true" becomes "expr", "expr == false" becomes "not (expr)"
                        if (operand == "<>")
                            return (bool)right ? GetNotValue(left) : left; // "expr != true" becomes "not (expr)", "expr != false" becomes "expr"
                    }
                }

                var leftEnum = left as EnumMemberAccess;
                var rightEnum = right as EnumMemberAccess;

                var rightNeedsCoercing = leftEnum != null && rightEnum == null;
                var leftNeedsCoercing = rightEnum != null && leftEnum == null;

                if (rightNeedsCoercing)
                {
                    var rightPartialSql = right as PartialSqlString;
                    if (rightPartialSql == null)
                    {
                        right = GetValue(right, leftEnum.EnumType);
                    }
                }
                else if (leftNeedsCoercing)
                {
                    var leftPartialSql = left as PartialSqlString;
                    if (leftPartialSql == null)
                    {
                        left = DialectProvider.GetQuotedValue(left, rightEnum.EnumType);
                    }
                }
                else if (!(left is PartialSqlString) && !(right is PartialSqlString))
                {
                    var evaluatedValue = CachedExpressionCompiler.Evaluate(PreEvaluateBinary(b, left, right));
                    var result = VisitConstant(Expression.Constant(evaluatedValue));
                    return result;
                }
                else if (!(left is PartialSqlString))
                {
                    left = DialectProvider.GetQuotedValue(left, left?.GetType());
                }
                else if (!(right is PartialSqlString))
                {
                    right = GetValue(right, right?.GetType());
                }
            }

            if (left.ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                Swap(ref left, ref right); // "null is x" will not work, so swap the operands
            }

            var separator = sep;
            if (right.ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                if (operand == "=")
                    operand = "is";
                else if (operand == "<>")
                    operand = "is not";

                separator = " ";
            }

            if (operand == "+" && b.Left.Type == typeof(string) && b.Right.Type == typeof(string))
                return BuildConcatExpression(new List<object> {left, right});

            VisitFilter(operand, originalLeft, originalRight, ref left, ref right);

            switch (operand)
            {
                case "MOD":
                case "COALESCE":
                    return new PartialSqlString($"{operand}({left},{right})");
                default:
                    return new PartialSqlString("(" + left + separator + operand + separator + right + ")");
            }
        }

        private BinaryExpression PreEvaluateBinary(BinaryExpression b, object left, object right)
        {
            var visitedBinaryExp = b;

            if (IsParameterAccess(b.Left) || IsParameterAccess(b.Right))
            {
                var eLeft = !IsParameterAccess(b.Left) ? b.Left : Expression.Constant(left, b.Left.Type);
                var eRight = !IsParameterAccess(b.Right) ? b.Right : Expression.Constant(right, b.Right.Type);
                if (b.NodeType == ExpressionType.Coalesce)
                    visitedBinaryExp = Expression.Coalesce(eLeft, eRight, b.Conversion);
                else
                    visitedBinaryExp = Expression.MakeBinary(b.NodeType, eLeft, eRight, b.IsLiftedToNull, b.Method);
            }

            return visitedBinaryExp;
        }

        /// <summary>
        /// Determines whether the expression is the parameter inside MemberExpression which should be compared with TrueExpression.
        /// </summary>
        /// <returns>Returns true if the specified expression is the parameter inside MemberExpression which should be compared with TrueExpression;
        /// otherwise, false.</returns>
        protected virtual bool IsBooleanComparison(Expression e)
        {
            if (!(e is MemberExpression)) return false;

            var m = (MemberExpression)e;

            if (m.Member.DeclaringType.IsNullableType() &&
                m.Member.Name == "HasValue") //nameof(Nullable<bool>.HasValue)
                return false;

            return IsParameterAccess(m);
        }

        /// <summary>
        /// Determines whether the expression is the parameter.
        /// </summary>
        /// <returns>Returns true if the specified expression is parameter;
        /// otherwise, false.</returns>
        protected virtual bool IsParameterAccess(Expression e)
        {
            return CheckExpressionForTypes(e, new[] { ExpressionType.Parameter });
        }

        /// <summary>
        /// Determines whether the expression is a Parameter or Convert Expression.
        /// </summary>
        /// <returns>Returns true if the specified expression is parameter or convert;
        /// otherwise, false.</returns>
        protected virtual bool IsParameterOrConvertAccess(Expression e)
        {
            return CheckExpressionForTypes(e, new[] { ExpressionType.Parameter, ExpressionType.Convert });
        }

        protected bool CheckExpressionForTypes(Expression e, ExpressionType[] types)
        {
            while (e != null)
            {
                if (types.Contains(e.NodeType))
                {
                    var subUnaryExpr = e as UnaryExpression;
                    var isSubExprAccess = subUnaryExpr?.Operand is IndexExpression;
                    if (!isSubExprAccess)
                        return true;
                }

                var binaryExpr = e as BinaryExpression;
                if (binaryExpr != null)
                {
                    if (CheckExpressionForTypes(binaryExpr.Left, types))
                        return true;

                    if (CheckExpressionForTypes(binaryExpr.Right, types))
                        return true;
                }

                var methodCallExpr = e as MethodCallExpression;
                if (methodCallExpr != null)
                {
                    for (var i = 0; i < methodCallExpr.Arguments.Count; i++)
                    {
                        if (CheckExpressionForTypes(methodCallExpr.Arguments[i], types))
                            return true;
                    }

                    if (CheckExpressionForTypes(methodCallExpr.Object, types))
                        return true;
                }

                var unaryExpr = e as UnaryExpression;
                if (unaryExpr != null)
                {
                    if (CheckExpressionForTypes(unaryExpr.Operand, types))
                        return true;
                }

                var condExpr = e as ConditionalExpression;
                if (condExpr != null)
                {
                    if (CheckExpressionForTypes(condExpr.Test, types))
                        return true;

                    if (CheckExpressionForTypes(condExpr.IfTrue, types))
                        return true;

                    if (CheckExpressionForTypes(condExpr.IfFalse, types))
                        return true;
                }

                var memberExpr = e as MemberExpression;
                e = memberExpr?.Expression;
            }

            return false;
        }

        private static void Swap(ref object left, ref object right)
        {
            var temp = right;
            right = left;
            left = temp;
        }

        protected virtual void VisitFilter(string operand, object originalLeft, object originalRight, ref object left, ref object right)
        {
            if (skipParameterizationForThisExpression || visitedExpressionIsTableColumn)
                return;

            if (originalLeft is EnumMemberAccess && originalRight is EnumMemberAccess)
                return;

            if (operand == "AND" || operand == "OR" || operand == "is" || operand == "is not")
                return;

            if (!(right is PartialSqlString))
            {
                ConvertToPlaceholderAndParameter(ref right);
            }
        }

        protected virtual void ConvertToPlaceholderAndParameter(ref object right)
        {
            var parameter = AddParam(right);

            right = parameter.ParameterName;
        }

        protected virtual object VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null)
            {
                if (m.Member.DeclaringType.IsNullableType())
                {
                    if (m.Member.Name == "Value") //Can't use C# 6 yet: nameof(Nullable<bool>.Value)
                        return Visit(m.Expression);
                    if (m.Member.Name == "HasValue") //nameof(Nullable<bool>.HasValue)
                    {
                        var doesNotEqualNull = Expression.MakeBinary(ExpressionType.NotEqual, m.Expression, Expression.Constant(null));
                        return Visit(doesNotEqualNull); // Nullable<T>.HasValue is equivalent to "!= null"
                    }

                    throw new ArgumentException($"Expression '{m}' accesses unsupported property '{m.Member}' of Nullable<T>");
                }

                if (IsParameterOrConvertAccess(m))
                    return GetMemberExpression(m);
            }

            return CachedExpressionCompiler.Evaluate(m);
        }

        protected virtual object GetMemberExpression(MemberExpression m)
        {
            var propertyInfo = m.Member as PropertyInfo;

            var modelType = m.Expression.Type;
            if (m.Expression.NodeType == ExpressionType.Convert)
            {
                var unaryExpr = m.Expression as UnaryExpression;
                if (unaryExpr != null)
                {
                    modelType = unaryExpr.Operand.Type;
                }
            }

            OnVisitMemberType(modelType);

            var tableDef = modelType.GetModelDefinition();

            if (propertyInfo != null && propertyInfo.PropertyType.IsEnum())
                return new EnumMemberAccess(
                    GetQuotedColumnName(tableDef, m.Member.Name), propertyInfo.PropertyType);

            return new PartialSqlString(GetQuotedColumnName(tableDef, m.Member.Name));
        }

        protected virtual void OnVisitMemberType(Type modelType)
        {
            var tableDef = modelType.GetModelDefinition();
            if (tableDef != null)
                visitedExpressionIsTableColumn = true;
        }

        protected virtual object VisitMemberInit(MemberInitExpression exp)
        {
            return CachedExpressionCompiler.Evaluate(exp);
        }

        protected virtual object VisitNew(NewExpression nex)
        {
            var isAnonType = nex.Type.Name.StartsWith("<>");
            if (isAnonType)
            {
                var exprs = VisitExpressionList(nex.Arguments);

                for (var i = 0; i < exprs.Count; ++i)
                {
                    exprs[i] = SetAnonTypePropertyNamesForSelectExpression(exprs[i], nex.Arguments[i], nex.Members[i]);
                }

                return new SelectList(exprs);
            }

            return CachedExpressionCompiler.Evaluate(nex);
        }

        private object SetAnonTypePropertyNamesForSelectExpression(object expr, Expression arg, MemberInfo member)
        {
            // When selecting a column use the anon type property name, rather than the table property name, as the returned column name

            MemberExpression propertyExpr;
            if ((propertyExpr = arg as MemberExpression) != null && propertyExpr.Member.Name != member.Name)
                return new SelectItemExpression(DialectProvider, expr.ToString(), member.Name);

            // When selecting an entire table use the anon type property name as a prefix for the returned column name
            // to allow the caller to distinguish properties with the same names from different tables

            var paramExpr = arg as ParameterExpression;
            var selectList = paramExpr != null && paramExpr.Name != member.Name
                ? expr as SelectList
                : null;
            if (selectList != null)
            {
                foreach (var item in selectList.Items)
                {
                    var selectItem = item as SelectItem;
                    if (selectItem != null)
                    {
                        if (!string.IsNullOrEmpty(selectItem.Alias))
                        {
                            selectItem.Alias = member.Name + selectItem.Alias;
                        }
                        else
                        {
                            var columnItem = item as SelectItemColumn;
                            if (columnItem != null)
                            {
                                columnItem.Alias = member.Name + columnItem.ColumnName;
                            }
                        }
                    }
                }
            }

            var methodCallExpr = arg as MethodCallExpression;
            var mi = methodCallExpr?.Method;
            var declareType = mi?.DeclaringType;
            if (declareType != null && declareType.Name == "Sql" && mi.Name != "Desc" && mi.Name != "Asc" && mi.Name != "As" && mi.Name != "AllFields") 
                return new PartialSqlString(expr + " AS " + member.Name); // new { Count = Sql.Count("*") }

            return expr;
        }

        private static void StripAliases(SelectList selectList)
        {
            if (selectList == null)
                return;

            foreach (var item in selectList.Items)
            {
                var selectItem = item as SelectItem;
                if (selectItem != null)
                {
                    selectItem.Alias = null;
                }
            }
        }

        private class SelectList
        {
            public readonly IEnumerable<object> Items;

            public SelectList(IEnumerable<object> items)
            {
                this.Items = items;
            }

            public override string ToString()
            {
                return Items.ToSelectString();
            }
        }

        protected virtual object VisitParameter(ParameterExpression p)
        {
            var paramModelDef = p.Type.GetModelDefinition();
            if (paramModelDef != null)
                return new SelectList(DialectProvider.GetColumnNames(paramModelDef, true));

            return p.Name;
        }

        protected virtual object VisitConstant(ConstantExpression c)
        {
            if (c.Value == null)
                return new PartialSqlString("null");

            return c.Value;
        }

        protected virtual object VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    var o = Visit(u.Operand);
                    return GetNotValue(o);
                case ExpressionType.Convert:
                    if (u.Method != null)
                    {
                        var e = u.Operand;
                        if (IsParameterAccess(e))
                            return Visit(e);

                        return CachedExpressionCompiler.Evaluate(u);
                    }
                    break;
            }
            return Visit(u.Operand);
        }

        protected virtual object VisitIndexExpression(IndexExpression e)
        {
            var arg = e.Arguments[0];
            var constant = arg as ConstantExpression;
            var oIndex = constant != null
                ? constant.Value
                : CachedExpressionCompiler.Evaluate(arg);

            var index = (int)Convert.ChangeType(oIndex, typeof(int));
            var oCollection = CachedExpressionCompiler.Evaluate(e.Object);

            var list = oCollection as List<object>;
            if (list != null)
                return list[index];

            throw new NotImplementedException("Unknown Expression: " + e);
        }

        protected virtual object VisitConditional(ConditionalExpression e)
        {
            var test = IsBooleanComparison(e.Test)
                ? new PartialSqlString($"{VisitMemberAccess((MemberExpression) e.Test)}={GetQuotedTrueValue()}")
                : Visit(e.Test);

            if (test is bool)
            {
                if ((bool) test)
                {
                    var ifTrue = Visit(e.IfTrue);
                    if (e.IfTrue.Type == typeof(string) && !(ifTrue is PartialSqlString))
                        ifTrue = ConvertToParam(ifTrue);

                    return ifTrue;
                }

                var ifFalse = Visit(e.IfFalse);
                if (e.IfFalse.Type == typeof(string) && !(ifFalse is PartialSqlString))
                    ifFalse = ConvertToParam(ifFalse);

                return ifFalse;
            }
            else
            {
                var ifTrue = Visit(e.IfTrue);
                if (e.IfTrue.Type == typeof(string) && !(ifTrue is PartialSqlString))
                    ifTrue = ConvertToParam(ifTrue);

                var ifFalse = Visit(e.IfFalse);
                if (e.IfFalse.Type == typeof(string) && !(ifFalse is PartialSqlString))
                    ifFalse = ConvertToParam(ifFalse);

                return new PartialSqlString($"(CASE WHEN {test} THEN {ifTrue} ELSE {ifFalse} END)");
            }
        }

        private object GetNotValue(object o)
        {
            if (!(o is PartialSqlString))
                return !(bool) o;

            if (IsFieldName(o))
                return new PartialSqlString(o + "=" + GetQuotedFalseValue());

            return new PartialSqlString("NOT (" + o + ")");
        }

        protected virtual bool IsColumnAccess(MethodCallExpression m)
        {
            if (m.Object == null)
                return false;
            
            var methCallExp = m.Object as MethodCallExpression;
            if (methCallExp != null)
                return IsColumnAccess(methCallExp);

            var condExp = m.Object as ConditionalExpression;
            if (condExp != null)
                return IsParameterAccess(condExp);

            var exp = m.Object as MemberExpression;
            return IsParameterAccess(exp)
                   && IsJoinedTable(exp.Expression.Type);
        }

        protected virtual object VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Sql))
                return VisitSqlMethodCall(m);

            if (IsStaticArrayMethod(m))
                return VisitStaticArrayMethodCall(m);

            if (IsEnumerableMethod(m))
                return VisitEnumerableMethodCall(m);

            if (IsColumnAccess(m))
                return VisitColumnAccessMethod(m);

            if (IsStaticStringMethod(m))
                return VisitStaticStringMethodCall(m);

            return CachedExpressionCompiler.Evaluate(m);
        }

        protected virtual List<object> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            var list = new List<object>();
            for (int i = 0, n = original.Count; i < n; i++)
            {
                var e = original[i];
                if (e.NodeType == ExpressionType.NewArrayInit ||
                    e.NodeType == ExpressionType.NewArrayBounds)
                {
                    list.AddRange(VisitNewArrayFromExpressionList(e as NewArrayExpression));
                }
                else
                {
                    list.Add(Visit(e));
                }
            }
            return list;
        }

        protected virtual List<object> VisitInSqlExpressionList(ReadOnlyCollection<Expression> original)
        {
            var list = new List<object>();
            for (int i = 0, n = original.Count; i < n; i++)
            {
                var e = original[i];
                if (e.NodeType == ExpressionType.NewArrayInit ||
                    e.NodeType == ExpressionType.NewArrayBounds)
                {
                    list.AddRange(VisitNewArrayFromExpressionList(e as NewArrayExpression));
                }
                else if (e.NodeType == ExpressionType.MemberAccess)
                {
                    list.Add(VisitMemberAccess(e as MemberExpression));
                }
                else
                {
                    list.Add(Visit(e));
                }
            }
            return list;
        }

        protected virtual object VisitNewArray(NewArrayExpression na)
        {
            var exprs = VisitExpressionList(na.Expressions);
            var sb = StringBuilderCache.Allocate();
            foreach (var e in exprs)
            {
                sb.Append(sb.Length > 0 ? "," + e : e);
            }
            return StringBuilderCache.ReturnAndFree(sb);
        }

        protected virtual List<object> VisitNewArrayFromExpressionList(NewArrayExpression na)
        {
            var exprs = VisitExpressionList(na.Expressions);
            return exprs;
        }

        protected virtual string BindOperant(ExpressionType e)
        {
            switch (e)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "MOD";
                case ExpressionType.Coalesce:
                    return "COALESCE";
                default:
                    return e.ToString();
            }
        }

        protected virtual string GetQuotedColumnName(ModelDefinition tableDef, string memberName)
        {
            if (useFieldName)
            {
                var fd = tableDef.FieldDefinitions.FirstOrDefault(x => x.Name == memberName);
                var fieldName = fd != null
                    ? fd.FieldName
                    : memberName;

                if (tableDef.ModelType.IsInterface() && this.ModelDef.ModelType.HasInterface(tableDef.ModelType))
                {
                    tableDef = this.ModelDef;
                }

                var includePrefix = PrefixFieldWithTableName && fd?.CustomSelect == null && !tableDef.ModelType.IsInterface();
                return includePrefix
                    ? DialectProvider.GetQuotedColumnName(tableDef, fieldName)
                    : DialectProvider.GetQuotedColumnName(fieldName);
            }
            return memberName;
        }

        protected string RemoveQuoteFromAlias(string exp)
        {
            if ((exp.StartsWith("\"") || exp.StartsWith("`") || exp.StartsWith("'"))
                &&
                (exp.EndsWith("\"") || exp.EndsWith("`") || exp.EndsWith("'")))
            {
                exp = exp.Remove(0, 1);
                exp = exp.Remove(exp.Length - 1, 1);
            }
            return exp;
        }

        protected virtual bool IsFieldName(object quotedExp)
        {
            var fieldExpr = quotedExp.ToString().StripTablePrefixes();
            var unquotedExpr = fieldExpr.StripQuotes();

            var isTableField = modelDef.FieldDefinitionsArray
                .Any(x => GetColumnName(x.FieldName) == unquotedExpr);
            if (isTableField)
                return true;

            var isJoinedField = tableDefs.Any(t => t.FieldDefinitionsArray
                .Any(x => GetColumnName(x.FieldName) == unquotedExpr));

            return isJoinedField;
        }

        protected string GetColumnName(string fieldName)
        {
            return DialectProvider.NamingStrategy.GetColumnName(fieldName);
        }

        protected object GetTrueExpression()
        {
            return new PartialSqlString($"({GetQuotedTrueValue()}={GetQuotedTrueValue()})");
        }

        protected object GetFalseExpression()
        {
            return new PartialSqlString($"({GetQuotedTrueValue()}={GetQuotedFalseValue()})");
        }

        protected object GetQuotedTrueValue()
        {
            return new PartialSqlString(DialectProvider.GetQuotedValue(true, typeof(bool)));
        }

        protected object GetQuotedFalseValue()
        {
            return new PartialSqlString(DialectProvider.GetQuotedValue(false, typeof(bool)));
        }

        private void BuildSelectExpression(string fields, bool distinct)
        {
            OnlyFields = null;
            selectDistinct = distinct;

            selectExpression = $"SELECT {(selectDistinct ? "DISTINCT " : "")}" +
                               (string.IsNullOrEmpty(fields) ? DialectProvider.GetColumnNames(modelDef) : fields);
        }

        public IList<string> GetAllFields()
        {
            return modelDef.FieldDefinitions.ConvertAll(r => r.Name);
        }

        protected virtual bool IsStaticArrayMethod(MethodCallExpression m)
        {
            return (m.Object == null 
                && m.Method.Name == "Contains"
                && m.Arguments.Count == 2);
        }

        protected virtual object VisitStaticArrayMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Contains":
                    List<object> args = this.VisitExpressionList(m.Arguments);
                    object quotedColName = args.Last();

                    Expression memberExpr = m.Arguments[0];
                    if (memberExpr.NodeType == ExpressionType.MemberAccess)
                        memberExpr = m.Arguments[0] as MemberExpression;

                    return ToInPartialString(memberExpr, quotedColName);

                default:
                    throw new NotSupportedException();
            }
        }

        private static bool IsEnumerableMethod(MethodCallExpression m)
        {
            return m.Object != null
                && m.Object.Type.IsOrHasGenericInterfaceTypeOf(typeof(IEnumerable<>))
                && m.Object.Type != typeof(string)
                && m.Method.Name == "Contains"
                && m.Arguments.Count == 1;
        }

        protected virtual object VisitEnumerableMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Contains":
                    List<object> args = this.VisitExpressionList(m.Arguments);
                    object quotedColName = args[0];
                    return ToInPartialString(m.Object, quotedColName);

                default:
                    throw new NotSupportedException();
            }
        }

        private object ToInPartialString(Expression memberExpr, object quotedColName)
        {
            var result = CachedExpressionCompiler.Evaluate(memberExpr);

            var inArgs = Sql.Flatten(result as IEnumerable);

            var sqlIn = inArgs.Count > 0
                ? CreateInParamSql(inArgs)
                : "NULL";

            var statement = $"{quotedColName} IN ({sqlIn})";
            return new PartialSqlString(statement);
        }

        protected virtual bool IsStaticStringMethod(MethodCallExpression m)
        {
            return (m.Object == null 
                && m.Method.Name == "Concat");
        }

        protected virtual object VisitStaticStringMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Concat":
                    var args = VisitExpressionList(m.Arguments);
                    return BuildConcatExpression(args);

                default:
                    throw new NotSupportedException();
            }
        }

        private PartialSqlString BuildConcatExpression(List<object> args)
        {
            for (int i = 0; i < args.Count; i++)
            {
                if (!(args[i] is PartialSqlString))
                    args[i] = ConvertToParam(args[i]);
            }
            return ToConcatPartialString(args);
        }

        protected PartialSqlString ToConcatPartialString(List<object> args)
        {
            return new PartialSqlString(DialectProvider.SqlConcat(args));
        }

        protected virtual object VisitSqlMethodCall(MethodCallExpression m)
        {
            List<object> args = this.VisitInSqlExpressionList(m.Arguments);
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
                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }

        protected string ConvertInExpressionToSql(MethodCallExpression m, object quotedColName)
        {
            var argValue = CachedExpressionCompiler.Evaluate(m.Arguments[1]);

            if (argValue == null)
                return FalseLiteral; // "column IN (NULL)" is always false

            var enumerableArg = argValue as IEnumerable;
            if (enumerableArg != null)
            {
                var inArgs = Sql.Flatten(enumerableArg);
                if (inArgs.Count == 0)
                    return FalseLiteral; // "column IN ([])" is always false

                string sqlIn = CreateInParamSql(inArgs);
                return $"{quotedColName} IN ({sqlIn})";
            }

            var exprArg = argValue as ISqlExpression;
            if (exprArg != null)
            {
                var subSelect = exprArg.ToSelectStatement();
                var renameParams = new List<Tuple<string,string>>();
                foreach (var p in exprArg.Params)
                {
                    var oldName = p.ParameterName;
                    var newName = DialectProvider.GetParam(Params.Count.ToString());
                    if (oldName != newName)
                    {
                        var pClone = DialectProvider.CreateParam().PopulateWith(p);
                        renameParams.Add(Tuple.Create(oldName, newName));
                        pClone.ParameterName = newName;
                        Params.Add(pClone);
                    }
                    else
                    {
                        Params.Add(p);
                    }
                }

                for (var i = renameParams.Count - 1; i >= 0; i--)
                {
                    //Replace complete db params [@1] and not partial tokens [@1]0
                    var paramsRegex = new Regex(renameParams[i].Item1 + "([^\\d])");
                    subSelect = paramsRegex.Replace(subSelect, renameParams[i].Item2 + "$1");
                }
                
                return CreateInSubQuerySql(quotedColName, subSelect);
            }

            throw new NotSupportedException($"In({argValue.GetType()})");
        }

        protected virtual string CreateInSubQuerySql(object quotedColName,string subSelect)
        {
            return $"{quotedColName} IN ({subSelect})";
        }

        protected virtual object VisitColumnAccessMethod(MethodCallExpression m)
        {
            List<object> args = this.VisitExpressionList(m.Arguments);
            var quotedColName = Visit(m.Object);
            if (m.Object?.Type == typeof(string) && !(quotedColName is PartialSqlString))
                quotedColName = ConvertToParam(quotedColName);

            var statement = "";

            var wildcardArg = args.Count > 0 ? DialectProvider.EscapeWildcards(args[0].ToString()) : "";
            var escapeSuffix = wildcardArg.IndexOf('^') >= 0 ? " escape '^'" : "";
            switch (m.Method.Name)
            {
                case "Trim":
                    statement = $"ltrim(rtrim({quotedColName}))";
                    break;
                case "LTrim":
                    statement = $"ltrim({quotedColName})";
                    break;
                case "RTrim":
                    statement = $"rtrim({quotedColName})";
                    break;
                case "ToUpper":
                    statement = $"upper({quotedColName})";
                    break;
                case "ToLower":
                    statement = $"lower({quotedColName})";
                    break;
                case "StartsWith":
                    statement = !OrmLiteConfig.StripUpperInLike
                        ? $"upper({quotedColName}) like {ConvertToParam(wildcardArg.ToUpper() + "%")}{escapeSuffix}"
                        : $"{quotedColName} like {ConvertToParam(wildcardArg + "%")}{escapeSuffix}";
                    break;
                case "EndsWith":
                    statement = !OrmLiteConfig.StripUpperInLike
                        ? $"upper({quotedColName}) like {ConvertToParam("%" + wildcardArg.ToUpper())}{escapeSuffix}"
                        : $"{quotedColName} like {ConvertToParam("%" + wildcardArg)}{escapeSuffix}";
                    break;
                case "Contains":
                    statement = !OrmLiteConfig.StripUpperInLike
                        ? $"upper({quotedColName}) like {ConvertToParam("%" + wildcardArg.ToUpper() + "%")}{escapeSuffix}"
                        : $"{quotedColName} like {ConvertToParam("%" + wildcardArg + "%")}{escapeSuffix}";
                    break;
                case "Substring":
                    var startIndex = int.Parse(args[0].ToString()) + 1;
                    statement = args.Count == 2 
                        ? GetSubstringSql(quotedColName, startIndex, int.Parse(args[1].ToString())) 
                        : GetSubstringSql(quotedColName, startIndex);
                    break;
                case "ToString":
                    statement = m.Object?.Type == typeof(string) 
                        ? $"({quotedColName})"
                        : ToCast(quotedColName.ToString());
                    break;
                default:
                    throw new NotSupportedException();
            }
            return new PartialSqlString(statement);
        }

        protected virtual string ToCast(string quotedColName)
        {
            return $"cast({quotedColName} as varchar(1000))";
        }

        public virtual string GetSubstringSql(object quotedColumn, int startIndex, int? length = null)
        {
            return length != null
                ? $"substring({quotedColumn} from {startIndex} for {length.Value})"
                : $"substring({quotedColumn} from {startIndex})";
        }

        public IDbDataParameter CreateParam(string name,
            object value = null,
            ParameterDirection direction = ParameterDirection.Input,
            DbType? dbType = null,
            DataRowVersion sourceVersion = DataRowVersion.Default)
        {
            var p = DialectProvider.CreateParam();
            p.ParameterName = DialectProvider.GetParam(name);
            p.Direction = direction;
            p.SourceVersion = sourceVersion;

            if (p.DbType == DbType.String)
            {
                p.Size = DialectProvider.GetStringConverter().StringLength;
                string strValue = value as string;
                if (strValue != null && strValue.Length > p.Size)
                    p.Size = strValue.Length;
            }

            if (value != null)
            {
                DialectProvider.InitDbParam(p, value.GetType());
                p.Value = DialectProvider.GetParamValue(value, value.GetType());
            }
            else
            {
                p.Value = DBNull.Value;
            }

            if (dbType != null)
                p.DbType = dbType.Value;

            return p;
        }

        public IUntypedSqlExpression GetUntyped()
        {
            return new UntypedSqlExpressionProxy<T>(this);
        }
    }

    public interface ISqlExpression
    {
        List<IDbDataParameter> Params { get; }

        string ToSelectStatement();
        string SelectInto<TModel>();
    }

    public class PartialSqlString
    {
        public PartialSqlString(string text)
        {
            Text = text;
        }
        public string Text { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }

    public class EnumMemberAccess : PartialSqlString
    {
        public EnumMemberAccess(string text, Type enumType)
            : base(text)
        {
            if (!enumType.IsEnum()) throw new ArgumentException("Type not valid", nameof(enumType));

            EnumType = enumType;
        }

        public Type EnumType { get; private set; }
    }

    public abstract class SelectItem
    {
        protected SelectItem(IOrmLiteDialectProvider dialectProvider, string alias)
        {
            if (dialectProvider == null)
                throw new ArgumentNullException(nameof(dialectProvider));

            DialectProvider = dialectProvider;
            Alias = alias;
        }

        /// <summary>
        /// Unquoted alias for the column or expression being selected.
        /// </summary>
        public string Alias { get; set; }

        protected IOrmLiteDialectProvider DialectProvider { get; set; }

        public abstract override string ToString();
    }

    public class SelectItemExpression : SelectItem
    {
        public SelectItemExpression(IOrmLiteDialectProvider dialectProvider, string selectExpression, string alias)
            : base(dialectProvider, alias)
        {
            if (string.IsNullOrEmpty(selectExpression))
                throw new ArgumentNullException(nameof(selectExpression));
            if (string.IsNullOrEmpty(alias))
                throw new ArgumentNullException(nameof(alias));

            SelectExpression = selectExpression;
            Alias = alias;
        }

        /// <summary>
        /// The SQL expression being selected, including any necessary quoting.
        /// </summary>
        public string SelectExpression { get; set; }

        public override string ToString()
        {
            var text = SelectExpression;
            if (!string.IsNullOrEmpty(Alias)) // Note that even though Alias must be non-empty in the constructor it may be set to null/empty later
                text += " AS " + DialectProvider.GetQuotedName(Alias);
            return text;
        }
    }

    public class SelectItemColumn : SelectItem
    {
        public SelectItemColumn(IOrmLiteDialectProvider dialectProvider, string columnName, string columnAlias = null, string quotedTableAlias = null)
            : base(dialectProvider, columnAlias)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException(nameof(columnName));

            ColumnName = columnName;
            QuotedTableAlias = quotedTableAlias;
        }

        /// <summary>
        /// Unquoted column name being selected.
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Table name or alias used to prefix the column name, if any. Already quoted.
        /// </summary>
        public string QuotedTableAlias { get; set; }

        public override string ToString()
        {
            var text = DialectProvider.GetQuotedColumnName(ColumnName);

            if (!string.IsNullOrEmpty(QuotedTableAlias))
                text = QuotedTableAlias + "." + text;
            if (!string.IsNullOrEmpty(Alias))
                text += " AS " + DialectProvider.GetQuotedName(Alias);

            return text;
        }
    }

    public class OrmLiteDataParameter : IDbDataParameter
    {
        public DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }
        public bool IsNullable { get; set; }
        public string ParameterName { get; set; }
        public string SourceColumn { get; set; }
        public DataRowVersion SourceVersion { get; set; }
        public object Value { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }
    }

    public static class DbDataParameterExtensions
    {
        public static IDbDataParameter CreateParam(this IDbConnection db,
            string name,
            object value=null,
            Type fieldType = null,
            DbType? dbType=null,
            byte? precision=null,
            byte? scale=null,
            int? size=null)
        {
            return db.GetDialectProvider().CreateParam(name, value, fieldType, dbType, precision, scale, size);
        }

        public static IDbDataParameter CreateParam(this IOrmLiteDialectProvider dialectProvider,
            string name,
            object value = null,
            Type fieldType = null,
            DbType? dbType = null,
            byte? precision = null,
            byte? scale = null,
            int? size = null)
        {
            var to = dialectProvider.CreateParam();

            to.ParameterName = dialectProvider.GetParam(name);

            var valueType = fieldType ?? (value?.GetType() ?? typeof(string));

            if (value != null)
            {
                dialectProvider.InitDbParam(to, valueType);
                to.Value = dialectProvider.GetParamValue(value, valueType);
            }
            else
            {
                to.Value = DBNull.Value;
            }

            if (precision != null)
                to.Precision = precision.Value;
            if (scale != null)
                to.Scale = scale.Value;
            if (size != null)
                to.Size = size.Value;

            dialectProvider.InitDbParam(to, valueType);

            if (dbType != null)
                to.DbType = dbType.Value;

            return to;
        }

        public static IDbDataParameter AddParam(this IOrmLiteDialectProvider dialectProvider,
            IDbCommand dbCmd,
            object value,
            FieldDefinition fieldDef)
        {
            var paramName = dbCmd.Parameters.Count.ToString();
            var parameter = dialectProvider.CreateParam(paramName, value, fieldDef?.ColumnType);

            if (fieldDef != null)
                dialectProvider.SetParameter(fieldDef, parameter);

            dbCmd.Parameters.Add(parameter);
            return parameter;
        }
    }
}

