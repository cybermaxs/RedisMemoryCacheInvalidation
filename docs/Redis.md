Redis
=


Working Redis on Windows
---
The Redis project does not directly support Windows, however the Microsoft Open Tech group develops and maintains an [Windows port targeting Win64](https://github.com/MSOpenTech/redis).
So, the folder `tools` contains an exact copy of the nuget package [Redis-64](http://www.nuget.org/packages/Redis-64/) maintenaned by MsOpenTech. 
This is the easiest way to test locally this NoSql DB on Windows. Simply run `redis-server.exe` and that's all. In this case, the connection string is `localhost:6379`.
Please note that this port is also used on Windows Azure, so it's prod-ready.


on Production
---
You just need a redis server and it doesn't matter on the hosting platform. If you don't have Redis into you stack, there are several -cloud- providers like Azure, Redis to Go or Redis Watch.  