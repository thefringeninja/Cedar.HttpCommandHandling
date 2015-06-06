namespace Cedar.CommandHandling.Http
{
    using System;
    using System.Net.Http;
    using EnsureThat;
    using MidFunc = System.Func<System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task
        >, System.Func<System.Collections.Generic.IDictionary<string, object>,
            System.Threading.Tasks.Task
        >
    >;

    public static class MidFuncExtensions
    {
        public static HttpClient CreateEmbeddedClient(this MidFunc midFunc, Uri baseAddress = null)
        {
            Ensure.That(midFunc).IsNotNull();

            baseAddress = baseAddress ?? new Uri("http://localhost");

            var handler = new OwinHttpMessageHandler(midFunc)
            {
                UseCookies = true
            };

            return new HttpClient(handler, true)
            {
                BaseAddress = baseAddress
            };
        }
    }
}