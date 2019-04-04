using Borlay.Arrays;
using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class ArrayConverter : IConverter
    {
        public IConverterProvider ConverterProvider { get; private set; }

        public ArrayConverter(IConverterProvider converterProvider)
        {
            if (converterProvider == null)
                throw new ArgumentNullException(nameof(converterProvider));

            this.ConverterProvider = converterProvider;
        }

        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var elementType = obj.GetType().GetElementType();
            var converter = ConverterProvider.GetConverter(elementType, out var converterTypeId);

            bytes.AddBytes<short>(converterTypeId, 2, ref index);

            if(converter is IArrayConverter arrayConverter)
            {
                arrayConverter.AddArrayBytes(obj, bytes, ref index);
                return;
            }

            var arr = obj as Array;
            bytes.AddBytes<int>(arr.Length, 4, ref index);

            for (int i = 0; i < arr.Length; i++)
            {
                converter.AddBytes(arr.GetValue(i), bytes, ref index);
            }
        }

        public object GetObject(byte[] bytes, ref int index)
        {
            var converter = GetConverter(bytes, ref index);

            if (converter is IArrayConverter arrayConverter)
            {
                var obj = arrayConverter.GetArrayObject(bytes, ref index);
                return obj;
            }


            int length = bytes.GetValue<int>(4, ref index);

            var elementType = converter.GetType(bytes, index);
            var elArray = Array.CreateInstance(elementType, length);

            for (int i = 0; i < length; i++)
            {
                var elObj = converter.GetObject(bytes, ref index);
                elArray.SetValue(elObj, i);
            }

            return elArray;
        }

        public IConverter GetConverter(byte[] bytes, ref int index)
        {
            short converterTypeId = bytes.GetValue<short>(2, ref index);
            var converter = ConverterProvider.GetConverter(converterTypeId);
            return converter;
        }

        public Type GetType(byte[] bytes, int index)
        {
            var converter = GetConverter(bytes, ref index);
            var type = converter.GetType(bytes, index);
            return type.MakeArrayType();
        }
    }
}
