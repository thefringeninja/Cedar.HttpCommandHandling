namespace Cedar.CommandHandling.Http
{
    using System;
    using Cedar.CommandHandling.Http.Properties;
    using Cedar.CommandHandling.Http.TypeResolution;
    using EnsureThat;

    public class CommandHandlingSettings
    {
        private readonly ICommandHandlerResolver _handlerResolver;
        private readonly ResolveCommandType _resolveCommandType;
        private DeserializeCommand _deserializeCommand;
        private MapProblemDetailsFromException _mapProblemDetailsFromException;

        public CommandHandlingSettings(
            [NotNull] ICommandHandlerResolver handlerResolver,
            [NotNull] CommandMediaTypeMap commandMediaTypeMap)
            : this(handlerResolver, commandMediaTypeMap.GetCommandType)
        {}

        public CommandHandlingSettings(
            [NotNull] ICommandHandlerResolver handlerResolver,
            [NotNull] ResolveCommandType resolveCommandType)
        {
            Ensure.That(handlerResolver, "handlerResolver").IsNotNull();
            Ensure.That(resolveCommandType, "ResolveCommandType").IsNotNull();

            _handlerResolver = handlerResolver;
            _resolveCommandType = resolveCommandType;
            _deserializeCommand = CatchDeserializationExceptions(
                (commandReader, type) =>
                {
                    var body = commandReader.ReadToEnd(); // Will cause LOH problems if command json > 85KB
                    return SimpleJson.DeserializeObject(body, type, CommandClient.JsonSerializerStrategy);
                });
        }

        public MapProblemDetailsFromException MapProblemDetailsFromException
        {
            get
            {
                if(_mapProblemDetailsFromException == null)
                {
                    return _ => null;
                }
                return _mapProblemDetailsFromException;
            }
            set { _mapProblemDetailsFromException = value; }
        }

        public ICommandHandlerResolver HandlerResolver
        {
            get { return _handlerResolver; }
        }

        public ResolveCommandType ResolveCommandType
        {
            get { return _resolveCommandType; }
        }

        public Predispatch OnPredispatch { get; set; }

        /// <summary>
        /// Gets or sets the deserialize command delegate for custom deserialization. NOTE: if you expect that you 
        /// may have commands whose JSON representation exceeds 85KB, it is highly recommended that you use set this
        /// using Newtonsoft.Json or similar.
        /// </summary>
        /// <value>
        /// The deserialize command.
        /// </value>
        public DeserializeCommand DeserializeCommand
        {
            get { return _deserializeCommand; }
            set
            {
                Ensure.That(value, "value").IsNotNull();
                _deserializeCommand = CatchDeserializationExceptions(value);
            }
        }

        private static DeserializeCommand CatchDeserializationExceptions(DeserializeCommand deserializeCommand)
        {
            return (commandReader, type) =>
            {
                try
                {
                    return deserializeCommand(commandReader, type);
                }
                catch(Exception ex)
                {
                    if(ex is IHttpProblemDetailException)
                    {
                        throw;
                    }
                    throw new HttpProblemDetailsException<HttpProblemDetails>(new HttpProblemDetails
                    {
                        Title = "Error occured deserializing command.",
                        Status = 400,
                        Detail = ex.Message
                    });
                }
            };
        }
    }
}