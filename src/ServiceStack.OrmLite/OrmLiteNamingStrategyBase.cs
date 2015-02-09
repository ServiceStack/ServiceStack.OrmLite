﻿//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//   Tomasz Kubacki (tomasz.kubacki@gmail.com)
//
// Copyright 2012 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack.
//

namespace ServiceStack.OrmLite
{
    public class OrmLiteNamingStrategyBase : INamingStrategy
    {
        public virtual string GetSchemaName(string name)
        {
            return name;
        }

        public virtual string GetSchemaName(ModelDefinition modelDef)
        {
            return GetSchemaName(modelDef.Schema);
        }

        public virtual string GetTableName(string name)
        {
            return name;
        }

        public virtual string GetTableName(ModelDefinition modelDef)
        {
            return GetTableName(modelDef.ModelName);
        }

        public virtual string GetColumnName(string name)
        {
            return name;
        }

        public virtual string GetSequenceName(string modelName, string fieldName)
        {
            return "SEQ_" + modelName + "_" + fieldName;
        }

        public virtual string ApplyNameRestrictions(string name)
        {
            return name;
        }
    }
}
