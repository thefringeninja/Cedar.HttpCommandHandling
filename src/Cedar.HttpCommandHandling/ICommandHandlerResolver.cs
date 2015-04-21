namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Collections.Generic;
    using Cedar.CommandHandling;

    public interface ICommandHandlerResolver
    {
        Handler<CommandMessage<TCommand>> Resolve<TCommand>() where TCommand : class;

        IEnumerable<Type> KnownCommandTypes { get; }
    }
}