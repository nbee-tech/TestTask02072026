using Xunit;
using Services;
using System.Diagnostics;

namespace TestTask02072026.Tests
{
    public class CompressorServiceTests
    {
        [Fact]
        public void Compress_ReturnsExpectedString()
        {
            string result = CompressorService.Compress("aaabbcccdde");
        }
    }

    public class ServerServiceServiceTests
    {
        [Fact]
        public void AddToCount_WhenCalledInParallel_UpdatesSequentially()
        {
            ServerService.Reset();

            int tasksCount = 1000;

            Parallel.For(0, tasksCount, _ =>
            {
                ServerService.AddToCount(1);
            });

            Assert.Equal(1000, ServerService.GetCount());
        }

        [Fact]
        public async Task GetCount_WhenWriterIsActive_WaitsUntilWriterFinishes()
        {
            ServerService.Reset();

            Task writerTask = Task.Run(() =>
            {
                ServerService.AddToCountTest(10);
            });

            await Task.Delay(100); // give writer time to enter write lock

            var stopwatch = Stopwatch.StartNew();

            int count = ServerService.GetCount();

            stopwatch.Stop();

            await writerTask;

            Assert.Equal(10, count);
            Assert.True(stopwatch.ElapsedMilliseconds >= 350);
        }
    }
}


