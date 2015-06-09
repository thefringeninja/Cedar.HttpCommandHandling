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
            Type commandType = ResolveCommandType(Request.Content.Headers.ContentType.MediaType);
            object command = await DeserializeCommand(commandType);

            var metadata = new Dictionary<string, object>();
            _predispatch(metadata, Request.Headers);
            await _settings.HandlerResolver.Dispatch(commandId, command, metadata, cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private Type ResolveCommandType(string mediaType)
        {
            try
            {
                return _settings.ResolveCommandType(mediaType);
            }
            catch(Exception ex)
            {
                var problemDetails = new HttpProblemDetails
                {
                    Status = (int) HttpStatusCode.UnsupportedMediaType,
                    Detail = ex.Message
                };
                throw new HttpProblemDetailsException<HttpProblemDetails>(problemDetails);
            }
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
