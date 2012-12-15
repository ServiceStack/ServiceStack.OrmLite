using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ServiceStack.OrmLite
{
    public class JoinSqlBuilder<TNewPoco, TBasePoco>
    {
        private List<Join> joinList = new List<Join>();
        private List<KeyValuePair<string, WhereType>> whereList = new List<KeyValuePair<string, WhereType>>();
        private List<string> columnList = new List<string>();
        private List<string> orderByList = new List<string>();
        private List<string> orderByDescList = new List<string>();

        private string baseTableName = "";

        public JoinSqlBuilder()
        {
            baseTableName = typeof(TBasePoco).GetModelDefinition().ModelName;
        }

        private string Column<T>(string tableName, Expression<Func<T, object>> func)
        {
            var lst = ColumnList(tableName,func);
            if (lst == null || lst.Count != 1)
                throw new Exception("Expression should have only one column");
            return lst[0];
        }

        private List<string> ColumnList<T>(string tableName, Expression<Func<T, object>> func)
        {
            List<string> result = new List<string>();
            if (func == null || func.Body == null)
                return result;
            PropertyList(tableName,func.Body, result);
            return result;
        }

        private void ProcessUnary(string tableName,UnaryExpression u, List<string> lst)
        {
            if (u.NodeType == ExpressionType.Convert)
            {
                if (u.Method != null)
                {
                    throw new Exception("Invalid Expression provided");
                }
                PropertyList(tableName,u.Operand, lst);
                return;
            }
            throw new Exception("Invalid Expression provided");
        }

        protected void ProcessMemberAccess(string tableName, MemberExpression m, List<string> lst)
        {
            if (m.Expression != null
                && (m.Expression.NodeType == ExpressionType.Parameter || m.Expression.NodeType == ExpressionType.Convert))
            {
                lst.Add(string.Format("{0}.{1}", OrmLiteConfig.DialectProvider.GetQuotedTableName(tableName), OrmLiteConfig.DialectProvider.GetQuotedColumnName(m.Member.Name)));
                return;
            }
            throw new Exception("Only Members are allowed");
        }

        private void ProcessNew(string tableName, NewExpression nex, List<string> lst)
        {
            if (nex.Arguments == null || nex.Arguments.Count == 0)
                throw new Exception("Only column list allowed");
            foreach (var arg in nex.Arguments)
                PropertyList(tableName,arg, lst);
            return;
        }

        private void PropertyList(string tableName, Expression exp, List<string> lst)
        {
            if (exp == null)
                return;

            switch (exp.NodeType)
            {
                case ExpressionType.MemberAccess:
                    ProcessMemberAccess(tableName,exp as MemberExpression, lst);
                    return;

                case ExpressionType.Convert:
                    var ue = exp as UnaryExpression;
                    ProcessUnary(tableName,ue, lst);
                    return;

                case ExpressionType.New:
                    ProcessNew(tableName,exp as NewExpression, lst);
                    return;
            }
            throw new Exception("Only columns are allowed");
        }

        public JoinSqlBuilder<TNewPoco, TBasePoco> Select<T>(Expression<Func<T, object>> selectColumns)
        {
            Type associatedType = this.PreviousAssociatedType(typeof(T), typeof(T));
            if (associatedType == null)
            {
                throw new Exception("Either the source or destination table should be associated ");
            }

            this.columnList.AddRange(ColumnList(associatedType.GetModelDefinition().ModelName, selectColumns));
            return this;
        }

        public JoinSqlBuilder<TNewPoco, TBasePoco> Where<T>(Expression<Func<T, bool>> where)
        {
            return WhereInternal(WhereType.AND, where);
        }
        
        public JoinSqlBuilder<TNewPoco, TBasePoco> Or<T>(Expression<Func<T, bool>> where)
        {
            return WhereInternal(WhereType.OR, where);
        }
        
        public JoinSqlBuilder<TNewPoco, TBasePoco> And<T>(Expression<Func<T, bool>> where)
        {
            return WhereInternal(WhereType.AND, where);
        }

        private JoinSqlBuilder<TNewPoco, TBasePoco> WhereInternal<T>(WhereType whereType,Expression<Func<T, bool>> where)
        {
            Type associatedType = this.PreviousAssociatedType(typeof(T), typeof(T));
            if (associatedType == null)
            {
                throw new Exception("Either the source or destination table should be associated ");            
            }
            var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
            ev.WhereStatementWithoutWhereString = true;
            ev.PrefixFieldWithTableName = true;
            ev.Where(where);
            var str = ev.WhereExpression;
            if (String.IsNullOrEmpty(str) == false)
            {
                this.whereList.Add(new KeyValuePair<string,WhereType>(str,whereType));
            }
            return this;
        }

        private JoinSqlBuilder<TNewPoco, TBasePoco> OrderByInternal<T>(bool byDesc, Expression<Func<T, object>> orderByColumns)
        {
            Type associatedType = this.PreviousAssociatedType(typeof(T), typeof(T));
            if (associatedType == null)
            {
                throw new Exception("Either the source or destination table should be associated ");
            }

            if (byDesc)
                this.orderByDescList.AddRange(ColumnList(associatedType.GetModelDefinition().ModelName, orderByColumns));
            else
                this.orderByList.AddRange(ColumnList(associatedType.GetModelDefinition().ModelName, orderByColumns));
            return this;
        }

        public JoinSqlBuilder<TNewPoco, TBasePoco> Clear()
        {
            joinList.Clear();
            whereList.Clear();
            columnList.Clear();
            orderByList.Clear();
            orderByDescList.Clear();
            return this;
        }


        public JoinSqlBuilder<TNewPoco, TBasePoco> OrderBy<T>(Expression<Func<T, object>> sourceColumn)
        {
            return OrderByInternal<T>(false, sourceColumn);        
        }

        public JoinSqlBuilder<TNewPoco, TBasePoco> OrderByDescending<T>(Expression<Func<T, object>> sourceColumn)
        {
            return OrderByInternal<T>(true, sourceColumn);
        }

        public JoinSqlBuilder<TNewPoco, TBasePoco> InnerJoin<TSourceTable, TDestinationTable>(Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TDestinationTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection = null, Expression<Func<TDestinationTable, object>> destinationTableColumnSelection = null, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TDestinationTable, bool>> destinationWhere = null)
        {
            return JoinInternal<Join, TSourceTable, TDestinationTable>(JoinType.INNER,joinList, sourceColumn, destinationColumn, sourceTableColumnSelection, destinationTableColumnSelection, sourceWhere, destinationWhere);
        }

        public JoinSqlBuilder<TNewPoco, TBasePoco> LeftOuterJoin<TSourceTable, TDestinationTable>(Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TDestinationTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection = null, Expression<Func<TDestinationTable, object>> destinationTableColumnSelection = null, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TDestinationTable, bool>> destinationWhere = null)
        {
            return JoinInternal<Join, TSourceTable, TDestinationTable>(JoinType.LEFTOUTER,joinList, sourceColumn, destinationColumn, sourceTableColumnSelection, destinationTableColumnSelection, sourceWhere, destinationWhere);
        }

        public JoinSqlBuilder<TNewPoco, TBasePoco> RightOuterJoin<TSourceTable, TDestinationTable>(Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TDestinationTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection = null, Expression<Func<TDestinationTable, object>> destinationTableColumnSelection = null, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TDestinationTable, bool>> destinationWhere = null)
        {
            return JoinInternal<Join, TSourceTable, TDestinationTable>(JoinType.RIGHTOUTER, joinList, sourceColumn, destinationColumn, sourceTableColumnSelection, destinationTableColumnSelection, sourceWhere, destinationWhere);
        }

        public JoinSqlBuilder<TNewPoco, TBasePoco> FullOuterJoin<TSourceTable, TDestinationTable>(Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TDestinationTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection = null, Expression<Func<TDestinationTable, object>> destinationTableColumnSelection = null, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TDestinationTable, bool>> destinationWhere = null)
        {
            return JoinInternal<Join, TSourceTable, TDestinationTable>(JoinType.FULLOUTER, joinList, sourceColumn, destinationColumn, sourceTableColumnSelection, destinationTableColumnSelection, sourceWhere, destinationWhere);
        }

        public JoinSqlBuilder<TNewPoco, TBasePoco> SelfJoin<TSourceTable>(Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TSourceTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection = null, Expression<Func<TSourceTable, object>> destinationTableColumnSelection = null, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TSourceTable, bool>> destinationWhere = null)
        {
            return JoinInternal<Join, TSourceTable, TSourceTable>(JoinType.INNER, joinList, sourceColumn, destinationColumn, sourceTableColumnSelection, destinationTableColumnSelection, sourceWhere, destinationWhere);
        }

        private JoinSqlBuilder<TNewPoco, TBasePoco> JoinInternal<TJoin, TSourceTable, TDestinationTable>(JoinType joinType, List<TJoin> joinObjList, Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TDestinationTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection, Expression<Func<TDestinationTable, object>> destinationTableColumnSelection, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TDestinationTable, bool>> destinationWhere = null) where TJoin : Join, new()
        {
            Type associatedType = this.PreviousAssociatedType(typeof(TSourceTable), typeof(TDestinationTable));
            if (associatedType == null)
            {
                throw new Exception("Either the source or destination table should be associated ");
            }

            TJoin join = new TJoin();
            join.JoinType = joinType;
            join.Class1Type = typeof(TSourceTable);
            join.Class2Type = typeof(TDestinationTable);
           
            if (associatedType == join.Class1Type)
                join.RefType = join.Class2Type;
            else
                join.RefType = join.Class1Type;

            join.Class1TableName = join.Class1Type.GetModelDefinition().ModelName;
            join.Class2TableName = join.Class2Type.GetModelDefinition().ModelName;
            join.RefTypeTableName = join.RefType.GetModelDefinition().ModelName;
            join.Class1ColumnName = Column<TSourceTable>(join.Class1TableName,sourceColumn);
            join.Class2ColumnName = Column<TDestinationTable>(join.Class2TableName,destinationColumn);

            if (sourceTableColumnSelection != null)
            {
                columnList.AddRange(ColumnList<TSourceTable>(join.Class1TableName,sourceTableColumnSelection));
            }

            if (destinationTableColumnSelection != null)
            {
                columnList.AddRange(ColumnList<TDestinationTable>(join.Class2TableName,destinationTableColumnSelection));
            }

            if (sourceWhere != null)
            {
                var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<TSourceTable>();
                ev.Where(sourceWhere);
                var where = ev.WhereExpression;
                if (!String.IsNullOrEmpty(where))
                    whereList.Add(new KeyValuePair<string, WhereType>(where,WhereType.AND));
            }

            if (destinationWhere != null)
            {
                var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<TDestinationTable>();
                ev.Where(destinationWhere);
                var where = ev.WhereExpression;
                if (!String.IsNullOrEmpty(where))
                    whereList.Add(new KeyValuePair<string, WhereType>(where, WhereType.AND));
            }
            
            joinObjList.Add(join);
            return this;
        }

        private Type PreviousAssociatedType(Type sourceTableType, Type destinationTableType)
        {
            if (sourceTableType == typeof(TBasePoco) || destinationTableType == typeof(TBasePoco))
            {
                return typeof(TBasePoco);
            }

            foreach (var j in joinList)
            {
                if (j.Class1Type == sourceTableType || j.Class2Type == sourceTableType)
                {
                    return sourceTableType;
                }
                if (j.Class1Type == destinationTableType || j.Class2Type == destinationTableType)
                {
                    return destinationTableType;
                }
            }
            return null;
        }

        public string ToSql()
        {
            var sb = new StringBuilder();
            sb.Append("SELECT ");

            var colSB = new StringBuilder();

            if (columnList.Count > 0)
            {
                foreach (var col in columnList)
                {
                    colSB.AppendFormat("{0}{1}", colSB.Length > 0 ? "," : "", col);
                }
            }
            else
            {
                foreach ( var fi in typeof(TNewPoco).GetModelDefinition().FieldDefinitions)
                {
                    colSB.AppendFormat("{0}{1}", colSB.Length > 0 ? "," : "", String.IsNullOrEmpty(fi.BelongToModelName) ? (fi.FieldName) : ((OrmLiteConfig.DialectProvider.GetQuotedTableName(fi.BelongToModelName) + "." + OrmLiteConfig.DialectProvider.GetQuotedColumnName(fi.FieldName))));
                }
                if (colSB.Length == 0)
                    colSB.AppendFormat("\"{0}\".*", baseTableName);
            }

            sb.Append(colSB.ToString() + " \n");

            sb.AppendFormat("FROM {0} \n", baseTableName);

            foreach (var join in joinList)
            {
                if (join.JoinType == JoinType.INNER)
                    sb.Append(" INNER JOIN ");
                else if (join.JoinType == JoinType.LEFTOUTER)
                    sb.Append(" LEFT OUTER JOIN ");
                else if (join.JoinType == JoinType.RIGHTOUTER)
                    sb.Append(" RIGHT OUTER JOIN ");
                else if (join.JoinType == JoinType.FULLOUTER)
                    sb.Append(" FULL OUTER JOIN ");

                sb.AppendFormat(" {0} ON {1} = {2}  \n", join.RefTypeTableName, join.Class1ColumnName, join.Class2ColumnName);
            }

            if (whereList.Count > 0)
            {
                var whereSB = new StringBuilder();
                foreach (var where in whereList)
                {
                    whereSB.AppendFormat("{0}{1}", whereSB.Length > 0 ? (where.Value == WhereType.OR ? " OR " : " AND ") : "", where.Key);
                }
                sb.Append("WHERE " + whereSB.ToString() + " \n");
            }

            if (orderByList.Count > 0 || orderByDescList.Count > 0 )
            {
                var orderBySB = new StringBuilder();
                foreach (var ob in orderByList)
                {
                    orderBySB.AppendFormat("{0}{1} ASC ", orderBySB.Length > 0 ? "," : "", ob);
                }
                foreach (var obd in orderByDescList)
                {
                    orderBySB.AppendFormat("{0}{1} DESC ", orderBySB.Length > 0 ? "," : "", obd);
                }
                sb.Append("ORDER BY " + orderBySB.ToString()+" \n");
            }

            return sb.ToString();
        }
    }

    enum WhereType
    {
        AND,
        OR
    }


    enum JoinType
    {
        INNER,
        LEFTOUTER,
        RIGHTOUTER,
        FULLOUTER
    }

    class Join
    {
        public Type Class1Type {get;set; }
        public Type Class2Type {get;set; }
        public Type RefType {get;set; }
        public JoinType JoinType { get; set; }
        public string Class1TableName { get; set; }
        public string Class2TableName { get; set; }
        public string RefTypeTableName { get; set; }
        public string Class1ColumnName {get;set; }
        public string Class2ColumnName {get;set; }        
    }

}