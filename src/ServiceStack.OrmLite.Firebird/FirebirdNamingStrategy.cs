using System;

namespace ServiceStack.OrmLite.Firebird
{
    public class FirebirdNamingStrategy : OrmLiteNamingStrategyBase
    {
        private static int MaxNameLength { get; set; }

        public FirebirdNamingStrategy() : this(31) { }

        public FirebirdNamingStrategy(int maxNameLength)
        {
            MaxNameLength = maxNameLength;
        }

        public override string GetSchemaName(string name)
        {
            return name != null 
                ? ApplyNameRestrictions(name).ToUpper() 
                : null;
        }

        public override string GetTableName(string name)
        {
            return ApplyNameRestrictions(name).ToUpper();
        }

        public override string GetColumnName(string name)
        {
            return ApplyNameRestrictions(name).ToUpper();
        }

        public override string GetSequenceName(string modelName, string fieldName)
        {
            var seqName = ApplyNameRestrictions("gen_" + modelName + "_" + fieldName).ToLower();
            return seqName;
        }

        public override string ApplyNameRestrictions(string name)
        {
            if (name.Length > MaxNameLength) name = Squash(name);
            return name.TrimStart('_');
        }

        public override string GetTableName(ModelDefinition modelDef)
        {
            return GetTableName(modelDef.ModelName);
        }

        private static string Squash(string name)
        {
            // First try squashing out the vowels
            var squashed = name.Replace("a", "").Replace("e", "").Replace("i", "").Replace("o", "").Replace("u", "").Replace("y", "");
            squashed = squashed.Replace("A", "").Replace("E", "").Replace("I", "").Replace("O", "").Replace("U", "").Replace("Y", "");
            if (squashed.Length > MaxNameLength)
            {   // Still too long, squash out every 4th letter, starting at the 3rd
                for (var i = 2; i < squashed.Length - 1; i += 4)
                    squashed = squashed.Substring(0, i) + squashed.Substring(i + 1);
            }
            if (squashed.Length > MaxNameLength)
            {   // Still too long, truncate
                squashed = squashed.Substring(0, MaxNameLength);
            }
            return squashed;
        }
    }
}