namespace Cedar.CommandHandling.Http
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.CommandHandling.Http.Logging;
    using Cedar.CommandHandling.Http.TypeResolution;
    using EnsureThat;

    public delegate string ResolveMediaType(Type commandType, string serializationType);

    public static class CommandClient
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(CommandClient).Name);
        private static readonly SerializeCommand DefaultSerializeCommand = (textWriter, command) =>
        {
            // Will be problem for LOH if json string is > 85KB
            string commandJson = SimpleJson.SerializeObject(command, JsonSerializerStrategy);
            textWriter.Write(commandJson);
        };
        internal static readonly ProductInfoHeaderValue UserAgent;
        internal static readonly IJsonSerializerStrategy JsonSerializerStrategy = new CamelCasingSerializerStrategy();
        internal const string HttpProblemDetailsClrType = "Cedar-CommandHandling-HttpProblemDetails-ClrType";
        internal const string HttpProblemDetailsExceptionClrType = "Cedar-CommandHandling-HttpProblemDetailsException-ClrType";

        static CommandClient()
        {
            var type = typeof(CommandClient);
            var assemblyVersion = type.GetAssembly().GetName().Version;
            string version = "{0}.{1}".FormatWith(assemblyVersion.Major, assemblyVersion.Minor);
            UserAgent = new ProductInfoHeaderValue(type.FullName, version);
        }

        public static Task PutCommand(
            this HttpClient client,
            object command,
            Guid commandId,
            CommandMediaTypeMap commandMediaTypeMap,
            string basePath = null,
            Action<HttpRequestMessage> customizeRequest = null,
            SerializeCommand serializeCommand = null)
        {
            Ensure.That(client, "client").IsNotNull();
            Ensure.That(command, "command").IsNotNull();
            Ensure.That(commandId, "commandId").IsNotEmpty();
            Ensure.That(commandMediaTypeMap, "commandMediaTypeMap").IsNotNull();

            return PutCommand(
                client,
                command,
                commandId,
                commandMediaTypeMap.GetMediaType,
                basePath,
                customizeRequest,
                serializeCommand);
        }

        public static async Task PutCommand(
            this HttpClient client,
            object command,
            Guid commandId,
            ResolveMediaType resolveMediaType,
            string basePath = null,
            Action<HttpRequestMessage> customizeRequest = null,
            SerializeCommand serializeCommand = null)
        {
            Ensure.That(client, "client").IsNotNull();
            Ensure.That(command, "command").IsNotNull();
            Ensure.That(commandId, "commandId").IsNotEmpty();
            Ensure.That(resolveMediaType, "resolveMediaType").IsNotNull();

            basePath = basePath ?? string.Empty;
            var request = CreatePutCommandRequest(command, commandId, basePath, resolveMediaType, serializeCommand);
            if(customizeRequest != null)
            {
                customizeRequest(request);
            }

            Logger.InfoFormat("Put Command {0}. Type: {1}", commandId, command.GetType());
            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            Logger.InfoFormat("Put Command {0}. Response: {1}", commandId, response.ReasonPhrase);

            await response.EnsureCommandSuccess();
        }

        public static HttpRequestMessage CreatePutCommandRequest(
            object command,
            Guid commandId,
            string basePath,
            ResolveMediaType resolveMediaType,
            SerializeCommand serializeCommand = null)
        {
            Ensure.That(command, "command").IsNotNull();
            Ensure.That(commandId, "commandId").IsNotEmpty();
            Ensure.That(resolveMediaType).IsNotNull();

            serializeCommand = serializeCommand ?? DefaultSerializeCommand;
            using(var writer = new StringWriter())
            {
                serializeCommand(writer, command);

                var httpContent = new StringContent(writer.ToString());
                var mediaType = resolveMediaType(command.GetType(), "json");
                httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

                var request = new HttpRequestMessage(HttpMethod.Put, basePath + "/{0}".FormatWith(commandId))
                {
                    Content = httpContent
                };
                request.Headers.UserAgent.Add(UserAgent);
                request.Headers.Accept.Add(HttpProblemDetails.MediaTypeWithQualityHeaderValue);

                return request;
            }
        }

        public static async Task EnsureCommandSuccess(this HttpResponseMessage response)
        {
            if ((int)response.StatusCode >= 400
                && response.Content.Headers.ContentType != null
                && response.Content.Headers.ContentType.Equals(HttpProblemDetails.MediaTypeHeaderValue))
            {
                Ensure.That(response, "response").IsNotNull();

                // Extract problem details, if they are supplied.
                var body = await response.Content.ReadAsStringAsync();
                var problemDetailsClrType = response
                    .Headers
                    .Single(kvp => kvp.Key == HttpProblemDetailsClrType)
                    .Value
                    .Single();
                var exceptionClrType = response
                    .Headers
                    .Single(kvp => kvp.Key == HttpProblemDetailsExceptionClrType)
                    .Value
                    .Single();

                var problemDetailsType = GetType(problemDetailsClrType);
                var problemDetails = SimpleJson.DeserializeObject(body, problemDetailsType, JsonSerializerStrategy);

                var exceptionType = GetType(exceptionClrType);
                var exception = (Exception)Activator.CreateInstance(exceptionType, problemDetails);

                throw exception;
            }

            response.EnsureSuccessStatusCode();
        }

        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if(type == null)
            {
                throw new TypeLoadException("Failed to get type {0}".FormatWith(typeName));
            }
            return type;
        }

        private class CamelCasingSerializerStrategy : PocoJsonSerializerStrategy
        {
            protected override string MapClrMemberNameToJsonFieldName(string clrPropertyName)
            {
                return clrPropertyName.ToCamelCase();
            }
        }
    }
}