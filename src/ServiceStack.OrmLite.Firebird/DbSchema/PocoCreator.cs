using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ServiceStack.OrmLite.Firebird.DbSchema
{
	public class PocoCreator<TTable, TColumn, TProcedure, TParameter>
		where TTable : ITable, new()
		where TColumn : IColumn, new()
		where TProcedure : IProcedure, new()
		where TParameter : IParameter, new(){
		
		private bool hasId=false;
		private string idType= string.Empty;
		
		public PocoCreator()
		{

			OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "src");
			Usings ="using System;\n" +
					"using System.ComponentModel.DataAnnotations;\n" +
					"using ServiceStack.Common;\n" +
					"using ServiceStack.DataAnnotations;\n" +
					"using ServiceStack.DesignPatterns.Model;\n"+
					"using ServiceStack.OrmLite;\n";

			SpaceName = "Database.Records";
			MetadataClassName="Me";
			IdField="Id";
		}

		public bool GenerateMetadata{get;set;}
		
		public string MetadataClassName{get; set;}
		
		public string IdField{ get; set;}
		
		public string SpaceName
		{
			get;
			set;
		}

		public string Usings
		{
			get;
			set;
		}

		public string OutputDirectory
		{
			get;
			set;
		}

		public ISchema<TTable, TColumn, TProcedure, TParameter> Schema
		{
			get;
			set;
		}

		public void WriteClass(TTable table)
		{
			WriteClass(table, table.Name);
		}

		public void WriteClass(TTable table, string className)
		{

			StringBuilder properties= new StringBuilder();
			StringBuilder meProperties= new StringBuilder();
			List<TColumn> columns = Schema.GetColumns(table.Name);

			bool hasIdField = columns.Count(r => r.Name.ToUpper() == OrmLiteDialectProviderBase.IdField.ToUpper()) == 1;

			foreach (var cl in columns)
			{
				properties.AppendFormat("\t\t[Alias(\"{0}\")]\n", cl.Name);
				if (!string.IsNullOrEmpty(cl.Sequence)) properties.AppendFormat("\t\t[Sequence(\"{0}\")]\n", cl.Sequence);
				if (cl.IsPrimaryKey) properties.Append("\t\t[PrimaryKey]\n");
				if (cl.AutoIncrement) properties.Append("\t\t[AutoIncrement]\n");
				if (!cl.Nullable && TypeToString(cl.NetType)=="System.String") properties.Append("\t\t[Required]\n");
				if (cl.IsComputed) properties.Append("\t\t[Compute]\n");
				properties.AppendFormat("\t\tpublic {0}{1} {2} {{ get; set;}} \n\n",
										TypeToString(cl.NetType),
										(cl.Nullable && cl.NetType != typeof(string)) ? "?" : "",
										(cl.AutoIncrement && cl.IsPrimaryKey && !hasIdField) ?
											OrmLiteDialectProviderBase.IdField
											: ToDotName(cl.Name));
				
				if(GenerateMetadata){
					if(meProperties.Length==0)
						meProperties.AppendFormat("\n\t\t\tpublic static string TableName {{ get {{ return \"{0}\"; }}}}",
						                          table.Name);
					meProperties.AppendFormat("\n\t\t\tpublic static string {0} {{ get {{ return \"{1}\"; }}}}",
					                         ToDotName(cl.Name),cl.Name);
				}
				
				if(ToDotName(cl.Name)==IdField){
					hasId=true;
					idType = TypeToString(cl.NetType);
				}
			}
				    
			if (!Directory.Exists(OutputDirectory))
				Directory.CreateDirectory(OutputDirectory);
			
			className = ToDotName(className);

			using (TextWriter tw = new StreamWriter(Path.Combine(OutputDirectory, className + ".cs")))
			{

				StringBuilder ns = new StringBuilder();
				StringBuilder cl =  new StringBuilder();
				StringBuilder me = new StringBuilder();
				cl.AppendFormat("\t[Alias(\"{0}\")]\n", table.Name);
				if(GenerateMetadata){
					me.AppendFormat("\n\t\tpublic static class {0} {{\n\t\t\t{1}\n\n\t\t}}\n",
					                MetadataClassName, meProperties.ToString());
					
				}
				cl.AppendFormat("\tpublic partial class {0}{1}{{\n\n\t\tpublic {0}(){{}}\n\n{2}{3}\t}}",
								className, 
				                hasId?string.Format( ":IHasId<{0}>",idType):"", 
				                properties.ToString(),
				                me.ToString());

				
				
				ns.AppendFormat("namespace {0}\n{{\n{1}\n}}", SpaceName, cl.ToString());
				tw.WriteLine(Usings);
				tw.WriteLine(ns.ToString());
				
				
				
				tw.Close();
			}
		}

		public void WriteClass(TProcedure procedure)
		{
			WriteClass(procedure, procedure.Name);
		}

		public void WriteClass(TProcedure procedure, string className)
		{

		}

		private string ToDotName(string name)
		{

			StringBuilder t = new StringBuilder();
			string [] parts = name.Split('_');
			foreach (var s in parts)
			{
				t.Append(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower()));
			}
			return t.ToString();
		}


		private string TypeToString(Type type)
		{
			string st = type.ToString();
			return (!st.Contains("[")) ? st : st.Substring(st.IndexOf("[") + 1, st.IndexOf("]") - st.IndexOf("[") - 1);


		}

	}
}

