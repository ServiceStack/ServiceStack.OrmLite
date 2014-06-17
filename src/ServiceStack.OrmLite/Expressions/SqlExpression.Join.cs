using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ServiceStack.OrmLite
{
    public abstract partial class SqlExpression<T> : ISqlExpression
    {
        List<ModelDefinition> tableDefs = new List<ModelDefinition>();

        public SqlExpression<T> Join<Target>(Expression<Func<T, Target, bool>> joinExpr = null)
        {
            return InternalJoin("INNER JOIN", joinExpr);
        }

        public SqlExpression<T> Join<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr = null)
        {
            return InternalJoin("INNER JOIN", joinExpr);
        }

        public SqlExpression<T> LeftJoin<Target>(Expression<Func<T, Target, bool>> joinExpr = null)
        {
            return InternalJoin("LEFT JOIN", joinExpr);
        }

        public SqlExpression<T> LeftJoin<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr = null)
        {
            return InternalJoin("LEFT JOIN", joinExpr);
        }

        public SqlExpression<T> RightJoin<Target>(Expression<Func<T, Target, bool>> joinExpr = null)
        {
            return InternalJoin("RIGHT JOIN", joinExpr);
        }

        public SqlExpression<T> RightJoin<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr = null)
        {
            return InternalJoin("RIGHT JOIN", joinExpr);
        }

        public SqlExpression<T> FullJoin<Target>(Expression<Func<T, Target, bool>> joinExpr = null)
        {
            return InternalJoin("FULL JOIN", joinExpr);
        }

        public SqlExpression<T> FullJoin<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr = null)
        {
            return InternalJoin("FULL JOIN", joinExpr);
        }

        private SqlExpression<T> InternalJoin<Source, Target>(string joinType,
            Expression<Func<Source, Target, bool>> joinExpr)
        {
            PrefixFieldWithTableName = true;

            var sourceDef = typeof (Source).GetModelDefinition();
            var targetDef = typeof (Target).GetModelDefinition();

            var fromExpr = FromExpression;
            var sbJoin = new StringBuilder();

            string sqlExpr;

            //Changes how Sql Expressions are generated.
            useFieldName = true; sep = " ";

            if (joinExpr != null)
            {
                sqlExpr = Visit(joinExpr).ToString();
            }
            else
            {
                var parentDef = sourceDef;
                var childDef = targetDef;

                var refField = OrmLiteReadExtensions.GetRefFieldDefIfExists(parentDef, childDef);
                if (refField == null)
                {
                    parentDef = targetDef;
                    childDef = sourceDef;
                    refField = OrmLiteReadExtensions.GetRefFieldDefIfExists(parentDef, childDef);
                }

                if (refField == null)
                {
                    throw new ArgumentException("Could not infer relationship between {0} and {1}"
                                                    .Fmt(sourceDef.ModelName, targetDef.ModelName));
                }

                sqlExpr = "\n({0}.{1} = {2}.{3})".Fmt(
                    SqlTable(parentDef.ModelName),
                    SqlColumn(parentDef.PrimaryKey.FieldName),
                    SqlTable(childDef.ModelName),
                    SqlColumn(refField.FieldName));
            }

            var joinDef = tableDefs.Contains(targetDef) && !tableDefs.Contains(sourceDef)
                ? sourceDef
                : targetDef;

            sbJoin.Append(" {0} {1} ".Fmt(joinType, SqlTable(joinDef.ModelName)));
            sbJoin.Append(" ON ");
            sbJoin.Append(sqlExpr);

            FromExpression = fromExpr + sbJoin;

            if (tableDefs.Count == 0)
                tableDefs.Add(modelDef);
            if (!tableDefs.Contains(sourceDef))
                tableDefs.Add(sourceDef);
            if (!tableDefs.Contains(targetDef))
                tableDefs.Add(targetDef);

            return this;
        }

        public string SelectInto<TModel>()
        {
            if (typeof(TModel) == typeof(T) && !PrefixFieldWithTableName)
            {
                return ToSelectStatement();
            }

            if (this.tableDefs.Count == 0)
                this.tableDefs.Add(modelDef);

            var sbSelect = new StringBuilder();
            var selectDef = typeof(TModel).GetModelDefinition();
            foreach (var fieldDef in selectDef.FieldDefinitions)
            {
                var found = false;

                foreach (var tableDef in tableDefs)
                {
                    foreach (var tableFieldDef in tableDef.FieldDefinitions)
                    {
                        if (tableFieldDef.Name == fieldDef.Name)
                        {
                            found = true;
                            if (sbSelect.Length > 0)
                                sbSelect.Append(", ");

                            sbSelect.AppendFormat("{0}.{1}",
                                SqlTable(tableDef.ModelName),
                                tableFieldDef.GetQuotedName(DialectProvider));
                            break;
                        }
                    }

                    if (found)
                        break;
                }
            }

            SelectExpression = "SELECT " + sbSelect;

            return ToSelectStatement();
        }

        public virtual SqlExpression<T> Where<Target>(Expression<Func<Target, bool>> predicate)
        {
            AppendToWhere("AND", predicate);
            return this;
        }

        public virtual SqlExpression<T> Where<Source, Target>(Expression<Func<Source, Target, bool>> predicate)
        {
            AppendToWhere("AND", predicate);
            return this;
        }

        public virtual SqlExpression<T> And<Target>(Expression<Func<Target, bool>> predicate)
        {
            AppendToWhere("AND", predicate);
            return this;
        }

        public virtual SqlExpression<T> And<Source, Target>(Expression<Func<Source, Target, bool>> predicate)
        {
            AppendToWhere("AND", predicate);
            return this;
        }

        public virtual SqlExpression<T> Or<Target>(Expression<Func<Target, bool>> predicate)
        {
            AppendToWhere("OR", predicate);
            return this;
        }

        public virtual SqlExpression<T> Or<Source, Target>(Expression<Func<Source, Target, bool>> predicate)
        {
            AppendToWhere("OR", predicate);
            return this;
        }
    }
}