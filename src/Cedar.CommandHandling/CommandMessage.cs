namespace Cedar.CommandHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a command with associated metadata
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    public sealed class CommandMessage<TCommand> 
    {
        public readonly TCommand Command;
        public readonly Guid CommandId;
        public readonly IDictionary<string, object> Metadata = new Dictionary<string, object>();

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
            CommandId = commandId;
            Command = command;
            if(metadata != null)
            {
                Metadata = metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }
    }
}