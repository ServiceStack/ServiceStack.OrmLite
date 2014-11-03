using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite.Support
{
    internal abstract class LoadList<Into, From>
    {
        protected IDbCommand dbCmd;
        protected SqlExpression<From> expr;

        protected IOrmLiteDialectProvider dialectProvider;
        protected List<Into> parentResults;
        protected ModelDefinition modelDef;
        protected List<FieldDefinition> fieldDefs;
        protected string subSql;

        public List<FieldDefinition> FieldDefs
        {
            get { return fieldDefs; }
        }

        public List<Into> ParentResults
        {
            get { return parentResults; }
        }

        protected LoadList(IDbCommand dbCmd, SqlExpression<From> expr)
        {
            dialectProvider = dbCmd.GetDialectProvider();

            if (expr == null)
                expr = dialectProvider.SqlExpression<From>();

            this.dbCmd = dbCmd;
            this.expr = expr;

            var sql = expr.SelectInto<Into>();
            parentResults = dbCmd.ExprConvertToList<Into>(sql);

            modelDef = ModelDefinition<Into>.Definition;
            fieldDefs = modelDef.AllFieldDefinitionsArray.Where(x => x.IsReference).ToList();

            subSql = dialectProvider.GetLoadChildrenSubSelect(modelDef, expr);
        }

        protected string GetRefListSql(ModelDefinition refModelDef, FieldDefinition refField)
        {
            var sqlRef = "SELECT {0} FROM {1} WHERE {2} IN ({3})".Fmt(
                dialectProvider.GetColumnNames(refModelDef),
                dialectProvider.GetQuotedTableName(refModelDef),
                dialectProvider.GetQuotedColumnName(refField),
                subSql);

            return sqlRef;
        }

        protected void SetListChildResults(FieldDefinition fieldDef, Type refType, IList childResults, FieldDefinition refField)
        {
            var map = new Dictionary<object, List<object>>();
            List<object> refValues;

            foreach (var result in childResults)
            {
                var refValue = refField.GetValue(result);
                if (!map.TryGetValue(refValue, out refValues))
                {
                    map[refValue] = refValues = new List<object>();
                }
                refValues.Add(result);
            }

            var untypedApi = dbCmd.CreateTypedApi(refType);
            foreach (var result in parentResults)
            {
                var pkValue = modelDef.PrimaryKey.GetValue(result);
                if (map.TryGetValue(pkValue, out refValues))
                {
                    var castResults = untypedApi.Cast(refValues);
                    fieldDef.SetValueFn(result, castResults);
                }
            }
        }

        protected string GetRefSelfSql(FieldDefinition refSelf, ModelDefinition refModelDef)
        {
            //Load Self Table.RefTableId PK
            expr.Select(dialectProvider.GetQuotedColumnName(refSelf));
            var subSqlRef = expr.ToSelectStatement();

            var sqlRef = "SELECT {0} FROM {1} WHERE {2} IN ({3})".Fmt(
                dialectProvider.GetColumnNames(refModelDef),
                dialectProvider.GetQuotedTableName(refModelDef),
                dialectProvider.GetQuotedColumnName(refModelDef.PrimaryKey),
                subSqlRef);

            return sqlRef;
        }

        protected string GetRefFieldSql(ModelDefinition refModelDef, FieldDefinition refField)
        {
            var sqlRef = "SELECT {0} FROM {1} WHERE {2} IN ({3})".Fmt(
                dialectProvider.GetColumnNames(refModelDef),
                dialectProvider.GetQuotedTableName(refModelDef),
                dialectProvider.GetQuotedColumnName(refField),
                subSql);
            return sqlRef;
        }

        protected void SetRefSelfChildResults(FieldDefinition fieldDef, ModelDefinition refModelDef, FieldDefinition refSelf, IList childResults)
        {
            var map = new Dictionary<object, object>();

            foreach (var result in childResults)
            {
                var pkValue = refModelDef.PrimaryKey.GetValue(result);
                map[pkValue] = result;
            }

            foreach (var result in parentResults)
            {
                object childResult;
                var fkValue = refSelf.GetValue(result);
                if (fkValue != null && map.TryGetValue(fkValue, out childResult))
                {
                    fieldDef.SetValueFn(result, childResult);
                }
            }
        }

        protected void SetRefFieldChildResults(FieldDefinition fieldDef, FieldDefinition refField, IList childResults)
        {
            var map = new Dictionary<object, object>();

            foreach (var result in childResults)
            {
                var refValue = refField.GetValue(result);
                map[refValue] = result;
            }

            foreach (var result in parentResults)
            {
                object childResult;
                var pkValue = modelDef.PrimaryKey.GetValue(result);
                if (map.TryGetValue(pkValue, out childResult))
                {
                    fieldDef.SetValueFn(result, childResult);
                }
            }
        }
    }

    internal class LoadListSync<Into, From> : LoadList<Into, From>
    {
        public LoadListSync(IDbCommand dbCmd, SqlExpression<From> expr) : base(dbCmd, expr) {}

        public void SetRefFieldList(FieldDefinition fieldDef, Type refType)
        {
            var refModelDef = refType.GetModelDefinition();
            var refField = modelDef.GetRefFieldDef(refModelDef, refType);

            var sqlRef = GetRefListSql(refModelDef, refField);

            var childResults = dbCmd.ConvertToList(refType, sqlRef);

            SetListChildResults(fieldDef, refType, childResults, refField);
        }

        public void SetRefField(FieldDefinition fieldDef, Type refType)
        {
            var refModelDef = refType.GetModelDefinition();

            var refSelf = modelDef.GetSelfRefFieldDefIfExists(refModelDef, fieldDef);
            var refField = refSelf == null
                ? modelDef.GetRefFieldDef(refModelDef, refType)
                : modelDef.GetRefFieldDefIfExists(refModelDef);

            if (refField != null)
            {
                var sqlRef = GetRefFieldSql(refModelDef, refField);
                var childResults = dbCmd.ConvertToList(refType, sqlRef);
                SetRefFieldChildResults(fieldDef, refField, childResults);
            }
            else if (refSelf != null)
            {
                var sqlRef = GetRefSelfSql(refSelf, refModelDef);
                var childResults = dbCmd.ConvertToList(refType, sqlRef);
                SetRefSelfChildResults(fieldDef, refModelDef, refSelf, childResults);
            }
        }
    }

#if NET45
    internal class LoadListAsync<Into, From> : LoadList<Into, From>
    {
        public LoadListAsync(IDbCommand dbCmd, SqlExpression<From> expr) : base(dbCmd, expr) { }

        public async Task SetRefFieldListAsync(FieldDefinition fieldDef, Type refType, CancellationToken token)
        {
            var refModelDef = refType.GetModelDefinition();
            var refField = modelDef.GetRefFieldDef(refModelDef, refType);

            var sqlRef = GetRefListSql(refModelDef, refField);

            var childResults = await dbCmd.ConvertToListAsync(refType, sqlRef, token);

            SetListChildResults(fieldDef, refType, childResults, refField);
        }

        public async Task SetRefFieldAsync(FieldDefinition fieldDef, Type refType, CancellationToken token)
        {
            var refModelDef = refType.GetModelDefinition();

            var refSelf = modelDef.GetSelfRefFieldDefIfExists(refModelDef, fieldDef);
            var refField = refSelf == null
                ? modelDef.GetRefFieldDef(refModelDef, refType)
                : modelDef.GetRefFieldDefIfExists(refModelDef);

            if (refField != null)
            {
                var sqlRef = GetRefFieldSql(refModelDef, refField);
                var childResults = await dbCmd.ConvertToListAsync(refType, sqlRef, token);
                SetRefFieldChildResults(fieldDef, refField, childResults);
            }
            else if (refSelf != null)
            {
                var sqlRef = GetRefSelfSql(refSelf, refModelDef);
                var childResults = await dbCmd.ConvertToListAsync(refType, sqlRef, token);
                SetRefSelfChildResults(fieldDef, refModelDef, refSelf, childResults);
            }
        }
    }
#endif
}