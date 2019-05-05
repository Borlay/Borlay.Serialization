using System;
using System.Collections.Generic;
using System.Text;

namespace Borlay.Serialization
{
    public interface IContextProvider
    {
        ConverterContext GetContext(Type type);
        ConverterContext GetContext(long typeId);

        bool TryGetContext(Type type, out ConverterContext context);
        bool TryGetContext(long typeId, out ConverterContext context);

        bool Contains<T>();
        bool Contains(Type type);

        void AddContext<T>(ConverterContext converterContext, long typeId);
        void AddContext(ConverterContext converterContext, Type type, long typeId);

        void Clear();
    }

    public class ContextProvider : IContextProvider
    {
        protected Dictionary<long, ConverterContext> contexts = new Dictionary<long, ConverterContext>();
        protected Dictionary<Type, long> contextTypes = new Dictionary<Type, long>();

        public virtual bool Contains<T>()
        {
            return Contains(typeof(T));
        }

        public virtual bool Contains(Type type)
        {
            return contextTypes.ContainsKey(type);
        }

        public virtual void AddContext<T>(ConverterContext converterContext, long typeId)
        {
            AddContext(converterContext, typeof(T), typeId);
        }

        public virtual void AddContext(ConverterContext converterContext, Type type, long typeId)
        {
            contextTypes[type] = typeId;
            contexts[typeId] = converterContext;
        }

        public virtual ConverterContext GetContext(Type type)
        {
            return contexts[contextTypes[type]];
        }

        public virtual ConverterContext GetContext(long typeId)
        {
            return contexts[typeId];
        }

        public virtual void Clear()
        {
            contextTypes.Clear();
            contexts.Clear();
        }

        public bool TryGetContext(Type type, out ConverterContext context)
        {
            if(contextTypes.TryGetValue(type, out var typeId))
            {
                if (contexts.TryGetValue(typeId, out context))
                    return true;
            }

            context = null;
            return false;
        }

        public bool TryGetContext(long typeId, out ConverterContext context)
        {
            if (contexts.TryGetValue(typeId, out context))
                return true;

            context = null;
            return false;
        }
    }
}
