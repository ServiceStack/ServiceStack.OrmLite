//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2013 Service Stack LLC. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Reflection;

namespace ServiceStack.OrmLite
{
    public class FieldDefinition
    {
        public string Name { get; set; }

        public string Alias { get; set; }

        public string FieldName
        {
            get { return this.Alias ?? this.Name; }
        }

        public Type FieldType { get; set; }

        public Type TreatAsType { get; set; }

        public Type ColumnType
        {
            get { return TreatAsType ?? FieldType; }
        }

        public PropertyInfo PropertyInfo { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool AutoIncrement { get; set; }

        public bool IsNullable { get; set; }

        public bool IsIndexed { get; set; }

        public bool IsUnique { get; set; }

        public bool IsClustered { get; set; }

        public bool IsNonClustered { get; set; }

        public bool IsRowVersion { get; set; }

        public int? FieldLength { get; set; }  // Precision for Decimal Type

        public int? Scale { get; set; }  //  for decimal type

        public string DefaultValue { get; set; }

        public ForeignKeyConstraint ForeignKey { get; set; }

        public PropertyGetterDelegate GetValueFn { get; set; }

        public PropertySetterDelegate SetValueFn { get; set; }

        public object GetValue(object onInstance)
        {
            return this.GetValueFn == null ? null : this.GetValueFn(onInstance);
        }

        public string GetQuotedName(IOrmLiteDialectProvider dialectProvider)
        {
            return IsRowVersion
                ? dialectProvider.GetRowVersionColumnName(this)
                : dialectProvider.GetQuotedColumnName(FieldName);
        }

        public string GetQuotedValue(object fromInstance, IOrmLiteDialectProvider dialect = null)
        {
            var value = GetValue(fromInstance);
            return (dialect ?? OrmLiteConfig.DialectProvider).GetQuotedValue(value, ColumnType);
        }

        public string Sequence { get; set; }

        public bool IsComputed { get; set; }

        public string ComputeExpression { get; set; }

        public string BelongToModelName { get; set; }

        public bool IsReference { get; set; }

        public string CustomFieldDefinition { get; set; }

        public bool IsRefType { get; set; }

        public bool ShouldSkipInsert()
        {
            return AutoIncrement || IsComputed || IsRowVersion;
        }

        public bool ShouldSkipUpdate()
        {
            return IsComputed;
        }

        public bool ShouldSkipDelete()
        {
            return IsComputed;
        }
    }

    public class ForeignKeyConstraint
    {
        public ForeignKeyConstraint(Type type, string onDelete = null, string onUpdate = null, string foreignKeyName = null)
        {
            ReferenceType = type;
            OnDelete = onDelete;
            OnUpdate = onUpdate;
            ForeignKeyName = foreignKeyName;
        }

        public Type ReferenceType { get; private set; }
        public string OnDelete { get; private set; }
        public string OnUpdate { get; private set; }
        public string ForeignKeyName { get; private set; }

        public string GetForeignKeyName(ModelDefinition modelDef, ModelDefinition refModelDef, INamingStrategy NamingStrategy, FieldDefinition fieldDef)
        {
            if (ForeignKeyName.IsNullOrEmpty())
            {
                var modelName = modelDef.IsInSchema
                    ? modelDef.Schema + "_" + NamingStrategy.GetTableName(modelDef.ModelName)
                    : NamingStrategy.GetTableName(modelDef.ModelName);

                var refModelName = refModelDef.IsInSchema
                    ? refModelDef.Schema + "_" + NamingStrategy.GetTableName(refModelDef.ModelName)
                    : NamingStrategy.GetTableName(refModelDef.ModelName);

                var fkName = string.Format("FK_{0}_{1}_{2}", modelName, refModelName, fieldDef.FieldName);
                return NamingStrategy.ApplyNameRestrictions(fkName);
            }
            else { return ForeignKeyName; }
        }
    }
}