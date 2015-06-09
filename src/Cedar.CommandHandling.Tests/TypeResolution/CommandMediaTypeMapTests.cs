namespace Cedar.CommandHandling.TypeResolution
{
    using System;
    using Cedar.CommandHandling.Http.TypeResolution;
    using FluentAssertions;
    using Xunit;

    public class CommandMediaTypeMapTests
    {
        private readonly CommandMediaTypeMap _sut;

        public CommandMediaTypeMapTests()
        {
            _sut = new CommandMediaTypeMap(new MediaTypeWithDotVersionFormatter());
        }

        [Fact]
        public void With_unversioned_command_can_get_command_type()
        {
            _sut.Add("command", typeof(Command));

            _sut.GetCommandType("application/vnd.command+json")
                .Should()
                .Be<Command>();
        }

        [Fact]
        public void With_unversioned_media_type_can_get_command_type()
        {
            _sut.Add("command", typeof(Command));

            _sut.GetMediaType(typeof(Command))
                .Should()
                .Be("application/vnd.command+json");
        }

        [Fact]
        public void With_versioned_command_can_get_command_type()
        {
            _sut.Add("command", 2, typeof(Command_v2));

            _sut.GetCommandType("application/vnd.command.v2+json")
                .Should()
                .Be<Command_v2>();
        }

        [Fact]
        public void With_versioned_media_type_can_get_command_type()
        {
            _sut.Add("command", 2, typeof(Command_v2));

            _sut.GetMediaType(typeof(Command_v2))
                .Should()
                .Be("application/vnd.command.v2+json");
        }

        [Fact]
        public void When_add_duplicate_command_type_then_should_throw()
        {
            _sut.Add("command", 2, typeof(Command_v2));

            Action act = () => _sut.Add("commandX", 2, typeof(Command_v2));

            act.ShouldThrowExactly<ArgumentException>();
        }

        [Fact]
        public void When_add_duplicate_media_type_then_should_throw()
        {
            _sut.Add("command", 2, typeof(Command_v2));

            Action act = () => _sut.Add("command", 2, typeof(Command));

            act.ShouldThrowExactly<ArgumentException>();
        }
    }

    public class Command { }


    public class Command_v2 { }
}