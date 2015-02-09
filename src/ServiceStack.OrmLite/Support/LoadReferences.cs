﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite.Support
{
    internal abstract class LoadReferences<T>
    {
        protected IDbCommand dbCmd;
        protected T instance;
        protected ModelDefinition modelDef;
        protected List<FieldDefinition> fieldDefs;
        protected object pkValue;
        protected IOrmLiteDialectProvider dialectProvider;

        protected LoadReferences(IDbCommand dbCmd, T instance)
        {
            this.dbCmd = dbCmd;
            this.instance = instance;
            
            modelDef = ModelDefinition<T>.Definition;
            fieldDefs = modelDef.AllFieldDefinitionsArray.Where(x => x.IsReference).ToList();
            pkValue = modelDef.PrimaryKey.GetValue(instance);
            dialectProvider = dbCmd.GetDialectProvider();
        }

        public List<FieldDefinition> FieldDefs
        {
            get { return fieldDefs; }
        }

        protected string GetRefListSql(Type refType)
        {
            var refModelDef = refType.GetModelDefinition();

            var refField = modelDef.GetRefFieldDef(refModelDef, refType);

            var sqlFilter = dialectProvider.GetQuotedColumnName(refField.FieldName) + "={0}";
            var sql = dialectProvider.ToSelectStatement(refType, sqlFilter, pkValue);

            return sql;
        }

        protected string GetRefFieldSql(Type refType, FieldDefinition refField)
        {
            var sqlFilter = dialectProvider.GetQuotedColumnName(refField.FieldName) + "={0}";
            var sql = dialectProvider.ToSelectStatement(refType, sqlFilter, pkValue);
            return sql;
        }

        protected string GetRefSelfSql(Type refType, FieldDefinition refSelf, ModelDefinition refModelDef)
        {
            //Load Self Table.RefTableId PK
            var refPkValue = refSelf.GetValue(instance);
            var sqlFilter = dialectProvider.GetQuotedColumnName(refModelDef.PrimaryKey.FieldName) + "={0}";
            var sql = dialectProvider.ToSelectStatement(refType, sqlFilter, refPkValue);
            return sql;
        }
    }

    internal class LoadReferencesSync<T> : LoadReferences<T>
    {
        public LoadReferencesSync(IDbCommand dbCmd, T instance) 
            : base(dbCmd, instance) {}

        public void SetRefFieldList(FieldDefinition fieldDef, Type refType)
        {
            var sql = GetRefListSql(refType);

            var results = dbCmd.ConvertToList(refType, sql);
            fieldDef.SetValueFn(instance, results);
        }

        public void SetRefField(FieldDefinition fieldDef, Type refType)
        {
            var refModelDef = refType.GetModelDefinition();

            var refSelf = modelDef.GetSelfRefFieldDefIfExists(refModelDef, fieldDef);
            var refField = refSelf == null
                ? modelDef.GetRefFieldDef(refModelDef, refType)
                : modelDef.GetRefFieldDefIfExists(refModelDef);

            if (refSelf != null)
            {
                var sql = GetRefSelfSql(refType, refSelf, refModelDef);
                var result = dbCmd.ConvertTo(refType, sql);
                fieldDef.SetValueFn(instance, result);
            }
            else if (refField != null)
            {
                var sql = GetRefFieldSql(refType, refField);
                var result = dbCmd.ConvertTo(refType, sql);
                fieldDef.SetValueFn(instance, result);
            }
        }
    }

#if NET45
    internal class LoadReferencesAsync<T> : LoadReferences<T>
    {
        public LoadReferencesAsync(IDbCommand dbCmd, T instance)
            : base(dbCmd, instance) { }

        public async Task SetRefFieldList(FieldDefinition fieldDef, Type refType, CancellationToken token)
        {
            var sql = GetRefListSql(refType);

            var results = await dbCmd.ConvertToListAsync(refType, sql, token);
            fieldDef.SetValueFn(instance, results);
        }

        public async Task SetRefField(FieldDefinition fieldDef, Type refType, CancellationToken token)
        {
            var refModelDef = refType.GetModelDefinition();

            var refSelf = modelDef.GetSelfRefFieldDefIfExists(refModelDef, fieldDef);
            var refField = refSelf == null
                ? modelDef.GetRefFieldDef(refModelDef, refType)
                : modelDef.GetRefFieldDefIfExists(refModelDef);

            if (refField != null)
            {
                var sql = GetRefFieldSql(refType, refField);
                var result = await dbCmd.ConvertToAsync(refType, sql, token);
                fieldDef.SetValueFn(instance, result);
            }
            else if (refSelf != null)
            {
                var sql = GetRefSelfSql(refType, refSelf, refModelDef);
                var result = await dbCmd.ConvertToAsync(refType, sql, token);
                fieldDef.SetValueFn(instance, result);
            }
        }
    }
#endif
}