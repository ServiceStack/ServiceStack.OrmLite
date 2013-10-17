using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public abstract class SqlExpressionVisitor<T>
    {
        private Expression<Func<T, bool>> underlyingExpression;
        private List<string> orderByProperties = new List<string>(); 
        private string selectExpression = string.Empty;
        private string whereExpression;
        private string groupBy = string.Empty;
        private string havingExpression;
        private string orderBy = string.Empty;

        IList<string> updateFields = new List<string>();
        IList<string> insertFields = new List<string>();

        private string sep = string.Empty;
        private bool useFieldName = false;
        private ModelDefinition modelDef;
        public bool PrefixFieldWithTableName {get;set;}
        public bool WhereStatementWithoutWhereString { get; set; }

        protected string Sep
        {
            get { return sep; }
        }

        public SqlExpressionVisitor()
        {
            modelDef = typeof(T).GetModelDefinition();
            PrefixFieldWithTableName = false;
            WhereStatementWithoutWhereString = false;
        }

        /// <summary>
        /// Clear select expression. All properties will be selected.
        /// </summary>
        public virtual SqlExpressionVisitor<T> Select()
        {
            return Select(string.Empty);
        }

        /// <summary>
        /// set the specified selectExpression.
        /// </summary>
        /// <param name='selectExpression'>
        /// raw Select expression: "Select SomeField1, SomeField2 from SomeTable"
        /// </param>
        public virtual SqlExpressionVisitor<T> Select(string selectExpression)
        {

            if (string.IsNullOrEmpty(selectExpression))
            {
                BuildSelectExpression(string.Empty, false);
            }
            else
            {
                this.selectExpression = selectExpression;
            }
            return this;
        }

        /// <summary>
        /// Fields to be selected.
        /// </summary>
        /// <param name='fields'>
        /// x=> x.SomeProperty1 or x=> new{ x.SomeProperty1, x.SomeProperty2}
        /// </param>
        /// <typeparam name='TKey'>
        /// objectWithProperties
        /// </typeparam>
        public virtual SqlExpressionVisitor<T> Select<TKey>(Expression<Func<T, TKey>> fields)
        {
            sep = string.Empty;
            useFieldName = true;
            BuildSelectExpression(Visit(fields).ToString(), false);
            return this;
        }

        public virtual SqlExpressionVisitor<T> SelectDistinct<TKey>(Expression<Func<T, TKey>> fields)
        {
            sep = string.Empty;
            useFieldName = true;
            BuildSelectExpression(Visit(fields).ToString(), true);
            return this;
        }



        public virtual SqlExpressionVisitor<T> Where()
        {
            if (underlyingExpression != null) underlyingExpression = null; //Where() clears the expression
            return Where(string.Empty);
        }

        public virtual SqlExpressionVisitor<T> Where(string sqlFilter, params object[] filterParams)
        {
            whereExpression = !string.IsNullOrEmpty(sqlFilter) ? sqlFilter.SqlFormat(filterParams) : string.Empty;
            if (!string.IsNullOrEmpty(whereExpression)) whereExpression = (WhereStatementWithoutWhereString ? "" : "WHERE ") + whereExpression;
            return this;
        }

        public virtual SqlExpressionVisitor<T> Where(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                And(predicate);
            }
            else
            {
                underlyingExpression = null;
                whereExpression = string.Empty;
            }

            return this;
        }

        public virtual SqlExpressionVisitor<T> And(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                if (underlyingExpression == null)
                    underlyingExpression = predicate;
                else
                    underlyingExpression = underlyingExpression.And(predicate);

                ProcessInternalExpression();
            }
            return this;
        }

        public virtual SqlExpressionVisitor<T> Or(Expression<Func<T, bool>> predicate)
        {
            if (predicate != null)
            {
                if (underlyingExpression == null)
                    underlyingExpression = predicate;
                else
                    underlyingExpression = underlyingExpression.Or(predicate);

                ProcessInternalExpression();
            }
            return this;
        }

        private void ProcessInternalExpression()
        {
            useFieldName = true;
            sep = " ";
            whereExpression = Visit(underlyingExpression).ToString();
            if (!string.IsNullOrEmpty(whereExpression)) whereExpression = (WhereStatementWithoutWhereString ? "" : "WHERE ") + whereExpression;
        }
        
        public virtual SqlExpressionVisitor<T> GroupBy()
        {
            return GroupBy(string.Empty);
        }

        public virtual SqlExpressionVisitor<T> GroupBy(string groupBy)
        {
            this.groupBy = groupBy;
            return this;
        }

        public virtual SqlExpressionVisitor<T> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            sep = string.Empty;
            useFieldName = true;
            groupBy = Visit(keySelector).ToString();
            if (!string.IsNullOrEmpty(groupBy)) groupBy = string.Format("GROUP BY {0}", groupBy);
            return this;
        }


        public virtual SqlExpressionVisitor<T> Having()
        {
            return Having(string.Empty);
        }

        public virtual SqlExpressionVisitor<T> Having(string sqlFilter, params object[] filterParams)
        {
            havingExpression = !string.IsNullOrEmpty(sqlFilter) ? sqlFilter.SqlFormat(filterParams) : string.Empty;
            if (!string.IsNullOrEmpty(havingExpression)) havingExpression = "HAVING " + havingExpression;
            return this;
        }

        public virtual SqlExpressionVisitor<T> Having(Expression<Func<T, bool>> predicate)
        {

            if (predicate != null)
            {
                useFieldName = true;
                sep = " ";
                havingExpression = Visit(predicate).ToString();
                if (!string.IsNullOrEmpty(havingExpression)) havingExpression = "HAVING " + havingExpression;
            }
            else
                havingExpression = string.Empty;

            return this;
        }



        public virtual SqlExpressionVisitor<T> OrderBy()
        {
            return OrderBy(string.Empty);
        }

        public virtual SqlExpressionVisitor<T> OrderBy(string orderBy)
        {
            orderByProperties.Clear();
            this.orderBy = orderBy;
            return this;
        }

        public virtual SqlExpressionVisitor<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            sep = string.Empty;
            useFieldName = true;
            orderByProperties.Clear();
            var property  = Visit(keySelector).ToString();
            orderByProperties.Add(property + " ASC");
            BuildOrderByClauseInternal();
            return this;
        }

        public virtual SqlExpressionVisitor<T> ThenBy<TKey> (Expression<Func<T,TKey>> keySelector)
        {
            sep = string.Empty;
            useFieldName = true;
            var property = Visit(keySelector).ToString();
            orderByProperties.Add(property + " ASC");
            BuildOrderByClauseInternal();
            return this;
        }

        public virtual SqlExpressionVisitor<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            sep = string.Empty;
            useFieldName = true;
            orderByProperties.Clear();
            var property = Visit(keySelector).ToString();
            orderByProperties.Add(property + " DESC");
            BuildOrderByClauseInternal();
            return this;
        }

        public virtual SqlExpressionVisitor<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            sep = string.Empty;
            useFieldName = true;
            var property = Visit(keySelector).ToString();
            orderByProperties.Add(property + " DESC");
            BuildOrderByClauseInternal();
            return this;
        }

        private void BuildOrderByClauseInternal()
        {
            if (orderByProperties.Count > 0)
            {
                orderBy = "ORDER BY ";
                foreach(var prop in orderByProperties)
                {
                    orderBy += prop + ",";
                }
                orderBy = orderBy.TrimEnd(',');
            }
            else
            {
                orderBy = null;
            }
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
        public virtual SqlExpressionVisitor<T> Limit(int skip, int rows)
        {
            Rows = rows;
            Skip = skip;
            return this;
        }

        /// <summary>
        /// Set the specified rows for Sql Limit clause.
        /// </summary>
        /// <param name='rows'>
        /// Number of rows returned by a SELECT statement
        /// </param>
        public virtual SqlExpressionVisitor<T> Limit(int rows)
        {
            Rows = rows;
            Skip = 0;
            return this;
        }

        /// <summary>
        /// Clear Sql Limit clause
        /// </summary>
        public virtual SqlExpressionVisitor<T> Limit()
        {
            Skip = null;
            Rows = null;
            return this;
        }


        /// <summary>
        /// Fields to be updated.
        /// </summary>
        /// <param name='updatefields'>
        /// IList<string> containing Names of properties to be updated
        /// </param>
        public virtual SqlExpressionVisitor<T> Update(IList<string> updateFields)
        {
            this.updateFields = updateFields;
            return this;
        }

        /// <summary>
        /// Fields to be updated.
        /// </summary>
        /// <param name='fields'>
        /// x=> x.SomeProperty1 or x=> new{ x.SomeProperty1, x.SomeProperty2}
        /// </param>
        /// <typeparam name='TKey'>
        /// objectWithProperties
        /// </typeparam>
        public virtual SqlExpressionVisitor<T> Update<TKey>(Expression<Func<T, TKey>> fields)
        {
            sep = string.Empty;
            useFieldName = false;
            updateFields = Visit(fields).ToString().Split(',').ToList();
            return this;
        }

        /// <summary>
        /// Clear UpdateFields list ( all fields will be updated)
        /// </summary>
        public virtual SqlExpressionVisitor<T> Update()
        {
            this.updateFields = new List<string>();
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
        public virtual SqlExpressionVisitor<T> Insert<TKey>(Expression<Func<T, TKey>> fields)
        {
            sep = string.Empty;
            useFieldName = false;
            insertFields = Visit(fields).ToString().Split(',').ToList();
            return this;
        }

        /// <summary>
        /// fields to be inserted.
        /// </summary>
        /// <param name='insertFields'>
        /// IList&lt;string&gt; containing Names of properties to be inserted
        /// </param>
        public virtual SqlExpressionVisitor<T> Insert(IList<string> insertFields)
        {
            this.insertFields = insertFields;
            return this;
        }

        /// <summary>
        /// Clear InsertFields list ( all fields will be inserted)
        /// </summary>
        public virtual SqlExpressionVisitor<T> Insert()
        {
            this.insertFields = new List<string>();
            return this;
        }
        
        public virtual string ToDeleteRowStatement()
        {
            return string.Format("DELETE FROM {0} {1}",
                                                   OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef),
                                                   WhereExpression);
        }

        public virtual string ToUpdateStatement(T item, bool excludeDefaults = false)
        {
            var setFields = new StringBuilder();
            var dialectProvider = OrmLiteConfig.DialectProvider;

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (updateFields.Count > 0 && !updateFields.Contains(fieldDef.Name)) continue; // added
                var value = fieldDef.GetValue(item);
                if (excludeDefaults && (value == null || value.Equals(value.GetType().GetDefaultValue()))) continue; //GetDefaultValue?

                fieldDef.GetQuotedValue(item);

                if (setFields.Length > 0) setFields.Append(",");
                setFields.AppendFormat("{0} = {1}",
                    dialectProvider.GetQuotedColumnName(fieldDef.FieldName),
                    dialectProvider.GetQuotedValue(value, fieldDef.FieldType));
            }

            return string.Format("UPDATE {0} SET {1} {2}",
                                                dialectProvider.GetQuotedTableName(modelDef), setFields, WhereExpression);
        }

        public virtual string ToSelectStatement()
        {
            var sql = new StringBuilder();

            sql.Append(SelectExpression);
            sql.Append(string.IsNullOrEmpty(WhereExpression) ?
                       "" :
                       "\n" + WhereExpression);
            sql.Append(string.IsNullOrEmpty(GroupByExpression) ?
                       "" :
                       "\n" + GroupByExpression);
            sql.Append(string.IsNullOrEmpty(HavingExpression) ?
                       "" :
                       "\n" + HavingExpression);
            sql.Append(string.IsNullOrEmpty(OrderByExpression) ?
                       "" :
                       "\n" + OrderByExpression);

            return ApplyPaging(sql.ToString());
        }

        public virtual string ToCountStatement()
        {
            return OrmLiteConfig.DialectProvider.ToCountStatement(modelDef.ModelType, WhereExpression, null);
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
                return orderBy;
            }
            set
            {
                orderBy = value;
            }
        }

        public virtual string LimitExpression
        {
            get
            {
                if (!Skip.HasValue) return "";
                string rows;
                if (Rows.HasValue)
                {
                    rows = string.Format(",{0}", Rows.Value);
                }
                else
                {
                    rows = string.Empty;
                }
                return string.Format("LIMIT {0}{1}", Skip.Value, rows);
            }
        }

        public int? Rows { get; set; }
        public int? Skip { get; set; }

        public IList<string> UpdateFields
        {
            get
            {
                return updateFields;
            }
            set
            {
                updateFields = value;
            }
        }

        public IList<string> InsertFields
        {
            get
            {
                return insertFields;
            }
            set
            {
                insertFields = value;
            }
        }

        protected internal ModelDefinition ModelDef
        {
            get
            {
                return modelDef;
            }
            set
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

            if (exp == null) return string.Empty;
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

        protected virtual object VisitLambda(LambdaExpression lambda)
        {
            if (lambda.Body.NodeType == ExpressionType.MemberAccess && sep == " ")
            {
                MemberExpression m = lambda.Body as MemberExpression;

                if (m.Expression != null)
                {
                    string r = VisitMemberAccess(m).ToString();
                    return string.Format("{0}={1}", r, GetQuotedTrueValue());
                }

            }
            return Visit(lambda.Body);
        }

        protected virtual object VisitBinary(BinaryExpression b)
        {
            object left, right;
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
                    var result = Expression.Lambda(b).Compile().DynamicInvoke();
                    return new PartialSqlString(OrmLiteConfig.DialectProvider.GetQuotedValue(result, result.GetType()));
                }

                if(left as PartialSqlString == null)
                    left = ((bool) left) ? GetTrueExpression() : GetFalseExpression();
                if (right as PartialSqlString == null)
                    right = ((bool)right) ? GetTrueExpression() : GetFalseExpression();
            }
            else
            {
                left = Visit(b.Left);
                right = Visit(b.Right);

                if (left as EnumMemberAccess != null && right as PartialSqlString == null)
                {
                    var enumType = ((EnumMemberAccess)left).EnumType;

                    //enum value was returned by Visit(b.Right)
                    long numvericVal;
                    if (Int64.TryParse(right.ToString(), out numvericVal))
                        right = OrmLiteConfig.DialectProvider.GetQuotedValue(Enum.ToObject(enumType, numvericVal).ToString(),
                                                                     typeof(string));
                    else
                        right = OrmLiteConfig.DialectProvider.GetQuotedValue(right, right.GetType());
                }
                else if (right as EnumMemberAccess != null && left as PartialSqlString == null)
                {
                    var enumType = ((EnumMemberAccess)right).EnumType;

                    //enum value was returned by Visit(b.Left)
                    long numvericVal;
                    if (Int64.TryParse(left.ToString(), out numvericVal))
                        left = OrmLiteConfig.DialectProvider.GetQuotedValue(Enum.ToObject(enumType, numvericVal).ToString(),
                                                                     typeof(string));
                    else
                        left = OrmLiteConfig.DialectProvider.GetQuotedValue(left, left.GetType());
                }
                else if (left as PartialSqlString == null && right as PartialSqlString == null)
                {
                    var result = Expression.Lambda(b).Compile().DynamicInvoke();
                    return result;
                }
                else if (left as PartialSqlString == null)
                    left = OrmLiteConfig.DialectProvider.GetQuotedValue(left, left != null ? left.GetType() : null);
                else if (right as PartialSqlString == null)
                    right = OrmLiteConfig.DialectProvider.GetQuotedValue(right, right != null ? right.GetType() : null);
                    
            }

            if (operand == "=" && right.ToString().Equals("null", StringComparison.InvariantCultureIgnoreCase)) operand = "is";
            else if (operand == "<>" && right.ToString().Equals("null", StringComparison.InvariantCultureIgnoreCase)) operand = "is not";

            switch (operand)
            {
                case "MOD":
                case "COALESCE":
                    return new PartialSqlString(string.Format("{0}({1},{2})", operand, left, right));
                default:
                    return new PartialSqlString("(" + left + sep + operand + sep + right +")");
            }
        }

        protected virtual object VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null
                && (m.Expression.NodeType == ExpressionType.Parameter || m.Expression.NodeType == ExpressionType.Convert))
            {
                var propertyInfo = m.Member as PropertyInfo;

                if (propertyInfo.PropertyType.IsEnum)
                    return new EnumMemberAccess((PrefixFieldWithTableName ? OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef.ModelName) + "." : "") + GetQuotedColumnName(m.Member.Name), propertyInfo.PropertyType);

                return new PartialSqlString((PrefixFieldWithTableName ? OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef.ModelName)+"." : "") + GetQuotedColumnName(m.Member.Name));
            }

            var member = Expression.Convert(m, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            return getter();
        }

        protected virtual object VisitMemberInit(MemberInitExpression exp)
        {
            return Expression.Lambda(exp).Compile().DynamicInvoke();
        }

        protected virtual object VisitNew(NewExpression nex)
        {
            // TODO : check !
            var member = Expression.Convert(nex, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            try
            {
                var getter = lambda.Compile();
                return getter();
            }
            catch (System.InvalidOperationException)
            { // FieldName ?
                List<Object> exprs = VisitExpressionList(nex.Arguments);
                StringBuilder r = new StringBuilder();
                foreach (Object e in exprs)
                {
                    r.AppendFormat("{0}{1}",
                                   r.Length > 0 ? "," : "",
                                   e);
                }
                return r.ToString();
            }

        }

        protected virtual object VisitParameter(ParameterExpression p)
        {
            return p.Name;
        }

        public bool IsParameterized { get; set; }
        public Dictionary<string, object> Params = new Dictionary<string, object>();
        int paramCounter = 0;

        protected virtual object VisitConstant(ConstantExpression c)
        {
            if (c.Value == null)
                return new PartialSqlString("null");

            if (!IsParameterized)
            {
                return c.Value;
            }
            else
            {
                string paramPlaceholder = OrmLiteConfig.DialectProvider.ParamString + paramCounter++;
                Params.Add(paramPlaceholder, c.Value);
                return new PartialSqlString(paramPlaceholder);
            }
        }

        protected virtual object VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    var o = Visit(u.Operand);

                    if (o as PartialSqlString == null)
                        return !((bool)o);

                    if (IsFieldName(o))
                        o = o + "=" +  GetQuotedTrueValue();

                    return new PartialSqlString("NOT (" + o + ")");
                case ExpressionType.Convert:
                    if (u.Method != null)
                        return Expression.Lambda(u).Compile().DynamicInvoke();
                    break;
            }

            return Visit(u.Operand);

        }

        private bool IsColumnAccess(MethodCallExpression m)
        {
            if (m.Object != null && m.Object as MethodCallExpression != null)
                return IsColumnAccess(m.Object as MethodCallExpression);

            var exp = m.Object as MemberExpression;
            return exp != null 
                && exp.Expression != null 
                && exp.Expression.Type == typeof(T)
                && exp.Expression.NodeType == ExpressionType.Parameter;
        }

        protected virtual object VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Sql))
                return VisitSqlMethodCall(m);

			if (IsArrayMethod(m))
                return VisitArrayMethodCall(m);

            if (IsColumnAccess(m))
                return VisitColumnAccessMethod(m);

            return Expression.Lambda(m).Compile().DynamicInvoke();
        }

        protected virtual List<Object> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Object> list = new List<Object>();
            for (int i = 0, n = original.Count; i < n; i++)
            {
                if (original[i].NodeType == ExpressionType.NewArrayInit ||
                 original[i].NodeType == ExpressionType.NewArrayBounds)
                {

                    list.AddRange(VisitNewArrayFromExpressionList(original[i] as NewArrayExpression));
                }
                else
                    list.Add(Visit(original[i]));

            }
            return list;
        }

        protected virtual object VisitNewArray(NewArrayExpression na)
        {

            List<Object> exprs = VisitExpressionList(na.Expressions);
            StringBuilder r = new StringBuilder();
            foreach (Object e in exprs)
            {
                r.Append(r.Length > 0 ? "," + e : e);
            }

            return r.ToString();
        }

        protected virtual List<Object> VisitNewArrayFromExpressionList(NewArrayExpression na)
        {

            List<Object> exprs = VisitExpressionList(na.Expressions);
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

        protected virtual string GetQuotedColumnName(string memberName)
        {

            if (useFieldName)
            {
                FieldDefinition fd = modelDef.FieldDefinitions.FirstOrDefault(x => x.Name == memberName);
                string fn = fd != default(FieldDefinition) ? fd.FieldName : memberName;
                return OrmLiteConfig.DialectProvider.GetQuotedColumnName(fn);
            }
            else
            {
                return memberName;
            }
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
            FieldDefinition fd =
                modelDef.FieldDefinitions.
                    FirstOrDefault(x =>
                        OrmLiteConfig.DialectProvider.
                        GetQuotedColumnName(x.FieldName) == quotedExp.ToString());
            return (fd != default(FieldDefinition));
        }

        protected object GetTrueExpression()
        {
            return new PartialSqlString(string.Format("({0}={1})", GetQuotedTrueValue(), GetQuotedTrueValue()));
        }

        protected object GetFalseExpression()
        {
            return new PartialSqlString(string.Format("({0}={1})", GetQuotedTrueValue(), GetQuotedFalseValue()));
        }

        protected static object GetQuotedTrueValue()
        {
            return new PartialSqlString(OrmLiteConfig.DialectProvider.GetQuotedValue(true, typeof (bool)));
        }

        protected static object GetQuotedFalseValue()
        {
            return new PartialSqlString(OrmLiteConfig.DialectProvider.GetQuotedValue(false, typeof(bool)));
        }

        private void BuildSelectExpression(string fields, bool distinct)
        {

            selectExpression = string.Format("SELECT {0}{1} \nFROM {2}",
                (distinct ? "DISTINCT " : ""),
                (string.IsNullOrEmpty(fields) ?
                    OrmLiteConfig.DialectProvider.GetColumnNames(modelDef) :
                    fields),
                OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef));
        }

        public IList<string> GetAllFields()
        {
            return modelDef.FieldDefinitions.ConvertAll(r => r.Name);
        }

        protected virtual string ApplyPaging(string sql)
        {
            sql = sql + (string.IsNullOrEmpty(LimitExpression) ? "" :"\n" + LimitExpression);
            return sql;
        }

        private bool IsArrayMethod(MethodCallExpression m)
        {
            if (m.Object == null && m.Method.Name == "Contains")
            {
                if (m.Arguments.Count == 2)
                    return true;
            }

            return false;
        }

        protected virtual object VisitArrayMethodCall(MethodCallExpression m)
        {
            string statement;

            switch (m.Method.Name)
            {
                case "Contains":
                    List<Object> args = this.VisitExpressionList(m.Arguments);
                    object quotedColName = args[1];

                    var memberExpr = m.Arguments[0];
                    if (memberExpr.NodeType == ExpressionType.MemberAccess)
                        memberExpr = (m.Arguments[0] as MemberExpression);

                    var member = Expression.Convert(memberExpr, typeof(object));
                    var lambda = Expression.Lambda<Func<object>>(member);
                    var getter = lambda.Compile();

                    var inArgs = Sql.Flatten(getter() as IEnumerable);

                    StringBuilder sIn = new StringBuilder();

                    if (inArgs.Any())
                    {
                        foreach (Object e in inArgs)
                        {
                            sIn.AppendFormat("{0}{1}",
                                             sIn.Length > 0 ? "," : "",
                                             OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()));
                        }
                    }
                    else
                    {
                        // The collection is empty, so avoid generating invalid SQL syntax of "ColumnName IN ()".
                        // Instead, just select from the null set via "ColumnName IN (NULL)"
                        sIn.Append("NULL");
                    }

                    statement = string.Format("{0} {1} ({2})", quotedColName, "In", sIn.ToString());
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }

        protected virtual object VisitSqlMethodCall(MethodCallExpression m)
        {
            List<Object> args = this.VisitExpressionList(m.Arguments);
            object quotedColName = args[0];
            args.RemoveAt(0);

            string statement;

            switch (m.Method.Name)
            {
                case "In":

                    var member = Expression.Convert(m.Arguments[1], typeof(object));
                    var lambda = Expression.Lambda<Func<object>>(member);
                    var getter = lambda.Compile();

                    var inArgs = Sql.Flatten(getter() as IEnumerable);

                    StringBuilder sIn = new StringBuilder();
                    foreach (Object e in inArgs)
                    {
                        if (!typeof(ICollection).IsAssignableFrom(e.GetType()))
                        {
                            sIn.AppendFormat("{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()));
                        }
                        else
                        {
                            var listArgs = e as ICollection;
                            foreach (Object el in listArgs)
                            {
                                sIn.AppendFormat("{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         OrmLiteConfig.DialectProvider.GetQuotedValue(el, el.GetType()));
                            }
                        }
                    }

                    statement = string.Format("{0} {1} ({2})", quotedColName, m.Method.Name, sIn.ToString());
                    break;
                case "Desc":
                    statement = string.Format("{0} DESC", quotedColName);
                    break;
                case "As":
                    statement = string.Format("{0} As {1}", quotedColName,
                        OrmLiteConfig.DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(args[0].ToString())));
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

        protected virtual object VisitColumnAccessMethod(MethodCallExpression m)
        {
            List<Object> args = this.VisitExpressionList(m.Arguments);
            var quotedColName = Visit(m.Object);
            var statement = "";

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
                    statement = string.Format("upper({0}) like {1} ", quotedColName, OrmLiteConfig.DialectProvider.GetQuotedParam(args[0].ToString().ToUpper() + "%"));
                    break;
                case "EndsWith":
                    statement = string.Format("upper({0}) like {1}", quotedColName, OrmLiteConfig.DialectProvider.GetQuotedParam("%" + args[0].ToString().ToUpper()));
                    break;
                case "Contains":
                    statement = string.Format("upper({0}) like {1}", quotedColName, OrmLiteConfig.DialectProvider.GetQuotedParam("%" + args[0].ToString().ToUpper() + "%"));
                    break;
                case "Substring":
                    var startIndex = Int32.Parse(args[0].ToString()) + 1;
                    if (args.Count == 2)
                    {
                        var length = Int32.Parse(args[1].ToString());
                        statement = string.Format("substring({0} from {1} for {2})",
                                                  quotedColName,
                                                  startIndex,
                                                  length);
                    }
                    else
                        statement = string.Format("substring({0} from {1})",
                                         quotedColName,
                                         startIndex);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return new PartialSqlString(statement);
        }
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
        public EnumMemberAccess(string text, Type enumType) : base(text)
        {
            if (!enumType.IsEnum) throw new ArgumentException("Type not valid", "enumType");

            EnumType = enumType;
        }

        public Type EnumType { get; private set; }
    }

}

