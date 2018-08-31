using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Core.Interfaces;
using RedisMemoryCacheInvalidation.Utils;
using System;
using System.Globalization;
using System.Runtime.Caching;

namespace RedisMemoryCacheInvalidation.Monitor
{
    public class RedisChangeMonitor : ChangeMonitor, INotificationObserver<string>
    {
        private readonly string _key;
        private readonly IDisposable _unsubscriber;

        /// <summary>
        /// RedisChangeMonitor.
        /// </summary>
        /// <param name="notifier">Registration handler</param>
        /// <param name="key">invalidation Key</param>
        public RedisChangeMonitor(INotificationManager<string> notifier, string key)
        {
            Guard.NotNull(notifier, nameof(notifier));
            Guard.NotNullOrEmpty(key, nameof(key));

            var flag = true;
            try
            {
                _unsubscriber = notifier.Subscribe(key, this);
                UniqueId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
                _key = key;
                flag = false;
            }
            catch (Exception)
            {
                //any error
                flag = true;
            }
            finally
            {
                InitializationComplete();
                if (flag)
                {
                    base.Dispose();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            // always Unsubscribe on dispose
            Unsubscribe();
        }

        public override string UniqueId { get; }

        public void Notify(string value)
        {
            if (value == _key)
            {
                OnChanged(null);
            }
        }

        private void Unsubscribe()
        {
            _unsubscriber?.Dispose();
        }
    }
}
