using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace ServiceStack.OrmLite
{
    public class OrmLiteContext
    {
        public static readonly OrmLiteContext Instance = new OrmLiteContext();

        /// <summary>
        /// Tell ServiceStack to use ThreadStatic Items Collection for Context Scoped items.
        /// Warning: ThreadStatic Items aren't pinned to the same request in async services which callback on different threads.
        /// </summary>
        public static bool UseThreadStatic = false;

        [ThreadStatic]
        public static IDictionary ContextItems;

        /// <summary>
        /// Gets a list of items for this context. 
        /// </summary>
        public virtual IDictionary Items
        {
            get
            {
                return GetItems() ?? (CreateItems());
            }
            set
            {
                CreateItems(value);
            }
        }

        private const string _key = "__OrmLite.Items";

        private IDictionary GetItems()
        {
            try
            {
                if (UseThreadStatic)
                    return ContextItems;

                return CallContext.LogicalGetData(_key) as IDictionary;
            }
            catch (NotImplementedException)
            {
                //Fixed in Mono master: https://github.com/mono/mono/pull/817
                return CallContext.GetData(_key) as IDictionary;
            }
        }

        private IDictionary CreateItems(IDictionary items = null)
        {
            try
            {
                if (UseThreadStatic)
                {
                    ContextItems = items ?? (items = new Dictionary<object, object>());
                }
                else
                {
                    CallContext.LogicalSetData(_key, items ?? (items = new ConcurrentDictionary<object, object>()));
                }
            }
            catch (NotImplementedException)
            {
                //Fixed in Mono master: https://github.com/mono/mono/pull/817
                CallContext.SetData(_key, items ?? (items = new ConcurrentDictionary<object, object>()));
            }
            return items;
        }

        public T GetOrCreate<T>(Func<T> createFn)
        {
            if (Items.Contains(typeof(T).Name))
                return (T)Items[typeof(T).Name];

            return (T)(Items[typeof(T).Name] = createFn());
        }

        internal static string LastCommandText
        {
            get
            {
                return Instance.Items["LastCommandText"] as string;
            }
            set
            {
                Instance.Items["LastCommandText"] = value ?? "";
            }
        }
    }
}