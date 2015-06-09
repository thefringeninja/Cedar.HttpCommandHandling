namespace Cedar.CommandHandling
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class CommandHandlerModuleTests
    {
        [Fact]
        public void When_add_duplicate_command_then_should_throw()
        {
            var module = new CommandHandlerModule();
            module.For<Command>().Handle((_, __) => Task.FromResult(0));

            Action act = () => module.For<Command>().Handle((_, __) => Task.FromResult(0));

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void Can_get_command_types()
        {
            var module = new CommandHandlerModule();
            
            module.For<Command>().Handle((_,__) => Task.FromResult(0));

            var commandTypes = module.CommandTypes.ToList();

            commandTypes.Should().HaveCount(1);
            commandTypes.Single().Should().Be(typeof(Command));
        }
    }
}