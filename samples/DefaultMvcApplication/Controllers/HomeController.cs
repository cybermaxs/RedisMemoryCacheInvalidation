using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using RedisMemoryCacheInvalidation;

namespace DefaultMvcApplication.Controllers
{
    public class HomeController : Controller
    {
        private const string childkey = "notexpirablekey";
        private const string parentkey = "mynotifmessage";

        public ActionResult Index()
        {
            var cache = MemoryCache.Default;
            if (!cache.Contains(childkey))
            {
                var policy = new CacheItemPolicy();
                policy.ChangeMonitors.Add(CacheInvalidation.CreateChangeMonitor(parentkey));
                policy.AbsoluteExpiration = DateTime.Now.AddYears(1);
                cache.Add(childkey, DateTime.Now, policy);
            }

            DateTime dt = (DateTime)cache[childkey];

            ViewBag.Message = string.Format("'{0}' was set at {1}", childkey, dt.ToLongTimeString());

            return View();
        }

        public ActionResult Invalidate()
        {
            CacheInvalidation.RedisBus.Invalidate(parentkey);

            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
