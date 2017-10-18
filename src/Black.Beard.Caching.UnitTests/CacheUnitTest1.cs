using Black.Beard.Caching.Runtime;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Black.Beard.Caching.UnitTests
{
    [TestClass]
    public class CacheUnitTest1
    {


        /// <summary>
        /// check that in multithread the fetcher is called just one time.
        /// </summary>
        [TestMethod]
        public void Concurrency_Fetcher_UnitTest()
        {

            string key1 = "key1";
            int count = 0;
            object _lock = new object();
            string expected = "test";            
            RuntimeLocalCache cache = CreateCache(new SystemClock());

            Func<object> fnc = () =>
            {

                Thread.Sleep(5);

                lock (_lock)
                    count++;

                return expected;

            };

            Task.WaitAll(

                Task.Run(() => cache.GetOrResolve(key1, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key1, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key1, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key1, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key1, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key1, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key1, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key1, fnc, null))

            );

            var actual = cache.Get(key1);

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(count, 1);

        }


        /// <summary>
        /// check that two keys can be concurrency initalized
        /// </summary>
        [TestMethod]
        public void Concurrency_Fetcher_UnitTest2()
        {

            string key1 = "key1";
            string key2 = "key2";
            int count = 0;
            int max = 0;
            object _lock = new object();
            string expected = "test";
            RuntimeLocalCache cache = CreateCache(new SystemClock());

            Func<object> fnc = () =>
            {

                lock (_lock)
                    count++;

                max = Math.Max(max, count);
                Thread.Sleep(5);
                max = Math.Max(max, count);

                lock (_lock)
                    count--;

                return expected;

            };

            Task.WaitAll(

                Task.Run(() => cache.GetOrResolve(key1, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key2, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key1, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key2, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key1, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key2, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key1, fnc, null)),
                Task.Run(() => cache.GetOrResolve(key2, fnc, null))

            );

            Assert.AreEqual(expected, cache.Get(key1));
            Assert.AreEqual(expected, cache.Get(key2));
            Assert.AreEqual(max, 2);

        }

        /// <summary>
        /// check that two keys can be concurrency initialized
        /// </summary>
        [TestMethod]
        public void Duration_UnitTest()
        {

            string key1 = "key1";
            string expected = "test";
            string myPolicy = "myPolicy";
            var clock = new SystemClock();
            RuntimeLocalCache cache = CreateCache(new SystemClock());

            cache.Policies.Add(new Bb.Caching.CachePolicy() { Name = myPolicy, IsDefault = true, Duration = 10, Mode = Bb.Caching.PolicyMode.Default });

            Func<object> fnc = () =>
            {
                return expected;
            };


            cache.GetOrResolve(key1, fnc, myPolicy);

            Assert.AreEqual(expected, cache.Get(key1));

            clock.UtcNow.Add(TimeSpan.FromMinutes(1));


        }

        private RuntimeLocalCache CreateCache(ISystemClock clock)
        {            
            return new RuntimeLocalCache(new MemoryCache(new MemoryCacheOptions() { Clock = clock, }));
        }

    }
}
