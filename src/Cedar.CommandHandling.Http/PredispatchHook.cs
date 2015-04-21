namespace Cedar.CommandHandling.Http
{
    using System.Collections.Generic;

    public delegate void PredispatchHook(IDictionary<string, object> metadata);
}