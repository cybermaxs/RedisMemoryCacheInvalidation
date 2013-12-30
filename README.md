RedisMemoryCacheInvalidation
============================

Custom ChangeMonitor to allow System.Runtime.MemoryCache invalidation using Redis PubSub.

Demo Application
-------------------

A sample web site is provided. Do not hesitate to open and test it.

How to use it ?
------------------

Redis connection settings can be provider by code or by configuration.

- Code

```csharp
  //somewhere in your global.asax (for an asp.net application)
  // be sure a redis instance is running on localhost
  // check /tools/redis-server.exe
  CacheInvalidation.UseRedis(new RedisConnectionInfo() { Host = "localhost" });
```

- Configuration

```csharp
  //somewhere in your global.asax (for an asp.net application)
  // be sure a redis instance is running on localhost
  // check /tools/redis-server.exe
  CacheInvalidation.UseRedis(new RedisConnectionInfo() { Host = "localhost" });
  
  //in your configuration file
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
	<configSections>
		<section name="redis" type="RedisMemoryCacheInvalidation.Configuration.RedisConfigurationSection, RedisMemoryCacheInvalidation" />
	</configSections>
	<redis host="localhost"></redis>
</configuration>
```

Here is the minimal code required to create a changemonitor. 
Feel free to adapt to your context.
```csharp
var cacheItem = new CacheItem("onekey", "onevalue");
var policy = new CacheItemPolicy();
policy.ChangeMonitors.Add(CacheInvalidation.CreateChangeMonitor(cacheItem));
policy.AbsoluteExpiration = DateTime.Now.AddYears(1);
MemoryCache.Default.Add(cacheItem, policy);
```
your can also create items that are depending on a specific redis message.
```csharp
var cacheItem = new CacheItem("onekey", "onevalue");
var policy = new CacheItemPolicy();
policy.ChangeMonitors.Add(CacheInvalidation.CreateChangeMonitor("mycustominvalidationmessage"));
policy.AbsoluteExpiration = DateTime.Now.AddYears(1);
MemoryCache.Default.Add(cacheItem, policy);
```
How it works ?
------------------
Read the introduction post here.
