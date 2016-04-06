namespace Cedar.CommandHandling.TypeResolution
{
    using Cedar.CommandHandling.Http.TypeResolution;
    using Shouldly;
    using Xunit;

    public class CommandMediaTypeWithQualifierVersionFormatterTests
    {
        private readonly CommandMediaTypeWithQualifierVersionFormatter _sut;

        public CommandMediaTypeWithQualifierVersionFormatterTests()
        {
            _sut = new CommandMediaTypeWithQualifierVersionFormatter();
        }

        [Fact]
        public void Can_parse_unversioned_media_type()
        {
            var parsedMediaType = _sut.Parse("application/vnd.command+json");

            parsedMediaType.CommandName.ShouldBe("command");
            parsedMediaType.Version.ShouldBe(null);
            parsedMediaType.SerializationType.ShouldBe("json");
        }

        [Fact]
        public void Can_generate_unversioned_media_type()
        {
            var formatter = new CommandMediaTypeWithDotVersionFormatter();

            string mediaType = formatter.GetMediaType(new CommandNameAndVersion("command"), "xml");

            mediaType.ShouldBe("application/vnd.command+xml");
        }

        [Fact]
        public void Can_parse_versioned_media_type()
        {
            var parsedMediaType = _sut.Parse("application/vnd.command+json;v=2");

            parsedMediaType.CommandName.ShouldBe("command");
            parsedMediaType.Version.ShouldBe(2);
            parsedMediaType.SerializationType.ShouldBe("json");
        }

        [Fact]
        public void Can_generate_versioned_media_type()
        {
            string mediaType = _sut.GetMediaType(new CommandNameAndVersion("command", 2), "xml");

            mediaType.ShouldBe("application/vnd.command+xml;v=2");
        }
    }
}