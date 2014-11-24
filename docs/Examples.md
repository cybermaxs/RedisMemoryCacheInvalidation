How to use
=

Once RedisMemoryCacheInvalidation is configured, local cache invalidation is a two-steps process : capturing invalidation messages and handling those notification messages.


Sending invalidation messages
---
You can use one of the folowing methods.

- Send a pubsub message from any redis client `PUBLISH invalidate onemessagekey`.
- Send an invalidation message from `InvalidationManager.InvalidateAsync("onemessagekey")`
- Capture keyspace events for one particular key. Note : the redis server should be configured to support keyspace events. (off by default)

Handling notification messages
---
This behavior is entirely configured via `InvalidationSettings.InvalidationStrategyType`. As it's marked with a FlagsAttribute, you can use one or more strategies.

- Automatically removed a cache item from the cache

The easiest way to invalidate local cache items. If the  The core will try to remove cache items 
For example, if you add a cache item like this :

```
CacheItem cacheItem = new CacheItem("mycacheKey", "cachevalue");
CacheItemPolicy policy = new CacheItemPolicy();
policy.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
MyCache.Add(cacheItem, policy);
```

Calling  `PUBLISH invalidate mycacheKey` or `InvalidationManager.InvalidateAsync("mycacheKey")` will remove that item from the cache.

- Notify ChangeMonitors

[ChangeMonitor](http://msdn.microsoft.com/en-us/library/system.runtime.caching.changemonitor(v=vs.110).aspx) is defined as "Provides a base class for a derived custom type that monitors changes in the state of the data which a cache item depends on.""

You can create a custom monitor (watching for `myinvalidationKey`) like this :

```
CacheItem cacheItem = new CacheItem("cacheKey", "cachevalue");
CacheItemPolicy policy = new CacheItemPolicy();
policy.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
policy.ChangeMonitors.Add(InvalidationManager.CreateChangeMonitor("myinvalidationKey"));
MyCache.Add(cacheItem, policy);
```

When , the corresponding cache item will be automatically removed.
One interesting feature is that you can create several change monitors watching for the same key.

- invoke a callback

Suppose you're using another caching implementation (Entlib, System.Web.Caching, ...), there is another way to be notified with `InvalidationStrategyType.External`.
Each time a notification message is intercepted, the callback defined in `InvalidationSettings.InvalidationCallback` is invoked.
It's up to you to remove/flush/reload the cache item.
