using System;
using System.Globalization;
using System.Runtime.Caching;

namespace RedisMemoryCacheInvalidation
{
    public class RedisChangeMonitor : ChangeMonitor, IObserver<string>
    {
        private readonly ITopicObservable<string> bus;
        private readonly string uniqueId;
        private readonly string key;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="bus">Registration handler</param>
        /// <param name="key">invalidation Key</param>
        public RedisChangeMonitor(ITopicObservable<string> bus, string key)
        {
            if (bus == null)
            {
                throw new ArgumentNullException("bus");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }
            bool flag = true;
            try
            {
                this.bus = bus;
                this.unsubscriber=this.bus.Subscribe(key, this);
                this.uniqueId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                this.key = key;
                flag = false;
            }
            finally
            {
                base.InitializationComplete();
                if (flag)
                {
                    base.Dispose();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            // always Unsubscribe
            this.Unsubscribe();
        }

        public override string UniqueId
        {
            get { return this.uniqueId; }
        }

        private IDisposable unsubscriber;

        #region IObserver
        public void OnCompleted()
        {
            this.Unsubscribe();
        }

        public void OnError(Exception error)
        {
            this.Unsubscribe();
        }

        public void OnNext(string value)
        {
            if (value == key)
                base.OnChanged(null);
        }
        #endregion

        public void Unsubscribe()
        {
            unsubscriber.Dispose();
        }
    }
}
