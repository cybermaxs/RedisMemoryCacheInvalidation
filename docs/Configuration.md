Configuration
=

Settings
---
To configure `RedisMemoryCacheInvalidation`, you should use one of the `InvalidationManager.ConfigureAsync` methods. 
Three parameters are available to configure it : 

- __redisConfig:string__ : Redis connection string. Check [StackExchange.Redis Configuration model](https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md) for more details. A basic example is `localhost:6379`.
- __mux:ConnectionMultiplexer__ : an existing StackExchange.Redis.ConnectionMultiplexer that you want to reuse.
- __settings:InvalidationSettings__ : see below for more details.

InvalidationSettings is the main configuration object
- __InvalidationStrategy:InvalidationStrategyType__ : How to handle invalidation notifications : notify ChangeMonitor, execute callback or automatically remove an item from the cache. 
- __TargetCache:MemoryCache__ : the target MemoryCache instance when `InvalidationStrategy` is set to `AutoCacheRemoval`.
- __EnableKeySpaceNotifications:bool__ : allow subscribe to keyevents notification `__keyevent*__:`.       
- __InvalidationCallback:Action__ : an callback that is invoked when  `InvalidationStrategy` is set to `External`.

When to configure ?
---
Thanks to StackExchange.Redis a persistent connection is established between your application and the redis server. 
That's why it's important to configure it very early at startup : Global.asax, Owin Startup, ... In this way, you won't lose notification messages.