using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace ServiceStack.OrmLite.Oracle
{
    public class OracleExecFilter : OrmLiteExecFilter
    {
        public override IDbCommand CreateCommand(IDbConnection dbConn)
        {
            var command = base.CreateCommand(dbConn);

            var action = GetBindByNameSetter(command.GetType());
            if (action != null) action(command, true);

            return command;
        }

        private static readonly Dictionary<Type, Action<IDbCommand, bool>> Cache = new Dictionary<Type, Action<IDbCommand, bool>>();
        private static Action<IDbCommand, bool> GetBindByNameSetter(Type commandType)
        {
            if (commandType == null) return null;

            Action<IDbCommand, bool> action;
            if (Cache.TryGetValue(commandType, out action)) return action;

            var prop = commandType.GetProperty("BindByName", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo setter;
            if (prop != null && prop.CanWrite && prop.PropertyType == typeof(bool)
                && prop.GetIndexParameters().Length == 0 && (setter = prop.GetSetMethod()) != null)
            {
                var methodName = commandType.GetOperationName() + "_BindByName";
                var method = new DynamicMethod(methodName, null, new []{ typeof(IDbCommand), typeof(bool) });
                var il = method.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, commandType);
                il.Emit(OpCodes.Ldarg_1);
                il.EmitCall(OpCodes.Callvirt, setter, null);
                il.Emit(OpCodes.Ret);
                action = (Action<IDbCommand, bool>)method.CreateDelegate(typeof(Action<IDbCommand, bool>));
            }
            Cache.Add(commandType, action);
            return action;
        }
    }
}
