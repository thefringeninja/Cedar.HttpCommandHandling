namespace Cedar.CommandHandling.Http
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Cedar.CommandHandling;
    using Cedar.CommandHandling.Http.Logging;
    using Cedar.CommandHandling.Http.Properties;
    using Cedar.CommandHandling.Http.TypeResolution;

    internal class CommandHandlerController : ApiController
    {
        private static readonly MethodInfo DispatchCommandMethodInfo = typeof(CommandHandlerController)
            .GetMethod("DispatchCommand", BindingFlags.Static | BindingFlags.NonPublic);
        //private Dictionary<Type, Func<Guid, ClaimsPrincipal>> 

        private readonly CommandHandlingSettings _settings;
        private readonly Predispatch _predispatch;
        private static ILog Logger = LogProvider.For<CommandHandlerController>();

        public CommandHandlerController(CommandHandlingSettings settings)
        {
            _settings = settings;

            var temp = _settings.OnPredispatch ?? ((_, __) => { });
            _predispatch = (metadata, headers) =>
            {
                metadata[CommandMessageExtensions.UserKey] = (User as ClaimsPrincipal) ?? new ClaimsPrincipal(new ClaimsIdentity());
                try
                {
                    temp(metadata, headers);
                }
                catch(Exception ex)
                {
                    Logger.ErrorException("Exception occured invoking the Predispatch hook", ex);
                }
            };
        }

        [Route("{commandId}")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutCommand(Guid commandId, CancellationToken cancellationToken)
        {
            IParsedMediaType parsedMediaType = ParseMediaType();
            Type commandType = ResolveCommandType(parsedMediaType);
            if(!string.Equals(parsedMediaType.SerializationType, "json", StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            object command = await DeserializeCommand(commandType);
            MethodInfo dispatchCommandMethod = DispatchCommandMethodInfo.MakeGenericMethod(command.GetType());
            
            Func<Task> func = async () => await ((Task)dispatchCommandMethod.Invoke(null,
               new[]
                {
                    _settings.HandlerResolver,
                    commandId,
                    command,
                    _predispatch,
                    Request.Headers,
                    cancellationToken
                })).NotOnCapturedContext();

            await func();

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private IParsedMediaType ParseMediaType()
        {
            string mediaType = Request.Content.Headers.ContentType.MediaType;
            IParsedMediaType parsedMediaType = _settings.ParseMediaType(mediaType);
            if (parsedMediaType == null)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            return parsedMediaType;
        }

        private Type ResolveCommandType(IParsedMediaType parsedMediaType)
        {
            Type commandType = _settings.ResolveCommandType(parsedMediaType.TypeName, parsedMediaType.Version);
            if (commandType == null)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            return commandType;
        }

        private async Task<object> DeserializeCommand(Type commandType)
        {
            var commandString = await Request.Content.ReadAsStringAsync();
            return SimpleJson.DeserializeObject(commandString, commandType, CommandClient.JsonSerializerStrategy);
        }

        [UsedImplicitly]
        private static async Task DispatchCommand<TCommand>(
            ICommandHandlerResolver handlerResolver,
            Guid commandId,
            TCommand command,
            Predispatch predispatch,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders,
            CancellationToken cancellationToken)
            where TCommand : class
        {
            var commandMessage = new CommandMessage<TCommand>(commandId, command);
            predispatch(commandMessage.Metadata, requestHeaders);
            await handlerResolver.Resolve<TCommand>()(commandMessage, cancellationToken);
        }
    }
}