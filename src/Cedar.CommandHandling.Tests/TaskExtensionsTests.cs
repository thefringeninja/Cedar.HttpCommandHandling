namespace Cedar.CommandHandling
{
    using System;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    public class TaskExtensionsTests
    {
        [Fact]
        public async Task Should_timeout()
        {
            Func<Task> act = () => Task.Delay(10000).WithTimeout(TimeSpan.FromMilliseconds(1));

            await act.ShouldThrowAsync<TimeoutException>();
        }
    }
}