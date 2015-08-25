using RedisMemoryCacheInvalidation.Core.Interfaces;
using RedisMemoryCacheInvalidation.Utils;
using System;

namespace RedisMemoryCacheInvalidation.Core
{
    internal class Unsubscriber : IDisposable
    {
        private SynchronizedCollection<INotificationObserver<string>> observers;
        private INotificationObserver<string> observer;

        public Unsubscriber(SynchronizedCollection<INotificationObserver<string>> observers, INotificationObserver<string> observer)
        {
            Guard.NotNull(observers, nameof(observers));
            Guard.NotNull(observer, nameof(observer));

            this.observers = observers;
            this.observer = observer;
        }

        public void Dispose()
        {
            if (observer != null && observers.Contains(observer))
                observers.Remove(observer);
        }
    }
}
