using System;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.VistaDB
{
    public class VistaDB5Expression<T> : SqlExpression<T>
    {
        protected virtual string GetPagingOffsetExpression(int rows)
        {
            return String.Format("\nOFFSET {0} ROWS", rows);
        }

        protected virtual string GetPagingFetchExpression(int rows)
        {
            return String.Format("\nFETCH NEXT {0} ROWS ONLY", Rows.Value);
        }

        protected override string ApplyPaging(string sql)
        {
            var skip = Skip.GetValueOrDefault();

            if ((skip > 0 || Rows.HasValue) && String.IsNullOrWhiteSpace(OrderByExpression))
            {
                //Ordering by the first column in select list
                sql += "\nORDER BY 1";
            }

            if (skip > 0)
                sql += this.GetPagingOffsetExpression(skip);

            if (Rows.HasValue)
            {
                if (skip == 0)
                    sql += this.GetPagingOffsetExpression(0);

                sql += this.GetPagingFetchExpression(Rows.Value);
            }

            return sql;
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

        public override string LimitExpression { get { return ""; } }

        protected override object VisitColumnAccessMethod(MethodCallExpression m)
        {
            if (m.Arguments.Count == 1 && m.Method.Name == "Equals")
            {
                return Visit(
                    Expression.Equal(
                        Expression.Convert(m.Object, typeof(object)),
                        Expression.Convert(m.Arguments.First(), typeof(object))));
            }
            else
            {
                return base.VisitColumnAccessMethod(m);
            }
        }

        public override SqlExpression<T> Clone()
        {
            return CopyTo(new VistaDB5Expression<T>());
        }
    }
}
