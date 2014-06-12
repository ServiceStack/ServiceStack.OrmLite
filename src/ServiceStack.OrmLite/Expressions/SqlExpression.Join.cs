using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ServiceStack.OrmLite
{
    public abstract partial class SqlExpression<T> : ISqlExpression, ISelectableSqlExpression
    {
        List<ModelDefinition> tableDefs = new List<ModelDefinition>();

        public SqlExpression<T> Join<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr = null)
        {
            PrefixFieldWithTableName = true;

            var sourceDef = typeof(Source).GetModelDefinition();
            var targetDef = typeof(Target).GetModelDefinition();

            if (tableDefs.Count == 0)
                tableDefs.Add(modelDef);
            if (!tableDefs.Contains(sourceDef))
                tableDefs.Add(sourceDef);
            if (!tableDefs.Contains(targetDef))
                tableDefs.Add(targetDef);

            var fromExpr = FromExpression;
            var sbJoin = new StringBuilder();

            string sqlExpr;

            if (joinExpr != null)
            {
                sqlExpr = Visit(joinExpr).ToString();
            }
            else
            {
                var refField = OrmLiteReadExtensions.GetRefFieldDef(sourceDef, targetDef, typeof(Source));
                if (refField == null)
                    throw new ArgumentException("Could not infer relationship between {0} and {1}"
                        .Fmt(sourceDef.ModelName, targetDef.ModelName));

                sqlExpr = "\n({0}.{1} = {2}.{3})".Fmt(
                    sourceDef.ModelName.SqlTable(),
                    sourceDef.PrimaryKey.FieldName.SqlColumn(),
                    targetDef.ModelName.SqlTable(),
                    refField.FieldName.SqlColumn());
            }

            sbJoin.Append(" INNER JOIN {0} ".Fmt(targetDef.ModelName.SqlTable()));
            sbJoin.Append(" ON ");
            sbJoin.Append(sqlExpr);

            FromExpression = fromExpr + sbJoin;

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
            foreach (var fieldDef in modelDef.FieldDefinitions)
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
                                tableDef.ModelName.SqlTable(),
                                tableFieldDef.IsRowVersion 
                                    ? OrmLiteConfig.DialectProvider.GetRowVersionColumnName(tableFieldDef)
                                    : tableFieldDef.FieldName.SqlColumn());
                            break;
                        }
                    }

                    if (found)
                        break;
                }

                if (!found)
                {
                    if (sbSelect.Length > 0)
                        sbSelect.Append(", ");

                    sbSelect.AppendFormat("{0}.{1}",
                        modelDef.ModelName.SqlTable(),
                        fieldDef.IsRowVersion
                            ? OrmLiteConfig.DialectProvider.GetRowVersionColumnName(fieldDef)
                            : fieldDef.FieldName.SqlColumn());
                }
            }

            SelectExpression = "SELECT " + sbSelect;

            return ToSelectStatement();
        }
    }
}