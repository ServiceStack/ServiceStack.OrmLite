using System;
using System.Data;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public enum OnFkOption
    {
        Cascade,
        SetNull,
        NoAction,
        SetDefault,
        Restrict
    }

    public static class OrmLiteDDLExtensions
    {
        public static void AlterTable<T>(this IDbConnection dbConn, string command)
        {
            AlterTable(dbConn, typeof(T), command);
        }

        public static void AlterTable(this IDbConnection dbConn, Type modelType, string command)
        {
            string sql = string.Format("ALTER TABLE {0} {1};",
                                       OrmLiteConfig.DialectProvider.GetQuotedTableName(modelType.GetModelDefinition()),
                                       command);
            dbConn.ExecuteSql(sql);
        }

        public static void AddColumn<T>(this IDbConnection dbConn,
                                        Expression<Func<T, object>> field)
        {
            var modelDef = ModelDefinition<T>.Definition;
            var fieldDef = modelDef.GetFieldDefinition<T>(field);
            dbConn.AddColumn(typeof(T), fieldDef);
        }


        public static void AddColumn(this IDbConnection dbConn, Type modelType, FieldDefinition fieldDef)
        {
            var command = OrmLiteConfig.DialectProvider.ToAddColumnStatement(modelType, fieldDef);
            dbConn.ExecuteSql(command);
        }


        public static void AlterColumn<T>(this IDbConnection dbConn, Expression<Func<T, object>> field)
        {
            var modelDef = ModelDefinition<T>.Definition;
            var fieldDef = modelDef.GetFieldDefinition<T>(field);
            dbConn.AlterColumn(typeof(T), fieldDef);
        }

        public static void AlterColumn(this IDbConnection dbConn, Type modelType, FieldDefinition fieldDef)
        {
            var command = OrmLiteConfig.DialectProvider.ToAlterColumnStatement(modelType, fieldDef);
            dbConn.ExecuteSql(command);
        }


        public static void ChangeColumnName<T>(this IDbConnection dbConn,
                                               Expression<Func<T, object>> field,
                                               string oldColumnName)
        {
            var modelDef = ModelDefinition<T>.Definition;
            var fieldDef = modelDef.GetFieldDefinition<T>(field);
            dbConn.ChangeColumnName(typeof(T), fieldDef, oldColumnName);
        }

        public static void ChangeColumnName(this IDbConnection dbConn,
                                            Type modelType,
                                            FieldDefinition fieldDef,
                                            string oldColumnName)
        {
            var command = OrmLiteConfig.DialectProvider.ToChangeColumnNameStatement(modelType, fieldDef, oldColumnName);
            dbConn.ExecuteSql(command);
        }

        public static void DropColumn<T>(this IDbConnection dbConn, string columnName)
        {
            dbConn.DropColumn(typeof(T), columnName);
        }


        public static void DropColumn(this IDbConnection dbConn, Type modelType, string columnName)
        {
            string command = string.Format("ALTER TABLE {0} DROP {1};",
                                           OrmLiteConfig.DialectProvider.GetQuotedTableName(modelType.GetModelDefinition().ModelName),
                                           OrmLiteConfig.DialectProvider.GetQuotedName(columnName));

            dbConn.ExecuteSql(command);
        }



        public static void AddForeignKey<T, TForeign>(this IDbConnection dbConn,
                                                     Expression<Func<T, object>> field,
                                                     Expression<Func<TForeign, object>> foreignField,
                                                     OnFkOption onUpdate,
                                                     OnFkOption onDelete,
                                                     string foreignKeyName = null)
        {
            string command = OrmLiteConfig.DialectProvider.ToAddForeignKeyStatement(field,
                                                                                    foreignField,
                                                                                    onUpdate,
                                                                                    onDelete,
                                                                                    foreignKeyName);
            dbConn.ExecuteSql(command);
        }


        public static void DropForeignKey<T>(this IDbConnection dbConn, string foreignKeyName)
        {
            string command = string.Format("ALTER TABLE {0} DROP FOREIGN KEY {1};",
                                           OrmLiteConfig.DialectProvider.GetQuotedTableName(ModelDefinition<T>.Definition.ModelName),
                                           OrmLiteConfig.DialectProvider.GetQuotedName(foreignKeyName));
            dbConn.ExecuteSql(command);
        }


        public static void CreateIndex<T>(this IDbConnection dbConn, Expression<Func<T, object>> field,
                                          string indexName = null, bool unique = false)
        {
            var command = OrmLiteConfig.DialectProvider.ToCreateIndexStatement(field, indexName, unique);
            dbConn.ExecuteSql(command);
        }


        public static void DropIndex<T>(this IDbConnection dbConn, string indexName)
        {
            string command = string.Format("ALTER TABLE {0} DROP INDEX  {1};",
                                           OrmLiteConfig.DialectProvider.GetQuotedTableName(ModelDefinition<T>.Definition.ModelName),
                                           OrmLiteConfig.DialectProvider.GetQuotedName(indexName));
            dbConn.ExecuteSql(command);
        }

    }
}
