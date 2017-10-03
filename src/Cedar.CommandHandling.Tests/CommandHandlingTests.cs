namespace Cedar.CommandHandling
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cedar.CommandHandling.Http;
    using Shouldly;
    using Xunit;

    public class CommandHandlingTests : IClassFixture<CommandHandlingFixture>
    {
        private readonly CommandHandlingFixture _fixture;

        public CommandHandlingTests(CommandHandlingFixture commandHandlingFixture)
        {
            _fixture = commandHandlingFixture;
        }

        [Fact]
        public void When_put_valid_command_then_should_not_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task> act = () => client.PutCommand(new Command(), Guid.NewGuid(), _fixture.CommandMediaTypeMap);

                act.ShouldNotThrow();
            }
        }

        [Fact]
        public async Task When_put_valid_command_then_shoule_receive_the_command()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                var commandId = Guid.NewGuid();
                await client.PutCommand(new Command(), commandId, _fixture.CommandMediaTypeMap);
                var receivedCommand = _fixture.ReceivedCommands.Last();
                var commandMessage = (CommandMessage<Command>)receivedCommand;

                commandMessage.Command.ShouldBeOfType<Command>();
                commandMessage.Command.ShouldNotBeNull();
                commandMessage.CommandId.ShouldBe(commandId);
                commandMessage.GetUser().ShouldNotBeNull();
            }
        }

        [Fact]
        public void When_put_command_whose_handler_throws_standard_exception_then_should_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task> act = () => client.PutCommand(
                    new CommandThatThrowsStandardException(),
                    Guid.NewGuid(),
                    _fixture.CommandMediaTypeMap);

                act.ShouldThrow<HttpRequestException>();
            }
        }

        [Fact]
        public void When_put_command_whose_handler_throws_http_problem_details_exception_then_should_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task> act = () => client.PutCommand(
                    new CommandThatThrowsProblemDetailsException(),
                    Guid.NewGuid(),
                    _fixture.CommandMediaTypeMap);

                var exception = act.ShouldThrow<HttpProblemDetailsException<HttpProblemDetails>>();

                exception.ProblemDetails.ShouldNotBeNull();
                exception.ProblemDetails.Instance.ShouldNotBeNull();
                exception.ProblemDetails.Detail.ShouldNotBeNull();
                exception.ProblemDetails.Title.ShouldNotBeNull();
                exception.ProblemDetails.Type.ShouldNotBeNull();
            }
        }

        [Fact]
        public void When_put_command_whose_handler_throws_exception_mapped_to_http_problem_details_exception_then_should_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task> act = () => client.PutCommand(
                    new CommandThatThrowsMappedException(),
                    Guid.NewGuid(),
                    _fixture.CommandMediaTypeMap);

                var exception = act.ShouldThrow<HttpProblemDetailsException<HttpProblemDetails>>();

                exception.ProblemDetails.ShouldNotBeNull();
                exception.ProblemDetails.Instance.ShouldBeNull();
                exception.ProblemDetails.Detail.ShouldNotBeNull();
                exception.ProblemDetails.Title.ShouldNotBeNull();
                exception.ProblemDetails.Type.ShouldNotBeNull();
            }
        }

        [Fact]
        public void When_put_command_whose_handler_throws_custom_problem_details_exception_then_should_throw()
        {
            using(var client = _fixture.CreateHttpClient())
            {
                Func<Task> act = () => client.PutCommand(
                    new CommandThatThrowsCustomProblemDetailsException(),
                    Guid.NewGuid(),
                    _fixture.CommandMediaTypeMap);

                var exception = act.ShouldThrow<CustomProblemDetailsException>();

                exception.ProblemDetails.ShouldNotBeNull();
                exception.ProblemDetails.Instance.ShouldNotBeNull();
                exception.ProblemDetails.Detail.ShouldNotBeNull();
                exception.ProblemDetails.Title.ShouldNotBeNull();
                exception.ProblemDetails.Type.ShouldNotBeNull();
                exception.ProblemDetails.Name.ShouldNotBeNull();
            }
        }

        [Fact]
        public void When_command_endpoint_is_not_found_then_should_throw()
        {
            using (var client = _fixture.CreateHttpClient())
            {
                Func<Task> act = () => client.PutCommand(
                    new Command(),
                    Guid.NewGuid(),
                    _fixture.CommandMediaTypeMap,
                    "notfoundpath");

                act.ShouldThrow<HttpRequestException>();
            }
        }

        [Theory]
        [InlineData("text/html")]
        [InlineData("text/html+unsupported")]
        public async Task When_request_MediaType_does_not_have_a_valid_serialization_then_should_get_Unsupported_Media_Type(string mediaType)
        {
            using (var client = _fixture.CreateHttpClient())
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    Guid.NewGuid().ToString())
                {
                    Content = new StringContent("text")
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                var response = await client.SendAsync(request);

                response.StatusCode.ShouldBe(HttpStatusCode.UnsupportedMediaType);
            }
        }
    }
}