using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using SparkSharp;

namespace Test
{
    [TestFixture]
    public class CosmosDbLivySessionPoolTest
    {
        [Test]
        public async Task GetSessionAlwaysReturnOneOfFiveSessions()
        {
            var livyClient = Substitute.For<ILivyClient>();
            
            var livySession1 = Substitute.For<ILivySession>();
            var livySession2 = Substitute.For<ILivySession>();
            var livySession3 = Substitute.For<ILivySession>();
            var livySession4 = Substitute.For<ILivySession>();
            var livySession5 = Substitute.For<ILivySession>();

            livyClient.CreateSessionAsync(Arg.Any<LivySessionConfiguration>())
                .Returns(Task.FromResult(livySession1),
                         Task.FromResult(livySession2),
                         Task.FromResult(livySession3),
                         Task.FromResult(livySession4),
                         Task.FromResult(livySession5),
                         null);

            var pool = new CosmosDbLivySessionPool(livyClient, new CosmosCollectionSettings(), LivySessionConfiguration.Default, 5);

            var result = await Enumerable.Repeat(0, 100).Select(_ => pool.GetSessionAsync());

            Assert.That(result.Distinct().Count(), Is.EqualTo(5));
        }

        [Test]
        public async Task OnceSessionReleasedGetSessionWillQueueWorkOnIt()
        {
            var livyClient = Substitute.For<ILivyClient>();
            
            var livySession1 = Substitute.For<ILivySession>();
            var livySession2 = Substitute.For<ILivySession>();
            var livySession3 = Substitute.For<ILivySession>();
            var livySession4 = Substitute.For<ILivySession>();
            var livySession5 = Substitute.For<ILivySession>();

            var source1 = new TaskCompletionSource<bool>();
            var source2 = new TaskCompletionSource<bool>();
            var source3 = new TaskCompletionSource<bool>();
            var source4 = new TaskCompletionSource<bool>();
            var source5 = new TaskCompletionSource<bool>();

            livySession1.WaitForSessionAsync().Returns(source1.Task);
            livySession2.WaitForSessionAsync().Returns(source2.Task);
            livySession3.WaitForSessionAsync().Returns(source3.Task);
            livySession4.WaitForSessionAsync().Returns(source4.Task);
            livySession5.WaitForSessionAsync().Returns(source5.Task);

            livyClient.CreateSessionAsync(Arg.Any<LivySessionConfiguration>())
                .Returns(Task.FromResult(livySession1),
                         Task.FromResult(livySession2),
                         Task.FromResult(livySession3),
                         Task.FromResult(livySession4),
                         Task.FromResult(livySession5),
                         null);

            var pool = new CosmosDbLivySessionPool(livyClient, new CosmosCollectionSettings(), LivySessionConfiguration.Default, 5);

            async void WaitAndReleaseSession3()
            {
                await Task.Delay(100);
                source3.SetResult(true);
            }

            WaitAndReleaseSession3();

            var result = await Enumerable.Repeat(0, 100).Select(_ => pool.GetSessionAsync());

            var sessions = result.GroupBy(s => s).Select(g => new { g.Key, Count = g.Count() });

            ILivySession GetSession(ISparkSqlSession sqlSession) => 
                ((Lazy<Task<ILivySession>>)
                typeof(CosmosDbLivySession).GetField("_session", BindingFlags.NonPublic | BindingFlags.Instance)?
                                           .GetValue(sqlSession))?.Value.Result;

            var enumerable = sessions.Single(g => GetSession(g.Key) == livySession3);

            Assert.That(enumerable.Count, Is.EqualTo(96));
        }
    }
}
