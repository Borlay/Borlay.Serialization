using Borlay.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class DataConverter : IConverter
    {
        public IContextProvider ContextProvider { get; private set; }

        public byte Version { get; } = 1;

        public DataConverter(IContextProvider contextProvider)
        {
            if (contextProvider == null)
                throw new ArgumentNullException(nameof(contextProvider));

            this.ContextProvider = contextProvider;
        }

        public void AddBytes(object obj, byte[] bytes, ref int index)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var context = ContextProvider.GetContext(obj.GetType());
            var fieldProperties = context.Properties;

            var startIndex = index;
            index += 4; // rezervuojam ilgiui;

            bytes[index++] = this.Version;
            //bytes.AddBytes<short>(1, 2, ref index); // data converter versija
            bytes.AddBytes<int>(context.SpaceId, 4, ref index); // space type
            bytes.AddBytes<short>(context.TypeId, 2, ref index); // data type

            for (int p = 0; p < fieldProperties.Length; p++)
            {
                var currentIndex = index;

                var property = fieldProperties[p];

                if (property.Array != null && false)
                {
                    var objValue = property.PropertyInfo.GetValue(obj);
                    if (objValue == null)
                    {
                        if (property.Array.MinLength > 0 || property.Include.IsRequired)
                            throw new Exception($"Property '{property.PropertyInfo.Name}' min length is '{property.Array.MinLength}'");

                        continue;
                    }

                    Array value = null;

                    if (objValue is ByteArray)
                        value = ((ByteArray)objValue).Bytes;
                    else if (property.PropertyInfo.PropertyType.IsArray)
                        value = (Array)objValue;
                    else
                        throw new Exception($"Property '{property.PropertyInfo.Name}' shoul by array");


                    if (value.Length > short.MaxValue || value.Length < ushort.MinValue)
                        throw new Exception($"Property '{property.PropertyInfo.Name}' length should be between '{ushort.MinValue}' and '{short.MaxValue}' but  is '{value.Length}'");

                    short length = (short)value.Length;
                    short minLength = property.Array.MinLength;
                    short maxLength = property.Array.MaxLength;
                    bool onlyMinOrMax = property.Array.OnlyMinOrMax;

                    if (length < 0)
                        throw new Exception($"Property '{property.PropertyInfo.Name}' length should be greater than 0");

                    if ((bytes.Length - length) < index) throw new IndexOutOfRangeException("Byte array length is less than needed");

                    if (length < minLength)
                        throw new Exception($"Property '{property.PropertyInfo.Name}' min length is '{minLength}' but is '{length}'");
                    if (length > maxLength)
                        throw new Exception($"Property '{property.PropertyInfo.Name}' max length is '{maxLength}' but is '{length}'");
                    if (onlyMinOrMax)
                    {
                        if (length != minLength && length != maxLength)
                            throw new Exception($"Property '{property.PropertyInfo.Name}' should be '{minLength}' or '{maxLength}' length but is '{length}'");
                    }

                    if (length == 0)
                    {
                        if (property.Include.IsRequired)
                            throw new Exception($"Property '{property.PropertyInfo.Name}' is required'");

                        continue;
                    }

                    bytes[index++] = property.Include.Order;
                    bytes.AddBytes<short>(length, 2, ref index);

                    if (value is byte[] || value is bool[])
                    {
                        Buffer.BlockCopy(value, 0, bytes, index, value.Length);
                        index += value.Length;
                        continue;
                    }

                    var converter = property.Converter;
                    if (converter == null)
                        throw new KeyNotFoundException($"Converter for property '{property.PropertyInfo.Name}' not found");

                    for (int i = 0; i < value.Length; i++)
                    {
                        converter.AddBytes(value.GetValue(i), bytes, ref index);
                    }
                    continue;
                }
                else
                {
                    var objValue = property.PropertyInfo.GetValue(obj);
                    if (objValue == null)
                    {
                        if (property.Include.IsRequired)
                            throw new ArgumentException($"Property '{property.PropertyInfo.Name}' is required but is null");

                        continue;
                    }

                    var converter = property.Converter;
                    if (converter == null)
                        throw new KeyNotFoundException($"Converter for property '{property.PropertyInfo.Name}' not found");

                    bytes[index++] = property.Include.Order;
                    converter.AddBytes(objValue, bytes, ref index);

                    continue;
                }

                if (index <= currentIndex) // at least one byte shoud be added per property
                    throw new Exception("Something went wrong");
            }

            var count = index - (startIndex + 4);
            bytes.AddBytes<int>(count, 4, ref startIndex);
        }



        public object GetObject(byte[] bytes, ref int index)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                throw new ArgumentException(nameof(bytes.Length));

            int count = bytes.GetValue<int>(4, ref index);
            var startIndex = index;

            byte version = bytes[index++]; //bytes.GetValue<short>(2, ref index); // data converter versija

            if (version != this.Version)
                throw new VersionMismatchException($"Should be {this.Version} but was {version}");

            int spaceId = bytes.GetValue<int>(4, ref index);
            short typeId = bytes.GetValue<short>(2, ref index);

            var context = ContextProvider.GetContext((spaceId << 4) + typeId);
            var fieldProperties = context.Properties;

            var obj = Activator.CreateInstance(context.Type);

            for (int p = 0; p < fieldProperties.Length; p++)
            {
                if ((index - startIndex) > count)
                    throw new Exception("Something went wrong");

                var property = fieldProperties[p];

                var order = -1;
                if ((index - startIndex) < count)
                    order = bytes[index];

                if(property.Include.Order != order || ((index - startIndex) == count))
                {
                    if (property.Include.IsRequired)
                        throw new ArgumentException($"Property '{property.PropertyInfo.Name}' is required but was null");
                    continue;
                }

                index += 1;

                if (property.Array != null && false)
                {
                    short length = bytes.GetValue<short>(2, ref index);
                    short minLength = property.Array.MinLength;
                    short maxLength = property.Array.MaxLength;
                    bool onlyMinOrMax = property.Array.OnlyMinOrMax;

                    if ((bytes.Length - length) < index) throw new IndexOutOfRangeException("Byte array length is less than needed");

                    if (length < minLength)
                        throw new ArgumentException($"Property '{property.PropertyInfo.Name}' min length is '{minLength}' but is '{length}'");
                    if (length > maxLength)
                        throw new ArgumentException($"Property '{property.PropertyInfo.Name}' max length is '{maxLength}' but is '{length}'");
                    if (onlyMinOrMax)
                    {
                        if (length != minLength && length != maxLength)
                            throw new ArgumentException($"Property '{property.PropertyInfo.Name}' should be '{minLength}' or '{maxLength}' length but is '{length}'");
                    }

                    if (length <= 0)
                    {
                        if (property.Include.IsRequired)
                            throw new ArgumentException($"Property '{property.PropertyInfo.Name}' is required'");

                        continue;
                    }

                    if (property.PropertyInfo.PropertyType.Equals(typeof(byte[]))
                        || property.PropertyInfo.PropertyType.Equals(typeof(ByteArray)))
                    {
                        var array = new byte[length];
                        Array.Copy(bytes, index, array, 0, array.Length);
                        index += array.Length;

                        if (property.PropertyInfo.PropertyType.Equals(typeof(byte[])))
                        {
                            property.PropertyInfo.SetValue(obj, array);
                            continue;
                        }

                        if (property.PropertyInfo.PropertyType.Equals(typeof(ByteArray)))
                        {
                            var byteArray = new ByteArray(array);
                            property.PropertyInfo.SetValue(obj, byteArray);
                            continue;
                        }

                        throw new ArgumentException($"Property '{property.PropertyInfo.Name}' type should be byte[] or ByteArray");
                    }
                    else if (property.PropertyInfo.PropertyType.Equals(typeof(bool[])))
                    {
                        var array = new bool[length];
                        Buffer.BlockCopy(bytes, index, array, 0, array.Length);
                        index += array.Length;

                        property.PropertyInfo.SetValue(obj, array);
                        continue;
                    }

                    var converter = property.Converter;
                    if (converter == null)
                        throw new KeyNotFoundException($"Converter for property '{property.PropertyInfo.Name}' not found");

                    var elArray = Array.CreateInstance(property.PropertyInfo.PropertyType
                            .GetElementType(), length);

                    for (int i = 0; i < length; i++)
                    {
                        var elObj = converter.GetObject(bytes, ref index);
                        elArray.SetValue(elObj, i);
                    }
                    property.PropertyInfo.SetValue(obj, elArray);
                    continue;
                }
                else
                {
                    var converter = property.Converter;
                    if (converter == null)
                        throw new KeyNotFoundException($"Converter for property '{property.PropertyInfo.Name}' not found");


                    var value = converter.GetObject(bytes, ref index);
                    property.PropertyInfo.SetValue(obj, value);

                    continue;
                }
            }

            if ((index - startIndex) != count)
                throw new Exception("Something went wrong");

            return obj;
        }

        public Type GetType(byte[] bytes, ref int index)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length < (index + 3))
                throw new IndexOutOfRangeException(nameof(bytes.Length));

            byte version = bytes[index++];
            if (version != this.Version)
                throw new VersionMismatchException($"Should be {this.Version} but was {version}");


            int spaceId = bytes.GetValue<int>(4, ref index);
            short typeId = bytes.GetValue<short>(2, ref index);
            var context = ContextProvider.GetContext((spaceId << 4) + typeId);

            return context.Type;


            //ushort count = bytes.GetValue<ushort>(2, ref index);
            //var startIndex = index;

            //byte version = bytes[index++]; //bytes.GetValue<short>(2, ref index); // data converter versija

            //if (version != this.Version)
            //    throw new VersionMismatchException($"Should be {this.Version} but was {version}");

            //short typeId = bytes.GetValue<short>(2, ref index);

            //var context = ContextProvider.GetContext(typeId);

            //return context.Type;
        }

        public void AddType(Type type, byte[] bytes, ref int index)
        {
            var context = ContextProvider.GetContext(type);
            bytes[index++] = this.Version;

            bytes.AddBytes<int>(context.SpaceId, 4, ref index); // space type
            bytes.AddBytes<short>(context.TypeId, ref index); // data type
        }
    }
}
