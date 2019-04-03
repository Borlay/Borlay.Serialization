using Borlay.Serialization.Converters;
using Borlay.Serialization.Notations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public static class ConverterExtensions
    {
        public static byte[] ToBytes(this IConverter converter, object obj, ushort bufferSize = 1024)
        {
            var buffer = new byte[bufferSize];
            var index = 0;
            converter.AddBytes(obj, buffer, ref index);
            var bytes = new byte[index];
            Array.Copy(buffer, bytes, index);
            return bytes;
        }

        public static object GetValue(this IConverter converter, byte[] bytes, ref int index)
        {
            return converter.GetObject(bytes, ref index);
        }


        public static ConverterContext CreateContext(this IConverterProvider converterProvider, Type type, out short typeId)
        {
            var dataAttribute = type.GetTypeInfo().GetCustomAttribute<DataAttribute>(false);
            if (dataAttribute.TypeId >= 30000 && !dataAttribute.IsSystem)
                throw new ArgumentException($"Data types from 30000 must have set IsSystem to true. Current: {dataAttribute.TypeId}. Type: {type.FullName}");

            var properties = type.GetTypeInfo().GetProperties()
                .Where(p => p.GetCustomAttribute<IncludeAttribute>(true) != null).Select(property =>
                new PropertyContext()
                {
                    Include = property.GetCustomAttribute<IncludeAttribute>(true),
                    Array = property.GetCustomAttribute<ArrayAttribute>(true),
                    PropertyInfo = property,
                    Converter = converterProvider.GetConverter(property.PropertyType, out var converterTypeId),
                }).OrderBy(p => p.Include.Order).ToArray();

            if (dataAttribute == null)
                throw new ArgumentException($"Type '{type.Name}' should contain DataAttribute");

            //if (properties.Length == 0) // todo patikrinti ar tikrai (EmptyResponse)
            //   throw new ArgumentException($"Type '{type.Name}' should contain properties with IncludeAttribute");

            var converterContext = new ConverterContext()
            {
                Type = type,
                TypeId = dataAttribute.TypeId,
                Data = dataAttribute,
                Properties = properties,
            };

            typeId = dataAttribute.TypeId;
            return converterContext;
        }
    }
}
