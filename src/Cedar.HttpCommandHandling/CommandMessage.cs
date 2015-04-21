namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    /// Represents a command with associated metadata
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    public class CommandMessage<TCommand> 
    {
        private readonly TCommand _command;
        private readonly Guid _commandId;
        private readonly IDictionary<string, object> _metadata = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandMessage{TCommand}"/> class.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        /// <param name="command">The command.</param>
        /// <param name="metadata">The metadata.</param>
        public CommandMessage(
            Guid commandId,
            TCommand command,
            IDictionary<string, object> metadata = null)
        {
            _commandId = commandId;
            _command = command;
            if(metadata != null)
            {
                _metadata = metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        public IDictionary<string, object> Metadata
        {
            get { return _metadata; }
        }

        public Guid CommandId
        {
            get { return _commandId; }
        }

        public TCommand Command
        {
            get { return _command; }
        }
    }

    public static class CommandMessageExtensions
    {
        private const string UserKey = "Cedar.HttpCommandHandling#User";

        public static T Get<T>(this IDictionary<string, object> dictionary, string key)
        {
            object value;
            if(dictionary.TryGetValue(key, out value))
            {
                return (T) value;
            }
            return default(T);
        }

        public static ClaimsPrincipal GetUser<T>(this CommandMessage<T> commandMessage)
        {
            return commandMessage.Metadata.Get<ClaimsPrincipal>(UserKey);
        }

        public static CommandMessage<T> SetUser<T>(this CommandMessage<T> commandMessage, ClaimsPrincipal user)
        {
            commandMessage.Metadata[UserKey] = user;
            return commandMessage;
        }
    }
}