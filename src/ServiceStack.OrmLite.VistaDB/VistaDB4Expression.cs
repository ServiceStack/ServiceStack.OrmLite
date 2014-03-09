using System;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.VistaDB
{
    public class VistaDB4Expression<T> : SqlExpression<T>
    {
        private const string SelectClause = "SELECT";
        private const string SelectDistinctClause = "SELECT DISTINCT";

        private static string GetFieldsList(string sql, out bool isDistinct)
        {
            isDistinct = sql.StartsWith(SelectDistinctClause);

            var prefix = isDistinct
                ? SelectDistinctClause
                : SelectClause;

            var fieldsList = sql.Substring(prefix.Length);
            return fieldsList.Remove(fieldsList.IndexOf("FROM")).Trim();
        }

        private string ApplyEfficientPaging(int skip, int take, string orderBy)
        {
            var tableName = OrmLiteConfig.DialectProvider.GetQuotedTableName(ModelDef).Trim();

            bool isDistinct;
            var fieldsList = GetFieldsList(SelectExpression, out isDistinct);

            if (isDistinct)
                throw new NotSupportedException("Distinct is not supported with both skip and take");

            var sql = this.BuildDeclareFirstIdExpression();
            sql += String.Join(" ",
                this.BuildSelectTopNIdExpression(skip),
                " FROM " + tableName,
                GroupByExpression,
                HavingExpression,
                orderBy,
                ";");

            sql += String.Join(" ",
                SelectClause + " TOP " + take,
                fieldsList,
                " FROM " + tableName,
                this.BuildWhereIdGTEFirstId(),
                GroupByExpression,
                HavingExpression,
                orderBy).Trim();

            return sql;
        }

        public override string ToSelectStatement()
        {
            if (!Skip.HasValue && !Rows.HasValue)
                return base.ToSelectStatement();

            AssertValidSkipRowValues();

            var skip = this.Skip.GetValueOrDefault();
            var take = this.Rows.HasValue ? this.Rows.Value : int.MaxValue;

            var sql = "";
            if (skip == 0)
            {
                sql = base.ToSelectStatement();
                if (take == int.MaxValue || sql == null)
                    return sql; ;

                bool isDistinct;
                var fieldsList = GetFieldsList(SelectExpression, out isDistinct);

                var clause = isDistinct
                    ? SelectDistinctClause
                    : SelectClause;

                if (sql.Length < clause.Length)
                    return sql;

                if (isDistinct)
                    return "SELECT TOP " + take + " t.* FROM (" + sql + ") t;";
                else 
                    return SelectClause + " TOP " + take + " " + sql.Substring(clause.Length, sql.Length - clause.Length);
            }
            else
            {
                var orderBy = this.BuildOrderByIdExpression();

                if (String.IsNullOrEmpty(OrderByExpression))
                    return this.ApplyEfficientPaging(skip, take, orderBy);

                if (OrderByExpression.Equals(orderBy, StringComparison.OrdinalIgnoreCase) ||
                    OrderByExpression.Equals(orderBy + " ASC", StringComparison.OrdinalIgnoreCase) ||
                    OrderByExpression.Equals(orderBy + " DESC", StringComparison.OrdinalIgnoreCase))
                    return this.ApplyEfficientPaging(skip, take, orderBy);

                var tableName = OrmLiteConfig.DialectProvider.GetQuotedTableName(ModelDef).Trim();

                bool isDistinct;
                var fieldsList = GetFieldsList(SelectExpression, out isDistinct);

                if (isDistinct)
                    throw new NotSupportedException("Distinct is not supported with both skip and take");

                var select = String.Join(" ",
                    String.Format("SELECT TOP {0} {1} ",
                        skip,
                        OrmLiteConfig.DialectProvider.GetQuotedColumnName(ModelDef.PrimaryKey.FieldName)),
                    "FROM " + tableName,
                    GroupByExpression,
                    HavingExpression,
                    OrderByExpression);

                var where = String.IsNullOrWhiteSpace(WhereExpression)
                    ? String.Format("{0} NOT IN ({1})", OrmLiteConfig.DialectProvider.GetQuotedColumnName(ModelDef.PrimaryKey.FieldName), select)
                    : String.Format("({0}) AND {1} NOT IN ({2})", WhereExpression.Trim().Substring("WHERE".Length), OrmLiteConfig.DialectProvider.GetQuotedColumnName(ModelDef.PrimaryKey.FieldName), select);

                where = "WHERE " + where;

                return String.Join(" ",
                    SelectClause + " TOP " + take,
                    fieldsList,
                    " FROM " + tableName,
                    where,
                    GroupByExpression,
                    HavingExpression,
                    OrderByExpression).Trim();
            }
        }

        public override string ToUpdateStatement(T item, bool excludeDefaults = false)
        {
            var setFields = new StringBuilder();
            var dialectProvider = OrmLiteConfig.DialectProvider;

            foreach (var fieldDef in ModelDef.FieldDefinitions)
            {
                if (UpdateFields.Count > 0 && !UpdateFields.Contains(fieldDef.Name) || fieldDef.AutoIncrement)
                    continue; // added

                var value = fieldDef.GetValue(item);
                if (excludeDefaults && (value == null || value.Equals(value.GetType().GetDefaultValue())))
                    continue; //GetDefaultValue?

                fieldDef.GetQuotedValue(item);

                if (setFields.Length > 0) setFields.Append(",");
                setFields.AppendFormat("{0} = {1}",
                    dialectProvider.GetQuotedColumnName(fieldDef.FieldName),
                    dialectProvider.GetQuotedValue(value, fieldDef.FieldType));
            }

            return string.Format("UPDATE {0} SET {1} {2}",
                dialectProvider.GetQuotedTableName(ModelDef), setFields, WhereExpression);
        }

        protected virtual void AssertValidSkipRowValues()
        {
            if (Skip.HasValue && Skip.Value < 0)
                throw new ArgumentException(String.Format("Skip value:'{0}' must be>=0", Skip.Value));

            if (Rows.HasValue && Rows.Value < 0)
                throw new ArgumentException(string.Format("Rows value:'{0}' must be>=0", Rows.Value));
        }

        protected virtual string BuildOrderByIdExpression()
        {
            if (ModelDef.PrimaryKey == null)
                throw new ApplicationException("Malformed model, no PrimaryKey defined");

            return String.Format("ORDER BY {0}", OrmLiteConfig.DialectProvider.GetQuotedColumnName(ModelDef.PrimaryKey.FieldName));
        }

        protected virtual string BuildWhereIdGTEFirstId()
        {
            if (ModelDef.PrimaryKey == null)
                throw new ApplicationException("Malformed model, no PrimaryKey defined");

            var where = String.IsNullOrWhiteSpace(WhereExpression)
                ? String.Format("{0} > @first_id", OrmLiteConfig.DialectProvider.GetQuotedColumnName(ModelDef.PrimaryKey.FieldName))
                : String.Format("({0}) AND {1} > @first_id", WhereExpression.Trim().Substring("WHERE".Length), OrmLiteConfig.DialectProvider.GetQuotedColumnName(ModelDef.PrimaryKey.FieldName));
            
            return "WHERE " + where;
        }

        protected virtual string BuildSelectTopNIdExpression(int skip)
        {
            if (ModelDef.PrimaryKey == null)
                throw new ApplicationException("Malformed model, no PrimaryKey defined");
            
            return String.Format("SELECT TOP {0} @first_id = {1} ", skip, OrmLiteConfig.DialectProvider.GetQuotedColumnName(ModelDef.PrimaryKey.FieldName));
        }

        protected virtual string BuildDeclareFirstIdExpression()
        {
            if (ModelDef.PrimaryKey == null)
                throw new ApplicationException("Malformed model, no PrimaryKey defined");

            return String.Format("DECLARE @first_id {0};",
                OrmLiteConfig.DialectProvider.GetColumnTypeDefinition(ModelDef.PrimaryKey.FieldType));
        }

        public override string LimitExpression
        {
            get
            {
                return "";
            }
        }

        protected override object VisitColumnAccessMethod(MethodCallExpression m)
        {
            if (m.Arguments.Count == 1 && m.Method.Name == "Equals")
            {
                var caller = this.Visit(m.Object);
                var arg = this.Visit(m.Arguments.First());

                return new PartialSqlString(String.Format("{0} = {1}", caller, arg));
            }
            else
            {
                return base.VisitColumnAccessMethod(m);
            }
        }

        public override SqlExpression<T> Clone()
        {
            return CopyTo(new VistaDB4Expression<T>());
        }
    }
}