namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Collections.Generic;

    public interface ICommandHandlerResolver
    {
        Handler<CommandMessage<TCommand>> Resolve<TCommand>() where TCommand : class;

        IEnumerable<Type> KnownCommandTypes { get; }
    }
}