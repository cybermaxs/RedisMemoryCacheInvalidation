using RedisMemoryCacheInvalidation.Core.Interfaces;
using RedisMemoryCacheInvalidation.Utils;
using System;
using System.Linq;
using System.Collections.Concurrent;

namespace RedisMemoryCacheInvalidation.Core
{
    /// <summary>
    /// Manager subscriptions and notifications.
    /// </summary>
    internal class NotificationManager : INotificationManager<string>
    {
        internal ConcurrentDictionary<string, SynchronizedCollection<INotificationObserver<string>>> SubscriptionsByTopic { get; set; }

        public NotificationManager()
        {
            SubscriptionsByTopic = new ConcurrentDictionary<string, SynchronizedCollection<INotificationObserver<string>>>();
        }
        public void Notify(string topicKey)
        {
            var subscriptions = SubscriptionsByTopic.GetOrAdd(topicKey, new SynchronizedCollection<INotificationObserver<string>>());

            if (subscriptions.Count <= 0) return;

            foreach (INotificationObserver<string> observer in subscriptions.ToList()) //avoid collection modified
            {
                observer.Notify(topicKey);
            }
        }

        public IDisposable Subscribe(string topicKey, INotificationObserver<string> observer)
        {
            var subscriptions = SubscriptionsByTopic.GetOrAdd(topicKey, new SynchronizedCollection<INotificationObserver<string>>());

            if (!subscriptions.Contains(observer))
                subscriptions.Add(observer);

            return new Unsubscriber(subscriptions, observer);
        }
    }
}
