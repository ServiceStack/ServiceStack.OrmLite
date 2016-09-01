using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public delegate string JoinFormatDelegate(IOrmLiteDialectProvider dialect, ModelDefinition tableDef, string joinExpr);

    public abstract partial class SqlExpression<T> : ISqlExpression
    {
        protected List<ModelDefinition> tableDefs = new List<ModelDefinition>();

        public bool IsJoinedTable(Type type)
        {
            return tableDefs.FirstOrDefault(x => x.ModelType == type) != null;
        }

        public SqlExpression<T> Join<Target>(Expression<Func<T, Target, bool>> joinExpr = null)
        {
            return InternalJoin("INNER JOIN", joinExpr);
        }

        public SqlExpression<T> Join<Target>(Expression<Func<T, Target, bool>> joinExpr, JoinFormatDelegate joinFormat)
        {
            if (joinFormat == null)
                throw new ArgumentNullException(nameof(joinFormat));

            return InternalJoin("INNER JOIN", joinExpr, joinFormat);
        }

        public SqlExpression<T> Join<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr = null)
        {
            return InternalJoin("INNER JOIN", joinExpr);
        }

        public SqlExpression<T> Join<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr, JoinFormatDelegate joinFormat)
        {
            return InternalJoin("INNER JOIN", joinExpr, joinFormat);
        }

        public SqlExpression<T> Join(Type sourceType, Type targetType, Expression joinExpr = null)
        {
            return InternalJoin("INNER JOIN", joinExpr, sourceType.GetModelDefinition(), targetType.GetModelDefinition());
        }

        public SqlExpression<T> LeftJoin<Target>(Expression<Func<T, Target, bool>> joinExpr = null)
        {
            return InternalJoin("LEFT JOIN", joinExpr);
        }

        public SqlExpression<T> LeftJoin<Target>(Expression<Func<T, Target, bool>> joinExpr, JoinFormatDelegate joinFormat)
        {
            if (joinFormat == null)
                throw new ArgumentNullException(nameof(joinFormat));

            return InternalJoin("LEFT JOIN", joinExpr, joinFormat);
        }

        public SqlExpression<T> LeftJoin<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr = null)
        {
            return InternalJoin("LEFT JOIN", joinExpr);
        }

        public SqlExpression<T> LeftJoin<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr, JoinFormatDelegate joinFormat)
        {
            return InternalJoin("LEFT JOIN", joinExpr, joinFormat);
        }

        public SqlExpression<T> LeftJoin(Type sourceType, Type targetType, Expression joinExpr = null)
        {
            return InternalJoin("LEFT JOIN", joinExpr, sourceType.GetModelDefinition(), targetType.GetModelDefinition());
        }

        public SqlExpression<T> RightJoin<Target>(Expression<Func<T, Target, bool>> joinExpr = null)
        {
            return InternalJoin("RIGHT JOIN", joinExpr);
        }

        public SqlExpression<T> RightJoin<Target>(Expression<Func<T, Target, bool>> joinExpr, JoinFormatDelegate joinFormat)
        {
            if (joinFormat == null)
                throw new ArgumentNullException(nameof(joinFormat));

            return InternalJoin("RIGHT JOIN", joinExpr, joinFormat);
        }

        public SqlExpression<T> RightJoin<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr = null)
        {
            return InternalJoin("RIGHT JOIN", joinExpr);
        }

        public SqlExpression<T> RightJoin<Source, Target>(Expression<Func<Source, Target, bool>> joinExpr, JoinFormatDelegate joinFormat)
        {
            return InternalJoin("RIGHT JOIN", joinExpr, joinFormat);
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

        protected SqlExpression<T> InternalJoin<Source, Target>(string joinType, Expression<Func<Source, Target, bool>> joinExpr, JoinFormatDelegate joinFormat = null)
        {
            var sourceDef = typeof(Source).GetModelDefinition();
            var targetDef = typeof(Target).GetModelDefinition();

            return InternalJoin(joinType, joinExpr, sourceDef, targetDef, joinFormat);
        }

        private string InternalCreateSqlFromExpression(Expression joinExpr, bool isCrossJoin) 
        {
            return $"{(isCrossJoin ? "WHERE" : "ON")} {VisitJoin(joinExpr)}";
        }

        private string InternalCreateSqlFromDefinitions(ModelDefinition sourceDef, ModelDefinition targetDef, bool isCrossJoin) 
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
                if(!isCrossJoin)
                    throw new ArgumentException($"Could not infer relationship between {sourceDef.ModelName} and {targetDef.ModelName}");

                return string.Empty;
            }

            return "{0}\n({1}.{2} = {3}.{4})".Fmt(
                isCrossJoin ? "WHERE" : "ON",
                DialectProvider.GetQuotedTableName(parentDef),
                SqlColumn(parentDef.PrimaryKey.FieldName),
                DialectProvider.GetQuotedTableName(childDef),
                SqlColumn(refField.FieldName));
        }

        public SqlExpression<T> CustomJoin(string joinString)
        {
            PrefixFieldWithTableName = true;
            FromExpression += " " + joinString;
            return this;
        }

        private SqlExpression<T> InternalJoin(string joinType, Expression joinExpr, ModelDefinition sourceDef, ModelDefinition targetDef, JoinFormatDelegate joinFormat = null)
        {
            PrefixFieldWithTableName = true;

            //Changes how Sql Expressions are generated.
            useFieldName = true;
            sep = " ";

            if (!tableDefs.Contains(sourceDef))
                tableDefs.Add(sourceDef);
            if (!tableDefs.Contains(targetDef))
                tableDefs.Add(targetDef);

            var isCrossJoin = "CROSS JOIN".Equals(joinType);

            var sqlExpr = joinExpr != null 
                ? InternalCreateSqlFromExpression(joinExpr, isCrossJoin)
                : InternalCreateSqlFromDefinitions(sourceDef, targetDef, isCrossJoin);

            var joinDef = tableDefs.Contains(targetDef) && !tableDefs.Contains(sourceDef)
                ? sourceDef
                : targetDef;

            FromExpression += joinFormat != null
                ? $" {joinType} {joinFormat(DialectProvider, joinDef, sqlExpr)}"
                : $" {joinType} {SqlTable(joinDef)} {sqlExpr}";

            return this;
        }

        public string SelectInto<TModel>()
        {
            if ((CustomSelect && OnlyFields  == null) || (typeof(TModel) == typeof(T) && !PrefixFieldWithTableName))
            {
                return ToSelectStatement();
            }

            var sbSelect = StringBuilderCache.Allocate();
            var selectDef = modelDef;
            var orderedDefs = tableDefs;

            if (typeof(TModel) != typeof(List<object>) && 
                typeof(TModel) != typeof(Dictionary<string, object>) &&
                typeof(TModel) != typeof(object)) //dynamic
            {
                selectDef = typeof(TModel).GetModelDefinition();
                if (selectDef != modelDef && tableDefs.Contains(selectDef))
                {
                    orderedDefs = tableDefs.ToList(); //clone
                    orderedDefs.Remove(selectDef);
                    orderedDefs.Insert(0, selectDef);
                }
            }

            foreach (var fieldDef in selectDef.FieldDefinitions)
            {
                var found = false;

                if (fieldDef.BelongToModelName != null)
                {
                    var tableDef = orderedDefs.FirstOrDefault(x => x.Name == fieldDef.BelongToModelName);
                    if (tableDef != null)
                    {
                        var matchingField = FindWeakMatch(tableDef, fieldDef);
                        if (matchingField != null)
                        {
                            if (OnlyFields == null || OnlyFields.Contains(fieldDef.Name))
                            {
                                if (sbSelect.Length > 0)
                                    sbSelect.Append(", ");

                                if (fieldDef.CustomSelect == null)
                                {
                                    sbSelect.Append($"{DialectProvider.GetQuotedColumnName(tableDef, matchingField)} AS {SqlColumn(fieldDef.Name)}");
                                }
                                else
                                {
                                    sbSelect.Append(fieldDef.CustomSelect + " AS " + fieldDef.FieldName);
                                }

                                continue;
                            }
                        }
                    }
                }

                foreach (var tableDef in orderedDefs)
                {
                    foreach (var tableFieldDef in tableDef.FieldDefinitions)
                    {
                        if (tableFieldDef.Name == fieldDef.Name)
                        {
                            if (OnlyFields != null && !OnlyFields.Contains(fieldDef.Name))
                                continue;

                            if (sbSelect.Length > 0)
                                sbSelect.Append(", ");

                            if (fieldDef.CustomSelect == null)
                            {
                                sbSelect.Append($"{SqlTable(tableDef)}.{tableFieldDef.GetQuotedName(DialectProvider)}");

                                if (tableFieldDef.Alias != null)
                                    sbSelect.Append(" AS ").Append(SqlColumn(fieldDef.Name));
                            }
                            else
                            {
                                sbSelect.Append(tableFieldDef.CustomSelect).Append(" AS ").Append(tableFieldDef.FieldName);
                            }

                            found = true;
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
                        var matchingField = FindWeakMatch(tableDef, fieldDef);
                        if (matchingField != null)
                        {
                            if (OnlyFields != null && !OnlyFields.Contains(fieldDef.Name))
                                continue;

                            if (sbSelect.Length > 0)
                                sbSelect.Append(", ");

                            sbSelect.Append($"{DialectProvider.GetQuotedColumnName(tableDef, matchingField)} as {SqlColumn(fieldDef.Name)}");
                            
                            break;
                        }
                    }
                }
            }

            var select = StringBuilderCache.ReturnAndFree(sbSelect);

            var columns = select.Length > 0 ? select : "*";
            SelectExpression = "SELECT " + (selectDistinct ? "DISTINCT " : "") + columns;

            return ToSelectStatement();
        }

        private static FieldDefinition FindWeakMatch(ModelDefinition tableDef, FieldDefinition fieldDef)
        {
            return tableDef.FieldDefinitions
                .FirstOrDefault(x =>
                    string.Compare(tableDef.Name + x.Name, fieldDef.Name, StringComparison.OrdinalIgnoreCase) == 0
                    || string.Compare(tableDef.ModelName + x.FieldName, fieldDef.Name, StringComparison.OrdinalIgnoreCase) == 0);
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