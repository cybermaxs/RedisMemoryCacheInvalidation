using RedisMemoryCacheInvalidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;

namespace SampleWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private const string parentkey = "mynotifmessage";

        public ActionResult Index()
        {
            var cache = MemoryCache.Default;

            var cacheItem = new CacheItem("onekey", DateTime.Now);
            if (!cache.Contains(cacheItem.Key))
            {
                var policy = new CacheItemPolicy();
                policy.ChangeMonitors.Add(InvalidationManager.CreateChangeMonitor(parentkey));
                policy.AbsoluteExpiration = DateTime.Now.AddYears(1); // just to create not expirable item
                MemoryCache.Default.Add(cacheItem, policy);
            }


            DateTime dt = (DateTime)cache[cacheItem.Key];

            ViewBag.Message = string.Format("'{0}' was set at {1}", cacheItem.Key, dt.ToLongTimeString());

            return View();
        }

        public ActionResult Invalidate()
        {
            InvalidationManager.InvalidateAsync(parentkey);

            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}