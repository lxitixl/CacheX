using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Internal;
using Moq;
using NUnit.Framework;

namespace CacheX.Test
{
    public class Tests
    {
        private Mock<ISystemClock> _systemClock;
        private CacheX<string, string> _cacheX;

        [SetUp]
        public void Setup()
        {
            _systemClock = new Mock<ISystemClock>();
            _cacheX = new(systemClock: _systemClock.Object);
        }

        [Test]
        public async Task Cache_Not_Expired()
        {
            _systemClock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
            var result1 = await _cacheX.GetOrAdd("key", async () => await Task.FromResult("result1"), TimeSpan.FromMinutes(10));
            Assert.AreEqual("result1", result1);

            _systemClock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow.AddDays(-1));
            var result2 = await _cacheX.GetOrAdd("key", async () => await Task.FromResult("result2"), TimeSpan.FromMinutes(10));
            Assert.AreEqual("result1", result2);
        }

        [Test]
        public async Task Cache_Is_Expired()
        {
            _systemClock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);
            var result1 = await _cacheX.GetOrAdd("key", async () => await Task.FromResult("result1"), TimeSpan.FromMinutes(10));
            Assert.AreEqual("result1", result1);

            _systemClock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow.AddDays(1));
            var result2 = await _cacheX.GetOrAdd("key", async () => await Task.FromResult("result2"), TimeSpan.FromMinutes(10));
            Assert.AreEqual("result2", result2);
        }
    }
}