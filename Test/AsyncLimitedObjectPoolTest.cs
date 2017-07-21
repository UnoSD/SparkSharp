using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SparkSharp;

namespace Test
{
    [TestFixture]
    public class AsyncLimitedObjectPoolTest
    {
        [Test]
        public async Task Rent()
        {
            var count = 0;

            var pool = new AsyncLimitedObjectPool<int>(() => Task.FromResult(Interlocked.Increment(ref count)), 1);

            var rented = await pool.RentAsync();

            Assert.That(rented, Is.EqualTo(1));
        }

        [Test]
        public async Task RentTwice()
        {
            var pool = GetPool(2);

            var rented1 = await pool.RentAsync();
            var rented2 = await pool.RentAsync();

            Assert.That(rented1, Is.EqualTo(1));
            Assert.That(rented2, Is.EqualTo(2));
        }

        [Test]
        public async Task RentTwiceOnOneMaxSecondNotAvailable()
        {
            var pool = GetPool(1);

            await pool.RentAsync();

            var rented2 = pool.RentAsync();

            Assert.That(rented2.IsCompleted, Is.False);
        }

        [Test]
        public async Task RentTwiceOnTwoMaxSecondAvailable()
        {
            var pool = GetPool(2);

            await pool.RentAsync();

            var rented2 = pool.RentAsync();

            Assert.That(rented2.IsCompleted, Is.True);
        }

        [Test]
        public async Task RentTwiceOnOneMaxSecondNotAvailableUntilFirstReturned()
        {
            var pool = GetPool(1);

            var rented1 = await pool.RentAsync();

            var rented2 = pool.RentAsync();

            Assert.That(rented2.IsCompleted, Is.False);

            pool.Return(rented1);

            Assert.That(await rented2, Is.EqualTo(1));
        }

        [Test]
        public async Task RentTwiceOnTwoMaxReturnAndRerent()
        {
            var pool = GetPool(2);

            var rented1 = await pool.RentAsync();
            var rented2 = await pool.RentAsync();

            pool.Return(rented1);
            pool.Return(rented2);

            var renteds = await Task.WhenAll(pool.RentAsync().AsTask(), pool.RentAsync().AsTask());

            Assert.That(renteds, Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public async Task RentThreeTimesOnOneMaxFirstTryingToRentIsFirstGettingFreed()
        {
            var pool = GetPool(1);

            var rented1 = await pool.RentAsync();

            var rented2 = pool.RentAsync();
            var rented3 = pool.RentAsync();

            pool.Return(rented1);

            var firstCompleted = await Task.WhenAny(rented2.AsTask(), rented3.AsTask(), Task.Delay(1000));

            Assert.That(rented2.AsTask(), Is.SameAs(firstCompleted));
        }

        static AsyncLimitedObjectPool<int> GetPool(int max)
        {
            var count = 0;

            return new AsyncLimitedObjectPool<int>(() => Task.FromResult(Interlocked.Increment(ref count)), max);
        }
    }
}