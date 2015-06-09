namespace Cedar.CommandHandling.Http.TypeResolution
{
    /// <summary>
    ///     Represents a parsed media type. Example, given a media type 'application/vnd.foo.bar.v2+json'
    ///     an implementation would then result in Typename='foo.bar', Version='2' and SerializationType = 'json'
    /// </summary>
    public class ParsedMediaType
    {
        /// <summary>
        ///     Gets the name of the type as extracted from the media type.
        /// </summary>
        /// <value>
        ///     The name of the type.
        /// </value>
        public readonly string CommandName;

        /// <summary>
        ///     Gets the version of the type as extracted from the media type. If no version
        ///     is parsed, then it will be null.
        /// </summary>
        /// <value>
        ///     The version of the media type.
        /// </value>
        public readonly int? Version;

        /// <summary>
        ///     Gets the serialization type.
        /// </summary>
        public readonly string SerializationType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsedMediaType"/> class.
        /// </summary>
        /// <param name="commandName">The command type name.</param>
        /// <param name="version">The version.</param>
        /// <param name="serializationType">Type of the serialization.</param>
        public ParsedMediaType(string commandName, int? version, string serializationType)
        {
            CommandName = commandName;
            Version = version;
            SerializationType = serializationType;
        }
    }
}