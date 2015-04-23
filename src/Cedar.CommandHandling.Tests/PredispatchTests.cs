namespace Cedar.CommandHandling
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cedar.CommandHandling.Http;
    using FluentAssertions;
    using Xunit;

    public class PredispatchTests
    {
        [Fact]
        public async Task Should_invoke_predispatch_hook()
        {
            var module = new CommandHandlerModule();
            string correlationId = null;
            const string correlationIdKey = "CorrelationId";
            module.For<Command>().Handle((commandMessage, __) =>
            {
                correlationId = commandMessage.Metadata.Get<string>(correlationIdKey);
                return Task.FromResult(0);
            });
            var settings = new CommandHandlingSettings(new CommandHandlerResolver(module))
            {
                OnPredispatch = (metadata, headers) =>
                {
                    var correlationIdHeader = headers.SingleOrDefault(kvp => kvp.Key == correlationIdKey);
                    if(correlationIdHeader.Value != null)
                    {
                        metadata[correlationIdKey] = correlationIdHeader.Value.SingleOrDefault();
                    }
                }
            };

            var midFunc = CommandHandlingMiddleware.HandleCommands(settings);

            using(var client = midFunc.CreateEmbeddedClient())
            {
                await client.PutCommand(new Command(), Guid.NewGuid(), customizeRequest: request =>
                {
                    request.Headers.Add(correlationIdKey, "cor-1");
                });

                correlationId.Should().Be("cor-1");
            }
        }
    }
}