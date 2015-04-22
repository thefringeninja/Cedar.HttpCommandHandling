namespace Cedar.CommandHandling
{
    using Cedar.CommandHandling.Http;
    using Cedar.CommandHandling.Http.TypeResolution;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class CommandHandlingSettingsTests
    {
        [Fact]
        public void Can_set_ParseMediaType()
        {
            var sut = new CommandHandlingSettings(A.Fake<ICommandHandlerResolver>(), A.Fake<ResolveCommandType>());
            var parseMediaType = A.Fake<ParseMediaType>();

            sut.ParseMediaType = parseMediaType;

            sut.ParseMediaType.Should().Be(parseMediaType);
        }
    }
}