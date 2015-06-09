namespace Cedar.CommandHandling.TypeResolution
{
    using Cedar.CommandHandling.Http.TypeResolution;
    using FluentAssertions;
    using Xunit;

    public class MediaTypeParserTests
    {
        [Fact]
        public void Can_parse_with_dot_version()
        {
            var parsedMediaType = MediaTypeParsers.MediaTypeWithDotVersion("application/vnd.command.v2+json");

            parsedMediaType.CommandName.Should().Be("command");
            parsedMediaType.Version.Should().Be(2);
            parsedMediaType.SerializationType.Should().Be("json");
        }

        [Fact]
        public void Can_parse_with_minuse_version()
        {
            var parsedMediaType = MediaTypeParsers.MediaTypeWithMinusVersion("application/vnd.command-v2+json");

            parsedMediaType.CommandName.Should().Be("command");
            parsedMediaType.Version.Should().Be(2);
            parsedMediaType.SerializationType.Should().Be("json");
        }

        [Fact]
        public void Can_parse_with_qualifier_version()
        {
            var parsedMediaType = MediaTypeParsers.MediaTypeWithQualifierVersion("application/vnd.command+json;v=2");

            parsedMediaType.CommandName.Should().Be("command");
            parsedMediaType.Version.Should().Be(2);
            parsedMediaType.SerializationType.Should().Be("json");
        }

        [Fact]
        public void Can_parse_with_no_version()
        {
            var parsedMediaType = MediaTypeParsers.MediaTypeWithoutVersion("application/vnd.command+json");

            parsedMediaType.CommandName.Should().Be("command");
            parsedMediaType.Version.Should().Be(null);
            parsedMediaType.SerializationType.Should().Be("json");
        }
    }
}