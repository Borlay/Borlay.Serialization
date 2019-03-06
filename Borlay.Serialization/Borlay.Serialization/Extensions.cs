using Borlay.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Borlay.Serialization
{
    public static class Extensions
    {
        public static void Append(this List<byte> list, ByteArray byteArray)
        {
            if(byteArray == null)
            {
                list.AddRange(BitConverter.GetBytes((ushort)0));
                return;
            }
            list.AddRange(BitConverter.GetBytes((ushort)byteArray.Bytes.Length));
            list.AddRange(byteArray.Bytes);
        }

        public static void Append(this List<byte> list, byte value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public static void Append(this List<byte> list, ulong value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public static void Append(this List<byte> list, uint value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public static void Append(this List<byte> list, ushort value)
        {
            list.AddRange(BitConverter.GetBytes(value));
        }

        public static TValue[] Get<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, params TKey[] keys)
        {
            TValue[] result = new TValue[keys.Length];
            for(int i = 0; i < keys.Length; i++)
            {
                result[i] = dictionary[keys[i]];
            }
            return result;
        }

        public static T[] ToValues<T>(this IEnumerable<List<T>> lists)
        {
            List<T> result = new List<T>();
            foreach(var list in lists)
            {
                result.AddRange(list);
            }
            return result.ToArray();
        }

        public static void AddToList<TKey, TValue>(
            this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            List<TValue> result = null;

            if (dictionary.ContainsKey(key))
                result = dictionary[key];
            else
            {
                result = new List<TValue>();
                dictionary.Add(key, result);
            }

            if(!result.Any(r => r.Equals(value)))
                result.Add(value); 
        }
    }
}
