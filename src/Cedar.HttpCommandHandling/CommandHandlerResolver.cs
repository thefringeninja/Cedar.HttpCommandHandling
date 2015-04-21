namespace Cedar.HttpCommandHandling
{
    using System;
    using System.Collections.Generic;
    using Cedar.CommandHandling;

    public class CommandHandlerResolver : ICommandHandlerResolver
    {
        private readonly HashSet<Type> _knownCommandTypes = new HashSet<Type>();
        private readonly Dictionary<Type, object> _handlers = new Dictionary<Type, object>(); 

        public CommandHandlerResolver(params CommandHandlerModule[] commandHandlerModules)
        {
            foreach(var module in commandHandlerModules)
            {
                foreach(var handlerRegistration in module.HandlerRegistrations)
                {
                    if (!_knownCommandTypes.Add(handlerRegistration.MessageType))
                    {
                        throw new InvalidOperationException(
                            "Attempt to register multiple handlers for command type {0}"
                                .FormatWith(handlerRegistration.MessageType));
                    }
                    _handlers[handlerRegistration.RegistrationType] = handlerRegistration.HandlerInstance;
                }
            }
        }

        public IEnumerable<Type> KnownCommandTypes
        {
            get { return _knownCommandTypes; }
        }

        public Handler<CommandMessage<TCommand>> Resolve<TCommand>() where TCommand : class
        {
            object handler;
            if(_handlers.TryGetValue(typeof(Handler<CommandMessage<TCommand>>), out handler))
            {
                return (Handler<CommandMessage<TCommand>>) handler;
            }
            return null;
        }
    }
}