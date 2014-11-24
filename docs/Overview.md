Overview
=

We need to use all the tools at our disposal to develop faster and more robust applications. 
One of the ways we can achieve this is by using caching. Previously, under the `System.Web.Caching.Cache` namespace, the new  -4.0- `System.Runtime.Caching.Memory` is more mature and still as powerful as its ancestor. 
It’s really easy to use and I think that every asp.net programmers have already used it.

The default caching strategy is the cache-aside programming pattern: This means that if your data is not present in the cache, your application, and not something else, must reload data into the cache from the original data source.

This works very well for most common use cases, but there is a hidden trade-off: caching means working with stale data. Should I increase the cache duration? Should I keep short TTL value? It’s never easy to answer to these questions, because it simply depends on your context: number of clients, user load, database activity…

Another common problem is how to update/remove instantly something from the cache on all your cache clients, such as this typical request from Product Owner ” I want a parameter to be instantly updated on all our XXX servers, but do not fear, I will only change it a few times per year”

Here comes `RedisMemoryCacheInlivation` : a small library that will help you to invalidate one or more items into you local memory cache.

Supports .net 4.0 and later.
Supports any kind of .net application (asp.net WebForms & MVC, WPF & WinForms, Console, ...).

As the name suggests, this implementation relies on [Redis](http://redis.io/), especially on [PubSub feature](http://redis.io/topics/pubsub).
In the current version of this implemention, __nothing is stored__ on the Redis server.
