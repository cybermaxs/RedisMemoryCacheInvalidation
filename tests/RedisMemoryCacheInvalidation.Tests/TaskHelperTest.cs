using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RedisMemoryCacheInvalidation.Tests
{
    [TestClass]
    public class TaskHelperTest
    {
        [TestMethod]
        public void TaskHelper_Delayed_ShouldWaitDelay()
        {
            var watcher = Stopwatch.StartNew();
            TaskHelper.Delay(TimeSpan.FromSeconds(5)).ContinueWith(t =>
                {
                    watcher.Stop();
                }).Wait();

            Assert.IsTrue( Math.Abs(5000- watcher.ElapsedMilliseconds)/5000 < 0.1, watcher.ElapsedMilliseconds.ToString() );
        }
    }
}
