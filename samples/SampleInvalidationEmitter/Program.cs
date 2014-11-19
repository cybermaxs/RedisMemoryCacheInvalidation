using RedisMemoryCacheInvalidation;
using System;

namespace SampleInvalidationEmitter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Simple Invalidation Emitter");

            InvalidationManager.Configure("localhost:6379").Wait();

            Console.WriteLine("IsConnected : "+ InvalidationManager.IsConnected);

            Console.WriteLine("enter a key to send invalidation (default is 'mynotifmessage'): ");
            var key = Console.ReadLine();
            var task = InvalidationManager.Invalidate(string.IsNullOrEmpty(key) ? "mynotifmessage": key);

            Console.WriteLine("message send to {0} clients", task.Result);
            Console.ReadLine();     
        }
    }
}
