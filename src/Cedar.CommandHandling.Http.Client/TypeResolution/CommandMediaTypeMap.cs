namespace Cedar.CommandHandling.Http.TypeResolution
{
    using System;
    using System.Collections.Generic;

    public class CommandMediaTypeMap
    {
        private readonly CommandMediaTypeFormatter _commandMediaTypeFormatter;
        private readonly Dictionary<CommandNameAndVersion, Type> _mediaTypeToCommandType 
            = new Dictionary<CommandNameAndVersion, Type>(CommandNameAndVersion.CommandNameAndVersionComparer); 
        private readonly Dictionary<Type, CommandNameAndVersion> _commandTypeToMediaType
            = new Dictionary<Type, CommandNameAndVersion>();

        public CommandMediaTypeMap(CommandMediaTypeFormatter commandMediaTypeFormatter)
        {
            _commandMediaTypeFormatter = commandMediaTypeFormatter;
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
            var parsedMediaType = _commandMediaTypeFormatter.Parse(mediaType);
            var key = new CommandNameAndVersion(parsedMediaType.CommandName, parsedMediaType.Version);
            return _mediaTypeToCommandType[key];
        }

        public string GetMediaType(Type commandType, string serializationType = "json")
        {
            var commandNameAndVersion = _commandTypeToMediaType[commandType];
            return _commandMediaTypeFormatter.GetMediaType(commandNameAndVersion, serializationType);
        }
    }
}
