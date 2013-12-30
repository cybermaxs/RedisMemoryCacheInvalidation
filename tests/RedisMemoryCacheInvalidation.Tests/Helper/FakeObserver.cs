using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedisMemoryCacheInvalidation.Tests.Helper
{
    public class FakeObserver : IObserver<string>
    {
        public bool CompletedCalled { get; set; }
        public bool ErrorCalled { get; set; }
        public bool NextCalled { get; set; }
        public string NextTopic { get; set; }

        public void OnCompleted()
        {
            CompletedCalled = true;
        }

        public void OnError(Exception error)
        {
            ErrorCalled = true;
        }

        public void OnNext(string value)
        {
            NextCalled = true;
            NextTopic = value;
        }
    }
}
