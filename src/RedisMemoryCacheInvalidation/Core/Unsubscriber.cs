using RedisMemoryCacheInvalidation.Core.Interfaces;
using RedisMemoryCacheInvalidation.Utils;
using System;

namespace RedisMemoryCacheInvalidation.Core
{
    public class Unsubscriber : IDisposable
    {
        private SynchronizedCollection<INotificationObserver<string>> observers;
        private INotificationObserver<string> observer;

        public Unsubscriber(SynchronizedCollection<INotificationObserver<string>> observers, INotificationObserver<string> observer)
        {
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
