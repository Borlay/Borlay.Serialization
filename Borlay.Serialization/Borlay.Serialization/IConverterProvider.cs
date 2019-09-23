using Borlay.Arrays;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Borlay.Serialization
{
    public interface IConverterProvider
    {
        IConverter GetConverter<T>(out short typeId);
        IConverter GetConverter(Type type, out short typeId);
        IConverter GetConverter(short typeId);

        void AddConverter<T>(IConverter converter, short typeId);
        void AddConverter(IConverter converter, Type type, short typeId);

        void Clear();
    }

    public class ConverterProvider : IConverterProvider
    {
        protected Dictionary<Type, short> converterTypes = new Dictionary<Type, short>(); // all together, data types and converter types
        protected Dictionary<short, IConverter> converters = new Dictionary<short, IConverter>();

        public virtual void AddConverter<T>(IConverter converter, short typeId)
        {
            AddConverter(converter, typeof(T), typeId);
        }

        public virtual void AddConverter(IConverter converter, Type type, short typeId)
        {
            converterTypes[type] = typeId;
            converters[typeId] = converter;
        }

        public virtual IConverter GetConverter<T>(out short typeId)
        {
            return GetConverter(typeof(T), out typeId);
        }

        public virtual Type ToConverterType(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            Type nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                return nullableType;

            if (type.IsArray)
                return typeof(Array); //type.GetElementType();

            if (typeInfo.IsEnum)
                return typeof(Enum);

            if (typeInfo.IsClass)
                return typeof(object);

            if (typeInfo.IsValueType && !typeInfo.IsPrimitive)
                return typeof(object);

            throw new Exception($"Type '{type.FullName}' doesn't have converter type");
        }

        public virtual IConverter GetConverter(Type type, out short typeId)
        {
            do
            {
                if (converterTypes.TryGetValue(type, out typeId))
                    return GetConverter(typeId);

                type = ToConverterType(type);
            } while (true);
        }

        public virtual IConverter GetConverter(short typeId)
        {
            if (converters.TryGetValue(typeId, out var converter))
                return converter;

            throw new KeyNotFoundException($"Converter for type '{typeId}' not found");
        }

        public virtual void Clear()
        {
            converterTypes.Clear();
            converters.Clear();
        }
    }
}
