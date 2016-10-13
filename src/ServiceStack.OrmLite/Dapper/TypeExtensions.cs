using System;
using System.Reflection;
using System.Collections.Generic;

namespace ServiceStack.OrmLite.Dapper
{
    internal static class TypeExtensions
    {
        public static string Name(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().Name;
#else
            return type.Name;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }
        public static bool IsGenericType(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }
        public static bool IsInterface(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif
        }

        public static Type UnderlyingSystemType(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().AsType();
#else
            return type.UnderlyingSystemType;
#endif
        }
#if NETSTANDARD1_3
        public static IEnumerable<Attribute> GetCustomAttributes(this Type type, bool inherit)
        {
            return type.GetTypeInfo().GetCustomAttributes(inherit);
        }

        public static TypeCode GetTypeCode(Type type)
        {
            if (type == null) return TypeCode.Empty;
            TypeCode result;
            if (typeCodeLookup.TryGetValue(type, out result)) return result;

            if (type.IsEnum())
            {
                type = Enum.GetUnderlyingType(type);
                if (typeCodeLookup.TryGetValue(type, out result)) return result;
            }
            return TypeCode.Object;
        }

        public static Type GetTypeFromTypeCode(this TypeCode typeCode)
        {
            Type result;

            if (typeFromTypeCodeLookup.TryGetValue(typeCode, out result))
                return result;

            return typeof(Object);
        }
        static readonly Dictionary<Type, TypeCode> typeCodeLookup = new Dictionary<Type, TypeCode>
        {
            {typeof(bool), TypeCode.Boolean },
            {typeof(byte), TypeCode.Byte },
            {typeof(char), TypeCode.Char},
            {typeof(DateTime), TypeCode.DateTime},
            {typeof(decimal), TypeCode.Decimal},
            {typeof(double), TypeCode.Double },
            {typeof(short), TypeCode.Int16 },
            {typeof(int), TypeCode.Int32 },
            {typeof(long), TypeCode.Int64 },
            {typeof(object), TypeCode.Object},
            {typeof(sbyte), TypeCode.SByte },
            {typeof(float), TypeCode.Single },
            {typeof(string), TypeCode.String },
            {typeof(ushort), TypeCode.UInt16 },
            {typeof(uint), TypeCode.UInt32 },
            {typeof(ulong), TypeCode.UInt64 },
        };

        static readonly Dictionary<TypeCode, Type> typeFromTypeCodeLookup = new Dictionary<TypeCode, Type>
        {
            {TypeCode.Boolean, typeof(bool) },
            {TypeCode.Byte , typeof(byte) },
            {TypeCode.Char, typeof(char) },
            {TypeCode.DateTime, typeof(DateTime) },
            {TypeCode.Decimal, typeof(decimal) },
            {TypeCode.Double , typeof(double) },
            {TypeCode.Int16, typeof(short) },
            {TypeCode.Int32, typeof(int) },
            {TypeCode.Int64, typeof(long) },
            {TypeCode.Object, typeof(object) },
            {TypeCode.SByte, typeof(sbyte) },
            {TypeCode.Single, typeof(float) },
            {TypeCode.String, typeof(string) },
            {TypeCode.UInt16, typeof(ushort) },
            {TypeCode.UInt32, typeof(uint) },
            {TypeCode.UInt64, typeof(ulong) },
        };

#else
        public static TypeCode GetTypeCode(Type type)
        {
            return Type.GetTypeCode(type);
        }
#endif
        public static MethodInfo GetPublicInstanceMethod(this Type type, string name, Type[] types)
        {
#if NETSTANDARD1_3
            var method = type.GetMethod(name, types);
            return (method != null && method.IsPublic && !method.IsStatic) ? method : null;
#else
            return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, null, types, null);
#endif
        }


    }
}
