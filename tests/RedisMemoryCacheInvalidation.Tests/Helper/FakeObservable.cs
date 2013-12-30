using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedisMemoryCacheInvalidation.Tests.Helper
{
    public class FakeObservable : ITopicObservable<string>, IInvalidationMessageBus
    {
        public List<IObserver<string>> observers = new List<IObserver<string>>();

        public IDisposable Subscribe(string topic, IObserver<string> observer)
        {
            observers.Add(observer);
            return new Unsubscriber(observers, observer);
        }

        public void Notify(string key)
        {       
            foreach (IObserver<string> observer in observers.ToList())
            {
                observer.OnNext(key);
            }
        }

        public System.Threading.Tasks.Task<long> Invalidate(string key)
        {
            throw new NotImplementedException();
        }
    }
}
