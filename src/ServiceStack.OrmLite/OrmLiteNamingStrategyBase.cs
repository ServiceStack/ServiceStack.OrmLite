//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//   Tomasz Kubacki (tomasz.kubacki@gmail.com)
//
// Copyright 2012 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

namespace ServiceStack.OrmLite
{
	public class OrmLiteNamingStrategyBase : INamingStrategy
	{
		public virtual string GetTableName(string name)
		{
			return name;
		}
 
		public virtual string GetColumnName(string name)
		{
			return name;
		}
	}
}
