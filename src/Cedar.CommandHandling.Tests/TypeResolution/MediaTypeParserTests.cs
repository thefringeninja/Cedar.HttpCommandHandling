namespace Cedar.CommandHandling.TypeResolution
{
    using Cedar.CommandHandling.Http.TypeResolution;
    using Shouldly;
    using Xunit;

    public class MediaTypeParserTests
    {
        [Fact]
        public void Can_parse_with_dot_version()
        {
            var parsedMediaType = MediaTypeParsers.MediaTypeWithDotVersion("application/vnd.command.v2+json");

            parsedMediaType.CommandName.ShouldBe("command");
            parsedMediaType.Version.ShouldBe(2);
            parsedMediaType.SerializationType.ShouldBe("json");
        }

        [Fact]
        public void Can_parse_with_minuse_version()
        {
            var parsedMediaType = MediaTypeParsers.MediaTypeWithMinusVersion("application/vnd.command-v2+json");

            parsedMediaType.CommandName.ShouldBe("command");
            parsedMediaType.Version.ShouldBe(2);
            parsedMediaType.SerializationType.ShouldBe("json");
        }

        [Fact]
        public void Can_parse_with_qualifier_version()
        {
            var parsedMediaType = MediaTypeParsers.MediaTypeWithQualifierVersion("application/vnd.command+json;v=2");

            parsedMediaType.CommandName.ShouldBe("command");
            parsedMediaType.Version.ShouldBe(2);
            parsedMediaType.SerializationType.ShouldBe("json");
        }

        [Fact]
        public void Can_parse_with_no_version()
        {
            var parsedMediaType = MediaTypeParsers.MediaTypeWithoutVersion("application/vnd.command+json");

            parsedMediaType.CommandName.ShouldBe("command");
            parsedMediaType.Version.ShouldBe(null);
            parsedMediaType.SerializationType.ShouldBe("json");
        }
    }
}