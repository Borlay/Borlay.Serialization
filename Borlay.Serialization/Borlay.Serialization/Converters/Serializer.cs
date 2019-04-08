using Borlay.Arrays;
using Borlay.Serialization.Notations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Borlay.Serialization.Converters
{
    public class Serializer : ISerializer
    {
        public static Encoding DefaultStringEncoding { get; set; } = Encoding.UTF8;

        public IContextProvider ContextProvider { get; protected set; }
        public IConverterProvider ConverterProvider { get; protected set; }

        public short Version => 1;

        public byte Type => 1;

        public Serializer()
        {
            ContextProvider = new ContextProvider();
            ConverterProvider = new ConverterProvider();
            InitializeConverters();
        }

        public Serializer(IContextProvider contextProvider, IConverterProvider converterProvider)
        {
            if (contextProvider == null)
                throw new ArgumentNullException(nameof(contextProvider));

            if (converterProvider == null)
                throw new ArgumentNullException(nameof(converterProvider));

            this.ContextProvider = contextProvider;
            this.ConverterProvider = converterProvider;
            InitializeConverters();
        }

        protected void InitializeConverters()
        {
            ConverterProvider.AddConverter<byte>(new ByteValueConverter(), 30002);
            ConverterProvider.AddConverter<bool>(new BoolValueConverter(), 30003);

            ConverterProvider.AddValueConverter<ushort>(30004);
            ConverterProvider.AddValueConverter<uint>(30005);
            ConverterProvider.AddValueConverter<ulong>(30006);

            ConverterProvider.AddValueConverter<short>(30007);
            ConverterProvider.AddValueConverter<int>(30008);
            ConverterProvider.AddValueConverter<long>(30009);

            ConverterProvider.AddValueConverter<float>(30010);
            ConverterProvider.AddValueConverter<double>(30011);
            ConverterProvider.AddValueConverter<decimal>(30012);

            ConverterProvider.AddValueConverter<char>(30013);


            ConverterProvider.AddConverter<ByteArray>(new ByteArrayConverter(), 30101);
            ConverterProvider.AddConverter<string>(new StringConverter(DefaultStringEncoding), 30102);
            ConverterProvider.AddConverter<Enum>(new EnumConverter(), 30103);
            ConverterProvider.AddConverter<DateTime>(new DateTimeConverter(), 30104);

            ConverterProvider.AddConverter<object>(new DataConverter(ContextProvider), 30200);
            ConverterProvider.AddConverter<Array>(new ArrayConverter(ConverterProvider), 30201);
        }
        
        public virtual void Register<T>()
        {
            Register(typeof(T));
        }

        public virtual void Register(Type type)
        {
            var converterContext = ConverterProvider.CreateContext(type, out var typeId);
            ContextProvider.AddContext(converterContext, type, typeId);
        }

        // add byte array length checks in ValueConverter or maybe byte.Add extensions, tiksliau get.

        public virtual void AddBytes(object obj, byte[] bytes, ref int index) // byte is 2024 length
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            var type = obj.GetType();
            var converter = ConverterProvider.GetConverter(type, out var converterTypeId);

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
            var converter = ConverterProvider.GetConverter(converterTypeId);
            return converter;
        }

        public virtual void LoadFromReference<T>()
        {
            LoadFromReference(typeof(T));
        }

        public virtual void LoadFromReference(Type referenceType)
        {
            foreach (var ptype in GetTypesFromReference(referenceType))
            {
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

        protected static IEnumerable<Type> GetTypesFromReference<T>()
        {
            return GetTypesFromReference(typeof(T));
        }

        protected static IEnumerable<Type> GetTypesFromReference(Type referenceType)
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
