using RedisMemoryCacheInvalidation.Core.Interfaces;
using System;

namespace RedisMemoryCacheInvalidation.Core
{
    /// <summary>
    /// Manage a list of subscription. Basically a custom IObservable to support topic-based subscriptions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INotificationManager<T>
    {
        IDisposable Subscribe(string topic, INotificationObserver<T> observer);

        void Notify(string topicKey);
    }
}
