namespace Cedar.CommandHandling.Http.TypeResolution
{
    public class CommandMediaTypeWithDotVersionFormatter : CommandMediaTypeFormatter
    {
        public CommandMediaTypeWithDotVersionFormatter(string prefix = "application/vnd.")
            : base(prefix)
        {}

        public override string GetMediaType(CommandNameAndVersion commandNameAndVersion, string serializationType)
        {
            if(commandNameAndVersion.Version.HasValue)
            {
                return string.Format("{0}{1}.v{2}+{3}",
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
            return MediaTypeParsers.MediaTypeWithDotVersion(mediaType) 
                   ?? MediaTypeParsers.MediaTypeWithoutVersion(mediaType);
        }
    }
}