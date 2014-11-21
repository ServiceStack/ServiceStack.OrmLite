using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ServiceStack.OrmLite
{
    public abstract partial class SqlExpression<T> : ISqlExpression
    {
        List<ModelDefinition> tableDefs = new List<ModelDefinition>();

        bool IsJoinedTable(Type type)
        {
            return tableDefs.FirstOrDefault(x => x.ModelType == type) != null;
        }

        public SqlExpression<T> Join<Target>(Expression<Func<T, Target, bool>> joinExpr = null)
        {
            return InternalJoin("INNER JOIN", joinExpr);
        }

        public SqlExpression<T> Join<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr = null)
        {
            return InternalJoin("INNER JOIN", joinExpr);
        }

        public SqlExpression<T> Join(Type sourceType, Type targetType, Expression joinExpr = null)
        {
            return InternalJoin("INNER JOIN", joinExpr, sourceType.GetModelDefinition(), targetType.GetModelDefinition());
        }

        public SqlExpression<T> LeftJoin<Target>(Expression<Func<T, Target, bool>> joinExpr = null)
        {
            return InternalJoin("LEFT JOIN", joinExpr);
        }

        public SqlExpression<T> LeftJoin<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr = null)
        {
            return InternalJoin("LEFT JOIN", joinExpr);
        }

        public SqlExpression<T> LeftJoin(Type sourceType, Type targetType, Expression joinExpr = null)
        {
            return InternalJoin("LEFT JOIN", joinExpr, sourceType.GetModelDefinition(), targetType.GetModelDefinition());
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

        public SqlExpression<T> CrossJoin<Target>(Expression<Func<T, Target, bool>> joinExpr = null)
        {
            return InternalJoin("CROSS JOIN", joinExpr);
        }

        public SqlExpression<T> CrossJoin<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr = null)
        {
            return InternalJoin("CROSS JOIN", joinExpr);
        }

        private SqlExpression<T> InternalJoin<Source, Target>(string joinType, 
            Expression<Func<Source, Target, bool>> joinExpr)
        {
            var sourceDef = typeof(Source).GetModelDefinition();
            var targetDef = typeof(Target).GetModelDefinition();

            return InternalJoin(joinType, joinExpr, sourceDef, targetDef);
        }

        private string InternalCreateSqlFromExpression(Expression joinExpr) 
        {
            return "ON {0}".Fmt(Visit(joinExpr).ToString());
        }

        private string InternalCreateSqlFromDefinitions(ModelDefinition sourceDef, ModelDefinition targetDef, bool allowMissingOnClause) 
        {
            var parentDef = sourceDef;
            var childDef = targetDef;

            var refField = parentDef.GetRefFieldDefIfExists(childDef);
            if (refField == null) 
            {
                parentDef = targetDef;
                childDef = sourceDef;
                refField = parentDef.GetRefFieldDefIfExists(childDef);
            }

            if (refField == null) 
            {
                if(!allowMissingOnClause)
                    throw new ArgumentException("Could not infer relationship between {0} and {1}".Fmt(sourceDef.ModelName, targetDef.ModelName));

                return string.Empty;
            }

            return "ON\n({0}.{1} = {2}.{3})".Fmt(
                DialectProvider.GetQuotedTableName(parentDef),
                SqlColumn(parentDef.PrimaryKey.FieldName),
                DialectProvider.GetQuotedTableName(childDef),
                SqlColumn(refField.FieldName));
        }

        private SqlExpression<T> InternalJoin(string joinType, 
            Expression joinExpr, ModelDefinition sourceDef, ModelDefinition targetDef)
        {
            PrefixFieldWithTableName = true;

            //Changes how Sql Expressions are generated.
            useFieldName = true;
            sep = " ";

            var sqlExpr = joinExpr != null 
                ? InternalCreateSqlFromExpression(joinExpr) 
                : InternalCreateSqlFromDefinitions(sourceDef, targetDef, "CROSS JOIN".Equals(joinType));

            var joinDef = tableDefs.Contains(targetDef) && !tableDefs.Contains(sourceDef)
                              ? sourceDef
                              : targetDef;

            FromExpression += " {0} {1} {2}".Fmt(joinType, SqlTable(joinDef), sqlExpr);

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

            var sbSelect = new StringBuilder();
            var selectDef = typeof(TModel).GetModelDefinition();

            var orderedDefs = tableDefs;
            if (selectDef != modelDef && tableDefs.Contains(selectDef))
            {
                orderedDefs = tableDefs.ToList(); //clone
                orderedDefs.Remove(selectDef);
                orderedDefs.Insert(0, selectDef);
            }

            foreach (var fieldDef in selectDef.FieldDefinitions)
            {
                var found = false;

                foreach (var tableDef in orderedDefs)
                {
                    foreach (var tableFieldDef in tableDef.FieldDefinitions)
                    {
                        if (tableFieldDef.Name == fieldDef.Name)
                        {
                            found = true;
                            if (sbSelect.Length > 0)
                                sbSelect.Append(", ");

                            sbSelect.AppendFormat("{0}.{1}",
                                SqlTable(tableDef),
                                tableFieldDef.GetQuotedName(DialectProvider));

                            if (tableFieldDef.Alias != null)
                                sbSelect.Append(" AS ").Append(DialectProvider.NamingStrategy.GetColumnName(fieldDef.Name));

                            break;
                        }
                    }

                    if (found)
                        break;
                }

                if (!found)
                {
                    // Add support for auto mapping `{Table}{Field}` convention
                    foreach (var tableDef in orderedDefs)
                    {
                        var matchingField = tableDef.FieldDefinitions
                            .FirstOrDefault(x =>
                                string.Compare(tableDef.Name + x.Name, fieldDef.Name, StringComparison.OrdinalIgnoreCase) == 0
                             || string.Compare(tableDef.ModelName + x.FieldName, fieldDef.Name, StringComparison.OrdinalIgnoreCase) == 0);

                        if (matchingField != null)
                        {
                            if (sbSelect.Length > 0)
                                sbSelect.Append(", ");

                            sbSelect.AppendFormat("{0} as {1}",
                                DialectProvider.GetQuotedColumnName(tableDef, matchingField),
                                fieldDef.Name);
                            
                            break;
                        }
                    }
                }
            }

            var columns = sbSelect.Length > 0 ? sbSelect.ToString() : "*";
            SelectExpression = "SELECT " + (selectDistinct ? "DISTINCT " : "") + columns;

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

        public Tuple<ModelDefinition,FieldDefinition> FirstMatchingField(string fieldName)
        {
            foreach (var tableDef in tableDefs)
            {
                var firstField = tableDef.FieldDefinitions.FirstOrDefault(x => 
                    string.Compare(x.Name, fieldName, StringComparison.OrdinalIgnoreCase) == 0
                 || string.Compare(x.FieldName, fieldName, StringComparison.OrdinalIgnoreCase) == 0);

                if (firstField != null)
                {
                    return Tuple.Create(tableDef, firstField);
                }
            }
            //Fallback to fully qualified '{Table}{Field}' property convention
            foreach (var tableDef in tableDefs)
            {
                var firstField = tableDef.FieldDefinitions.FirstOrDefault(x =>
                    string.Compare(tableDef.Name + x.Name, fieldName, StringComparison.OrdinalIgnoreCase) == 0
                 || string.Compare(tableDef.ModelName + x.FieldName, fieldName, StringComparison.OrdinalIgnoreCase) == 0);

                if (firstField != null)
                {
                    return Tuple.Create(tableDef, firstField);
                }
            }
            return null;
        }
    }
}