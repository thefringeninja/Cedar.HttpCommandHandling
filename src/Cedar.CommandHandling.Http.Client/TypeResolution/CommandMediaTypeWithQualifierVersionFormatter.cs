namespace Cedar.CommandHandling.Http.TypeResolution
{
    public class CommandMediaTypeWithQualifierVersionFormatter : CommandMediaTypeFormatter
    {
        public CommandMediaTypeWithQualifierVersionFormatter(string prefix = "application/vnd.")
            : base(prefix)
        {}

        public override string GetMediaType(CommandNameAndVersion commandNameAndVersion, string serializationType)
        {
            if (commandNameAndVersion.Version.HasValue)
            {
                return string.Format("{0}{1}+{2};v={3}",
                    Prefix,
                    commandNameAndVersion.CommandName,
                    serializationType,
                    commandNameAndVersion.Version);
            }
            return string.Format("{0}{1}+{2}",
                Prefix,
                commandNameAndVersion.CommandName,
                serializationType);
        }

        public override ParsedMediaType Parse(string mediaType)
        {
            return MediaTypeParsers.MediaTypeWithQualifierVersion(mediaType)
                   ?? MediaTypeParsers.MediaTypeWithoutVersion(mediaType);
        }
    }
}