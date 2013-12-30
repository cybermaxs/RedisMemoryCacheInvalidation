using System;

namespace RedisMemoryCacheInvalidation
{
    /// <summary>
    /// Extended IObservable to support topic-based subscriptions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITopicObservable<out T>
    {
        IDisposable Subscribe(string topic, IObserver<T> observer);
    }
}
