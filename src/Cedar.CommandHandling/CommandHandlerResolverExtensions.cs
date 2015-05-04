namespace Cedar.CommandHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public static class CommandHandlerResolverExtensions
    {
        private static readonly MethodInfo s_dispatchInternalMethod = typeof(CommandHandlerResolverExtensions)
            .GetRuntimeMethods()
            .Single(m => m.Name.Equals("DispatchInternal", StringComparison.Ordinal));

        public static async Task Dispatch(
            this ICommandHandlerResolver handlerResolver,
            Guid commandId,
            object command,
            IDictionary<string, object> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            metadata = metadata ?? new Dictionary<string, object>();
            var eventType = command.GetType();
            var dispatchMethod = s_dispatchInternalMethod.MakeGenericMethod(eventType);

            var paramaters = new[]
            {
                handlerResolver, commandId, command, metadata, cancellationToken
            };

            await (Task) dispatchMethod.Invoke(handlerResolver, paramaters);
        }

        private static async Task DispatchInternal<TCommand>(
            ICommandHandlerResolver handlerResolver,
            Guid commandId,
            TCommand command,
            IDictionary<string, object> metadata,
            CancellationToken cancellationToken)
            where TCommand : class
        {
            var commandMessage = new CommandMessage<TCommand>(commandId, command, metadata);
            await handlerResolver.Resolve<TCommand>()(commandMessage, cancellationToken);
        }
    }
}