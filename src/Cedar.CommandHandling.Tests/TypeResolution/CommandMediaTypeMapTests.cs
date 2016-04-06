namespace Cedar.CommandHandling.TypeResolution
{
    using System;
    using Cedar.CommandHandling.Http.TypeResolution;
    using Shouldly;
    using Xunit;

    public class CommandMediaTypeMapTests
    {
        private readonly CommandMediaTypeMap _sut;

        public CommandMediaTypeMapTests()
        {
            _sut = new CommandMediaTypeMap(new CommandMediaTypeWithDotVersionFormatter());
        }

        [Fact]
        public void With_unversioned_command_can_get_command_type()
        {
            _sut.Add("command", typeof(Command));

            _sut.GetCommandType("application/vnd.command+json")
                .ShouldBe(typeof(Command));
        }

        [Fact]
        public void With_unversioned_media_type_can_get_command_type()
        {
            _sut.Add("command", typeof(Command));

            _sut.GetMediaType(typeof(Command))
                .ShouldBe("application/vnd.command+json");
        }

        [Fact]
        public void With_versioned_command_can_get_command_type()
        {
            _sut.Add("command", 2, typeof(Command_v2));

            _sut.GetCommandType("application/vnd.command.v2+json")
                .ShouldBe(typeof(Command_v2));
        }

        [Fact]
        public void With_versioned_media_type_can_get_command_type()
        {
            _sut.Add("command", 2, typeof(Command_v2));

            _sut.GetMediaType(typeof(Command_v2))
                .ShouldBe("application/vnd.command.v2+json");
        }

        [Fact]
        public void When_add_duplicate_command_type_then_should_throw()
        {
            _sut.Add("command", 2, typeof(Command_v2));

            Action act = () => _sut.Add("commandX", 2, typeof(Command_v2));

            act.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void When_add_duplicate_media_type_then_should_throw()
        {
            _sut.Add("command", 2, typeof(Command_v2));

            Action act = () => _sut.Add("command", 2, typeof(Command));

            act.ShouldThrow<ArgumentException>();
        }
    }

    public class Command { }


    public class Command_v2 { }
}