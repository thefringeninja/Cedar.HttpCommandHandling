namespace Cedar.CommandHandling.Http
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Cedar.CommandHandling.Http.Logging;
    using Cedar.CommandHandling.Http.TypeResolution;
    using Microsoft.IO;

    internal class CommandHandlerController : ApiController
    {
        private static readonly ILog s_logger = LogProvider.For<CommandHandlerController>();
        private readonly CommandHandlingSettings _settings;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly Predispatch _predispatch;

        public CommandHandlerController(
            CommandHandlingSettings settings,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager)
        {
            _settings = settings;
            _recyclableMemoryStreamManager = recyclableMemoryStreamManager;

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
                    s_logger.ErrorException("Exception occured invoking the Predispatch hook", ex);
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

            var metadata = new Dictionary<string, object>();
            _predispatch(metadata, Request.Headers);
            await _settings.HandlerResolver.Dispatch(commandId, command, metadata, cancellationToken);

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
            var memoryStream = _recyclableMemoryStreamManager.GetStream(); // StreamReader below will do the dispose
            await Request.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            using (var reader = new StreamReader(memoryStream))
            {
                return _settings.DeserializeCommand(reader, commandType);
            }
        }
    }
}
