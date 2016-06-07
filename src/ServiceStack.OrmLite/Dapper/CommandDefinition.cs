#if NET45
#define ASYNC
#endif

using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

//Apache 2.0 License: https://github.com/StackExchange/dapper-dot-net/blob/master/License.txt
namespace ServiceStack.OrmLite.Dapper
{
    /// <summary>
    /// Represents the key aspects of a sql operation
    /// </summary>
    public struct CommandDefinition
    {
        internal static CommandDefinition ForCallback(object parameters)
        {
            if (parameters is DynamicParameters)
            {
                return new CommandDefinition(parameters);
            }
            else
            {
                return default(CommandDefinition);
            }
        }

        internal void OnCompleted()
        {
            var p = Parameters as SqlMapper.IParameterCallbacks;
            if (p != null) p.OnCompleted();
        }

        /// <summary>
        /// The command (sql or a stored-procedure name) to execute
        /// </summary>
        public string CommandText { get; private set; }

        /// <summary>
        /// The parameters associated with the command
        /// </summary>
        public object Parameters { get; private set; }

        /// <summary>
        /// The active transaction for the command
        /// </summary>
        public IDbTransaction Transaction { get; private set; }

        /// <summary>
        /// The effective timeout for the command
        /// </summary>
        public int? CommandTimeout { get; private set; }

        /// <summary>
        /// The type of command that the command-text represents
        /// </summary>
        public CommandType? CommandType { get; private set; }

        /// <summary>
        /// Should data be buffered before returning?
        /// </summary>
        public bool Buffered
        {
            get { return (Flags & CommandFlags.Buffered) != 0; }
        }

        /// <summary>
        /// Should the plan for this query be cached?
        /// </summary>
        internal bool AddToCache
        {
            get { return (Flags & CommandFlags.NoCache) == 0; }
        }

        /// <summary>
        /// Additional state flags against this command
        /// </summary>
        public CommandFlags Flags { get; set; }

        /// <summary>
        /// Can async queries be pipelined?
        /// </summary>
        public bool Pipelined
        {
            get { return (Flags & CommandFlags.Pipelined) != 0; }
        }

        /// <summary>
        /// Initialize the command definition
        /// </summary>
        public CommandDefinition(string commandText, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null,
                                 CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered
#if ASYNC
                                 , CancellationToken cancellationToken = default(CancellationToken)
#endif
            ) : this()
        {
            CommandText = commandText;
            Parameters = parameters;
            Transaction = transaction;
            CommandTimeout = commandTimeout;
            CommandType = commandType;
            Flags = flags;
#if ASYNC
            CancellationToken = cancellationToken;
#endif
        }

        private CommandDefinition(object parameters) : this()
        {
            Parameters = parameters;
        }

#if ASYNC

        /// <summary>
        /// For asynchronous operations, the cancellation-token
        /// </summary>
        public CancellationToken CancellationToken { get; }
#endif

        internal IDbCommand SetupCommand(IDbConnection cnn, Action<IDbCommand, object> paramReader)
        {
            var cmd = cnn.CreateCommand();
            var init = GetInit(cmd.GetType());
            if (init != null) init(cmd);
            if (Transaction != null)
                cmd.Transaction = Transaction;
            cmd.CommandText = CommandText;
            if (CommandTimeout.HasValue)
            {
                cmd.CommandTimeout = CommandTimeout.Value;
            }
            else if (SqlMapper.Settings.CommandTimeout.HasValue)
            {
                cmd.CommandTimeout = SqlMapper.Settings.CommandTimeout.Value;
            }
            if (CommandType.HasValue)
                cmd.CommandType = CommandType.Value;
            if (paramReader != null) paramReader(cmd, Parameters);
            return cmd;
        }

        private static SqlMapper.Link<Type, Action<IDbCommand>> commandInitCache;

        private static Action<IDbCommand> GetInit(Type commandType)
        {
            if (commandType == null)
                return null; // GIGO
            Action<IDbCommand> action;
            if (SqlMapper.Link<Type, Action<IDbCommand>>.TryGet(commandInitCache, commandType, out action))
            {
                return action;
            }
            var bindByName = GetBasicPropertySetter(commandType, "BindByName", typeof(bool));
            var initialLongFetchSize = GetBasicPropertySetter(commandType, "InitialLONGFetchSize", typeof(int));

            action = null;
            if (bindByName != null || initialLongFetchSize != null)
            {
                var method = new DynamicMethod(commandType.Name + "_init", null, new Type[] { typeof(IDbCommand) });
                var il = method.GetILGenerator();

                if (bindByName != null)
                {
                    // .BindByName = true
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, commandType);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.EmitCall(OpCodes.Callvirt, bindByName, null);
                }
                if (initialLongFetchSize != null)
                {
                    // .InitialLONGFetchSize = -1
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, commandType);
                    il.Emit(OpCodes.Ldc_I4_M1);
                    il.EmitCall(OpCodes.Callvirt, initialLongFetchSize, null);
                }
                il.Emit(OpCodes.Ret);
                action = (Action<IDbCommand>)method.CreateDelegate(typeof(Action<IDbCommand>));
            }
            // cache it
            SqlMapper.Link<Type, Action<IDbCommand>>.TryAdd(ref commandInitCache, commandType, ref action);
            return action;
        }

        private static MethodInfo GetBasicPropertySetter(Type declaringType, string name, Type expectedType)
        {
            var prop = declaringType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite && prop.PropertyType == expectedType && prop.GetIndexParameters().Length == 0)
            {
                return prop.GetSetMethod();
            }
            return null;
        }
    }

}
