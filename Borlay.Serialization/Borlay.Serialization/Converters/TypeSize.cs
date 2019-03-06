using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public static class TypeSize
    {
        private static Dictionary<Type, byte> sizeCache = new Dictionary<Type, byte>();

        static TypeSize()
        {
            Register<bool>(1);
            Register<byte>(1);
            Register<sbyte>(1);
            Register<short>(2);
            Register<int>(4);
            Register<long>(8);
            Register<ushort>(2);
            Register<uint>(4);
            Register<ulong>(8);

            
            Register<float>(4);
            Register<double>(8);
            Register<decimal>(16);

            Register<char>(2);
        }

        public static void Register<T>(byte size)
        {
            sizeCache.Add(typeof(T), size);
        }

        public static bool TrySizeOf<T>(out byte size)
        {
            return TrySizeOf(typeof(T), out size);
        }

        public static bool TrySizeOf(Type type, out byte size)
        {
            if (type.IsArray)
                type = type.GetElementType();

            return sizeCache.TryGetValue(type, out size);
        }

        public static int SizeOf<T>()
        {
            return SizeOf(typeof(T));
        }

        public static int SizeOf(Type type)
        {
            if (type.IsArray)
                type = type.GetElementType();

            if (sizeCache.TryGetValue(type, out var value))
                return value;

            throw new KeyNotFoundException($"Type '{type}' in size cache not found");
        }
    }
}
