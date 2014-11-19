using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

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
            this.items = new List<T>();
            this.sync = new object();
        }

        public int Count
        {
            get { lock (this.sync) { return this.items.Count; } }
        }

        public void Add(T item)
        {
            lock (this.sync)
            {
                int index = this.items.Count;
                this.items.Insert(index, item);
            }
        }

        public void Clear()
        {
            lock (this.sync)
            {
                this.items.Clear();
            }
        }

        public void CopyTo(T[] array, int index)
        {
            lock (this.sync)
            {
                this.items.CopyTo(array, index);
            }
        }

        public bool Contains(T item)
        {
            lock (this.sync)
            {
                return this.items.Contains(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (this.sync)
            {
                return this.items.GetEnumerator();
            }
        }

        int InternalIndexOf(T item)
        {
            int count = items.Count;

            for (int i = 0; i < count; i++)
            {
                if (object.Equals(items[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool Remove(T item)
        {
            lock (this.sync)
            {
                int index = this.InternalIndexOf(item);
                if (index < 0)
                    return false;

                this.items.RemoveAt(index);
                return true;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList)this.items).GetEnumerator();
        }
    }
}
