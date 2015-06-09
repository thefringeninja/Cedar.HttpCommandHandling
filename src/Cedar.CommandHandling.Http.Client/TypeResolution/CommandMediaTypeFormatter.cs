namespace Cedar.CommandHandling.Http.TypeResolution
{
    public abstract class CommandMediaTypeFormatter
    {
        public readonly string Prefix;

        public CommandMediaTypeFormatter(string prefix)
        {
            Prefix = prefix;
        }

        public abstract string GetMediaType(CommandNameAndVersion commandNameAndVersion, string serializationType);

        public abstract ParsedMediaType Parse(string mediaType);
    }
}