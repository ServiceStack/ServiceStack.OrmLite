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
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public abstract partial class SqlExpression<T> : ISqlExpression, IHasUntypedSqlExpression
    {
        private const string TrueLiteral = "(1=1)";
        private const string FalseLiteral = "(1=0)";

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
        private ModelDefinition modelDef;
        public bool PrefixFieldWithTableName { get; set; }
        public bool WhereStatementWithoutWhereString { get; set; }
        public IOrmLiteDialectProvider DialectProvider { get; set; }
        public List<IDbDataParameter> Params { get; set; } 

        protected string Sep
        {
            get { return sep; }
        }

        public SqlExpression(IOrmLiteDialectProvider dialectProvider)
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
            if (selectExpression != null)
                selectExpression.SqlVerifyFragment();
    
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
                            var qualifiedField = DialectProvider.GetQuotedColumnName(tableDef, fieldDef);

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

                var qualifiedName = DialectProvider.GetQuotedColumnName(match.Item1, match.Item2);

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
            if (tables != null)
                tables.SqlVerifyFragment();

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
            if (underlyingExpression != null) underlyingExpression = null; //Where() clears the expression
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
            AppendToWhere("AND", FormatFilter(rawSql, filterParams));
            return this;
        }

        public virtual SqlExpression<T> Where(string sqlFilter, params object[] filterParams)
        {
            AppendToWhere("AND", FormatFilter(sqlFilter.SqlVerifyFragment(), filterParams));
            return this;
        }

        public virtual SqlExpression<T> UnsafeAnd(string rawSql, params object[] filterParams)
        {
            AppendToWhere("AND", FormatFilter(rawSql, filterParams));
            return this;
        }

        public virtual SqlExpression<T> And(string sqlFilter, params object[] filterParams)
        {
            AppendToWhere("AND", FormatFilter(sqlFilter.SqlVerifyFragment(), filterParams));
            return this;
        }

        public virtual SqlExpression<T> UnsafeOr(string rawSql, params object[] filterParams)
        {
            AppendToWhere("OR", FormatFilter(rawSql, filterParams));
            return this;
        }

        public virtual SqlExpression<T> Or(string sqlFilter, params object[] filterParams)
        {
            AppendToWhere("OR", FormatFilter(sqlFilter.SqlVerifyFragment(), filterParams));
            return this;
        }

        public virtual SqlExpression<T> AddCondition(string condition, string sqlFilter, params object[] filterParams)
        {
            AppendToWhere(condition, FormatFilter(sqlFilter.SqlVerifyFragment(), filterParams));
            return this;
        }

        public virtual SqlExpression<T> Where(Expression<Func<T, bool>> predicate)
        {
            AppendToWhere("AND", predicate);
            return this;
        }

        public virtual SqlExpression<T> And(Expression<Func<T, bool>> predicate)
        {
            AppendToWhere("AND", predicate);
            return this;
        }

        public virtual SqlExpression<T> Or(Expression<Func<T, bool>> predicate)
        {
            AppendToWhere("OR", predicate);
            return this;
        }

        protected void AppendToWhere(string condition, Expression predicate)
        {
            if (predicate == null)
                return;

            useFieldName = true;
            sep = " ";
            var newExpr = WhereExpressionToString(Visit(predicate));
            AppendToWhere(condition, newExpr);
        }

        private static string WhereExpressionToString(object expression)
        {
            if (expression is bool)
                return (bool)expression ? TrueLiteral : FalseLiteral;
            return expression.ToString();
        }

        protected void AppendToWhere(string condition, string sqlExpression)
        {
            whereExpression = string.IsNullOrEmpty(whereExpression)
                ? (WhereStatementWithoutWhereString ? "" : "WHERE ")
                : whereExpression + " " + condition + " ";

            whereExpression += sqlExpression;
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

        public virtual SqlExpression<T> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            sep = string.Empty;
            useFieldName = true;
            return GroupBy(Visit(keySelector).ToString());
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

            var sbOrderBy = StringBuilderCache.Allocate();
            foreach (var field in fields)
            {
                var tableDef = GetModelDefinition(field);
                var qualifiedName = modelDef != null
                    ? DialectProvider.GetQuotedColumnName(tableDef, field)
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
                var qualifiedName = DialectProvider.GetQuotedColumnName(field.Item1, field.Item2);

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
            var fields = Visit(keySelector).ToString();
            orderByProperties.Add(fields);
            BuildOrderByClauseInternal();
            return this;
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
            var fields = Visit(keySelector).ToString();
            orderByProperties.Add(fields);
            BuildOrderByClauseInternal();
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
            var orderBySql = Visit(keySelector).ToString();
            orderBySql.ParseTokens()
                .Each(x => orderByProperties.Add(x + " DESC"));
            BuildOrderByClauseInternal();
            return this;
        }

        public virtual SqlExpression<T> OrderByDescending(string orderBy)
        {
            orderByProperties.Clear();
            orderBy.SqlVerifyFragment();
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
            var orderBySql = Visit(keySelector).ToString();
            orderBySql.ParseTokens()
                .Each(x => orderByProperties.Add(x + " DESC"));
            BuildOrderByClauseInternal();
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
            InsertFields = Visit(fields).ToString().Split(',').ToList();
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
            return string.Format("DELETE FROM {0} {1}",
                DialectProvider.GetQuotedTableName(modelDef), WhereExpression);
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
                    .Append(DialectProvider.AddParam(dbCmd, value, fieldDef.ColumnType).ParameterName);
            }

            if (setFields.Length == 0)
                throw new ArgumentException("No non-null or non-default values were provided for type: " + typeof(T).Name);

            dbCmd.CommandText = string.Format("UPDATE {0} SET {1} {2}",
                DialectProvider.GetQuotedTableName(modelDef), StringBuilderCache.ReturnAndFree(setFields), WhereExpression);
        }

        public virtual string ToSelectStatement()
        {
            var sql = DialectProvider
                .ToSelectStatement(modelDef, SelectExpression, BodyExpression, OrderByExpression, Offset, Rows);

            return sql;
        }

        public virtual string ToCountStatement()
        {
            return "SELECT COUNT(*)" + BodyExpression;
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

        public string BodyExpression
        {
            get
            {
                return FromExpression
                    + (string.IsNullOrEmpty(WhereExpression) ? "" : "\n" + WhereExpression)
                    + (string.IsNullOrEmpty(GroupByExpression) ? "" : "\n" + GroupByExpression)
                    + (string.IsNullOrEmpty(HavingExpression) ? "" : "\n" + HavingExpression);
            }
        }

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

        protected internal virtual object Visit(Expression exp)
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
                    string r = VisitMemberAccess(m).ToString();
                    if (m.Expression.Type.IsNullableType())
                        return r;

                    return string.Format("{0}={1}", r, GetQuotedTrueValue());
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
                var m = b.Left as MemberExpression;
                if (m != null && m.Expression != null
                    && m.Expression.NodeType == ExpressionType.Parameter)
                    left = new PartialSqlString(string.Format("{0}={1}", VisitMemberAccess(m), GetQuotedTrueValue()));
                else
                    left = Visit(b.Left);

                m = b.Right as MemberExpression;
                if (m != null && m.Expression != null
                    && m.Expression.NodeType == ExpressionType.Parameter)
                    right = new PartialSqlString(string.Format("{0}={1}", VisitMemberAccess(m), GetQuotedTrueValue()));
                else
                    right = Visit(b.Right);

                if (left as PartialSqlString == null && right as PartialSqlString == null)
                {
                    var result = CachedExpressionCompiler.Evaluate(b);
                    return result;
                }

                if (left as PartialSqlString == null)
                    left = ((bool)left) ? GetTrueExpression() : GetFalseExpression();
                if (right as PartialSqlString == null)
                    right = ((bool)right) ? GetTrueExpression() : GetFalseExpression();
            }
            else if ((operand == "=" || operand == "<>") && b.Left is MethodCallExpression && ((MethodCallExpression)b.Left).Method.Name == "CompareString")
            {
                //Handle VB.NET converting (x => x.Name == "Foo") into (x => CompareString(x.Name, "Foo", False)
                var methodExpr = (MethodCallExpression)b.Left;
                var args = this.VisitExpressionList(methodExpr.Arguments);
                right = GetValue(args[1], typeof(string));
                ConvertToPlaceholderAndParameter(ref right);
                return new PartialSqlString("({0} {1} {2})".Fmt(args[0], operand, right));
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
                else if (left as PartialSqlString == null && right as PartialSqlString == null)
                {
                    var evaluatedValue = CachedExpressionCompiler.Evaluate(b);
                    var result = VisitConstant(Expression.Constant(evaluatedValue));
                    return result;
                }
                else if (left as PartialSqlString == null)
                {
                    left = DialectProvider.GetQuotedValue(left, left != null ? left.GetType() : null);
                }
                else if (right as PartialSqlString == null)
                {
                    right = GetValue(right, right != null ? right.GetType() : null);
                }
            }

            if (left.ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                Swap(ref left, ref right); // "null is x" will not work, so swap the operands
            }

            if (operand == "=" && right.ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                operand = "is";
            else if (operand == "<>" && right.ToString().Equals("null", StringComparison.OrdinalIgnoreCase))
                operand = "is not";

            VisitFilter(operand, originalLeft, originalRight, ref left, ref right);

            switch (operand)
            {
                case "MOD":
                case "COALESCE":
                    return new PartialSqlString(string.Format("{0}({1},{2})", operand, left, right));
                default:
                    return new PartialSqlString("(" + left + sep + operand + sep + right + ")");
            }
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

                    throw new ArgumentException(string.Format("Expression '{0}' accesses unsupported property '{1}' of Nullable<T>", m, m.Member));
                }

                if (m.Expression.NodeType == ExpressionType.Parameter || m.Expression.NodeType == ExpressionType.Convert)
                    return GetMemberExpression(m);
            }

            return CachedExpressionCompiler.Evaluate(m);
        }

        private object GetMemberExpression(MemberExpression m)
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

            if (propertyInfo != null && propertyInfo.PropertyType.IsEnum)
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
                var r = StringBuilderCache.Allocate();
                foreach (object e in exprs)
                {
                    if (r.Length > 0)
                        r.Append(",");

                    r.Append(e);
                }
                return StringBuilderCache.ReturnAndFree(r);
            }

            return CachedExpressionCompiler.Evaluate(nex);
        }

        protected virtual object VisitParameter(ParameterExpression p)
        {
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
                        return CachedExpressionCompiler.Evaluate(u);
                    }
                    break;
            }
            return Visit(u.Operand);
        }

        private object GetNotValue(object o)
        {
            if (o as PartialSqlString == null)
                return !((bool) o);

            if (IsFieldName(o))
                return new PartialSqlString(o + "=" + GetQuotedFalseValue());

            return new PartialSqlString("NOT (" + o + ")");
        }

        private bool IsColumnAccess(MethodCallExpression m)
        {
            if (m.Object != null && m.Object as MethodCallExpression != null)
                return IsColumnAccess(m.Object as MethodCallExpression);
            
            var exp = m.Object as MemberExpression;
            return exp != null
                && exp.Expression != null
                && IsJoinedTable(exp.Expression.Type)
                && exp.Expression.NodeType == ExpressionType.Parameter;
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
                    list.Add(GetMemberExpression(e as MemberExpression));
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

                return PrefixFieldWithTableName 
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

        protected bool IsFieldName(object quotedExp)
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
            return new PartialSqlString(string.Format("({0}={1})", GetQuotedTrueValue(), GetQuotedTrueValue()));
        }

        protected object GetFalseExpression()
        {
            return new PartialSqlString(string.Format("({0}={1})", GetQuotedTrueValue(), GetQuotedFalseValue()));
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

            selectExpression = string.Format("SELECT {0}{1}",
                (selectDistinct ? "DISTINCT " : ""),
                (string.IsNullOrEmpty(fields) ?
                    DialectProvider.GetColumnNames(modelDef) :
                    fields));
        }

        public IList<string> GetAllFields()
        {
            return modelDef.FieldDefinitions.ConvertAll(r => r.Name);
        }

        private bool IsStaticArrayMethod(MethodCallExpression m)
        {
            if (m.Object == null && m.Method.Name == "Contains")
            {
                return m.Arguments.Count == 2;
            }

            return false;
        }

        protected virtual object VisitStaticArrayMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Contains":
                    List<Object> args = this.VisitExpressionList(m.Arguments);
                    object quotedColName = args.Last();

                    Expression memberExpr = m.Arguments[0];
                    if (memberExpr.NodeType == ExpressionType.MemberAccess)
                        memberExpr = (m.Arguments[0] as MemberExpression);

                    return ToInPartialString(memberExpr, quotedColName);

                default:
                    throw new NotSupportedException();
            }
        }

        private bool IsEnumerableMethod(MethodCallExpression m)
        {
            if (m.Object != null
                && m.Object.Type.IsOrHasGenericInterfaceTypeOf(typeof(IEnumerable<>))
                && m.Object.Type != typeof(string)
                && m.Method.Name == "Contains")
            {
                return m.Arguments.Count == 1;
            }

            return false;
        }

        protected virtual object VisitEnumerableMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Contains":
                    List<Object> args = this.VisitExpressionList(m.Arguments);
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

            var statement = string.Format("{0} {1} ({2})", quotedColName, "In", sqlIn);
            return new PartialSqlString(statement);
        }

        protected virtual object VisitSqlMethodCall(MethodCallExpression m)
        {
            List<object> args = this.VisitInSqlExpressionList(m.Arguments);
            object quotedColName = args[0];
            args.RemoveAt(0);

            string statement;

            switch (m.Method.Name)
            {
                case "In":
                    statement = ConvertInExpressionToSql(m, quotedColName);
                    break;

                case "Desc":
                    statement = string.Format("{0} DESC", quotedColName);
                    break;
                case "As":
                    statement = string.Format("{0} As {1}", quotedColName,
                        DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(args[0].ToString())));
                    break;
                case "Sum":
                case "Count":
                case "Min":
                case "Max":
                case "Avg":
                    statement = string.Format("{0}({1}{2})",
                        m.Method.Name,
                        quotedColName,
                        args.Count == 1 ? string.Format(",{0}", args[0]) : "");
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
                return string.Format("{0} {1} ({2})", quotedColName, m.Method.Name, sqlIn);
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
                    subSelect = subSelect.Replace(renameParams[i].Item1, renameParams[i].Item2);
                }

                return string.Format("{0} {1} ({2})", quotedColName, "IN", subSelect);
            }

            throw new NotSupportedException("In({0})".Fmt(argValue.GetType()));
        }

        protected virtual object VisitColumnAccessMethod(MethodCallExpression m)
        {
            List<object> args = this.VisitExpressionList(m.Arguments);
            var quotedColName = Visit(m.Object);
            var statement = "";

            var wildcardArg = args.Count > 0 ? DialectProvider.EscapeWildcards(args[0].ToString()) : "";
            var escapeSuffix = wildcardArg.IndexOf('^') >= 0 ? " escape '^'" : "";
            switch (m.Method.Name)
            {
                case "Trim":
                    statement = string.Format("ltrim(rtrim({0}))", quotedColName);
                    break;
                case "LTrim":
                    statement = string.Format("ltrim({0})", quotedColName);
                    break;
                case "RTrim":
                    statement = string.Format("rtrim({0})", quotedColName);
                    break;
                case "ToUpper":
                    statement = string.Format("upper({0})", quotedColName);
                    break;
                case "ToLower":
                    statement = string.Format("lower({0})", quotedColName);
                    break;
                case "StartsWith":
                    if (!OrmLiteConfig.StripUpperInLike)
                    {
                        statement = string.Format("upper({0}) like {1}{2}",
                            quotedColName,
                            ConvertToParam(wildcardArg.ToUpper() + "%"),
                            escapeSuffix);
                    }
                    else
                    {
                        statement = string.Format("{0} like {1}{2}",
                            quotedColName,
                            ConvertToParam(wildcardArg + "%"),
                            escapeSuffix);
                    }
                    break;
                case "EndsWith":
                    if (!OrmLiteConfig.StripUpperInLike)
                    {
                        statement = string.Format("upper({0}) like {1}{2}",
                            quotedColName,
                            ConvertToParam("%" + wildcardArg.ToUpper()),
                            escapeSuffix);
                    }
                    else
                    {
                        statement = string.Format("{0} like {1}{2}",
                            quotedColName,
                            ConvertToParam("%" + wildcardArg),
                            escapeSuffix);
                    }
                    break;
                case "Contains":
                    if (!OrmLiteConfig.StripUpperInLike)
                    {
                        statement = string.Format("upper({0}) like {1}{2}",
                            quotedColName,
                            ConvertToParam("%" + wildcardArg.ToUpper() + "%"),
                            escapeSuffix);
                    }
                    else
                    {
                        statement = string.Format("{0} like {1}{2}",
                            quotedColName,
                            ConvertToParam("%" + wildcardArg + "%"),
                            escapeSuffix);
                    }
                    break;
                case "Substring":
                    var startIndex = int.Parse(args[0].ToString()) + 1;
                    if (args.Count == 2)
                    {
                        var length = int.Parse(args[1].ToString());
                        statement = GetSubstringSql(quotedColName, startIndex, length);
                    }
                    else
                    {
                        statement = GetSubstringSql(quotedColName, startIndex);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
            return new PartialSqlString(statement);
        }

        public virtual string GetSubstringSql(object quotedColumn, int startIndex, int? length = null)
        {
            return length != null
                ? string.Format("substring({0} from {1} for {2})", quotedColumn, startIndex, length.Value)
                : string.Format("substring({0} from {1})", quotedColumn, startIndex);
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
                p.Size = DialectProvider.GetStringConverter().StringLength;

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
            if (!enumType.IsEnum) throw new ArgumentException("Type not valid", "enumType");

            EnumType = enumType;
        }

        public Type EnumType { get; private set; }
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

            var valueType = fieldType ?? (value != null ? value.GetType() : typeof(string));

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

        public static IDbDataParameter AddParam(this IOrmLiteDialectProvider dialectProvider, IDbCommand dbCmd, object value, Type fieldType = null)
        {
            var paramName = dbCmd.Parameters.Count.ToString();

            var parameter = dialectProvider.CreateParam(paramName, value, fieldType);
            dbCmd.Parameters.Add(parameter);
            return parameter;
        }
    }
}

