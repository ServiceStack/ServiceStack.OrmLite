using System;
namespace ServiceStack.OrmLite
{
    public interface INamingStrategy
    {
        string GetTableName(string name);
        string GetTableName(ModelDefinition modelDef);
        string GetColumnName(string name);
        string GetSequenceName(string modelName, string fieldName);
        string ApplyNameRestrictions(string name);
    }
}
