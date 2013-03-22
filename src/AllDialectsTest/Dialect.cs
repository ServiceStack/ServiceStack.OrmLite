using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data;
using ServiceStack.Common.Utils;
using ServiceStack.DataAnnotations;
using ServiceStack.Common;
using System.Reflection;
using System.IO;
using ServiceStack.OrmLite;

namespace AllDialectsTest
{
	public class Dialect
	{
		//private static Dictionary<string, Types> dialectDbTypes = new Dictionary<string, Types>();

		public string Name { get; set; }
		public string PathToAssembly { get; set; }
		public string AssemblyName { get; set; }
		public string ClassName { get; set; }
		public string InstanceFieldName { get; set; }
		public string ConnectionString { get; set; }

		public IOrmLiteDialectProvider DialectProvider
		{
			get
			{
				var assembly = Assembly.LoadFrom(Path.Combine(PathToAssembly, AssemblyName));
				var type = assembly.GetType(ClassName);
				if (type == null)
					throw new Exception(
						string.Format("Can not load type '{0}' from assembly '{1}'",
							ClassName, Path.Combine(PathToAssembly, AssemblyName)));
				
				var fi = type.GetField(InstanceFieldName);
				if (fi == null)
					throw new Exception(
						string.Format("Can not get Field '{0}' from class '{1}'",
							InstanceFieldName, ClassName));

				var o = fi.GetValue(null);
				var dialect = o as IOrmLiteDialectProvider;

				if (dialect == null)
					throw new Exception(
						string.Format("Can not cast  from '{0}' to '{1}'",
							o, typeof(IOrmLiteDialectProvider))
					);


				//Angel can you check if we need this now? DbTypes are now static per Dialiect provider so shouldn't conflict, i.e. DbTypes<TDialect>

				//Types types;
				//if (dialectDbTypes.TryGetValue(Name, out types))
				//{
				//    DbTypes.ColumnTypeMap = Clone(types.ColumnTypeMap);
				//    DbTypes.ColumnDbTypeMap = Clone(types.ColumnDbTypeMap);
				//}
				//else
				//{
				//    dialectDbTypes[Name] = new Types {
				//        ColumnTypeMap = Clone(DbTypes.ColumnTypeMap),
				//        ColumnDbTypeMap = Clone(DbTypes.ColumnDbTypeMap)
				//    };
				//}
				return dialect;
			}
		}

		//private class Types
		//{
		//    public Dictionary<Type, string> ColumnTypeMap = new Dictionary<Type, string>();
		//    public Dictionary<Type, DbType> ColumnDbTypeMap = new Dictionary<Type, DbType>();
		//}

		//private Dictionary<TKey, TValue> Clone<TKey, TValue>(Dictionary<TKey, TValue> original)
		//{
		//    var clone = new Dictionary<TKey, TValue>(original);
		//    return clone;
		//}

	}
}

