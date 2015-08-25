RedisMemoryCacheInvalidation
============================

[![Build status](https://ci.appveyor.com/api/projects/status/o64bqf543kype8eq?svg=true)](https://ci.appveyor.com/project/Cybermaxs/redismemorycacheinvalidation)
[![Nuget](https://img.shields.io/nuget/dt/redismemorycacheinvalidation.svg)](http://nuget.org/packages/redismemorycacheinvalidation)
[![Nuget](https://img.shields.io/nuget/v/redismemorycacheinvalidation.svg)](http://nuget.org/packages/redismemorycacheinvalidation)

System.Runtime.MemoryCache invalidation using Redis PubSub feature.


Installing via NuGet
---
```
Install-Package RedisMemoryCacheInvalidation
```


How to use it ?
---

__quick start__


First, you have to configure the library, mainly to setup a persistent redis connection and various stuff
```csharp
  // somewhere in your global.asax/startup.cs
  InvalidationManager.Configure("localhost:6379", new InvalidationSettings());
```
Redis connection string follow [StackExchange.Redis Configuration model](https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md)

Threre are at least 3 ways to send invalidation messages :
- send an invalidation message via any redis client following the command `PUBLISH invalidate onemessagekey`
- use `InvalidationManager.InvalidateAsync` (same as the previous one)
- use keyspace notification (yes, RedisMemoryCacheInvalidation supports it)

Once an invalidation message is intercepted by the library, you can invalidate one or more items at the same time by using `InvalidationSettings.InvalidationStrategy`
- `InvalidationStrategyType.ChangeMonitor` => a custom custom change monitor `InvalidationManager.CreateChangeMonitor`
- `InvalidationStrategyType.AutoCacheRemoval` => use the automatic MemoryCache removal configured at `InvalidationSettings.ConfigureAsync`
- `InvalidationStrategyType.External` => use the callback configured at `InvalidationSettings.InvalidationCallback`

__Read more__
- [Overview] (https://github.com/Cybermaxs/RedisMemoryCacheInvalidation/blob/master/docs/Overview.md)
- [Working with Redis] (https://github.com/Cybermaxs/RedisMemoryCacheInvalidation/blob/master/docs/Redis.md)
- [Configuration] (https://github.com/Cybermaxs/RedisMemoryCacheInvalidation/blob/master/docs/Configuration.md)
- [Examples] (https://github.com/Cybermaxs/RedisMemoryCacheInvalidation/blob/master/docs/Examples.md)

How it works ?
---
Read the introduction post for the initial version (beginning of 2014) here : https://techblog.betclicgroup.com/2013/12/31/implementing-local-memorycache-invalidation-with-redis/

License
---
Licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT)

Want to contribute ?
---
- Beginner => Download, Star, Comment/Tweet, Kudo, ...
- Amateur => Ask for help, send feature request, send bugs
- Pro => Pull request, promote

Thank you
