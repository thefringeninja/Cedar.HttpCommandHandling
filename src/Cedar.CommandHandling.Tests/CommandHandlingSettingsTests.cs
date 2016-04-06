namespace Cedar.CommandHandling
{
    using System;
    using System.IO;
    using Cedar.CommandHandling.Http;
    using Cedar.CommandHandling.Http.TypeResolution;
    using FakeItEasy;
    using Shouldly;
    using Xunit;

    public class CommandHandlingSettingsTests
    {
        [Fact]
        public void When_deserializer_throws_then_should_throw_HttpProblemDetailsException()
        {
            var sut = new CommandHandlingSettings(A.Fake<ICommandHandlerResolver>(), A.Fake<ResolveCommandType>());

            Action act = () =>
            {
                using(var reader = new StringReader("xyx"))
                {
                    sut.DeserializeCommand(reader, typeof(CommandHandlingSettingsTests));
                }
            };

            var exception = act.ShouldThrow<HttpProblemDetailsException<HttpProblemDetails>>();
            exception.ProblemDetails.Status.ShouldBe(400);
        }

        [Fact]
        public void When_custom_deserializer_throws_then_should_throw_HttpProblemDetailsException()
        {
            var sut = new CommandHandlingSettings(A.Fake<ICommandHandlerResolver>(), A.Fake<ResolveCommandType>())
            {
                DeserializeCommand = (_, __) => { throw new Exception(); }
            };

            Action act = () =>
            {
                using(var reader = new StringReader("xyx"))
                {
                    sut.DeserializeCommand(reader, typeof(CommandHandlingSettingsTests));
                }
            };

            act.ShouldThrow<HttpProblemDetailsException<HttpProblemDetails>>();
        }

        [Fact]
        public void When_custom_deserializer_throws_HttpProblemDetailsException_then_it_should_propagate()
        {
            var expected = new HttpProblemDetailsException<HttpProblemDetails>(new HttpProblemDetails());
            var sut = new CommandHandlingSettings(A.Fake<ICommandHandlerResolver>(), A.Fake<ResolveCommandType>())
            {
                DeserializeCommand = (_, __) => { throw expected; }
            };

            Exception thrown = null;
            try
            {
                using (var reader = new StringReader("xyx"))
                {
                    sut.DeserializeCommand(reader, typeof(CommandHandlingSettingsTests));
                }
            }
            catch(Exception ex)
            {
                thrown = ex;
            }

            thrown.ShouldBe(expected);
        }
    }
}