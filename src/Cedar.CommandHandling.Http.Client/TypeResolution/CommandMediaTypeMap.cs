namespace Cedar.CommandHandling.Http.TypeResolution
{
    using System;
    using System.Collections.Generic;

    public abstract class MediaTypeFormatter
    {
        public readonly string Prefix;

        public MediaTypeFormatter(string prefix = "application/vnd.")
        {
            Prefix = prefix;
        }

        public abstract string GetMediaType(CommandNameAndVersion commandNameAndVersion, string serializationType);

        public abstract ParsedMediaType Parse(string mediaType);
    }

    public class MediaTypeWithDotVersionFormatter : MediaTypeFormatter
    {
        public MediaTypeWithDotVersionFormatter(string prefix = "application/vnd.")
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

    public class CommandMediaTypeMap
    {
        private readonly MediaTypeFormatter _mediaTypeFormatter;
        private readonly Dictionary<CommandNameAndVersion, Type> _mediaTypeToCommandType 
            = new Dictionary<CommandNameAndVersion, Type>(CommandNameAndVersion.CommandNameAndVersionComparer); 
        private readonly Dictionary<Type, CommandNameAndVersion> _commandTypeToMediaType
            = new Dictionary<Type, CommandNameAndVersion>();

        public CommandMediaTypeMap(MediaTypeFormatter mediaTypeFormatter)
        {
            _mediaTypeFormatter = mediaTypeFormatter;
        }

        public void Add(string commandName, Type commandType)
        {
            Add(commandName, null, commandType);
        }

        public void Add(string commandName, int? version, Type commandType)
        {
            Add(new CommandNameAndVersion(commandName, version), commandType);
        }

        public void Add(CommandNameAndVersion commandNameAndVersion, Type commandType)
        {
            _mediaTypeToCommandType.Add(commandNameAndVersion, commandType);
            _commandTypeToMediaType.Add(commandType, commandNameAndVersion);
        }

        public Type GetCommandType(string mediaType)
        {
            var parsedMediaType = _mediaTypeFormatter.Parse(mediaType);
            var key = new CommandNameAndVersion(parsedMediaType.CommandName, parsedMediaType.Version);
            return _mediaTypeToCommandType[key];
        }

        public string GetMediaType(Type commandType, string serializationType = "json")
        {
            var commandNameAndVersion = _commandTypeToMediaType[commandType];
            return _mediaTypeFormatter.GetMediaType(commandNameAndVersion, serializationType);
        }
    }

    public class CommandNameAndVersion
    {
        public readonly string CommandName;
        public readonly int? Version;

        public CommandNameAndVersion(string commandName, int? version)
        {
            CommandName = commandName;
            Version = version;
        }

        private sealed class CommandNameAndVersionEqualityComparer : IEqualityComparer<CommandNameAndVersion>
        {
            public bool Equals(CommandNameAndVersion x, CommandNameAndVersion y)
            {
                if(ReferenceEquals(x, y))
                {
                    return true;
                }
                if(ReferenceEquals(x, null))
                {
                    return false;
                }
                if(ReferenceEquals(y, null))
                {
                    return false;
                }
                if(x.GetType() != y.GetType())
                {
                    return false;
                }
                return string.Equals(x.CommandName, y.CommandName) && x.Version == y.Version;
            }

            public int GetHashCode(CommandNameAndVersion obj)
            {
                unchecked
                {
                    return (obj.CommandName.GetHashCode()*397) ^ obj.Version.GetHashCode();
                }
            }
        }

        private static readonly IEqualityComparer<CommandNameAndVersion> s_commandNameAndVersionComparerInstance =
            new CommandNameAndVersionEqualityComparer();

        public static IEqualityComparer<CommandNameAndVersion> CommandNameAndVersionComparer
        {
            get { return s_commandNameAndVersionComparerInstance; }
        }

        public override string ToString()
        {
            return string.Format("CommandName = {0}; Version = {1}", CommandName, Version);
        }
    }
}
