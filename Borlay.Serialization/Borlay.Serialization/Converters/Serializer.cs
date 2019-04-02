using Borlay.Arrays;
using Borlay.Serialization.Notations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class Serializer : ISerializer, IContextProvider
    {
        private Dictionary<short, ConverterContext> contexts = new Dictionary<short, ConverterContext>();
        private Dictionary<Type, short> contextTypes = new Dictionary<Type, short>();

        private Dictionary<Type, short> converterTypes = new Dictionary<Type, short>(); // all together, data types and converter types
        private Dictionary<short, IConverter> converters = new Dictionary<short, IConverter>();

        public static Encoding DefaultStringEncoding { get; set; } = Encoding.UTF8;

        public byte Version => 1;
        public byte SerializerType => 1;

        public Serializer()
        {
            InitializeConverters();
        }

        public virtual void Register<T>()
        {
            var type = typeof(T);
            Register(type);
        }

        public virtual bool Contains<T>()
        {
            return Contains(typeof(T));
        }

        public virtual bool Contains(Type type)
        {
            return contextTypes.ContainsKey(type);
        }

        public virtual void Register(Type type)
        {
            var dataAttribute = type.GetTypeInfo().GetCustomAttribute<DataAttribute>(false);
            if (dataAttribute.TypeId < 100 && !dataAttribute.IsSystem)
                throw new ArgumentException($"Data types from 0 to 99 are reserved. Current {dataAttribute.TypeId}");

            var properties = type.GetTypeInfo().GetProperties()
                .Where(p => p.GetCustomAttribute<IncludeAttribute>(true) != null).Select(property =>
                new PropertyContext()
                {
                    Include = property.GetCustomAttribute<IncludeAttribute>(true),
                    Array = property.GetCustomAttribute<ArrayAttribute>(true),
                    PropertyInfo = property,
                    Converter = GetConverter(property.PropertyType, out var converterTypeId),
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
            AddContext(type, dataAttribute.TypeId, converterContext);
        }

        public virtual void AddContext(Type type, short typeId, ConverterContext converterContext)
        {
            contextTypes.Add(type, typeId);
            contexts.Add(typeId, converterContext);
        }

        public void AddValueConverter<T>(short typeId)
        {
            AddConverter<T>(new ValueConverter<T>(), typeId);
        }

        public void AddConverter<T>(IConverter converter, short typeId)
        {
            converterTypes[typeof(T)] = typeId;
            converters[typeId] = converter;
        }

        private void InitializeConverters()
        {
            AddConverter<byte>(new ByteValueConverter(), 30002);
            AddConverter<bool>(new BoolValueConverter(), 30003);

            AddValueConverter<ushort>(30004);
            AddValueConverter<uint>(30005);
            AddValueConverter<ulong>(30006);

            AddValueConverter<short>(30007);
            AddValueConverter<int>(30008);
            AddValueConverter<long>(30009);

            AddValueConverter<float>(30010);
            AddValueConverter<double>(30011);
            AddValueConverter<decimal>(30012);

            AddValueConverter<char>(30013);


            AddConverter<ByteArray>(new ByteArrayConverter(), 30101);
            AddConverter<string>(new StringConverter(DefaultStringEncoding), 30102);
            AddConverter<Enum>(new EnumConverter(), 30103);

            AddConverter<object>(new DataConverter(this), 30200);
        }

        // todo 
        // test ByteArrayConverter
        // test value object serializations.
        // add byte array length checks in ValueConverter or maybe byte.Add extensions, tiksliau get.

        public virtual void AddBytes(object obj, byte[] bytes, ref int index) // byte is 2024 length
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            var type = obj.GetType();
            var converter = GetConverter(type, out var converterTypeId);

            bytes[index++] = this.Version;

            bytes.AddBytes<short>(converterTypeId, 2, ref index);
            
            converter.AddBytes(obj, bytes, ref index);
        }

        public virtual object GetObject(byte[] bytes, ref int index)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (bytes[index++] != this.Version)
                throw new VersionMismatchException($"{this.Version}");

            var converterTypeId = bytes.GetValue<short>(2, ref index);
            var converter = GetConverter(converterTypeId);
            var obj = converter.GetObject(bytes, ref index);

            return obj;
        }

        public virtual IConverter GetConverter<T>(out short typeId)
        {
            return GetConverter(typeof(T), out typeId);
        }

        public virtual Type ToConverterType(Type type)
        {
            if (type.IsArray)
                return ToConverterType(type.GetElementType());

            if (type.GetTypeInfo().IsEnum)
                return typeof(Enum);

            if (type.GetTypeInfo().IsClass)
                return typeof(object);

            return type;
        }

        public virtual IConverter GetConverter(Type type, out short typeId)
        {
            if (converterTypes.TryGetValue(type, out typeId))
                return GetConverter(typeId);

            var converterType = ToConverterType(type);

            if(converterTypes.TryGetValue(converterType, out typeId))
                return GetConverter(typeId);

            throw new KeyNotFoundException($"Converter for type '{type.Name}' not found");
        }

        public virtual IConverter GetConverter(short typeId)
        {
            if (converters.TryGetValue(typeId, out var converter))
                return converter;

            throw new KeyNotFoundException($"Converter for type '{typeId}' not found");
        }

        public virtual void LoadFromReference<T>()
        {
            LoadFromReference(typeof(T));
        }

        public virtual void LoadFromReference(Type referenceType)
        {
            foreach (var ptype in GetTypesFromReference(referenceType))
            {
                if (!Contains(ptype))
                    Register(ptype);
            }
        }

        public static Serializer CreateFromReference<T>()
        {
            return CreateFromReference(typeof(T));
        }

        public static Serializer CreateFromReference(Type referenceType)
        {
            var converter = new Serializer();
            converter.LoadFromReference(referenceType);
            return converter;
        }

        public static IEnumerable<Type> GetTypesFromReference<T>()
        {
            return GetTypesFromReference(typeof(T));
        }

        public static IEnumerable<Type> GetTypesFromReference(Type referenceType)
        {
            var assembly = referenceType.GetTypeInfo().Assembly;
            var assemblyNames = assembly.GetReferencedAssemblies();

            var assemblies = assemblyNames.Select(a => Assembly.Load(a)).ToList();
            assemblies.Add(assembly);

            return from a in assemblies
                   from t in a.GetTypes()
                   where t.GetTypeInfo().GetCustomAttribute<DataAttribute>(true) != null
                   && t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract
                   select t;
        }

        public ConverterContext GetContext(Type type)
        {
            return contexts[contextTypes[type]];
        }

        public ConverterContext GetContext(short typeId)
        {
            return contexts[typeId];
        }
    }   

    public class VersionMismatchException : Exception
    {
        public VersionMismatchException(string currentVersion)
            : base($"Protocol version mismatch. Current: '{currentVersion}'")
        {

        }
    }
}
