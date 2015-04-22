namespace Cedar.CommandHandling.Http
{
    internal interface IHttpProblemDetailException
    {
        HttpProblemDetails ProblemDetails { get; }
    }
}