using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RedisMemoryCacheInvalidation.Utils
{
    /// <summary>
    /// Inspired by System.ServiceModel.SynchronizedCollection
    /// https://github.com/Microsoft/referencesource/blob/master/System.ServiceModel/System/ServiceModel/SynchronizedCollection.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ExcludeFromCodeCoverage]
    public class SynchronizedCollection<T> : ICollection<T>
    {
        private List<T> items;
        private object sync;

        public SynchronizedCollection()
        {
            items = new List<T>();
            sync = new object();
        }

        public int Count
        {
            get { lock (sync) { return items.Count; } }
        }

        public void Add(T item)
        {
            lock (sync)
            {
                int index = items.Count;
                items.Insert(index, item);
            }
        }

        public void Clear()
        {
            lock (sync)
            {
                items.Clear();
            }
        }

        public void CopyTo(T[] array, int index)
        {
            lock (sync)
            {
                items.CopyTo(array, index);
            }
        }

        public bool Contains(T item)
        {
            lock (sync)
            {
                return items.Contains(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (sync)
            {
                return items.GetEnumerator();
            }
        }

        int InternalIndexOf(T item)
        {
            int count = items.Count;

            for (int i = 0; i < count; i++)
            {
                if (Equals(items[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool Remove(T item)
        {
            lock (sync)
            {
                int index = InternalIndexOf(item);
                if (index < 0)
                    return false;

                items.RemoveAt(index);
                return true;
            }
        }

        bool ICollection<T>.IsReadOnly => false;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList)items).GetEnumerator();
        }
    }
}
