using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using System;
using System.Globalization;
using System.Runtime.Caching;

namespace RedisMemoryCacheInvalidation.Monitor
{
    public class RedisChangeMonitor : ChangeMonitor, INotificationObserver<string>
    {
        private readonly string uniqueId;
        private readonly string key;
        private IDisposable unsubscriber;
        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="notifier">Registration handler</param>
        /// <param name="key">invalidation Key</param>
        public RedisChangeMonitor(INotificationManager<string> notifier, string key)
        {
            if (notifier == null)
            {
                throw new ArgumentNullException("notificationBus");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }
            bool flag = true;
            try
            {
                this.unsubscriber = notifier.Subscribe(key, this);
                this.uniqueId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                this.key = key;
                flag = false;
            }
            catch
            {
                //any error
                flag = true;
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
            // always Unsubscribe on dispose
            this.Unsubscribe();
        }

        public override string UniqueId
        {
            get { return this.uniqueId; }
        }

        #region INotification
        public void Notify(string value)
        {
            if (value == key)
                base.OnChanged(null);
        }
        #endregion

        private void Unsubscribe()
        {
            if (this.unsubscriber != null)
                this.unsubscriber.Dispose();
        }
    }
}
