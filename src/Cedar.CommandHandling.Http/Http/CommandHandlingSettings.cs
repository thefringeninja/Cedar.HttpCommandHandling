namespace Cedar.CommandHandling.Http
{
    using System;
    using Cedar.CommandHandling.Http.Properties;
    using Cedar.CommandHandling.Http.TypeResolution;
    using CuttingEdge.Conditions;

    public class CommandHandlingSettings
    {
        private readonly ICommandHandlerResolver _handlerResolver;
        private readonly ResolveCommandType _resolveCommandType;
        private DeserializeCommand _deserializeCommand;
        private MapProblemDetailsFromException _mapProblemDetailsFromException;
        private ParseMediaType _parseMediaType = MediaTypeParsers.AllCombined;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandHandlingSettings"/> class using
        ///     <see cref="CommandTypeResolvers.FullNameWithUnderscoreVersionSuffix"/> as the command type resolver.
        /// </summary>
        /// <param name="handlerResolver">The handler resolver.</param>
        public CommandHandlingSettings([NotNull] ICommandHandlerResolver handlerResolver)
            : this(
                handlerResolver,
                CommandTypeResolvers.FullNameWithUnderscoreVersionSuffix(handlerResolver.KnownCommandTypes))
        {}

        public CommandHandlingSettings(
            [NotNull] ICommandHandlerResolver handlerResolver,
            [NotNull] ResolveCommandType resolveCommandType)
        {
            Condition.Requires(handlerResolver, "handlerResolver").IsNotNull();
            Condition.Requires(resolveCommandType, "ResolveCommandType").IsNotNull();

            _handlerResolver = handlerResolver;
            _resolveCommandType = resolveCommandType;
            _deserializeCommand = CatchDeserializationExceptions(
                (body, type) => SimpleJson.DeserializeObject(body, type, CommandClient.JsonSerializerStrategy));
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

        public ParseMediaType ParseMediaType
        {
            get { return _parseMediaType; }
            set
            {
                Condition.Requires(value, "value").IsNotNull();
                _parseMediaType = value;
            }
        }

        public Predispatch OnPredispatch { get; set; }

        public DeserializeCommand DeserializeCommand
        {
            get { return _deserializeCommand; }
            set
            {
                Condition.Requires(value, "value").IsNotNull();
                _deserializeCommand = CatchDeserializationExceptions(value);
            }
        }

        private static DeserializeCommand CatchDeserializationExceptions(DeserializeCommand deserializeCommand)
        {
            return (commandString, type) =>
            {
                try
                {
                    return deserializeCommand(commandString, type);
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