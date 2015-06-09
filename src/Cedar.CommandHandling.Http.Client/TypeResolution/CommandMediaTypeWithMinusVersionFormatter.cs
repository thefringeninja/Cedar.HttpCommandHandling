namespace Cedar.CommandHandling.Http.TypeResolution
{
    public class CommandMediaTypeWithMinusVersionFormatter : CommandMediaTypeFormatter
    {
        public CommandMediaTypeWithMinusVersionFormatter(string prefix = "application/vnd.")
            : base(prefix)
        {}

        public override string GetMediaType(CommandNameAndVersion commandNameAndVersion, string serializationType)
        {
            if (commandNameAndVersion.Version.HasValue)
            {
                return string.Format("{0}{1}-v{2}+{3}",
                    Prefix,
                    commandNameAndVersion.CommandName,
                    commandNameAndVersion.Version,
                    serializationType);
            }
            return string.Format("{0}{1}+{2}",
                Prefix,
                commandNameAndVersion.CommandName,
                serializationType);
        }

        public override ParsedMediaType Parse(string mediaType)
        {
            return MediaTypeParsers.MediaTypeWithMinusVersion(mediaType)
                   ?? MediaTypeParsers.MediaTypeWithoutVersion(mediaType);
        }
    }
}