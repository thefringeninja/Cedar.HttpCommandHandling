namespace Cedar.CommandHandling
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class CommandHandlerResolverTests
    {
        [Fact]
        public void Can_resolve_handler()
        {
            var module = new TestCommandHandlerModule();
            var resolver = new CommandHandlerResolver(module);

            Handler<CommandMessage<Command>> handler = resolver.Resolve<Command>();

            handler.Should().NotBeNull();
        }


        [Fact]
        public async Task Can_dispatch()
        {
            var module = new TestCommandHandlerModule();
            var resolver = new CommandHandlerResolver(module); 

            await resolver.Dispatch(Guid.NewGuid(), new Command());

            module.Counter.Should().Be(1);
        }

        private class TestCommandHandlerModule : CommandHandlerModule
        {
            public int Counter;

            public TestCommandHandlerModule()
            {
                For<Command>()
                    .Handle(_ =>
                    {
                        Counter++;
                    });
            }
        }
    }
}
