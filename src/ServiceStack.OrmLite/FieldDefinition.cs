//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2013 ServiceStack, Inc. All Rights Reserved.
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

        public string FieldName => this.Alias ?? this.Name;

        public Type FieldType { get; set; }

        public object FieldTypeDefaultValue { get; set; }

        public Type TreatAsType { get; set; }

        public Type ColumnType => TreatAsType ?? FieldType;

        public PropertyInfo PropertyInfo { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool AutoIncrement { get; set; }

        public bool AutoId { get; set; }

        public bool IsNullable { get; set; }

        public bool IsIndexed { get; set; }

        public bool IsUniqueIndex { get; set; }

        public bool IsClustered { get; set; }

        public bool IsNonClustered { get; set; }
        
        public string IndexName { get; set; }

        public bool IsRowVersion { get; set; }

        public int? FieldLength { get; set; }  // Precision for Decimal Type

        public int? Scale { get; set; }  //  for decimal type

        public string DefaultValue { get; set; }

        public string CheckConstraint { get; set; }

        public bool IsUniqueConstraint { get; set; }
        
        public ForeignKeyConstraint ForeignKey { get; set; }

        public GetMemberDelegate GetValueFn { get; set; }

        public SetMemberDelegate SetValueFn { get; set; }

        public object GetValue(object onInstance)
        {
            return this.GetValueFn?.Invoke(onInstance);
        }

        public string GetQuotedName(IOrmLiteDialectProvider dialectProvider)
        {
            return IsRowVersion
                ? dialectProvider.GetRowVersionSelectColumn(this).ToString()
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

        public string CustomSelect { get; set; }

        public bool RequiresAlias => Alias != null || CustomSelect != null;

        public string BelongToModelName { get; set; }

        public bool IsReference { get; set; }

        public string CustomFieldDefinition { get; set; }

        public bool IsRefType { get; set; }

        public bool IgnoreOnUpdate { get; set; }

        public bool IgnoreOnInsert { get; set; }

        public bool ReturnOnInsert { get; set; }

        public override string ToString() => Name;

        public bool ShouldSkipInsert() => IgnoreOnInsert || AutoIncrement || IsComputed || IsRowVersion;

        public bool ShouldSkipUpdate() => IgnoreOnUpdate || IsComputed;

        public bool ShouldSkipDelete() => IsComputed;

        public bool IsSelfRefField(FieldDefinition fieldDef)
        {
            return (fieldDef.Alias != null && IsSelfRefField(fieldDef.Alias))
                    || IsSelfRefField(fieldDef.Name);
        }

        public bool IsSelfRefField(string name)
        {
            return (Alias != null && Alias + "Id" == name)
                    || Name + "Id" == name;
        }

        public FieldDefinition Clone(Action<FieldDefinition> modifier = null)
        {
            var fieldDef = new FieldDefinition
            {
                Name = Name,
                Alias = Alias,
                FieldType = FieldType,
                FieldTypeDefaultValue = FieldTypeDefaultValue,
                TreatAsType = TreatAsType,
                PropertyInfo = PropertyInfo,
                IsPrimaryKey = IsPrimaryKey,
                AutoIncrement = AutoIncrement,
                AutoId = AutoId,
                IsNullable = IsNullable,
                IsIndexed = IsIndexed,
                IsUniqueIndex = IsUniqueIndex,
                IsClustered = IsClustered,
                IsNonClustered = IsNonClustered,
                IsRowVersion = IsRowVersion,
                FieldLength = FieldLength,
                Scale = Scale,
                DefaultValue = DefaultValue,
                CheckConstraint = CheckConstraint,
                IsUniqueConstraint = IsUniqueConstraint,
                ForeignKey = ForeignKey,
                GetValueFn = GetValueFn,
                SetValueFn = SetValueFn,
                Sequence = Sequence,
                IsComputed = IsComputed,
                ComputeExpression = ComputeExpression,
                CustomSelect = CustomSelect,
                BelongToModelName = BelongToModelName,
                IsReference = IsReference,
                CustomFieldDefinition = CustomFieldDefinition,
                IsRefType = IsRefType,
            };

            modifier?.Invoke(fieldDef);
            return fieldDef;
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

        public string GetForeignKeyName(ModelDefinition modelDef, ModelDefinition refModelDef, INamingStrategy namingStrategy, FieldDefinition fieldDef)
        {
            if (ForeignKeyName.IsNullOrEmpty())
            {
                var modelName = modelDef.IsInSchema
                    ? $"{modelDef.Schema}_{namingStrategy.GetTableName(modelDef.ModelName)}"
                    : namingStrategy.GetTableName(modelDef.ModelName);

                var refModelName = refModelDef.IsInSchema
                    ? $"{refModelDef.Schema}_{namingStrategy.GetTableName(refModelDef.ModelName)}"
                    : namingStrategy.GetTableName(refModelDef.ModelName);

                var fkName = $"FK_{modelName}_{refModelName}_{fieldDef.FieldName}";
                return namingStrategy.ApplyNameRestrictions(fkName);
            }
            return ForeignKeyName;
        }
    }
}