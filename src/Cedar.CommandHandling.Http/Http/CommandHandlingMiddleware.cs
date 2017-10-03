namespace Cedar.CommandHandling.Http
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Cedar.CommandHandling.Http.Logging;
    using Cedar.CommandHandling.Http.TypeResolution;
    using EnsureThat;
    using Microsoft.IO;
    using Microsoft.Owin;
    using Microsoft.Owin.Builder;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using Owin;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>;

    /// <summary>
    ///     Middleware to handle commands.
    /// </summary>
    public static class CommandHandlingMiddleware
    {
        private static readonly RecyclableMemoryStreamManager s_StreamManager = new RecyclableMemoryStreamManager();
        private static readonly ILog s_logger = LogProvider.GetLogger(typeof(CommandHandlingMiddleware));
        private static readonly JsonSerializer s_Serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        /// <summary>
        ///     Creats a command handling middlware.
        /// </summary>
        /// <param name="settings">Settings to configure the middleware.</param>
        /// <returns>An owin middleware function (MidFunc) that represents </returns>
        public static MidFunc HandleCommands(CommandHandlingSettings settings)
        {
            Ensure.That(settings, "settings").IsNotNull();

            var builder = new AppBuilder()
                .MapWhen(IsPuttingCommand, inner => inner.Use(HandleCommand(settings)));

            return next =>
            {
                builder.Run(ctx => next(ctx.Environment));
                return builder.Build();
            };
        }

        private static bool IsPuttingCommand(IOwinContext context)
            => context.Request.Method == "PUT" && TryParseCommandId(context.Request, out var _);

        private static bool TryParseCommandId(IOwinRequest request, out Guid value)
            => Guid.TryParse(request.Path.Value?.Remove(0, 1), out value);

        private static MidFunc HandleCommand(CommandHandlingSettings settings) => next => async env =>
        {
            var context = new OwinContext(env);

            var onPredispatch = settings.OnPredispatch ?? DefaultOnPredispatch;

            TryParseCommandId(context.Request, out var commandId);

            try
            {
                var commandType = ResolveCommandType(context.Request.ContentType, settings.ResolveCommandType);

                var command = await DeserializeCommand(context.Request.Body, commandType, settings.DeserializeCommand);

                var metadata = new Dictionary<string, object>
                {
                    [CommandMessageExtensions.UserKey] = context.Request.User as ClaimsPrincipal
                                                         ?? new ClaimsPrincipal(new ClaimsIdentity())
                };

                try
                {
                    onPredispatch(metadata, context.Request.Headers);
                }
                catch(Exception ex)
                {
                    s_logger.ErrorException("Exception occured invoking the Predispatch hook", ex);
                }

                await settings.HandlerResolver.Dispatch(commandId, command, metadata, context.Request.CallCancelled);
            }
            catch(Exception ex)
            {
                var httpProblemDetailException = ex as IHttpProblemDetailException;

                var problemDetails = httpProblemDetailException?.ProblemDetails
                                     ?? settings.MapProblemDetailsFromException(ex);

                if(problemDetails == null)
                {
                    context.Response.StatusCode = 500;
                    context.Response.ReasonPhrase = "Internal Server Error";
                    return;
                }

                await HandleHttpProblemDetails(context, problemDetails, httpProblemDetailException);
            }

            void DefaultOnPredispatch(
                IDictionary<string, object> _,
                IEnumerable<KeyValuePair<string, string[]>> __)
            { }
        };

        private static Type ResolveCommandType(string mediaType, ResolveCommandType resolveCommandType)
        {
            try
            {
                return resolveCommandType(mediaType);
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

        private static async Task<object> DeserializeCommand(
            Stream body,
            Type commandType,
            DeserializeCommand deserializeCommand)
        {
            var memoryStream = s_StreamManager.GetStream(); // StreamReader below will do the dispose
            await body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            using(var reader = new StreamReader(memoryStream))
            {
                return deserializeCommand(reader, commandType);
            }
        }

        private static async Task HandleHttpProblemDetails(IOwinContext context, HttpProblemDetails problemDetails, IHttpProblemDetailException ex = null)
        {
            var problemDetailsType = problemDetails.GetType();
            
            var exceptionTypeName = ex?.GetType().AssemblyQualifiedName
                ?? typeof(HttpProblemDetailsException<>).MakeGenericType(problemDetailsType).AssemblyQualifiedName;

            // .NET Client will use these custom headers to deserialize and activate the correct types
            context.Response.Headers.Append(CommandClient.HttpProblemDetailsExceptionClrType, exceptionTypeName);
            context.Response.Headers.Append(CommandClient.HttpProblemDetailsClrType, problemDetailsType.AssemblyQualifiedName);
            
            context.Response.StatusCode = problemDetails.Status;
            context.Response.ContentType = HttpProblemDetails.MediaTypeHeaderValue.ToString();
            
            var writer = new JsonTextWriter(new StreamWriter(context.Response.Body));
            
            await JObject.FromObject(problemDetails, s_Serializer).WriteToAsync(writer, context.Request.CallCancelled);
            await writer.FlushAsync(context.Request.CallCancelled);
        }
    }
}