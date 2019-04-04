using Borlay.Arrays;
using Borlay.Serialization.Notations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class Serializer : ISerializer, IConverterProvider, IContextProvider
    {
        private Dictionary<short, ConverterContext> contexts = new Dictionary<short, ConverterContext>();
        private Dictionary<Type, short> contextTypes = new Dictionary<Type, short>();

        private Dictionary<Type, short> converterTypes = new Dictionary<Type, short>(); // all together, data types and converter types
        private Dictionary<short, IConverter> converters = new Dictionary<short, IConverter>();

        public static Encoding DefaultStringEncoding { get; set; } = Encoding.UTF8;

        public short Version => 1;
        //public byte SerializerType => 1;

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

        public virtual void Register<T>(Type type)
        {
            Register(typeof(T));
        }

        public virtual void Register(Type type)
        {
            var converterContext = this.CreateContext(type, out var typeId);
            AddContext(converterContext, type, typeId);
        }

        public virtual void AddContext<T>(ConverterContext converterContext, short typeId)
        {
            AddContext(converterContext, typeof(T), typeId);
        }

        public virtual void AddContext(ConverterContext converterContext, Type type, short typeId)
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
            AddConverter(converter, typeof(T), typeId);
        }

        public void AddConverter(IConverter converter, Type type, short typeId)
        {
            converterTypes[type] = typeId;
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
            AddConverter<DateTime>(new DateTimeConverter(), 30104);

            AddConverter<object>(new DataConverter(this), 30200);
            AddConverter<Array>(new ArrayConverter(this), 30201);
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

            bytes.AddBytes<short>(Version, 2, ref index);
            bytes.AddBytes<short>(converterTypeId, 2, ref index);
            
            converter.AddBytes(obj, bytes, ref index);
        }

        public virtual object GetObject(byte[] bytes, ref int index)
        {
            var converter = GetConverter(bytes, ref index);
            var obj = converter.GetObject(bytes, ref index);

            return obj;
        }

        public virtual IConverter GetConverter(byte[] bytes, ref int index)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            var version = bytes.GetValue<short>(2, ref index);

            if (version != this.Version)
                throw new VersionMismatchException($"Should be {this.Version} but was {version}");

            var converterTypeId = bytes.GetValue<short>(2, ref index);
            var converter = GetConverter(converterTypeId);
            return converter;
        }

        public virtual IConverter GetConverter<T>(out short typeId)
        {
            return GetConverter(typeof(T), out typeId);
        }

        public virtual Type ToConverterType(Type type)
        {
            if (type.IsArray)
                return typeof(Array); //type.GetElementType();

            if (type.GetTypeInfo().IsEnum)
                return typeof(Enum);

            if (type.GetTypeInfo().IsClass)
                return typeof(object);

            throw new Exception($"Type '{type.FullName}' doesn't have converter type");

            //return type;
        }

        public virtual IConverter GetConverter(Type type, out short typeId)
        {
            do
            {
                if (converterTypes.TryGetValue(type, out typeId))
                    return GetConverter(typeId);

                type = ToConverterType(type);
            } while (true);

            //var converterType = ToConverterType(type);

            //if(converterTypes.TryGetValue(converterType, out typeId))
            //    return GetConverter(typeId);

            //throw new KeyNotFoundException($"Converter for type '{type.Name}' not found");
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

        public Type GetType(byte[] bytes, int index)
        {
            var converter = GetConverter(bytes, ref index);
            var type = converter.GetType(bytes, index);
            return type;
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
